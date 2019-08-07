using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.UI.ModelViews
{
    public class TOUPageVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Types.TOU.TestParameters Input { get; set; } = new Types.TOU.TestParameters();

        public Types.TOU.TestResults Output { get; set; } = new Types.TOU.TestResults();
    }
}
