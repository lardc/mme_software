using PropertyChanged;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SCME.Types.Commutation;
using SCME.WpfControlLibrary.Commands;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProfilesPageProfileVm : EditProfileVm
    {
        public MyProfile SelectedProfile { get; set; }

        [DependsOn(nameof(SelectedProfile), nameof(IsEditModeActive))]
        public bool IsEditModeEnabled => IsEditModeActive == false && SelectedProfile != null;

        public bool IsEditModeActive { get; set; }

        [DependsOn(nameof(IsEditModeActive))] public bool IsEditModeInActive => !IsEditModeActive;
        
        public TestParametersType SelectedTestParametersType { get; set; } = TestParametersType.Gate;
        public Dictionary<string, int> MmeCodes { get; set; }

        public string SelectedMmeCode { get; set; }

        public string SearchingName { get; set; }

        public bool ReadOnlyMode { get; set; }

        [DependsOn(nameof(ReadOnlyMode), nameof(SelectedProfile))]
        public bool ShowHeightMeasure => ReadOnlyMode && SelectedProfile != null;

        public bool IsSingleMmeCode { get; set; }


        [DependsOn(nameof(NextAction), nameof(SelectedProfile))]

        public bool ButtonNextIsVisible => NextAction != null;
        public Action NextAction { get; set; }
        public RelayCommand ButtonNextRelayCommand => new RelayCommand((o) => NextAction?.Invoke(), (o) => SelectedProfile != null);
        
        public ObservableCollection<MyProfile> Profiles { get; set; }
        public ObservableCollection<MyProfile> LoadedProfiles { get; set; }

        public ProfileDeepData ProfileDeepDataCopy { get; set; }
        public string SelectedProfileNameCopy { get; set; }

        [DependsOn(nameof(SelectedProfile))] public bool IsClampingCommutationActive => SelectedProfile != null;
    }
}