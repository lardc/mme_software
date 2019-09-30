using System.Collections.Generic;
using System.ServiceModel;
using SCME.Types.Profiles;

namespace SCME.Types.Database
{
    [ServiceContract(Namespace = "http://proton-electrotex.com/SCME", SessionMode = SessionMode.Required)]
    public interface IDbService
    {
        [OperationContract]
        Dictionary<string, int> GetMmeCodes();
        
        [OperationContract]
        List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null);
        
        [OperationContract]
        List<MyProfile> GetProfileChildSuperficially(MyProfile profile);
        
        [OperationContract]
        ProfileDeepData LoadProfileDeepData(MyProfile profile);
        
        [OperationContract]
        bool ProfileNameExists(string profileName);

        [OperationContract]
        string GetFreeProfileName();
        
        [OperationContract]
        int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode);

        [OperationContract]
        void RemoveProfile(MyProfile profile, string mmeCode);
    }
}