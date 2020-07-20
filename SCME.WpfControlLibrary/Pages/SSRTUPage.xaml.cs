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
        private Action _stop;

        public SSRTUPage()
        {
            InitializeComponent();
        }

        public SSRTUPage(TestParametersType testParametersType, Action start, Action stop)
        {
            InitializeComponent();
            _start = start;
            _stop = stop;
            switch (testParametersType)
            {
                case TestParametersType.OutputLeakageCurrent:
                    ItemVM = new SCME.Types.OutputLeakageCurrent.TestParameters();
                    MainTabControl.SelectedIndex = 2;
                    TextBlockValueName.Text = "Ток утечки";
                    break;
                case TestParametersType.OutputResidualVoltage:
                    ItemVM = new SCME.Types.OutputResidualVoltage.TestParameters();
                    MainTabControl.SelectedIndex = 1;
                    TextBlockValueName.Text = "Остаточное напряжение";
                    break;
                case TestParametersType.InputOptions:
                    ItemVM = new SCME.Types.InputOptions.TestParameters();
                    MainTabControl.SelectedIndex = 0;
                    TextBlockValueName.Text = "Параметры входа";
                    break;
                case TestParametersType.ProhibitionVoltage:
                    ItemVM = new SCME.Types.ProhibitionVoltage.TestParameters();
                    MainTabControl.SelectedIndex = 3;
                    TextBlockValueName.Text = "Напряжение запрета";
                    break;
                default:
                    break;
            }

            ItemVM.HideMinMax = true;
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

        public void SSRTUHandler(DeviceState deviceState, Types.SSRTU.TestResults testResults)
        {
            VM.CanStart = true;
            VM.Result = testResults.Value;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            VM.CanStart = true;
            _stop();
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
