using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class TextBoxWithTimer : TextBox
    {
        private readonly DispatcherTimer _dispatcherTimerFindProfile = new DispatcherTimer();
        public EventHandler Tick { get; set; }
        
        public double Interval
        {
            get => _dispatcherTimerFindProfile.Interval.TotalMilliseconds;
            set => _dispatcherTimerFindProfile.Interval = TimeSpan.FromMilliseconds(value);
        }

        public TextBoxWithTimer() : base()
        {
            _dispatcherTimerFindProfile.Tick += OnDispatcherTimerOnTick;
            TextChanged += (sender, args) => _dispatcherTimerFindProfile.Start();
        }

        private void OnDispatcherTimerOnTick(object sender, EventArgs e)
        {
            _dispatcherTimerFindProfile.Stop();
            Tick?.Invoke(sender, e);
        }
    }
}