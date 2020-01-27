using System;
using System.Globalization;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class FrequencyDivisorToFrequency : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (50 / System.Convert.ToUInt16(value)).ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ushort)(50 / System.Convert.ToUInt16(value));
        }
    }
}
