using System;

namespace SMLogging
{
    [Serializable]
    internal class ErrorTraceData
    {
        public Guid ActivityId { get; set; }

        public Guid CorrelationId { get; set; }

        public Guid MessageId { get; set; }

        public string ClientIpAddress { get; set; }

        public string ApplicationName { get; set; }

        public string MachineName { get; set; }

        public string MachineIpAddress { get; set; }

        public Uri Target { get; set; }

        public string Action { get; set; }
    }
}
