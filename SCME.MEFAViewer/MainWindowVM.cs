using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SCME.MEFAViewer.Annotations;

namespace SCME.MEFAViewer
{
    
    public class MainWindowVm: INotifyPropertyChanged
    {
        public ObservableCollection<MmeTile> MmeTiles { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}