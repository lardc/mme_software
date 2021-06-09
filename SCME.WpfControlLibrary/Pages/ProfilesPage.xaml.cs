using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.Database;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.ViewModels;

namespace SCME.WpfControlLibrary.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage
    {
        public ProfilesPageProfileVm ProfileVm { get; set; }
        private readonly IDbService _dbService;
        private readonly bool _isWithoutChild;

        private readonly DispatcherTimer _dispatcherTimerFindProfile = new DispatcherTimer();

        public event Action GoBackAction;
        public event Action AfterLoadAction;

        private Dictionary<string, int> GetMMeCodes => ProfileVm.IsSingleMmeCode
            ? _dbService.GetMmeCodes().Where(m => m.Key == ProfileVm.SelectedMmeCode).ToDictionary(m => m.Key, m => m.Value)
            : _dbService.GetMmeCodes().Where(m => m.Key != Constants.MME_CODE_IS_ACTIVE_NAME).ToDictionary(m => m.Key, m => m.Value);

        public ProfilesPage(IDbService dbService, string mmeCode, bool isSingleMmeCode = false, bool isWithoutChild = false, bool readOnlyMode = false, bool specialMeasure = false)
        {
            ProfileVm = new ProfilesPageProfileVm(dbService);

            if (dbService == null) throw new ArgumentNullException(nameof(dbService));
            if (mmeCode == null) throw new ArgumentNullException(nameof(mmeCode));
//            if (mmeCode.Trim() == "")  throw new ArgumentException (nameof(mmeCode)); 

            ProfileVm.IsSingleMmeCode = isSingleMmeCode;
            InitializeComponent();

            AddTestParameterUserControl.IsReadOnly = ProfileVm.ReadOnlyMode = readOnlyMode;

            _dbService = dbService;
            _isWithoutChild = isWithoutChild;

            ProfileVm.SpecialMeasure = specialMeasure;

            ProfileVm.SelectedMmeCode = mmeCode;
            ProfileVm.MmeCodes = GetMMeCodes;

            if (ProfileVm.SelectedMmeCode == string.Empty)
                ProfileVm.SelectedMmeCode = ProfileVm.MmeCodes.First().Key;

            if (!ProfileVm.MmeCodes.ContainsKey(ProfileVm.SelectedMmeCode))
            {
                _dbService.InsertMmeCode(ProfileVm.SelectedMmeCode);
                ProfileVm.MmeCodes = GetMMeCodes;
            }


            _dispatcherTimerFindProfile.Tick += OnDispatcherTimerFindProfileOnTick;
            _dispatcherTimerFindProfile.Interval = new TimeSpan(9, 0, 0, 1, 500);
        }

        private void OnDispatcherTimerFindProfileOnTick(object o, EventArgs args)
        {
            _dispatcherTimerFindProfile.Stop();
            ProfileVm.ProfilesSource.View.Refresh();
        }

        public void LoadTopProfiles() =>
            ProfileVm.ProfilesSource.Source = ProfileVm.Profiles = new ObservableCollection<MyProfile>(_dbService.GetProfilesSuperficially(ProfileVm.SelectedMmeCode));


        private void BeginEditProfile()
        {
            if (!ProfileVm.SpecialMeasure)
            {
                ProfileVm.ProfileDeepDataCopy = ProfileVm.SelectedProfile.DeepData.Copy();
                ProfileVm.SelectedProfileNameCopy = ProfileVm.SelectedProfile.Name.Copy();
            }

            ProfileVm.IsEditModeActive = true;
        }

        private void BeginEditProfile_Click(object sender, RoutedEventArgs e)
        {
            BeginEditProfile();
        }

        private void CreateNewProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileVm.SelectedProfile = null;
            ProfileVm.ProfileDeepDataCopy = new ProfileDeepData();
            ProfileVm.SelectedProfileNameCopy = _dbService.GetFreeProfileName();
            ProfileVm.IsEditModeActive = true;
        }

        private bool CheckName(string oldName)
        {
            if (ProfileVm.SelectedProfileNameCopy.Equals(oldName))
                return true;
            if (_dbService.ProfileNameExists(ProfileVm.SelectedProfileNameCopy) == false)
                return true;
            new DialogWindow(Properties.Resources.Error, Properties.Resources.PprofileNameAlreadyExists).ShowDialog();
            return false;
        }


        private void EndEditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileVm.SpecialMeasure)
            {
                ProfileVm.SelectedProfile.DeepData = ProfileVm.ProfileDeepDataCopy.Copy();
                ProfileVm.IsEditModeActive = false;
                return;
            }

            var oldName = ProfileVm.SelectedProfile?.Name;
            if (CheckName(oldName) == false)
                return;

            ProfileVm.IsEditModeActive = false;

            var oldProfile = ProfileVm.SelectedProfile;
            MyProfile newProfile;
            if (oldProfile == null)
            {
                newProfile = new MyProfile(0, ProfileVm.SelectedProfileNameCopy, Guid.NewGuid(), 0, DateTime.Now).GenerateNextVersion(ProfileVm.ProfileDeepDataCopy, ProfileVm.SelectedProfileNameCopy);
                newProfile.Id = _dbService.InsertUpdateProfile(oldProfile, newProfile, ProfileVm.SelectedMmeCode);

                ProfileVm.Profiles.Insert(0, newProfile);
            }
            else
            {
                newProfile = oldProfile.GenerateNextVersion(ProfileVm.ProfileDeepDataCopy, ProfileVm.SelectedProfileNameCopy);
                newProfile.Id = _dbService.InsertUpdateProfile(oldProfile, newProfile, ProfileVm.SelectedMmeCode);

                ProfileVm.Profiles.Insert(ProfileVm.Profiles.IndexOf(oldProfile), newProfile);
                ProfileVm.Profiles.Remove(oldProfile);
            }

            ProfileVm.SelectedProfile = newProfile;
            HeightExpander.IsExpanded = false;
        }

        private void CancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileVm.IsEditModeActive = false;
            if (ProfileVm.SelectedProfile == null)
            {
                ProfileVm.ProfileDeepDataCopy = null;
                ProfileVm.SelectedProfileNameCopy = string.Empty;
                return;
            }

            ProfileVm.ProfileDeepDataCopy = ProfileVm.SelectedProfile.DeepData;
            ProfileVm.SelectedProfileNameCopy = ProfileVm.SelectedProfile.Name;
            HeightExpander.IsExpanded = false;
        }

        private void AddTestParametersEvent_Click()
        {
            var testParametersAndNormatives = ProfileVm.ProfileDeepDataCopy.TestParametersAndNormatives;
            var maxOrder = testParametersAndNormatives.Count > 0 ? testParametersAndNormatives.Max(m => m.Order) : 0;

            if (ProfileVm.SelectedTestParametersType == TestParametersType.Clamping)
            {
                HeightMeasure.Visibility = Visibility.Visible;
                ProfileVm.ProfileDeepDataCopy.IsHeightMeasureEnabled = true;
                return;
            }

            var newTestParameter = BaseTestParametersAndNormatives.CreateParametersByType(ProfileVm.SelectedTestParametersType);
            newTestParameter.IsEnabled = true;
            newTestParameter.Order = maxOrder + 1;
            testParametersAndNormatives.Add(newTestParameter);
        }

        private void DisableHeightMeasure_Click(object sender, RoutedEventArgs e)
        {
            HeightMeasure.Visibility = Visibility.Collapsed;
            ProfileVm.ProfileDeepDataCopy.IsHeightMeasureEnabled = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProfileVm.ProfileDeepDataCopy = null;
            ProfileVm.SearchingName = string.Empty;
            ProfileVm.SelectedProfileNameCopy = string.Empty;
            LoadTopProfiles();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ProfileVm.MmeCodes.Count == 0)
                MessageBox.Show(Properties.Resources.Error, Properties.Resources.MissingMMECodes);
            AfterLoadAction?.Invoke();
        }

        public void RefreshProfile(MyProfile newProfile)
        {
            //Убираем задваивание
            newProfile.DeepData.TestParametersAndNormatives.Clear();
            _dbService.InvalidCacheById(ProfileVm.SelectedProfile.Id, ProfileVm.SelectedMmeCode);


            ProfileVm.Profiles.Insert(ProfileVm.Profiles.IndexOf(ProfileVm.SelectedProfile), newProfile);
            ProfileVm.Profiles.Remove(ProfileVm.SelectedProfile);
            ProfileVm.SelectedProfile = newProfile;
        }

        public void RemoveSelectedProfile()
        {
            ProfileVm.Profiles.Remove(ProfileVm.SelectedProfile);
        }


        private void TextBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as ValidatingTextBox).Text != ProfileVm.SearchingName)
                return;
            ProfileVm.CountViewProfielsN = 0;
            ProfileVm.CountViewProfiels = 100;
            ProfileVm.ProfilesSource.View.Refresh();
            //_dispatcherTimerFindProfile.Start();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ProfileVm.SelectedProfile = null;
            ProfileVm.ProfileDeepDataCopy = null;
            GoBackAction?.Invoke();
            NavigationService?.GoBack();
        }

        private void ListViewProfiles_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ProfileVm.SelectedProfile == null)
                    return;
//            if (_disabledProfileSelectionChanged)
//                return;
                if (ProfileVm.SpecialMeasure)
                    BeginEditProfile();
            }
            catch (Exception exception)
            {
                new DialogWindow("Error OnSelectionChanged", exception.ToString()).ShowDialog();
                throw;
            }
        
        }

        private void TestParametersListView_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled) 
                return;
            
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent, Source = sender};
            var parent = (UIElement)((Control) sender).Parent;
            parent.RaiseEvent(eventArg);
        }

        private static TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is TChildItem item)
                    return item;

                var childOfChild = FindVisualChild<TChildItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void ListViewProfiles_MouseMove(object sender, MouseEventArgs e)
        {
            var sv = FindVisualChild<ScrollViewer>((sender as ListViewMouseLeftButtonScroll));
            if ((sv.ExtentHeight - sv.VerticalOffset) / sv.ExtentHeight < 0.1)
            {
                ProfileVm.CountViewProfielsN = 0;
                ProfileVm.CountViewProfiels += 100;
                ProfileVm.ProfilesSource.View.Refresh();
            }
        }
    }
}