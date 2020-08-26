using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SCME.Types;
using SCME.Types.Profiles;
using SCME.UI.IO;
using SCME.UIServiceConfig.Properties;
using SCME.WpfControlLibrary;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.Pages;

namespace SCME.UI.PagesUser
{
    /// <summary>
    ///     Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage
    {
        private CAccount CurrentAccount { get; set; }

        public LoginPage()
        {
            InitializeComponent();

            CurrentAccount = new CAccount();

            accountsListBox.ItemsSource = new AccountEngine(Settings.Default.AccountsPath).Collection;

            if (accountsListBox.Items.Count > 0)
                accountsListBox.SelectedIndex = 0;
            else
                buttonNext.Visibility = Visibility.Hidden;
        }

        private void btBack_OnClick(object sender, RoutedEventArgs e)
        {
                NavigationService?.Navigate(Cache.UserWorkMode);
        }


        private LoadingAnimationWindow loadingAnimationWindow;
        private void ButtonNext_OnClick(object Sender, RoutedEventArgs E)
        {
            double left = Cache.Main.GetWaitProgressBarPoint.X + Cache.Main.Left;
            double top = Cache.Main.GetWaitProgressBarPoint.Y + Cache.Main.Top;
            double width = Cache.Main.GetWaitProgressBarSize.X;
            double  height = Cache.Main.GetWaitProgressBarSize.Y;
            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                loadingAnimationWindow = new WpfControlLibrary.CustomControls.LoadingAnimationWindow();
                loadingAnimationWindow.Left = left;
                loadingAnimationWindow.Top = top;
                loadingAnimationWindow.Width = width;
                loadingAnimationWindow.Height = height;
                loadingAnimationWindow.Show();
                System.Windows.Threading.Dispatcher.Run();
            }));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();



            //            if (!Cache.Main.IsProfilesParsed)
            //                ProfilesDbLogic.ImportProfilesFromDb();

            if (tbPassword.Text == CurrentAccount.Password && NavigationService != null)
            {
                lblIncorrect.Content = "";
                Cache.Main.VM.AccountName = CurrentAccount.Name;
//                Cache.ProfileEdit.ClearFilter();
//                Cache.ProfileSelection.InitFilter();
//                Cache.ProfileSelection.InitSorting();
                //NavigationService.Navigate(Cache.ProfileSelection);
                PrepareMoveToSelectProfilePage(Cache.ProfilesPageSelectForTest);
                Cache.ProfilesPageSelectForTest.AfterLoadAction -= ProfilesPageSelectForTest_AfterLoadAction;
                Cache.ProfilesPageSelectForTest.AfterLoadAction += ProfilesPageSelectForTest_AfterLoadAction;
                Debug.Assert(NavigationService != null, nameof(NavigationService) + " != null");
                NavigationService.Navigate(Cache.ProfilesPageSelectForTest);
            }
            else
                lblIncorrect.Content = Properties.Resources.PasswordIncorrect;

            tbPassword.Text = string.Empty;
        }

        private void ProfilesPageSelectForTest_AfterLoadAction()
        {
            loadingAnimationWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                loadingAnimationWindow.Close();
            }));
        }

        public static void PrepareMoveToSelectProfilePage(ProfilesPage profilesPage)
        {
            Cache.Main.VM.AccountNameIsVisibility = true;
            profilesPage.LoadTopProfiles();
            profilesPage.Title = $"{Properties.Resources.AllProfiles}: {profilesPage.ProfileVm.Profiles.Count}";
//            Cache.ProfilesPageSelectForTest.GoBackAction += () =>
//            {
//                var navigationService = Cache.ProfilesPageSelectForTest.NavigationService;
//                Debug.Assert(navigationService != null, nameof(navigationService) + " != null");
//                navigationService.Navigate(Cache.Login);
//            };
            profilesPage.ProfileVm.NextAction = () =>
            {
                
                Cache.UserTest.Profile = profilesPage.ProfileVm.SelectedProfile.ToProfile();
                Cache.UserTest.Title = Properties.Resources.UserTestPage_Title + ", " + Properties.Resources.Profile.ToLower() + ": " + "\n" + Cache.UserTest.Profile;
                //запоминаем в UserTest флаг 'Режим специальных измерений' для возможности корректной работы её MultiIdentificationFieldsToVisibilityConverter 
                Cache.UserTest.SpecialMeasureMode = (Cache.WorkMode == UserWorkMode.SpecialMeasure);

                var navigationService = profilesPage.NavigationService;

                Cache.UserTest.InitSorting();
                Cache.UserTest.InitTemp();
                    
                Debug.Assert(navigationService != null, "navigationService != null");
                navigationService.Navigate(Cache.UserTest);
            };
            
        }

        private void AccountsListBox_OnSelectionChanged(object Sender, SelectionChangedEventArgs E)
        {
            CurrentAccount = (CAccount) accountsListBox.SelectedItem;
            lblIncorrect.Content = "";
        }

        private void LoginPage_Loaded(object Sender, RoutedEventArgs E)
        {
            lblIncorrect.Content = "";
//            Cache.Main.VM.AccountName = string.Empty;
        }

        //private void TbPassword_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (WpfControlLibrary.Properties.Settings.Default.IsTouchUI && FindParent<Window>(this) is IMainWindow window)
        //        window.ShowKeyboard(true, sender as );
        //}
    }
}