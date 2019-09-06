using MahApps.Metro;
using PropertyChanged;
using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.ProfileBuilder.ViewModels.ConnectPage
{
    [AddINotifyPropertyChangedInterface]
    public class ConnectPageVM
    {
        public ConnectToMSSQLVM ConnectToMSSQLVM { get; set; } = new ConnectToMSSQLVM();
        public ConnectToSQLiteVM ConnectToSQLiteVM { get; set; } = new ConnectToSQLiteVM();
    }
}
