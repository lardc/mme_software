using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using PropertyChanged;
using SCME.InterfaceImplementations.Common.DbService;
using SCME.InterfaceImplementations.NewImplement.SQLite;
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
            MmeCodes = new ObservableCollection<string>(_dbService.GetMmeCodes().Select(m => m.Key).Except(new string[] {SQLiteDbService.MME_CODE_IS_ACTIVE_NAME}));
            MmeCodes.CollectionChanged += MmeCodesOnCollectionChanged;

            ActiveProfiles = new CollectionViewSource()
            {
                Source = _activeProfiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(SQLiteDbService.MME_CODE_IS_ACTIVE_NAME)),
                SortDescriptions = {new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending)}
            };

            ActiveProfilesForMmeCodes = new CollectionViewSource()
            {
                Source = _activeProfiles,
                SortDescriptions = {new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending)}
            };

            InactiveProfiles = new CollectionViewSource()
            {
                Source = _inactiveProfiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(string.Empty).Except(_activeProfiles, (m, n) => m.Id == n.Id)),
                SortDescriptions = {new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending)}
            };

            ActiveProfiles.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                args.Accepted = string.IsNullOrEmpty(ActiveProfileFilterName) || profile.Name.ToUpper().Contains(ActiveProfileFilterName.ToUpper());
            };

            InactiveProfiles.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                args.Accepted = string.IsNullOrEmpty(InactiveProfileFilterName) || profile.Name.ToUpper().Contains(InactiveProfileFilterName.ToUpper());
            };

            ActiveProfilesForMmeCodes.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                args.Accepted = string.IsNullOrEmpty(ActiveProfilesForMmeCodesFilterName) || profile.Name.ToUpper().Contains(ActiveProfilesForMmeCodesFilterName.ToUpper());
            };
        }


        #region ProfilesActiveInactive

        private readonly ObservableCollection<MyProfile> _activeProfiles;
        private readonly ObservableCollection<MyProfile> _inactiveProfiles;

        public CollectionViewSource ActiveProfiles { get; set; }
        public CollectionViewSource InactiveProfiles { get; set; }
        public CollectionViewSource ActiveProfilesForMmeCodes { get; set; }


        public List<MyProfile> SelectedActiveProfiles { get; } = new List<MyProfile>();
        public List<MyProfile> SelectedInactiveProfiles { get; } = new List<MyProfile>();

        public string ActiveProfileFilterName { get; set; }
        public string InactiveProfileFilterName { get; set; }
        public string ActiveProfilesForMmeCodesFilterName { get; set; }


        public RelayCommand MoveToInactive => new RelayCommand(o =>
        {
            foreach (var i in SelectedActiveProfiles.ToList())
            {
                _dbService.RemoveMmeCodeToProfile(i.Id, SQLiteDbService.MME_CODE_IS_ACTIVE_NAME);
                _activeProfiles.Remove(i);
                _inactiveProfiles.Add(i);
            }
        }, o => SelectedActiveProfiles.Count > 0);

        public RelayCommand MoveToActive => new RelayCommand(o =>
        {
            foreach (var i in SelectedInactiveProfiles.ToList())
            {
                _dbService.InsertMmeCodeToProfile(i.Id, SQLiteDbService.MME_CODE_IS_ACTIVE_NAME);
                _inactiveProfiles.Remove(i);
                _activeProfiles.Add(i);
            }
        }, o => SelectedInactiveProfiles.Count > 0);

        #endregion

        #region MmeCodesToProfile

        private MyProfile _selectedActiveProfile;

        public MyProfile SelectedActiveProfile
        {
            get => _selectedActiveProfile;
            set
            {
                _selectedActiveProfile = value;
                SelectedEnabledMmeCodes.Clear();
                SelectedDisabledMmeCodes.Clear();
                if (_selectedActiveProfile == null)
                {
                    EnabledMmeCodes = null;
                    DisabledMmeCodes = null;
                }
                else
                {
                    EnabledMmeCodes = new ObservableCollection<string>(_dbService.GetMmeCodesByProfile(_selectedActiveProfile).Except(new string[] {SQLiteDbService.MME_CODE_IS_ACTIVE_NAME}));
                    DisabledMmeCodes = new ObservableCollection<string>(MmeCodes.Except(EnabledMmeCodes));
                }
            }
        }


        public ObservableCollection<string> SelectedEnabledMmeCodes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedDisabledMmeCodes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> EnabledMmeCodes { get; set; }
        public ObservableCollection<string> DisabledMmeCodes { get; set; }

        [DependsOn(nameof(SelectedActiveProfile))]
        public RelayCommand AddMmeCodeToProfile => new RelayCommand(o =>
        {
            foreach (var i in SelectedDisabledMmeCodes.ToList())
            {
                _dbService.InsertMmeCodeToProfile(SelectedActiveProfile.Id, i);
                DisabledMmeCodes.Remove(i);
                EnabledMmeCodes.Add(i);
            }
        }, o => SelectedDisabledMmeCodes?.Count > 0);

        [DependsOn(nameof(SelectedActiveProfile))]
        public RelayCommand RemoveMmeCodeToProfile => new RelayCommand(o =>
        {
            foreach (var i in SelectedEnabledMmeCodes.ToList())
            {
                _dbService.RemoveMmeCodeToProfile(SelectedActiveProfile.Id, i);
                EnabledMmeCodes.Remove(i);
                DisabledMmeCodes.Add(i);
            }
        }, o => SelectedEnabledMmeCodes?.Count > 0);

        #endregion

        #region MmeCodes

        private void MmeCodesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
//            IList<string>[] collections = new IList<string>[]
//            {
//                SelectedEnabledMmeCodes,
//                SelectedDisabledMmeCodes,
//                EnabledMmeCodes,
//                DisabledMmeCodes,
//            };
//            foreach (var collection in collections)
//                foreach (string oldItem in e.OldItems)
//                    collection?.Remove(oldItem);
        }

        public ObservableCollection<string> MmeCodes { get; set; }
        public string NewMmeCode { get; set; } = string.Empty;

        public RelayCommand AddMmeCode => new RelayCommand((o) =>
        {
            if (new DialogWindow(WpfControlLibrary.Properties.Resources.Confirmation, $"{WpfControlLibrary.Properties.Resources.AddMmeCode}: {NewMmeCode}", true).ShowDialogWithResult() == false)
                return;
            _dbService.InsertMmeCode(NewMmeCode);
            MmeCodes.Add(NewMmeCode);
            
            var tmp = SelectedActiveProfile;
            SelectedActiveProfile = null;
            SelectedActiveProfile = tmp;
        }, (o) => string.IsNullOrWhiteSpace((NewMmeCode)) == false && MmeCodes.Contains(NewMmeCode) == false);

        public string SelectedMmeCodeFromRemove { get; set; }

        public RelayCommand RemoveMmeCode => new RelayCommand((o) =>
        {
            if (new DialogWindow(WpfControlLibrary.Properties.Resources.Confirmation, $"{WpfControlLibrary.Properties.Resources.RemoveMmeCode}: {SelectedMmeCodeFromRemove}", true).ShowDialogWithResult() == false)
                return;
            _dbService.RemoveMmeCode(SelectedMmeCodeFromRemove);
            MmeCodes.Remove(SelectedMmeCodeFromRemove);

            var tmp = SelectedActiveProfile;
            SelectedActiveProfile = null;
            SelectedActiveProfile = tmp;
        }, (o) => SelectedMmeCodeFromRemove != null);

        #endregion



//
//        public RelayCommand EditMmeCodesToProfile => new RelayCommand(o => { IsEditModeActive = true; }, o => IsEditModeEnabled);
//        public RelayCommand DeleteAllMmeCodesToProfile => new RelayCommand(o =>
//        {
//            if (new DialogWindow(WpfControlLibrary.Properties.Resources.Confirmation, WpfControlLibrary.Properties.Resources.ConfirmationOfDeletion, true ).ShowDialogWithResult() == false)
//                return;
//            foreach (var i in MmeCodesByProfile)
//                i.IsCheckedNewValue = false;
//            SaveMmeCodesToProfile.Execute(null);
//        }, o => IsEditModeEnabled && MmeCodesByProfile.Count(m=> m.IsCheckedNewValue) > 0);
//        
//        
//        public RelayCommand SaveMmeCodesToProfile => new RelayCommand(o =>
//            {
//                IsEditModeActive = false;
//                var changedMmeCodesToProfile = MmeCodesByProfile.Where(m => m.IsCheckedNewValue != m.IsCheckedOldValue && m.Name != WpfControlLibrary.Properties.Resources.Inactive).ToList().Copy();
//                var deletedMmeCodesToProfile = changedMmeCodesToProfile.Where(m => m.IsCheckedNewValue == false).ToList();
//                
//                foreach (var i in deletedMmeCodesToProfile)
//                    _dbService.RemoveMmeCodeToProfile(SelectedProfile.Id, i.Name);
//
//                foreach (var i in changedMmeCodesToProfile.Where(m => m.IsCheckedNewValue))
//                    _dbService.InsertMmeCodeToProfile(SelectedProfile.Id, i.Name);
//                
//                //Если нет ни одного привязанного КИП
//                if(MmeCodesByProfile.FirstOrDefault(m=> m.IsCheckedNewValue) == null)
//                    //Добавляем профиль в кэш неактивных
//                    _dbService.InsertMmeCodeToProfile(SelectedProfile.Id, string.Empty);
//                
//
//                //Если среди удалённых привязок КИП есть текущий
//                if (deletedMmeCodesToProfile.SingleOrDefault(m => m.Name == SelectedMmeCode) != null)
//                    //Удаляем его из списка
//                    _profiles.Remove(SelectedProfile);
//
//                //Если выбраны неактивные профили
//                else if (SelectedMmeCode == WpfControlLibrary.Properties.Resources.Inactive) 
//                    //Если отмечен хоть один КИП
//                    if(MmeCodesByProfile.FirstOrDefault(m=> m.IsCheckedNewValue) != null)
//                    {
//                        //Сначала удалим профиль из кеша так как удаление сбивает SelectedItem 
//                        _dbService.RemoveMmeCodeToProfile(SelectedProfile.Id, string.Empty);
//                        //То удалим профль из колекции
//                        _profiles.Remove(SelectedProfile);
//                    }
//
//
//            }, o => IsEditModeActive 
//        );
//
//        public RelayCommand CancelMmeCodesToProfile => new RelayCommand(o =>
//            {
//                IsEditModeActive = false;
//                SetMmeCodesByProfile();
//            }, o => IsEditModeActive
//        );
//        
//        public RelayCommand<Page> BackCommand => new RelayCommand<Page>(page => page.NavigationService?.GoBack(), page => IsEditModeInActive); 
//
//        private  ObservableCollection<MyProfile> _profiles;
//        public string ProfileFilterName { get; set; } = string.Empty;
//
//
//
//        public ObservableCollection<string> ActiveAndInactiveMmeCodes { get; set; } = new ObservableCollection<string>()
//        {
//            WpfControlLibrary.Properties.Resources.IsActive,
//            WpfControlLibrary.Properties.Resources.Inactive
//        };
//        
//        
//        public ObservableCollection<string> MmeCodes { get; set; }
//        private string _selectedMmeCode;
//
//        
//        public string SelectedMmeCode
//        {
//            get => _selectedMmeCode;
//            set
//            {
//                _selectedMmeCode = value;
//                if (_selectedMmeCode == WpfControlLibrary.Properties.Resources.IsActive)
//                    ProfilesSource.Source = _profiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(SQLiteDbService.MME_CODE_IS_ACTIVE_NAME));
//                else
//                    ProfilesSource.Source = _profiles = new ObservableCollection<MyProfile>(MmeCodes.Except(new string[]{WpfControlLibrary.Properties.Resources.Inactive}).SelectMany(m=>
//                        _dbService.GetProfilesSuperficially(m)));
////                _selectedMmeCode = value;
////                SelectedProfile = null;
////                if (SelectedMmeCode == WpfControlLibrary.Properties.Resources.Inactive)
////                    ProfilesSource.Source = _profiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(string.Empty));
////                else 
////                    ProfilesSource.Source = _profiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(_selectedMmeCode));
//            }
//        }
//
//        public CollectionViewSource ProfilesSource { get; }
//
//        public new MyProfile SelectedProfile
//        {
//            get => base.SelectedProfile;
//            set
//            {
//                base.SelectedProfile = value;
//                SetMmeCodesByProfile();
//            }
//        }
//
//        public ObservableCollection<MmeCodeCheckboxDataTemplateVm> MmeCodesByProfile { get; set; }
//
//        private void SetMmeCodesByProfile()
//        {
//            if (SelectedProfile == null)
//                MmeCodesByProfile = null;
//            else
//            {
//                var mmeCodesByProfile = _dbService.GetMmeCodesByProfile(SelectedProfile);
//                MmeCodesByProfile = new ObservableCollection<MmeCodeCheckboxDataTemplateVm>(MmeCodes.Where(m => m != WpfControlLibrary.Properties.Resources.Inactive).Select(m => new MmeCodeCheckboxDataTemplateVm()
//                {
//                    Name = m,
//                    IsCheckedOldValue = mmeCodesByProfile.Contains(m),
//                    IsCheckedNewValue = mmeCodesByProfile.Contains(m)
//                }));
//            }
//        }
    }
}