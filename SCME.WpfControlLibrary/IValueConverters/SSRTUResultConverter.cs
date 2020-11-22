using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SCME.Types.VTM;
using SCME.WpfControlLibrary.Properties;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class SSRTUResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double?)value;
            if (val == null)
                return "-";
            else
            {
                if (val == 0)
                    return 0.ToString();
                return val.Value.ToString().TrimEnd('0');
            }
        }


        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}