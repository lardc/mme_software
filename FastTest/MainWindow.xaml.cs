using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SCME.WpfControlLibrary;

namespace FastTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

       
        public MainWindow()
        {
            ResourceBinding.Scaling(0.75);
            //Entities DB = new Entities();

            try
            {
              //  var profilesByMME = DB.MME_CODES.Include(m=> m.MME_CODES_TO_PROFILES).Single(m => m.Name == "MME008").MME_CODES_TO_PROFILES.Select(m => m.PROFILE).GroupBy(m => m.PROF_NAME);
//                var highVersionProfiles = profilesByMME.Select(m => m.OrderByDescending(n => n.PROF_VERS).First()).ToList();


            }
            catch(Exception ex)
            {

            }
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
