using System;
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
            return new RequestLoggingBehavior();
        }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(RequestLoggingBehavior); }
        }
    }
}
