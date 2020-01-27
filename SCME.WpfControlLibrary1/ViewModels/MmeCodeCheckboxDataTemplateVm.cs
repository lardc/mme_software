using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MmeCodeCheckboxDataTemplateVm
    {
        public string Name { get; set; }
        public bool IsCheckedNewValue { get; set; }
        public bool IsCheckedOldValue { get; set; }
    }
}
