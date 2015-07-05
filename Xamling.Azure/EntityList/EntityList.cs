using System.Collections.Generic;
using XamlingCore.Portable.Model.Contract;

namespace Xamling.Azure.EntityList
{
    public class EntityList<T>
        where T:IEntity
       
    {
        public List<T> Entities { get; set; }
    }
}
