using Microsoft.WindowsAzure.Storage.Table;
using Xamling.Azure.Contract;
using Xamling.Azure.Entities;
using Xamling.Portable.Contract;

namespace Xamling.Azure.Table
{
    public class EntityCacheTableRepo : TableRepo<EntityCacheTableEntity>, IEntityCacheTableRepo
    {
        public EntityCacheTableRepo(CloudTableClient tableClient, ILogService logService)
            : base(tableClient, logService)
        {
        }
    }
}
