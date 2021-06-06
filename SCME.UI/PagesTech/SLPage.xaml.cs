using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SCME.Types;
using SCME.UIServiceConfig.Properties;
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
    public partial class SLPage
    {
        //Цветовая индикация
        private readonly SolidColorBrush ColorGreen, ColorOrange;
        //Состояние тестирования
        private bool isRunning;

        /// <summary>Инициализирует новый экземпляр класса SLPage</summary>
        internal SLPage()
        {
            //Установка базовых параметров SL и пресса
            Parameters = new Types.VTM.TestParameters
            {
                UseLsqMethod = Settings.Default.UseVTMPostProcessing,
                IsEnabled = true
            };
            ClampParameters = new Types.Clamping.TestParameters
            {
                StandardForce = Types.Clamping.ClampingForceInternal.Custom,
                CustomForce = 5,
                IsHeightMeasureEnabled = false
            };
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            Temperature = 25;
            InitializeComponent();
            ColorGreen = (SolidColorBrush)FindResource("xGreen1");
            ColorOrange = (SolidColorBrush)FindResource("xOrange1");
            //Предварительная очистка всех статусов
            Status_Clear();
        }

        /// <summary>Параметры SL</summary>
        public Types.VTM.TestParameters Parameters
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

        /// <summary>Установка всех результатов</summary>
        /// <param name="state">Состояние</param>
        internal void SetResultAll(DeviceState state)
        {
            if (state == DeviceState.InProcess)
                Status_Clear();
            else
                IsRunning = false;
        }

        /// <summary>Установка результата теста SL</summary>
        /// <param name="state">Состояние</param>
        /// <param name="result">Результат теста</param>
        internal void SetResultVtm(DeviceState state, Types.VTM.TestResults result)
        {
            Label_Set(lblVtm, state, string.Format("{0}", result.Voltage));
            lblItm.Content = string.Format("{0}", result.Current);
            if (state != DeviceState.InProcess)
            {
                IsRunning = false;
                if (state == DeviceState.Success)
                {
                    Chart_Plot(@"Itm", ColorGreen.Color, result.ITMArray);
                    Chart_Plot(@"Utm", ColorOrange.Color, result.VTMArray);
                }
            }
            else
                Status_Clear();
        }

        /// <summary>Установка ошибок</summary>
        /// <param name="fault">Ошибка</param>
        internal void SetFault(Types.VTM.HWFaultReason fault)
        {
            labelFault.Content = fault.ToString();
            labelFault.Visibility = Visibility.Visible;
            IsRunning = false;
        }

        /// <summary>Установка предупреждений</summary>
        /// <param name="warning">Предупреждение</param>
        internal void SetWarning(Types.VTM.HWWarningReason warning)
        {
            labelWarning.Content = warning.ToString();
            labelWarning.Visibility = Visibility.Visible;
        }

        /// <summary>Установка проблем</summary>
        /// <param name="problem">Проблема</param>
        internal void SetProblem(Types.VTM.HWProblemReason problem)
        {
            labelWarning.Content = problem.ToString();
            labelWarning.Visibility = Visibility.Visible;
        }

        private void Status_Clear() //Очистка всех статусов
        {
            labelWarning.Visibility = Visibility.Collapsed;
            labelFault.Visibility = Visibility.Collapsed;
            Label_Reset(lblVtm);
            Label_Reset(lblItm);
            chartPlotter.Children.RemoveAll(typeof(LineGraph));
        }

        private static void Label_Set(ContentControl target, DeviceState state, string message) //Установка значения
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

        private static void Label_Reset(ContentControl target) //Сброс значения
        {
            target.Content = string.Empty;
            target.Background = Brushes.Transparent;
        }

        private void Chart_Plot(string lineName, Color lineColor, IEnumerable<short> uPoints) //Отрисовка графика
        {
            List<PointF> Points = uPoints.Select((T, I) => new PointF(I, T)).ToList();
            EnumerableDataSource<PointF> DataSource = new EnumerableDataSource<PointF>(Points);
            DataSource.SetXMapping(P => P.X * 50);
            DataSource.SetYMapping(P => P.Y);
            chartPlotter.AddLineGraph(DataSource, lineColor, 3, lineName);
            chartPlotter.FitToView();
        }

        private void btnStart_OnClick(object sender, RoutedEventArgs e) //Запуск тестирования
        {
            ScrollViewer.ScrollToBottom();
            Start();
        }

        /// <summary>Запуск тестирования</summary>
        internal void Start()
        {
            if (IsRunning)
                return;
            Types.Commutation.TestParameters ParamCommutation = new Types.Commutation.TestParameters
            {
                BlockIndex = !Cache.Clamp.clampPage.UseTmax ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                CommutationType = ConverterUtil.MapCommutationType(CommType),
                Position = ConverterUtil.MapModulePosition(ModPosition)
            };
            Types.Gate.TestParameters ParamGTU = new Types.Gate.TestParameters
            {
                IsEnabled = false
            };
            Types.BVT.TestParameters ParamBVT = new Types.BVT.TestParameters
            {
                IsEnabled = false
            };
            Types.ATU.TestParameters ParamATU = new Types.ATU.TestParameters
            {
                IsEnabled = false
            };
            Types.QrrTq.TestParameters ParamQrrTq = new Types.QrrTq.TestParameters
            {
                IsEnabled = false
            };
            Types.IH.TestParameters ParamIH = new Types.IH.TestParameters
            {
                IsEnabled = false
            };
            Types.RCC.TestParameters ParamRCC = new Types.RCC.TestParameters
            {
                IsEnabled = false
            };
            Types.TOU.TestParameters ParamTOU = new Types.TOU.TestParameters
            {
                IsEnabled = false
            };
            ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;
            Status_Clear();
            if (!Cache.Net.Start(ParamGTU, Parameters, ParamBVT, ParamATU, ParamQrrTq, ParamIH, ParamRCC, ParamCommutation, ClampParameters, ParamTOU))
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

        private void btnTemperature_OnClick(object sender, RoutedEventArgs e) //Нагрев
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