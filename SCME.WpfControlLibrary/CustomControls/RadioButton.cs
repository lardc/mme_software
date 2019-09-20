using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class RadioButton : System.Windows.Controls.RadioButton
    {
        public RadioButton()
        {
            Loaded+=OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        
        }
    }
}