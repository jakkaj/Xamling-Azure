using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Xamling.Azure.Portable.Contract;
using XamlingCore.Portable.Data.Glue;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Logger
{
    public static class LogExtensions
    {
        public static XResult<T> Log<T>(this XResult<T> result, string operationName = null)
        {
            if (ContainerHost.Container == null)
            {
                Debug.WriteLine("Warning - ContainerHost.Container is empty. Could not log operation");
                return result;
            }

            var logger = ContainerHost.Container.Resolve<ILogService>();
            logger.TrackOperation(result, operationName);

            return result;
        } 
    }
}
