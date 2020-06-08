using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using SCME.Types;
using SCME.Types.Profiles;
using SCME.Types.Utils;

namespace SCME.ProfileServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        Namespace = "http://proton-electrotex.com/SCME",
        ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    public class ProfileServer : IProfileProviderService
    {
        List<Profile> IProfileProviderService.GetProfileList(string MMECode)
        {
            try
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Info, String.Format("Profile list request from {0}", MMECode));

                var list = GenerateListFromMMECode(MMECode);

                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Info, String.Format("Returned {0} profiles for {1}", list.Count, MMECode));

                return list;
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Error, ex.Message);
            }

            return null;
        }

        string IProfileProviderService.GetProfileListAsXml(string MMECode)
        {
            try
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Info, String.Format("Profile list request from {0} as XML", MMECode));

                var list = GenerateListFromMMECode(MMECode);

                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Info, String.Format("Returned {0} profiles for {1} as XML", list.Count, MMECode));

                return ProfileDictionary.SerializeToXmlAsString(list);
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Error, ex.Message);
            }

            return "";
        }

        ProfileDictionaryObject IProfileProviderService.ReadProfileDictionary(string ManagerID)
        {
            try
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Info, String.Format("Profile dictionary request from {0}", ManagerID));

                if (SystemHost.Dictionary == null)
                {
                    SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Warning, "Dictionary is not loaded properly - return empty result");
                    return null;
                }

                return SystemHost.Dictionary.Root;
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Error, ex.Message);
            }

            return null;
        }

        void IProfileProviderService.WriteProfileDictionary(ProfileDictionaryObject Dictionary, string ManagerID)
        {
            try
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Info, String.Format("Profile dictionary update from {0}", ManagerID));

                if (SystemHost.Dictionary == null)
                {
                    SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Warning, "Dictionary is not loaded properly - operation cancelled");
                    return;
                }

                SystemHost.Dictionary.Root = Dictionary;
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Error, ex.Message);
            }
        }

        private static List<Profile> GenerateListFromMMECode(string MMECode)
        {
            if (SystemHost.Dictionary == null || SystemHost.Dictionary.Root == null)
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Warning, "Dictionary is not loaded properly - return empty result");
                return null;
            }

            if (SystemHost.ConfigList == null || SystemHost.ConfigList.Configurations == null)
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Warning, "Configuration list is not loaded properly - return empty result");
                return null;
            }

            var guidList = SystemHost.ConfigList.Configurations.FirstOrDefault(Arg => Arg.MMECode == MMECode);

            if (guidList == null)
            {
                SystemHost.Journal.AppendLog("ProfileServer", LogJournalMessageType.Warning, String.Format("No such code {0} - return empty result", MMECode));
                return null;
            }

            var list = new List<Profile>();
            AddMembersMatchedToConfig(list, SystemHost.Dictionary.Root, guidList.ProfileKeyList);

            return list;
        }

        private static void AddMembersMatchedToConfig(ICollection<Profile> ProfileList, ProfileDictionaryObject Parent, IList<Guid> Config)
        {
            foreach (var member in Parent.ChildrenList)
            {
                var item = member as Profile;

                if (item != null)
                {
                    if(Config.Any(Arg => Arg == item.Key))
                        ProfileList.Add(item);
                }
                else
                    AddMembersMatchedToConfig(ProfileList, member, Config);
            }
        }
    }
}
