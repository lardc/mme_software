using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.dVdt;
using SCME.UIServiceConfig.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace SCME.UI.PagesTech
{
    public partial class DVdtPage : Page
    {
        //Цветовая индикация
        private readonly SolidColorBrush m_XGreen;
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

        /// <summary>Инициализирует новый экземпляр класса GatePage</summary>
        public DVdtPage()
        {
            ClampParameters = new Types.Clamping.TestParameters
            {
                StandardForce = Types.Clamping.ClampingForceInternal.Custom,
                CustomForce = 5.0f,
                IsHeightMeasureEnabled = false
            };
            Temperature = 25;
            Parameters = new TestParameters()
            {
                IsEnabled = true,
                Mode = DvdtMode.Detection
            };
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            InitializeComponent();
            m_XGreen = (SolidColorBrush)FindResource("xGreen1");
            ClearStatus();
        }

        internal bool IsRunning
        {
            get => m_IsRunning;
            set
            {
                m_IsRunning = value;
                btnBack.IsEnabled = !m_IsRunning;
            }
        }

        internal void SetResult(DeviceState state, TestResults result)
        {
            if (state == DeviceState.InProcess)
                ClearStatus();
            else
            {
                IsRunning = false;
                SetLabel(labelResult, state, result.Passed, result.Passed ? "OK" : "NotOk");
                SetLabel(labelVoltageRate, state, true, result.VoltageRate.ToString());
                PointF point;
                float pointX, pointY = Parameters.Voltage;
                if (Parameters.Mode == DvdtMode.Confirmation)
                    pointX = Parameters.Voltage / (float)Parameters.VoltageRate;
                else
                    pointX = Parameters.Voltage / (float)result.VoltageRate;
                point = new PointF(pointX, pointY);
                Plot(point, Parameters.Voltage);
            }
        }

        public void SetTopTemp(int temeprature)
        {
            TopTempLabel.Content = temeprature;
            int bottomTemp = Temperature - 2;
            int topTemp = Temperature + 2;
            if (temeprature < bottomTemp || temeprature > topTemp)
                TopTempLabel.Background = Brushes.Tomato;
            else
                TopTempLabel.Background = Brushes.LightGreen;
        }

        public void SetBottomTemp(int temeprature)
        {
            BotTempLabel.Content = temeprature;
            int bottomTemp = Temperature - 2;
            int topTemp = Temperature + 2;
            if (temeprature < bottomTemp || temeprature > topTemp)
                BotTempLabel.Background = Brushes.Tomato;
            else
                BotTempLabel.Background = Brushes.LightGreen;
        }

        internal void SetWarning(HWWarningReason warning)
        {
            lblWarning.Content = warning.ToString();
            lblWarning.Visibility = Visibility.Visible;
        }

        internal void SetFault(HWFaultReason fault)
        {
            lblFault.Content = fault.ToString();
            lblFault.Visibility = Visibility.Visible;
            IsRunning = false;
        }

        private void ClearStatus()
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
            labelResult.Content = string.Empty;
            labelVoltageRate.Content = string.Empty;
            chartPlotter.Children.RemoveAll(typeof(LineGraph));
        }

        private static void SetLabel(ContentControl target, DeviceState state, bool IsFitWithNormatives, string value)
        {
            switch (state)
            {
                case DeviceState.InProcess:
                    target.Background = Brushes.Gold;
                    break;
                case DeviceState.Stopped:
                    target.Content = Properties.Resources.Stopped;
                    target.Background = Brushes.LightGreen;
                    break;
                case DeviceState.Success:
                    target.Content = value;
                    target.Background = IsFitWithNormatives ? Brushes.LightGreen : Brushes.LightPink;
                    break;
                case DeviceState.Problem:
                    target.Content = value;
                    target.Background = Brushes.Gold;
                    break;
                case DeviceState.Fault:
                    target.Content = Properties.Resources.Fault;
                    target.Background = Brushes.Tomato;
                    break;
                case DeviceState.None:
                    target.Content = string.Empty;
                    target.Background = Brushes.Transparent;
                    break;
            }
        }

        private void Plot(PointF point, ushort voltage)
        {
            List<PointF> Points = new List<PointF>();
            Points.Add(new PointF(0, 0));
            Points.Add(point);
            Points.Add(new PointF(200, voltage));
            EnumerableDataSource<PointF> DataSource = new EnumerableDataSource<PointF>(Points);
            DataSource.SetXMapping(P => P.X);
            DataSource.SetYMapping(P => P.Y);
            chartPlotter.AddLineGraph(DataSource, m_XGreen.Color, 3, @"dVdt");
            chartPlotter.FitToView();
        }

        private void btnStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
                return;
            ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;
            Types.Commutation.TestParameters commPar = new Types.Commutation.TestParameters()
            {
                BlockIndex = (!Cache.Clamp.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                CommutationType = ConverterUtil.MapCommutationType(CommType),
                Position = ConverterUtil.MapModulePosition(ModPosition)
            };
            List<BaseTestParametersAndNormatives> parameters = new List<BaseTestParametersAndNormatives>(1);
            parameters.Add(Parameters);
            if (!Cache.Net.Start(commPar, ClampParameters, parameters))
                return;
            IsRunning = true;
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
    }
}
