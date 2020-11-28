using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    class VisiblyEnabledToVisibly : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)values[0])
                return Visibility.Visible;
            else
            {
                if ((bool)values[1])
                    return Visibility.Visible;
                else 
                    return Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
