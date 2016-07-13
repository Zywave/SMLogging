using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SMLogging
{
    /// <summary>
    /// Represents a dispatch message inspector for logging requests.
    /// </summary>
    /// <seealso cref="IDispatchMessageInspector" />
    public class RequestLoggingDispatchMessageInspector : IDispatchMessageInspector
    {
        /// <summary>
        /// Initializes a new instance of the  <see cref="RequestLoggingDispatchMessageInspector"/> class.
        /// </summary>
        public RequestLoggingDispatchMessageInspector()
        {
            _traceSource = new TraceSource("System.ServiceModel.RequestLogging");
        }

        /// <summary>
        /// Called after an inbound message has been received but before the message is dispatched to the intended operation.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="channel">The incoming channel.</param>
        /// <param name="instanceContext">The current service instance.</param>
        /// <returns>
        /// The object used to correlate state. This object is passed back in the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.BeforeSendReply(System.ServiceModel.Channels.Message@,System.Object)" /> method.
        /// </returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var operationContext = OperationContext.Current;

            RemoteEndpointMessageProperty remoteEndpoint = null;
            if (operationContext?.IncomingMessageHeaders != null)
            {
                if (operationContext.IncomingMessageProperties.ContainsKey(RemoteEndpointMessageProperty.Name))
                {
                    remoteEndpoint = operationContext.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                }
            }

            var requestSize = 0;
            var requestMessage = request?.ToString();
            if (requestMessage != null)
            {
                requestSize = Encoding.UTF8.GetByteCount(requestMessage);
            }

            return new RequestTraceData
            {
                ClientIpAddress = remoteEndpoint?.Address,
                Target = request?.Headers?.To,
                Action = request?.Headers?.Action,
                RequestSize = requestSize,
                StartDateTime = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param>
        /// <param name="correlationState">The correlation object returned from the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.AfterReceiveRequest(System.ServiceModel.Channels.Message@,System.ServiceModel.IClientChannel,System.ServiceModel.InstanceContext)" /> method.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var endDateTime = DateTimeOffset.UtcNow;
            var requestTraceData = (RequestTraceData)correlationState;
            
            var responseSize = 0;
            var faultCode = "Success";
            if (reply != null)
            {
                var bufferedCopy = reply.CreateBufferedCopy(int.MaxValue);
                reply = bufferedCopy.CreateMessage();
                var responseMessage = bufferedCopy.CreateMessage().ToString();
                responseSize = Encoding.UTF8.GetByteCount(responseMessage);
                if (reply.IsFault)
                {
                    faultCode = GetFaultCode(responseMessage);
                }
            }

            _traceSource.TraceData(TraceEventType.Information, 0,
                requestTraceData.StartDateTime.ToString("yyyy-MM-dd"),
                requestTraceData.StartDateTime.ToString("HH:mm:ss.FFF"),
                requestTraceData.ClientIpAddress ?? "0.0.0.0",
                _processName,
                _serverName,
                _serverIpAddress ?? "0.0.0.0",
                requestTraceData.Target?.Scheme ?? "null",
                requestTraceData.Target?.Host ?? "null",
                requestTraceData.Target?.Port ?? 0,
                requestTraceData.Target?.ToString() ?? "null",
                requestTraceData.Action ?? "null",
                faultCode,
                responseSize,
                requestTraceData.RequestSize,
                (endDateTime - requestTraceData.StartDateTime).TotalMilliseconds
            );
        }

        private static string GetFaultCode(string message)
        {
            var faultCode = "UnknownFault";
            
            try
            {
                var document = XDocument.Parse(message);
                var ns = document.Root?.Name.Namespace;
                var faultCodeElement = document.Root?.Element(ns + "Body")?.Element(ns + "Fault")?.Element(ns + "Code");
                if (faultCodeElement != null)
                {
                    faultCode = GetFaultCode(faultCodeElement);
                }
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    throw;
                }
            }

            return faultCode;
        }

        private static string GetFaultCode(XElement faultCodeElement)
        {
            var ns = faultCodeElement.Name.Namespace;
            var code = faultCodeElement.Element(ns + "Value")?.Value;

            if (code != null)
            {
                code = code.Substring(code.IndexOf(":", StringComparison.Ordinal) + 1);
                code = Regex.Replace(code, @"\s+", "_");
            }

            var subFaultCodeElement = faultCodeElement.Element(ns + "Subcode");
            if (subFaultCodeElement != null)
            {
                code += ":" + GetFaultCode(subFaultCodeElement);
            }

            return code;
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
