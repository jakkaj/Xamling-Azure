using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamling.Azure.Portable.Contract;

namespace Xamling.Azure.Entities
{
    public class TestKeyEntity : IKeyEntity
    {
        public string Key { get; set; }
        public string PersonName {get; set; }
        public TestKeyEntity SubKey { get; set; }

    }
}
