using SCME.Types.SQL;
using System;
using System.Collections.Generic;

namespace SCME.Types.DatabaseServer
{
    public interface IProfilesService: IDisposable
    {

        List<ProfileForSqlSelect> SaveProfiles(List<ProfileItem> profileItems);

        
        List<ProfileItem> GetProfileItems();

        
        List<ProfileItem> GetProfileItemsByMme(string mmeCode);


        List<ProfileForSqlSelect> SaveProfilesFromMme(List<ProfileItem> profileItems, string mmeCode);


        ProfileItem GetProfileByProfName(string profName, string mmmeCode, ref bool Found);
    }
}
