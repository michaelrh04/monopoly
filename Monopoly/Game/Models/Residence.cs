using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monopoly.Game
{
    [Serializable]
    public class Residence : Property
    {

        #region Constructor
        /// <summary>
        /// Constructor for any Residence - a property of a colour set that can have a house upon it.
        /// </summary>
        public Residence(MonopolyWindowViewModel viewmodel) : base(viewmodel)
        {
            
        }
        #endregion

        #region Public properties
        public int[] Rent { get; set; }
        public int Houses { get; set; }
        public int HouseIncrementationPrice { get; set; }
        #endregion

        #region Subroutines
        public override void Land(Player player)
        {
            // Do nothing
        }
        public override int GetRentOwed()
        {
            // The base rent, if this property has has no houses on it, is saved as the first index.
            if(Houses == 0)
            {
                // However, if the full property set is owned by one player, this amount doubles. Check for this.
                if(Owner.Inventory[Set].Count == MonopolyWindowViewModel.Handler.BoardConfiguration.Residences[Set].Count)
                {
                    return Rent[0] * 2;
                }
                return Rent[0];
            }
            else
            {
                // Return the corresponding number.
                return Rent[Houses];
            }
        }
        #endregion
    }
}
