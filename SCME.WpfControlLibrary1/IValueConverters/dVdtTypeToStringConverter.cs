using SCME.Types.dVdt;
using SCME.WpfControlLibrary.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class dVdtTypeToStringConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = (DvdtMode)Value;
            var line = string.Empty;

            switch (type)
            {
                case DvdtMode.Confirmation:
                    line = Resources.DvdtConfirmation;
                    break;
                case DvdtMode.Detection:
                    line = Resources.DvdtDetection;
                    break;
            }

            return line;
        }


        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var type = DvdtMode.Confirmation;
            var line = (string)Value;

            if (line == Resources.DvdtConfirmation)
                type = DvdtMode.Confirmation;
            else if (line == Resources.DvdtDetection)
                type = DvdtMode.Detection;

            return type;
        }
    }
}
