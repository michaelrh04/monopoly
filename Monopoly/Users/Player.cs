using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Monopoly.Game;

namespace Monopoly
{
    /// <summary>
    /// Class for a player; instances of this class should be created for every player in the game.
    /// </summary>
    [Serializable]
    public class Player : INotifyPropertyChanged
    {

        #region Constructor
        public Player()
        {

        }
        #endregion

        #region Public properties
        // For appearances/identification.
        public string Name { get; set; }
        public int Digit { get; set; }
        public int Balance
        {
            get
            {
                return _Balance;
            }
            set
            {
                if(value != _Balance)
                {
                    // Configure BalanceChanged for graphical options
                    // The difference between before and now
                    BalanceChanged = (value - _Balance);
                    // Then, actually make the change
                    _Balance = value;
                    OnPropertyChanged("Balance");
                    OnPropertyChanged("BalanceChanged");
                }
            }
        }
        public int BalanceChanged 
        { 
            get
            {
                return _BalanceChanged;
            } 
            set
            {
                if(value != _BalanceChanged)
                {
                    _BalanceChanged = value;
                    OnPropertyChanged("BalanceChanged");
                }
            }
        }
        public string Icon { get; set; }
        public bool Online { get; set; }
        public bool IsJailed
        {
            get
            {
                return _IsJailed;
            }
            set
            {
                if(value != _IsJailed)
                {
                    _IsJailed = value;
                    if(value == false)
                    {
                        JailedTurns = 0;
                    }
                    OnPropertyChanged("IsJailed");
                }
            }
        }
        public int JailedTurns;
        // For the game.
        public Dictionary<string, ObservableCollection<Property>> Inventory
        {
            get
            {
                return _Inventory;
            }
            set
            {
                if(value != _Inventory)
                {
                    _Inventory = value;
                    OnPropertyChanged("Inventory");
                    OnPropertyChanged("InventoryCount");
                }
            }
        }
        public int Location { get; set; }
        /// <summary>
        /// Retrieves the number of properties currently owned by the player.
        /// </summary>
        public int InventoryCount
        {
            get
            {
                return CountProperties(0);
            }
        }
        public List<Player> InventoryChanged { get; set; }
        #endregion

        #region Private properties
        private Dictionary<string, ObservableCollection<Property>> _Inventory = new Dictionary<string, ObservableCollection<Property>>();
        public bool _IsJailed = false;
        private int _Balance;
        private int _BalanceChanged;
        #endregion

        #region Subroutines
        public void Setup(Gameboard gameboard)
        {
            // To setup the player, one dictionary reference for each property set must be added to the Inventory.
            // This includes Stations and Utilities.
            foreach(KeyValuePair<string, List<Residence>> propertySet in gameboard.Residences)
            {
                Inventory.Add(propertySet.Key, new ObservableCollection<Property>());
            }
            Inventory.Add("Stations", new ObservableCollection<Property>());
            Inventory.Add("Utilities", new ObservableCollection<Property>());
            // The setup is complete.
        }
        /// <summary>
        /// Count the properties owned by this player according to the selected mode.
        /// </summary>
        /// <param name="mode">0 = all</param>
        /// <returns></returns>
        private int CountProperties(int mode)
        {
            int number = 0;
            foreach(KeyValuePair<string, ObservableCollection<Property>> pair in Inventory)
            {
                if(mode == 0)
                {
                    number += pair.Value.Count;
                }
            }
            return number;
        }
        #endregion

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// PropertyChanged event, required for implementation
        /// </summary>
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// OnPropertyChanged event can be called with the property name to update components on the View bound to the aforementioned property
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        #endregion

    }
}
