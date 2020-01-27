using SCME.Types.BVT;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class BvtTestTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var en = (BVTTestType)Value;
            int index = int.Parse((string)Parameter);

            return en == BVTTestType.Both ||
                   en == BVTTestType.Direct && index == (int)BVTTestType.Direct ||
                   en == BVTTestType.Reverse && index == (int)BVTTestType.Reverse
                       ? Visibility.Visible
                       : Visibility.Collapsed;
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            throw new InvalidOperationException("ConvertBack method is not implemented in BvtTestTypeToVisibilityConverter");
        }
    }
}
