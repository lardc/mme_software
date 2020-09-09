using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using SCME.Types;
using SCME.Types.Profiles;
using SCME.UI.IO;
using SCME.UI.PagesTech;
using SCME.UI.PagesUser;
using SCME.UIServiceConfig.Properties;
using SCME.UI.ViewModels;
using SCME.WpfControlLibrary;
using SCME.WpfControlLibrary.CustomControls;
using DialogWindow = SCME.UI.CustomControl.DialogWindow;
using System.Reflection;
using System.Xml.Serialization;
using System.Text.Json;
using Newtonsoft.Json;

namespace SCME.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        private const string PROFILE_ENDPOINT_NAME = "SCME.ProfileService";

        private string _state = "LOCAL";
        private string _TopTitle = "Title";
        private readonly SolidColorBrush m_XRed, m_XOrange;
        private bool m_IsSafetyBreakIconVisible;
        private bool m_IsKeyboardShown;
        private Visibility m_GoTechButtonVisibility, m_AccountButtonVisibility, m_TechPasswordVisibility;
        private Page m_PrevPage;
        private Brush m_NominalClampPathStroke;

        public MainWindowVM VM { get; set; } = new MainWindowVM();
        public MainWindow()
        {
            try
            {
                foreach (var (key, value) in JsonConvert.DeserializeObject<Dictionary<ComplexParts, bool>>(Properties.SettingsUI.Default.ComplexPartsIsDisabled))
                    Cache.Welcome.DeviceSetEnabled(key, !value);
            }
            catch
            {
                // ignored
            }


            string s = Assembly.GetExecutingAssembly().GetName().Version.ToString(); 
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                //Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Localization);
//                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Settings.Default.Localization);
//                Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Localization);
//                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Localization);
//                LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
//                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch (Exception ex)
            {
                var dw = new DialogWindow("Localization error", ex.Message);
                dw.ButtonConfig(DialogWindow.EbConfig.OK);
                dw.ShowDialog();
            }

            Cache.Keyboards = new KeyboardLayouts(Path.IsPathRooted(Settings.Default.KeyboardsPath) ? Settings.Default.KeyboardsPath : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase),Settings.Default.KeyboardsPath));
            
            ResourceBinding.Scaling(0.60);

            InitializeComponent();


            m_XRed = (SolidColorBrush)FindResource("xRed1");
            m_XOrange = (SolidColorBrush)FindResource("xOrange1");

            VM.TechPasswordVisibility = Visibility.Collapsed;

            try
            {
                Cache.Main = this;
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
                var dw = new DialogWindow(Properties.Resources.ApplicationStartError, ex.Message);
                dw.ButtonConfig(DialogWindow.EbConfig.OK);
                dw.ShowDialog();

                Application.Current.Shutdown(1);
            }
            
            mainFrame.Navigated += MainFrameOnNavigated;
            mainFrame.Navigating += MainFrameOnNavigating;
            
        }

        public Point GetWaitProgressBarPoint => WaitProgressBar.TransformToAncestor(this).Transform(new Point(0,0));
        public Point GetWaitProgressBarSize => new Point(WaitProgressBar.Width, WaitProgressBar.Height);

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            VM.WaitProgressBarIsShow = true;

            var emulationMessages = new List<string>();
            
            if(Settings.Default.CommutationEmulation && Settings.Default.CommIsVisible)
                emulationMessages.Add(Properties.Resources.Commutation);
            
            if(Settings.Default.dVdtEmulation && Settings.Default.dVdtIsVisible)
                emulationMessages.Add(Properties.Resources.dVdt);
            
            /*if(Settings.Default.SctuEmulation && Settings.Default.sctu)
                emulationMessages.Add("SCTU");*/
            
            if(Settings.Default.CommutationExEmulation && Settings.Default.CommExIsVisible)
                emulationMessages.Add(Properties.Resources.Commutation + "2");
            
            if(Settings.Default.IHEmulation && Settings.Default.IHIsVisible)
                emulationMessages.Add(Properties.Resources.IH);
            
            if(Settings.Default.QrrTqEmulation && Settings.Default.QrrTqIsVisible)
                emulationMessages.Add(Properties.Resources.QrrTq);
            
            if(Settings.Default.SLEmulation && Settings.Default.SLIsVisible)
                emulationMessages.Add(Properties.Resources.Vtm);
            
            if(Settings.Default.ATUEmulation && Settings.Default.ATUIsVisible)
                emulationMessages.Add(Properties.Resources.ATU);
            
            if(Settings.Default.BVTEmulation && Settings.Default.BvtIsVisible)
                emulationMessages.Add(Properties.Resources.Bvt);
            
            if(Settings.Default.RACEmulation && Settings.Default.RACIsVisible)
                emulationMessages.Add(Properties.Resources.RAC);
            
            /*if(Settings.Default.RCCEmulation)
                emulationMessages.Add("RCC");*/
            
            if(Settings.Default.TOUEmulation && Settings.Default.TOUIsVisible)
                emulationMessages.Add(Properties.Resources.TOU);
            
            if(Settings.Default.AdapterEmulation )
                emulationMessages.Add(Properties.Resources.Adapter);
            
            if(Settings.Default.GateEmulation && Settings.Default.GateIsVisible)
                emulationMessages.Add(Properties.Resources.Gate);
            
            if(Settings.Default.GatewayEmulation )
                emulationMessages.Add(Properties.Resources.Gateway);
            
            if(Settings.Default.ClampingSystemEmulation && Settings.Default.ClampIsVisible)
                emulationMessages.Add(Properties.Resources.Clamp);

            var emulationMessage = string.Join(", ", emulationMessages);
            if (!string.IsNullOrEmpty(emulationMessage))
            {
                var dialogWindow = new DialogWindow(Properties.Resources.Warning, $"{Properties.Resources.WarningEmulation}:{Environment.NewLine}{emulationMessage}");
                dialogWindow.ButtonConfig(DialogWindow.EbConfig.OK);
                dialogWindow.ShowDialog();
            }
        }

        private void MainFrameOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            
        }

        private void MainFrameOnNavigated(object sender, NavigationEventArgs e)
        {
            VM.TopTitle = (e.Content as Page)?.Title;
            
        }


        private static void CurrentDomainOnUnhandledException(object Sender, UnhandledExceptionEventArgs Args)
        {
            MessageBox.Show(Args.ExceptionObject.ToString(), "Unhandled exception");
        }

        static void DispatcherUnhandledException(object Sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs E)
        {
            MessageBox.Show(E.Exception.ToString(), "Unhandled exception");
        }

        public bool ConnectionLabelVisible
        {
            set { connectionLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

    
   

        internal bool IsNeedToRestart { get; private set; }

        internal bool IsProfilesParsed { get; set; }

        internal TypeCommon.InitParams Param { get; private set; }

        internal void RestartRoutine(object Sender, RoutedEventArgs E)
        {
            
            Param = new TypeCommon.InitParams
            {
                TimeoutService = Cache.Welcome.GetTimeout(ComplexParts.Service),
                IsInternalEnabled = false,
                TimeoutAdapter = Cache.Welcome.GetTimeout(ComplexParts.Adapter),
                IsGateEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.Gate) && Settings.Default.GateIsVisible,
                TimeoutGate = Cache.Welcome.GetTimeout(ComplexParts.Gate),
                IsSLEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.SL) && Settings.Default.SLIsVisible,
                TimeoutSL = Cache.Welcome.GetTimeout(ComplexParts.SL),
                IsBVTEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.BVT) && Settings.Default.BvtIsVisible,
                TimeoutBVT = Cache.Welcome.GetTimeout(ComplexParts.BVT),
                IsClampEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.Clamping) && Settings.Default.ClampIsVisible,
                TimeoutClamp = Cache.Welcome.GetTimeout(ComplexParts.Clamping),
                TimeoutdVdt = Cache.Welcome.GetTimeout(ComplexParts.DvDt),
                IsdVdtEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.DvDt) && Settings.Default.dVdtIsVisible,
                TimeoutATU = Cache.Welcome.GetTimeout(ComplexParts.ATU),
                IsATUEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.ATU) && Settings.Default.ATUIsVisible,
                TimeoutQrrTq = Cache.Welcome.GetTimeout(ComplexParts.QrrTq),
                IsQrrTqEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.QrrTq) && Settings.Default.QrrTqIsVisible,
                TimeoutRAC = Cache.Welcome.GetTimeout(ComplexParts.RAC),
                IsRACEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.RAC) && Settings.Default.RACIsVisible,
                TimeoutIH = Cache.Welcome.GetTimeout(ComplexParts.IH),
                IsIHEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.IH) && Settings.Default.IHIsVisible,
                IsTOUEnabled = Cache.Welcome.IsDeviceEnabled(ComplexParts.TOU) && Settings.Default.TOUIsVisible,
                TimeoutTOU = Cache.Welcome.GetTimeout(ComplexParts.TOU),
                SafetyMode = VM.SafetyMode
            };

            if (!Equals(mainFrame.Content, Cache.Welcome))
                mainFrame.Navigate(Cache.Welcome);

            Cache.Welcome.IsRestartEnable = false;
            Cache.Welcome.IsBackEnable = false;
            Cache.Technician.AreButtonsEnabled(Param);
            Cache.Console.AreButtonEnabled(Param);
            Cache.Selftest.AreButtonEnabled(Param);

            IsNeedToRestart = true;
            IsProfilesParsed = false;
            Cache.Net.Deinitialize();
        }

        internal void SetClampState(Types.Clamping.SqueezingState State)
        {
            switch (State)
            {
                case Types.Clamping.SqueezingState.Down:
                    clampPath.Stroke = Brushes.DarkSeaGreen;
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
                    clampPath.Stroke = m_XRed;
                    break;
            }

            m_NominalClampPathStroke = clampPath.Stroke;
        }

        internal void SetClampWarning(Types.Clamping.HWWarningReason Warning)
        {
            if (Warning == Types.Clamping.HWWarningReason.None)
            {
                clampPath.Stroke = m_NominalClampPathStroke;
            }
            else
            {
                clampPath.Stroke = Brushes.Tomato;
            }
        }

        internal void SetClampFault(Types.Clamping.HWFaultReason Fault)
        {
            if (Fault == Types.Clamping.HWFaultReason.None)
            {
                clampPath.Stroke = m_NominalClampPathStroke;
            }
            else
            {
                clampPath.Stroke = Brushes.Tomato;
            }
        }

        private void BtnExit_Click(object Sender, RoutedEventArgs E)
        {
            Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object Sender, CancelEventArgs E)
        {
            IsNeedToRestart = false;

            var complexPartsIsDisabled = new Dictionary<ComplexParts, bool>();

            foreach (var i in (ComplexParts[])Enum.GetValues(typeof(ComplexParts)))
                complexPartsIsDisabled[i] = Cache.Welcome.IsDeviceEnabled(i);

            Properties.SettingsUI.Default.ComplexPartsIsDisabled = JsonConvert.SerializeObject(complexPartsIsDisabled);
            Properties.SettingsUI.Default.Save();

            if (Cache.Net != null)
                Cache.Net.Deinitialize();
        }

        private void MainWindow_OnClosed(object Sender, EventArgs E)
        {
            if (Settings.Default.RunExplorer)
                Process.Start("explorer.exe");
        }

        private void Frame_Navigating(object Sender, NavigatingCancelEventArgs E)
        {
            if (Settings.Default.IsAnimationEnabled)
            {
                var animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200))
                };
                mainFrame.BeginAnimation(OpacityProperty, animation);
            }
        }

        private void Frame_Navigated(object Sender, NavigationEventArgs E)
        {
//            VM.TopTitle = ((Page)mainFrame.Content).Title;
//            VM.GoTechButtonVisibility = Equals(mainFrame.Content, Cache.ProfileSelection) ||
//                                     Equals(mainFrame.Content, Cache.UserTest) ||
//                                     Equals(mainFrame.Content, Cache.Login)
//                                         ? Visibility.Visible
//                                         : Visibility.Collapsed;
//            VM.AccountButtonVisibility = Equals(mainFrame.Content, Cache.ProfileSelection) ||
//                                      Equals(mainFrame.Content, Cache.UserTest)
//                                          ? Visibility.Visible
//                                          : Visibility.Collapsed;

            if (Equals(mainFrame.Content, Cache.Password) && Settings.Default.IsTechPasswordEnabled)
                Cache.Technician.PreviousPage = m_PrevPage;
            m_PrevPage = E.Content as Page;

            if (Settings.Default.IsAnimationEnabled)
            {
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200))
                };
                mainFrame.BeginAnimation(OpacityProperty, animation);
            }
        }

        public void ServiceManLogin()
        {
            if (Equals(Cache.Main.mainFrame.Content, Cache.UserTest))
                Cache.UserTest.OnLeaveNotify();

//            if (!Cache.Main.IsProfilesParsed)
//                ProfilesDbLogic.ImportProfilesFromDb();

            if (Settings.Default.IsTechPasswordEnabled)
            {
                Cache.Password.AfterOkRoutine = delegate { mainFrame.Navigate(Cache.Technician); };
                mainFrame.Navigate(Cache.Password);                    
            }
            else
                mainFrame.Navigate(Cache.Technician);
        }

        private void Tech_Click(object Sender, RoutedEventArgs E)
        {
            ServiceManLogin();
        }

        private void Logout_Click(object Sender, RoutedEventArgs E)
        {
            Cache.Net.SetSafetyMode(SafetyMode.Internal);

            if (Equals(Cache.Main.mainFrame.Content, Cache.UserTest))
                Cache.UserTest.OnLeaveNotify();

            mainFrame.Navigate(Cache.Login);
        }

        #region Keyboard

        private void MyWindow_PreviewGotKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E)
        {
            if (E.NewFocus is ValidatingTextBox validatingTextBox)
                ShowKeyboard(!Settings.Default.NormalWindow, validatingTextBox);
        }

        private void MyWindow_MouseDown(object Sender, MouseButtonEventArgs E)
        {
            ShowKeyboard(false, null);
        }

        public void ShowKeyboard(bool Show, Control Control)
        {
            if (!m_IsKeyboardShown && Show)
            {
                m_IsKeyboardShown = true;

                var asPopup =
                    !(Control.TransformToAncestor(this).Transform(new Point(0, Control.ActualHeight)).Y >
                      Height - defaultKeyboard.Height);

                if (asPopup)
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

            if (m_IsKeyboardShown && !Show)
            {
                m_IsKeyboardShown = false;
                AnimateScrollViewer(0);
                defaultKeyboard.Hide(Settings.Default.IsAnimationEnabled);
            }
        }

        private void AnimateScrollViewer(double To)
        {
            keyboardBorder.Height = To;

            if (Settings.Default.IsAnimationEnabled)
            {
                var verticalAnimation = new DoubleAnimation
                {
                    To = To,
                    DecelerationRatio = .2,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);
                Storyboard.SetTarget(verticalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(verticalAnimation,
                                             new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));
                storyboard.Begin();
            }
            else
                scrollViewer.ScrollToVerticalOffset(To);
        }

        private void Keyboard_EnterPressed(object Sender, RoutedEventArgs E)
        {
            ShowKeyboard(false, null);
        }

        #endregion


    }

    public class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior),
                                                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement Target, double Value)
        {
            Target.SetValue(VerticalOffsetProperty, Value);
        }

        public static double GetVerticalOffset(FrameworkElement Target)
        {
            return (double)Target.GetValue(VerticalOffsetProperty);
        }

        private static void OnVerticalOffsetChanged(DependencyObject Target, DependencyPropertyChangedEventArgs E)
        {
            var scrollViewer = Target as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)E.NewValue);
            }
        }
    }
}