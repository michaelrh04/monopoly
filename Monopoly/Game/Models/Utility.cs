using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monopoly.Game
{
    [Serializable]
    public class Utility : Property
    {

        #region Constructor
        /// <summary>
        /// Constructor for any utility.
        /// </summary>
        public Utility(MonopolyWindowViewModel viewmodel) : base(viewmodel)
        {

        }
        #endregion

        #region Public properties
        public string Symbol { get; set; }
        #endregion

        #region Subroutines
        public override void Land(Player player)
        {
            
        }
        public override int GetRentOwed()
        {
            int multiplier = MonopolyWindowViewModel.Handler.BoardConfiguration.UtilityMultipliers[Owner.Inventory["Utilities"].Count(p => p.IsMortgaged == false) - 1];
            return multiplier * (MonopolyWindowViewModel.Handler.Roll.Item1 + MonopolyWindowViewModel.Handler.Roll.Item2);
        }
        #endregion
    }
}
