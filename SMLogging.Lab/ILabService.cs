using System.ServiceModel;

namespace SMLogging.Lab
{
    [ServiceContract]
    public interface ILabService
    {
        [OperationContract]
        string GetData(int value);
    }
}
