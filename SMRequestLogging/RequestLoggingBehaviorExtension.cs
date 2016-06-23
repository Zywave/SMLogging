using System;
using System.ServiceModel.Configuration;

namespace SMRequestLogging
{
    public class RequestLoggingBehaviorExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new RequestLoggingServiceBehavior();
        }

        public override Type BehaviorType
        {
            get { return typeof(RequestLoggingServiceBehavior); }
        }
    }
}
