using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace SMLogging
{
    /// <summary>
    /// Represents a error handler service behavior that logs service exceptions.
    /// </summary>
    /// <seealso cref="IServiceBehavior"/>
    /// <seealso cref="IEndpointBehavior"/>
    [AttributeUsage(AttributeTargets.Class)]
    public class ErrorLoggingBehavior : Attribute, IServiceBehavior, IEndpointBehavior
    {
        /// <summary>
        /// Initializes a new instance of the  <see cref="ErrorLoggingBehavior"/> class.
        /// </summary>
        public ErrorLoggingBehavior()
            : this(true)
        {
        }

        internal ErrorLoggingBehavior(bool enabled)
        {
            Enabled = enabled;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ErrorLoggingBehavior"/> is enabled.
        /// </summary>
        public bool Enabled { get; }

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
        /// Adds the <see cref="ErrorLoggingErrorHandler"/> to all of the the channel dispatchers.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null) throw new ArgumentNullException(nameof(serviceDescription));
            if (serviceHostBase == null) throw new ArgumentNullException(nameof(serviceHostBase));

            if (Enabled)
            {
                for (var i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
                {
                    var channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                    if (channelDispatcher != null)
                    {
                        channelDispatcher.ErrorHandlers.Add(new ErrorLoggingErrorHandler());
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
                endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new ErrorLoggingErrorHandler());
            }
        }

        /// <summary>
        /// Not implemented. Error handling not supported by client runtime.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            
        }

        /// <summary>
        /// Not implemented. No endpoint validation necessary.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}