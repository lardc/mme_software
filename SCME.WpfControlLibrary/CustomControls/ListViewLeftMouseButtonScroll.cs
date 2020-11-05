using System;
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
        private Point _pointMouseDown;
        private object _lastSelectedItem;


        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            try
            {
                var point = e.GetPosition(this);

                if (!(e.OriginalSource is Visual visual))
                    return;

                var obj = this.ContainerFromElement(visual);
                var element = (ListViewItem)obj;

                if (element == null)
                    return;

                var item = element.Content;

                if (item == _lastSelectedItem && Math.Pow(_pointMouseDown.X - point.X, 2) + Math.Pow(_pointMouseDown.Y - point.Y, 2) < 400)
                    SelectedItem = item;
            }
            catch
            {
                // ignored
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            try
            {
                _lastPoint = _pointMouseDown = e.GetPosition(this);

                if (!(e.OriginalSource is Visual visual))
                    return;

                var obj = ContainerFromElement(visual);
                var element = (ListViewItem)obj;

                if (element == null)
                    return;

                _lastSelectedItem = element.Content;

                e.Handled = true;
            }
            catch
            {
                // ignored
            }
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
            try
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    return;

                var sv = FindVisualChild<ScrollViewer>(this);

                if (_lastPoint != null)
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - (e.GetPosition(this).Y - _lastPoint.Value.Y));
                _lastPoint = e.GetPosition(this);
            }
            catch
            {
                // ignored
            }
        }


    }
}
