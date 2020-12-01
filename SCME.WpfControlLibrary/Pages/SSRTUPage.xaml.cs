using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SCME.WpfControlLibrary.Pages
{
    public partial class SSRTUPage : Page
    {
        public SSRTUVM VM { get; set; } = new SSRTUVM();
        public BaseTestParametersAndNormatives ItemVM { get; set; }

        private Action _start;
        public Action Stop { get; set; }

        public SSRTUPage()
        {
            InitializeComponent();
        }

        public SSRTUPage(TestParametersType testParametersType, Action start, Action stop)
        {
            InitializeComponent();
            _start = start;
            Stop = stop;
            switch (testParametersType)
            {
                case TestParametersType.OutputLeakageCurrent:
                    ItemVM = new SCME.Types.OutputLeakageCurrent.TestParameters();
                    MainTabControl.SelectedIndex = 2;
                    VM.ShowOutputLeakageCurrent = true;
                    break;
                case TestParametersType.OutputResidualVoltage:
                    ItemVM = new SCME.Types.OutputResidualVoltage.TestParameters();
                    MainTabControl.SelectedIndex = 1;
                    VM.ShowOutputResidualVoltage = true;
                    break;
                case TestParametersType.InputOptions:
                    ItemVM = new SCME.Types.InputOptions.TestParameters();
                    MainTabControl.SelectedIndex = 0;
                    VM.ShowInputOptions = true;
                    break;
                case TestParametersType.ProhibitionVoltage:
                    ItemVM = new SCME.Types.ProhibitionVoltage.TestParameters();
                    MainTabControl.SelectedIndex = 3;
                    break;
                case TestParametersType.AuxiliaryPower:
                    ItemVM = new SCME.Types.AuxiliaryPower.TestParameters();
                    MainTabControl.SelectedIndex = 4;
                    break;
                default:
                    break;
            }
            ItemVM.IsProfileStyle = false;
            
        }

        public void PostSSRTUNotificationEvent(string message, ushort problem, ushort warning, ushort fault, ushort disable)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogWindow = new DialogWindow("Ошибка оборудования", $"{message}\r\nproblem {problem}, warning {warning}, fault {fault}, disable {disable}"); ;
                dialogWindow.ShowDialog();
                VM.CanStart = true;
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

        public void SSRTUHandler(DeviceState deviceState, Types.SSRTU.TestResults testResults)
        {
            VM.CanStart = true;
            VM.Result = testResults.Value;
            VM.AuxiliaryCurrentPowerSupply1 = testResults.AuxiliaryCurrentPowerSupply1;
            VM.AuxiliaryCurrentPowerSupply2 = testResults.AuxiliaryCurrentPowerSupply2;
            VM.OpenResistance = testResults.OpenResistance;

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            VM.CanStart = true;
            Stop();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            VM.CanStart = false;
            _start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
