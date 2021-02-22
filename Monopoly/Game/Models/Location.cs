using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monopoly.Game
{
    [Serializable]
    public abstract class Location : INotifyPropertyChanged
    {

        #region Constructor
        public Location(MonopolyWindowViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }
        #endregion
        /// <summary>
        /// Used to find the viewmodel for updating the view.
        /// </summary>
        [NonSerialized]
        public MonopolyWindowViewModel ViewModel;
        /// <summary>
        /// Find the occupants of this tile.
        /// </summary>
        public List<Player> Occupants
        {
            get
            {
                return _Occupants;
            }
            set
            {
                if(value != _Occupants)
                {
                    _Occupants = value;
                    OnPropertyChanged("Occupants");
                }
            }
        }
        private List<Player> _Occupants = new List<Player>();

        [field: NonSerialized]
        public SolidColorBrush PropertyBackground
        {
            get
            {
                return _PropertyBackground;
            }
            set
            {
                if (value != _PropertyBackground)
                {
                    _PropertyBackground = value;
                    OnPropertyChanged("PropertyBackground");
                }
            }
        }
        [NonSerialized]
        private SolidColorBrush _PropertyBackground = new SolidColorBrush(Colors.LightGray);

        #region Subroutines
        public void Arrive(Player player)
        {
            if(player.Digit != -1)
            {
                // This player is not bankrupt, so they can be added to this tile without problem.
                Occupants.Add(player);
                // We've landed on this tile!
                // Call that abstract void that child classes will implement later.
                Land(player);
                OnPropertyChanged("Occupants");
            }
        }
        public abstract void Land(Player player);
        public void Depart(Player player)
        {
            Occupants.Remove(player);
            OnPropertyChanged("Occupants");
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
