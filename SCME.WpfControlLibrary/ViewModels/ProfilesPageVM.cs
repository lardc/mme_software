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
    public class ProfilesPageVm
    {
        public TestParametersType SelectedTestParametersType { get; set; } = TestParametersType.Gate;
        public Dictionary<string, int> MmeCodes { get; set; }

        public string SelectedMmeCode { get; set; }

        public string SearchingName { get; set; }

        [DependsOn(nameof(SelectedProfile), nameof(IsEditModeActive))] 
        public bool IsCancelSaveModeEnabled => IsEditModeActive;
        
        [DependsOn(nameof(SelectedProfile), nameof(IsEditModeActive))]
        public bool IsEditModeEnabled => IsEditModeActive == false && SelectedProfile != null;
        
        public bool IsEditModeActive { get; set; }
        
        [DependsOn(nameof(IsEditModeActive))] public bool IsEditModeInActive => !IsEditModeActive;
        
        [DependsOn(nameof(SelectedProfile))] public bool IsClampingCommutationActive => SelectedProfile != null; 

        public ObservableCollection<MyProfile> Profiles { get; set; }
        public ObservableCollection<MyProfile> LoadedProfiles { get; set; }

        public ProfileDeepData ProfileDeepDataCopy { get; set; }
        public string SelectedProfileNameCopy { get; set; }


        public MyProfile SelectedProfile { get; set; }
        
    }
}