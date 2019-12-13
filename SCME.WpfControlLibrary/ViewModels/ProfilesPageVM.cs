using PropertyChanged;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using SCME.Types;
using SCME.Types.Commutation;
using SCME.Types.Database;
using SCME.WpfControlLibrary.Commands;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProfilesPageProfileVm : EditProfileVm
    {
        private readonly IDbService _dbService;

        public ProfilesPageProfileVm(IDbService dbService)
        {
            _dbService = dbService;
            ProfilesSource = new CollectionViewSource() {SortDescriptions = {new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending)}};
            ProfilesSource.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                args.Accepted = profile.Name.ToUpper().Contains(SearchingName.ToUpper());
            };
        }

        private MyProfile _selectedProfile;
        public MyProfile SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                _selectedProfile = value;
                if (_selectedProfile != null)
                {
                    SelectedProfileNameCopy = _selectedProfile.Name;
                    SelectedProfile.DeepData = _dbService.LoadProfileDeepData(_selectedProfile);
                    ProfileDeepDataCopy = _selectedProfile.DeepData.Copy();
                }
                else
                    ProfileDeepDataCopy = null;
            }
        }

        [DependsOn(nameof(SelectedProfile), nameof(IsEditModeActive))]
        public bool IsEditModeEnabled => IsEditModeActive == false && SelectedProfile != null;

        public bool IsEditModeActive { get; set; }

        [DependsOn(nameof(IsEditModeActive))] public bool IsEditModeInActive => !IsEditModeActive;
        
        public TestParametersType SelectedTestParametersType { get; set; } = TestParametersType.Gate;
        public Dictionary<string, int> MmeCodes { get; set; }

        public string SelectedMmeCode { get; set; }

        public string SearchingName { get; set; }

        public bool ReadOnlyMode { get; set; }
        public bool SpecialMeasure { get; set; }

        [DependsOn(nameof(ReadOnlyMode), nameof(SelectedProfile))]
        public bool ShowHeightMeasure => ReadOnlyMode && SelectedProfile != null;

        public bool IsSingleMmeCode { get; set; }


        [DependsOn(nameof(NextAction), nameof(SelectedProfile))]

        public bool ButtonNextIsVisible => NextAction != null;
        public Action NextAction { get; set; }
        public RelayCommand ButtonNextRelayCommand => new RelayCommand((o) => NextAction?.Invoke(), (o) => SelectedProfile != null);

        public CollectionViewSource ProfilesSource { get; set; }
        public ObservableCollection<MyProfile> Profiles { get; set; }

        public ProfileDeepData ProfileDeepDataCopy { get; set; }
        public string SelectedProfileNameCopy { get; set; }

        [DependsOn(nameof(ProfileDeepDataCopy))] public bool IsClampingCommutationActive => ProfileDeepDataCopy != null;
    }
}