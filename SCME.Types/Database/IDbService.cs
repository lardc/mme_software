using System;
using System.Collections.Generic;
using System.Data.Common;
using System.ServiceModel;
using SCME.Types.Profiles;

namespace SCME.Types.Database
{
    [ServiceContract(Namespace = "http://proton-electrotex.com/SCME", SessionMode = SessionMode.Required)]
    public interface IDbService
    {
        [OperationContract]
        Dictionary<string, int> GetMmeCodes();

        [FaultContract(typeof(Exception))]
        [OperationContract]
        List<MyProfile> GetProfilesDeepByMmeCode(string mmeCode);
        
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
        List<string> GetMmeCodesByProfile(MyProfile profile);
        
        [OperationContract]
        int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode);

        [OperationContract]
        void RemoveProfile(MyProfile profile, string mmeCode);

        [OperationContract]
        void RemoveMmeCode(string mmeCode);
        
        [OperationContract]
        void RemoveMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null);

        [OperationContract]
        void InsertMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null);

        [OperationContract]
        void InsertMmeCode(string mmeCode);
    }
}