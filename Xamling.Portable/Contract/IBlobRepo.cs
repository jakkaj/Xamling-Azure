using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Portable.Contract
{
    public interface IBlobRepo
    {
        Task<XResult<bool>> UploadAsync(string blobName, byte[] data, string contentType = null);
        Task<XResult<bool>> UploadAsync(string blobName, string localFileName, string contentType = null);

        Task<XResult<byte[]>> DownloadByteAsync(string blobName);
        Task<XResult<bool>> DownloadByteAsync(string blobName, string fileName);
        string GetSharedAccess(string blobName, bool canWrite = false);

        Task<XResult<bool>> Exists(string blobName);
        Task<bool> CreateBlob();
        Task<XResult<bool>> UploadTextAsync(string blobName, string text);
        Task<XResult<string>> DownloadStringAsync(string blobName);
        Task<XResult<bool>> DeleteAsync(string blobName);
        Task<List<string>> List(string directoryName);
    }
}
