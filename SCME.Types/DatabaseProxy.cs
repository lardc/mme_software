using System.Collections.Generic;
using System.ServiceModel;
using SCME.Types.Database;
using SCME.Types.Profiles;

namespace SCME.Types
{
    public class DatabaseProxy : ClientBase<IDbService>, IDbService
    {
        public DatabaseProxy() : base("SCME.LocalDatabase")
        {
            
        }
        
        public Dictionary<string, int> GetMmeCodes()
        {
            return Channel.GetMmeCodes();
        }

        public List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null)
        {
            return Channel.GetProfilesSuperficially(mmeCode,name);
        }

        public List<MyProfile> GetProfileChildSuperficially(MyProfile profile)
        {
            return Channel.GetProfileChildSuperficially(profile);
        }

        public ProfileDeepData LoadProfileDeepData(MyProfile profile)
        {
            return Channel.LoadProfileDeepData(profile);
        }

        public bool ProfileNameExists(string profileName)
        {
            return Channel.ProfileNameExists(profileName);
        }

        public string GetFreeProfileName()
        {
            return Channel.GetFreeProfileName();
        }

        public int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode)
        {
            return Channel.InsertUpdateProfile(oldProfile, newProfile, mmeCode);
        }

        public void RemoveProfile(MyProfile profile, string mmeCode)
        {
            Channel.RemoveProfile(profile, mmeCode);
        }
    }
}