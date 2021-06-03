using System.Windows;
using System.Windows.Controls;

namespace SCME.UI.CustomControl
{
    public partial class GridListBoxLogs
    {
        public GridListBoxLogs()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler EndOfData;

        protected void InvokeEndOfData(RoutedEventArgs Args)
        {
            if (EndOfData != null)
                EndOfData(this, Args);
        }

        private void ScrollViewer_OnScrollChanged(object Sender, ScrollChangedEventArgs E)
        {
            var scrollViewer = (ScrollViewer) Sender;

            if (E.VerticalOffset >= scrollViewer.ScrollableHeight)
                InvokeEndOfData(new RoutedEventArgs());
        }
    }
}
