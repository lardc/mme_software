using System;
using System.Data.SQLite;
using System.Windows;
using SCME.Types;
using SCME.Types.Database;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.Pages;

namespace SCME.ProfileBuilder.Pages
{
    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    public partial class SelectEditModePage
    {
        public SelectEditModePage(bool contentLoaded)
        {
            _contentLoaded = contentLoaded;
        }

        public ViewModels.ConnectPage.ConnectPageVM Vm { get; set; } = new ViewModels.ConnectPage.ConnectPageVM();

        public SelectEditModePage()
        {
            InitializeComponent();
        }

        private IDbService GetMsSqlDbService()
        {
            var prop = Properties.Settings.Default;
            var connectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = prop.MSSQLServer,
                InitialCatalog = prop.MSSQLDatabase,
                IntegratedSecurity = prop.MSSQLIntegratedSecurity,
                ConnectTimeout = prop.SQLTimeout
            };

            if (prop.MSSQLIntegratedSecurity == false)
            {
                connectionStringBuilder.UserID = prop.MSSQLUserId;
                connectionStringBuilder.Password = prop.MSSQLPassword;
            }

            var sqlConnection = new System.Data.SqlClient.SqlConnection(connectionStringBuilder.ToString());
            var service = new InterfaceImplementations.NewImplement.MSSQL.MSSQLDbService(sqlConnection);
            service.Migrate();
            return service;
        }

        private IDbService GetSqliteDbService()
        {
            var prop = Properties.Settings.Default;
            var connectionStringBuilder = new SQLiteConnectionStringBuilder()
            {
                DataSource = prop.SQLiteFileName,
                DefaultTimeout = prop.SQLTimeout,
                SyncMode = SynchronizationModes.Full,
                JournalMode = SQLiteJournalModeEnum.Truncate,
                FailIfMissing = true
            };

            var sqliteConnection = new SQLiteConnection(connectionStringBuilder.ToString());
            var service = new InterfaceImplementations.NewImplement.SQLite.SQLiteDbService(sqliteConnection);
            service.Migrate();
            return service;
        }

        private void EditProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (Properties.Settings.Default.TypeDb)
                {
                    case TypeDb.SQLite:
                        NavigationService?.Navigate(new ProfilesPage(GetSqliteDbService(), Properties.Settings.Default.LastSelectedMMECode));
                        break;
                    case TypeDb.MSSQL:
                        NavigationService?.Navigate(new ProfilesPage(GetMsSqlDbService(), Properties.Settings.Default.LastSelectedMMECode));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }

        private void EditProfileMme_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (Properties.Settings.Default.TypeDb)
                {
                    case TypeDb.SQLite:
                        NavigationService?.Navigate(new MatchingProfilesCodesPage(GetSqliteDbService()));
                        break;
                    case TypeDb.MSSQL:
                        NavigationService?.Navigate(new MatchingProfilesCodesPage(GetMsSqlDbService()));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }
    }
}