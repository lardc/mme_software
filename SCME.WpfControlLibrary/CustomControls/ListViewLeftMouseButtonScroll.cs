using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SCME.Types.Profiles;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class ListViewMouseLeftButtonScroll : ListView
    {
        private Point? _lastPoint;
        private MyProfile _lastSelectProfile;


        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if(!(e.OriginalSource is Visual visual))
                return;
            
            var obj = this.ContainerFromElement(visual);
            var element = (ListViewItem)obj;

            if(element == null)
                return;

            var item = (MyProfile)element.Content;

            if (item == _lastSelectProfile)
                SelectedItem = item;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _lastPoint = e.GetPosition(this);

            var obj = ContainerFromElement((Visual)e.OriginalSource);
            var element = (ListViewItem)obj;

            if(element == null)
                return;

            _lastSelectProfile = (MyProfile)element.Content;

            e.Handled = true;
        }

        private static TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is TChildItem item)
                    return item;

                var childOfChild = FindVisualChild<TChildItem>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var sv = FindVisualChild<ScrollViewer>(this);

            if (_lastPoint != null)
                sv.ScrollToVerticalOffset(sv.VerticalOffset + (e.GetPosition(this).Y - _lastPoint.Value.Y));
            _lastPoint = e.GetPosition(this);
        }


    }
}
