using SCME.Types.BVT;
using SCME.WpfControlLibrary.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class BvtTypeToStringConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = (BVTTestType)Value;
            var line = string.Empty;

            switch (type)
            {
                case BVTTestType.Both:
                    line = Resources.Both;
                    break;
                case BVTTestType.Direct:
                    line = Resources.Direct;
                    break;
                case BVTTestType.Reverse:
                    line = Resources.Reverse;
                    break;
            }

            return line;
        }


        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = BVTTestType.Both;
            var line = (string)Value;

            if (line == Resources.Both)
                type = BVTTestType.Both;
            else if (line == Resources.Direct)
                type = BVTTestType.Direct;
            else if (line == Resources.Reverse)
                type = BVTTestType.Reverse;

            return type;
        }
    }
}
