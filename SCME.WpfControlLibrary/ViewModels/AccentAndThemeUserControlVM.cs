using System;
using MahApps.Metro;
using PropertyChanged;
using SCME.WpfControlLibrary.Wrappers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class AccentAndThemeUserControlVM
    {
        public AccentAndThemeUserControlVM()
        {
            
            AppThemes= ThemeManager.AppThemes.Select(m=> new AppThemeWrapper(m)).ToList();
            AccentColors = ThemeManager.Accents.Select(m=> new AccentWrapper(m)).ToList();
            SelectAccentWrapper = AccentColors.SingleOrDefault(m => m.Name == Properties.Settings.Default.Accent) ?? AccentColors.First();
            SelectAppThemesWrapper = AppThemes.SingleOrDefault(m=> m.Name == Properties.Settings.Default.AppTheme) ?? AppThemes.First();
        }

        
        public List<AppThemeWrapper> AppThemes { get; }

        public List<AccentWrapper>  AccentColors{ get; }


        private AccentWrapper _selectAccentWrapper;
        private AppThemeWrapper _selectAppThemesWrapper;

        private void ChangeAppStyle()
        {
            if (_selectAccentWrapper != null && _selectAppThemesWrapper != null)
                ThemeManager.ChangeAppStyle(Application.Current, _selectAccentWrapper.Accent, _selectAppThemesWrapper.AppTheme);
        }
        
        public double FontScaling
        {
            get => Properties.Settings.Default.FontScaling;
            set
            {
                Properties.Settings.Default.FontScaling = value;
                ResourceBinding.Scaling(value / 100.0);
            }
        }

        public AccentWrapper SelectAccentWrapper
        {
            get => _selectAccentWrapper;
            set
            {
                _selectAccentWrapper = value;
                Properties.Settings.Default.Accent = _selectAccentWrapper.Name;
                ChangeAppStyle();
            }
        }

        public AppThemeWrapper SelectAppThemesWrapper
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
