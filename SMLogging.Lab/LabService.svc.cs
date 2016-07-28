using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SMLogging.Lab
{
    public class LabService : ILabService
    {
        public string GetData(int value)
        {
            Debug.WriteLine($"GetData called: {value}");
            return value.ToString();
        }

        public IEnumerable<string> GetDatas(IEnumerable<int> values)
        {
            return values.Select(GetData);
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
