using System.Threading;

namespace SMRequestLogging.Lab
{
    public class LabService : ILabService
    {
        public string GetData(int value)
        {
            return value.ToString();
        }
    }
}
