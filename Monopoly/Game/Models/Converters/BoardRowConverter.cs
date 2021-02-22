using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monopoly.Converters
{
    class BoardRowConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int Position = (int)value;
            // Adjust if necessary for board rotation
            // To save necessary memory, we can just use the static value from the other class
            Position -= (int)BoardColumnConverter.BoardRotation;
            if (Position < 0) { Position += 40; }
            // Only properties 10 through 19 and 30 through 39 are explicitly affected by changes in columns.
            if (Position < 10)
            {
                return 10;
            }
            else if (Position > 9 && Position < 20)
            {
                return 10 - (Position - 10);
            }
            else if (Position > 19 && Position < 30)
            {
                return 0;
            }
            else
            {
                return Position - 30;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
