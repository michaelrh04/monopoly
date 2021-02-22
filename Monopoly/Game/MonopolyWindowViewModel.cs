using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Input;

using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Monopoly.Converters;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace Monopoly.Game
{
    /// <summary>
    /// MonopolyWindowViewModel provides a MVVM implementation for the Monopoly game window
    /// </summary>
    public class MonopolyWindowViewModel : INotifyPropertyChanged
    {
        #region Constructor
        /// <summary>
        /// Constructor for the MonopolyWindowViewModel. 
        /// The game board used must be passed as a parameter.
        /// </summary>
        /// <param name="savefile">The savefile to be used if selected.</param>
        /// <param name="boardDirectory">Directory of the game board to be used.</param>
        /// <param name="players">List of the players in the game.</param>
        /// <param name="settings">Dictionary of the settings to be used.</param>
        public MonopolyWindowViewModel(IDialogCoordinator instance, MonopolyHandler savefile, string boardDirectory = null, List<Player> players = null, Dictionary<string, object> settings = null)
        {
            Console.WriteLine("[DEBUG] Constructing the MonopolyWindowViewModel.");
            // Set the DialogCoordinator to the one define in App.xaml.cs
            Dialogs = instance;
            // If a savefile exists, load the savefile.
            var started = false;
            if (savefile != null)
            {
                Console.WriteLine("[DEBUG] Detected that this is a savegame.");
                started = true;
                // Load the savefile.
                Handler = savefile;
                Handler.ViewModel = this;
            }
            else
            {
                Console.WriteLine("[DEBUG] Detected that this is a new game.");
                // This is a new game, hurray!
                // The new handler...
                Handler = new MonopolyHandler(this, settings);
                // ...must load the selected board.
                Console.WriteLine("[DEBUG] Attempting to load the selected board.");
                Handler.BoardConfiguration = JsonConvert.DeserializeObject<Gameboard>(File.ReadAllText(boardDirectory));
                Console.WriteLine("[DEBUG] Attempt to load the selected board signalled as complete.");
                // ADD ERROR HANDLING HERE.
                // Then, the deseralised BoardConfiguration must be formatted for view. This must be set to equal the Board in the view.
                Console.WriteLine("[DEBUG] Attempting to format the board.");
                Handler.Board = Handler.BoardConfiguration.FormatForView();
                Console.WriteLine("[DEBUG] Attempt to format the board signalled as complete.");
                // It's time to establish the players.
                foreach (Player player in players)
                {
                    Handler.InitPlayer(player);
                }
            }
            // Establish visual configuration - board scale, rotations, etc.
            BoardScale = 1.05;
            RotateButtonRotation = 360;
            // The board is now ready to go, and the game can begin. 
            Handler.Start(started);
        }
        public IDialogCoordinator Dialogs;
        #endregion

        #region Command definitions
        /// <summary>
        /// Save and quit command.
        /// </summary>
        /// <returns></returns>
        public RelayCommand SaveQuit
        {
            get
            {
                return new RelayCommand(Handler._Save);
            }
        }
        /// <summary>
        /// Triggers the static SelectedProperty variable to update with the appropriate selected property, causing visual updates where appropriate.
        /// </summary>
        public RelayCommand PropertyClicked
        {
            get
            {
                return new RelayCommand(_PropertyClicked);
            }
        }
        /// <summary>
        /// Immediately dismisses the property currently selected.
        /// </summary>
        public RelayCommand DismissPropertyClicked
        {
            get
            {
                return new RelayCommand(_DismissPropertyClicked);
            }
        }
        /// <summary>
        /// Moves the SelectedProperty either one property to the left or one property to the right.
        /// </summary>
        public RelayCommand NextProperty
        {
            get
            {
                return new RelayCommand(_NextProperty);
            }
        }
        /// <summary>
        /// To allow for the calling of a next turn!
        /// </summary>
        public RelayCommand NextTurn { 
            get
            {
                return new RelayCommand(Handler._NextTurn);
            } 
        }
        public BoundCanExecuteCommand PayRent
        {
            get
            {
                return new BoundCanExecuteCommand(Handler._PayRent, CanPayRent);
            }
        }
        public BoundCanExecuteCommand PurchaseProperty
        {
            get
            {
                return new BoundCanExecuteCommand(Handler._PurchaseProperty, CanPurchaseProperty);
            }
        }
        public BoundCanExecuteCommand DeclineProperty
        {
            get
            {
                return new BoundCanExecuteCommand(Handler._DeclineProperty, CanDeclineProperty);
            }
        }
        public BoundCanExecuteCommand AddHouse
        {
            get
            {
                return new BoundCanExecuteCommand(Handler._AddHouse, CanAddHouse);
            }
        }
        public BoundCanExecuteCommand RemoveHouse
        {
            get
            {
                return new BoundCanExecuteCommand(Handler._RemoveHouse, CanRemoveHouse);
            }
        }
        public BoundCanExecuteCommand MortgageProperty
        {
            get
            {
                return new BoundCanExecuteCommand(Handler._ToggleMortgage, CanMortgageProperty);
            }
        }
        public BoundCanExecuteCommand UnmortgageProperty
        {
            get
            {
                return new BoundCanExecuteCommand(Handler._ToggleMortgage, CanUnmortgageProperty);
            }
        }
        public RelayCommand DeclareBankruptcy
        {
            get
            {
                return new RelayCommand(Handler._DeclareBankruptcy);
            }
        }
        #endregion

        private bool IsPlayerOnSelectedProperty(Player player)
        {
            foreach (Player occupant in SelectedProperty.Occupants)
            {
                if (occupant == player)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsPlayerSelectedPropertyOwner(Player player)
        {
            if(SelectedProperty == null || SelectedProperty.Owner == null || Handler.CurrentPlayer == null) { return false; }
            return SelectedProperty.Owner == player;
        }
        /// <summary>
        /// Easy subroutine for invoking OnPropertyChanged() for every of the central button options.
        /// </summary>
        public void ForcePropertyChanged()
        {
            string[] propertyNames = { "SelectedProperty", "PayRent", "PurchaseProperty", "DeclineProperty" };
            foreach (string propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
            string[] HpropertyNames = { "RollsComplete", "CurrentPlayer", "ActionsUnresolved" };
            foreach (string propertyName in HpropertyNames)
            {
                Handler.OnPropertyChanged(propertyName);
            }
            Handler.CurrentPlayer.OnPropertyChanged("Inventory");
            foreach(Player player in Handler.Players)
            {
                player.OnPropertyChanged("Balance");
                player.OnPropertyChanged("Inventory");
                player.OnPropertyChanged("InventoryCount");
            }
            foreach(Location location in Handler.Board)
            {
                // This will only apply to properties but it is worth doing anyway.
                location.OnPropertyChanged("Owner");
            }
        }
        #region Command predicates/requirements
        private bool CanPayRent()
        {
            if (SelectedProperty == null || Handler.HasPaidRent || SelectedProperty.IsMortgaged) { return false; }
            // Check this is the property the player is actually on.
            if (!IsPlayerOnSelectedProperty(Handler.CurrentPlayer)) { return false; }
            // Proceed.
            if(!IsPlayerSelectedPropertyOwner(Handler.CurrentPlayer) && SelectedProperty.Owner != null)
            {
                // Check against the rules as well!
                if(SelectedProperty.Owner.IsJailed && ((bool)Handler.Settings["allow_rent_collection_while_jailed"]) == false)
                {
                    return false;
                }
                // Else
                if (Handler.CurrentPlayer.Balance >= SelectedProperty.GetRentOwed())
                {
                    return true;
                }
            }
            return false;
        }
        private bool CanPurchaseProperty()
        {
            // Ensure that there IS a selected property!
            if (SelectedProperty == null) { return false; }
            // This is the opposite of the can-pay-rent property, with the exception of incorporating ability to pay additionally.
            if (IsPlayerOnSelectedProperty(Handler.CurrentPlayer) && SelectedProperty.Owner == null && Handler.CurrentPlayer.Balance >= SelectedProperty.Price)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CanDeclineProperty()
        {
            // Ensure that there IS a selected property!
            if (SelectedProperty == null || SelectedProperty.Owner != null) { return false; }
            return IsPlayerOnSelectedProperty(Handler.CurrentPlayer);
        }
        private bool CanAddHouse()
        {
            if (IsPlayerSelectedPropertyOwner(Handler.CurrentPlayer)
                && SelectedProperty is Residence res)
            {
                if (res.Houses == 5)
                {
                    // Maximum capacity reached.
                }
                else
                {
                    // We need to check that the house incrementation rules apply. Check this first.
                    if (((bool)Handler.Settings["allow_uneven_house_construction"]) == false)
                    {
                        foreach(Residence setMember in Handler.BoardConfiguration.Residences[res.Set])
                        {
                            if((setMember.Houses != res.Houses && setMember.Houses != res.Houses + 1) || setMember.Owner != Handler.CurrentPlayer)
                            {
                                return false;
                            }
                        }
                    }
                    // Now that we know adding a house/hotel is theoretically possible, we should check that the capacity is available:
                    if ((bool)Handler.Settings["limit_house_hotel_numbers"])
                    {
                        if(res.Houses == 5)
                        {
                            return Handler.GetHotelsAvailable() > 0;
                        } 
                        else
                        {
                            return Handler.GetHousesAvailable() > 0;
                        }
                    }
                    // And can they afford it?
                    if(Handler.CurrentPlayer.Balance < ((Residence)SelectedProperty).HouseIncrementationPrice)
                    {
                        return false;
                    }
                    // We're good to go!
                    return true;
                }
            }
            return false;
        }
        private bool CanRemoveHouse()
        {
            if (IsPlayerSelectedPropertyOwner(Handler.CurrentPlayer)
                && SelectedProperty is Residence res)
            {
                if (res.Houses == 0)
                {
                    // There are no houses here anyway.
                }
                else
                {
                    // We need to check that the house incrementation rules apply. Check this first.
                    if (((bool)Handler.Settings["allow_uneven_house_construction"]) == false)
                    {
                        foreach (Residence setMember in Handler.BoardConfiguration.Residences[res.Set])
                        {
                            if ((setMember.Houses != res.Houses && setMember.Houses != res.Houses - 1) || setMember.Owner != Handler.CurrentPlayer)
                            {
                                return false;
                            }
                        }
                    }
                    // If there is a hotel on this tile, we need to make sure it is physically possible for us to get a new house.
                    if (res.Houses == 5 && ((bool)Handler.Settings["limit_house_hotel_numbers"]) && Handler.GetHousesAvailable() == 0)
                    {
                        return false;
                    }
                    // We're good to go!
                    return true;
                }
            }
            return false;
        }
        private bool CanMortgageProperty()
        {
            if (IsPlayerSelectedPropertyOwner(Handler.CurrentPlayer)
                && !(SelectedProperty.IsMortgaged))
            {
                if (SelectedProperty is Residence)
                {
                    foreach (Residence res in Handler.BoardConfiguration.Residences[SelectedProperty.Set])
                    {

                        if (res.Houses > 0)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }
        private bool CanUnmortgageProperty()
        {
            if (IsPlayerSelectedPropertyOwner(Handler.CurrentPlayer)
                            && SelectedProperty.IsMortgaged
                            && Handler.CurrentPlayer.Balance > ((SelectedProperty.Price / 2) * 1.1))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// The MonopolyHandler handles all game management (allowing for easy saving/loading). The role of the ViewModel is to communicate graphical updates to the view and game updates to the handler.
        /// </summary>
        public static MonopolyHandler Handler { get; set; }
        #endregion

        #region Private properties

        #endregion

        #region Visual handling

        private double _MouseX;
        private double _MouseY;
        private double _BoardScale;
        private bool _LockMouseVisual;
        private double _RotateButtonRotation;
        private Property _SelectedProperty;

        #region Visual command definitions
        /// <summary>
        /// The MouseMove command promots changes in the MouseX and MouseY variables.
        /// </summary>
        public ICommand MouseMove
        {
            get
            {
                return new RelayCommand(_MouseMove);
            }
        }
        /// <summary>
        /// Rotating the board changes the enumerable BoardRotateMode of the converters and triggers changes in the bindings RotateButtonRotation and respective scale bindings for graphical effect.
        /// </summary>
        public ICommand RotateBoard
        {
            get
            {
                return new RelayCommand(_RotateBoardAsync);
            }
        }
        #endregion

        #region Visual public properties
        public double MouseX
        {
            get
            {
                return _MouseX;
            }
            set
            {
                if (_MouseX != value)
                {
                    _MouseX = Math.Round(value, 3);
                    OnPropertyChanged("MouseX");
                }
            }
        }
        public double MouseY
        {
            get
            {
                return _MouseY;
            }
            set
            {
                if (_MouseY != value)
                {
                    _MouseY = Math.Round(value, 3);
                    OnPropertyChanged("MouseY");
                }
            }
        }
        public double BoardScale
        {
            get
            {
                return _BoardScale;
            }
            set
            {
                if (_BoardScale != value)
                {
                    _BoardScale = value;
                    OnPropertyChanged("BoardScale");
                }
            }
        }
        public bool LockMouseVisual
        {
            get
            {
                return _LockMouseVisual;
            }
            set
            {
                if (_LockMouseVisual != value)
                {
                    _LockMouseVisual = value;
                    OnPropertyChanged("LockMouseVisual");
                }
            }
        }
        public double RotateButtonRotation
        {
            get
            {
                return _RotateButtonRotation;
            }
            set
            {
                if (_RotateButtonRotation != value)
                {
                    _RotateButtonRotation = value;
                    OnPropertyChanged("RotateButtonRotation");
                }
            }
        }
        public Property SelectedProperty
        {
            get
            {
                return _SelectedProperty;
            }
            set
            {
                if (value != _SelectedProperty)
                {
                    _SelectedProperty = value;
                    OnPropertyChanged("SelectedProperty");
                    ForcePropertyChanged();
                }
            }
        }
        #endregion

        #region Visual subroutines
        /// <summary>
        /// ViewModel subroutine for mouse movement
        /// </summary>
        /// <param name="_sender"></param>
        private void _MouseMove(object _sender)
        {
            var sender = _sender as UIElement;
            if (!LockMouseVisual)
            {
                MouseX = Mouse.GetPosition(sender).X;
                MouseY = Mouse.GetPosition(sender).Y;
            }
        }
        /// <summary>
        /// ViewModel subroutine for rotating the board
        /// </summary>
        /// <param name="_sender"></param>
        private async void _RotateBoardAsync(object _sender)
        {
            // Use BoardScale bindings to create a popping animation
            for (double i = 1.05; i < 1.10; i += 0.01)
            {
                BoardScale = i;
                RotateButtonRotation -= 9;
                await Task.Delay(10);
            }
            // Transform the board, roatating the properties
            switch (BoardColumnConverter.BoardRotation)
            {
                case BoardRotateMode.Thrice:
                    BoardColumnConverter.BoardRotation = BoardRotateMode.Normal;
                    break;
                default:
                    BoardColumnConverter.BoardRotation += 10;
                    break;
            }
            foreach (Location Landable in Handler.Board)
            {
                if (Landable != null)
                {
                    Landable.OnPropertyChanged("Position");
                }
            }
            // Use BoardScale bindings to finish the popping animation
            for (double i = 1.10; i > 1.05; i -= 0.01)
            {
                BoardScale = i;
                RotateButtonRotation -= 9;
                await Task.Delay(10);
            }
            BoardScale = 1.05;
            if (RotateButtonRotation <= 0)
            {
                RotateButtonRotation = 360;
            }
        }
        /// <summary>
        /// ViewModel subroutine for selecting a property
        /// </summary>
        /// <param name="_sender"></param>
        private void _PropertyClicked(object _sender)
        {
            if (SelectedProperty != (Property)_sender)
            {
                SelectedProperty = null;
                SelectedProperty = (Property)_sender;
            }
            else
            {
                _DismissPropertyClicked(null);
            }
        }
        /// <summary>
        /// ViewModel subroutine ford dismissing the property selected
        /// </summary>
        /// <param name="_sender"></param>
        private async void _DismissPropertyClicked(object _sender)
        {
            // If the sender is null, dismissal has been called from within the viewmodel.
            // If it is not null, it has been called from the view: as a result, for visual effect, delay action for a second.
            IsVisualPropertyNullConverter.ForceTrueResult = true;
            OnPropertyChanged("SelectedProperty");
            // Prompt the view to run the animation.
            IsVisualPropertyNullConverter.ForceTrueResult = false;
            await Task.Delay(250);
            SelectedProperty = null;
        }
        /// <summary>
        /// ViewModel subroutine for requesting the next property be displayed
        /// </summary>
        /// <param name="_sender"></param>
        private void _NextProperty(object _sender)
        {
            string direction = (string)_sender;
            int index = Handler.Board.IndexOf(SelectedProperty);
            if (direction == "Left")
            {
                // User wishes to select leftwards property.
                // Loop for a maximum of fourty times before selecting a new property.
                index++;
                for (int i = 0; i < 40; i++)
                {
                    // Adjust the counter value to fit the property
                    int j = i + index;
                    if (j > 39) { j -= 40; }
                    // Check if this property is applicable
                    if (Handler.Board[j] is Property property)
                    {
                        // This property is a property and can be displayed
                        _PropertyClicked(property);
                        // Break the loop
                        i = 40;
                    }
                }
                // Subroutine ends.
            }
            else if (direction == "Right")
            {
                // User wishes to select rightwards property.
                // Loop for a maximum of fourty times before selecting a new property.
                index--;
                for (int i = 39; i >= 0; i--)
                {
                    // Adjust the counter value to fit the property
                    int j = i - (39 - index);
                    if (i <= (39 - index)) { j += 39; }
                    // Check if this property is applicable
                    if (Handler.Board[j] is Property property)
                    {
                        // This property is a property and can be displayed
                        _PropertyClicked(property);
                        // Break the loop
                        i = -1;
                    }
                }
                // Subroutine ends.
            }
            else
            {
                throw new FormatException();
            }
        }
        #endregion

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
