﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SCME.WpfControlLibrary.DataProviders;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class TestParametrToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TestParameterDictionary.TestParametersList.Single(m => m.Type == value.GetType()).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}