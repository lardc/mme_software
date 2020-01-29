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
        [FaultContract(typeof(Exception))]
        [OperationContract]
        (MyProfile profile, bool IsInMmeCode) GetTopProfileByName(string mmeCode, string name);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        void ClearCacheByMmeCode(string mmeCode);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        Dictionary<string, int> GetMmeCodes();

        [FaultContract(typeof(Exception))]
        [OperationContract]
        List<MyProfile> GetProfilesDeepByMmeCode(string mmeCode);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        List<MyProfile> GetProfileChildSuperficially(MyProfile profile);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        void InvalidCacheById(int id, string mmeCode);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        ProfileDeepData LoadProfileDeepData(MyProfile profile);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        bool ProfileNameExists(string profileName);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        string GetFreeProfileName();

        [FaultContract(typeof(Exception))]
        [OperationContract]
        List<string> GetMmeCodesByProfile(int profileId, DbTransaction dbTransaction = null);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        void RemoveProfile(MyProfile profile, string mmeCode);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        void RemoveMmeCode(string mmeCode);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        void RemoveMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        void InsertMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        void InsertMmeCode(string mmeCode);

        [FaultContract(typeof(Exception))]
        [OperationContract]
        bool Migrate();
    }
}