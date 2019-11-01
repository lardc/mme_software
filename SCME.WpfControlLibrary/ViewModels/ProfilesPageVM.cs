using PropertyChanged;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCME.Types.Commutation;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProfilesPageProfileVm : EditProfileVm
    {
        public TestParametersType SelectedTestParametersType { get; set; } = TestParametersType.Gate;
        public Dictionary<string, int> MmeCodes { get; set; }

        public string SelectedMmeCode { get; set; }

        public string SearchingName { get; set; }

        public ObservableCollection<MyProfile> Profiles { get; set; }
        public ObservableCollection<MyProfile> LoadedProfiles { get; set; }

        public ProfileDeepData ProfileDeepDataCopy { get; set; }
        public string SelectedProfileNameCopy { get; set; }

        [DependsOn(nameof(SelectedProfile))] public bool IsClampingCommutationActive => SelectedProfile != null;
    }
}