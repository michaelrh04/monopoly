using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Monopoly.Menu
{
    /// <summary>
    /// Allows for the compilation of multiple objects into one single parameter, used for compiling (e.g.) the results of multiple components into one parameter for the ViewModel.
    /// This does NOT handle the parameters - the recieving commmand MUST know the format of the items it is recieving!
    /// </summary>
    class ParameterCompilerConverter : IMultiValueConverter
    {
        /// <summary>
        /// Package (clone) the items and return them.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Source: StackOverflow
            // https://stackoverflow.com/questions/1350598/passing-two-command-parameters-using-a-wpf-binding
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
