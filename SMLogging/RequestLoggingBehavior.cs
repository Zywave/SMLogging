using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace SMLogging
{
    /// <summary>
    /// Represents a request logging service behavior that adds a message inspector to log service requests.
    /// </summary>
    /// <seealso cref="IServiceBehavior"/>
    /// <seealso cref="IEndpointBehavior"/>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequestLoggingBehavior : Attribute, IServiceBehavior, IEndpointBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingBehavior"/> class.
        /// </summary>
        public RequestLoggingBehavior()
            : this(true, false)
        {
            
        }

        internal RequestLoggingBehavior(bool enabled, bool createBufferedMessageCopy)
        {
            Enabled = enabled;
            CreateBufferedMessageCopy = createBufferedMessageCopy;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="RequestLoggingBehavior"/> is enabled.
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to buffer the entire request and response messages in memory to get full message sizes and fault codes of streamed messages.
        /// </summary>
        public bool CreateBufferedMessageCopy { get; set; }

        #region IServiceBehavior Implementation

        /// <summary>
        /// Not implemented. No binding parameters necessary.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Adds the <see cref="RequestLoggingMessageInspector"/> to all of the the endpoint dispatchers.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (Enabled)
            {
                for (var i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
                {
                    var channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                    if (channelDispatcher != null)
                    {
                        foreach (var endpointDispatcher in channelDispatcher.Endpoints)
                        {
                            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(CreateMessageInspector());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Not implemented. No service validation necessary.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion

        #region IEndpointBehavior Implementation

        /// <summary>
        /// Not implemented. No binding parameters necessary.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Adds the <see cref="RequestLoggingMessageInspector"/> to the endpoint dispatcher.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (Enabled)
            {
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(CreateMessageInspector());
            }
        }

        /// <summary>
        /// Adds the <see cref="RequestLoggingMessageInspector"/> to the client runtime.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (Enabled)
            {
                clientRuntime.MessageInspectors.Add(CreateMessageInspector());
            }
        }

        /// <summary>
        /// Not implemented. No endpoint validation necessary.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion

        private RequestLoggingMessageInspector CreateMessageInspector()
        {
            return new RequestLoggingMessageInspector
            {
                CreateBufferedMessageCopy = CreateBufferedMessageCopy
            };
        }
    }
}