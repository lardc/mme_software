using System;
using System.Windows;
using System.Windows.Controls;

namespace SCME.ProfileBuilder.CustomControl.ConnectPage
{
    /// <summary>
    /// Логика взаимодействия для ConnectToMSSQL.xaml
    /// </summary>
    public partial class ConnectToMSSQL : UserControl
    {
        public event Action ConnetToMSSQL;
        public ConnectToMSSQL()
        {
            InitializeComponent();
        }

        private void ConnectToMSSQL_Click(object sender, RoutedEventArgs e)
        {
            ConnetToMSSQL?.Invoke();
        }
    }
}
