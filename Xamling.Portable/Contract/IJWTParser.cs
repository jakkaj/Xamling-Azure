﻿using System.Collections.Generic;

namespace Xamling.Portable.Contract
{
    public interface IJwtParser
    {
        string GetClaim(string claimType);
        bool LoadToken(string token);
        Dictionary<string, string> GetClaims();
    }
}