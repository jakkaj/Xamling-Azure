using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using Xamling.Azure.Contract;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Contract.Config;
using XamlingCore.Portable.Model.Other;

namespace Xamling.Azure.Redis
{
    public class RedisConnection : IRedisConnection
    {
        private readonly IConfig _config;
        private  ConnectionMultiplexer _connection;

        private bool _isRetrying = false;

        public RedisConnection(IConfig config)
        {
            _config = config;
            _connect();
        }

        void _connect()
        {
            try
            {
                var connectionString = _config["RedisConnectionString"];
                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = false;
                options.ConnectRetry = 2;

                _connection = ConnectionMultiplexer.Connect(options);

                if (_connection.IsConnected)
                {
                    _connection.ConnectionFailed += _connection_ConnectionFailed;
                    _isRetrying = false;
                }
                else
                {
                    //_logService.TrackTrace("RedisConnectionFailure", XSeverityLevel.Error);

                    if (!_isRetrying)
                    {
                        new Thread(_connectionRetry).Start();
                    }
                }
                
            }
            catch (Exception Ex)
            {
                //_logService.TrackTrace("RedisConnectionFailure", XSeverityLevel.Error);
                //_logService.TrackException(Ex);
                throw Ex;
            }
        }

        void _connectionRetry()
        {
            if (_isRetrying)
            {
                return;
            }

            _isRetrying = true;

            while (_isRetrying)
            {
                Thread.Sleep(10000);
                Debug.WriteLine("Redis connection retry");
                
                _connect();
                
            }
        }

        private void _connection_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            //_logService.TrackTrace("RedisConnectionFailure", XSeverityLevel.Error);
            _connection.ConnectionFailed -= _connection_ConnectionFailed;
            _connect();
        }

        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }

        public ISubscriber GetSubscriber()
        {
            return _connection.GetSubscriber();
        }
    }
}
