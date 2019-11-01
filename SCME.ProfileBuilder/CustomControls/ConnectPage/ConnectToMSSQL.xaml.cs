using System;
using System.Windows;
using System.Windows.Controls;

namespace SCME.ProfileBuilder.CustomControls.ConnectPage
{
    /// <summary>
    /// Логика взаимодействия для ConnectToMSSQL.xaml
    /// </summary>
    public partial class ConnectToMSSQL : UserControl
    {
        public event Action ConnectToMssqlEditProfiles;
        public event Action ConnectToMssqlEditProfileBindings;
        public ConnectToMSSQL()
        {
            InitializeComponent();
        }

        private void ConnectToMSSQLEditProfiles_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectToMssqlEditProfiles?.Invoke();
        }

        private void ConnectToMSSQLMmeCodesToProfile_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectToMssqlEditProfileBindings?.Invoke();
        }
    }
}
