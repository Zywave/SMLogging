using System;

namespace SMLogging
{
    [Serializable]
    internal class ErrorTraceData
    {
        public string MessageId { get; set; }

        public string ClientIpAddress { get; set; }

        public string ApplicationName { get; set; }

        public string MachineName { get; set; }

        public string MachineIpAddress { get; set; }

        public Uri Target { get; set; }

        public string Action { get; set; }
    }
}
