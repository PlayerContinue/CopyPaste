using System;
using System.Globalization;
using System.Windows.Data;

namespace CodePaste.Controllers.Converter
{
    internal class MainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType,
      object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            return value;
        }
    }
}