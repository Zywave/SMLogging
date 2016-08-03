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
    /// <summary>
    /// Represents a message inspector for logging requests.
    /// </summary>
    /// <seealso cref="IDispatchMessageInspector" />
    /// <seealso cref="IClientMessageInspector" />
    public class RequestLoggingMessageInspector : IDispatchMessageInspector, IClientMessageInspector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingMessageInspector"/> class.
        /// </summary>
        public RequestLoggingMessageInspector()
        {
            CreateBufferedMessageCopy = false;
            IgnoreDispatchReplyMessage = false;
            TraceSource = new TraceSource("System.ServiceModel.RequestLogging");
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether to buffer the entire request and response messages in memory to get full message sizes and fault codes of streamed messages.
        /// </summary>
        public bool CreateBufferedMessageCopy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the dispatch reply message. This can be used to prevent request logging from accessing the reply message which may result in double execution when 
        /// unresolved IEnumerable objects are returned by the service. As a result, dispatch requests will be recorded having an 'Unknown' status rather than 'Fault/Success' and response size will be recorded as -1.
        /// </summary>
        public bool IgnoreDispatchReplyMessage { get; set; }
        
        /// <summary>
        /// Gets the trace source.
        /// </summary>
        public TraceSource TraceSource { get; }

        #region IDispatchMessageInspector Implementation

        /// <summary>
        /// Collects trace data about the request.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="channel">The incoming channel.</param>
        /// <param name="instanceContext">The current service instance.</param>
        /// <returns>
        /// The object used to correlate state. This object is passed back in the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.BeforeSendReply(System.ServiceModel.Channels.Message@,System.Object)" /> method.
        /// </returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var traceImmediately = false;

            var data = new RequestTraceData();

            data.ServerIpAddress = _machineIpAddress;
            data.ApplicationName = _applicationName;
            data.MachineName = _machineName;

            if (request != null)
            {
                Guid activityId;
                Guid correlationId;
                if (MessageHelpers.ExtractActivityAndCorrelationId(request, out activityId, out correlationId))
                {
                    data.ActivityId = activityId;
                    data.CorrelationId = correlationId;
                }

                Guid messageId;
                if (MessageHelpers.ExtractMessageId(request, out messageId))
                {
                    data.MessageId = messageId;
                }

                string remoteEndpointAddress;
                if (MessageHelpers.ExtractRemoteEndpointAddress(request, out remoteEndpointAddress))
                {
                    data.ClientIpAddress = remoteEndpointAddress;
                }

                data.Target = request.Headers.To;
                data.Action = request.Headers.Action;

                string requestContent;
                if (CreateBufferedMessageCopy)
                {
                    var bufferedCopy = request.CreateBufferedCopy(int.MaxValue);
                    request = bufferedCopy.CreateMessage();
                    requestContent = bufferedCopy.CreateMessage().ToString();
                }
                else
                {
                    requestContent = request.ToString();
                }
                
                data.RequestSize = Encoding.UTF8.GetByteCount(requestContent);

                data.IsOneWay = request.Headers.Action != null && request.Headers.ReplyTo == null;

                traceImmediately = data.IsOneWay.Value;
            }

            data.StartDateTime = DateTimeOffset.UtcNow;

            if (traceImmediately)
            {
                TraceRequest("Dispatch", data);

                return null;
            }

            return data;
        }

        /// <summary>
        /// Collects trace data about the reply and logs the request.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param>
        /// <param name="correlationState">The correlation object returned from the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.AfterReceiveRequest(System.ServiceModel.Channels.Message@,System.ServiceModel.IClientChannel,System.ServiceModel.InstanceContext)" /> method.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var data = correlationState as RequestTraceData;
            if (data == null)
            {
                return;
            }

            data.EndDateTime = DateTimeOffset.UtcNow;

            if (!IgnoreDispatchReplyMessage && reply != null)
            {
                string replyContent;
                if (CreateBufferedMessageCopy)
                {
                    var bufferedCopy = reply.CreateBufferedCopy(int.MaxValue);
                    reply = bufferedCopy.CreateMessage();
                    replyContent = bufferedCopy.CreateMessage().ToString();
                }
                else
                {
                    replyContent = reply.ToString();
                }

                data.ResponseSize = Encoding.UTF8.GetByteCount(replyContent);

                data.IsFault = reply.IsFault;
                data.FaultCode = reply.IsFault ? MessageHelpers.GetFaultCode(replyContent) : null;
            }

            TraceRequest("Dispatch", data);
        }

        #endregion

        #region IClientMessageInspector Implementation

        /// <summary>
        /// Collects trace data about the request.
        /// </summary>
        /// <param name="request">The message to be sent to the service.</param>
        /// <param name="channel">The  client object channel.</param>
        /// <returns>
        /// The object that is returned as the correlationState argument of the <see cref="M:System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)" /> method. 
        /// This is null if no correlation state is used.The best practice is to make this a <see cref="T:System.Guid" /> to ensure that no two <paramref name="correlationState" /> objects are the same.
        /// </returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var traceImmediately = false;

            var data = new RequestTraceData();

            data.ClientIpAddress = _machineIpAddress;
            data.ApplicationName = _applicationName;
            data.MachineName = _machineName;

            if (request != null)
            {
                Guid activityId;
                Guid correlationId;
                if (MessageHelpers.ExtractActivityAndCorrelationId(request, out activityId, out correlationId))
                {
                    data.ActivityId = activityId;
                    data.CorrelationId = correlationId;
                }

                Guid messageId;
                if (MessageHelpers.ExtractMessageId(request, out messageId))
                {
                    data.MessageId = messageId;
                }

                data.Action = request.Headers.Action;

                string requestContent;
                if (CreateBufferedMessageCopy)
                {
                    var bufferedCopy = request.CreateBufferedCopy(int.MaxValue);
                    request = bufferedCopy.CreateMessage();
                    requestContent = bufferedCopy.CreateMessage().ToString();
                }
                else
                {
                    requestContent = request.ToString();
                }

                data.RequestSize = Encoding.UTF8.GetByteCount(requestContent);

                data.IsOneWay = request.Headers.Action != null && request.Headers.ReplyTo == null;

                traceImmediately = data.IsOneWay.Value;
            }

            if (channel != null)
            {
                data.Target = channel.Via;
            }

            data.StartDateTime = DateTimeOffset.UtcNow;

            if (traceImmediately)
            {
                TraceRequest("Client", data);

                return null;
            }

            return data;
        }

        /// <summary>
        /// Collects trace data about the reply and logs the request.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param>
        /// <param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var data = correlationState as RequestTraceData;
            if (data == null)
            {
                return;
            }

            data.EndDateTime = DateTimeOffset.UtcNow;

            if (reply != null)
            {
                string replyContent;
                if (CreateBufferedMessageCopy)
                {
                    var bufferedCopy = reply.CreateBufferedCopy(int.MaxValue);
                    reply = bufferedCopy.CreateMessage();
                    replyContent = bufferedCopy.CreateMessage().ToString();
                }
                else
                {
                    replyContent = reply.ToString();
                }

                data.ResponseSize = Encoding.UTF8.GetByteCount(replyContent);

                data.IsFault = reply.IsFault;
                data.FaultCode = reply.IsFault ? MessageHelpers.GetFaultCode(replyContent) : null;
            }

            TraceRequest("Client", data);
        }

        #endregion

        private void TraceRequest(string disposition, RequestTraceData data)
        {
            string status = null;
            if (data.IsOneWay.HasValue && data.IsOneWay.Value)
            {
                status = "OneWay";
            }
            else if (data.IsFault.HasValue)
            {
                status = data.IsFault.Value ? "Fault" : "Success";
            }

            int? timeTaken = null;
            if (data.StartDateTime.HasValue && data.EndDateTime.HasValue)
            {
                timeTaken = Convert.ToInt32((data.EndDateTime.Value - data.StartDateTime.Value).TotalMilliseconds);
            }

            TraceSource.TraceData(TraceEventType.Information, 0,
                data.ActivityId,
                data.CorrelationId,
                data.MessageId,
                disposition,
                data.StartDateTime?.ToString("yyyy-MM-dd") ?? "null",
                data.StartDateTime?.ToString("HH:mm:ss.FFF") ?? "null",
                data.ClientIpAddress ?? "0.0.0.0",
                data.ApplicationName ?? "null",
                data.MachineName ?? "null",
                data.ServerIpAddress ?? "0.0.0.0",
                data.Target?.Scheme ?? "null",
                data.Target?.Host ?? "null",
                data.Target?.Port ?? 0,
                data.Target?.ToString() ?? "null",
                data.Action ?? "null",
                status ?? "Unknown",
                data.FaultCode ?? "null",
                data.ResponseSize ?? -1,
                data.RequestSize ?? -1,
                timeTaken ?? -1
           );
        }
        
        private static readonly string _machineName;
        private static readonly string _machineIpAddress;
        private static readonly string _applicationName;

        static RequestLoggingMessageInspector()
        {
            _machineName = Dns.GetHostName();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _machineIpAddress = ip.ToString();
                    break;;
                }
            }

            _applicationName = AppDomain.CurrentDomain.FriendlyName;
        }
    }
}
