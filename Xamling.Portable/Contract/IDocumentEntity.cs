using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Xamling.Azure.Portable.Contract
{
    public interface IDocumentEntity
    {
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; }
    }
}
