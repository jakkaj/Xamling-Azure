using System;
using System.Collections.Generic;
using Xamling.Azure.Portable.Entity;
using XamlingCore.Portable.Model.Other;
using XamlingCore.Portable.Model.Response;

namespace Xamling.Azure.Portable.Contract
{
    public interface ILogService
    {
        void TrackEvent(string eventName, IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null);

        void TrackException(Exception exception, IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null);

        void TrackMetric(string name, double value, IDictionary<string, string> properties = null);
        void TrackPageView(string name);

        void TrackRequest(string name, DateTimeOffset timestamp, TimeSpan duration, string responseCode,
            bool success);

        void TrackTrace(string message);
        void TrackTrace(string message, IDictionary<string, string> properties);
        void TrackTrace(string message, XSeverityLevel severityLevel);
        void TrackTrace(string message, XSeverityLevel severityLevel, IDictionary<string, string> properties);
        XResult<T> TrackOperation<T>(XResult<T> operation, string operationName = null);
    }
}