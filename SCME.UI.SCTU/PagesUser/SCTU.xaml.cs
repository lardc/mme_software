using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using System.Drawing;
using Color = System.Windows.Media.Color;
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
using SCME.Types.Clamping;

namespace SCME.UI.PagesUser
{
    /// <summary>
    /// Interaction logic for SCTU.xaml
    /// </summary>
    public partial class SCTU : Page
    {
        public Types.Commutation.ModuleCommutationType CommType { get; set; }
        public Types.Commutation.ModulePosition ModPosition { get; set; }
        public Types.Clamping.TestParameters ClampParameters { get; set; }
        public SctuTestParameters Parameters { get; set; }
        private const int TIME_STEP = 5;
        private readonly SolidColorBrush m_XGreen, m_XOrange;
        private const int RoomTemperature = 25;
        public int Temperature { get; set; }

        ViewportAxesRangeRestriction restr = new ViewportAxesRangeRestriction();

        public SCTU()
        {
            Parameters = new SctuTestParameters()
            {
                Type = SctuDutType.Diode,
                Value = 100,
                ShuntResistance = ushort.Parse(UserSettings.Default.ShuntResistance)
            };

            ClampParameters = new Types.Clamping.TestParameters
            {
                StandardForce = Types.Clamping.ClampingForceInternal.Custom,
                CustomForce = 5,
                IsHeightMeasureEnabled = false
            };

            Temperature = RoomTemperature;

            InitializeComponent();

            m_XGreen = (SolidColorBrush)FindResource("xGreen1");
            m_XOrange = (SolidColorBrush)FindResource("xOrange1");

            //ограничиваем координату X от 0 до 11000 мкс
            
            restr.XRange = new DisplayRange(0, 0);
            restr.YRange = new DisplayRange(0, 0);
            chartPlotter.Viewport.Restrictions.Add(restr);

            ClearStatus();
        }

        private void ClearStatus()
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
            labelResultVoltage.Content = "";
            labelResultCurrent.Content = "";
            labelMeasureGain.Content = "";
            chartPlotter.Children.RemoveAll(typeof(LineGraph));
        }

        private void StartButtonEnabled(bool Enabled)
        {
            //управляем доступностью кнопки btnStart
            btnStart.IsEnabled = Enabled;

            //освобождать рабочее место можно только когда доступна кнопка btnStart
            btnWorkPlaceIsFree.IsEnabled = Enabled;
        }

        public void StopButtonRoutine(bool StartButtonIsEnabled)
        {
            //раз мы здесь - значит была нажата кнопка Stop
            StartButtonEnabled(StartButtonIsEnabled);

            if (btnStart.IsEnabled)
                ClearStatus();
        }

        int countSetResult = 0;

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            countSetResult = 0;
            UserSettings.Default.ShuntResistance = TextBoxResistance.Text;
            UserSettings.Default.Save();
            StartButtonEnabled(false);

            ClearStatus();
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            var commPar = new Types.Commutation.TestParameters()
            {
                BlockIndex = (!Cache.Clamp.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                CommutationType = ConverterUtil.MapCommutationType(CommType),
                Position = ConverterUtil.MapModulePosition(ModPosition)
            };

            //если пресс был зажат вручную - не стоит пробовать зажимать его ещё раз
            ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;

            var parameters = new List<BaseTestParametersAndNormatives>(1);
            parameters.Add(Parameters);

            Cache.Net.Start(commPar, ClampParameters, parameters);
        }

        private void Plot(string LineName, Color LineColor, IEnumerable<int> UPoints)
        {
            if (UPoints.Count() == 0)
                return;

            var points = UPoints.Select((Time, Value) => new PointF(Value, Time)).ToList();

            if(LineName != Properties.Resources.Graph_V)
                restr.YRange.End = points.Max(m => m.Y) * 1.01;

            restr.XRange.End = Math.Max(restr.XRange.End, points.Max(m => m.X) * 1.01 * TIME_STEP) ;

            var dataSource = new EnumerableDataSource<PointF>(points);
            dataSource.SetXMapping(P => P.X * TIME_STEP);
            if (LineName != Properties.Resources.Graph_V)
                dataSource.SetYMapping(P => P.Y);
            else
            {
                var coefficient = restr.YRange.End / points.Max(m => m.Y) / 1.01;
                dataSource.SetYMapping(P => P.Y * coefficient);
            }

            chartPlotter.AddLineGraph(dataSource, LineColor, 3, LineName);
            chartPlotter.FitToView();

            
        }
        public void SetTopTemp(int temeprature)
        {
            TopTempLabel.Content = temeprature;
            var bottomTemp = Temperature - 2;
            var topTemp = Temperature + 2;
            if (temeprature < bottomTemp || temeprature > topTemp)
            {
                TopTempLabel.Background = System.Windows.Media.Brushes.Tomato;
            }
            else
            {
                TopTempLabel.Background = System.Windows.Media.Brushes.LightGreen;
            }
        }

        public void SetBottomTemp(int temeprature)
        {
            BotTempLabel.Content = temeprature;
            var bottomTemp = Temperature - 2;
            var topTemp = Temperature + 2;
            if (temeprature < bottomTemp || temeprature > topTemp)
            {
                BotTempLabel.Background = System.Windows.Media.Brushes.Tomato;
            }
            else
            {
                BotTempLabel.Background = System.Windows.Media.Brushes.LightGreen;
            }
        }

        public void SetResults(SctuHwState state, SctuTestResults results)
        {
            switch (state)
            {
                //данные готовы при состоянии SCTU WaitTimeOut
                case SctuHwState.WaitTimeOut:
                    countSetResult++;
                    labelResultVoltage.Content = results.VoltageValue;//((double)results.VoltageValue / 1000).ToString("0.00");
                    labelResultCurrent.Content = results.CurrentValue;
                    labelMeasureGain.Content = Math.Round(results.MeasureGain, 3).ToString();

                    //выводим графики тока и напряжения
                    Plot(@"I", m_XGreen.Color, results.CurrentData);
                    Plot(@"U", m_XOrange.Color, results.VoltageData);
                    break;

                case SctuHwState.Ready:
                    StartButtonEnabled(true);
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (vtbShockCurrent != null)
            {
                string sItem = (sender as ComboBox).SelectedItem.ToString();
                SctuWaveFormType WaveFormType = (SctuWaveFormType)Enum.Parse(typeof(SctuWaveFormType), sItem);

                switch (WaveFormType)
                {
                    //в зависимости от значения WaveFormType считываем значение соответствующего ресурса из ...ResourceDictionaries\Constraints
                    case SctuWaveFormType.Sinusoidal:
                        vtbShockCurrent.Maximum = (float)Application.Current.Resources["SctuShockCurrentSinusoidalMax"];
                        break;

                    case SctuWaveFormType.Trapezium:
                        vtbShockCurrent.Maximum = (float)Application.Current.Resources["SctuShockCurrentTrapeziumMax"];
                        break;
                }
            }
        }

        private void btnАdjustmentClick(object sender, RoutedEventArgs e)
        {
            //спрашиваем пароль наладчика и если пользователь его верно вводит - будет открыта TechnicanPage
            Page page = null;
            page = Cache.Password;

            if (page != null && NavigationService != null)
                NavigationService.Navigate(page);
        }

        private void StartHeating_Click(object sender, RoutedEventArgs e)
        {
            Cache.Net.StartHeating(Temperature);
        }

        private void StotHeating_Click(object sender, RoutedEventArgs e)
        {
            Cache.Net.StopHeating();
        }

        private void btnWorkPlaceIsFree_OnClick(object sender, RoutedEventArgs e)
        {
            //освобождение рабочего места. аналоговый канал измерения который был выбран ранее при этом не меняется
            Cache.Net.ActivationWorkPlace(ComplexParts.Sctu, ChannelByClumpType.NullValue, SctuWorkPlaceActivationStatuses.WORKPLACE_IS_FREE);

            //открываем ActivationWorkPage, она прочитает REG_WORKPLACE_ACTIVATION_STATUS и покажет то, что должна показать
            Cache.Main.mainFrame.Navigate(Cache.ActivationWorkPage);
        }

        internal void ClampDownAfterAlarm(SqueezingState state)
        {
            if (state == SqueezingState.Down && countSetResult == 0)
                StartButtonEnabled(true);
        }
    }

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


}
