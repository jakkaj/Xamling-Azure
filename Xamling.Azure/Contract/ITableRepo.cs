using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Contract
{
    public interface ITableRepo<T> where T : TableEntity, new()
    {
        Task<bool> CreateTable();
        Task<XResult<T>> Get(string partitionKey, string rowKey);
        Task<XResult<List<T>>> Get(string partitionKey);
        Task<XResult<T>> Add(T entity);
        Task<XResult<T>> AddOrUpdate(T entity);
        Task<XResult<T>> Delete(T entity);
        Task<XResult<T>> OperationWrap<T>(Func<Task<TableResult>> func);
        Task<XResult<T>> OperationWrap<T>(Func<Task<T>> func);
    }
}
