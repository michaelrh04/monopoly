using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    /// <summary>
    /// All the model logic for free parking
    /// </summary>
    [Serializable]
    public class FreeParking : Location
    {
        #region Constructor
        /// <summary>
        /// The free parking class represents the free parking square
        /// </summary>
        /// <param name="position">Its position on the board.</param>
        public FreeParking(int position, MonopolyWindowViewModel viewmodel) : base(viewmodel)
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
            if (((bool)MonopolyWindowViewModel.Handler.Settings["do_taxation_in_free_parking"])) {
                player.Balance += MonopolyWindowViewModel.Handler.CumulativeTaxation;
                MonopolyWindowViewModel.Handler.CumulativeTaxation = 0;
            }
        }
        #endregion

    }
}
