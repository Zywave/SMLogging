using System;
using System.ServiceModel.Configuration;

namespace SMLogging
{
    /// <summary>
    /// Represents a <see cref="BehaviorExtensionElement"/> for <see cref="ErrorLoggingBehavior"/>.
    /// </summary>
    /// <seealso cref="BehaviorExtensionElement" />
    public class ErrorLoggingBehaviorExtension : BehaviorExtensionElement
    {
        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>
        /// The behavior extension.
        /// </returns>
        protected override object CreateBehavior()
        {
            return new ErrorLoggingBehavior();
        }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(ErrorLoggingBehavior); }
        }
    }
}
