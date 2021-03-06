﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Xamling.Azure.Portable.Contract;
using XamlingCore.Portable.Data.Mapper;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.DocumentDB
{
    public class DocumentRepo<T> : IDocumentRepo<T> where T : class, IDocumentEntity, new()
    {
        private readonly IDocumentConnection _documentConnection;
        private readonly ILogService _logService;
        private DocumentCollection _collection;
        private DocumentClient _client;

        public DocumentRepo(IDocumentConnection documentConnection, ILogService logService)
        {
            _documentConnection = documentConnection;
            _logService = logService;
        }

        async Task _init()
        {
            if (_collection == null)
            {
                _collection = await _documentConnection.GetCollection();
                _client = _documentConnection.Client;
            }
        }

        public async Task<XResult<T>> Get(string key)
        {
            var listItems = await GetList(key);

            if (!listItems)
            {
                return listItems.Copy<T>();
            }

            return new XResult<T>(listItems.Object.FirstOrDefault());
        }

        public async Task<XResult<IList<T>>> GetList(string key)
        {
            return await GetList(_ => _.Id == key);
        }

        public async Task<IQueryable<T>> GetQuery()
        {
            await _init();

            var q = _client.CreateDocumentQuery<T>(_collection.DocumentsLink);

            return q;
        }

        public async Task<XResult<IList<T>>> Query(IQueryable<T> query)
        {
            await _init();

            var documents = await _queryAsync(query);

            if (!documents)
            {
                return documents.Copy<IList<T>>();
            }

            var listItems = documents.Object;

            if (listItems == null || listItems.Count == 0)
            {
                return XResult<IList<T>>.GetNotFound();
            }

            return new XResult<IList<T>>(listItems);
        }

        public async Task<XResult<IList<T>>> GetListSQL(string query)
        {
            await _init();

            var q = _client.CreateDocumentQuery<T>(_collection.DocumentsLink, query);
        
            var documents = await _queryAsync(q);

            if (!documents)
            {
                return documents.Copy<IList<T>>();
            }

            var listItems = documents.Object;

            if (listItems == null || listItems.Count == 0)
            {
                return XResult<IList<T>>.GetNotFound();
            }

            return new XResult<IList<T>>(listItems);
        }

        public async Task<XResult<IList<T>>> GetList(params Expression<Func<T, bool>>[] queries)
        {
            if (queries.Length == 0)
            {
                return XResult<IList<T>>.GetBadRequest("No queries passed in to GetList");
            }

            await _init();

            var q = _client.CreateDocumentQuery<T>(_collection.DocumentsLink)
                .Where(queries[0]);
           
            foreach (var subQuery in queries)
            {
                if (subQuery == queries[0])
                {
                    continue;
                }
                q = q.Where(subQuery);
            }

            var documents = await _queryAsync(q);

            if (!documents)
            {
                return documents.Copy<IList<T>>();
            }

            var listItems = documents.Object;

            if (listItems == null || listItems.Count == 0)
            {
                return XResult<IList<T>>.GetNotFound();
            }

            return new XResult<IList<T>>(listItems);
        }

        public async Task<XResult<T>> AddOrUpdate(T entity, TimeSpan? maxAge = null)
        {
            await _init();

            var q = _client.CreateDocumentQuery<Document>(_collection.DocumentsLink)
                .Where(d => d.Id == entity.Id);

            dynamic getExistingDocumentResult = q.AsEnumerable().FirstOrDefault();

            if (getExistingDocumentResult != null)
            {
                T d = getExistingDocumentResult;
                entity.CopyProperties(d);

                var replaceResult = await OperationWrapResourceResponse(
                    () => _client.ReplaceDocumentAsync(getExistingDocumentResult.SelfLink, d));


                return replaceResult;
            }

            var result = await OperationWrapResourceResponse(() =>
            {
                var document = _client.CreateDocumentAsync(_collection.SelfLink, entity);
                return document;
            });

            return !result ? result.Copy<T>() : result;
        }

        public async Task<XResult<bool>> Delete(string key)
        {
            await _init();

            var q = _client.CreateDocumentQuery<Document>(_collection.DocumentsLink)
                .Where(d => d.Id == key);

            var result = q.AsEnumerable().FirstOrDefault();
            if (result != null)
            {
                var deleteResult = await OperationWrapResourceResponse(() => _client.DeleteDocumentAsync(result.SelfLink));
                if (!deleteResult)
                {
                    return deleteResult.Copy<bool>();
                }
            }

            return new XResult<bool>(true);
        }

       
        private async Task<XResult<IList<T>>> _queryAsync(IQueryable<T> query)
        {
            var docQuery = query.AsDocumentQuery();
            var batches = new List<IEnumerable<T>>();

            do
            {
                var batch = await OperationWrap(() => docQuery.ExecuteNextAsync<T>());

                if (!batch)
                {
                    return batch.Copy<IList<T>>();
                }

                batches.Add(batch.Object);
            }
            while (docQuery.HasMoreResults);

            var docs = batches.SelectMany(b => b).ToList();

            return new XResult<IList<T>>(docs);
        }

        public async Task<XResult<T>> OperationWrap(Func<Task<T>> func)
        {
            try
            {
                var result = await func();

                return new XResult<T>(result);
            }
            catch (Exception ex)
            {
                _logService.TrackException(ex);
                return XResult<T>.GetException("Document DB " + ex.ToString(), ex);
            }
        }

        public async Task<XResult<IList<T>>> OperationWrap(Func<Task<FeedResponse<T>>> func)
        {
            try
            {
                var result = await func();

                var docs = result.ToList();

                return new XResult<IList<T>>(docs);
            }
            catch (Exception ex)
            {
                _logService.TrackException(ex);
                return XResult<IList<T>>.GetException("Document DB " + ex.ToString(), ex);
            }
        }

        public async Task<XResult<T>> OperationWrapResourceResponse(Func<Task<ResourceResponse<Document>>> func)
        {
            try
            {
                var result = await func();

                if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.Created && result.StatusCode != HttpStatusCode.NoContent)
                {
                    return
                        XResult<T>.GetBadRequest($"Document Database not OK result: {(int)result.StatusCode}");
                }

                dynamic d = result.Resource;

                T dt = d;

                return new XResult<T>(dt);
            }
            catch (Exception ex)
            {
                _logService.TrackException(ex);
                return XResult<T>.GetException("Document DB " + ex.ToString(), ex);
            }
        }
    }
}
