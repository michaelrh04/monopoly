using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    /// <summary>
    /// Taxation tiles represent locations on the board that tax the user.
    /// </summary>
    [Serializable]
    public class Tax : Location
    {

        #region Constructor
        public Tax(MonopolyWindowViewModel viewmodel) : base(viewmodel)
        {

        }
        #endregion

        #region Public properties
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Position { get; set; }
        #endregion

        #region Subroutines
        public override void Land(Player player)
        {
            // This may put the player into the negative! Hence, we need to prevent the player going to the next turn if the player is below £0.
            player.Balance -= Cost;
            MonopolyWindowViewModel.Handler.CumulativeTaxation += Cost;
        }
        #endregion
    }
}
