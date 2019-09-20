using SCME.InterfaceImplementations.Common;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro;


namespace SCME.WpfControlLibrary.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage : Page
    {
        public ProfilesPageVM Vm { get; set; } = new ProfilesPageVM();
        private readonly ILoadProfilesServiceTest _loadProfilesService;
        private readonly ISaveProfileServiceTest _saveProfileServiceTest;

        private HashSet<Guid> _DeepLoadedProfiles = new HashSet<Guid>();
        private bool _isIgnoreTreeViewSelectionChanged = false;
        

        public ProfilesPage(ILoadProfilesServiceTest loadProfilesService, ISaveProfileServiceTest saveProfileServiceTest, string MMECode = null)
        {
            
            InitializeComponent();
            _loadProfilesService = loadProfilesService;
            _saveProfileServiceTest = saveProfileServiceTest;
            if (MMECode != null)
                Vm.SelectedMMECode = MMECode;
            else
            {
                Vm.MmeCodes = _loadProfilesService.GetMMECodes();
                if (string.IsNullOrEmpty(Vm.SelectedMMECode) && Vm.MmeCodes.Count != 0)
                    Vm.SelectedMMECode = Vm.MmeCodes.First().Key;
            }
        }

        private void LoadTopProfiles()
        {
            Vm.Profiles = new ObservableCollection<MyProfile>(_loadProfilesService.GetProfilesSuperficially(Vm.SelectedMMECode));
            foreach (var i in Vm.Profiles)
                i.IsTop = true;
        }

        
        private void ProfilesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_isIgnoreTreeViewSelectionChanged)
                return;

            Vm.SelectedProfile = e.NewValue as MyProfile;
            if (Vm.SelectedProfile == null)
                return;


            var selectedKey = Vm.SelectedProfile.Key;
            var findKey = _DeepLoadedProfiles.FirstOrDefault(m => m == selectedKey);

            if (findKey == Guid.Empty)
            {
                _DeepLoadedProfiles.Add(selectedKey);

                var superficiallyProfile = Vm.SelectedProfile;
                Vm.SelectedProfile.ProfileDeepData = _loadProfilesService.LoadProfileDeepData(Vm.SelectedProfile);
                if (Vm.SelectedProfile.IsTop)
                {
                    Vm.SelectedProfile.Children = new ObservableCollection<MyProfile>(_loadProfilesService.GetProfileChildsSuperficially(Vm.SelectedProfile));
                    foreach (var i in Vm.SelectedProfile.Children)
                        i.IsTop = false;
                }
            }

            Vm.ProfileDeepData = Vm.SelectedProfile.ProfileDeepData;
        }

        private void SearchProfiles_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(Vm.SelectedProfile) is TreeViewItem treeViewItem)
                treeViewItem.IsSelected = false;
            Vm.Profiles = new ObservableCollection<MyProfile>(Vm.HideProfilesForSearch.Where(m => m.Name.Contains(Vm.SearchingName)));
        }

        private void BeginEditProfile_Click(object sender, RoutedEventArgs e)
        {
            Vm.SelectedProfile = (MyProfile) ((Button) sender).DataContext;
            ((TreeViewItem) ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(Vm.SelectedProfile)).IsSelected = true;
            Vm.ProfileDeepData = Vm.SelectedProfile.ProfileDeepData.Copy();
            Vm.IsEditModeEnabled = false;
        }

        private void EndEditProfile_Click(object sender, RoutedEventArgs e)
        {
            Vm.IsEditModeEnabled = true;

            var oldProfile = Vm.SelectedProfile;
            var newProfile = oldProfile.GenerateNextVersion(Vm.ProfileDeepData);

            newProfile.Id =_saveProfileServiceTest.InsertUpdateProfile(oldProfile, newProfile, Vm.SelectedMMECode);

            oldProfile.IsTop = false;
            oldProfile.Children = null;

            _isIgnoreTreeViewSelectionChanged = true;

            Vm.Profiles.Insert(Vm.Profiles.IndexOf(oldProfile), newProfile);
            Vm.Profiles.Remove(oldProfile);

            ((TreeViewItem) ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(newProfile)).IsSelected = true;

            Vm.SelectedProfile = newProfile;
            _isIgnoreTreeViewSelectionChanged = false;

            _DeepLoadedProfiles.Add(newProfile.Key);
        }

        private void CancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            Vm.IsEditModeEnabled = true;
            Vm.ProfileDeepData = Vm.SelectedProfile.ProfileDeepData;
        }

        private void AddTestParametersEvent_Click(TestParametersType t)
        {
            var testParametersAndNormatives = Vm.ProfileDeepData.TestParametersAndNormatives;
            var maxOrder = testParametersAndNormatives.Count > 0 ? testParametersAndNormatives.Max(m => m.Order) : 0;

            var newTestParameter = BaseTestParametersAndNormatives.CreateParametersByType(t);
            newTestParameter.Order = maxOrder + 1;
            testParametersAndNormatives.Add(newTestParameter);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _DeepLoadedProfiles.Clear();
            Vm.SearchingName = string.Empty;
            LoadTopProfiles();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(Vm.MmeCodes.Count == 0)
            {
                MessageBox.Show(Properties.Resources.Error, Properties.Resources.MissingMMECodes);
                return;
            }
            LoadTopProfiles();
        }
    }
}
