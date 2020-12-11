using SCME.WpfControlLibrary.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SCME.WpfControlLibrary.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для LoadingAnimationWindow.xaml
    /// </summary>
    public partial class LoadingAnimationWindow : Window
    {
        public ProfilesPage ProfilesPage { get; set; }
        public LoadingAnimationWindow()
        {
            InitializeComponent();
            //DispatcherTimer dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Tick += DispatcherTimer_Tick;
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            //dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //if (ProfilesPage.Render)
            //    Close();
        }
    }
}
