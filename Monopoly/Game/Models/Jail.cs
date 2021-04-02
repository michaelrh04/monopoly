using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    /// <summary>
    /// All the model logic for chance cards
    /// </summary>
    [Serializable]
    public class Jail : Location
    {
        #region Constructor
        /// <summary>
        /// The class representative of the 'jail' tile
        /// </summary>
        /// <param name="position">Its position on the board.</param>
        public Jail(int position, MonopolyWindowViewModel viewmodel) : base(viewmodel)
        {
            Position = position;
        }
        #endregion

        #region Public properties
        public int Position { get; set; }
        #endregion

        #region Subroutines
        public override void Land(Player player)
        {
            // Do nothing
        }
        #endregion
    }
}
