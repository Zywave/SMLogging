using System;
using System.ServiceModel.Configuration;

namespace SMLogging
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
