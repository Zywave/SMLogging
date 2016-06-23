using System.ServiceModel;

namespace SMRequestLogging.Lab
{
    [ServiceContract]
    public interface ILabService
    {
        [OperationContract]
        string GetData(int value);
    }
}
