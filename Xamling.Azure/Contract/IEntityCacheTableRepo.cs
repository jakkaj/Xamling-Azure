using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Azure.Entities;

namespace Xamling.Azure.Contract
{
    public interface IEntityCacheTableRepo : ITableRepo<EntityCacheTableEntity>
    {
    }
}
