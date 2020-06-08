using System.Collections.Generic;
using SCME.Types;

namespace SCME.Interfaces
{
    public interface ISaveProfileService
    {
        long SaveProfileItem(ProfileItem profileItem);

        void DeleteProfiles(List<ProfileItem> profilesToDelete);

        void DeleteProfiles(List<ProfileItem> profilesToDelete, string mmeCode);

        void SaveProfileItem(ProfileItem profileItem, string mmeCode);
    }
}
