using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    /// <summary>
    /// All the model logic for the go square
    /// </summary>
    [Serializable]
    public class Go : Location
    {
        #region Constructor
        /// <summary>
        /// The Go class is for representing the go square
        /// </summary>
        /// <param name="position">Its position on the board.</param>
        public Go(int position, MonopolyWindowViewModel viewmodel) : base(viewmodel)
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
            // This player has landed on go. Grant them the cash applicable.
            player.Balance += int.Parse(MonopolyWindowViewModel.Handler.Settings["passing_go_amount"].ToString()) * int.Parse(MonopolyWindowViewModel.Handler.Settings["passing_go_amount_multiplier"].ToString());
        }
        #endregion
    }
}
