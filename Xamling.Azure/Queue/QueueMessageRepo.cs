using System;
using System.Data;
using System.Data.SqlTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Xamling.Azure.Entities;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Contract.Serialise;
using XamlingCore.Portable.Model.Resiliency;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Queue
{
    public class QueueMessageRepo<T> : IQueueMessageRepo<T> where T : class, new()
    {
        private readonly IEntitySerialiser _serialiser;
        private readonly ILogService _logService;
        private readonly CloudQueue _q;

        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

        public QueueMessageRepo(CloudQueueClient queueClient, IEntitySerialiser serialiser, ILogService logService)
        {
            _serialiser = serialiser;
            _logService = logService;

            var n = typeof(T).Name;

            if (!n.EndsWith("QueueMessage"))
            {
                throw new InvalidOperationException("Queue message breaks convention. Enure ends with QueueMessage");
            }

            n = n.Replace("QueueMessage", "");

            n = n.ToLower();

            _q = queueClient.GetQueueReference(n);
        }

        public async Task<bool> CreateQueue()
        {
            return await _q.CreateIfNotExistsAsync();
        }

        public async Task<bool> PeekMessages()
        {
            var m = await _q.PeekMessageAsync();
            return m != null;
        }

        public async Task<XResult<bool>> ResetMessageTimespan(QueueMessageWrapper<T> message, TimeSpan newTimespan)
        {
            var m = new CloudQueueMessage(message.MessageId, message.PopReceipt);

            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
            {
                await _q.UpdateMessageAsync(m, newTimespan, MessageUpdateFields.Visibility);
                return true;
            }));

            return result;
        }

        public async Task<XResult<bool>>  Post(T entity)
        {
            var s = _serialiser.Serialise(entity);

            var m = new CloudQueueMessage(s);
            
            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
            {
                await _q.AddMessageAsync(m);
                return true;
            }));

            return result;
        }

        public async Task<XResult<Portable.Entity.QueueMessageWrapper<T>>> Next()
        {
            var result = await XResiliant.Default.Run(() => OperationWrap<Portable.Entity.QueueMessageWrapper<T>>(async () =>
            {
                var m = await _q.GetMessageAsync(_defaultTimeout, null, null);

                if (m != null)
                {
                    var s = m.AsString;

                    var entity = _serialiser.Deserialise<T>(s);

                    return new Portable.Entity.QueueMessageWrapper<T>(entity, m.Id, m.PopReceipt, m.DequeueCount);
                }

                return null;
            }));

            return result;
        }

        public async Task<int?> GetQueueLength()
        {
            return _q.ApproximateMessageCount;
        }

        public async Task<XResult<bool>>  DeleteMessage(QueueMessageWrapper<T> message)
        {
            var result = await XResiliant.Default.Run(() => OperationWrap<bool>(async () =>
            {
                await _q.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                return true;
            }));

            return result;
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
