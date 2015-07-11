using System;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Portable.Contract
{
    public interface IQueueMessageRepo<T> where T : class, new()
    {
        Task<XResult<bool>>  Post(T entity);
        Task<XResult<QueueMessageWrapper<T>>> Next();
        Task<int?> GetQueueLength();
        Task<XResult<bool>>  DeleteMessage(QueueMessageWrapper<T> wrapper);
        Task<XResult<T>> OperationWrap<T>(Func<Task<T>> func);
        Task<bool> CreateQueue();
        Task<bool> PeekMessages();
        Task<XResult<bool>> ResetMessageTimespan(QueueMessageWrapper<T> message, TimeSpan newTimespan);
    }
}