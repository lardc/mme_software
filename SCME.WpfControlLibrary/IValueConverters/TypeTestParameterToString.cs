using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SCME.WpfControlLibrary.DataProviders;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class TypeTestParameterToString : IValueConverter
    {
        private static readonly Dictionary<Type, string> TypesToString = TestTypeEnumDictionary.GetTestParametersTypes().ToDictionary(m => m.Type, m => m.Name);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return TypesToString[value.GetType()];
            throw new ArgumentNullException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}