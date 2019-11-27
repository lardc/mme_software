using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using SCME.UI.CustomControl;

namespace SCME.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(SCME.UI.Properties.Settings.Default.Localization);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(SCME.UI.Properties.Settings.Default.Localization);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            base.OnStartup(e);
        }

        private static void Current_DispatcherUnhandledException(object Sender, DispatcherUnhandledExceptionEventArgs E)
        {
            var dw = new DialogWindow("Application crashed!",
                string.Format("{0}\n{1}", E.Exception.Message, E.Exception))
            {
                Top = SystemParameters.WorkArea.Top,
                Left = SystemParameters.WorkArea.Left,
                Width = SystemParameters.WorkArea.Width,
                Height = SystemParameters.WorkArea.Height,
            };

            dw.ButtonConfig(DialogWindow.EbConfig.OK);
            dw.ShowDialog();
        }
    }
}