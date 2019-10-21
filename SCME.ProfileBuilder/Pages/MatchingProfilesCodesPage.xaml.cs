using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SCME.ProfileBuilder.ViewModels;
using SCME.Types.Database;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.Pages;

namespace SCME.ProfileBuilder.Pages
{
    public partial class MatchingProfilesCodesPage
    {
        private readonly IDbService _dbService;
        public MatchingProfilesCodesPageVm Vm { get; set; }

        public MatchingProfilesCodesPage(IDbService dbService)
        {
            Vm = new MatchingProfilesCodesPageVm(dbService);
            InitializeComponent();
            _dbService = dbService;
        }

        #region ProfilesActiveInactive

        // ReSharper disable once UnusedMember.Local
        private void OnDispatcherTimerFindActiveProfileOnTick(object sender, EventArgs e)
        {
            Vm.ActiveProfiles.View.Refresh();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDispatcherTimerFindInactiveProfileOnTick(object sender, EventArgs e)
        {
            Vm.InactiveProfiles.View.Refresh();
        }

        private void ListViewActiveProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.SelectedActiveProfiles.AddRange(e.AddedItems.Cast<MyProfile>());
            Vm.SelectedActiveProfiles.RemoveAll(m => e.RemovedItems.Cast<MyProfile>().Contains(m));
        }

        private void ListViewInactiveProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.SelectedInactiveProfiles.AddRange(e.AddedItems.Cast<MyProfile>());
            Vm.SelectedInactiveProfiles.RemoveAll(m => e.RemovedItems.Cast<MyProfile>().Contains(m));
        }
        
        private void ActiveProfiles_Drop(object sender, DragEventArgs e)
        {
            Vm.MoveToActive.Execute(null);
        }
        
        private void InactiveProfiles_Drop(object sender, DragEventArgs e)
        {
            Vm.MoveToInactive.Execute(null);
        }
        
        #endregion

        #region ProfilesForAndFormMmeCode

        private void ListViewProfilesFromMmeCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.SelectedProfilesFromMmeCode.AddRange(e.AddedItems.Cast<MyProfile>());
            Vm.SelectedProfilesFromMmeCode.RemoveAll(m => e.RemovedItems.Cast<MyProfile>().Contains(m));
        }
        
        private void ListViewProfilesForMmeCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.SelectedProfilesForMmeCode.AddRange(e.AddedItems.Cast<MyProfile>());
            Vm.SelectedProfilesForMmeCode.RemoveAll(m => e.RemovedItems.Cast<MyProfile>().Contains(m));
        }

        private void ProfilesFromMmeCode_Drop(object sender, DragEventArgs e)
        {
            Vm.AddProfileToMmeCode.Execute(null);
        }
        
        private void ProfilesForMmeCode_Drop(object sender, DragEventArgs e)
        {
            Vm.RemoveProfileFromMmeCode.Execute(null);
        }
        
        private void OnDispatcherTimerFindProfileFromMmeCodeOnTick(object sender, EventArgs e)
        {
            Vm.ProfilesFromMmeCode.View.Refresh();
        }
        
        private void OnDispatcherTimerFindProfileForMmeCodeOnTick(object sender, EventArgs e)
        {
            Vm.ProfilesForMmeCode.View.Refresh();
        }
        
        #endregion
        
        private void NavigateToConnectPage_OnClick(object sender, RoutedEventArgs e)
        {
            if(Cache.ProfilesPage == null)
                Cache.ProfilesPage = new ProfilesPage(_dbService, Properties.Settings.Default.LastSelectedMMECode);
            NavigationService?.Navigate(Cache.ProfilesPage);
        }
        
        private void NavigateBack_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
        
     
        
    }
}