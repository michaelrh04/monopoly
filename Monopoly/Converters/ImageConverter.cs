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
    /// Used to convert any string to the source of an image.
    /// </summary>
    class LobbyImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "/Monopoly;component/Resources/Tokens/" + (string)value + ".png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
