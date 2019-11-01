using SCME.Types.QrrTq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class TDcFallRateValueFromEnum : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            ushort value = (ushort)((TDcFallRate)Value);

            return value.ToString();
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            return Enum.Parse(TargetType, (string)Value);
        }
    }
}
