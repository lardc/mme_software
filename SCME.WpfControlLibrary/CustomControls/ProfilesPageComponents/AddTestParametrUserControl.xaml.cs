using PropertyChanged;
using SCME.Types.BaseTestParams;
using SCME.WpfControlLibrary.IValueConverters;
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

namespace SCME.WpfControlLibrary.CustomControls.ProfilesPageComponents
{
    /// <summary>
    /// Логика взаимодействия для AddTestParametrUserControl.xaml
    /// </summary>
    public partial class AddTestParameterUserControl : UserControl
    {
        public event Action AddTestParametersEvent;

        public AddTestParameterUserControl()
        {
            InitializeComponent();
        }

        private void AddTestParameters_Click(object sender, RoutedEventArgs e)
        {
            AddTestParametersEvent?.Invoke();
        }
    }
}
