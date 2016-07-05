using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace SMLogging
{
    /// <summary>
    /// Represents a error handler service behavior that logs service exceptions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ErrorLoggingBehavior : Attribute, IServiceBehavior, IErrorHandler
    {
        /// <summary>
        /// Initializes a new instance of the  <see cref="ErrorLoggingBehavior"/> class.
        /// </summary>
        public ErrorLoggingBehavior()
        {
            _traceSource = new TraceSource("System.ServiceModel.ErrorLogging");
        }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter 
        /// interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null) throw new ArgumentNullException("serviceDescription");
            if (serviceHostBase == null) throw new ArgumentNullException("serviceHostBase");
            
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                dispatcher.ErrorHandlers.Add(this);
            }
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

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
                var context = OperationContext.Current;

                var operationContext = OperationContext.Current;

                RemoteEndpointMessageProperty remoteEndpoint = null;
                if (operationContext.IncomingMessageProperties.ContainsKey(RemoteEndpointMessageProperty.Name))
                {
                    remoteEndpoint = operationContext.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                }

                var clientIpAddress = remoteEndpoint?.Address;
                var target = operationContext.IncomingMessageHeaders.To;
                var action = operationContext.IncomingMessageHeaders.Action;

                _traceSource.TraceData(TraceEventType.Error, 0,
                    clientIpAddress,
                    _processName,
                    _serverName,
                    _serverIpAddress,
                    target.Scheme,
                    target.Host,
                    target.Port,
                    target,
                    action,
                    error);
            }
        }
        
        private readonly TraceSource _traceSource;

        private static readonly string _serverName;
        private static readonly string _serverIpAddress;
        private static readonly string _processName;

        static ErrorLoggingBehavior()
        {
            _serverName = Dns.GetHostName();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _serverIpAddress = ip.ToString();
                    break; ;
                }
            }

            _processName = AppDomain.CurrentDomain.FriendlyName;
        }
    }
}