using Newtonsoft.Json;
using SCME.Types;
using SCME.UI.IO;
using SCME.UI.ViewModels;
using SCME.UIServiceConfig.Properties;
using SCME.WpfControlLibrary;
using SCME.WpfControlLibrary.CustomControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using DialogWindow = SCME.UI.CustomControl.DialogWindow;

namespace SCME.UI
{
    public partial class MainWindow : IMainWindow
    {
        //Состояние виртуальной клавиатуры
        private bool IsKeyboardShown;
        //Предыдущая страница
        private Page PrevPage;
        //Базовый цвет иконки пресса
        private Brush NominalClampPathStroke;

        /// <summary>Инициализирует новый экземпляр класса MainWindow</summary>
        public MainWindow()
        {
            try
            {
                foreach ((ComplexParts key, bool value) in JsonConvert.DeserializeObject<Dictionary<ComplexParts, bool>>(Properties.SettingsUI.Default.ComplexPartsIsDisabled))
                    Cache.Welcome.DeviceSetEnabled(key, !value);
            }
            catch { }
            //Версия сборки
            string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //Добавление обработчиков исключений
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            //Подгрузка ограничений
            Constraints_Load();
            //Установка локализованной клавиатуры
            Cache.Keyboards = new KeyboardLayouts(Path.IsPathRooted(Settings.Default.KeyboardsPath) ? Settings.Default.KeyboardsPath : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), Settings.Default.KeyboardsPath));
            ResourceBinding.Scaling(0.60);
            InitializeComponent();
            VM.TechPasswordVisibility = Visibility.Collapsed;
            try
            {
                Cache.Main = this;
                //Проверка разрешения экрана
                if (Settings.Default.WindowIs1280x1024)
                {
                    Top = SystemParameters.WorkArea.Top;
                    Left = SystemParameters.WorkArea.Left;
                    Width = 1280;
                    Height = 1024;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    if (Settings.Default.NormalWindow)
                    {

                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        ResizeMode = ResizeMode.CanResize;
                        WindowState = WindowState.Maximized;

                    }
                    else
                    {
                        Top = SystemParameters.WorkArea.Top;
                        Left = SystemParameters.WorkArea.Left;
                        Width = SystemParameters.WorkArea.Width;
                        Height = SystemParameters.WorkArea.Height;
                    }
                }
                Cache.Storage = new LocalStorage(Settings.Default.StoragePath);
                Cache.Net = new ControlLogic();
                VM.IsSafetyBreakIconVisible = false;
                RestartRoutine(null, null);
            }
            catch (Exception ex)
            {
                DialogWindow DialogWindow = new DialogWindow(Properties.Resources.ApplicationStartError, ex.Message);
                DialogWindow.ButtonConfig(DialogWindow.EbConfig.OK);
                DialogWindow.ShowDialog();
                Application.Current.Shutdown(1);
            }
        }

        /// <summary>Модель представления главного окна</summary>
        public MainWindowVM VM
        {
            get; set;
        } = new MainWindowVM();

        /// <summary>Необходимость в перезапуске приложения</summary>
        internal bool NeedsToRestart
        {
            get; private set;
        }

        /// <summary>Чтение профилей</summary>
        internal bool AreProfilesParsed
        {
            get; set;
        }

        /// <summary>Параметры запуска приложения</summary>
        internal TypeCommon.InitParams Param
        {
            get; private set;
        }

        /// <summary>Видимость строки состояния подключения</summary>
        public bool ConnectionLabelVisible
        {
            set => connectionLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Constraints_Load() //Подгрузка ограничений
        {
            string ConstraintsPath = Path.Combine(Path.GetDirectoryName(Settings.Default.AccountsPath), "Constraints.xaml");
            //Проверка существования файла
            if (!File.Exists(ConstraintsPath))
                return;
            ResourceDictionary ResourceDictionaryUser;
            using (FileStream Stream = new FileStream(ConstraintsPath, FileMode.Open))
                ResourceDictionaryUser = (ResourceDictionary)XamlReader.Load(Stream);
            ResourceDictionary ResourceDictionaryApplication = Application.Current.Resources.MergedDictionaries.First(m => m.Source.AbsolutePath.Contains("Constraints.xaml"));
            //Перебор всех ограничений
            foreach (object Entry in ResourceDictionaryUser)
            {
                DictionaryEntry DictionaryEntry = (DictionaryEntry)Entry;
                ResourceDictionaryApplication.Remove(DictionaryEntry.Key);
                ResourceDictionaryApplication.Add(DictionaryEntry.Key, DictionaryEntry.Value);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) //Загрузка окна
        {
            VM.WaitProgressBarIsShow = true;
            //Сообщения об эмулируемых блоках
            List<string> EmulationMessages = new List<string>();
            //Эмуляция адаптера
            if (Settings.Default.AdapterEmulation)
                EmulationMessages.Add(Properties.Resources.Adapter);
            //Эмуляция шлюза
            if (Settings.Default.GatewayEmulation)
                EmulationMessages.Add(Properties.Resources.Gateway);
            //Эмуляция блока коммутации
            if (Settings.Default.CommutationEmulation && Settings.Default.CommIsVisible)
                EmulationMessages.Add(Properties.Resources.Commutation);
            //Эмуляция блока коммутации 2
            if (Settings.Default.CommutationExEmulation && Settings.Default.CommExIsVisible)
                EmulationMessages.Add(Properties.Resources.Commutation + "2");
            //Эмуляция блока GTU
            if (Settings.Default.GateEmulation && Settings.Default.GateIsVisible)
                EmulationMessages.Add(Properties.Resources.Gate);
            //Эмуляция блока SL
            if (Settings.Default.SLEmulation && Settings.Default.SLIsVisible)
                EmulationMessages.Add(Properties.Resources.Vtm);
            //Эмуляция блока BVT
            if (Settings.Default.BVTEmulation && Settings.Default.BvtIsVisible)
                EmulationMessages.Add(Properties.Resources.Bvt);
            //Эмуляция пресса
            if (Settings.Default.ClampingSystemEmulation && Settings.Default.ClampIsVisible)
                EmulationMessages.Add(Properties.Resources.Clamp);
            //Эмуляция блока dUdt
            if (Settings.Default.dVdtEmulation && Settings.Default.dVdtIsVisible)
                EmulationMessages.Add(Properties.Resources.dVdt);
            //Эмуляция блока ATU
            if (Settings.Default.ATUEmulation && Settings.Default.ATUIsVisible)
                EmulationMessages.Add(Properties.Resources.ATU);
            //Эмуляция блока QrrTq
            if (Settings.Default.QrrTqEmulation && Settings.Default.QrrTqIsVisible)
                EmulationMessages.Add(Properties.Resources.QrrTq);
            //Эмуляция блока R A-C
            
            //if (Settings.Default.RACEmulation && Settings.Default.RACIsVisible)
            //    EmulationMessages.Add(Properties.Resources.RAC);
            
            //Эмуляция блока TOU
            if (Settings.Default.TOUEmulation && Settings.Default.TOUIsVisible)
                EmulationMessages.Add(Properties.Resources.TOU);
            //Получение строки из сообщений
            string EmulationMessage = string.Join(", ", EmulationMessages);
            //Строка пуста
            if (string.IsNullOrEmpty(EmulationMessage))
                return;
            DialogWindow DialogWindow = new DialogWindow(Properties.Resources.Warning, string.Format("{0}:\n{1}", Properties.Resources.WarningEmulation, EmulationMessage));
            DialogWindow.ButtonConfig(DialogWindow.EbConfig.OK);
            DialogWindow.ShowDialog();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) //Закрытие приложения
        {
            Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e) //Деинициализация приложения
        {
            NeedsToRestart = false;
            if (Cache.Net != null)
                Cache.Net.Deinitialize();
        }

        private void MainWindow_Closed(object sender, EventArgs e) //Запуск explorer.exe
        {
            if (Settings.Default.RunExplorer)
                Process.Start("explorer.exe");
        }

        private async void VersionInfo_Click(object sender, RoutedEventArgs e) //Информация о текущей версии
        {
            ToolTip VersionToolTip = (ToolTip)VersionInfo.ToolTip;
            VersionToolTip.IsOpen = true;
            await System.Threading.Tasks.Task.Delay(10000);
            VersionToolTip.IsOpen = false;
        }

        private void Logout_Click(object sender, RoutedEventArgs e) //Выход из профиля
        {
            Cache.Net.SetSafetyMode(SafetyMode.Internal);
            if (Equals(Cache.Main.mainFrame.Content, Cache.UserTest))
                Cache.UserTest.OnLeaveNotify();
            mainFrame.Navigate(Cache.Login);
        }

        private void Tech_Click(object sender, RoutedEventArgs e) //Переход в режим наладчика
        {
            ServiceManLogin();
        }

        /// <summary>Переход в режим наладчика</summary>
        public void ServiceManLogin()
        {
            if (Equals(Cache.Main.mainFrame.Content, Cache.UserTest))
                Cache.UserTest.OnLeaveNotify();
            if (Settings.Default.IsTechPasswordEnabled)
            {
                Cache.Password.AfterOkRoutine = delegate
                {
                    mainFrame.Navigate(Cache.Technician);
                };
                mainFrame.Navigate(Cache.Password);
            }
            else
                mainFrame.Navigate(Cache.Technician);
        }

        private void MainWindow_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) //Открытие виртуальной клавиатуры
        {
            if (e.NewFocus is ValidatingTextBox TextBox)
                ShowKeyboard(!Settings.Default.NormalWindow, TextBox);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e) //Закрытие виртуальной клавиатуры
        {
            ShowKeyboard(false, null);
        }

        public void ShowKeyboard(bool show, Control control) //Отрисовка виртуальной клавиатуры
        {
            //Необходимо открытие
            if (!IsKeyboardShown && show)
            {
                IsKeyboardShown = true;
                //Наслоение на объекты страницы
                bool AsPopup = !(control.TransformToAncestor(this).Transform(new Point(0, control.ActualHeight)).Y > Height - defaultKeyboard.Height);
                if (AsPopup)
                {
                    defaultKeyboard.Show(Settings.Default.IsAnimationEnabled);
                    keyboardPopup.IsOpen = true;
                }
                else
                {
                    keyboardBorder.Height = defaultKeyboard.Height;
                    defaultKeyboard.Show(Settings.Default.IsAnimationEnabled);
                    keyboardPopup.IsOpen = true;
                    AnimateScrollViewer(defaultKeyboard.Height);
                }
            }
            //Необходимо сокрытие
            if (IsKeyboardShown && !show)
            {
                IsKeyboardShown = false;
                AnimateScrollViewer(0);
                defaultKeyboard.Hide(Settings.Default.IsAnimationEnabled);
            }
        }

        private void AnimateScrollViewer(double to) //Анимация скроллвьюера для отрисовки виртуальной клавиатуры
        {
            keyboardBorder.Height = to;
            if (Settings.Default.IsAnimationEnabled)
            {
                DoubleAnimation VerticalAnimation = new DoubleAnimation
                {
                    To = to,
                    DecelerationRatio = 0.2,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                };
                Storyboard Storyboard = new Storyboard();
                Storyboard.Children.Add(VerticalAnimation);
                Storyboard.SetTarget(VerticalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(VerticalAnimation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
                Storyboard.Begin();
            }
            else
                scrollViewer.ScrollToVerticalOffset(to);
        }

        private void Keyboard_EnterPressed(object sender, RoutedEventArgs e) //Подтверждение ввода на виртуальной клавиатуре
        {
            ShowKeyboard(false, null);
        }

        private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e) //Анимация при навигации
        {
            if (!Settings.Default.IsAnimationEnabled)
                return;
            DoubleAnimation Animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            mainFrame.BeginAnimation(OpacityProperty, Animation);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e) //Навигация между страницами
        {
            VM.TopTitle = ((Page)e.Content)?.Title;
            if (Equals(mainFrame.Content, Cache.Password) && Settings.Default.IsTechPasswordEnabled)
                Cache.Technician.PreviousPage = PrevPage;
            PrevPage = (Page)e.Content;
            
            if (!Settings.Default.IsAnimationEnabled)
                return;
            //Анимация перехода
            DoubleAnimation Animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            mainFrame.BeginAnimation(OpacityProperty, Animation);
        }

        internal void RestartRoutine(object sender, RoutedEventArgs e) //Процесс перезапуска
        {
            Param = new TypeCommon.InitParams
            {
                TimeoutService = Cache.Welcome.GetTimeout(ComplexParts.Service),
                IsInternalEnabled = false,
                //Адаптер
                TimeoutAdapter = Cache.Welcome.GetTimeout(ComplexParts.Adapter),
                //GTU
                IsGateEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.Gate) && Settings.Default.GateIsVisible,
                TimeoutGate = Cache.Welcome.GetTimeout(ComplexParts.Gate),
                //SL
                IsSLEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.SL) && Settings.Default.SLIsVisible,
                TimeoutSL = Cache.Welcome.GetTimeout(ComplexParts.SL),
                //BVT
                IsBVTEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.BVT) && Settings.Default.BvtIsVisible,
                TimeoutBVT = Cache.Welcome.GetTimeout(ComplexParts.BVT),
                //Пресс
                IsClampEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.Clamping) && Settings.Default.ClampIsVisible,
                TimeoutClamp = Cache.Welcome.GetTimeout(ComplexParts.Clamping),
                //dUdt
                IsdVdtEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.DvDt) && Settings.Default.dVdtIsVisible,
                TimeoutdVdt = Cache.Welcome.GetTimeout(ComplexParts.DvDt),
                //ATU
                TimeoutATU = Cache.Welcome.GetTimeout(ComplexParts.ATU),
                IsATUEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.ATU) && Settings.Default.ATUIsVisible,
                //QrrTq
                TimeoutQrrTq = Cache.Welcome.GetTimeout(ComplexParts.QrrTq),
                IsQrrTqEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.QrrTq) && Settings.Default.QrrTqIsVisible,
                //R A-C
                TimeoutRAC = Cache.Welcome.GetTimeout(ComplexParts.RAC),
                IsRACEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.RAC) && Settings.Default.RACIsVisible,
                //IH
                TimeoutIH = Cache.Welcome.GetTimeout(ComplexParts.IH),
                IsIHEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.IH) && Settings.Default.IHIsVisible,
                //TOU
                IsTOUEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.TOU) && Settings.Default.TOUIsVisible,
                TimeoutTOU = Cache.Welcome.GetTimeout(ComplexParts.TOU),
                SafetyMode = VM.SafetyMode,
                SoftVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };
            //Открытие главной страницы
            if (!Equals(mainFrame.Content, Cache.Welcome))
                mainFrame.Navigate(Cache.Welcome);
            Cache.Welcome.IsRestartEnable = false;
            Cache.Welcome.IsBackEnable = false;
            Cache.Technician.AreButtonsEnabled(Param);
            Cache.Console.AreButtonEnabled(Param);
            Cache.Selftest.AreButtonEnabled(Param);
            NeedsToRestart = true;
            AreProfilesParsed = false;
            Cache.Net.Deinitialize();
        }

        internal void SetClampState(Types.Clamping.SqueezingState state) //Установка состояния пресса
        {
            SolidColorBrush m_XOrange = (SolidColorBrush)FindResource("xOrange1");
            switch (state)
            {
                case Types.Clamping.SqueezingState.Down:
                    clampPath.Stroke = Brushes.Green;
                    break;
                case Types.Clamping.SqueezingState.Squeezing:
                    clampPath.Stroke = Brushes.Gold;
                    break;
                case Types.Clamping.SqueezingState.Up:
                    clampPath.Stroke = m_XOrange;
                    break;
                case Types.Clamping.SqueezingState.Unsqueezing:
                    clampPath.Stroke = Brushes.Gold;
                    break;
                case Types.Clamping.SqueezingState.Undeterminated:
                    clampPath.Stroke = m_XOrange;
                    break;
            }
            NominalClampPathStroke = clampPath.Stroke;
        }

        internal void SetClampWarning(Types.Clamping.HWWarningReason warning) //Установка предупреждений пресса
        {
            if (warning == Types.Clamping.HWWarningReason.None)
                clampPath.Stroke = NominalClampPathStroke;
            else
                clampPath.Stroke = Brushes.Tomato;
        }

        internal void SetClampFault(Types.Clamping.HWFaultReason fault) //Установка ошибок пресса
        {
            if (fault == Types.Clamping.HWFaultReason.None)
                clampPath.Stroke = NominalClampPathStroke;
            else
                clampPath.Stroke = Brushes.Tomato;
        }

        public Point GetWaitProgressBarPoint() //Получение положения колеса загрузки
        {
            return WaitProgressBar.TransformToAncestor(this).Transform(new Point(0, 0));
        }

        public Point GetWaitProgressBarSize() //Получение размера колеса загрузки
        {
            return new Point(WaitProgressBar.Width, WaitProgressBar.Height);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args) //Обработчик исключений
        {
            MessageBox.Show(args.ExceptionObject.ToString(), "Unhandled exception");
        }

        static void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) //Обработчик исключений текущего диспетчера
        {
            MessageBox.Show(e.Exception.ToString(), "Unhandled exception");
        }
    }

    public class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer ScrollViewer = (ScrollViewer)target;
            if (ScrollViewer != null)
                ScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }
}