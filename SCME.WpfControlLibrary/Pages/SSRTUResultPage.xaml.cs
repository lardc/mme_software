using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.DataTemplates.TestParameters;
using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    /// Логика взаимодействия для SSRTUResultPage.xaml
    /// </summary>
    public partial class SSRTUResultPage : Page
    {


        private Action _start;
        public Action Stop { get; set; }
        private Profile _profile;

        public SSRTUResultVM VM { get; set; } = new SSRTUResultVM();
        public SSRTUResultComponentVM VMPosition1 { get; set; } = new SSRTUResultComponentVM() { Postition = 1};
        public SSRTUResultComponentVM VMPosition2 { get; set; } = new SSRTUResultComponentVM() { Postition = 2 };
        public SSRTUResultComponentVM VMPosition3 { get; set; } = new SSRTUResultComponentVM() { Postition = 3 };

        public Dictionary<int, SSRTUResultComponentVM> VMByPosition{ get; set; }
        public SSRTUResultPage()
        {
            InitializeComponent();
        }

        public SSRTUResultPage(Profile profile, Action start, Action stop)
        {
            InitializeComponent();
            _start = start;
            _profile = profile;
            Stop = stop;
            VMByPosition = new Dictionary<int, SSRTUResultComponentVM>();
            VMByPosition[1] = VMPosition1;
            VMByPosition[2] = VMPosition2;
            VMByPosition[3] = VMPosition3;
            foreach (var i in VMByPosition)
                i.Value.DutPackageType = profile.DutPackageType;

            foreach (var i in profile.TestParametersAndNormatives)
            {
                var sSRTUResultComponentVM = VMByPosition[i.NumberPosition];
                switch (i)
                {
                    case SCME.Types.InputOptions.TestParameters j:
                        if (j.TypeManagement == Types.TypeManagement.DCAmperage)
                        {
                            sSRTUResultComponentVM.InputVoltageMin = j.InputVoltageMinimum;
                            sSRTUResultComponentVM.InputVoltageMax = j.InputVoltageMaximum;
                            sSRTUResultComponentVM.InputVoltage = 0;
                            foreach (var t in VMByPosition.Values)
                                t.ShowInputAmperage = true;
                        }
                        else
                        {
                            sSRTUResultComponentVM.InputAmperageMin = j.InputCurrentMinimum;
                            sSRTUResultComponentVM.InputAmperageMax = j.InputCurrentMaximum;
                            sSRTUResultComponentVM.InputAmperage = 0;
                            foreach (var t in VMByPosition.Values)
                                t.ShowInputAmperage = false;
                        }
                        if(j.ShowAuxiliaryVoltagePowerSupply1)
                        {
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMin1 = j.AuxiliaryCurrentPowerSupplyMinimum1;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMax1 = j.AuxiliaryCurrentPowerSupplyMaximum1;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupply1 = 0;
                        }
                        if (j.ShowAuxiliaryVoltagePowerSupply2)
                        {
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMin2 = j.AuxiliaryCurrentPowerSupplyMinimum2;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMax2 = j.AuxiliaryCurrentPowerSupplyMaximum2;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupply2 = 0;
                        }
                        break;
                    case SCME.Types.OutputLeakageCurrent.TestParameters j:
                        sSRTUResultComponentVM.LeakageCurrentMin = j.LeakageCurrentMinimum;
                        sSRTUResultComponentVM.LeakageCurrentMax = j.LeakageCurrentMaximum;
                        sSRTUResultComponentVM.LeakageCurrent = 0;
                        break;
                    case SCME.Types.OutputResidualVoltage.TestParameters j:
                        sSRTUResultComponentVM.ResidualVoltageMin = j.OutputResidualVoltageMinimum;
                        sSRTUResultComponentVM.ResidualVoltageMax = j.OutputResidualVoltageMaximum;
                        sSRTUResultComponentVM.ResidualVoltage = 0;
                        if (j.OpenState)
                        {
                            sSRTUResultComponentVM.OpenResistanceMin = j.OpenResistanceMinimum;
                            sSRTUResultComponentVM.OpenResistanceMax = j.OpenResistanceMaximum;
                            sSRTUResultComponentVM.OpenResistance = 0;
                        }
                        break;
                    case SCME.Types.ProhibitionVoltage.TestParameters j:
                        sSRTUResultComponentVM.ProhibitionVoltageMin = j.ProhibitionVoltageMinimum;
                        sSRTUResultComponentVM.ProhibitionVoltageMax = j.ProhibitionVoltageMaximum;
                        sSRTUResultComponentVM.ProhibitionVoltage = 0;
                        break;
                    default:
                        break;
                }
            }
        }

        public void PostSSRTUNotificationEvent(ushort problem, ushort warning, ushort fault, ushort disable)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogWindow = new DialogWindow("Ошибка оборудования", $"problem {problem} warning {warning}, fault {fault}, disable {disable}"); ;
                dialogWindow.ShowDialog();
            }));
        }
    

        public void PostAlarmEvent()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogWindow = new DialogWindow("Внимание", "Нарушен периметр безопасности");
                dialogWindow.ShowDialog();
            }));
        }

        private int countEndingTests;
        public void SSRTUHandler(DeviceState deviceState, Types.SSRTU.TestResults testResults)
        {
            var q = VMByPosition[testResults.NumberPosition];

            switch (testResults.TestParametersType)
            {
                case TestParametersType.InputOptions:
                    if (testResults.InputOptionsIsAmperage)
                        q.InputAmperage = testResults.Value;
                    else
                        q.InputVoltage = testResults.Value;
                    if(q.ShowAuxiliaryCurrentPowerSupply1)
                        q.AuxiliaryCurrentPowerSupply1 = testResults.AuxiliaryCurrentPowerSupply1;
                    if(q.ShowAuxiliaryCurrentPowerSupply2)
                        q.AuxiliaryCurrentPowerSupply2 = testResults.AuxiliaryCurrentPowerSupply2;
                    break;
                case TestParametersType.OutputLeakageCurrent:
                    q.LeakageCurrent = testResults.Value;
                    break;
                case TestParametersType.OutputResidualVoltage:
                    q.ResidualVoltage = testResults.Value;
                    q.OpenResistance = testResults.OpenResistance;
                    break;
                case TestParametersType.ProhibitionVoltage:
                    q.ProhibitionVoltage = testResults.Value;
                    break;
                default:
                    break;
            }
            if (++countEndingTests == _profile.TestParametersAndNormatives.Count)
                VM.CanStart = true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            VM.CanStart = true;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            VM.CanStart = false;
            countEndingTests = 0;
            _start();
            return;
            VM.CanStart = false;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                
                VM.CanStart = true;
            });
            return;
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
