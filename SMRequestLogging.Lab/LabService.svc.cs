using System.Threading;

namespace SMRequestLogging.Lab
{
    public class LabService : ILabService
    {
        public string GetData(int value)
        {
            //Thread.Sleep(100);
            return value.ToString();
        }
    }
}
