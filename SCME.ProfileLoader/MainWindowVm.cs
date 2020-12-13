using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SCME.ProfileLoader
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")  => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool CommaIsDelimiter { get; set; } = true;
        public ObservableCollection<string> MmeCodes { get; set; }
    }
}
