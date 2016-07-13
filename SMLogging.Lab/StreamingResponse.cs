using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace SMLogging.Lab
{
    [MessageContract(WrapperName = "StreamingResponse", IsWrapped = true)]
    public class StreamingResponse : IDisposable
    {
        [MessageHeader]
        public long Length { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream Data { get; set; }

        public void Dispose()
        {
            if (Data != null)
            {
                Data.Dispose();
                Data = null;
            }
        }
    }
}