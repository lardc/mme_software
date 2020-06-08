using System.Collections.Generic;
using System.ServiceModel;
using SCME.Types.Profiles;

namespace SCME.Types
{
    [ServiceContract(Namespace = "http://proton-electrotex.com/SCME",
        SessionMode = SessionMode.Required)]
    public interface IProfileProviderService
    {
        [OperationContract]
        [FaultContract(typeof(FaultData))]
        string GetProfileListAsXml(string MMECode);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<Profile> GetProfileList(string MMECode);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        ProfileDictionaryObject ReadProfileDictionary(string ManagerID);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void WriteProfileDictionary(ProfileDictionaryObject Dictionary, string ManagerID);
    }
}