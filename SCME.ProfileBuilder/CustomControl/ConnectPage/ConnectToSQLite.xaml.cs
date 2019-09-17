using Microsoft.Win32;
using SCME.ProfileBuilder.ViewModels.ConnectPage;
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
    /// Логика взаимодействия для ConnectToSQLite.xaml
    /// </summary>
    public partial class ConnectToSQLite : UserControl
    {
        public event Action ConnetToSQLite;
        private ConnectToSQLiteVM _VM => DataContext as ConnectToSQLiteVM;
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

            _VM.SQLiteFileName = openFileDialog.FileName;
        }

        private void SQliteConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnetToSQLite.Invoke();
        }
    }
}
