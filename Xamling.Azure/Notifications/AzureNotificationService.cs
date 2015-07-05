//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.ServiceBus.Notifications;
//using Xamling.Portable.Contract;
//using XamlingCore.Portable.Contract.Config;
//using XamlingCore.Portable.Contract.Serialise;
//using XamlingCore.Portable.Model.Response;

//namespace Xamling.Azure.Notifications
//{
//    public class AzureNotificationService : IAzureNotificationService
//    {
//        private readonly IEntitySerialiser _serialiser;
//        private readonly IConfig _config;
//        private readonly ISystemLoggerService _logService;

//        public AzureNotificationService(IEntitySerialiser serialiser, 
//            IConfig config, ISystemLoggerService logService)
//        {
//            _serialiser = serialiser;
//            _config = config;
//            _logService = logService;
//        }

//        public async Task<XResult<bool>> SendPush(Guid userId, ClientMessage m)
//        {
//            var dict = new Dictionary<string, string>();
            
//            dict.Add("t", m.Title);
//            dict.Add("s", m.Message);

//            _logService.SendingPush(m.Id, userId.ToString(), _serialiser.Serialise(m));

//            //clear them out so they are not serialised and sent again

//            m.Title = null;
//            m.Message = null;

//            var ser = _serialiser.Serialise(m);

//            dict.Add("m", ser);

//            try
//            {
//                var hub =
//                    NotificationHubClient.CreateClientFromConnectionString(_config["HubsConnectionString"],
//                        _config["HubsPath"]);

//                var result = await hub.SendTemplateNotificationAsync(dict, userId.ToString());

//                return new XResult<bool>(true);
//            }
//            catch (Exception ex)
//            {
//                _logService.LogException("NotifcationHubs", ex);
//            }

//            return XResult<bool>.GetFailed();

//        }
//    }
//}
