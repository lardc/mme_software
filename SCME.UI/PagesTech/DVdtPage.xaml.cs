using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SCME.Types;
using SCME.Types.BaseTestParams;
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
        private readonly SolidColorBrush ColorGreen;
        //Состояние тестирования
        private bool isRunning;

        /// <summary>Инициализирует новый экземпляр класса GatePage</summary>
        public DVdtPage()
        {
            //Установка базовых параметров dUdt и пресса
            Parameters = new Types.dVdt.TestParameters()
            {
                Mode = Types.dVdt.DvdtMode.Detection,
                IsEnabled = true
            };
            ClampParameters = new Types.Clamping.TestParameters
            {
                StandardForce = Types.Clamping.ClampingForceInternal.Custom,
                CustomForce = 5,
                IsHeightMeasureEnabled = false
            };
            Temperature = 25;
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            InitializeComponent();
            ColorGreen = (SolidColorBrush)FindResource("xGreen1");
            //Предварительная очистка всех статусов
            Status_Clear();
        }

        /// <summary>Параметры dUdt</summary>
        public Types.dVdt.TestParameters Parameters
        {
            get; set;
        }

        /// <summary>Параметры пресса</summary>
        public Types.Clamping.TestParameters ClampParameters
        {
            get; set;
        }

        /// <summary>Тип коммутации</summary>
        public Types.Commutation.ModuleCommutationType CommType
        {
            get; set;
        }

        /// <summary>Позиция</summary>
        public Types.Commutation.ModulePosition ModPosition
        {
            get; set;
        }

        /// <summary>Температура</summary>
        public int Temperature
        {
            get; set;
        }

        /// <summary>Состояние тестирования</summary>
        internal bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                btnStart.IsEnabled = !isRunning;
                btnBack.IsEnabled = !isRunning;
            }
        }

        /// <summary>Установка результатов</summary>
        /// <param name="state">Состояние</param>
        internal void SetResult(DeviceState state, Types.dVdt.TestResults result)
        {
            if (state == DeviceState.InProcess)
                Status_Clear();
            else
            {
                IsRunning = false;
                Label_Set(labelResult, state, result.Passed, result.Passed ? "OK" : "NotOk");
                Label_Set(labelVoltageRate, state, true, result.VoltageRate.ToString());
                PointF Point;
                float PointX, PointY = Parameters.Voltage;
                if (Parameters.Mode == Types.dVdt.DvdtMode.Confirmation)
                    PointX = Parameters.Voltage / (float)Parameters.VoltageRate;
                else
                    PointX = Parameters.Voltage / (float)result.VoltageRate;
                Point = new PointF(PointX, PointY);
                Chart_Plot(Point, Parameters.Voltage);
            }
        }

        /// <summary>Установка ошибок</summary>
        /// <param name="fault">Ошибка</param>
        internal void SetFault(Types.dVdt.HWFaultReason fault)
        {
            lblFault.Content = fault.ToString();
            lblFault.Visibility = Visibility.Visible;
            IsRunning = false;
        }

        /// <summary>Установка предупреждений</summary>
        /// <param name="warning">Предупреждение</param>
        internal void SetWarning(Types.dVdt.HWWarningReason warning)
        {
            lblWarning.Content = warning.ToString();
            lblWarning.Visibility = Visibility.Visible;
        }

        private void Status_Clear() //Очистка всех статусов
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
            labelResult.Content = string.Empty;
            labelVoltageRate.Content = string.Empty;
            chartPlotter.Children.RemoveAll(typeof(LineGraph));
        }

        private static void Label_Set(ContentControl target, DeviceState state, bool IsFitWithNormatives, string value) //Установка значения
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

        private void Chart_Plot(PointF point, ushort voltage) //Отрисовка графика
        {
            List<PointF> Points = new List<PointF>();
            Points.Add(new PointF(0, 0));
            Points.Add(point);
            Points.Add(new PointF(50, voltage));
            EnumerableDataSource<PointF> DataSource = new EnumerableDataSource<PointF>(Points);
            DataSource.SetXMapping(P => P.X);
            DataSource.SetYMapping(P => P.Y);
            chartPlotter.AddLineGraph(DataSource, ColorGreen.Color, 3, @"dUdt");
            chartPlotter.FitToView();
        }

        private void btnStart_OnClick(object sender, RoutedEventArgs e) //Запуск тестирования
        {
            Start();
        }

        /// <summary>Запуск тестирования</summary>
        internal void Start()
        {
            if (IsRunning)
                return;
            Types.Commutation.TestParameters ParamCommutation = new Types.Commutation.TestParameters()
            {
                BlockIndex = !Cache.Clamp.UseTmax ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                CommutationType = ConverterUtil.MapCommutationType(CommType),
                Position = ConverterUtil.MapModulePosition(ModPosition)
            };
            ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;
            List<BaseTestParametersAndNormatives> ParametersAndNormatives = new List<BaseTestParametersAndNormatives>(1)
            {
                Parameters
            };
            if (!Cache.Net.Start(ParamCommutation, ClampParameters, ParametersAndNormatives))
                return;
            IsRunning = true;
        }

        private void btnStop_OnClick(object sender, RoutedEventArgs e) //Остановка тестирования
        {
            Cache.Net.StopByButtonStop();
        }

        private void btnBack_OnClick(object sender, RoutedEventArgs e) //Переход на предыдущую страницу
        {
            if (NavigationService != null)
                NavigationService.GoBack();
        }

        private void btnTemperature_OnClick(object sender, RoutedEventArgs e)  //Нагрев
        {
            Cache.Net.StartHeating(Temperature);
        }

        /// <summary>Установка температуры верхней пластины</summary>
        /// <param name="temeprature">Температура</param>
        public void SetTopTemp(int temeprature)
        {
            TopTempLabel.Content = temeprature;
            int BottomTemp = Temperature - 2;
            int TopTemp = Temperature + 2;
            if (temeprature < BottomTemp || temeprature > TopTemp)
                TopTempLabel.Background = Brushes.Tomato;
            else
                TopTempLabel.Background = Brushes.LightGreen;
        }

        /// <summary>Установка температуры нижней пластины</summary>
        /// <param name="temeprature">Температура</param>
        public void SetBottomTemp(int temeprature)
        {
            BotTempLabel.Content = temeprature;
            int BottomTemp = Temperature - 2;
            int TopTemp = Temperature + 2;
            if (temeprature < BottomTemp || temeprature > TopTemp)
                BotTempLabel.Background = Brushes.Tomato;
            else
                BotTempLabel.Background = Brushes.LightGreen;
        }
    }
}
