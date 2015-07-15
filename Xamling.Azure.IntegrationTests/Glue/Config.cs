using System.Configuration;
using System.Linq;
using XamlingCore.Portable.Contract.Config;

namespace Xamling.Azure.IntegrationTests.Glue
{
    public class WebConfig : IConfig
    {
        private string appendix = null;

        public WebConfig()
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("RunMode"))
            {
                appendix = ConfigurationManager.AppSettings["RunMode"];
                if (string.IsNullOrWhiteSpace(appendix))
                {
                    appendix = null;
                }
            }
        }

        public string this[string index]
        {
            get
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(index + appendix))
                {
                    return ConfigurationManager.AppSettings[index + appendix];
                }

                return ConfigurationManager.AppSettings[index];
            }
        }
    }
}
