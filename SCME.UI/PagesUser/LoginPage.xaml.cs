using SCME.Types;
using SCME.UIServiceConfig.Properties;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.Pages;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SCME.UI.PagesUser
{
    /// <summary>Страница авторизации</summary>
    public partial class LoginPage
    {
        //Анимация загрузки окна
        private LoadingAnimationWindow LoadingAnimationWindow;

        /// <summary>Инициализирует новый объект класса LoginPage</summary>
        public LoginPage()
        {
            InitializeComponent();
            CurrentAccount = new CAccount();
            //Подгрузка списка пользователей
            accountsListBox.ItemsSource = new AccountEngine(Settings.Default.AccountsPath).Collection;
            //Список пользователей пуст
            if (accountsListBox.Items.Count == 0)
                buttonNext.Visibility = Visibility.Hidden;
            else
                accountsListBox.SelectedIndex = 0;
        }

        /// <summary>Текущий пользователь</summary>
        private CAccount CurrentAccount
        {
            get; set;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e) //Загрузка окна
        {
            lblIncorrect.Content = string.Empty;
        }

        private void BtBack_OnClick(object sender, RoutedEventArgs e) //Переход на предыдущую страницу
        {
                NavigationService?.Navigate(Cache.UserWorkMode);
        }        
        
        private void ButtonNext_OnClick(object sender, RoutedEventArgs e) //Переход на следующую страницу
        {
            //Отображение анимации загрузки окна
            double Left = Cache.Main.GetWaitProgressBarPoint().X + Cache.Main.Left;
            double Top = Cache.Main.GetWaitProgressBarPoint().Y + Cache.Main.Top;
            double Width = Cache.Main.GetWaitProgressBarSize().X;
            double  Height = Cache.Main.GetWaitProgressBarSize().Y;
            Thread NewWindowThread = new Thread(new ThreadStart(() =>
            {
                LoadingAnimationWindow = new LoadingAnimationWindow
                {
                    Left = Left,
                    Top = Top,
                    Width = Width,
                    Height = Height
                };
                LoadingAnimationWindow.Show();
                Dispatcher.Run();
            }));
            NewWindowThread.SetApartmentState(ApartmentState.STA);
            NewWindowThread.IsBackground = true;
            NewWindowThread.Start();
            //Некорректный пароль
            if (tbPassword.Text != CurrentAccount.Password || NavigationService == null)
            {
                lblIncorrect.Content = Properties.Resources.PasswordIncorrect;
                tbPassword.Text = string.Empty;
                return;
            }
            lblIncorrect.Content = string.Empty;
            Cache.Main.VM.AccountName = CurrentAccount.Name;
            PrepareMoveToSelectProfilePage(Cache.ProfilesPageSelectForTest);
            Cache.ProfilesPageSelectForTest.AfterLoadAction -= ProfilesPageSelectForTest_AfterLoadAction;
            Cache.ProfilesPageSelectForTest.AfterLoadAction += ProfilesPageSelectForTest_AfterLoadAction;
            Debug.Assert(NavigationService != null, nameof(NavigationService) + " != null");
            NavigationService.Navigate(Cache.ProfilesPageSelectForTest);
            tbPassword.Text = string.Empty;
        }

        private void ProfilesPageSelectForTest_AfterLoadAction() //Окончание анимации загрузки окна
        {
            LoadingAnimationWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadingAnimationWindow.Close();
            }));
        }

        /// <summary>Подготовка к переходу на страницу профилей</summary>
        /// <param name="profilesPage">Страница профилей</param>
        public static void PrepareMoveToSelectProfilePage(ProfilesPage profilesPage)
        {
            Cache.Main.VM.AccountNameIsVisibility = true;
            profilesPage.LoadTopProfiles();
            profilesPage.Title = string.Format("{0}: {1}", Properties.Resources.AllProfiles, profilesPage.ProfileVm.Profiles.Count);
            profilesPage.ProfileVm.NextAction = () =>
            {
                Cache.UserTest.Profile = profilesPage.ProfileVm.SelectedProfile.ToProfile();
                Cache.UserTest.Title = string.Format("{0}, {1}: \n{2}", Properties.Resources.UserTestPage_Title, Properties.Resources.Profile.ToLower(), Cache.UserTest.Profile);
                Cache.UserTest.SpecialMeasureMode = (Cache.WorkMode == UserWorkMode.SpecialMeasure);
                NavigationService navigationService = profilesPage.NavigationService;
                Cache.UserTest.InitSorting();
                Cache.UserTest.InitTemp();                    
                Debug.Assert(navigationService != null, "navigationService != null");
                navigationService.Navigate(Cache.UserTest);
            };
        }

        private void AccountsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) //Выбор в списке пользователей
        {
            CurrentAccount = (CAccount)accountsListBox.SelectedItem;
            lblIncorrect.Content = string.Empty;
        }
    }
}