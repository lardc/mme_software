using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class DoobleMaxLength : IValueConverter
    {
        public int Round { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = Math.Round((double)value, Round).ToString(CultureInfo.InvariantCulture);
            val.TrimEnd('0');
            val.TrimEnd(',');
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = Math.Round((double)value, Round).ToString(CultureInfo.InvariantCulture);
            val.TrimEnd('0');
            val.TrimEnd(',');
            return double.Parse(val.Replace('.',','));
        }
    }
}
