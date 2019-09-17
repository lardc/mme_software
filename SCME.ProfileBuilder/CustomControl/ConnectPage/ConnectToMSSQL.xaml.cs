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
