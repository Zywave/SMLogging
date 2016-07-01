using System;

namespace SMLogging
{
    internal class RequestTraceData
    {
        public DateTimeOffset DateTime { get; set; }

        public string ClientIpAddress { get; set; }

        public Uri Target { get; set; }

        public string Action { get; set; }
    }
}
