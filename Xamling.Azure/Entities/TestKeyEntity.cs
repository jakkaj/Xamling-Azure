using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamling.Azure.Portable.Contract;

namespace Xamling.Azure.Entities
{
    public class TestKeyEntity : IDocumentEntity
    {
       
        public string PersonName {get; set; }
        public TestKeyEntity SubKey { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
