using System;
using System.Collections.Generic;
using System.Data.Common;
using System.ServiceModel;
using System.Threading;
using SCME.Types.Database;
using SCME.Types.Profiles;

namespace SCME.Types
{
    public class DatabaseProxy : ClientBase<IDbService>, IDbService
    {
  
        
        public DatabaseProxy(string name) : base(name)
        {
        }

        public Dictionary<string, int> GetMmeCodes()
        {
            return Channel.GetMmeCodes();
        }

        public List<MyProfile> GetProfilesDeepByMmeCode(string mmeCode)
        {
            try
            {
                return Channel.GetProfilesDeepByMmeCode(mmeCode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null)
        {
            return Channel.GetProfilesSuperficially(mmeCode, name);
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

        public List<string> GetMmeCodesByProfile(MyProfile profile, DbTransaction dbTransaction = null)
        {
            return Channel.GetMmeCodesByProfile(profile, dbTransaction);
        }

        public int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode)
        {
            return Channel.InsertUpdateProfile(oldProfile, newProfile, mmeCode);
        }

        public void RemoveProfile(MyProfile profile, string mmeCode)
        {
            Channel.RemoveProfile(profile, mmeCode);
        }

        public void RemoveMmeCode(string mmeCode)
        {
            throw new NotImplementedException();
        }

        public void RemoveMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null)
        {
            throw new NotImplementedException();
        }

        public void InsertMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null)
        {
            throw new NotImplementedException();
        }

        public void InsertMmeCode(string mmeCode)
        {
            Channel.InsertMmeCode(mmeCode);
        }

        public bool Migrate()
        {
            return Channel.Migrate();
        }
    }
}