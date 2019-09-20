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

        public static double FontCmFactor { get; set; } = 1;
        
        public static List<AppThemeWrapper> AppThemes { get; set; } = ThemeManager.AppThemes.Select(m=> new AppThemeWrapper(m)).ToList();

        public static List<AccentWrapper>  AccentColors{ get; set; } = ThemeManager.Accents.Select(m=> new AccentWrapper(m)).ToList();


        private static AccentWrapper _selectAccentWrapper;
        private static AppThemeWrapper _selectAppThemesWrapper;

        private static void ChangeAppStyle()
        {
            if (_selectAccentWrapper != null && _selectAppThemesWrapper != null)
                ThemeManager.ChangeAppStyle(Application.Current, _selectAccentWrapper.Accent, _selectAppThemesWrapper.AppTheme);
        }

        public static AccentWrapper SelectAccentWrapper
        {
            get => _selectAccentWrapper;
            set
            {
                _selectAccentWrapper = value;
                Properties.Settings.Default.Accent = _selectAccentWrapper.Name;
                ChangeAppStyle();
            }
        }

        public static AppThemeWrapper SelectAppThemesWrapper
        {
            get => _selectAppThemesWrapper;
            set
            {
                _selectAppThemesWrapper = value;
                Properties.Settings.Default.AppTheme = _selectAppThemesWrapper.Name;
                ChangeAppStyle();
            }
        } 

    }
}
