using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SCME.ProfileBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);

            // now set the Green accent and dark theme
            //ThemeManager.ChangeAppStyle(Current,
            //                            ThemeManager.GetAccent(ProfileBuilder.Properties.Settings.Default.Accent),
            //                            ThemeManager.GetAppTheme(ProfileBuilder.Properties.Settings.Default.Theme));
        }
    }
}
