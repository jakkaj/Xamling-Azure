using System;
using Xamling.Portable.Entity;

namespace Xamling.Portable.Contract
{
    public interface IJwtCreator
    {
        XUserToken CreateToken(Guid tokenId, Guid userId, Guid sessionId, int redcatId, bool refresh = false);
    }
}
