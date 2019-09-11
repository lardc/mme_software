using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using MahApps.Metro.Controls;
using SCME.InterfaceImplementations;
using SCME.ProfileBuilder.CommonPages;
using SCME.ProfileBuilder.PagesTech;
using SCME.ProfileBuilder.Properties;
using SCME.Types.DatabaseServer;

namespace SCME.ProfileBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ViewModels.MainWindowVM VM { get; set; } = new ViewModels.MainWindowVM();
        public MainWindow()
        {

            DataContext = VM;
            

            InitializeComponent();

            
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.CurrentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.CurrentCulture);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
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
            Cache.ConnectPage.ConnectToMSSQL();
        }
    }
}
