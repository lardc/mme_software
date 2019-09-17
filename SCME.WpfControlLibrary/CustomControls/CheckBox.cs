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
            Border borderNormal = VisualHelper.FindChild<Border>(this, "normal");
            borderNormal.SetBinding(WidthProperty, new Binding()
            {
                Source = ResourceBinding.CheckBoxBorderSize
            });
            borderNormal.SetBinding(HeightProperty, new Binding()
            {
                Source = ResourceBinding.CheckBoxBorderSize
            });

            Border borderDisabled = VisualHelper.FindChild<Border>(this, "disabled");
            borderDisabled.SetBinding(WidthProperty, new Binding()
            {
                Source = ResourceBinding.CheckBoxBorderSize
            });
            borderDisabled.SetBinding(HeightProperty, new Binding()
            {
                Source = ResourceBinding.CheckBoxBorderSize
            });

            Path path = VisualHelper.FindChild<Path>(this, "checkBox");
            path.SetBinding(WidthProperty, new Binding()
            {
                Source = ResourceBinding.CheckBoxPathWidth
            });
            path.SetBinding(HeightProperty, new Binding()
            {
                Source = ResourceBinding.CheckBoxPathHeight
            });


            Grid grid = (borderNormal.Parent as Grid).Parent as Grid;
            grid.ColumnDefinitions[0].Width = new System.Windows.GridLength();
        }
    }
}
