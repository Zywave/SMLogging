using System;

namespace SMLogging.Lab
{
    public class LabService : ILabService
    {
        public string GetData(int value)
        {
            throw new InvalidOperationException();

            return value.ToString();
        }
    }
}
