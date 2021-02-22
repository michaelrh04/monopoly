using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monopoly.Converters
{
    /// <summary>
    /// A class to allow for the conversion of objects to a true or false value, depending on whether or not they equal null.
    /// </summary>
    class IsVisualPropertyNullConverter : IValueConverter
    {
        /// <summary>
        /// The converter for checking if a bound value is null.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>If value is null, returns true. If value is not null, returns false.</returns>
        public static bool ForceTrueResult { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(ForceTrueResult) { return true; }
            if(value == null)
            {
                return true;
            } 
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
