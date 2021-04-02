using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    /// <summary>
    /// All the model logic for the 'go to jail' tile
    /// </summary>
    [Serializable]
    public class GoToJail : Location
    {
        #region Constructor
        /// <summary>
        /// The chance class is for the 'go to jail' tile, and requires the parameter for position it holds.
        /// </summary>
        /// <param name="position">Its position on the board.</param>
        public GoToJail(int position, MonopolyWindowViewModel viewmodel) : base(viewmodel)
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
            MonopolyWindowViewModel.Handler.Jail(true, player);
            // If the player rolled a double, they would otherwise still need to take a turn. We need to undo this.
            MonopolyWindowViewModel.Handler.ActionsUnresolved--;
            MonopolyWindowViewModel.Handler._RollsRemaining = 0;
            MonopolyWindowViewModel.Handler.OnPropertyChanged("RollsComplete");
            Depart(player);
        }
        #endregion
    }
}
