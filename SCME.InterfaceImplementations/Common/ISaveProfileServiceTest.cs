using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations.Common
{
    public interface ISaveProfileServiceTest
    {
        int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string MMECode);
        void RemoveProfile(MyProfile profile);
    }
}
