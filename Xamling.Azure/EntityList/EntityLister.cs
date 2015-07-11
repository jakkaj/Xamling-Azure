using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Contract.Cache;
using XamlingCore.Portable.Contract.Entities;
using XamlingCore.Portable.Model.Contract;

namespace Xamling.Azure.EntityList
{
    public class EntityLister<T> : IEntityLister<T> where T:IEntity
    {
        private readonly ISecureSessionEntityCache _cache;

        public EntityLister(ISecureSessionEntityCache entityCache)
        {
            _cache = entityCache;
        }

        public async Task<List<T>> Get(string listId)
        {
            var entityList = await _cache.GetEntity<EntityList<T>>(_getEntityKey(listId));

            return entityList?.Entities;
        }

        public async Task<bool> Add(T entity, string listId)
        {
            var entityList = await _cache.GetEntity<EntityList<T>>(_getEntityKey(listId));

            if (entityList == null)
            {
                entityList = new EntityList<T>
                {
                    Entities = new List<T>()
                };
            }

            var existingInList = entityList.Entities.FirstOrDefault(_ => _.Id == entity.Id);

            if (existingInList != null)
            {
                entityList.Entities.Remove(existingInList);
            }

            entityList.Entities.Add(entity);

            return await _cache.SetEntity(_getEntityKey(listId), entityList);
        }

        public async Task<bool> Remove(Guid entityId, string listId)
        {
            var entityListFromSource = await Get(listId);

            if (entityListFromSource == null)
            {
                return false;
            }

            var e = entityListFromSource.FirstOrDefault(_ => _.Id == entityId);
            if (e == null)
            {
                return false;
            }

            entityListFromSource.Remove(e);

            var entityList = new EntityList<T>
            {
                Entities = entityListFromSource
            };

            return await _cache.SetEntity(_getEntityKey(listId), entityList);
        }

        string _getEntityKey(string key)
        {
            return $"list-{key}";
        }
    }
}
