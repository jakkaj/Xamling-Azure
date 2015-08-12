using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Xamling.Azure.Portable.Contract;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Contract.Serialise;
using XamlingCore.Portable.Model.Other;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Logger
{
    public class LogService : ILogService
    {
        private readonly IEntitySerialiser _entitySerialiser;
        private readonly TelemetryClient _telemetry;
        public LogService(IEntitySerialiser entitySerialiser)
        {
            _entitySerialiser = entitySerialiser;
            _telemetry = new TelemetryClient();
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            _telemetry.TrackEvent(eventName, properties, metrics);
        }

        public void TrackException(Exception exception, IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            _telemetry.TrackException(exception, properties, metrics);
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            _telemetry.TrackMetric(name, value, properties);
        }

        public void TrackPageView(string name)
        {
            _telemetry.TrackPageView(name);
        }

        public void TrackRequest(string name, DateTimeOffset timestamp, TimeSpan duration, string responseCode,
            bool success)
        {
            _telemetry.TrackRequest(name, timestamp, duration, responseCode, success);
        }

        public void TrackTrace(string message)
        {
            _telemetry.TrackTrace(message);
        }

        public void TrackTrace(string message, IDictionary<string, string> properties)
        {
            _telemetry.TrackTrace(message, properties);
        }

        public void TrackTrace(string message, XSeverityLevel severityLevel)
        {
            _telemetry.TrackTrace(message, Mapper.Map<SeverityLevel>(severityLevel));
        }

        public void TrackTrace(string message, XSeverityLevel severityLevel, IDictionary<string, string> properties)
        {
            _telemetry.TrackTrace(message, Mapper.Map<SeverityLevel>(severityLevel), properties);
        }

        public void TrackOperation<T>(XResult<T> operation, string operationName = null)
        {
            var op = _entitySerialiser.Serialise(operation);

            var dict = new Dictionary<string, string>();

            if (operationName != null)
            {
                dict.Add("Name", operationName);
            }

            dict.Add("Id", operation.Id.ToString());
            dict.Add("Message", operation.Message);
            dict.Add("CallerMemberName", operation.CallerInfo.MemberName);
            dict.Add("Result", operation.ResultCode.ToString());
            dict.Add("StatusCode", operation.StatusCode.ToString());
            dict.Add("XResult", op);

            if (operation.ResultCode == OperationResults.Exception || operation.Exception != null)
            {
                dict.Add("ExceptionType", "XResult");
                TrackException(operation.Exception, dict);
                return;
            }

            TrackTrace("XResult", operation.IsSuccess ? XSeverityLevel.Information : XSeverityLevel.Error, dict);
        }
    }
}
