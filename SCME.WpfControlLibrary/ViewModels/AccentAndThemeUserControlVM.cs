using MahApps.Metro;
using PropertyChanged;
using SCME.WpfControlLibrary.Wrappers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public static class AccentAndThemeUserControlVM
    {
        static AccentAndThemeUserControlVM()
        {
            SelectAccentWrapper = AccentColors.Single(m => m.Name == Properties.Settings.Default.Accent);
            SelectAppThemesWrapper = AppThemes.Single(m=> m.Name == Properties.Settings.Default.AppTheme);
        }

        public static List<AccentWrapper> AccentColors { get; set; } = ThemeManager.Accents.Select(m => new AccentWrapper(m)).ToList();

        public static List<AppThemeWrapper> AppThemes { get; set; } = ThemeManager.AppThemes.Select(m => new AppThemeWrapper(m)).ToList();


        private static AccentWrapper _SelectAccentWrapper;
        private static AppThemeWrapper _SelectAppThemesWrapper;

        private static void ChangeAppStyle()
        {
            if(_SelectAccentWrapper != null && _SelectAppThemesWrapper != null)
                ThemeManager.ChangeAppStyle(Application.Current, _SelectAccentWrapper.Accent, _SelectAppThemesWrapper.AppTheme);
        }

        public static AccentWrapper SelectAccentWrapper
        {
            get => _SelectAccentWrapper;
            set
            {
                _SelectAccentWrapper = value;
                Properties.Settings.Default.Accent = _SelectAccentWrapper.Name;
                ChangeAppStyle();
            }
        }

        public static AppThemeWrapper SelectAppThemesWrapper
        {
            get => _SelectAppThemesWrapper;
            set
            {
                _SelectAppThemesWrapper = value;
                Properties.Settings.Default.AppTheme = _SelectAppThemesWrapper.Name;
                ChangeAppStyle();
            }
        } 

    }
}
