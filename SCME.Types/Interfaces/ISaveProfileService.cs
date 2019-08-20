using SCME.Types.SQL;
using System.Collections.Generic;

namespace SCME.Types.Interfaces
{
    public interface ISaveProfileService
    {
        ProfileForSqlSelect SaveProfileItem(ProfileItem profileItem);

        void DeleteProfiles(List<ProfileItem> profilesToDelete);

        void DeleteProfiles(List<ProfileItem> profilesToDelete, string mmeCode);

        ProfileForSqlSelect SaveProfileItem(ProfileItem profileItem, string mmeCode);
    }
}
