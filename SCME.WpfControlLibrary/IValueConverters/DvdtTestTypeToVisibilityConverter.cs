using SCME.Types.dVdt;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class DvdtTestTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = (DvdtMode)Value;

            if (type == DvdtMode.Confirmation)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            throw new InvalidOperationException("ConvertBack method is not implemented in VtmTestTypeToVisibilityConverter");
        }
    }
}
