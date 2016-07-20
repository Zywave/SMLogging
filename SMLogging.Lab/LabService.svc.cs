using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SMLogging.Lab
{
    public class LabService : ILabService
    {
        public string GetData(int value)
        {
            throw new Exception("BAM");

            return value.ToString();
        }

        public Task<string> GetData2(int value)
        {
            return Task.FromResult(value.ToString());
        }

        public void DoSomething(int value)
        {
            
        }
    }
}
