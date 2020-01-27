using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCME.Types.Profiles;

namespace SCME.InterfaceImplementations.Common
{
    public class ProfileCache
    {
        public  MyProfile Profile { get; set; }
        public  bool IsDeepLoad { get; set; }
        public  bool IsChildLoad { get; set; }
        //public bool IsMmeCodesLoad { get; set; }
        public List<string> MmeCodes { get; set; }

        
        
        public ProfileCache(MyProfile profile)
        {
            Profile = profile ?? throw new ArgumentNullException();
        }

        
    }
}
