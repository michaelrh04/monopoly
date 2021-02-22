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
    /// A class to allow for the conversion of utility strings ("bulb" or "tap") to their respective symbols.
    /// </summary>
    class UtilitySymbolConverter : IValueConverter
    {
        /// <summary>
        /// The converter for converting a string into a PackIconFontAwesomeKind.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>If value is "bulb", returns the bulb PackIconFontAwesomeKind. If value is "tap", returns the respective tap kind. Else, returns a map marker.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((string)value == "Bulb")
            {
                return PackIconFontAwesomeKind.LightbulbRegular;
            } 
            else if ((string)value == "Tap")
            {
                return PackIconFontAwesomeKind.FaucetSolid;
            } else
            {
                return PackIconFontAwesomeKind.MapMarkerAltSolid;
            }
            // This could be extended to allow for further icons later on.
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
