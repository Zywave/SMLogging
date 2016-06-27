using System.Threading;

namespace SMRequestLogging.Lab
{
    public class LabService : ILabService
    {
        public string GetData(int value)
        {
            Thread.Sleep(10);
            return value.ToString();
        }
    }
}
