using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.ProfileBuilder.ViewModels.ConnectPage
{
    [AddINotifyPropertyChangedInterface]
    public class ConnectToSQLiteVM
    {
        public string SQLiteFileName { get => Properties.Settings.Default.SQLiteFileName; set => Properties.Settings.Default.SQLiteFileName = value; }

        [DependsOn(nameof(SQLiteFileName))]
        public bool ConnectedButtonEnabled
        {
            get 
            {
                return File.Exists(SQLiteFileName);
            }
        }
    }
}
