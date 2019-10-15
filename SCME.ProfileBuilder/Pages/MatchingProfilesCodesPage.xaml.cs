using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SCME.ProfileBuilder.ViewModels;
using SCME.Types.Database;
using SCME.Types.Profiles;

namespace SCME.ProfileBuilder.Pages
{
    public partial class MatchingProfilesCodesPage : Page
    {
        private readonly IDbService _dbService;
        public MatchingProfilesCodesPageVm Vm { get; set; }

        public MatchingProfilesCodesPage(IDbService dbService)
        {
            Vm = new MatchingProfilesCodesPageVm(dbService);
            InitializeComponent();
            _dbService = dbService;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

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

        // ReSharper disable once UnusedMember.Local
        private void OnDispatcherTimerFindActiveProfileForMmeCodesOnTick(object sender, EventArgs e)
        {
            Vm.ActiveProfilesForMmeCodes.View.Refresh();
        }
        
        private void BeginEditProfile_Click(object sender, RoutedEventArgs e)
        {
            Vm.IsEditModeActive = true;
        }
        
        private void CancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            Vm.IsEditModeActive = false;
        }
        
        private void EndEditProfile_Click(object sender, RoutedEventArgs e)
        {
            Vm.IsEditModeActive = false;
        }

        private void ListViewActiveProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.SelectedActiveProfiles.AddRange(e.AddedItems.Cast<MyProfile>());
            Vm.SelectedActiveProfiles.RemoveAll(m=> e.RemovedItems.Cast<MyProfile>().Contains(m));
        }
        
        private void ListViewInactiveProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vm.SelectedInactiveProfiles.AddRange(e.AddedItems.Cast<MyProfile>());
            Vm.SelectedInactiveProfiles.RemoveAll(m=> e.RemovedItems.Cast<MyProfile>().Contains(m));
        }

        private void DisabledMmeCode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var i in e.AddedItems.Cast<string>())
                Vm.SelectedDisabledMmeCodes.Add(i);
            foreach (var i in e.RemovedItems.Cast<string>())
                Vm.SelectedDisabledMmeCodes.Remove(i);
        }
        
        private void EnabledMmeCode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var i in e.AddedItems.Cast<string>())
                Vm.SelectedEnabledMmeCodes.Add(i);
            foreach (var i in e.RemovedItems.Cast<string>())
                Vm.SelectedEnabledMmeCodes.Remove(i);
        }
    }
}