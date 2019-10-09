using System;
using SCME.Types.Database;
using SCME.WpfControlLibrary.CustomControls;

namespace SCME.ProfileBuilder.Pages
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

        private IDbService GetMsSqlDbService()
        {
            var vm = Vm.ConnectToMSSQLVM;

            var connectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder {DataSource = vm.Server, InitialCatalog = vm.Database, IntegratedSecurity = vm.IntegratedSecurity};

            if (vm.IntegratedSecurity == false)
            {
                connectionStringBuilder.UserID = vm.UserId;
                connectionStringBuilder.Password = vm.Password;
            }

            var sqlConnection = new System.Data.SqlClient.SqlConnection(connectionStringBuilder.ToString());
            return new InterfaceImplementations.NewImplement.MSSQL.MSSQLDbService(sqlConnection);
        }
        
        private IDbService GetSqliteDbService()
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
            return new InterfaceImplementations.NewImplement.SQLite.SQLiteDbService(sqliteConnection);
        }
        
        private void ConnectToMssql()
        {
            try
            {
                Cache.ProfilesPage = new ProfilesPage(GetMsSqlDbService(), Properties.Settings.Default.LastSelectedMMECode);
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
                Cache.ProfilesPage = new ProfilesPage(GetSqliteDbService(), Properties.Settings.Default.LastSelectedMMECode);
                NavigationService?.Navigate(Cache.ProfilesPage);
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }

        
        private void ConnectToSQLite_OnConnectToSqLiteEditProfileBindings()
        {
            try
            {
                NavigationService?.Navigate(new MatchingProfilesCodesPage(GetSqliteDbService()));
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }
        
        private void ConnectToMsSql_OnConnectToSqLiteEditProfileBindings()
        {
            try
            {
                NavigationService?.Navigate(new MatchingProfilesCodesPage(GetMsSqlDbService()));
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }
    }
}
