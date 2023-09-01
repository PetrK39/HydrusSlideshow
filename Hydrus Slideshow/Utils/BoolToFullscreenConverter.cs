using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Hydrus_Slideshow.Utils
{
    internal class BoolToFullscreenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolVal = (bool)value;

            if(targetType == typeof(WindowState))
            {
                return boolVal ? WindowState.Maximized : WindowState.Normal;
            }
            if(targetType == typeof(WindowStyle))
            {
                return boolVal ? WindowStyle.None : WindowStyle.SingleBorderWindow;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
