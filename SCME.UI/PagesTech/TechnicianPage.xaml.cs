using SCME.Types;
using SCME.UI.IO;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SCME.UI.PagesTech
{
    public partial class TechnicianPage
    {
        //Предыдущая страница
        internal object PreviousPage;
        //Отступ на страницах
        private Thickness PageMargin = new Thickness(0, 0, 10, 0);

        /// <summary>Инициализирует новый экземпляр класса TechnicianPage</summary>
        public TechnicianPage()
        {
            InitializeComponent();
        }

        private void TechnicianPage_Loaded(object sender, RoutedEventArgs e) //Загрузка страницы
        {
            Cache.ProfilesPage.GoBackAction += () =>
            {
                ProfilesDbLogic.LoadProfile(Cache.ProfilesPage.ProfileVm.Profiles.ToList());
            };
            Cache.Main.VM.AccountNameIsVisibility = false;
        }

        /// <summary>Проверка доступности кнопок</summary>
        /// <param name="param">Параметры конфигурации</param>
        internal void AreButtonsEnabled(TypeCommon.InitParams param)
        {
            btnGTU.IsEnabled = param.IsGateEnabled;
            btnSL.IsEnabled = param.IsSLEnabled;
            btnBVT.IsEnabled = param.IsBVTEnabled;
            btndVdt.IsEnabled = param.IsdVdtEnabled;
            btnATU.IsEnabled = param.IsATUEnabled;
            btnQrrTq.IsEnabled = param.IsQrrTqEnabled;
            btnTOU.IsEnabled = param.IsTOUEnabled;
        }

        private void Button_Click(object sender, RoutedEventArgs e) //Выбор теста в режиме наладчика
        {
            Button ClickedButton = (Button)sender;
            Page Page = null;
            switch (Convert.ToUInt16(ClickedButton.CommandParameter))
            {
                case 1:
                    Cache.Gate = new GatePage();
                    Page = Cache.Gate;
                    Page.Margin = PageMargin;
                    break;
                case 2:
                    Cache.Sl = new SLPage();
                    Page = Cache.Sl;
                    Page.Margin = PageMargin;
                    break;
                case 3:
                    Cache.Bvt = new BvtPage();
                    Page = Cache.Bvt;
                    Page.Margin = PageMargin;
                    break;
                case 4:
                    Page = Cache.Settings;                    
                    break;
                case 6:
                    Page = Cache.Console;
                    break;
                case 7:
                    Page = Cache.ProfilesPage;
                    break;
                case 9:
                    Page = Cache.Results;
                    break;
                case 10:
                    Cache.Welcome.IsBackEnable = true;
                    Cache.Welcome.IsRestartEnable = true;
                    Page = Cache.Welcome;
                    break;
                case 11:
                    Page = Cache.Clamp;
                    Page.Margin = PageMargin;
                    break;
                case 12:
                    Cache.DVdt = new DVdtPage();
                    Page = Cache.DVdt;
                    Page.Margin = PageMargin;
                    break;
                case 13:
                    Cache.ATU = new ATUPage();
                    Page = Cache.ATU;
                    Page.Margin = PageMargin;
                    break;
                case 14:
                    Cache.QrrTq = new QrrTqPage();
                    Page = Cache.QrrTq;
                    Page.Margin = PageMargin;
                    break;
                case 17:
                    Cache.TOU = new TOUPage();
                    Page = Cache.TOU;
                    Page.Margin = PageMargin;
                    break;
            }
            if (Page != null && NavigationService != null)
                NavigationService.Navigate(Page);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) //Переход на предыдущую страницу
        {
            if (NavigationService == null)
                return;
            if (PreviousPage == null)
                NavigationService.Navigate(Cache.Login);
            Cache.Password.AfterOkRoutine = delegate
            {
                Cache.Main.mainFrame.Navigate(Cache.Technician);
            };
            NavigationService.GoBack();
        }
    }
}