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


namespace SCME.WpfControlLibrary.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage : Page
    {
        public ProfilesPageVM VM { get; set; } = new ProfilesPageVM();
        private readonly ILoadProfilesServiceTest _LoadProfilesService;
        private readonly ISaveProfileServiceTest _SaveProfileServiceTest;

        private HashSet<Guid> _DeepLoadedProfiles = new HashSet<Guid>();
        private bool IsIgnoreTreeViewSelectionChanged = false;
        

        public ProfilesPage(ILoadProfilesServiceTest loadProfilesService, ISaveProfileServiceTest saveProfileServiceTest, string MMECode = null)
        {
            InitializeComponent();
            _LoadProfilesService = loadProfilesService;
            _SaveProfileServiceTest = saveProfileServiceTest;
            if (MMECode != null)
                VM.SelectedMMECode = MMECode;
            else
            {
                VM.MMECodes = _LoadProfilesService.GetMMECodes();
                if (string.IsNullOrEmpty(VM.SelectedMMECode) && VM.MMECodes.Count != 0)
                    VM.SelectedMMECode = VM.MMECodes.First().Key;
            }
        }

        private void LoadTopProfiles()
        {
            VM.Profiles = new ObservableCollection<MyProfile>(_LoadProfilesService.GetProfilesSuperficially(VM.SelectedMMECode));
            foreach (var i in VM.Profiles)
                i.IsTop = true;
        }

        
        private void ProfilesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsIgnoreTreeViewSelectionChanged)
                return;

            VM.SelectedProfile = e.NewValue as MyProfile;
            if (VM.SelectedProfile == null)
                return;


            var selectedKey = VM.SelectedProfile.Key;
            var findKey = _DeepLoadedProfiles.FirstOrDefault(m => m == selectedKey);

            if (findKey == Guid.Empty)
            {
                _DeepLoadedProfiles.Add(selectedKey);

                var superficiallyProfile = VM.SelectedProfile;
                VM.SelectedProfile.ProfileDeepData = _LoadProfilesService.LoadProfileDeepData(VM.SelectedProfile);
                if (VM.SelectedProfile.IsTop)
                {
                    VM.SelectedProfile.Childrens = new ObservableCollection<MyProfile>(_LoadProfilesService.GetProfileChildsSuperficially(VM.SelectedProfile));
                    foreach (var i in VM.SelectedProfile.Childrens)
                        i.IsTop = false;
                }
            }

            VM.ProfileDeepData = VM.SelectedProfile.ProfileDeepData;
        }

        private void SearchProfiles_Click(object sender, RoutedEventArgs e)
        {
            var treeViewItem = (ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(VM.SelectedProfile) as TreeViewItem);
            if (treeViewItem != null)
                treeViewItem.IsSelected = false;
            VM.Profiles = new ObservableCollection<MyProfile>(VM.HideProfilesForSearch.Where(m => m.Name.Contains(VM.SearchingName)));
        }

        private void BeginEditProfile_Click(object sender, RoutedEventArgs e)
        {
            VM.SelectedProfile = (sender as Button).DataContext as MyProfile;
            (ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(VM.SelectedProfile) as TreeViewItem).IsSelected = true;
            VM.ProfileDeepData = VM.SelectedProfile.ProfileDeepData.Copy();
            VM.IsEditModeEnabled = false;
        }

        private void EndEditProfile_Click(object sender, RoutedEventArgs e)
        {
            VM.IsEditModeEnabled = true;

            var oldProfile = VM.SelectedProfile;
            var newProfile = oldProfile.GenerateNextVersion(VM.ProfileDeepData);

            newProfile.Id =_SaveProfileServiceTest.InsertUpdateProfile(oldProfile, newProfile, VM.SelectedMMECode);

            oldProfile.IsTop = false;
            oldProfile.Childrens = null;

            IsIgnoreTreeViewSelectionChanged = true;

            VM.Profiles.Insert(VM.Profiles.IndexOf(oldProfile), newProfile);
            VM.Profiles.Remove(oldProfile);

            (ProfilesTreeView.ItemContainerGenerator.ContainerFromItem(newProfile) as TreeViewItem).IsSelected = true;

            VM.SelectedProfile = newProfile;
            IsIgnoreTreeViewSelectionChanged = false;

            _DeepLoadedProfiles.Add(newProfile.Key);
        }

        private void CancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            VM.IsEditModeEnabled = true;
            VM.ProfileDeepData = VM.SelectedProfile.ProfileDeepData;
        }

        private void AddTestParametersEvent_Click(TestParametersType t)
        {
            var testParametersAndNormatives = VM.SelectedProfile.ProfileDeepData.TestParametersAndNormatives;
            var maxOrder = testParametersAndNormatives.Count > 0 ? testParametersAndNormatives.Max(m => m.Order) : 0;

            var newTestParameter = BaseTestParametersAndNormatives.CreateParametersByType(t);
            newTestParameter.Order = maxOrder + 1;

            VM.SelectedProfile.ProfileDeepData.TestParametersAndNormatives.Add(newTestParameter);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _DeepLoadedProfiles.Clear();
            VM.SearchingName = string.Empty;
            LoadTopProfiles();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(VM.MMECodes.Count == 0)
            {
                MessageBox.Show(Properties.Resources.Error, Properties.Resources.MissingMMECodes);
                return;
            }
            LoadTopProfiles();
        }
    }
}
