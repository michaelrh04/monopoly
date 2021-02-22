using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Converters
{
    /// <summary>
    /// Use to provide values for property transition around the board upon a rotation request
    /// </summary>
    enum BoardRotateMode
    {
        /// <summary>
        /// 0 degrees of rotation.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 90 degrees of rotation (once rotated).
        /// </summary>
        Once = 10,
        /// <summary>
        /// 180 degrees of rotation (twice rotated).
        /// </summary>
        Twice = 20,
        /// <summary>
        /// 270 degrees of rotation (thrice rotated).
        /// </summary>
        Thrice = 30
    }
}
