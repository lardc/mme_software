using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using SCME.MEFAViewer.Annotations;

namespace SCME.MEFAViewer
{
    public class MmeTile: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Brush Color { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}