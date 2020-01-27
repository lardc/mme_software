using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class DragGrid : Grid
    {
        private Point _startPosition;
        private bool _pressDown = false;
        
        public DragGrid() : base()
        {
            MouseDown += (sender, e) =>
            {
                _pressDown = true;
                _startPosition = e.GetPosition((IInputElement)sender);
            };

            MouseMove += (sender, e) =>
            {
                if (_pressDown == false)
                    return;

                if (ShouldStartDrag(e) == false)
                    return;

                var grid = (Grid) sender;
                _pressDown = false;
                DragDrop.DoDragDrop(grid, grid.DataContext, DragDropEffects.Move);
            };

            MouseUp += (sender, e) => _pressDown = false;
        }
        
        
        
        private bool ShouldStartDrag(MouseEventArgs e)
        {
            var curPos = e.GetPosition(null);
            return
                Math.Abs(curPos.Y-_startPosition.Y) > SystemParameters.MinimumVerticalDragDistance ||
                Math.Abs(curPos.X-_startPosition.X) > SystemParameters.MinimumHorizontalDragDistance;
        }
    }
}