using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SCME.ProfileBuilder
{
    public class AccentWrapper
    {
        private Accent _Accent;
        public string Name => _Accent.Name;
        public Brush Color => new  SolidColorBrush( (Color)(_Accent.Resources as ResourceDictionary)["AccentColor"]);
        public AccentWrapper(Accent accent)
        {
            _Accent = accent;
        }
    }
}
