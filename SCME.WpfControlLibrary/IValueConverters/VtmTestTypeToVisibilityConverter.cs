using SCME.Types.VTM;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class VtmTestTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = (VTMTestType)Value;
            int index = int.Parse((string)Parameter);

            if ((type == VTMTestType.Ramp && index == 0) || (type == VTMTestType.Sinus && index == 1) ||
                (type == VTMTestType.Curve && index == 2))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            throw new InvalidOperationException("ConvertBack method is not implemented in VtmTestTypeToVisibilityConverter");
        }
    }
}
