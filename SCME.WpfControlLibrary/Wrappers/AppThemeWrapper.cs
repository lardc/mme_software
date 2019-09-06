using MahApps.Metro;
using System.Windows;
using System.Windows.Media;

namespace SCME.WpfControlLibrary.Wrappers
{
    public class AppThemeWrapper
    {
        public AppTheme AppTheme { get; private set; }
        public string Name => AppTheme.Name;
        public Brush Color => (AppTheme.Resources as ResourceDictionary)["WindowBackgroundBrush"] as SolidColorBrush;
        public AppThemeWrapper(AppTheme appTheme)
        {
            AppTheme = appTheme;
        }
    }
}
