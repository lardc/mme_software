using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.WpfControlLibrary.DataTemplates.TestParameters;
using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SCME.WpfControlLibrary.Pages
{
    /// <summary>
    /// Логика взаимодействия для ImpulseResultPage.xaml
    /// </summary>
    public partial class ImpulseResultPage : Page
    {
        public ImpulseResultVM VM { get; set; } = new ImpulseResultVM();
        public ImpulseResultComponentVM VMPosition1 { get; set; } = new ImpulseResultComponentVM() { Postition = 1};
        public ImpulseResultComponentVM VMPosition2 { get; set; } = new ImpulseResultComponentVM() { Postition = 2 };
        public ImpulseResultComponentVM VMPosition3 { get; set; } = new ImpulseResultComponentVM() { Postition = 3 };
        public ImpulseResultPage()
        {
            InitializeComponent();
        }

        public ImpulseResultPage(BaseTestParametersAndNormatives[] parameters)
        {
            InitializeComponent();
            Dictionary<int, ImpulseResultComponentVM> q = new Dictionary<int, ImpulseResultComponentVM>();
            q[1] = VMPosition1;
            q[2] = VMPosition2;
            q[3] = VMPosition3;
            foreach (var i in parameters)
            {
                var impulseResultComponentVM = q[i.NumberPosition];
                switch (i)
                {
                    case SCME.Types.InputOptions.TestParameters j:
                        if (j.TypeManagement == Types.TypeManagement.DCAmperage)
                        {
                            impulseResultComponentVM.InputVoltageMin = j.InputVoltageMinimum;
                            impulseResultComponentVM.InputVoltageMax = j.InputVoltageMaximum;
                            impulseResultComponentVM.InputVoltage = 0;
                        }
                        else
                        {
                            impulseResultComponentVM.InputAmperageMin = j.InputCurrentMinimum;
                            impulseResultComponentVM.InputAmperageMax = j.InputCurrentMaximum;
                            impulseResultComponentVM.InputAmperage = 0;
                        }
                        break;
                    case SCME.Types.OutputLeakageCurrent.TestParameters j:
                        impulseResultComponentVM.LeakageCurrentMin = j.LeakageCurrentMinimum;
                        impulseResultComponentVM.LeakageCurrentMax = j.LeakageCurrentMaximum;
                        impulseResultComponentVM.LeakageCurrent = 0;
                        break;
                    case SCME.Types.OutputResidualVoltage.TestParameters j:
                        impulseResultComponentVM.ResidualVoltageMin = j.OutputResidualVoltageMinimum;
                        impulseResultComponentVM.ResidualVoltageMax = j.OutputResidualVoltageMaximum;
                        impulseResultComponentVM.ResidualVoltage = 0;
                        break;
                    case SCME.Types.ProhibitionVoltage.TestParameters j:
                        impulseResultComponentVM.ProhibitionVoltageMin = j.ProhibitionVoltageMinimum;
                        impulseResultComponentVM.ProhibitionVoltageMax = j.ProhibitionVoltageMaximum;
                        impulseResultComponentVM.ProhibitionVoltage = 0;
                        break;
                    default:
                        break;
                }
            }
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            VM.CanStart = true;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            VM.CanStart = false;
            Random random = new Random(DateTime.Now.Millisecond);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                VMPosition1.InputAmperage = random.Next(0,6);
                VMPosition2.InputVoltage = random.Next(0, 6);
                VMPosition3.LeakageCurrent = random.Next(0, 6);
                VM.CanStart = true;
            });
        }
    }
}
