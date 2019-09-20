using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class CheckBox : System.Windows.Controls.CheckBox
    {
        public CheckBox()
        {
            Loaded += CheckBox_Loaded;
        }

        private void CheckBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var borderNormal = VisualHelper.FindChild<Border>(this, "normal");
            borderNormal.SetResourceReference(WidthProperty, "SCME.CheckBoxBorderSize");
            borderNormal.SetResourceReference(HeightProperty, "SCME.CheckBoxBorderSize");

            var borderDisabled = VisualHelper.FindChild<Border>(this, "disabled");
            borderDisabled.SetResourceReference(WidthProperty, "SCME.CheckBoxBorderSize");
            borderDisabled.SetResourceReference(HeightProperty, "SCME.CheckBoxBorderSize");

            var path = VisualHelper.FindChild<Path>(this, "checkBox");
            path.SetResourceReference(WidthProperty, "SCME.CheckBoxPathWidth");
            path.SetResourceReference(HeightProperty, "SCME.CheckBoxPathHeight");

            if ((borderNormal.Parent as Grid)?.Parent is Grid grid) 
                grid.ColumnDefinitions[0].Width = new System.Windows.GridLength();
        }
    }
}
