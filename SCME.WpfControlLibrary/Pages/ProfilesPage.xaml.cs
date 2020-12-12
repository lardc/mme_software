using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
        public bool Render { get; set; }
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
            _dispatcherTimerFindProfile.Interval = new TimeSpan(0, 0, 1);
        }

        private void OnDispatcherTimerFindProfileOnTick(object o1, EventArgs args1)
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
            StartAnimationWait();
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
            try
            {
                var profile = _dbService.GetTopProfileByName("IsActive", ProfileVm.SelectedProfile.Name);
            }
            catch
            { }

            new DialogWindow(Properties.Resources.Error, Properties.Resources.PprofileNameAlreadyExists).ShowDialog();
            return false;
        }


        private void EndEditProfile_Click(object sender, RoutedEventArgs e)
        {
            var testParametersAndNormatives = ProfileVm.ProfileDeepDataCopy.TestParametersAndNormatives;

            for(int i = 1; i <= 3; i++)
            {
                string error = null;
                if (ProfileVm.SelectedTestParametersType == TestParametersType.AuxiliaryPower && testParametersAndNormatives.Count(m => m.TestParametersType == TestParametersType.AuxiliaryPower && m.NumberPosition == i) > 1)
                    error = "Превышен лимит измерений для типа вспомогательное напряжение";
                if (ProfileVm.SelectedTestParametersType == TestParametersType.InputOptions && testParametersAndNormatives.Count(m => m.TestParametersType == TestParametersType.InputOptions && m.NumberPosition == i) > 4 )
                    error = "Превышен лимит измерений для типа параметры входа";
                if (ProfileVm.SelectedTestParametersType == TestParametersType.OutputLeakageCurrent && testParametersAndNormatives.Count(m => m.TestParametersType == TestParametersType.OutputLeakageCurrent && m.NumberPosition == i) > 3)
                    error = "Превышен лимит измерений для типа ток утечки на выходе";
                if (ProfileVm.SelectedTestParametersType == TestParametersType.OutputResidualVoltage && testParametersAndNormatives.Count(m => m.TestParametersType == TestParametersType.OutputResidualVoltage && m.NumberPosition == i) > 2)
                    error = "Превышен лимит измерений для типа выходное остаточное напряжение";

                if(error != null)
                {
                    DialogWindow dw = new DialogWindow("Ошибка", $"{error}. Номер канала: {i}");
                    dw.ShowDialog();
                    return;
                }
            }

            StartAnimationWait();
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
        }

        public static void CheckIndexes(ObservableCollection<BaseTestParametersAndNormatives> parameters)
        {

            var q = parameters.GroupBy(m => m.TestParametersType).ToList();
            foreach (var i in parameters.GroupBy(m => m.TestParametersType))
            {
                var w = i.GroupBy(m => m.NumberPosition).ToList();
                foreach (var j in i.GroupBy(m => m.NumberPosition))
                {
                    var n = 1;
                    foreach (var t in j)
                    {
                        if (t.Index != n)
                            t.Index = n;
                        n++;
                    }
                }
            }
        }

        private void _testParametersAndNormatives_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var parameters = sender as ObservableCollection<BaseTestParametersAndNormatives>;
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                if (e.NewItems[0] is Types.AuxiliaryPower.TestParameters)
                    foreach (var i in parameters)
                        i.HaveAuxiliaryPower = true;

                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    if (e.OldItems[0] is Types.AuxiliaryPower.TestParameters)
                        foreach (var i in parameters)
                            i.HaveAuxiliaryPower = false;
        }

        private void AddTestParametersEvent_Click()
        {
            /*switch (ProfileVm.SelectedTestParametersType)
            {
                case TestParametersType.AuxiliaryPower:
                    if(ProfileVm.ProfileDeepDataCopy.TestParametersAndNormatives.Cast<Types.AuxiliaryPower.TestParameters>().Count(m=> m != null) > 0)
                        DialogWindow db = new DialogWindow("Ошибка", "");
                    break ;
            }*/

            
            
            var testParametersAndNormatives = ProfileVm.ProfileDeepDataCopy.TestParametersAndNormatives;



                var maxOrder = testParametersAndNormatives.Count > 0 ? testParametersAndNormatives.Max(m => m.Order) : 0;

            var newTestParameter = BaseTestParametersAndNormatives.CreateParametersByType(ProfileVm.SelectedTestParametersType);
            newTestParameter.DutPackageType = ProfileVm.ProfileDeepDataCopy.DutPackageType;
            newTestParameter.IsEnabled = true;
            newTestParameter.Order = maxOrder + 1;

            
            testParametersAndNormatives.Add(newTestParameter);
            CheckIndexes(ProfileVm.ProfileDeepDataCopy.TestParametersAndNormatives);

            if (newTestParameter.TestParametersType == TestParametersType.AuxiliaryPower)
                    foreach (var i in testParametersAndNormatives)
                        i.HaveAuxiliaryPower = true;

            listViewTestParameters.ScrollBottom();
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
            if(loadingAnimationWindow == null)
                CreateAnimationWait();
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
                StartAnimationWait();
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

        private LoadingAnimationWindow loadingAnimationWindow;
        private void StartAnimationWait()
        {
            
            loadingAnimationWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                loadingAnimationWindow.Show();
            }));

            Dispatcher.BeginInvoke(new Action(() =>
            {
               loadingAnimationWindow.Dispatcher.BeginInvoke(new Action(() =>
               {
                   loadingAnimationWindow.Hide();
                   //loadingAnimationWindow.Visibility = Visibility.Collapsed;
               }));
                Render = true;
            }), DispatcherPriority.ContextIdle, null);
        }
        private void CreateAnimationWait()
        {
            Render = false;
            var window = Application.Current.MainWindow;
            double left = window.Left + window.ActualWidth / 2 - 100;
            double top = window.Top + 20;
            double width = 200;
            double height = 200;

            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    loadingAnimationWindow = new WpfControlLibrary.CustomControls.LoadingAnimationWindow();
                    loadingAnimationWindow.ProfilesPage = this;
                    loadingAnimationWindow.Left = left;
                    loadingAnimationWindow.Top = top;
                    loadingAnimationWindow.Width = width;
                    loadingAnimationWindow.Height = height;

                    Dispatcher.Run();
                }
                catch(Exception ex1)
                {

                }

            }));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();


        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (new DialogWindow("", "Вы действительно хотите удалить профиль", true).ShowDialogWithResult() == true)
            {
                _dbService.DeletingByRenaming(ProfileVm.SelectedProfile);
                ProfileVm.Profiles.Remove(ProfileVm.SelectedProfile);

            }
        }

        private void ListViewProfiles_MouseMove(object sender, MouseEventArgs e)
        {
            var sv = FindVisualChild<ScrollViewer>((sender as ListView));
            if ((sv.ExtentHeight - sv.VerticalOffset) / sv.ExtentHeight < 0.15)
            {
                if (ProfileVm.Profiles.Count < ProfileVm.CountViewProfiels)
                    return;
                ProfileVm.CountViewProfielsN = 0;
                ProfileVm.CountViewProfiels += 100;
                ProfileVm.ProfilesSource.View.Refresh();
            }
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



        private void BeginCloneProfile_Click(object sender, RoutedEventArgs e)
        {
            var newProfile = ProfileVm.SelectedProfile.Copy();
            var name = newProfile.Name;

                name += " - копия";
                if(ProfileVm.Profiles.FirstOrDefault(m => m.Name == name) != null || _dbService.ProfileNameExists(name) != false)
                {
                    for (var i = 2; ; i++)
                    {
                        name += $"({i})";
                        if (ProfileVm.Profiles.FirstOrDefault(m => m.Name == name) == null && _dbService.ProfileNameExists(name) == false)
                            break;
                        name = name.Substring(0, name.Length - $"({i})".Length);
                    }
                }
            

            newProfile.Key = Guid.NewGuid();
            newProfile.Name = name;
            newProfile.Timestamp = DateTime.Now;
            newProfile.Version = 1;
            newProfile.Id = _dbService.InsertUpdateProfile(null, newProfile, ProfileVm.SelectedMmeCode);
            ProfileVm.Profiles.Insert(ProfileVm.Profiles.IndexOf(ProfileVm.SelectedProfile), newProfile);
        }
    }
}