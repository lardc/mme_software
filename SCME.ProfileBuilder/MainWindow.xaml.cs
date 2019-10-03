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
            ResourceBinding.Scaling(0.75);
            
            DataContext = VM;
            InitializeComponent();
            Cache.Main = this;
            Cache.ConnectPage = new ConnectPage();
            MainFrame.Navigate(Cache.ConnectPage);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.LastSelectedMMECode = Cache.ProfilesPage.Vm.SelectedMmeCode;
            Settings.Default.Save();
            WpfControlLibrary.Properties.Settings.Default.Save();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Cache.ConnectPage.ConnectToMssql();
            
        }
    }
}
