using System;
using System.Globalization;
using System.Windows.Data;

namespace CodePaste.Controllers.Converter
{
    /// <summary>
    /// Return true only when owner type and value type match
    /// </summary>
    internal class HiddenObject : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }

        public object ConvertBack(object value, Type targetType,
      object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            return value;
        }
    }
}