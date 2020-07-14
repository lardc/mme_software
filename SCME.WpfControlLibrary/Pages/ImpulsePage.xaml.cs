using SCME.Types.BaseTestParams;
using System.Windows;
using System.Windows.Controls;

namespace SCME.WpfControlLibrary.Pages
{
    public partial class ImpulsePage : Page
    {
        public BaseTestParametersAndNormatives ItemVM { get; set; }

        public ImpulsePage()
        {
            InitializeComponent();
        }

        public ImpulsePage(TestParametersType testParametersType)
        {
            InitializeComponent();
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
