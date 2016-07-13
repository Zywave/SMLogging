using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace SMLogging.Lab
{
    [MessageContract(WrapperName = "StreamingRequest", IsWrapped = true)]
    public class StreamingRequest
    {
        [MessageBodyMember(Order = 1)]
        public int Value { get; set; }
    }
}