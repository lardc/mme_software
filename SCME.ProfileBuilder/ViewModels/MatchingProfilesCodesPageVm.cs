using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using PropertyChanged;
using SCME.Types.Database;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.Commands;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.ViewModels;

namespace SCME.ProfileBuilder.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MatchingProfilesCodesPageVm : EditProfileVm
    {
        private readonly IDbService _dbService;

        public MatchingProfilesCodesPageVm(IDbService dbService)
        {
            _dbService = dbService;
            MmeCodes = new ObservableCollection<string>(_dbService.GetMmeCodes().Select(m => m.Key)) {WpfControlLibrary.Properties.Resources.Inactive};
            ProfilesSource = new CollectionViewSource();
            ProfilesSource.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                args.Accepted = string.IsNullOrEmpty(ProfileFilterName) || profile.Name.ToUpper().Contains(ProfileFilterName.ToUpper());
            };
            ProfilesSource.SortDescriptions.Add(new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending));
        }

        public RelayCommand<string> AddMmeCode => new RelayCommand<string>((s) =>
        {
            if (new DialogWindow(WpfControlLibrary.Properties.Resources.Confirmation, $"{WpfControlLibrary.Properties.Resources.AddMmeCode}: {s}", true ).ShowDialogWithResult() == false)
                return;
            _dbService.InsertMmeCode(s);
            MmeCodes.Add(s);
            if(MmeCodesByProfile == null)
                return;
            ;
            MmeCodesByProfile.Add(new MmeCodeCheckboxDataTemplateVm()
            {
                Name = s,
                IsCheckedNewValue = false,
                IsCheckedOldValue = false
            });
        }, (s) => string.IsNullOrWhiteSpace((s)) == false && MmeCodes.Contains(s) == false);

        public RelayCommand EditMmeCodesToProfile => new RelayCommand(o => { IsEditModeActive = true; }, o => IsEditModeEnabled);

        public RelayCommand SaveMmeCodesToProfile => new RelayCommand(o =>
            {
                IsEditModeActive = false;
                var changedMmeCodesToProfile = MmeCodesByProfile.Where(m => m.IsCheckedNewValue != m.IsCheckedOldValue).ToList();
                var deleteMmeCodesToProfile = changedMmeCodesToProfile.Where(m => m.IsCheckedNewValue == false).ToList();
                foreach (var i in deleteMmeCodesToProfile)
                    _dbService.RemoveMmeCodeToProfile(SelectedProfile.Id, i.Name);

                foreach (var i in changedMmeCodesToProfile.Where(m => m.IsCheckedNewValue))
                    _dbService.InsertMmeCodeToProfile(SelectedProfile.Id, i.Name);

                if (deleteMmeCodesToProfile.SingleOrDefault(m => m.Name == SelectedMmeCode) != null)
                    _profiles.Remove(SelectedProfile);


            }, o => IsEditModeActive 
        );

        public RelayCommand CancelMmeCodesToProfile => new RelayCommand(o =>
            {
                IsEditModeActive = false;
                SetMmeCodesByProfile();
            }, o => IsEditModeActive
        );
        
        public RelayCommand<Page> BackCommand => new RelayCommand<Page>(page => page.NavigationService?.GoBack(), page => IsEditModeInActive); 

        private  ObservableCollection<MyProfile> _profiles;
        public string ProfileFilterName { get; set; } = string.Empty;

        public string NewMmeCode { get; set; } = string.Empty;
        public ObservableCollection<string> MmeCodes { get; set; }
        private string _selectedMmeCode;

        public string SelectedMmeCode
        {
            get => _selectedMmeCode;
            set
            {
                _selectedMmeCode = value;
                MmeCodesByProfile = null;
                if (SelectedMmeCode == WpfControlLibrary.Properties.Resources.Inactive)
                    ProfilesSource.Source = _profiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(string.Empty));
                else 
                    ProfilesSource.Source = _profiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(_selectedMmeCode));
            }
        }

        public CollectionViewSource ProfilesSource { get; }

        public new MyProfile SelectedProfile
        {
            get => base.SelectedProfile;
            set
            {
                base.SelectedProfile = value;
                if(value == null)
                    return;;
                SetMmeCodesByProfile();
            }
        }

        public ObservableCollection<MmeCodeCheckboxDataTemplateVm> MmeCodesByProfile { get; set; }

        private void SetMmeCodesByProfile()
        {
            var mmeCodesByProfile = _dbService.GetMmeCodesByProfile(SelectedProfile);
            MmeCodesByProfile = new ObservableCollection<MmeCodeCheckboxDataTemplateVm>(MmeCodes.Select(m => new MmeCodeCheckboxDataTemplateVm()
            {
                Name = m,
                IsCheckedOldValue = mmeCodesByProfile.Contains(m),
                IsCheckedNewValue = mmeCodesByProfile.Contains(m)
            }));
        }
    }
}