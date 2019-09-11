using System;
using System.Globalization;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class FrequencyDivisorToFrequency : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            return (50 / (ushort)Value).ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            return (ushort)(50 / UInt16.Parse((string)Value));
        }
    }
}
