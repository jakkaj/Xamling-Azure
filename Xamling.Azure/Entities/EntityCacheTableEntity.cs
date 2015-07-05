using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Azure.Table;

namespace Xamling.Azure.Entities
{
    public class EntityCacheTableEntity : TableEntityBase
    {
        public string Data { get; set; }
    }
}
