using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace SMLogging
{
    public class RequestLoggingDispatchMessageInspector : IDispatchMessageInspector
    {
        public RequestLoggingDispatchMessageInspector()
        {
            _traceSource = new TraceSource("System.ServiceModel.RequestLogging");
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var operationContext = OperationContext.Current;

            RemoteEndpointMessageProperty remoteEndpoint = null;
            if (operationContext.IncomingMessageProperties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                remoteEndpoint = operationContext.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            }
            
            return new RequestTraceData
            {
                DateTime = DateTimeOffset.UtcNow,
                ClientIpAddress = remoteEndpoint?.Address,
                Target = request.Headers.To,
                Action = request.Headers.Action
            };
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var requestTraceData = (RequestTraceData)correlationState;

            var requestMessage = OperationContext.Current.RequestContext.RequestMessage.ToString();
            var requestSize = Encoding.UTF8.GetByteCount(requestMessage);

            var bufferedCopy = reply.CreateBufferedCopy(int.MaxValue);
            reply = bufferedCopy.CreateMessage();

            var responseMessage = bufferedCopy.CreateMessage().ToString();
            var responseSize = Encoding.UTF8.GetByteCount(responseMessage);

            _traceSource.TraceData(TraceEventType.Information, 0,
                requestTraceData.DateTime.ToString("yyyy-MM-dd"),
                requestTraceData.DateTime.ToString("HH:mm:ss.FFF"),
                requestTraceData.ClientIpAddress,
                _processName,
                _serverName,
                _serverIpAddress,
                requestTraceData.Target.Scheme,
                requestTraceData.Target.Host,
                requestTraceData.Target.Port,
                requestTraceData.Target,
                requestTraceData.Action,
                reply.IsFault ? 1 : 0, //TODO: Get better fault codes if possible
                responseSize,
                requestSize,
                (DateTimeOffset.UtcNow - requestTraceData.DateTime).TotalMilliseconds
            );
        }

        private readonly TraceSource _traceSource;

        private static readonly string _serverName;
        private static readonly string _serverIpAddress;
        private static readonly string _processName;

        static RequestLoggingDispatchMessageInspector()
        {
            _serverName = Dns.GetHostName();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _serverIpAddress = ip.ToString();
                    break;;
                }
            }

            _processName = AppDomain.CurrentDomain.FriendlyName;
        }
    }
}
