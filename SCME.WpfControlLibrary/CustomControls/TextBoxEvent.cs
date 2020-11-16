using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SCME.WpfControlLibrary.CustomControls
{
    public partial class TextBoxEvent : ResourceDictionary
    {

        private void NumericUpDown_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.IsTouchUI && FindParent<Window>(sender as System.Windows.Controls.Control) is IMainWindow window)
                window.ShowKeyboard(true, sender as System.Windows.Controls.Control);
            //throw new System.NotImplementedException();
        }

        private void NumericUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.IsTouchUI && FindParent<Window>(sender as System.Windows.Controls.Control) is IMainWindow window)
                window.ShowKeyboard(false, sender as System.Windows.Controls.Control);
            //throw new System.NotImplementedException();
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
}
