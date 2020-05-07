using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml;
using SCME.UI.CustomControl;
using SCME.UI.Properties;

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
            UIServiceConfig.Settings.LoadSettings(true);

            if(UIServiceConfig.Properties.Settings.Default.DebugUpdate)
            {
                try
                {
                    File.WriteAllText("Version info.txt", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                }
                catch(Exception ex)
                {
                    File.WriteAllText("Debug update.txt", ex.ToString());
                }
            }

            
            SCME.UI.Properties.Resources.Culture = new CultureInfo(UIServiceConfig.Properties.Settings.Default.Localization);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(UIServiceConfig.Properties.Settings.Default.Localization);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(UIServiceConfig.Properties.Settings.Default.Localization);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            
            base.OnStartup(e);
        }

        private static void Current_DispatcherUnhandledException(object Sender, DispatcherUnhandledExceptionEventArgs E)
        {
            File.WriteAllText("error.txt", E.Exception.ToString());
            System.IO.File.WriteAllText("Critical error UI.txt", E.Exception.ToString());
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