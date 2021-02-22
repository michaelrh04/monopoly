using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Monopoly.Game
{
    /// <summary>
    /// The Property abstract class is a parent to every class of a property that can be owned.
    /// As a result, it contains definitions for common properties: for example, the property name.
    /// </summary>
    [Serializable]
    public abstract class Property : Location
    {
        #region Constructor
        public Property(MonopolyWindowViewModel viewmodel) : base(viewmodel)
        {

        }
        #endregion

        #region Public properties
        public string Name { get; set; }
        public string Set { get; set; }
        public Player Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                if (value != _Owner)
                {
                    _Owner = value;
                    OnPropertyChanged("Owner");
                    OnPropertyChanged("OwnerName");
                }
            }
        }
        /// <summary>
        /// OwnerName returns a lowercase string containing the owner's name of this property, or 'unowned' if it has no owner.
        /// </summary>
        public string OwnerName
        {
            get
            {
                if(Owner == null)
                {
                    return "unowned";
                } else
                {
                    if(Owner.Name.ToLower().Last() == 's')
                    {
                        return Owner.Name.ToLower() + "'"; // For example, James'
                    } else
                    {
                        return Owner.Name.ToLower() + "'s"; // For example, John's
                    }
                }
            }
        }
        public bool IsSelectedForTrade { get; set; }
        public int Price { get; set; }
        public string Hex { get; set; }
        public bool IsMortgaged
        {
            get
            {
                return _IsMortgaged;
            }
            set
            {
                if (value != _IsMortgaged)
                {
                    _IsMortgaged = value; 
                    OnPropertyChanged("IsMortgaged");

                }
            }
        }
        /// <summary>
        /// PropertyColour will allow the View to fetch a SolidColourBrush value for the property from the Models.
        /// </summary>
        [field: NonSerialized]
        public SolidColorBrush PropertyColour 
        { 
            get
            {
                try
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#" + Hex);
                    // Taken from StackOverflow.
                    // https://stackoverflow.com/a/10710201
                }
                catch (FormatException)
                {
                    return new SolidColorBrush(Colors.Transparent);
                    //THIS MUST BE FIXED LATER
                    //THIS MUST BE FIXED LATER
                    //THIS MUST BE FIXED LATER
                    //THIS MUST BE FIXED LATER
                    //THIS MUST BE FIXED LATER
                }
            } 
        }
        /// <summary>
        /// The PropertyBackground brush allows for individual control of different aspects of the board, used to create visual effects.
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// Gets the amount of rent owed.
        /// </summary>
        /// <returns>Amount of rent owed.</returns>
        public abstract int GetRentOwed();
        #endregion

        #region Private properties
        private Player _Owner;
        private bool _IsMortgaged;
        #endregion

        #region Subroutine requirements
        public void Purchase(Player newOwner, int CustomPrice = -1)
        {
            if(CustomPrice == -1) { CustomPrice = Price; }
            Console.WriteLine("[DEBUG] New owner " + newOwner.Name + " is purchasing " + Name + " for " + CustomPrice);
            // Remove from any previous owner
            if(Owner != null)
            {
                Owner.Inventory[Set].Remove(this);
            }
            Owner = newOwner;
            newOwner.Inventory[Set].Add(this);
            newOwner.Balance -= CustomPrice;
        }
        #endregion

    }
}
