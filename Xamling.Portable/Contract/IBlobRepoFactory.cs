using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamling.Portable.Contract
{
    public interface IBlobRepoFactory
    {
        IBlobRepo GetBlobRepo(string containerName);
    }
}
