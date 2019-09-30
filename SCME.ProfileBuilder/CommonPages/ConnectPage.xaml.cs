using System;
using SCME.WpfControlLibrary.CustomControls;

namespace SCME.ProfileBuilder.CommonPages
{
    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    public partial class ConnectPage
    {
        public ViewModels.ConnectPage.ConnectPageVM Vm { get; set; } = new ViewModels.ConnectPage.ConnectPageVM();

        public ConnectPage()
        {
            InitializeComponent();
        }

        public void ConnectToMssql()
        {
            try
            {
                var vm = Vm.ConnectToMSSQLVM;

                var connectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();

                connectionStringBuilder.DataSource = vm.Server;
                connectionStringBuilder.InitialCatalog = vm.Database;
                connectionStringBuilder.IntegratedSecurity = vm.IntegratedSecurity;
                if (vm.IntegratedSecurity == false)
                {
                    connectionStringBuilder.UserID = vm.UserId;
                    connectionStringBuilder.Password = vm.Password;
                }

                var sqlConnection = new System.Data.SqlClient.SqlConnection(connectionStringBuilder.ToString());
                Cache.ProfilesPage = new ProfilesPage(new InterfaceImplementations.NewImplement.MSSQL.MSSQLDbService(sqlConnection));

                NavigationService?.Navigate(Cache.ProfilesPage);
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }

        private void ConnectToSqLite()
        {
            try
            {
                var vm = Vm.ConnectToSQLiteVM;

                var connectionStringBuilder = new System.Data.SQLite.SQLiteConnectionStringBuilder()
                {
                    DataSource = vm.SQLiteFileName,
                    SyncMode = System.Data.SQLite.SynchronizationModes.Full,
                    JournalMode = System.Data.SQLite.SQLiteJournalModeEnum.Truncate,
                    FailIfMissing = true
                };

                var sqliteConnection = new System.Data.SQLite.SQLiteConnection(connectionStringBuilder.ToString());
                Cache.ProfilesPage = new ProfilesPage(new InterfaceImplementations.NewImplement.SQLite.SQLiteDbService(sqliteConnection));

                NavigationService?.Navigate(Cache.ProfilesPage);
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }
    }
}
