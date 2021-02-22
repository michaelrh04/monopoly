using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    /// <summary>
    /// When using dummies for visual effect, the type of dummy can be specified using DummyType.
    /// </summary>
    public enum DummyType
    {
        #region For use within the View of the Monopoly game
        /// <summary>
        /// Type representing a full set ownership.
        /// </summary>
        SetUnowned,
        /// <summary>
        /// Type representing unownership of a full set.
        /// </summary>
        SetOwned,
        /// <summary>
        /// Type representing an unbuilt house.
        /// </summary>
        HouseUnowned, 
        /// <summary>
        /// Type representing a built house.
        /// </summary>
        HouseOwned,
        /// <summary>
        /// Type representing an unbuilt hotel.
        /// </summary>
        HotelUnowned,
        /// <summary>
        /// Type representing a built hotel.
        /// </summary>
        HotelOwned,
        /// <summary>
        /// Type representing an unowned station.
        /// </summary>
        StationUnowned, 
        /// <summary>
        /// Type representing an owned station.
        /// </summary>
        StationOwned,
        /// <summary>
        /// Type representing an unowned utility.
        /// </summary>
        UtilityUnowned,
        /// <summary>
        /// Type representing an owned utility.
        /// </summary>
        UtilityOwned
        #endregion
    }
}
