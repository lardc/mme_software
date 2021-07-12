using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using SCME.MEFAViewer.Annotations;

namespace SCME.MEFAViewer
{
    public class MmeTile: INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Brush Color { get; set; }

        
        public DateTime? LastStartTimestamp { get; set; }
        
        public string SWVersionAtLastStart { get; set; }
        
        public long? TestCounterSinceLastStart { get; set; }
        public long? TestCounterTotal { get; set; }
        public long? TestCounter { get; set; }
        public DateTime TestCounterBeginDateTime { get; set; } = DateTime.Now.AddDays(-7);
        public DateTime TestCounterEndDateTime { get; set; } = DateTime.Now;
        

        public TimeSpan? WorkingHoursSinceLastStart { get; set; }
        public TimeSpan? WorkingHoursTotal { get; set; }
        public TimeSpan? WorkingHours { get; set; }
        public DateTime WorkingHoursBeginDateTime { get; set; } = DateTime.Now.AddDays(-7);
        public DateTime WorkingHoursEndDateTime { get; set; } = DateTime.Now;
        
        
        public long? HardwareErrorCounterTotal { get; set; }
        public long? HardwareErrorCounter { get; set; }
        public DateTime HardwareErrorCounterBeginDateTime { get; set; }= DateTime.Now.AddDays(-7);
        public DateTime HardwareErrorCounterEndDateTime { get; set; } =DateTime.Now;
        
        public DateTime? LastTestTimestamp { get; set; }
        public string LastState { get; set; }
        
        public long? ActiveProfilesCount { get; set; }
        
        public DateTime? LastSwUpdateTimestamp { get; set; }
        
        
        
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}