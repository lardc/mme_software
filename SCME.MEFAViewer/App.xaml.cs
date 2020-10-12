using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace SCME.MEFAViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AppSettings AppSettings { get; private set; }


        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText("appsettings.json"));
        }
    }
}