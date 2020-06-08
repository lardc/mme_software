using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using SCME.InterfaceImplementations;
using SCME.Interfaces;
using SCME.ProfileBuilder.CustomControl;
using SCME.ProfileBuilder.PagesTech;
using SCME.ProfileBuilder.Properties;
using SCME.Types;
using SCME.Types.DatabaseServer;

namespace SCME.ProfileBuilder
{
    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    public partial class ConnectPage : Page
    {
        private string dbSettings = "synchronous=Full;journal mode=Truncate;failifmissing=True";

        public ConnectPage()
        {
            InitializeComponent();
        }
        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "SQLite database|*.sqlite",
                FilterIndex = 1
            };
            var result = dlg.ShowDialog();
            if (result != true) return;
            // Open document
            var filename = dlg.FileName;
            tbDbPath.Text = filename;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbDbPath.Text))
            {
                try
                {
                    IProfilesService service = new SQLiteProfilesService(string.Format("data source={0};{1}", tbDbPath.Text, dbSettings));
                    IProfilesConnectionService connectionService = new SQLiteProfilesConnectionService(string.Format("data source={0};{1}", tbDbPath.Text, dbSettings));
                    Cache.ProfileEdit = new ProfilePage(service);
                    Cache.ConnectionsPage = new Connections(connectionService);
                    if (NavigationService != null)
                    {
                        NavigationService.Navigate(Cache.ProfileEdit);
                    }
                }
                catch (Exception ex)
                {
                    var dw = new DialogWindow("Error", ex.Message);
                    dw.ButtonConfig(DialogWindow.EbConfig.OK);
                    dw.ShowDialog();
                }
            }

        }

        private void ButtonWcfConnect_OnClick(object sender, RoutedEventArgs e)
        {
            var client = new CentralDatabaseServiceClient();
            Cache.ProfileEdit = new ProfilePage(client);
            Cache.ConnectionsPage = new Connections(client);
            if (NavigationService != null)
            {
                NavigationService.Navigate(Cache.ProfileEdit);
            }
        }
    }
}
