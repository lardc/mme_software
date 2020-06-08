using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.SCTU;
using SCME.UI.Properties;

namespace SCME.UI.PagesUser
{
    /// <summary>
    /// Interaction logic for SCTU.xaml
    /// </summary>
    public partial class SCTU : Page
    {
        public Types.Commutation.ModuleCommutationType CommType { get; set; }
        public Types.Commutation.ModulePosition ModPosition { get; set; }

        public SctuTestParameters Parameters { get; set; }

        public SCTU()
        {
            Parameters = new SctuTestParameters()
            {
                Type = SctuDutType.Diod,
                Value = 100,
                ShuntResistance = ushort.Parse(UserSettings.Default.ShuntResistance)
            };
            InitializeComponent();
            ClearStatus();
        }

        private void ClearStatus()
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
            labelResultVoltage.Content = "";
            labelResultCurrent.Content = "";
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            UserSettings.Default.ShuntResistance = TextBoxResistance.Text;
            UserSettings.Default.Save();
            btnStart.IsEnabled = false;
            
            ClearStatus();
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            var commPar = new Types.Commutation.TestParameters()
            {
                BlockIndex = (!Cache.Clamp.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                CommutationType = ConverterUtil.MapCommutationType(CommType),
                Position = ConverterUtil.MapModulePosition(ModPosition)
            };
            var clamp = new Types.Clamping.TestParameters()
            {
                SkipClamping = true
            };
            var parameters = new List<BaseTestParametersAndNormatives>(1);
            parameters.Add(Parameters);

            Cache.Net.Start(commPar, clamp, parameters);
        }

        public void SetResults(SctuHwState state, SctuTestResults results)
        {
            switch (state)
            {
                case SctuHwState.PulseEnd:
                    labelResultVoltage.Content = results.VoltageValue;//((double)results.VoltageValue / 1000).ToString("0.00");
                    labelResultCurrent.Content = results.CurrentValue;
                    break;
                case SctuHwState.Ready:
                    btnStart.IsEnabled = true;
                    break;
            }
        }
    }
}
