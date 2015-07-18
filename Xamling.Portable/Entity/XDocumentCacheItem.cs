using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Contract;
using XamlingCore.Portable.Model.Cache;

namespace Xamling.Azure.Portable.Entity
{
    public class XDocumentCacheItem<T> : IDocumentEntity
        where T : class, new()
    {
        public string Id { get; set; }
        public XDocumentCacheItem()
        {
            DateStamp = DateTime.UtcNow;
        }
        public XDocumentCacheItem(T item)
        {
            Item = item;
            DateStamp = DateTime.UtcNow;
        }

        public T Item { get; set; }

        public DateTime DateStamp { get; set; }
        public TimeSpan? MaxAge { get; set; }

    }
}
