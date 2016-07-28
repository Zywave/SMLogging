using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SMLogging.Lab
{
    [ServiceContract]
    public interface ILabService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        IEnumerable<string> GetDatas(IEnumerable<int> values);

        [OperationContract]
        Task<string> GetData2(int value);

        [OperationContract(IsOneWay = true)]
        void DoSomething(int value);
    }
}
