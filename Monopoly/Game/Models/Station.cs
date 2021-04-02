using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monopoly.Game
{
    [Serializable]
    public class Station : Property
    {

        #region Constructor
        /// <summary>
        /// Constructor for any station.
        /// </summary>
        public Station(MonopolyWindowViewModel viewmodel) : base(viewmodel)
        {
            
        }
        #endregion

        #region Public properties

        #endregion

        #region Subroutines
        public override void Land(Player player)
        {
            
        }
        public override int GetRentOwed()
        {
            return MonopolyWindowViewModel.Handler.BoardConfiguration.StationsRent[Owner.Inventory["Stations"].Count(p => p.IsMortgaged == false) - 1];
        }
        #endregion
    }
}
