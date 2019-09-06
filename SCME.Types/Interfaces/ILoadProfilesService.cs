using SCME.Types.Profiles;
using System;
using System.Collections.Generic;

namespace SCME.Types.Interfaces
{
    public interface ILoadProfilesService
    {
        List<ProfileItem> GetProfileItemsSuperficially(string mmeCode);
        List<ProfileItem> GetProfileItemsDeep(string mmeCode);
        List<ProfileItem> GetProfileItemsWithChildSuperficially(string mmeCode);
        Profile GetProfileDeep(Guid key);

        List<ProfileItem> GetProfileItems();
        List<ProfileItem> GetProfileItems(string mmeCode);
        ProfileItem GetProfileByProfName(string profName, string mmmeCode, ref bool Found);
    }
}
