using System;
using System.IO;
using System.ServiceModel;

namespace SMLogging.Lab
{
    public class StreamingLabService : IStreamingLabService
    {
        [OperationBehavior(AutoDisposeParameters = true)]
        public StreamingResponse GetData(StreamingRequest request)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(request.Value);
            writer.Flush();
            stream.Position = 0;
            return new StreamingResponse()
            {
                Data = stream,
                Length = stream.Length
            };
        }
    }
}
