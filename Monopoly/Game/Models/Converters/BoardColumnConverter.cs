using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monopoly.Converters
{
    class BoardColumnConverter : IValueConverter
    {
        public static BoardRotateMode BoardRotation = BoardRotateMode.Normal;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int Position = (int)value;
            // Adjust if necessary for board rotation
            Position -= (int)BoardRotation;
            if (Position < 0) { Position += 40; }
            // Only properties 0 through 9 and 20 through 29 are explicitly affected by changes in columns.
            if (Position < 10)
            {
                return (10 - Position);
            }
            else if (Position > 9 && Position < 20)
            {
                return 0;
            }
            else if (Position > 19 && Position < 30)
            {
                return (Position - 20);
            } else
            {
                return 10;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
