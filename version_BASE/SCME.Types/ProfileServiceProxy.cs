using System.Collections.Generic;
using System.ServiceModel;
using SCME.Types.Profiles;

namespace SCME.Types
{
    public class ProfileServiceProxy : ClientBase<IProfileProviderService>, IProfileProviderService
    {
        public ProfileServiceProxy(string ServerEndpointConfigurationName)
            : base(ServerEndpointConfigurationName)
        {
        }

        public List<Profile> GetProfileList(string MMECode)
        {
            return Channel.GetProfileList(MMECode);
        }

        public ProfileDictionaryObject ReadProfileDictionary(string ManagerID)
        {
            return Channel.ReadProfileDictionary(ManagerID);
        }

        public void WriteProfileDictionary(ProfileDictionaryObject Dictionary, string ManagerID)
        {
            Channel.WriteProfileDictionary(Dictionary, ManagerID);
        }

        public string GetProfileListAsXml(string MMECode)
        {
            return Channel.GetProfileListAsXml(MMECode);
        }
    }
}