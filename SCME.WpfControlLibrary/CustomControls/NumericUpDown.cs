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
            var repeatButtonUp = VisualHelper.FindChild<RepeatButton>(this, "PART_NumericUp");
            repeatButtonUp.SetResourceReference(MarginProperty, "SCME.RepeatButtonUpDownMarginLeft");
            repeatButtonUp.SetResourceReference(WidthProperty, "SCME.RepeatButtonUpSize");
            repeatButtonUp.SetResourceReference(HeightProperty, "SCME.RepeatButtonUpSize");
            
            var pathUp = VisualHelper.FindChild<Path>(this, "PolygonUp");
            pathUp.Stretch = Stretch.UniformToFill;
            pathUp.SetResourceReference(WidthProperty, "SCME.RepeatButtonUpSize");
            pathUp.SetResourceReference(HeightProperty, "SCME.RepeatButtonUpSize");
        }

        private void SetSizeDown()
        {
            var repeatButtonDown = VisualHelper.FindChild<RepeatButton>(this, "PART_NumericDown");
            repeatButtonDown.SetResourceReference(MarginProperty, "SCME.RepeatButtonUpDownMarginLeft");
            repeatButtonDown.SetResourceReference(WidthProperty, "SCME.RepeatButtonUpSize");
            repeatButtonDown.SetResourceReference(HeightProperty, "SCME.RepeatButtonUpSize");
            
            var pathDown = VisualHelper.FindChild<Path>(this, "PolygonDown");
            pathDown.Stretch = Stretch.UniformToFill;
            pathDown.SetResourceReference(WidthProperty, "SCME.RepeatButtonUpSize");
            pathDown.SetResourceReference(HeightProperty, "SCME.RepeatButtonDownSize");
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
