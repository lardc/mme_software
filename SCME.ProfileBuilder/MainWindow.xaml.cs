using System.Windows;
using MahApps.Metro.Controls;
using SCME.ProfileBuilder.CommonPages;
using SCME.ProfileBuilder.Properties;
using SCME.WpfControlLibrary;

namespace SCME.ProfileBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ViewModels.MainWindowVM VM { get; set; } = new ViewModels.MainWindowVM();
        public MainWindow()
        {
            ResourceBinding.Scaling();
            
            DataContext = VM;
            InitializeComponent();
            Cache.Main = this;
            Cache.ConnectPage = new ConnectPage();
            MainFrame.Navigate(Cache.ConnectPage);
            

            //var connectionString = @"Server=IVAN-PC\SQLEXPRESS01; Database=SCME_ResultsDB; Integrated Security=true;";
            //IProfilesService service = new SQLProfilesService(connectionString);
            //mainFrame.Navigate(new ProfilesPage(service));


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
            WpfControlLibrary.Properties.Settings.Default.Save();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Cache.ConnectPage.ConnectToMssql();
            ResourceBinding.Scaling(0.75);
        }
    }
}
