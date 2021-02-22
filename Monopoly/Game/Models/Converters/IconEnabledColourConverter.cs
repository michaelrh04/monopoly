using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Monopoly.Converters
{
    /// <summary>
    /// The IconEnabledColourConverter class converts the enabled status of any button into an enabled/disabled colour for the button's icon.
    /// </summary>
    class IconEnabledColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine(value);
            if(!(bool)value)
            {
                return new SolidColorBrush(Color.FromRgb(180, 180, 180));
            }
            return new SolidColorBrush(Colors.DarkGray);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
