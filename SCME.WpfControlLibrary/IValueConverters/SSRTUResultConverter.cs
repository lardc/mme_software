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
        public int Round { get; set; } = 3;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double?)value;
            if (val == null)
                return "n/a";
            else
            {
                if(val == double.Epsilon)
                    return "-"; 
                if (val == 0)
                    return 0.ToString();
                return Math.Round(val.Value, Round).ToString().TrimEnd('0');
            }
        }


        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}