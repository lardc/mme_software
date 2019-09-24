﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using SCME.InterfaceImplementations;
using SCME.ProfileBuilder.CustomControl;
using SCME.Types.DatabaseServer;
using SCME.WpfControlLibrary.CustomControls;

namespace SCME.ProfileBuilder.CommonPages
{
    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    public partial class ConnectPage : Page
    {
        public ViewModels.ConnectPage.ConnectPageVM VM { get; set; } = new ViewModels.ConnectPage.ConnectPageVM();

        public ConnectPage()
        {
            InitializeComponent();
        }

        public async void ConnectToMSSQL()
        {
            try
            {
                var vm = VM.ConnectToMSSQLVM;

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
                await Cache.Main.ShowMessageAsync("Error", ex.Message, MessageDialogStyle.Affirmative);
            }
        }

        public async void ConnectToSQLite()
        {
            try
            {
                var vm = VM.ConnectToSQLiteVM;

                System.Data.SQLite.SQLiteConnectionStringBuilder connectionStringBuilder = new System.Data.SQLite.SQLiteConnectionStringBuilder()
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
                await Cache.Main.ShowMessageAsync("Error", ex.Message, MessageDialogStyle.Affirmative);
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            ((TextBlock) sender).IsEnabled = true;
        }
    }
}
