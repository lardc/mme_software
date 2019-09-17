using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class NumericUpDown : MahApps.Metro.Controls.NumericUpDown
    {
        public NumericUpDown()
        {
            Loaded += NumericUpDown_Loaded;
        }


        private void SetSizeUp()
        {
            RepeatButton repeatButtonUp = VisualHelper.FindChild<RepeatButton>(this, "PART_NumericUp");
            Path pathUp = VisualHelper.FindChild<Path>(this, "PolygonUp");
 
            pathUp.Stretch = Stretch.UniformToFill;

            repeatButtonUp.SetBinding(MarginProperty, new Binding()
            {
                Source = new Thickness(ResourceBinding.RepeatButtonUpDownMarginLeft, 0, 0, 0)
            });

            repeatButtonUp.SetBinding(WidthProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonUpSize
            });
            repeatButtonUp.SetBinding(HeightProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonUpSize
            });

            pathUp.SetBinding(WidthProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonUpSize
            });
            pathUp.SetBinding(HeightProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonUpSize
            });
        }

        private void SetSizeDown()
        {
            RepeatButton repeatButtonUp = VisualHelper.FindChild<RepeatButton>(this, "PART_NumericDown");
            Path pathUp = VisualHelper.FindChild<Path>(this, "PolygonDown");

            pathUp.Stretch = Stretch.UniformToFill;

            repeatButtonUp.SetBinding(MarginProperty, new Binding()
            {
               Source = new Thickness(ResourceBinding.RepeatButtonUpDownMarginLeft, 0, 0, 0)
            });

            repeatButtonUp.SetBinding(WidthProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonUpSize
            });
            repeatButtonUp.SetBinding(HeightProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonUpSize
            });

            pathUp.SetBinding(WidthProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonUpSize
            });
            pathUp.SetBinding(HeightProperty, new Binding()
            {
                Source = ResourceBinding.RepeatButtonDownSize
            });
        }

        private void NumericUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            SetSizeUp();
            SetSizeDown();
            //RepeatButton repeatButtonUp = VisualHelper.FindChild<RepeatButton>(this, "PART_NumericUp");
            //repeatButtonUp.SetBinding(WidthProperty, new Binding()
            //{
            //    Source = ResourceBinding.RepeatButtonUpDownSize
            //});
            //repeatButtonUp.SetBinding(HeightProperty, new Binding()
            //{
            //    Source = ResourceBinding.RepeatButtonUpDownSize
            //});

            //Path pathUp = VisualHelper.FindChild<Path>(this, "PolygonUp");
            //pathUp.SetBinding(WidthProperty, new Binding()
            //{
            //    Source = ResourceBinding.RepeatButtonUpDownSize
            //});
            //pathUp.SetBinding(HeightProperty, new Binding()
            //{
            //    Source = ResourceBinding.RepeatButtonUpDownSize
            //});
        }
    }
}
