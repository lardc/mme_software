using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.WpfControlLibrary.DataTemplates.TestParameters;
using SCME.WpfControlLibrary.Pages;
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

namespace TestWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var q = new BaseTestParametersAndNormatives[]
            {
                new SCME.Types.InputOptions.TestParameters(){NumberPosition = 1, TypeManagement = TypeManagement.ACVoltage, InputCurrentMinimum = 3, InputCurrentMaximum = 5},
                new SCME.Types.InputOptions.TestParameters(){NumberPosition = 2, TypeManagement = TypeManagement.DCAmperage, InputVoltageMinimum = 3, InputVoltageMaximum = 4},
                new SCME.Types.OutputLeakageCurrent.TestParameters(){NumberPosition = 3, LeakageCurrentMinimum = 1, LeakageCurrentMaximum = 4}

            };
            //MainFrame.Navigate(new SSRTUResultPage(null));
        }
    }
}
