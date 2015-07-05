using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamlingCore.Portable.Model.Contract;

namespace Xamling.Portable.Contract
{
    public interface IEntityLister<T> where T : IEntity
    {
        Task<List<T>> Get(string listId);
        Task<bool> Add(T entity, string listId);
        Task<bool> Remove(Guid entityId, string listId);
    }
}