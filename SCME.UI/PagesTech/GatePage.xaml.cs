using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SCME.Types;
using SCME.UIServiceConfig.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace SCME.UI.PagesTech
{
    public partial class GatePage
    {
        //Цветовая индикация
        private readonly SolidColorBrush m_XRed, m_XGreen, m_XOrange;
        //Состояние тестирования
        private bool m_IsRunning;

        /// <summary>Инициализирует новый экземпляр класса GatePage</summary>
        internal GatePage()
        {
            Parameters = new Types.Gate.TestParameters
            {
                IsEnabled = true
            };
            ClampParameters = new Types.Clamping.TestParameters
            {
                StandardForce = Types.Clamping.ClampingForceInternal.Custom,
                CustomForce = 5.0f,
                IsHeightMeasureEnabled = false
            };
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            Temperature = 25;
            InitializeComponent();
            m_XRed = (SolidColorBrush)FindResource("xRed1");
            m_XGreen = (SolidColorBrush)FindResource("xGreen1");
            m_XOrange = (SolidColorBrush)FindResource("xOrange1");
            ClearStatus();
        }

        public int Temperature
        {
            get; set;
        }

        public Types.Gate.TestParameters Parameters
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

        internal void SetResultAll(DeviceState state)
        {
            if (state == DeviceState.InProcess)
                ClearStatus();
            else
                IsRunning = false;
        }

        internal void SetResultKelvin(DeviceState state, bool isKelvinOk)
        {
            SetLabel(lblKelvin, state, isKelvinOk ? Properties.Resources.Ok : Properties.Resources.Fault);
        }

        internal void SetResultResistance(DeviceState state, float resistance)
        {
            SetLabel(lblResistance, state, resistance.ToString());
        }

        internal void SetResultGT(DeviceState state, float igt, float vgt, IList<short> arrayI, IList<short> arrayV)
        {
            SetLabel(lblIGT, state, igt.ToString());
            SetLabel(lblVGT, state, vgt.ToString());
            if (state == DeviceState.Success)
            {
                Plot(@"Igt", m_XRed.Color, arrayI);
                Plot(@"Vgt", m_XOrange.Color, arrayV);
            }
        }

        private void Pulse_Gate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Types.Gate.CalibrationResultGate Result = Cache.Net.GatePulseCalibrationGate(ushort.Parse(calibrationCurrent.Text));
                actualCurrent.Content = Result.Current.ToString(CultureInfo.InvariantCulture);
                actualVoltage.Content = Result.Voltage.ToString(CultureInfo.InvariantCulture);
            }
            catch { }
        }

        private void Pulse_Main_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ushort Result = Cache.Net.GatePulseCalibrationMain(ushort.Parse(calibrationCurrent.Text));
                actualCurrent.Content = Result.ToString(CultureInfo.InvariantCulture);
                actualVoltage.Content = "0";
            }
            catch { }
        }
        
        internal void SetResultIh(DeviceState state, float ih, IList<short> array)
        {
            SetLabel(lblIH, state, ih.ToString());
            if (state == DeviceState.Success)
                Plot(@"Ih", m_XGreen.Color, array);
        }

        internal void SetResultIl(DeviceState state, float il)
        {
            SetLabel(lblIL, state, il.ToString());
        }

        internal void SetResultVgnt(DeviceState state, float vgnt, ushort ignt)
        {
            SetLabel(lblVgnt, state, vgnt.ToString());
            SetLabel(lblIgnt, state, ignt.ToString());
        }

        internal void SetWarning(Types.Gate.HWWarningReason warning)
        {
            if (lblWarning.Visibility != Visibility.Visible)
            {

                lblWarning.Content = warning.ToString();
                lblWarning.Visibility = Visibility.Visible;
            }
        }

        internal void SetProblem(Types.Gate.HWProblemReason problem)
        {
            lblWarning.Content = problem.ToString();
            lblWarning.Visibility = Visibility.Visible;
        }

        internal void SetFault(Types.Gate.HWFaultReason fault)
        {
            lblFault.Content = fault.ToString();
            lblFault.Visibility = Visibility.Visible;
            IsRunning = false;
        }

        private void ClearStatus()
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
            ResetLabel(lblKelvin);
            ResetLabel(lblResistance);
            ResetLabel(lblIGT);
            ResetLabel(lblVGT);
            ResetLabel(lblIH);
            ResetLabel(lblIL);

            chartPlotter.Children.RemoveAll(typeof(LineGraph));
        }

        private static void SetLabel(ContentControl target, DeviceState state, string message)
        {
            target.Content = string.Empty;
            switch (state)
            {
                case DeviceState.Success:
                    target.Background = Brushes.LightGreen;
                    target.Content = message;
                    break;
                case DeviceState.Problem:
                    target.Background = Brushes.Gold;
                    target.Content = message;
                    break;
                case DeviceState.InProcess:
                    target.Background = Brushes.Gold;
                    break;
                case DeviceState.Stopped:
                case DeviceState.Fault:
                    target.Background = Brushes.Tomato;
                    break;
                default:
                    target.Background = Brushes.Transparent;
                    break;
            }
        }

        private void ResetLabel(ContentControl target)
        {
            target.Content = string.Empty;
            target.Background = Brushes.Transparent;
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

        internal void Start()
        {
            if (IsRunning)
                return;
            Types.VTM.TestParameters paramVtm = new Types.VTM.TestParameters
            {
                IsEnabled = false
            };
            Types.BVT.TestParameters paramBvt = new Types.BVT.TestParameters
            {
                IsEnabled = false
            };
            Types.ATU.TestParameters paramATU = new Types.ATU.TestParameters
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
            if (!Cache.Net.Start(Parameters, paramVtm, paramBvt, paramATU, paramQrrTq, paramIH, paramRCC, new Types.Commutation.TestParameters
                                 {
                                     BlockIndex = (!Cache.Clamp.clampPage.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                                     CommutationType = ConverterUtil.MapCommutationType(CommType),
                                     Position = ConverterUtil.MapModulePosition(ModPosition)
                                 }, ClampParameters, paramTOU))
                return;
            ClearStatus();
            IsRunning = true;
        }

        private void btnStart_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollViewer.ScrollToBottom();
            Start();
        }

        private void btnStop_OnClick(object sender, RoutedEventArgs e)
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

        public void SetTopTemp(int temeprature)
        {
            TopTempLabel.Content = temeprature;
            int bottomTemp = Temperature - 2;
            int topTemp = Temperature + 2;
            TopTempLabel.Background = temeprature < bottomTemp || temeprature > topTemp ? Brushes.Tomato : Brushes.LightGreen;
        }

        public void SetBottomTemp(int temeprature)
        {
            BotTempLabel.Content = temeprature;
            var bottomTemp = Temperature - 2;
            var topTemp = Temperature + 2;
            if (temeprature < bottomTemp || temeprature > topTemp)
            {
                BotTempLabel.Background = Brushes.Tomato;
            }
            else
            {
                BotTempLabel.Background = Brushes.LightGreen;
            }
        }
    }
}