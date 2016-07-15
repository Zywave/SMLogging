using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace SMLogging
{
    /// <summary>
    /// Represents an error handler for logging service errors.
    /// </summary>
    /// <seealso cref="IErrorHandler" />
    public class ErrorLoggingErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLoggingErrorHandler"/> class.
        /// </summary>
        public ErrorLoggingErrorHandler()
        {
            TraceSource = new TraceSource("System.ServiceModel.ErrorLogging");
        }

        /// <summary>
        /// Gets the trace source.
        /// </summary>
        public TraceSource TraceSource { get; }

        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether the dispatcher aborts the session and the instance 
        /// context in certain cases.
        /// </summary>
        /// <param name="error">The exception thrown during processing.</param>
        /// <returns>
        /// true if Windows Communication Foundation (WCF) should not abort the session (if there is one) and instance context if the instance
        ///  context is not <see cref="System.ServiceModel.InstanceContextMode.Single" />; otherwise, false. The default is false.
        /// </returns>
        public bool HandleError(Exception error)
        {
            //Logging error from ProvideFault instead, so that operation context can be accessed.

            return false;
        }

        /// <summary>
        /// Enables the creation of a custom <see cref="System.ServiceModel.FaultException{T}" /> that is returned from an exception in the course of a service method.
        /// </summary>
        /// <param name="error">The <see cref="System.Exception" /> object thrown in the course of the service operation.</param>
        /// <param name="version">The SOAP version of the message.</param>
        /// <param name="fault">The <see cref="System.ServiceModel.Channels.Message" /> object that is returned to the client, or service, in the duplex case.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error != null)
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

                var messageId = GetMessageId(operationContext?.IncomingMessageHeaders?.MessageId);
                var clientIpAddress = remoteEndpoint?.Address;
                var target = operationContext?.IncomingMessageHeaders?.To;
                var action = operationContext?.IncomingMessageHeaders?.Action;

                TraceSource.TraceData(TraceEventType.Error, 0,
                    messageId ?? "null",
                    clientIpAddress ?? "0.0.0.0",
                    _applicationName,
                    _machineName,
                    _machineIpAddress ?? "0.0.0.0",
                    target?.Scheme ?? "null",
                    target?.Host ?? "null",
                    target?.Port ?? 0,
                    target?.ToString() ?? "null",
                    action ?? "null",
                    error);
            }
        }

        private static string GetMessageId(UniqueId messageUniqueId)
        {
            if (messageUniqueId != null)
            {
                Guid messageGuid;
                if (messageUniqueId.TryGetGuid(out messageGuid))
                {
                    return messageGuid.ToString();
                }

                return messageUniqueId.ToString();
            }

            return null;
        }

        private static readonly string _machineName;
        private static readonly string _machineIpAddress;
        private static readonly string _applicationName;

        static ErrorLoggingErrorHandler()
        {
            _machineName = Dns.GetHostName();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _machineIpAddress = ip.ToString();
                    break; ;
                }
            }

            _applicationName = AppDomain.CurrentDomain.FriendlyName;
        }
    }
}
