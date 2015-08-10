using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Contract.Cache;
using XamlingCore.Portable.Contract.Entities;

namespace Xamling.Azure.Portable.Repo
{
    public class DocumentRepo
    {
        private readonly IDocumentEntityCache _documentCache;
        private readonly IRedisEntityCache _redisCache;
        private List<IEntityCache> _caches;

        public DocumentRepo(IDocumentEntityCache documentCache, IRedisEntityCache redisCache)
        {
            _documentCache = documentCache;
            _redisCache = redisCache;
        }
    }
}
