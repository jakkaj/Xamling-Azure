using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using Xamling.Azure.Contract;
using Xamling.Portable.Contract;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Table
{
    public class TableRepo<T> : ITableRepo<T> where T : TableEntity, new()
    {
        private readonly ILogService _logService;
        private CloudTable _table;

        public TableRepo(CloudTableClient tableClient, ILogService logService)
        {
            _logService = logService;
            var n = typeof(T).Name;

            if (!n.EndsWith("TableEntity"))
            {
                throw new InvalidOperationException("Table entity breaks convention. Enure ends with TableEntity");
            }

            n = n.Replace("TableEntity", "");

            _table = tableClient.GetTableReference(n);
        }

        public async Task<bool> CreateTable()
        {
            return await _table.CreateIfNotExistsAsync();
        }

        public async Task<XResult<T>> Get(string partitionKey, string rowKey)
        {
            var result = await OperationWrap<T>(() =>
            {
                var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                var retrievedResult = _table.ExecuteAsync(retrieveOperation);

                return retrievedResult;
            });

            return result;
        }

        public async Task<XResult<List<T>>> Get(string partitionKey)
        {
            var result = await OperationWrap<List<T>>(async () =>
            {
                var q = _table.CreateQuery<T>().Where(_ => _.PartitionKey == partitionKey);
                return await _querySegment(q.AsTableQuery());
            });

            return result;
        }

        public async Task<XResult<T>> Add(T entity)
        {

            var result = await OperationWrap<T>(() =>
            {
                var insertOperation = TableOperation.Insert(entity);

                // Execute the insert operation.
                var tableResult = _table.ExecuteAsync(insertOperation);

                return tableResult;
            });

            return result;

        }

        public async Task<XResult<T>> AddOrUpdate(T entity)
        {
            var result = await OperationWrap<T>(() =>
            {
                var insertOperation = TableOperation.InsertOrReplace(entity);

                // Execute the insert operation.
                var tableResult = _table.ExecuteAsync(insertOperation);

                return tableResult;
            });

            return result;
        }

        public async Task<XResult<T>> Delete(T entity)
        {
            var result = await OperationWrap<T>(() =>
            {
                var insertOperation = TableOperation.Delete(entity);

                // Execute the insert operation.
                var tableResult = _table.ExecuteAsync(insertOperation);

                return tableResult;
            });

            return result;
        }

        async Task<List<T>> _querySegment(TableQuery<T> query)
        {
            TableQuerySegment<T> querySegment = null;
            var returnList = new List<T>();

            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = await query
                                             .ExecuteSegmentedAsync(querySegment != null ?
                                                 querySegment.ContinuationToken : null);
                if (querySegment.Results != null)
                {
                    returnList.AddRange(querySegment.Results);
                }

            }

            return returnList;
        }

        public async Task<XResult<T>> OperationWrap<T>(Func<Task<TableResult>> func)
        {
            try
            {
                var result = await func();
                
                if (result.HttpStatusCode != (int)HttpStatusCode.OK && result.HttpStatusCode != (int)HttpStatusCode.NoContent)
                {
                    return
                        XResult<T>.GetBadRequest(String.Format("Table Storage not OK result: {0}",
                            result.HttpStatusCode));
                }

                if (result.Result == null)
                {
                    return
                        XResult<T>.GetNotFound(String.Format("Table Storage item not found: {0}",
                            result.HttpStatusCode));
                }

                return new XResult<T>((T)result.Result);
            }
            catch (Exception ex)
            {
                _logService.TrackException(ex);
                return XResult<T>.GetException("TableStorage " + ex.ToString(), ex);
            }
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
                return XResult<T>.GetException("TableStorage " + ex.ToString(), ex);
            }
        }
    }
}
