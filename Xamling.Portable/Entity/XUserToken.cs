using System;
using XamlingCore.Portable.Model.Contract;

namespace Xamling.Portable.Entity
{
    public class XUserToken : IEntity
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
