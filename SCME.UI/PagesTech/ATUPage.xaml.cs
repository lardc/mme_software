using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SCME.Types;
using SCME.Types.ATU;
using SCME.UIServiceConfig.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;


namespace SCME.UI.PagesTech
{
    public partial class ATUPage : Page
    {
        //Цветовая индикация
        private readonly SolidColorBrush m_XGreen, m_XBlue;
        //Состояние тестирования
        private bool m_IsRunning;

        public int Temperature
        {
            get; set;
        }

        public TestParameters Parameters
        {
            get; set;
        }

        public Types.Clamping.TestParameters ClampParameters
        {
            get; set;
        }

        public Types.Commutation.ModuleCommutationType CommType
        {
            get; set;
        }

        public Types.Commutation.ModulePosition ModPosition
        {
            get; set;
        }

        /// <summary>Инициализирует новый экземпляр класса ATUPage</summary>
        internal ATUPage()
        {
            Parameters = new TestParameters
            {
                IsEnabled = true
            };
            ClampParameters = new Types.Clamping.TestParameters
            {
                StandardForce = Types.Clamping.ClampingForceInternal.Custom,
                CustomForce = 5
            };
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            Temperature = 25;
            InitializeComponent();
            m_XGreen = (SolidColorBrush)FindResource("xGreen1");
            m_XBlue = (SolidColorBrush)FindResource("xBlue1");
            ClearStatus();
        }

        internal bool IsRunning
        {
            get => m_IsRunning;
            set
            {
                m_IsRunning = value;
                btnStart.IsEnabled = !m_IsRunning;
                btnBack.IsEnabled = !m_IsRunning;
            }
        }

        internal void Start()
        {
            if (IsRunning)
                return;
            Types.GTU.TestParameters paramGate = new Types.GTU.TestParameters
            {
                IsEnabled = false
            };
            Types.VTM.TestParameters paramVtm = new Types.VTM.TestParameters
            {
                IsEnabled = false
            };
            Types.BVT.TestParameters paramBvt = new Types.BVT.TestParameters
            {
                IsEnabled = false
            };
            Types.QrrTq.TestParameters paramQrrTq = new Types.QrrTq.TestParameters
            {
                IsEnabled = false
            };
            Types.IH.TestParameters paramIH = new Types.IH.TestParameters
            {
                IsEnabled = false
            };
            Types.RCC.TestParameters paramRCC = new Types.RCC.TestParameters
            {
                IsEnabled = false
            };
            Types.TOU.TestParameters paramTOU = new Types.TOU.TestParameters
            {
                IsEnabled = false
            };
            ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;
            if (!Cache.Net.Start(paramGate, paramVtm, paramBvt, Parameters, paramQrrTq, paramIH, paramRCC,
                                 new Types.Commutation.TestParameters
                                 {
                                     BlockIndex = (!Cache.Clamp.clampPage.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                                     CommutationType = ConverterUtil.MapCommutationType(CommType),
                                     Position = ConverterUtil.MapModulePosition(ModPosition)
                                 }, ClampParameters, paramTOU))
                return;

            ClearStatus();
            IsRunning = true;
        }

        internal void SetResultAll(Types.DeviceState state)
        {
            if (state == Types.DeviceState.InProcess)
                ClearStatus();
            else
                IsRunning = false;
        }

        internal void SetResult(Types.DeviceState state, TestResults result)
        {
            if (state == Types.DeviceState.InProcess)
                ClearStatus();
            else
            {
                IsRunning = false;
                SetLabel(lbAtuUBR, state, true, result.UBR.ToString());
                SetLabel(lbAtuUPRSM, state, true, result.UPRSM.ToString());
                SetLabel(lbAtuIPRSM, state, true, string.Format("{0:0.00}", result.IPRSM));
                SetLabel(lbAtuPRSM, state, true, string.Format("{0:0.00}", result.PRSM));

                Plot(@"U", m_XGreen.Color, result.ArrayVDUT);
                Plot(@"I", m_XBlue.Color, result.ArrayIDUT);
            }
        }

        private void Plot(string lineName, Color lineColor, IEnumerable<short> uPoints)
        {
            List<PointF> Points = uPoints.Select((T, I) => new PointF(I, T)).ToList();
            EnumerableDataSource<PointF> DataSource = new EnumerableDataSource<PointF>(Points);
            DataSource.SetXMapping(P => P.X);
            DataSource.SetYMapping(P => P.Y);
            chartPlotter.AddLineGraph(DataSource, lineColor, 3, lineName);
            chartPlotter.FitToView();
        }

        public void SetTopTemp(int temperature)
        {
            TopTempLabel.Content = temperature;
            int bottomTemp = Temperature - 2;
            int topTemp = Temperature + 2;
            if (temperature < bottomTemp || temperature > topTemp)
                TopTempLabel.Background = Brushes.Tomato;
            else
                TopTempLabel.Background = Brushes.LightGreen;
        }

        public void SetBottomTemp(int temperature)
        {
            BotTempLabel.Content = temperature;
            int bottomTemp = Temperature - 2;
            int topTemp = Temperature + 2;
            if (temperature < bottomTemp || temperature > topTemp)
                BotTempLabel.Background = Brushes.Tomato;
            else
                BotTempLabel.Background = Brushes.LightGreen;
        }

        private void ResetLabel(ContentControl target)
        {
            target.Content = string.Empty;
            target.Background = Brushes.Transparent;
        }

        internal void ClearMeasureResult()
        {
            ResetLabel(lbAtuUBR);
            ResetLabel(lbAtuUPRSM);
            ResetLabel(lbAtuIPRSM);
            ResetLabel(lbAtuPRSM);
        }

        internal void SetColorByWarning(ushort warning)
        {
            switch (warning)
            {
                case (ushort)HWWarningReason.Idle:
                case (ushort)HWWarningReason.FacetBreak:
                    lbAtuWarning.Background = Brushes.Orange;
                    break;
                case (ushort)HWWarningReason.Break:
                case (ushort)HWWarningReason.Short:               
                    lbAtuWarning.Background = (SolidColorBrush)FindResource("xRed1");
                    break;
                default:
                    lbAtuWarning.Background = Brushes.Gold;
                    break;
            }
        }

        internal void SetWarning(ushort warning)
        {
            SetColorByWarning(warning);
            HWWarningReason WarningReason = (HWWarningReason)warning;
            lbAtuWarning.Content = WarningReason.ToString();
            lbAtuWarning.Visibility = Visibility.Visible;
        }

        internal void SetFault(ushort fault)
        {
            HWFaultReason FaultReason = (Types.ATU.HWFaultReason)fault;
            lbAtuFaultReason.Content = FaultReason.ToString();
            lbAtuFaultReason.Visibility = Visibility.Visible;
            IsRunning = false;
        }

        private void ClearStatus()
        {
            lbAtuWarning.Visibility = Visibility.Collapsed;
            lbAtuFaultReason.Visibility = Visibility.Collapsed;
            ClearMeasureResult();
            chartPlotter.Children.RemoveAll(typeof(LineGraph));
        }

        private static void SetLabel(ContentControl target, Types.DeviceState state, bool isFitWithNormatives, string value)
        {
            switch (state)
            {
                case DeviceState.None:
                    target.Content = "";
                    target.Background = Brushes.Transparent;
                    break;
                case DeviceState.InProcess:
                    target.Background = Brushes.Gold;
                    break;
                case DeviceState.Success:
                    target.Content = value;
                    target.Background = isFitWithNormatives ? Brushes.LightGreen : Brushes.LightPink;
                    break;
                case DeviceState.Stopped:
                    target.Content = Properties.Resources.Stopped;
                    target.Background = Brushes.LightGreen;
                    break;
                case DeviceState.Problem:
                    target.Content = value;
                    target.Background = Brushes.Gold;
                    break;
                case DeviceState.Fault:
                    target.Content = Properties.Resources.Fault;
                    target.Background = Brushes.Tomato;
                    break;
            }
        }

        private void btnStart_OnClick(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void btnStop_OnClick(object Sender, RoutedEventArgs E)
        {
            Cache.Net.StopByButtonStop();
        }

        private void btnBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
                NavigationService.GoBack();
        }

        private void btnTemperature_OnClick(object sender, RoutedEventArgs e)
        {
            Cache.Net.StartHeating(Temperature);
        }
    }
}
