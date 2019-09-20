using MahApps.Metro;
using SCME.ProfileBuilder.Properties;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;

namespace SCME.ProfileBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.CurrentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.CurrentCulture);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));
        }
    }
}
