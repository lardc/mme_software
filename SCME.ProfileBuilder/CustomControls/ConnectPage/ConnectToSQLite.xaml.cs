using Microsoft.Win32;
using SCME.ProfileBuilder.ViewModels.ConnectPage;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SCME.ProfileBuilder.CustomControls.ConnectPage
{
    /// <summary>
    /// Логика взаимодействия для ConnectToSQLite.xaml
    /// </summary>
    public partial class ConnectToSQLite : UserControl
    {
        public event Action ConnectToSqLiteEditProfiles;
        public event Action ConnectToSqLiteEditProfileBindings;
        private ConnectToSQLiteVM Vm => DataContext as ConnectToSQLiteVM;
        public ConnectToSQLite()
        {
            InitializeComponent();
        }

        private void SelectSqlPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "SQLite database|*.sqlite",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            Vm.SQLiteFileName = openFileDialog.FileName;
        }

        private void ConnetToSQLiteEditProfiles_Click(object sender, RoutedEventArgs e)
        {
            ConnectToSqLiteEditProfiles?.Invoke();
        }
        
        private void ConnetToSQLiteEditProfileBindings_Click(object sender, RoutedEventArgs e)
        {
            ConnectToSqLiteEditProfileBindings?.Invoke();
        }
        
    }
}
