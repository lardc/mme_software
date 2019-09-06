using SCME.Types.VTM;
using SCME.WpfControlLibrary.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class VtmTestTypeToStringConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = (VTMTestType)Value;
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

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = VTMTestType.Ramp;
            var line = (string)Value;

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
