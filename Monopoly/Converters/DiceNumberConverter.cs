using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monopoly
{
    class DiceNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch(value)
            {
                default:
                    return PackIconFontAwesomeKind.QuestionCircleRegular;
                case 1:
                    return PackIconFontAwesomeKind.DiceOneSolid;
                case 2:
                    return PackIconFontAwesomeKind.DiceTwoSolid;
                case 3:
                    return PackIconFontAwesomeKind.DiceThreeSolid;
                case 4:
                    return PackIconFontAwesomeKind.DiceFourSolid;
                case 5:
                    return PackIconFontAwesomeKind.DiceFiveSolid;
                case 6:
                    return PackIconFontAwesomeKind.DiceSixSolid;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
