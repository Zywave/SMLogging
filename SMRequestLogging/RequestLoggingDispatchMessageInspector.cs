using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace SMRequestLogging
{
    public class RequestLoggingDispatchMessageInspector : IDispatchMessageInspector
    {
        public RequestLoggingDispatchMessageInspector()
        {
            _traceSource = new TraceSource("System.ServiceModel.RequestLogging");
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return new RequestTraceData
            {
                DateTime = DateTimeOffset.UtcNow,
                From = request.Headers.From?.Uri,
                Identity = request.Headers.From?.Identity,
                To = request.Headers.To,
                Action = request.Headers.Action
            };
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var requestTraceData = (RequestTraceData)correlationState;

            _traceSource.TraceData(TraceEventType.Information, 0,
                requestTraceData.From, 
                requestTraceData.Identity,
                requestTraceData.Action,
                Environment.MachineName, 
                //ServerIPAddress
                //ServerPort
                //Method (GET)
                requestTraceData.To,
                //ProtocolStatus
                //ProtocolSubStatus
                //Win32Status
                //BytesSent
                //BytesRecieved
                (DateTimeOffset.UtcNow - requestTraceData.DateTime).TotalMilliseconds,               
                requestTraceData.To.Scheme, 
                requestTraceData.To.Host
                //UserAgent
                //Cookie
                //Referer

            );
        }

        private readonly TraceSource _traceSource;
    }
}
