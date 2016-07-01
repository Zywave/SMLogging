using System.ServiceModel;

namespace SMLogging.Lab.Client
{
    public class LabServiceProxy : ClientBase<ILabService>, ILabService
    {
        public string GetData(int value)
        {
            return Channel.GetData(value);
        }
    }
}
