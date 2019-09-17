using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations.Common
{
    public interface ILoadProfilesServiceTest
    {
        Dictionary<string, int> GetMMECodes();
        List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null);
        List<MyProfile> GetProfileChildsSuperficially(MyProfile profile);
        ProfileDeepData LoadProfileDeepData(MyProfile profile);
    }
}
