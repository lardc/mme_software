using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SCME.ProfileBuilder.ViewModels;
using SCME.Types.Database;

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
            Vm.SelectedMmeCode = Vm.MmeCodes.Last();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // ReSharper disable once UnusedMember.Local
        private void OnDispatcherTimerFindProfileOnTick(object sender, EventArgs e)
        {
            Vm.ProfilesSource.View.Refresh();
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
    }
}