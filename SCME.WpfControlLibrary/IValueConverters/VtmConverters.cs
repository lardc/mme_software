using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SCME.Types.VTM;
using SCME.WpfControlLibrary.Properties;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class VtmConverters
    {
        public class VtmTestTypeToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var type = (VTMTestType)value;
                int index = int.Parse((string)parameter);

                if ((type == VTMTestType.Ramp && index == 0) || (type == VTMTestType.Sinus && index == 1) ||
                    (type == VTMTestType.Curve && index == 2))
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new InvalidOperationException("ConvertBack method is not implemented in VtmTestTypeToVisibilityConverter");
            }
        }
        
        public class VtmTestTypeToStringConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var type = (VTMTestType)value;
                var line = string.Empty;

                switch (type)
                {
                
                    case VTMTestType.Ramp:
                        line = Resources.Ramp;
                        break;
                    case VTMTestType.Sinus:
                        line = Resources.Sinus;
                        break;
                    case VTMTestType.Curve:
                        line = Resources.Curve;
                        break;
                }

                return line;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var type = VTMTestType.Ramp;
                var line = (string)value;

                if (line == Resources.Ramp)
                    type = VTMTestType.Ramp;
                else if (line == Resources.Sinus)
                    type = VTMTestType.Sinus;
                else if (line == Resources.Curve)
                    type = VTMTestType.Curve;

                return type;
            }
        }
    }
}