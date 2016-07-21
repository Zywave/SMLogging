using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace SMLogging
{

    /// <summary>
    /// Represents a <see cref="BehaviorExtensionElement"/> for <see cref="RequestLoggingBehavior"/>.
    /// </summary>
    /// <seealso cref="BehaviorExtensionElement" />
    public class RequestLoggingBehaviorExtension : BehaviorExtensionElement
    {
        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>
        /// The behavior extension.
        /// </returns>
        protected override object CreateBehavior()
        {
            return new RequestLoggingBehavior(Enabled, CreateBufferedMessageCopy, IgnoreDispatchReplyMessage, AddMessageIdRequestHeader);
        }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(RequestLoggingBehavior); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the behavior is enabled.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to buffer the entire request and response messages in memory to get full message sizes and fault codes of streamed messages.
        /// </summary>
        [ConfigurationProperty("createBufferedMessageCopy", DefaultValue = false)]
        public bool CreateBufferedMessageCopy
        {
            get { return (bool)this["createBufferedMessageCopy"]; }
            set { this["createBufferedMessageCopy"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the dispatch reply message. This can be used to prevent request logging from accessing the reply message which may result in double execution when 
        /// unresolved IEnumerable objects are returned by the service. As a result, dispatch requests will be recorded having an 'Unknown' status rather than 'Fault/Success' and response size will be recorded as -1.
        /// </summary>
        [ConfigurationProperty("ignoreDispatchReplyMessage", DefaultValue = false)]
        public bool IgnoreDispatchReplyMessage
        {
            get { return (bool)this["ignoreDispatchReplyMessage"]; }
            set { this["ignoreDispatchReplyMessage"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the client should add a message ID request header when it is not avaiable.
        /// </summary>
        [ConfigurationProperty("addMessageIdRequestHeader", DefaultValue = true)]
        public bool AddMessageIdRequestHeader
        {
            get { return (bool)this["addMessageIdRequestHeader"]; }
            set { this["addMessageIdRequestHeader"] = value; }
        }
    }
}
