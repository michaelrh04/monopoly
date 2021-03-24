using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

using Monopoly.Game;
using Monopoly.Editor;

namespace Monopoly.Menu
{
    public class MainMenuViewModel : INotifyPropertyChanged
    {
        #region Constructor
        public MainMenuViewModel(IDialogCoordinator instance)
        {
            // Set the DialogCoordinator to the one define in App.xaml.cs
            Dialogs = instance;
            // Establish the settings dictionary with default values.
            Settings.Add("amount_begin_with", 1500.0);
            Settings.Add("passing_go_amount", 200.0);
            Settings.Add("passing_go_amount_multiplier", 1.0);
            Settings.Add("do_trigger_auctions", true);
            Settings.Add("do_taxation_in_free_parking", false);
            Settings.Add("allow_rent_collection_while_jailed", true);
            Settings.Add("allow_uneven_house_construction", false);
            Settings.Add("limit_house_hotel_numbers", true);
            // Populate the AvailableBoards list
            AvailableBoards = GetAvailableBoards();
        }
        #endregion

        #region Public definitions
        public Action Close { get; set; }
        public RelayCommand RemovePlayer 
        { 
            get
            {
                return new RelayCommand(LobbyRemovePlayer);
            } 
        }
        public RelayCommand Play
        {
            get
            {
                return new RelayCommand(StartGame);
            }
        }
        public RelayCommand ClearList
        {
            get
            {
                return new RelayCommand(_ClearList);
            }
        }
        public RelayCommand OpenEditor
        {
            get
            {
                return new RelayCommand(_OpenEditor);
            }
        }
        public Dictionary<string, object> Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                if(value != _Settings)
                {
                    _Settings = value;
                    OnPropertyChanged("Settings");
                }
            }
        }
        private Dictionary<string, object> _Settings = new Dictionary<string, object>();
        #endregion

        #region For the login alone

        #region Command definitions
        public RelayCommand Login
        {
            get
            {
                return new RelayCommand(_Login, CanLoginExecute);
            }
        }
        public RelayCommand Register
        {
            get
            {
                return new RelayCommand(_Register, CanRegisterExecute);
            }
        }
        public RelayCommand JoinAsGuest
        {
            get
            {
                return new RelayCommand(_JoinAsGuest, CanJoinAsGuestExecute);
            }
        }
        public RelayCommand ResetLoginStatus
        {
            get
            {
                return new RelayCommand(_ResetLoginStatus, CanNewPlayerAdd);
            }
        }
        public RelayCommand SelectBoard
        {
            get
            {
                return new RelayCommand(_SelectBoard);
            }
        }
        #endregion

        #region Public properties
        /// <summary>
        /// The list of players. This is an ObservableCollection, and so already inherits from INotifyPropertyChanged.
        /// </summary>
        public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
        /// <summary>
        /// Used to determine the status of the login for the view's update.
        /// ONE: 
        /// </summary>
        public Status LoginStatus
        {
            get
            {
                return _LoginStatus;
            }
            set
            {
                if (value != _LoginStatus)
                {
                    _LoginStatus = value;
                    OnPropertyChanged("LoginStatus");
                }
            }
        }
        /// <summary>
        /// Used to monitor whether or not the flyout is open, as well as to allow it to be closed from the ViewModel.
        /// </summary>
        public bool FlyoutOpen
        {
            get
            {
                return _FlyoutOpen;
            }
            set
            {
                if(value != _FlyoutOpen)
                {
                    _FlyoutOpen = value;
                    OnPropertyChanged("FlyoutOpen");
                }
            }
        }
        public ObservableCollection<BoardData> AvailableBoards { get; set; }
        #endregion

        #region Private properties
        // For the LoginStatus property.
        private Status _LoginStatus;
        // For the FlyoutOpen property.
        private bool _FlyoutOpen;
        // To allow for only one necessary instance of the MySqlConnection.
        private MySqlConnection sqlConnection = new MySqlConnection(@"Server=sql262.main-hosting.eu;Initial Catalog=u725584021_projmonopoly;User id=u725584021_projmonopoly;Password=pr0jM0n0p0ly");
        // To allow for the creation of dialogs in the View from the ViewModel.
        private IDialogCoordinator Dialogs;
        // The directory of the board chosen
        private string BoardDirectory = null;
        #endregion

        #region Subroutines
        private async void _Login(object sender)
        {
            object[] details = (object[])sender;
            // Establish a connection to the database
            LoginStatus = Status.Ongoing;
            await Task.Run(() =>
            {
                try
                {
                    sqlConnection.Open();
                    // To inquire, we need to find the result where both username and password are given correctly
                    string query = @"SELECT * FROM `accounts` WHERE username = '" + details[0] + "' AND password = '" + ((PasswordBox)details[1]).Password + "'";
                    MySqlCommand search = new MySqlCommand(query, sqlConnection);
                    object result = search.ExecuteScalar();
                    sqlConnection.Close();
                    if (result == null)
                    {
                        // This account was not found!
                        LoginStatus = Status.Terminated;
                    }
                }
                catch (MySqlException)
                {
                    // We could not connect to the SQL server! Display this and return.
                    LoginStatus = Status.Failed;
                }
                catch (System.IO.IOException)
                {
                    // We could not connect to the SQL server! Display this and return.
                    LoginStatus = Status.Failed;
                }
            });
            if (LoginStatus == Status.Ongoing)
            {
                // The account has been found!
                LoginStatus = Status.Completed;
                LobbyAddPlayer(Tuple.Create((string)details[0], (int)details[2], true));
            }
        }
        private async void _Register(object sender)
        {
            object[] details = (object[])sender;
            // Establish a connection to the database
            LoginStatus = Status.Ongoing;
            await Task.Run(() =>
            {
                try
                {
                    sqlConnection.Open();
                    // First, we need to check that neither USERNAME nor EMAIL (only) are in use.
                    // We can do this by checking for any result that matches either query.
                    string duplicationCheck = @"SELECT * FROM `accounts` WHERE `username` = '" + details[0] + "' OR `email` = '" + details[4] + "'";
                    MySqlCommand check = new MySqlCommand(duplicationCheck, sqlConnection);
                    if(check.ExecuteScalar() != null)
                    {
                        // Details exist. This is not a possible registration.
                        LoginStatus = Status.Unavailable;
                        sqlConnection.Close();
                        return;
                    }
                    // We should now write to the server. The primary key (key) will be autoincremented database-side.
                    string query = @"INSERT INTO `accounts` (`username`, `email`, `password`, `firstname`, `surname`) VALUES ('" + details[0] + "', '" + details[4] + "', '" + ((PasswordBox)details[1]).Password + "', '" + details[2] + "', '" + details[3] + "');";
                    MySqlCommand addition = new MySqlCommand(query, sqlConnection);
                    addition.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e);
                    // We could not connect to the SQL server! Display this and return.
                    LoginStatus = Status.Failed;
                    sqlConnection.Close();
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e);
                    // We could not connect to the SQL server! Display this and return.
                    LoginStatus = Status.Failed;
                    sqlConnection.Close();
                }
            });
            if (LoginStatus == Status.Ongoing)
            {
                // The account has been created!
                LoginStatus = Status.Completed;
                LobbyAddPlayer(Tuple.Create((string)details[0], (int)details[5], true));
            }
        }
        private void _JoinAsGuest(object sender)
        {
            object[] details = (object[])sender;
            LoginStatus = Status.Completed;
            LobbyAddPlayer(Tuple.Create((string)details[0], (int)details[1], false));
        }
        private void _ResetLoginStatus(object sender)
        {
            LoginStatus = Status.Pending;
        }
        #endregion

        #endregion

        #region Command predicates
        /// <summary>
        /// Predicate to determine whether or not the LOGIN can execute
        /// </summary>
        Predicate<object[]> CanLoginExecute = (object[] LoginCredidentials) =>
            ((string)LoginCredidentials[0] != "" && // Username
            ((PasswordBox)LoginCredidentials[1]).Password != "" && // Password
            (int)LoginCredidentials[2] != -1); // Icon select box
        /// <summary>
        /// Predicate to determine whether or not the REGISTER can execute
        /// </summary>
        Predicate<object[]> CanRegisterExecute = (object[] RegisterCredidentials) =>
            ((string)RegisterCredidentials[0] != "" && // Username
            ((PasswordBox)RegisterCredidentials[1]).Password != "" && // Password
            (string)RegisterCredidentials[2] != "" && // First name(s)
            (string)RegisterCredidentials[3] != "" && // Surname(s)
            (string)RegisterCredidentials[4] != "" && // Email address
            (int)RegisterCredidentials[5] != -1); // Icon select box
        /// <summary>
        /// Predicate to determine whether or not the GUEST can execute
        /// </summary>
        Predicate<object[]> CanJoinAsGuestExecute = (object[] RegisterCredidentials) =>
            ((string)RegisterCredidentials[0] != "" && // Username
            (int)RegisterCredidentials[1] != -1); // Icon select box
        /// <summary>
        /// Predicate to determine whether or not a new player can be added
        /// </summary>
        Predicate<object[]> CanNewPlayerAdd = (object[] PlayerList) =>
        PlayerList != null &&
        ((List<Player>)PlayerList[0]).Count() < 6;
        #endregion

        /// <summary>
        /// Yes-no settings for a MetroDialog.
        /// </summary>
        public static MetroDialogSettings YesNoSettings = new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", ColorScheme = MetroDialogColorScheme.Accented };

        #region Private subroutines
        /// <summary>
        /// Add a player to the lobby.
        /// </summary>
        /// <param name="PlayerData">Must contain the Name, Icon (as a string) and a true/false online value in the tuple.</param>
        private async void LobbyAddPlayer(Tuple<string, int, bool> PlayerData)
        {
            // Check that a player with this name isn't already connected first!
            string[] Icons = { "Battleship", "Boot", "Car", "Cat", "Dog", "Thimble", "Top Hat", "Wheelbarrow" };
            foreach (Player player in Players)
            {
                if(player.Name == PlayerData.Item1 || player.Icon == Icons[PlayerData.Item2])
                {
                    // This player (name) is already connected/used or that icon is already taken!
                    LoginStatus = Status.Unavailable;
                    return;
                }
            }
            Players.Add(new Player { Name = PlayerData.Item1, Digit = (Players.Count + 1), Icon = Icons[PlayerData.Item2], Online = PlayerData.Item3 });
            // Close flyout here, waiting for a short period first (for visual effect)
            await Task.Delay(500); FlyoutOpen = false; LoginStatus = Status.Pending;
        }
        /// <summary>
        /// Remove a player to the lobby.
        /// </summary>
        /// <param name="sender">Must be an object of the Player class, the player you wish to remove.</param>
        private async void LobbyRemovePlayer(object sender)
        {
            Player subtract = (Player)sender;
            // Are we sure the player wants to do this?
            var DialogResult = await Dialogs.ShowMessageAsync(this, "Are you sure?", "You're about to delete " + subtract.Name + " and remove them from this lobby. This cannot be undone - are you sure?", MessageDialogStyle.AffirmativeAndNegative, YesNoSettings);
            // Check for the answer...
            if(DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            }
            // The player would like to continue with the operation. Remove the player...
            ObservableCollection<Player> newPlayers = new ObservableCollection<Player>();
            foreach(Player player in Players)
            {
                if(player == subtract)
                {
                    subtract = null;
                } 
                else
                {
                    if(subtract == null)
                    {
                        player.Digit--;
                    }
                    newPlayers.Add(player);
                }
            }
            Players = newPlayers; OnPropertyChanged("Players");
        }
        /// <summary>
        /// Clear the lobby list
        /// </summary>
        /// <param name="sender"></param>
        private async void _ClearList(object sender)
        {
            // Are we sure the player wants to do this?
            var DialogResult = await Dialogs.ShowMessageAsync(this, "", "Are you sure you want to clear the list? This will immediately delete all added players (and cannot be undone)!", MessageDialogStyle.AffirmativeAndNegative, YesNoSettings);
            // Check for the answer...
            if (DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            }
            Players.Clear();
        }
        /// <summary>
        /// Start the game!
        /// </summary>
        /// <param name="sender"></param>
        private async void StartGame(object sender)
        {
            // Check if we are starting a new game or loading an old game.
            if (Players.Count == 0)
            {
                // We are loading an old game.
                // Open the file dialog within a try-catch (to detect for improper closure).
                try
                {
                    OpenFileDialog openSavegame = new OpenFileDialog();
                    openSavegame.Filter = "Savegame files (*.monopoly)|*.monopoly";
                    openSavegame.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Monopoly\\Saves";
                    openSavegame.ShowDialog();
                    if (openSavegame.FileName == null)
                    {
                        return;
                    }
                    FileStream savegame = File.Open(openSavegame.FileName, FileMode.Open);
                    BinaryFormatter formatter = new BinaryFormatter();
                    //It serialize the employee object  

                    object newObject = formatter.Deserialize(savegame);
                    MonopolyHandler handler = (MonopolyHandler)newObject;
                    // Create the new window
                    MonopolyWindow NewMonopolyWindow = new MonopolyWindow();
                    // Use the deserialised handler to launch back into the game
                    NewMonopolyWindow.DataContext = new MonopolyWindowViewModel(DialogCoordinator.Instance, handler);
                    // Let's go!
                    NewMonopolyWindow.Show();
                    await Task.Delay(1000);
                    if (NewMonopolyWindow.IsActive)
                    {
                        Close();
                    }
                    else
                    {
                        await Dialogs.ShowMessageAsync(this, "", "That savefile appears corrupted or otherwise incompatible with this version of Monopoly. Please try again, load a different savefile, or begin a new game.", MessageDialogStyle.Affirmative);
                    }
                    savegame.Close();
                }
                catch (ArgumentException)
                {
                    await Dialogs.ShowMessageAsync(this, "", "Please ensure you have selected a savefile compatible with this version of Monopoly.", MessageDialogStyle.Affirmative);
                }
            }
            else if (Players.Count == 1)
            {
                return; // Not enough players to begin!
            }
            else
            {
                // Check if a board is selected.
                if(BoardDirectory == null)
                {
                    await Dialogs.ShowMessageAsync(this, "", "You haven't selected a board to play on yet!");
                    return;
                }
                MonopolyWindow NewMonopolyWindow = new MonopolyWindow();
                // Assign DataContext, within the MVVM pattern
                NewMonopolyWindow.DataContext = new MonopolyWindowViewModel(DialogCoordinator.Instance, null, BoardDirectory, Players.ToList<Player>(), Settings);
                NewMonopolyWindow.Show();
                Close();
            }
        }
        /// <summary>
        /// Populates the AvailableBoards 
        /// </summary>
        /// <param name="sender"></param>
        private ObservableCollection<BoardData> GetAvailableBoards(object sender = null)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Monopoly\\Boards";
            Directory.CreateDirectory(path);
            string[] boardsAvailable = Directory.GetFiles(path, "*.mboard");
            ObservableCollection<BoardData> boards = new ObservableCollection<BoardData>();
            foreach(string board in boardsAvailable)
            {
                var newBoard = JsonConvert.DeserializeObject<BoardData>(File.ReadAllText(board));
                newBoard.BoardDirectory = board;
                boards.Add(newBoard);
            }
            return boards;
        }
        /// <summary>
        /// Selects a board.
        /// </summary>
        /// <param name="sender"></param>
        private void _SelectBoard(object sender)
        {
            BoardData data = (BoardData)sender;
            BoardDirectory = data.BoardDirectory;
        }
        /// <summary>
        /// Allows for the (board) editor to be opened.
        /// </summary>
        /// <param name="sender"></param>
        private void _OpenEditor(object sender)
        {
            EditorWindow newEditorWindow = new EditorWindow();
            EditorWindowViewModel newEditorWindowViewModel = new EditorWindowViewModel(Dialogs);
            newEditorWindowViewModel.Close = new Action(() => newEditorWindow.Close());
            newEditorWindow.DataContext = newEditorWindowViewModel;
            newEditorWindow.Show();
        }
        #endregion

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// PropertyChanged event, required for implementation
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// OnPropertyChanged event can be called with the property name to update components on the View bound to the aforementioned property
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            // This is a cheat-y way of continually refreshing the list. I'm doing it anyway.
            AvailableBoards = GetAvailableBoards();
            var e = new PropertyChangedEventArgs(propertyName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        #endregion
    }
}
