using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Xamling.Portable.Contract;
using Xamling.Portable.Entity;
using XamlingCore.Portable.Model.Resiliency;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Blob
{
    public class BlobRepo : IBlobRepo
    {
        private readonly ILogService _logService;
        readonly CloudBlobContainer _container;
        public BlobRepo(CloudBlobClient blobClient, ILogService logService, string containerName)
        {
            _logService = logService;
            _container = blobClient.GetContainerReference(containerName.ToLower());
        }

        public async Task<bool> CreateBlob()
        {
            return await _container.CreateIfNotExistsAsync();
        }

        void _logDelete(string name)
        {
            _logService.TrackTrace("Blob Delete", XSeverityLevel.Information, new Dictionary<string, string>
            {
                {"BlobName", name }, {"ContainerName", _container.Name }, {"ContainerUrl", _container.Uri.ToString() }
            });
        }

        void _logRead(string name)
        {
            _logService.TrackTrace("Blob Read", XSeverityLevel.Information, new Dictionary<string, string>
            {
                {"BlobName", name }, {"ContainerName", _container.Name }, {"ContainerUrl", _container.Uri.ToString() }
            });
        }

        void _logWrite(string name)
        {
            _logService.TrackTrace("Blob Write", XSeverityLevel.Information, new Dictionary<string, string>
            {
                {"BlobName", name }, {"ContainerName", _container.Name }, {"ContainerUrl", _container.Uri.ToString() }
            });
        }

        public async Task<XResult<bool>> UploadAsync(string blobName, byte[] data, string contentType = null)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
              {
                  using (var ms = new MemoryStream(data))
                  {
                      await c.UploadFromStreamAsync(ms);
                  }

                  _logWrite(blobName);

                  if (contentType != null)
                  {
                      c.Properties.ContentType = contentType;
                      await c.SetPropertiesAsync();
                  }

                  return true;
              }));


            return result;
        }

        public async Task<XResult<bool>> UploadTextAsync(string blobName, string text)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
            {
                await c.UploadTextAsync(text);
                _logWrite(blobName);
                return true;
            }));

            return result;
        }

        public async Task<XResult<bool>> UploadAsync(string blobName, string localFileName, string contentType = null)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
            {
                using (var fs = new FileStream(localFileName, FileMode.Open, FileAccess.Read))
                {
                    await c.UploadFromStreamAsync(fs);
                }

                _logWrite(blobName);

                if (contentType != null)
                {
                    c.Properties.ContentType = contentType;
                    await c.SetPropertiesAsync();
                }
                return true;
            }));

            return result;
        }

        public async Task<XResult<bool>> DeleteAsync(string blobName)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
            {
                var readResult = await c.DeleteIfExistsAsync();
                _logDelete(blobName);
                return readResult;
            }));

            return result;
        }

        public async Task<XResult<bool>> Exists(string blobName)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Exception.Run(() => OperationWrap<bool>(async () =>
            {
                var readResult = await c.ExistsAsync();
                _logRead(blobName);
                return readResult;
            }));

            return result;
        }

        public async Task<XResult<byte[]>> DownloadByteAsync(string blobName)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Default.Run(() => OperationWrap<byte[]>(async () =>
            {
                if (!(await Exists(blobName)).Object)
                {
                    return null;
                }

                using (var ms = new MemoryStream())
                {
                    await c.DownloadToStreamAsync(ms);
                    _logRead(blobName);
                    return ms.ToArray();
                }
            }));

            return result;

        }

        public async Task<XResult<string>> DownloadStringAsync(string blobName)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Default.Run(() => OperationWrap<string>(async () =>
            {
                if (!(await Exists(blobName)).Object)
                {
                    return null;
                }

                var text = await c.DownloadTextAsync();
                _logRead(blobName);
                return text;
            }));

            return result;
        }

        public async Task<XResult<bool>> DownloadByteAsync(string blobName, string fileName)
        {
            var c = _getBlob(blobName);

            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
            {
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    await c.DownloadToStreamAsync(fs);
                    _logRead(blobName);
                    return true;
                }
            }));

            if (!result)
            {
                return XResult<bool>.GetFailed("Could not read blob");
            }

            if (!File.Exists(fileName))
            {
                return XResult<bool>.GetFailed("Blob file check not present");
            }

            var b = File.ReadAllBytes(fileName);

            return new XResult<bool>(b.Length > 0);
        }

        public string GetSharedAccess(string blobName, bool canWrite = false)
        {
            var p = SharedAccessBlobPermissions.Read;

            if (canWrite)
            {
                p = p | SharedAccessBlobPermissions.Write;
            }

            var c = _getBlob(blobName);

            var s = new SharedAccessBlobPolicy
            {
                Permissions = p,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-15),
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(21)
            };
            var apString = c.GetSharedAccessSignature(s);
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", c.Uri, apString);
        }

        public async Task<List<string>> List(string directoryName)
        {
            var dir = _getDirectory(directoryName);

            BlobContinuationToken continuationToken = null;

            do
            {
                var result =
                    await dir.ListBlobsSegmentedAsync(true, BlobListingDetails.All, Int32.MaxValue, null, null, null);

                continuationToken = result.ContinuationToken;

                foreach (var item in result.Results)
                {
                    var bb = item as CloudBlockBlob;
                    if (bb == null)
                    {
                        continue;
                    }
                    Debug.WriteLine($"Name: {bb.Name}");
                }

            } while (continuationToken != null);

            return null;
        }

        CloudBlobDirectory _getDirectory(string directoryName)
        {
            directoryName = directoryName.Replace("\\", "/");
            directoryName = directoryName.Trim('/');
            return _container.GetDirectoryReference(directoryName);
        }
        
        CloudBlockBlob _getBlob(string blobName)
        {
            blobName = blobName.Replace("\\", "/");
            blobName = blobName.Trim('/');
            return _container.GetBlockBlobReference(blobName);
        }

        public async Task<XResult<T>> OperationWrap<T>(Func<Task<T>> func)
        {
            try
            {
                var result = await func();

                return new XResult<T>(result);
            }
            catch (Exception ex)
            {
                _logService.TrackException(ex);
                return XResult<T>.GetException("BlobStorage " + ex.ToString(), ex);
            }
        }

    }
}
