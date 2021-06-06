using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using SCME.Types;
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
    public partial class BvtPage
    {
        //Цветовая индикация
        private readonly SolidColorBrush ColorRed, ColorGreen;
        //Состояние тестирования
        private bool isRunning;
        private bool WasCurrentMore;

        /// <summary>Параметры BVT</summary>
        public Types.BVT.TestParameters Parameters
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
        private bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                btnStart.IsEnabled = !isRunning;
                btnBack.IsEnabled = !isRunning;
            }
        }

        /// <summary>Инициализирует новый экземпляр класса BvtPage</summary>
        internal BvtPage()
        {
            //Установка базовых параметров BVT и пресса
            Parameters = new Types.BVT.TestParameters
            {
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
            ColorRed = (SolidColorBrush)FindResource("xRed1");
            ColorGreen = (SolidColorBrush)FindResource("xGreen3");
            //Предварительная очистка всех статусов
            Status_Clear();
            ViewportAxesRangeRestriction restr = new ViewportAxesRangeRestriction { YRange = new DisplayRange(-7, 7) };
            chartPlotter.Viewport.Restrictions.Add(restr);
        }

        //добавил
        public class DisplayRange
        {
            public double Start { get; set; }
            public double End { get; set; }

            public DisplayRange(double start, double end)
            {
                Start = start;
                End = end;
            }
        }

        public class ViewportAxesRangeRestriction : IViewportRestriction
        {
            public DisplayRange XRange = null;
            public DisplayRange YRange = null;

            public Rect Apply(Rect oldVisible, Rect newVisible, Viewport2D viewport)
            {
                if (XRange != null)
                {
                    newVisible.X = XRange.Start;
                    newVisible.Width = XRange.End - XRange.Start;
                }

                if (YRange != null)
                {
                    newVisible.Y = YRange.Start;
                    newVisible.Height = YRange.End - YRange.Start;
                }

                return newVisible;
            }

            public event EventHandler Changed;
        }
        //окончание добавления

        /// <summary>Установка всех результатов</summary>
        /// <param name="state">Состояние</param>
        internal void SetResultAll(DeviceState state)
        {
            if (state == DeviceState.InProcess)
                Status_Clear();
            else
                IsRunning = false;
        }

        /// <summary>Установка результата прямого</summary>
        /// <param name="state">Состояние</param>
        /// <param name="result">Результат теста</param>
        internal void SetResultBvtDirect(DeviceState state, Types.BVT.TestResults result)
        {
            labelWarning.Visibility = Visibility.Collapsed;
            labelFault.Visibility = Visibility.Collapsed;
            Label_Set(labelDirect, state, string.Format("{0}{1} : {2}{3}", result.VDRM, Properties.Resources.V, result.IDRM, Properties.Resources.mA));
            if (state == DeviceState.Success)
                Chart_PlotYX("Direct", ColorRed.Color, result.VoltageData, result.CurrentData);
        }

        /// <summary>Установка результата обратного</summary>
        /// <param name="state">Состояние</param>
        /// <param name="result">Результат теста</param>
        internal void SetResultReverseBvt(DeviceState state, Types.BVT.TestResults result)
        {
            labelWarning.Visibility = Visibility.Collapsed;
            labelFault.Visibility = Visibility.Collapsed;
            Label_Set(labelReverse, state, string.Format("{0}{1} : {2}{3}", result.VRRM, Properties.Resources.V, result.IRRM, Properties.Resources.mA));
            if (state == DeviceState.Success)
                Chart_PlotYX("Reverse", ColorGreen.Color, result.VoltageData, result.CurrentData);
        }

        /// <summary>Установка ошибок</summary>
        /// <param name="fault">Ошибка</param>
        internal void SetFault(Types.BVT.HWFaultReason fault)
        {
            labelFault.Content = fault.ToString();
            labelFault.Visibility = Visibility.Visible;
            IsRunning = false;

        }

        /// <summary>Установка предупреждений</summary>
        /// <param name="warning">Предупреждение</param>
        internal void SetWarning(Types.BVT.HWWarningReason warning)
        {
            labelWarning.Content = warning.ToString();
            labelWarning.Visibility = Visibility.Visible;
        }

        /// <summary>Установка проблем</summary>
        /// <param name="problem">Проблема</param>
        internal void SetProblem(Types.BVT.HWProblemReason problem)
        {
            labelWarning.Content = problem.ToString();
            labelWarning.Visibility = Visibility.Visible;
        }

        private void Status_Clear() //Очистка всех статусов
        {
            labelWarning.Visibility = Visibility.Collapsed;
            labelFault.Visibility = Visibility.Collapsed;
            Label_Reset(labelDirect);
            Label_Reset(labelReverse);
            chartPlotter.Children.RemoveAll(typeof(LineGraph));
            chartPlotter.Children.RemoveAll(typeof(MarkerPointsGraph));
        }

        internal void SetResultBvtUdsmUrsmDirect(DeviceState state, Types.BVT.TestResults result)
        {
            //            SetLabel(labelDirectSM, State,
            //                $"{Result.VDSM}{Properties.Resources.V} : {Result.IDSM}{Properties.Resources.mA}");
        }

        internal void SetResultBvtUdsmUrsmReverse(DeviceState state, Types.BVT.TestResults result)
        {
            //            SetLabel(labelReverseSM, State,
            //                $"{Result.VRSM}{Properties.Resources.V} : {Result.IRSM}{Properties.Resources.mA}");
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

        private void Label_Reset(ContentControl target) //Сброс значения
        {
            target.Content = string.Empty;
            target.Background = Brushes.Transparent;
        }

        private void Chart_PlotYX(string lineName, Color lineColor, ICollection<short> uxPoints, IEnumerable<short> uyPoints) //Отрисовка графика
        {
            int Crop = uxPoints.Count - 600;
            IEnumerable<short> DataI = uxPoints.Skip(Crop).Select(I => Math.Abs(I) <= 2 ? (short)0 : I);
            IEnumerable<short> DataV = uyPoints.Skip(Crop);
            if (DataI.Any() && (DataI.Min() < -7 * 10 || Math.Abs(DataI.Max()) > 7 * 10) || WasCurrentMore)
            {
                WasCurrentMore = true;
                chartPlotter.Viewport.Restrictions.Clear();
            }
            else
            {
                chartPlotter.Viewport.Restrictions.Clear();
                ViewportAxesRangeRestriction Restriction = new ViewportAxesRangeRestriction
                {
                    YRange = new DisplayRange(-7, 7)
                };
                chartPlotter.Viewport.Restrictions.Add(Restriction);
            }
            IEnumerable<PointF> Points = DataI.Zip(DataV, (I, V) => new PointF(V, I / 10.0f)).Select(P => Math.Abs(P.X) < 200 ? new PointF(P.X, 0) : P);
            EnumerableDataSource<PointF> DataSource = new EnumerableDataSource<PointF>(Points);
            DataSource.SetXMapping(P => P.X);
            DataSource.SetYMapping(P => P.Y);
            chartPlotter.AddLineGraph(DataSource, lineColor, 3, lineName);
            chartPlotter.FitToView();
        }

        private void btnStart_OnClick(object sender, RoutedEventArgs e)  //Запуск тестирования
        {
            WasCurrentMore = false;
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
            Types.VTM.TestParameters ParamSL = new Types.VTM.TestParameters
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
            Parameters.VoltageFrequency = (ushort)Settings.Default.BVTVoltageFrequency;
            Parameters.MeasurementMode = Types.BVT.BVTMeasurementMode.ModeV;
            if (!Cache.Net.Start(ParamGTU, ParamSL, Parameters, ParamATU, ParamQrrTq, ParamIH, ParamRCC, ParamCommutation, ClampParameters, ParamTOU))
                return;
            Status_Clear();
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
            TopTempLabel.Background = temeprature < BottomTemp || temeprature > TopTemp ? Brushes.Tomato : Brushes.LightGreen;
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