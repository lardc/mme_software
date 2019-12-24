using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Navigation;
using SCME.InterfaceImplementations.NewImplement.MSSQL;
using SCME.ProfileBuilder.Pages;
using SCME.ProfileBuilder.Properties;
using SCME.WpfControlLibrary;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.IValueConverters;

namespace SCME.ProfileBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ResourceBinding.Scaling(0.60);
            
            InitializeComponent();
            Cache.Main = this;
            Cache.SelectEditModePage = new SelectEditModePage();
            
            var navigationService = NavigationService.GetNavigationService(MainFrame);
            
            MainFrame?.Navigate(Cache.SelectEditModePage);
            
            //MainFrame.Navigate(new ProfilesPage(new MSSQLDbService(new SqlConnection(@"Data Source=IVAN-PC\SQLEXPRESS01;Initial Catalog=SCME_ResultsDBTest;Integrated Security=true;")), "MME005"));
            //MainFrame?.Navigate(new MatchingProfilesCodesPage(new MSSQLDbService(new SqlConnection(@"Data Source=IVAN-PC\SQLEXPRESS01;Initial Catalog=SCME_ResultsDBTest;Integrated Security=true;"))));
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.LastSelectedMMECode = Cache.ProfilesPage?.ProfileVm.SelectedMmeCode;
            Settings.Default.Save();
            WpfControlLibrary.Properties.Settings.Default.Save();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //new DynamicResourcesChanger().Show();
            //Cache.ConnectPage.ConnectToMssql();
            
        }
    }
}
