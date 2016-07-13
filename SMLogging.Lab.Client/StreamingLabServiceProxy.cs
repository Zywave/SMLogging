using System.ServiceModel;
using System.Threading.Tasks;

namespace SMLogging.Lab.Client
{
    public class StreamingLabServiceProxy : ClientBase<IStreamingLabService>, IStreamingLabService
    {
        public StreamingResponse GetData(StreamingRequest request)
        {
            return Channel.GetData(request);
        }
    }
}
