using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Resources;
using System.Windows.Media.Imaging;

namespace Monopoly
{
    /// <summary>
    /// Returns true if all parameters are equal to one another.
    /// </summary>
    class EqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            bool all_equal = true;
            for(int i = 1; i < value.Length; i++)
            {
                if(value[i] != value[i - 1])
                {
                    all_equal = false;
                }
            }
            return all_equal;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
