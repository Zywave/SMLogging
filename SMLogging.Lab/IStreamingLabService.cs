using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace SMLogging.Lab
{
    [ServiceContract]
    public interface IStreamingLabService
    {
        [OperationContract]
        StreamingResponse GetData(StreamingRequest request);
    }
    
}