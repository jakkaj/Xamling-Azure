using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Portable.Contract
{
    public interface IDocumentRepo<T> where T : class, IDocumentEntity, new()
    {
        Task<XResult<T>> Get(string key);
        Task<XResult<IList<T>>> GetList(string key);
        Task<XResult<T>> AddOrUpdate(T entity, TimeSpan? maxAge = null);
        Task<XResult<bool>> Delete(string key);
        Task<XResult<IList<T>>> GetList(Expression<Func<T, bool>> query);
    }
}