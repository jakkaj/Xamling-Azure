using System;
using Xamling.Azure.Portable.Entity;

namespace Xamling.Azure.Portable.Contract
{
    public interface IJwtCreator
    {
        XUserToken CreateToken(Guid tokenId, Guid userId, Guid sessionId, int redcatId, bool refresh = false);
    }
}
