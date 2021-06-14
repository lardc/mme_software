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
        private readonly SolidColorBrush ColorRed, ColorGreen, ColorOrange;
        //Состояние тестирования
        private bool isRunning;

        /// <summary>Инициализирует новый экземпляр класса GatePage</summary>
        internal GatePage()
        {
            //Установка базовых параметров GTU и пресса
            Parameters = new Types.GTU.TestParameters
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
            ColorGreen = (SolidColorBrush)FindResource("xGreen1");
            ColorOrange = (SolidColorBrush)FindResource("xOrange1");
            //Предварительная очистка всех статусов
            Status_Clear();
        }

        /// <summary>Параметры GTU</summary>
        public Types.GTU.TestParameters Parameters
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

        /// <summary>Установка результата теста Кельвина</summary>
        /// <param name="state">Состояние</param>
        /// <param name="isKelvinOk">Результат теста</param>
        internal void SetResultKelvin(DeviceState state, bool isKelvinOk)
        {
            Label_Set(lblKelvin, state, isKelvinOk ? Properties.Resources.Ok : Properties.Resources.Fault);
        }

        /// <summary>Установка результата сопротивления</summary>
        /// <param name="state">Состояние</param>
        /// <param name="resistance">Сопротивление</param>
        internal void SetResultResistance(DeviceState state, float resistance)
        {
            Label_Set(lblResistance, state, resistance.ToString());
        }

        /// <summary>Установка результата VGT</summary>
        /// <param name="state">Состояние</param>
        /// <param name="igt">IGT</param>
        /// <param name="vgt">UGT</param>
        /// <param name="arrayI">Эндпоинты IGT</param>
        /// <param name="arrayV">Эндпоинты UGT</param>
        internal void SetResultGT(DeviceState state, float igt, float vgt, IList<short> arrayI, IList<short> arrayV)
        {
            Label_Set(lblIGT, state, igt.ToString());
            Label_Set(lblVGT, state, vgt.ToString());
            if (state == DeviceState.Success)
            {
                Chart_Plot(@"Igt", ColorRed.Color, arrayI);
                Chart_Plot(@"Ugt", ColorOrange.Color, arrayV);
            }
        }

        /// <summary>Установка результата IH</summary>
        /// <param name="state">Состояние</param>
        /// <param name="ih">IH</param>
        /// <param name="array">Эндпоинты IH</param>
        internal void SetResultIh(DeviceState state, float ih, IList<short> array)
        {
            Label_Set(lblIH, state, ih.ToString());
            if (state == DeviceState.Success)
                Chart_Plot(@"Ih", ColorGreen.Color, array);
        }

        /// <summary>Установка результата IL</summary>
        /// <param name="state">Состояние</param>
        internal void SetResultIl(DeviceState state, float il)
        {
            Label_Set(lblIL, state, il.ToString());
        }

        /// <summary>Установка результата теста UGNT</summary>
        /// <param name="state">Состояние</param>
        /// <param name="vgnt">UGNT</param>
        /// <param name="ignt">IGNT</param>
        internal void SetResultVgnt(DeviceState state, ushort ignt, float vgnt)
        {
            Label_Set(lblIGNT, state, ignt.ToString());
            Label_Set(lblVGNT, state, vgnt.ToString());
        }

        /// <summary>Установка ошибок</summary>
        /// <param name="fault">Ошибка</param>
        internal void SetFault(Types.GTU.HWFaultReason fault)
        {
            lblFault.Content = fault.ToString();
            lblFault.Visibility = Visibility.Visible;
            IsRunning = false;
        }

        /// <summary>Установка предупреждений</summary>
        /// <param name="warning">Предупреждение</param>
        internal void SetWarning(Types.GTU.HWWarningReason warning)
        {
            lblWarning.Content = warning.ToString();
            lblWarning.Visibility = Visibility.Visible;
        }
        
        /// <summary>Установка проблем</summary>
        /// <param name="problem">Проблема</param>=
        internal void SetProblem(Types.GTU.HWProblemReason problem)
        {
            lblWarning.Content = problem.ToString();
            lblWarning.Visibility = Visibility.Visible;
        }

        private void Status_Clear() //Очистка всех статусов
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
            Label_Reset(lblKelvin);
            Label_Reset(lblResistance);
            Label_Reset(lblIGT);
            Label_Reset(lblVGT);
            Label_Reset(lblIH);
            Label_Reset(lblIL);
            Label_Reset(lblVGNT);
            Label_Reset(lblIGNT);
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

        private void Label_Reset(ContentControl target) //Сброс значения
        {
            target.Content = string.Empty;
            target.Background = Brushes.Transparent;
        }

        private void Chart_Plot(string lineName, Color lineColor, IEnumerable<short> uPoints) //Отрисовка графика
        {
            List<PointF> Points = uPoints.Select((T, I) => new PointF(I, T)).ToList();
            EnumerableDataSource<PointF> DataSource = new EnumerableDataSource<PointF>(Points);
            DataSource.SetXMapping(P => P.X);
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
            Types.VTM.TestParameters ParamSL = new Types.VTM.TestParameters
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
            if (!Cache.Net.Start(Parameters, ParamSL, ParamBVT, ParamATU, ParamQrrTq, ParamIH, ParamRCC, ParamCommutation, ClampParameters, ParamTOU))
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

        private void Pulse_Gate_Click(object sender, RoutedEventArgs e) //Импульс управления
        {
            try
            {
                Types.GTU.CalibrationResultGate Result = Cache.Net.GatePulseCalibrationGate(ushort.Parse(calibrationCurrent.Text));
                actualCurrent.Content = Result.Current.ToString(CultureInfo.InvariantCulture);
                actualVoltage.Content = Result.Voltage.ToString(CultureInfo.InvariantCulture);
            }
            catch { }
        }

        private void Pulse_Main_Click(object sender, RoutedEventArgs e) //Силовой импульс
        {
            try
            {
                ushort Result = Cache.Net.GatePulseCalibrationMain(ushort.Parse(calibrationCurrent.Text));
                actualCurrent.Content = Result.ToString(CultureInfo.InvariantCulture);
                actualVoltage.Content = "0";
            }
            catch { }
        }
    }
}