using System;
using System.Data.SqlClient;
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

        public SelectEditModePage()
        {
            InitializeComponent();
        }

        private Properties.Settings Settings => Properties.Settings.Default;

        private SqlConnectionStringBuilder GetSqlConnectionStringBuilder =>
            new SqlConnectionStringBuilder()
            {
                DataSource = Settings.MSSQLServer,
                InitialCatalog = Settings.MSSQLDatabase,
                IntegratedSecurity = Settings.MSSQLIntegratedSecurity,
                ConnectTimeout = Settings.SQLTimeout,
                UserID = Settings.MSSQLIntegratedSecurity == false ? Settings.MSSQLUserId : null,
                Password = Settings.MSSQLIntegratedSecurity == false ? Settings.MSSQLPassword : null
            };

        private SQLiteConnectionStringBuilder GetSQLiteConnectionStringBuilder =>
            new SQLiteConnectionStringBuilder()
            {
                DataSource = Settings.SQLiteFileName,
                DefaultTimeout = Settings.SQLTimeout,
                SyncMode = SynchronizationModes.Full,
                JournalMode = SQLiteJournalModeEnum.Truncate,
                FailIfMissing = true
            };

        private IDbService GetMsSqlDbService()
        {
            var connectionStringBuilder = GetSqlConnectionStringBuilder;
          
            var sqlConnection = new SqlConnection(connectionStringBuilder.ToString());
            var service = new InterfaceImplementations.NewImplement.MSSQL.MSSQLDbService(sqlConnection);
            service.Migrate();
            return service;
        }

        private IDbService GetSqliteDbService()
        {
            var connectionStringBuilder = GetSQLiteConnectionStringBuilder;

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

                SetTitle();
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

                SetTitle();
            }
            catch (Exception ex)
            {
                new DialogWindow(WpfControlLibrary.Properties.Resources.Error, ex.ToString()).ShowDialog();
            }
        }

        private void SetTitle()
        {
            switch (Properties.Settings.Default.TypeDb)
            {
                case TypeDb.SQLite:
                    Cache.Main.Title = $"SCME.ProfileBuilder SQLite {GetSQLiteConnectionStringBuilder.DataSource}";
                    break;
                case TypeDb.MSSQL:
                    var connection = GetSqlConnectionStringBuilder;
                    Cache.Main.Title = $"SCME.ProfileBuilder MSSQL Server={connection.DataSource} Database={connection.InitialCatalog}";
                    break;
                default:
                    throw new NotImplementedException();
            }
            
        }
    }
}