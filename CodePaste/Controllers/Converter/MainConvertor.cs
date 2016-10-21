using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace CodePaste.Controllers.Converter
{
    class MainConverter : IValueConverter
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
