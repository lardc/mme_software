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
    [AddINotifyPropertyChangedInterface]
    public partial class AddTestParametrUserControl : UserControl
    {
        public event AddTestParametrDelegate AddTestParametersEvent;

        public TestParametersType SelectedParametersType { get; set; }

        public AddTestParametrUserControl()
        {
            InitializeComponent();
        }

        private void AddTestParameters_Click(object sender, RoutedEventArgs e)
        {
            AddTestParametersEvent?.Invoke(SelectedParametersType);
        }
    }
}
