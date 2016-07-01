using System.Threading;

namespace SMLogging.Lab
{
    public class LabService : ILabService
    {
        public string GetData(int value)
        {
            return value.ToString();
        }
    }
}
