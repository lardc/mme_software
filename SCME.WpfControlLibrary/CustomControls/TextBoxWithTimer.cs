using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class TextBoxWithTimer : TextBox
    {
        private readonly DispatcherTimer _dispatcherTimerFindProfile = new DispatcherTimer();

        public event EventHandler Tick
        {
            add =>
                _dispatcherTimerFindProfile.Tick += (sender, args) =>
                {
                    _dispatcherTimerFindProfile.Stop();
                    value(sender, args);
                };

            remove => _dispatcherTimerFindProfile.Tick -= value;
        }


        public double Interval
        {
            get => _dispatcherTimerFindProfile.Interval.TotalMilliseconds;
            set => _dispatcherTimerFindProfile.Interval = TimeSpan.FromMilliseconds(value);
        }

        public TextBoxWithTimer() : base()
        {
            TextChanged += (sender, args) => _dispatcherTimerFindProfile.Start();
        }
    }
}