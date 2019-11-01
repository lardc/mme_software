using MahApps.Metro;
using System.Windows;
using System.Windows.Media;

namespace SCME.WpfControlLibrary.Wrappers
{
    public class AccentWrapper
    {
        public Accent Accent { get; private set; }
        public string Name => Accent.Name;
        public Brush Color => new SolidColorBrush((Color)(Accent.Resources as ResourceDictionary)["AccentColor"]);
        public AccentWrapper(Accent accent)
        {
            Accent = accent;
        }
    }
}
