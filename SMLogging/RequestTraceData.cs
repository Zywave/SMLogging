using System;

namespace SMLogging
{
    [Serializable]
    internal class RequestTraceData
    {
        public Guid ActivityId { get; set; }

        public Guid CorrelationId { get; set; }

        public Guid MessageId { get; set; }

        public DateTimeOffset? StartDateTime { get; set; }

        public DateTimeOffset? EndDateTime { get; set; }

        public string ClientIpAddress { get; set; }

        public string ApplicationName { get; set; }

        public string MachineName { get; set; }

        public string ServerIpAddress { get; set; }

        public Uri Target { get; set; }

        public string Action { get; set; }

        public bool? IsOneWay { get; set; }

        public bool? IsFault { get; set; }

        public string FaultCode { get; set; } 

        public int? ResponseSize { get; set; }

        public int? RequestSize { get; set; }
    }
}
