using System;
using System.ServiceModel;

namespace SMRequestLogging
{
    internal class RequestTraceData
    {
        public DateTimeOffset DateTime { get; set; }

        public Uri From { get; set; }

        public Uri To { get; set; }

        public EndpointIdentity Identity { get; set; }

        public string Action { get; set; }
    }
}
