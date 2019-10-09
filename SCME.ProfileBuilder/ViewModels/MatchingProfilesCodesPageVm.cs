using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using PropertyChanged;
using SCME.Types;
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
        public RelayCommand DeleteAllMmeCodesToProfile => new RelayCommand(o =>
        {
            if (new DialogWindow(WpfControlLibrary.Properties.Resources.Confirmation, WpfControlLibrary.Properties.Resources.ConfirmationOfDeletion, true ).ShowDialogWithResult() == false)
                return;
            foreach (var i in MmeCodesByProfile)
                i.IsCheckedNewValue = false;
            SaveMmeCodesToProfile.Execute(null);
        }, o => IsEditModeEnabled && MmeCodesByProfile.Count(m=> m.IsCheckedNewValue) > 0);
        
        
        public RelayCommand SaveMmeCodesToProfile => new RelayCommand(o =>
            {
                IsEditModeActive = false;
                var changedMmeCodesToProfile = MmeCodesByProfile.Where(m => m.IsCheckedNewValue != m.IsCheckedOldValue && m.Name != WpfControlLibrary.Properties.Resources.Inactive).ToList().Copy();
                var deletedMmeCodesToProfile = changedMmeCodesToProfile.Where(m => m.IsCheckedNewValue == false).ToList();
                
                foreach (var i in deletedMmeCodesToProfile)
                    _dbService.RemoveMmeCodeToProfile(SelectedProfile.Id, i.Name);

                foreach (var i in changedMmeCodesToProfile.Where(m => m.IsCheckedNewValue))
                    _dbService.InsertMmeCodeToProfile(SelectedProfile.Id, i.Name);
                
                //Если нет ни одного привязанного КИП
                if(MmeCodesByProfile.FirstOrDefault(m=> m.IsCheckedNewValue) == null)
                    //Добавляем профиль в кэш неактивных
                    _dbService.InsertMmeCodeToProfile(SelectedProfile.Id, string.Empty);
                

                //Если среди удалённых привязок КИП есть текущий
                if (deletedMmeCodesToProfile.SingleOrDefault(m => m.Name == SelectedMmeCode) != null)
                    //Удаляем его из списка
                    _profiles.Remove(SelectedProfile);

                //Если выбраны неактивные профили
                else if (SelectedMmeCode == WpfControlLibrary.Properties.Resources.Inactive) 
                    //Если отмечен хоть один КИП
                    if(MmeCodesByProfile.FirstOrDefault(m=> m.IsCheckedNewValue) != null)
                    {
                        //Сначала удалим профиль из кеша так как удаление сбивает SelectedItem 
                        _dbService.RemoveMmeCodeToProfile(SelectedProfile.Id, string.Empty);
                        //То удалим профль из колекции
                        _profiles.Remove(SelectedProfile);
                    }


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
                SelectedProfile = null;
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
                SetMmeCodesByProfile();
            }
        }

        public ObservableCollection<MmeCodeCheckboxDataTemplateVm> MmeCodesByProfile { get; set; }

        private void SetMmeCodesByProfile()
        {
            if (SelectedProfile == null)
                MmeCodesByProfile = null;
            else
            {
                var mmeCodesByProfile = _dbService.GetMmeCodesByProfile(SelectedProfile);
                MmeCodesByProfile = new ObservableCollection<MmeCodeCheckboxDataTemplateVm>(MmeCodes.Where(m => m != WpfControlLibrary.Properties.Resources.Inactive).Select(m => new MmeCodeCheckboxDataTemplateVm()
                {
                    Name = m,
                    IsCheckedOldValue = mmeCodesByProfile.Contains(m),
                    IsCheckedNewValue = mmeCodesByProfile.Contains(m)
                }));
            }
        }
    }
}