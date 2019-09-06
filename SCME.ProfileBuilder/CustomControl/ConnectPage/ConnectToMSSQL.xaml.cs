using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SCME.ProfileBuilder.CustomControl.ConnectPage
{
    /// <summary>
    /// Логика взаимодействия для ConnectToMSSQL.xaml
    /// </summary>
    public partial class ConnectToMSSQL : UserControl
    {


        public ConnectToMSSQL()
        {
            InitializeComponent();
        }

        private void ButtonSQLConnect_Click(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(tbDBName.Text) && !string.IsNullOrEmpty(tbServerName.Text))
            //{
            //    try
            //    {
            //        var connectionString = (chkDbIntSec.IsChecked != null && (bool)chkDbIntSec.IsChecked)
            //            ? $"Server={tbServerName.Text}; Database={tbDBName.Text}; Integrated Security=true;"
            //            : $"Server={tbServerName.Text}; Database={tbDBName.Text}; User Id={tbDBUser.Text}; Password={tbDBPassword.Text};";

            //        IProfilesService service = new SQLProfilesService(connectionString);
            //        IProfilesConnectionService connectionService = new SQLProfilesConnectionService(connectionString, service);

            //        Cache.ProfileEdit = new ProfilePage(service);
            //        Cache.ConnectionsPage = new Connections(connectionService);

            //        NavigationService?.Navigate(Cache.ProfileEdit);
            //    }
            //    catch (Exception ex)
            //    {
            //        var dw = new DialogWindow("Error", ex.Message);
            //        dw.ButtonConfig(DialogWindow.EbConfig.OK);
            //        dw.ShowDialog();
            //    }
            //}
        }
    }
}
