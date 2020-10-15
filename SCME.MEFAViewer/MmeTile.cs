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

        public string SoftVersion { get; set; }
        public DateTime? LastStartTimestamp { get; set; }
        public long? TestCounterTotal { get; set; }
        public long? HardwareErrorCounterTotal { get; set; }
        public DateTime? LastTestTimestamp { get; set; }
        public DateTime? LastSWUpdateTimestamp { get; set; }
        public long? ActiveProfilesCount { get; set; }
        public long? TestCounter { get; set; }
        public long? HardwareErrorCounter { get; set; }
        
        public DateTime TestCounterBeginDateTime { get; set; } =  DateTime.Now - new TimeSpan(7, 0, 0, 0);
        public DateTime TestCounterEndDateTime { get; set; } = DateTime.Now;
        
        public DateTime HardwareErrorCounterBeginDateTime { get; set; }= DateTime.Now - new TimeSpan(7, 0, 0, 0);
        public DateTime HardwareErrorCounterEndDateTime { get; set; } =DateTime.Now;

        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}