using System;
using System.Collections.Generic;
using SCME.Types;

namespace SCME.Interfaces
{
    public interface IProfilesService: IDisposable
    {
        void SaveProfiles(List<ProfileItem> profileItems);

        List<ProfileItem> GetProfileItems();

        List<ProfileItem> GetProfileItems(string mmeCode);

        void SaveProfilesWithMme(List<ProfileItem> profileItems, string mmeCode);
    }
}
