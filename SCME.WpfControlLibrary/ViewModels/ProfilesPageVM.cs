using PropertyChanged;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProfilesPageVM
    {
        public List<Profile> Profiles { get; set; }
        public Profile SelectedProfile { get; set; }
    }
}
