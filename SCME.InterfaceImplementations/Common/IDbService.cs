using System.Collections.Generic;
using SCME.Types.Profiles;

namespace SCME.InterfaceImplementations.Common
{
    public interface IDbService
    {
        Dictionary<string, int> GetMmeCodes();
        List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null);
        List<MyProfile> GetProfileChildSuperficially(MyProfile profile);
        ProfileDeepData LoadProfileDeepData(MyProfile profile);
        int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode);
        void RemoveProfile(MyProfile profile);
    }
}