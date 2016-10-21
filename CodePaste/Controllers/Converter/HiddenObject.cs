using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CodePaste.Controllers.Converter
{
    /// <summary>
    /// Return true only when owner type and value type match
    /// </summary>
    class HiddenObject : IValueConverter
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
