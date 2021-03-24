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
            //Land = new RelayCommand(_Land);
            //AddHouse = new AddHouseCommand(_AddHouse); basically, same for remove house
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
            int multiplier = MonopolyWindowViewModel.Handler.BoardConfiguration.UtilityMultipliers[0];
            if (Owner.Inventory["Utilities"].Count(p => p.IsMortgaged == false) == MonopolyWindowViewModel.Handler.BoardConfiguration.Utilities.Count)
            {
                multiplier = MonopolyWindowViewModel.Handler.BoardConfiguration.UtilityMultipliers[1];
            }
            return multiplier * (MonopolyWindowViewModel.Handler.Roll.Item1 + MonopolyWindowViewModel.Handler.Roll.Item2);
        }
        #endregion

    }
}
