using MahApps.Metro.Controls.Dialogs;
using SCME.InterfaceImplementations.Common;
using SCME.ProfileBuilder.ViewModels;
using SCME.Types.BaseTestParams;
using SCME.Types.DatabaseServer;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace SCME.ProfileBuilder.PagesTech
{
    /// <summary>
    /// Логика взаимодействия для ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage : Page
    {
        public ProfilesPageVM VM { get; set; } = new ProfilesPageVM();
        private readonly SQLLoadProfilesServiceTest LoadProfilesService;

        private HashSet<Guid> _DeepLoadedProfiles = new HashSet<Guid>();

        public ProfilesPage(SQLLoadProfilesServiceTest loadProfilesService)
        {
            InitializeComponent();
            LoadProfilesService = loadProfilesService;       
        }


        private void ProfilesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM.SelectedProfile = e.NewValue as MyProfile;
            if (VM.SelectedProfile == null)
                return;

            var selectedKey = VM.SelectedProfile.Key;
            var findKey = _DeepLoadedProfiles.FirstOrDefault(m=> m == selectedKey);

            if(findKey == Guid.Empty)
            {    
                _DeepLoadedProfiles.Add(selectedKey);

                var superficiallyProfile = VM.SelectedProfile;
                VM.SelectedProfile.ProfileDeepData = LoadProfilesService.LoadProfileDeepData(VM.SelectedProfile);
                if (VM.SelectedProfile.IsTop)
                    VM.SelectedProfile.Childrens = new ObservableCollection<MyProfile>(LoadProfilesService.GetProfileChildsSuperficially(VM.SelectedProfile));
            }
        }

        private async void SelectMMECode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VM.MMECodes.Count == 0)
            {
                await Cache.Main.ShowMessageAsync("Error", Properties.Resources.MMECodesEmpty, MessageDialogStyle.Affirmative);
                return;
            }
            VM.Profiles = new ObservableCollection<MyProfile>(LoadProfilesService.GetProfilesSuperficially(VM.SelectedMMECode));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            VM.MMECodes = LoadProfilesService.GetMMECodes();
           
        }

        private void SearchProfiles_Click(object sender, RoutedEventArgs e)
        {

        }


        private void BeginEditProfile_Click(object sender, RoutedEventArgs e)
        {
            VM.IsEditModeEnabled = false;
        }

        private void EndEditProfile_Click(object sender, RoutedEventArgs e)
        {
            VM.IsEditModeEnabled = true;
        }

        private void CancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            VM.IsEditModeEnabled = true;
            VM.SelectedProfile.ProfileDeepData = LoadProfilesService.LoadProfileDeepData(VM.SelectedProfile);
        }

        private void AddTestParametrUserControl_AddTestParametersClick(object sender, RoutedEventArgs e)
        {

        }

        private void AddTestParametersEvent_Click(TestParametersType t)
        {
            var testParametersAndNormatives = VM.SelectedProfile.ProfileDeepData.TestParametersAndNormatives;
            var maxOrder = testParametersAndNormatives.Count > 0 ? testParametersAndNormatives.Max(m => m.Order) : 0;

            var newTestParameter = BaseTestParametersAndNormatives.CreateParametersByType(t);
            newTestParameter.Order = maxOrder + 1;

            VM.SelectedProfile.ProfileDeepData.TestParametersAndNormatives.Add(newTestParameter);
        }

     
    }
}
