using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Management.Converters
{
    public class IntegerToUsCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int amount = (int)value;
            var info = CultureInfo.GetCultureInfo("en-US");
            var result = amount.ToString("C", info);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


