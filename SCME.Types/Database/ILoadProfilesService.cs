using System.Collections.Generic;
using SCME.Types.Profiles;

namespace SCME.Types.Database
{
    public interface ILoadProfilesService
    {
        Dictionary<string, int> GetMmeCodes();
        List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null);
        List<MyProfile> GetProfileChildSuperficially(MyProfile profile);
        ProfileDeepData LoadProfileDeepData(MyProfile profile);
    }
}
