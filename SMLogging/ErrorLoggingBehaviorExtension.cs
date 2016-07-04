using System;
using System.ServiceModel.Configuration;

namespace SMLogging
{
    public class ErrorLoggingBehaviorExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new ErrorLoggingBehavior();
        }

        public override Type BehaviorType
        {
            get { return typeof(ErrorLoggingBehavior); }
        }
    }
}
