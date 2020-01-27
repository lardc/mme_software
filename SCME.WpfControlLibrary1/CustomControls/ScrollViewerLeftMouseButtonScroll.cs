using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class ScrollViewerLeftMouseButtonScroll : ScrollViewer
    {
        private Point? _lastPoint;

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _lastPoint = e.GetPosition(this);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (_lastPoint != null)
                ScrollToVerticalOffset(VerticalOffset - (e.GetPosition(this).Y - _lastPoint.Value.Y));
            _lastPoint = e.GetPosition(this);
        }
    }
}
