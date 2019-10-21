using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
    public class MatchingProfilesCodesPageVm 
    {
        private readonly IDbService _dbService;

        public MatchingProfilesCodesPageVm(IDbService dbService)
        {
            _dbService = dbService;
            MmeCodes = new ObservableCollection<string>(_dbService.GetMmeCodes().Select(m => m.Key).Except(new string[] {SQLiteDbService.MME_CODE_IS_ACTIVE_NAME}));

            ActiveProfiles = new CollectionViewSource()
            {
                Source = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(SQLiteDbService.MME_CODE_IS_ACTIVE_NAME)),
                SortDescriptions = {new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending)}
            };
            
           InactiveProfiles = new CollectionViewSource()
            {
                Source = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(string.Empty).Except(ActiveProfilesSource)),
                SortDescriptions = {new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending)}
            };
           
           ProfilesFromMmeCode = new CollectionViewSource()
           {
               SortDescriptions = {new SortDescription(nameof(MyProfile.Name), ListSortDirection.Ascending)}
           };

           ProfilesForMmeCode = new CollectionViewSource()
           {
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
            
            ProfilesForMmeCode.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                args.Accepted = string.IsNullOrEmpty(ProfileForMmeCodeFilterName) || profile.Name.ToUpper().Contains(ProfileForMmeCodeFilterName.ToUpper());
            };
            
            ProfilesFromMmeCode.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                if (string.IsNullOrEmpty(_selectedMmeCode))
                    args.Accepted = false;
                else
                    args.Accepted = string.IsNullOrEmpty(ProfileFromMmeCodeFilterName) || profile.Name.ToUpper().Contains(ProfileFromMmeCodeFilterName.ToUpper());
                    
            };
                
            ProfilesForMmeCode.Filter += (sender, args) =>
            {
                var profile = (MyProfile) args.Item;
                args.Accepted = string.IsNullOrEmpty(ProfileForMmeCodeFilterName) || profile.Name.ToUpper().Contains(ProfileForMmeCodeFilterName.ToUpper());
                    
            };
        }

        #region ProfilesActiveInactive

        private ObservableCollection<MyProfile> ActiveProfilesSource => (ObservableCollection<MyProfile>) ActiveProfiles.Source;
        private  ObservableCollection<MyProfile> InactiveProfilesSource => (ObservableCollection<MyProfile>) InactiveProfiles.Source;

        public CollectionViewSource ActiveProfiles { get; set; }
        public CollectionViewSource InactiveProfiles { get; set; }

        public List<MyProfile> SelectedActiveProfiles { get; } = new List<MyProfile>();
        public List<MyProfile> SelectedInactiveProfiles { get; } = new List<MyProfile>();

        public string ActiveProfileFilterName { get; set; }
        public string InactiveProfileFilterName { get; set; }

        public RelayCommand MoveToInactive => new RelayCommand(o =>
        {
            foreach (var i in SelectedActiveProfiles.ToList())
            {
                foreach (var j in _dbService.GetMmeCodesByProfile(i))
                    _dbService.RemoveMmeCodeToProfile(i.Id, j);
                
                ActiveProfilesSource.Remove(i);
                InactiveProfilesSource.Add(i);

                ProfilesForMmeCodeSource.Remove(i);
                ProfilesFromMmeCodeSource.Remove(i);
            }
        }, o => SelectedActiveProfiles.Count > 0);

        public RelayCommand MoveToActive => new RelayCommand(o =>
        {
            foreach (var i in SelectedInactiveProfiles.ToList())
            {
                _dbService.InsertMmeCodeToProfile(i.Id, SQLiteDbService.MME_CODE_IS_ACTIVE_NAME);
                
                InactiveProfilesSource.Remove(i);
                ActiveProfilesSource.Add(i);
                
                if(ProfilesFromMmeCodeSource.Contains(i) == false)
                    ProfilesForMmeCodeSource.Add(i);
            }
        }, o => SelectedInactiveProfiles.Count > 0);

        #endregion

        #region MatchingProfiles

        private string _selectedMmeCode;

        public string SelectedMmeCode
        {
            get => _selectedMmeCode;
            set
            {
                _selectedMmeCode = value;

                ProfilesFromMmeCodeSource = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(_selectedMmeCode));
                ProfilesForMmeCodeSource = new ObservableCollection<MyProfile>(ActiveProfilesSource.Except(ProfilesFromMmeCodeSource));
            }
        }

        public string ProfileFromMmeCodeFilterName { get; set; }
        public string ProfileForMmeCodeFilterName { get; set; }

        private ObservableCollection<MyProfile> ProfilesFromMmeCodeSource
        {
            get => (ObservableCollection<MyProfile>) ProfilesFromMmeCode.Source;
            set => ProfilesFromMmeCode.Source = value;
        }

        private ObservableCollection<MyProfile> ProfilesForMmeCodeSource
        {
            get => (ObservableCollection<MyProfile>) ProfilesForMmeCode.Source;
            set => ProfilesForMmeCode.Source = value;
        }

        public CollectionViewSource ProfilesFromMmeCode { get; set; }
        public CollectionViewSource ProfilesForMmeCode { get; set; }

        public List<MyProfile> SelectedProfilesFromMmeCode { get; } = new List<MyProfile>();
        public List<MyProfile> SelectedProfilesForMmeCode { get; } = new List<MyProfile>();
        
        public RelayCommand AddProfileToMmeCode => new RelayCommand(o =>
        {
            foreach (var i in SelectedProfilesForMmeCode.ToList())
            {
                _dbService.InsertMmeCodeToProfile(i.Id, SelectedMmeCode);
                
                ProfilesForMmeCodeSource.Remove(i);
                ProfilesFromMmeCodeSource.Add(i);
            }
        }, o => SelectedProfilesForMmeCode.Count > 0);
        
        public RelayCommand RemoveProfileFromMmeCode => new RelayCommand(o =>
        {
            foreach (var i in SelectedProfilesFromMmeCode.ToList())
            {
                _dbService.RemoveMmeCodeToProfile(i.Id, SelectedMmeCode);
                ProfilesFromMmeCodeSource.Remove(i);
                ProfilesForMmeCodeSource.Add(i);
            }
        }, o => SelectedProfilesFromMmeCode.Count > 0);
        
        
        #endregion

        #region MmeCodes

        public ObservableCollection<string> MmeCodes { get; set; }

        public string NewMmeCode { get; set; } = string.Empty;

        public RelayCommand AddMmeCode => new RelayCommand((o) =>
        {
            if (new DialogWindow(WpfControlLibrary.Properties.Resources.Confirmation, $"{WpfControlLibrary.Properties.Resources.AddMmeCode}: {NewMmeCode}", true).ShowDialogWithResult() == false)
                return;

            _dbService.InsertMmeCode(NewMmeCode);
            MmeCodes.Add(NewMmeCode);
        }, (o) => string.IsNullOrWhiteSpace((NewMmeCode)) == false && MmeCodes.Contains(NewMmeCode) == false);

        public string SelectedMmeCodeFromRemove { get; set; }

        public RelayCommand RemoveMmeCode => new RelayCommand((o) =>
        {
            if (new DialogWindow(WpfControlLibrary.Properties.Resources.Confirmation, $"{WpfControlLibrary.Properties.Resources.RemoveMmeCode}: {SelectedMmeCodeFromRemove}", true).ShowDialogWithResult() == false)
                return;
            _dbService.RemoveMmeCode(SelectedMmeCodeFromRemove);
            MmeCodes.Remove(SelectedMmeCodeFromRemove);

        }, (o) => SelectedMmeCodeFromRemove != null);

        #endregion
    }
}