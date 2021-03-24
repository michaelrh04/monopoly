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
    public class Chance : Location
    {
        #region Constructor
        /// <summary>
        /// The chance class is for all chance cards, and requires the parameter for position it holds.
        /// </summary>
        /// <param name="position">It's position on the board.</param>
        public Chance(int position, MonopolyWindowViewModel viewmodel) : base(viewmodel)
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
            MonopolyWindowViewModel.Handler.DrawCard(0);
        }
        #endregion

    }
}
