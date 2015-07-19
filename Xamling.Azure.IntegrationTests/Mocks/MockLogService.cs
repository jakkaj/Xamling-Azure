using System;
using System.Collections.Generic;

using Xamling.Azure.Portable.Contract;
using XamlingCore.Portable.Model.Other;
using XamlingCore.Portable.Model.Response;
using XamlingCore.Portable.View.ViewModel;

namespace RZ.NET.Tests.Mocks
{
    public class MockLogService : ILogService
    {
        public void TrackView<T>(T viewModel) where T : XViewModel
        {

        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {

        }

        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {

        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {

        }

        public void TrackPageView(string name)
        {

        }

        public void TrackRequest(string name, DateTimeOffset timestamp, TimeSpan duration, string responseCode, bool success)
        {
            
        }

        public void TrackTrace(string message)
        {

        }

        public void TrackTrace(string message, IDictionary<string, string> properties)
        {

        }

       

        public void TrackTrace(string message, XSeverityLevel severityLevel)
        {

        }

        public void TrackTrace(string message, XSeverityLevel severityLevel, IDictionary<string, string> properties)
        {

        }

        public void TrackOperation<T>(XResult<T> operation)
        {

        }

       
    }
}
