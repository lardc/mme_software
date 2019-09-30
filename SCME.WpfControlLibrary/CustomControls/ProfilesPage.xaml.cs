﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.Database;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.ViewModels;

namespace SCME.WpfControlLibrary.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage
    {
        public ProfilesPageVm Vm { get; set; } = new ProfilesPageVm();
        private readonly IDbService _dbService;

        private bool _isIgnoreTreeViewSelectionChanged;
        private readonly DispatcherTimer _dispatcherTimerFindProfile = new DispatcherTimer();

        public ProfilesPage(IDbService dbService, string mmeCode = null)
        {
            InitializeComponent();
            _dbService = dbService;
            if (mmeCode != null)
                Vm.SelectedMmeCode = mmeCode;
            else
            {
                Vm.MmeCodes = _dbService.GetMmeCodes();
                if (string.IsNullOrEmpty(Vm.SelectedMmeCode) && Vm.MmeCodes.Count != 0)
                    Vm.SelectedMmeCode = Vm.MmeCodes.First().Key;
            }

            _dispatcherTimerFindProfile.Tick += OnDispatcherTimerFindProfileOnTick;

            _dispatcherTimerFindProfile.Interval = new TimeSpan(0, 0, 1);
        }

        private void OnDispatcherTimerFindProfileOnTick(object o1, EventArgs args1)
        {
            _dispatcherTimerFindProfile.Stop();
            if (ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(Vm.SelectedProfile) is TreeViewItem treeViewItem) treeViewItem.IsSelected = false;
            Vm.Profiles = new ObservableCollection<MyProfile>(Vm.LoadedProfiles.Where(m => m.Name.ToUpper().Contains(Vm.SearchingName.ToUpper())));
        }

        private void LoadTopProfiles()
        {
            Vm.LoadedProfiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(Vm.SelectedMmeCode));
            foreach (var i in Vm.LoadedProfiles)
                i.IsTop = true;
            Vm.Profiles = new ObservableCollection<MyProfile>(Vm.LoadedProfiles);
        }

        private void ProfilesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_isIgnoreTreeViewSelectionChanged)
                return;

            Vm.SelectedProfile = e.NewValue as MyProfile;
            if (Vm.SelectedProfile == null)
                return;

            Vm.SelectedProfileNameCopy = Vm.SelectedProfile.Name;
            Vm.SelectedProfile.ProfileDeepData = _dbService.LoadProfileDeepData(Vm.SelectedProfile);
            if (Vm.SelectedProfile.IsTop)
            {
                Vm.SelectedProfile.Children = new ObservableCollection<MyProfile>(_dbService.GetProfileChildSuperficially(Vm.SelectedProfile));
                foreach (var i in Vm.SelectedProfile.Children)
                    i.IsTop = false;
            }

            Vm.ProfileDeepDataCopy = Vm.SelectedProfile.ProfileDeepData;
        }

        private void BeginEditProfile_Click(object sender, RoutedEventArgs e)
        {
            ((TreeViewItem) ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(Vm.SelectedProfile)).IsSelected = true;
            Vm.ProfileDeepDataCopy = Vm.SelectedProfile.ProfileDeepData.Copy();
            Vm.SelectedProfileNameCopy = Vm.SelectedProfile.Name.Copy();
            Vm.IsEditModeActive = true;
        }

        private void CreateNewProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Vm.SelectedProfile != null)
                ((TreeViewItem) ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(Vm.SelectedProfile)).IsSelected = false;
            Vm.SelectedProfile = null;
            Vm.ProfileDeepDataCopy = new ProfileDeepData();
            Vm.SelectedProfileNameCopy = _dbService.GetFreeProfileName();
            Vm.IsEditModeActive = true;
        }

        private bool CheckName(string oldName)
        {
            if (Vm.SelectedProfileNameCopy.Equals(oldName))
                return true;
            if (_dbService.ProfileNameExists(Vm.SelectedProfileNameCopy) == false)
                return true;
            new DialogWindow(Properties.Resources.Error, "").ShowDialog();
            return false;
        }


        private void EndEditProfile_Click(object sender, RoutedEventArgs e)
        {
            var oldName = Vm.SelectedProfile?.Name;
            if (CheckName(oldName) == false)
                return;

            Vm.IsEditModeActive = false;

            var oldProfile = Vm.SelectedProfile;
            MyProfile newProfile;
            if (oldProfile == null)
            {
                newProfile = new MyProfile(0, Vm.SelectedProfileNameCopy, Guid.NewGuid(), 0, DateTime.Now).GenerateNextVersion(Vm.ProfileDeepDataCopy, Vm.SelectedProfileNameCopy);
                newProfile.Id = _dbService.InsertUpdateProfile(oldProfile, newProfile, Vm.SelectedMmeCode);

                _isIgnoreTreeViewSelectionChanged = true;
                Vm.Profiles.Add(newProfile);
                Vm.LoadedProfiles.Add(newProfile);
            }
            else
            {
                newProfile = oldProfile.GenerateNextVersion(Vm.ProfileDeepDataCopy, Vm.SelectedProfileNameCopy);

                newProfile.Id = _dbService.InsertUpdateProfile(oldProfile, newProfile, Vm.SelectedMmeCode);

                oldProfile.IsTop = false;
                oldProfile.Children = null;

                _isIgnoreTreeViewSelectionChanged = true;

                Vm.Profiles.Insert(Vm.Profiles.IndexOf(oldProfile), newProfile);
                Vm.Profiles.Remove(oldProfile);
                Vm.LoadedProfiles.Insert(Vm.LoadedProfiles.IndexOf(oldProfile), newProfile);
                Vm.LoadedProfiles.Remove(oldProfile);
            }

            //If VirtualizingStackPanel.IsVirtualizing="True" and UpdateLayout not call then ContainerFromItem return null;
            ProfilesTreeView.UpdateLayout();
            _isIgnoreTreeViewSelectionChanged = false;

            ((TreeViewItem) ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(newProfile)).IsSelected = true;
            Vm.SelectedProfile = newProfile;
        }

        private void CancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            Vm.IsEditModeActive = false;
            if (Vm.SelectedProfile == null) 
                return;
            Vm.ProfileDeepDataCopy = Vm.SelectedProfile.ProfileDeepData;
            Vm.SelectedProfileNameCopy = Vm.SelectedProfile.Name;
        }

        private void AddTestParametersEvent_Click()
        {
            var testParametersAndNormatives = Vm.ProfileDeepDataCopy.TestParametersAndNormatives;
            var maxOrder = testParametersAndNormatives.Count > 0 ? testParametersAndNormatives.Max(m => m.Order) : 0;

            var newTestParameter = BaseTestParametersAndNormatives.CreateParametersByType(Vm.SelectedTestParametersType);
            newTestParameter.IsEnabled = true;
            newTestParameter.Order = maxOrder + 1;
            testParametersAndNormatives.Add(newTestParameter);
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.SearchingName = string.Empty;
            LoadTopProfiles();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Vm.MmeCodes.Count == 0)
            {
                MessageBox.Show(Properties.Resources.Error, Properties.Resources.MissingMMECodes);
                return;
            }

            LoadTopProfiles();
        }


        private void TextBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            _dispatcherTimerFindProfile.Start();
        }
    }
}