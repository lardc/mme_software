using SCME.Types.BaseTestParams;
using System.Windows;
using System.Windows.Controls;

namespace SCME.WpfControlLibrary.Pages
{
    public partial class ImpulsePage : Page
    {

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
                    MainTabControl.SelectedIndex = 2;
                    TextBlockValueName.Text = "Ток утечки";
                    break;
                case TestParametersType.OutputResidualVoltage:
                    MainTabControl.SelectedIndex = 1;
                    TextBlockValueName.Text = "Остаточное напряжение";
                    break;
                case TestParametersType.InputOptions:
                    MainTabControl.SelectedIndex = 0;
                    TextBlockValueName.Text = "Параметры входа";
                    break;
                case TestParametersType.ProhibitionVoltage:
                    MainTabControl.SelectedIndex = 3;
                    TextBlockValueName.Text = "Напряжение запрета";
                    break;
                default:
                    break;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
