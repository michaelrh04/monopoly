using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Monopoly.Menu;

namespace Monopoly.Game
{
    /// <summary>
    /// The MonopolyHandler is the class that holds information about (and, therefore, manages) the game of Monopoly.
    /// </summary>
    [Serializable]
    public class MonopolyHandler : INotifyPropertyChanged
    {
        #region Constructor
        [NonSerialized]
        public MonopolyWindowViewModel ViewModel;
        public MonopolyHandler(MonopolyWindowViewModel parentViewModel, Dictionary<string, object> settings)
        {
            Console.WriteLine("[DEBUG] Constructing the MonopolyHandler.");
            ViewModel = parentViewModel;
            Settings = settings;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// The board configuration. Used to determine number of property sets per player and the basic board configuration.
        /// </summary>
        public Gameboard BoardConfiguration;
        /// <summary>
        /// The container for all properties to be displayed on the board.
        /// </summary>
        public ObservableCollection<Location> Board { get; set; }
        /// <summary>
        /// The lit of players playing the game.
        /// </summary>
        public List<Player> Players { get; private set; } = new List<Player>();
        /// <summary>
        /// The current player (whose turn it is).
        /// </summary>
        public Player CurrentPlayer
        {
            get
            {
                return _CurrentPlayer;
            }
            set
            {
                if (value != _CurrentPlayer)
                {
                    _CurrentPlayer = value;
                    OnPropertyChanged("CurrentPlayer");
                }
            }
        }
        /// <summary>
        /// Stores chance cards.
        /// </summary>
        public Queue<Card> ChanceCards;
        /// <summary>
        /// Stores community chest cards.
        /// </summary>
        public Queue<Card> CommunityChestCards;
        /// <summary>
        /// Defines whether or not the cover is open,
        /// </summary>
        public bool CoverOpen
        {
            get
            {
                return _CoverOpen;
            }
            set
            {
                if (value != _CoverOpen)
                {
                    _CoverOpen = value;
                    OnPropertyChanged("CoverOpen");
                }
            }
        }
        /// <summary>
        /// A tuple containing both the dice rolls 1 and 2.
        /// </summary>
        public Tuple<int, int> Roll
        {
            get
            {
                return _Roll;
            }
            set
            {
                if (value != _Roll)
                {
                    _Roll = value;
                    OnPropertyChanged("Roll");
                }
            }
        }
        /// <summary>
        /// To check whether or not a players turns have been expended.
        /// </summary>
        public bool RollsComplete
        {
            get 
            {
                if(_RollsRemaining <= 0 || AnimationOngoing)
                {
                    // You cannot roll again/yet!
                    return false;
                } 
                else
                {
                    if(Board[CurrentPlayer.Location] is Property property)
                    {
                        if (property.Owner == null)
                        {
                            // The property is unowned! You cannot roll again yet.
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
        /// <summary>
        /// The settings of the current game in progress. This can be a static variable: it is the same for all games we have.
        /// </summary>
        public Dictionary<string, object> Settings { get; private set; }
        /// <summary>
        /// List of bids.
        /// </summary>
        public ObservableCollection<Tuple<Player, int>> Bids 
        { 
            get
            {
                return _Bids;
            }
            set
            {
                if (value != _Bids)
                {
                    _Bids = value;
                    OnPropertyChanged("Bids");
                }
            }
        }
        /// <summary>
        /// Get the player waiting to make their auction bid.
        /// </summary>
        public Player NextAuctionPlayer
        {
            get
            {
                if(Bids.Count == 0) { return CurrentPlayer; }
                if (Bids[Bids.Count - 1].Item1 == Players[CurrentAuctionPlayerIndex])
                {
                    // The current player has just made a play. Move onto the next one.
                    CurrentAuctionPlayerIndex = CurrentAuctionPlayerIndex == (Players.Count - 1) ? CurrentAuctionPlayerIndex = 0 : CurrentAuctionPlayerIndex += 1;
                    while (Players[CurrentAuctionPlayerIndex].Digit == -1 || WithdrawnPlayers.Contains(Players[CurrentAuctionPlayerIndex]))
                    {
                        CurrentAuctionPlayerIndex++;
                        if (CurrentAuctionPlayerIndex == Players.Count) { CurrentAuctionPlayerIndex = 0; }
                        Console.WriteLine("[DEBUG] Triggering WHILE clause.");
                    }
                }
                return Players[CurrentAuctionPlayerIndex];
            }
        }
        /// <summary>
        /// Returns the largest bid yet made.
        /// </summary>
        public int MaximumBid
        {
            get
            {
                int value = 0;
                foreach(Tuple<Player, int> bid in Bids)
                {
                    value += bid.Item2;
                }
                return value;
            }
        }
        /// <summary>
        /// An integer representing the number of actions left unresolved. Once this number equals 0, the player can opt to end their turn.
        /// </summary>
        public int ActionsUnresolved
        {
            get
            {
                if (CurrentPlayer.Balance < 0)
                {
                    return 1;
                }
                else
                {
                    return _ActionsUnresolved;
                }
            }
            set
            {
                if (value != _ActionsUnresolved)
                {
                    _ActionsUnresolved = (value < 0 ? 0 : value); // Do not go below 0
                    OnPropertyChanged("ActionsUnresolved");
                    Console.WriteLine("[DEBUG] Remaining actions unresolved: " + _ActionsUnresolved);
                }
            }
        }
        /// <summary>
        /// Representing whether or not the auction is ongoing.
        /// </summary>
        public bool AuctionOngoing
        {
            get
            {
                return _AuctionOngoing;
            }
            set
            {
                if (value != _AuctionOngoing)
                {
                    _AuctionOngoing = value;
                    OnPropertyChanged("AuctionOngoing");
                }
            }
        }
        /// <summary>
        /// Represents the amount of money being offered FROM the current player in a trade.
        /// </summary>
        public int FromCurrentPlayerMoneyTrade { get; set; }
        /// <summary>
        /// Represents the amount of money being offered TO the current player in a trade.
        /// </summary>
        public int ToCurrentPlayerMoneyTrade { get; set; }
        /// <summary>
        /// Boolean value for whether the game is over or not.
        /// </summary>
        public bool GameOver
        {
            get
            {
                return _GameOver;
            }
            set
            {
                if (value != _GameOver)
                {
                    _GameOver = value;
                    OnPropertyChanged("GameOver");
                }
            }
        }
        /// <summary>
        /// Defines whether or not the MovePlayer subroutine is conducting an animation.
        /// </summary>
        public bool AnimationOngoing
        {
            get
            {
                return _AnimationOngoing;
            }
            set
            {
                if (value != _AnimationOngoing)
                {
                    _AnimationOngoing = value;
                    OnPropertyChanged("AnimationOngoing");
                    OnPropertyChanged("RollsComplete");
                    OnPropertyChanged("ActionsUnresolved");
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand RollDice
        {
            get
            {
                return new RelayCommand(_RollDice);
            }
        }
        public RelayCommand AddBidOne
        {
            get
            {
                return new RelayCommand(_IncreaseBid, null, CanAddBidOne);
            }
        }
        public RelayCommand AddBidTen
        {
            get
            {
                return new RelayCommand(_IncreaseBid, null, CanAddBidTen);
            }
        }
        public RelayCommand AddBidOneHundred
        {
            get
            {
                return new RelayCommand(_IncreaseBid, null, CanAddBidOneHundred);
            }
        }
        public RelayCommand WithdrawAuction
        {
            get
            {
                return new RelayCommand(_IncreaseBid);
            }
        }
        public RelayCommand Trade { 
            get
            {
                return new RelayCommand(_Trade);
            } 
        }
        #endregion

        #region Predicates and requirements
        private bool CanAddBidOne() { return NextAuctionPlayer.Balance >= MaximumBid + 1; }
        private bool CanAddBidTen() { return NextAuctionPlayer.Balance >= MaximumBid + 10; }
        private bool CanAddBidOneHundred() { return NextAuctionPlayer.Balance >= MaximumBid + 100; }
        #endregion

        #region Private properties
        private Random Randomiser = new Random();
        private Player _CurrentPlayer;
        private bool _CoverOpen;
        private int CurrentAuctionPlayerIndex;
        // For the game controls (e.g. dice rolls).
        public bool HasPaidRent;
        public int CumulativeTaxation;
        private Tuple<int, int> _Roll;
        public int _RollsRemaining;
        private int DoubleRollCounter;
        private int _ActionsUnresolved;
        private bool _AuctionOngoing;
        private bool _GameOver;
        private bool _AnimationOngoing;
        private ObservableCollection<Tuple<Player, int>> _Bids = new ObservableCollection<Tuple<Player, int>>();
        private List<Player> WithdrawnPlayers = new List<Player>();
        #endregion

        #region Subroutines
        #region Basic game functionality
        /// <summary>
        /// Initalise a player, ready to start the game.
        /// </summary>
        /// <param name="player"></param>
        public void InitPlayer(Player player)
        {
            // To do this, we pass the gameboard into the Setup() subroutine.
            // This will add a dictionary reference for every set on the board (e.g. a KeyPair for "Brown", and an empty Property list associated with it).
            // This setup uses the BoardConfiguration previously defined.
            player.Setup(BoardConfiguration);
            Players.Add(player);
            player.Balance = int.Parse(Settings["amount_begin_with"].ToString()) - (int.Parse(Settings["passing_go_amount"].ToString()) * int.Parse(Settings["passing_go_amount_multiplier"].ToString()));
            MovePlayer(player, -1, 0);
        }
        private async void _RollDice(object sender)
        {
            // Disable the roll dice button
            int tempRollsRemaining = _RollsRemaining;
            _RollsRemaining = 0; OnPropertyChanged("RollsComplete");
            int visualDelay = 50;
            // Now, check if jail conditions apply. If not, proceed with the rolling - but if yes and rules apply (3), must pay £50
            #region Jailing handling
            if (CurrentPlayer.IsJailed)
            {
                if(CurrentPlayer.JailedTurns == 3)
                {
                    MetroDialogSettings paySettings = new MetroDialogSettings()
                    {
                        AffirmativeButtonText = CurrentPlayer.Balance >= 50 ? "Post £50 bail" : "Inadequate funds",
                        NegativeButtonText = "Cancel payment",
                    };
                    var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "You've run out of chances to roll a double and gain parole! You must now pay a £50 release fee in order to leave jail.", MessageDialogStyle.AffirmativeAndNegative, paySettings);
                    if (DialogResult == MessageDialogResult.Affirmative)
                    {
                        if (CurrentPlayer.Balance >= 50)
                        {
                            CurrentPlayer.Balance -= 50;
                            Jail(false, CurrentPlayer);
                            _RollsRemaining = tempRollsRemaining;
                            ViewModel.ForcePropertyChanged();
                            _RollDice(sender); // Restart the process
                            return;
                        }
                    }
                    else
                    {
                        // Change nothing. The player will have to move through this process to proceed.
                        _RollsRemaining = tempRollsRemaining;
                        ViewModel.ForcePropertyChanged();
                        return;
                    }
                } else
                {
                    if(CurrentPlayer.Balance >= 50)
                    {
                        var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "You can afford the release fee! Would you like to leave jail immediately, paying a £50 release fee? If not, you will have to roll a double to leave (you have " + (3 - CurrentPlayer.JailedTurns) + " chances remaining).", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
                        if (DialogResult == MessageDialogResult.Affirmative)
                        {
                            CurrentPlayer.Balance -= 50;
                            Jail(false, CurrentPlayer);
                            _RollsRemaining = tempRollsRemaining;
                            ViewModel.ForcePropertyChanged();
                            _RollDice(sender); // Restart the process
                            return;
                        }
                        // If not, proceed with rolling as normal.
                    }
                    // If not, proceed with rolling as normal.
                }
            }
            #endregion
            // Proceed with the rolling
            for (int i = 0; i < 15; i++)
            {
                Roll = Tuple.Create(Randomiser.Next(1, 7), Randomiser.Next(1, 7));
                await Task.Delay(visualDelay); 
                // Make the next visual update take slightly longer.
                visualDelay += 25;
            }
            #region Jail double handing
            if (CurrentPlayer.IsJailed)
            {
                if(Roll.Item1 == Roll.Item2)
                {
                    // The player has broken from jail! Do not terminate the process.
                    Jail(false, CurrentPlayer);
                    DoubleRollCounter++;
                } 
                else
                {
                    CurrentPlayer.JailedTurns++;
                    // The player has failed to secure his freedom. Do not move the piece, and end the roll.
                    _RollsRemaining = tempRollsRemaining;
                    _RollsRemaining--; ActionsUnresolved--;
                    ViewModel.ForcePropertyChanged();
                    return;
                }
            } else
            {
                if(Roll.Item1 == Roll.Item2)
                {
                    DoubleRollCounter++;
                    if(DoubleRollCounter == 3)
                    {
                        Jail(true, CurrentPlayer);
                    }
                }
            }
            #endregion
            // The final roll has been decided.
            _RollsRemaining = tempRollsRemaining;
            Console.WriteLine("There are now " + _RollsRemaining + " rolls remaining.");
            int destination = CurrentPlayer.Location + Roll.Item1 + Roll.Item2;
            if(destination > 39) { destination -= 40; }
            // Move the player!
            MovePlayer(CurrentPlayer, CurrentPlayer.Location, destination);
            if (Roll.Item1 != Roll.Item2)
            {
                // Not a double. We can subtract from rolls remaining.
                _RollsRemaining--; ActionsUnresolved--;
                // Actions unresolved should be left untouched by default, unless the property IS a property.
                // In this case, it should be incremented. This is already done in the MovePlayer() subroutine.
            }
        }
        /// <summary>
        /// Move a player.
        /// </summary>
        /// <param name="player">Player to be moved.</param>
        /// <param name="from">Index to be moved from (to clear player from that list).</param>
        /// <param name="destination">Index to be moved to.</param>
        public async void MovePlayer(Player player, int from, int destination)
        {
            // Cement the player's move to the final tile.
            player.Location = destination;
            // Remove the player from the 'from' location.
            if (from == -1)
            {
                // If it does not equal -1, handle it (this has come from another tile on the board).
                // From = -1 when the player is being added from nowhere.
                Board[destination].Arrive(player);
                // This needs no animation; cancel it.
                return;
            } 
            else
            {
                // Ensure departure
                Board[destination].Depart(player);
            }
            // This section makes extensive use of color animations. These can be defined first and used in many situations afterwards.
            ColorAnimation FadeInColour = new ColorAnimation
            {
                From = Colors.LightGray,
                To = Color.FromRgb(254, 255, 153),
                Duration = new TimeSpan(0, 0, 0, 0, 500)
            };
            ColorAnimation FadeOutColour = new ColorAnimation
            {
                From = Color.FromRgb(254, 255, 153),
                To = Colors.LightGray,
                Duration = new TimeSpan(0, 0, 0, 0, 500)
            };
            // We also need to cancel any animation if the player has been sent to jail; these are instant actions.
            if (player.IsJailed)
            {
                Console.WriteLine("[DEBUG] This player, " + player.Name + ", is jailed.");
                ((Location)Board[destination]).Arrive(player);
                ((Location)Board[from]).PropertyBackground.BeginAnimation(SolidColorBrush.ColorProperty, FadeOutColour);
                return;
            }
            // Loop through all the properties from 'from' to 'destination' to provide that special graphical glow.
            // This gives the impression of a piece in movement.
            AnimationOngoing = true;
            int destination_loop = destination;
            if(destination < from) { destination_loop += 40; }
            ((Location)Board[destination]).PropertyBackground.BeginAnimation(SolidColorBrush.ColorProperty, FadeInColour);
            // Loop through all of the properties once, making the needed ones coloured, but waiting for the animations to finish first.
            await Task.Delay(1000);
            ((Location)Board[from]).Depart(player);
            #region Colouring in
            for (int i = from + 1; i < destination_loop; i++)
            {
                var j = i;
                if(i > 39) { j = i - 40; }
                ((Location)Board[j]).PropertyBackground.BeginAnimation(SolidColorBrush.ColorProperty, FadeInColour);
                await Task.Delay(75);
            }
            #endregion
            #region Colouring back out
            // Now move the player to the end, erasing the colours applied to the properties en route.
            ((Location)Board[destination]).Arrive(player);
            for (int i = from; i < destination_loop; i++)
            {
                var j = i;
                if (i > 39) { j = i - 40; }
                ((Location)Board[j]).PropertyBackground.BeginAnimation(SolidColorBrush.ColorProperty, FadeOutColour);
                await Task.Delay(75);
            }
            // Select the property landed upon if applicable.
            if((Location)Board[destination] is Property property)
            {
                ViewModel.SelectedProperty = property;
                if (property.Owner != player)
                {
                    ActionsUnresolved++;
                    if(property.IsMortgaged || (property.Owner.IsJailed && ((bool)Settings["allow_rent_collection_while_jailed"]) == false))
                    {
                        HasPaidRent = true;
                        ActionsUnresolved--;
                    } else
                    {
                        HasPaidRent = false;
                    }
                    
                }
            }
            #endregion
            AnimationOngoing = false;

            //
            //
            //

            // Now we need to handle the game mechanics as well.
            // First, has the player passed - but not landed upon - GO? Check for this.
            if (destination < from && destination != 0)
            {
                // The conditions are true!
                // Grant the player the passing_go_amount.
                player.Balance += int.Parse(Settings["passing_go_amount"].ToString());
            }
            ViewModel.ForcePropertyChanged();
        }
        /// <summary>
        /// Call the commencing of the game!
        /// </summary>
        public void Start(bool oldGame)
        {
            // Choose a random player.
            Console.WriteLine("[DEBUG] Players playing this game: " + Players.Count());
            if (!oldGame)
            {
                // Shuffle the list!!!
                Player[] tempPlayerList = new Player[Players.Count];
                int successes = 0;
                do
                {
                    int newIndex = Randomiser.Next(0, Players.Count);
                    if (tempPlayerList[newIndex] == null)
                    {
                        // Index not used.
                        tempPlayerList[newIndex] = Players[successes];
                        successes++;
                    }
                    // Else, the index is taken, try again.
                }
                while (successes < Players.Count);
                Players = tempPlayerList.ToList();
                // Then, shuffle the community chest and chance card queues
                // This may come as a shock to the FIFO structure, but remember that they are only shuffled once and then MUST be kept in a queue
                // Hence, I believe it is still appropriate to do
                Random rnd = new Random();
                Card[] chanceArray = ChanceCards.ToArray<Card>().OrderBy(x => rnd.Next()).ToArray();
                Card[] cChestArray = CommunityChestCards.ToArray<Card>().OrderBy(x => rnd.Next()).ToArray();
                // Restock!
                ChanceCards = new Queue<Card>();
                CommunityChestCards = new Queue<Card>();
                foreach(Card chance in chanceArray)
                {
                    ChanceCards.Enqueue(chance);
                }
                foreach (Card cCard in cChestArray)
                {
                    CommunityChestCards.Enqueue(cCard);
                }
                // Start the game!
                CurrentPlayer = Players[0];
                Console.WriteLine("[DEBUG] Attempting to start the game.");
                GameOver = false;
            }
            else
            {
                // This is an old game, so we don't need to set anything up.
                // However, we do need to re-assign ViewModels, which are removed during saving.
                foreach(Location location in Board)
                {
                    location.ViewModel = ViewModel;
                    // And now we're good to go!
                }
            }
            _NextTurn();
        }
        /// <summary>
        /// Next turn subroutine.
        /// </summary>
        public async void _NextTurn(object sender = null)
        {
            Console.WriteLine("[DEBUG] It's the next player's turn!");
            // Ensure that the cover is up and no auctions are ongoing!
            CoverOpen = true;
            AuctionOngoing = false;
            Console.WriteLine("[DEBUG] Cover deploying.");
            // Wait for cover to extend and select a new player.
            await Task.Delay(300);
            Console.WriteLine("[DEBUG] Cover deployed.");
            // Continue!
            // But we need to ensure that the newly selected player is not bankrupt already.
            // This can be done by re-ordering the list (which is required anyway for graphical effect) and elevating the next non-bankrupt player.
            // If a 'null' value is returned, however, a player must have won the game!
            Console.WriteLine("[DEBUG] Attempting to configure the ordering of the list.");
            var newList = GetListOrder();
            if (newList == null) { throw new NotImplementedException(); } 
            // The above would only trigger in the event of a game ending. As a result, it should never fire (the game should end before this).
            Players = newList;
            OnPropertyChanged("Players");
            // And the list of players should now be ordered to put the current player at the very top and bankrupt players at the very bottom.
            // The next player can now be selected.
            Console.WriteLine("[DEBUG] Configuration complete, selecting new player.");
            ViewModel.SelectedProperty = null;
            CurrentPlayer = Players[0];
            _RollsRemaining = 1; OnPropertyChanged("RollsComplete");
            // Since as it is mandatory a person must roll, we also need to set one as ActionsUnresolved.
            ActionsUnresolved = 1;
            Console.WriteLine("[DEBUG] Selection complete ("+CurrentPlayer.Name+"), executing visual effects.");
            // The location of a player is gently coloured. Remove previous colours and apply the new player's colour now.
            // This resets all colouring from all tiles, incl. ones currently yellow.
            foreach (Location location in Board)
            {
                location.PropertyBackground = new SolidColorBrush(Colors.LightGray);
                if (location.Occupants.Contains(CurrentPlayer))
                {
                    // Make sure we re-add a glow to this tile, as the current player is here.
                    location.PropertyBackground = new SolidColorBrush(Color.FromRgb(254, 255, 153));
                }
            }
            // Reset variables that need resetting:
            Roll = Tuple.Create(0, 0);
            DoubleRollCounter = 0;
            HasPaidRent = true; // By default
            foreach (Player p in Players)
            {
                // Reset the balance changed figure to clear values from the previous turn
                p.BalanceChanged = 0;
            }
            Console.WriteLine("[DEBUG] Execution complete. Ready to go.");
            ViewModel.ForcePropertyChanged();
        }
        /// <summary>
        /// Save and quit the game.
        /// </summary>
        /// <param name="_sender"></param>
        public async void _Save(object _sender)
        {
            if (ActionsUnresolved > 0)
            {
                // No! You can only save the game when your turn is finished.
                // This is incredibly round-about, as we need to call it from the handler (DataContext for dialogs is the handler) and yet the handler references this!
                await ViewModel.Dialogs.ShowMessageAsync(this, "", "You cannot save the game and exit until the current player's turn is complete. Please complete this player's turn and try again.", MessageDialogStyle.Affirmative);
                return;
            }
            SaveFileDialog saveGame = new SaveFileDialog();
            saveGame.Title = "Choose where to save your Monopoly game";
            saveGame.DefaultExt = "monopoly";
            saveGame.Filter = "Savegame files (*.monopoly)|*.monopoly";
            // Ensure a target directory has been created.
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Monopoly\\Saves");
            saveGame.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Monopoly\\Saves";
            if (saveGame.ShowDialog() == true)
            {
                Stream ms = File.OpenWrite(saveGame.FileName);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Close();
            }
            MetroDialogSettings mdS = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Resume game",
                NegativeButtonText = "Save and exit",
                ColorScheme = MetroDialogColorScheme.Accented
            };
            var choice = await ViewModel.Dialogs.ShowMessageAsync(this, "", "Your game has been saved to the chosen directory, under the name '" + saveGame.SafeFileName + "'. Would you like to continue playing or exit the application?", MessageDialogStyle.AffirmativeAndNegative, mdS);
            if (choice == MessageDialogResult.Negative)
            {
                // Close the program.
                Environment.Exit(0);
            }
        }
        /// <summary>
        /// Handles the drawing of chance/chest cards.
        /// </summary>
        /// <param name="cardType">0: CHANCE card, 1: COMMUNITY CHEST card</param>
        public async void DrawCard(int cardType)
        {
            Card drawnCard = cardType == 0 ? ChanceCards.Dequeue() : CommunityChestCards.Dequeue();
            MetroDialogSettings drawnCardSettings = new MetroDialogSettings
            {
                AffirmativeButtonText =
                    (drawnCard.Type == EnumCardType.AdvanceTo || drawnCard.Type == EnumCardType.GoBackTo) ? "Move as directed" :
                    (drawnCard.Type == EnumCardType.Pay || drawnCard.Type == EnumCardType.PayOrPenalty || drawnCard.Type == EnumCardType.Recieve) ? "Proceed with payment" :
                    "Go to jail",
                NegativeButtonText = "Draw another card",
                ColorScheme = MetroDialogColorScheme.Accented,
            };

            #region Text manipulation
            string cardText = drawnCard.Text;
            var cardTextSplit = cardText.Split('%');
            for(int i = 0; i < cardTextSplit.Count(); i++)
            {
                try
                {
                    int replacementIndex = int.Parse(cardTextSplit[i]);
                    Property getProperty = Board[drawnCard.Amounts[0]] as Property;
                    cardTextSplit[i] = getProperty.Name;
                } 
                catch (FormatException)
                {
                    // Do nothing - this is not an integer. Continue with execution.
                }
            }
            cardText = string.Join(null, cardTextSplit);
            #endregion

            var choice = await ViewModel.Dialogs.ShowMessageAsync(this, cardType == 0 ? "You've drawn a chance card!" : "You've drawn a community chest card!", cardText, drawnCard.Type == EnumCardType.PayOrPenalty ? MessageDialogStyle.AffirmativeAndNegative : MessageDialogStyle.Affirmative, drawnCardSettings);
            switch(drawnCard.Type)
            {
                case EnumCardType.AdvanceTo:
                case EnumCardType.GoBackTo:
                    if(drawnCard.Amounts[0] >= 0)
                    {
                        MovePlayer(CurrentPlayer, CurrentPlayer.Location, drawnCard.Amounts[0]);
                    } 
                    else
                    {
                        if(drawnCard.Type == EnumCardType.AdvanceTo)
                        {
                            MovePlayer(CurrentPlayer, CurrentPlayer.Location, CurrentPlayer.Location + Math.Abs(drawnCard.Amounts[0]));
                        } 
                        else
                        {
                            MovePlayer(CurrentPlayer, CurrentPlayer.Location, CurrentPlayer.Location - Math.Abs(drawnCard.Amounts[0]));
                        }
                    }
                    break;
                case EnumCardType.Pay:
                    if(drawnCard.TargetsPlayers)
                    {
                        foreach(Player player in Players)
                        {
                            // Do not deduct and add to yourself (even if, technically, it doesn't matter)
                            if(player != CurrentPlayer)
                            {
                                // Deduct from the current player per player in the game
                                player.Balance += drawnCard.Amounts[0];
                                CurrentPlayer.Balance -= drawnCard.Amounts[0];
                            }
                        }
                    } 
                    else
                    {
                        // Deduct from the current player only into thin air
                        // We also need to check (it might be relative costs in this section)
                        if (drawnCard.Amounts.Count() > 1)
                        {
                            // This is a houses/hotels dependent cost card
                            int housesOwned = 0;
                            int hotelsOwned = 0;
                            foreach (KeyValuePair<string, ObservableCollection<Property>> set in CurrentPlayer.Inventory)
                            {
                                foreach(Property property in set.Value)
                                {
                                    if(property is Residence res)
                                    {
                                        if(res.Houses == 5)
                                        {
                                            hotelsOwned++;
                                        } else
                                        {
                                            housesOwned += res.Houses;
                                        }

                                    }
                                }
                            }
                            CurrentPlayer.Balance -= drawnCard.Amounts[0] * housesOwned;
                            CurrentPlayer.Balance -= drawnCard.Amounts[1] * hotelsOwned;
                        }
                        else
                        {
                            CurrentPlayer.Balance -= drawnCard.Amounts[0];
                        }
                    }
                    break;
                case EnumCardType.Recieve:
                    if (drawnCard.TargetsPlayers)
                    {
                        foreach (Player player in Players)
                        {
                            // Do not deduct and add to yourself (even if, technically, it doesn't matter)
                            if (player != CurrentPlayer)
                            {
                                // Deduct from the current player per player in the game
                                player.Balance -= drawnCard.Amounts[0];
                                CurrentPlayer.Balance += drawnCard.Amounts[0];
                            }
                        }
                    }
                    else
                    {
                        // Add to the current player only from thin air
                        CurrentPlayer.Balance += drawnCard.Amounts[0];
                    }
                    break;
                case EnumCardType.PayOrPenalty:
                    if(choice == MessageDialogResult.Affirmative)
                    {
                        goto case EnumCardType.Pay;
                    }
                    else
                    {
                        DrawCard(drawnCard.Amounts[1]);
                    }
                    break;
                case EnumCardType.Jail:
                    Jail(true, CurrentPlayer);
                    // If the player rolled a double, they would otherwise still need to take a turn. We need to undo this.
                    _RollsRemaining = 0;
                    OnPropertyChanged("RollsComplete");
                    break;

            }
            ActionsUnresolved--;
            if(Roll.Item1 != Roll.Item2)
            {
                // The card drawn has been resolved, with no more actions (rolls) remaining
                ActionsUnresolved = 0;
                _RollsRemaining = 0;
                OnPropertyChanged("RollsComplete"); 
            }
            // Finally, we must requeue the card!!!
            if(cardType == 0)
            {
                ChanceCards.Enqueue(drawnCard);
            } else
            {
                CommunityChestCards.Enqueue(drawnCard);
            }
        }
        #endregion
        #region Game controls
        /// <summary>
        /// Pay the rent owed.
        /// </summary>
        /// <param name="sender"></param>
        public void _PayRent(object sender)
        {
            HasPaidRent = true;
            int rentOwed = ViewModel.SelectedProperty.GetRentOwed();
            Console.WriteLine("[DEBUG] The rent owed is " + rentOwed);
            CurrentPlayer.Balance -= rentOwed;
            ViewModel.SelectedProperty.Owner.Balance += rentOwed;
            ActionsUnresolved--; OnPropertyChanged("PayRent");
            ViewModel.ForcePropertyChanged();
        }
        /// <summary>
        /// Purchase a property.
        /// </summary>
        /// <param name="sender"></param>
        public async void _PurchaseProperty(object sender)
        {
            // Ask if the user really wants to do this first:
            var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "Checkout", "You are about to purchase " + ViewModel.SelectedProperty.Name + " for £" + ViewModel.SelectedProperty.Price + ". Would you like to confirm the purchase?", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
            // and then check for the answer:
            if (DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            }
            // Continue! The player would like to continue with the operation...
            ViewModel.SelectedProperty.Purchase(CurrentPlayer);
            ViewModel.ForcePropertyChanged();
            ActionsUnresolved--;
        }
        /// <summary>
        /// Decline a property.
        /// </summary>
        /// <param name="sender"></param>
        public async void _DeclineProperty(object sender)
        {
            // Ask if the user really wants to do this first:
            var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "Are you sure?", "Are you sure you wish to decline the option to purchase " + ViewModel.SelectedProperty.Name + "? You won't be able to obtain it unless you win it in an auction, trade for it or land on it again in the future.", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
            // and then check for the answer:
            if (DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            }
            // Continue! The player would like to continue with the operation...
            if ((bool)Settings["do_trigger_auctions"])
            {
                AuctionOngoing = true;
                Bids = new ObservableCollection<Tuple<Player, int>>();
                WithdrawnPlayers = new List<Player>();
                OnPropertyChanged("MaximumBid");
                CurrentAuctionPlayerIndex = Players.IndexOf(CurrentPlayer);
                OnPropertyChanged("NextAuctionPlayer");
            }
            else
            {
                ViewModel.SelectedProperty = null;
                ActionsUnresolved--;
            }
            ViewModel.ForcePropertyChanged();
        }
        public async void _AddHouse(object sender)
        {
            // Ask if the user really wants to do this first:
            var target = ViewModel.SelectedProperty as Residence;
            var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "The local authority has given you permission to increase the number of properties on " + target.Name + ". Do you wish to approve the construction?", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
            // and then check for the answer:
            if (DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            } 
            else
            {
                target.Houses++;
                CurrentPlayer.Balance -= target.HouseIncrementationPrice;
            }
            ViewModel.ForcePropertyChanged();
        }
        public async void _RemoveHouse(object sender)
        {
            // Ask if the user really wants to do this first:
            var target = ViewModel.SelectedProperty as Residence;
            var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "The local authority has given you permission to proceed with a demolition of one property on " + target.Name + ", and will pay compensation that 50% the price of original construction. Do you wish to approve the demolition?", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
            // and then check for the answer:
            if (DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            }
            else
            {
                target.Houses--;
                CurrentPlayer.Balance += int.Parse((0.5 * target.HouseIncrementationPrice).ToString());
            }
            ViewModel.ForcePropertyChanged();
        }
        public async void _ToggleMortgage(object sender)
        {
            var property = ViewModel.SelectedProperty;
            if(property.IsMortgaged)
            {
                // THE PROPERTY IS MORTGAGED
                var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "The bank is prepared to sign away and return your mortgage on this property, which will return it to your full ownership (allowing you to collect rent) at a 10% interest rate. Would you like to proceed with it?", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
                // and then check for the answer:
                if (DialogResult == MessageDialogResult.Negative)
                {
                    // The player has cancelled the operation. Return.
                    return;
                }
                property.IsMortgaged = false;
                // This integer does not need try-catch error handling as all values for a Monopoly price will be integers after this calculation (all prices are multiples of 10 anyway).
                double subtraction = property.Price * 1.1;
                CurrentPlayer.Balance -= int.Parse(subtraction.ToString());
            } 
            else
            {
                // THE PROPERTY IS NOT MORTGAGED
                var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "The bank is prepared to authorise your mortgage on this property, which will prevent you from collecting future rent here. You will also have to pay 10% in the future to unmortgage the property. Would you like to proceed with it?", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
                // and then check for the answer:
                if (DialogResult == MessageDialogResult.Negative)
                {
                    // The player has cancelled the operation. Return.
                    return;
                }
                property.IsMortgaged = true;
                CurrentPlayer.Balance += property.Price;
            }
            ViewModel.ForcePropertyChanged();
        }
        public async void _IncreaseBid(object sender)
        {
            // Check to make sure this button hasn't been pressed after the auction has just ended
            if(Bids.Count > 0)
            {
                if (Bids[Bids.Count - 1].Item1 == null) { return; }
            }
            // Check if this is a genuine increase (=1,10,100) or a withdrawal (=0).
            var newBid = Tuple.Create(Players[CurrentAuctionPlayerIndex], int.Parse(sender.ToString()));
            if(newBid.Item2 == 0)
            {
                // This is a withdrawal.
                // But, we should only add the player to "WithdrawnPlayers" if they are not bankrupt.
                if(Players[CurrentAuctionPlayerIndex].Digit != -1)
                {
                    WithdrawnPlayers.Add(Players[CurrentAuctionPlayerIndex]);
                }
                Bids.Add(newBid);
                OnPropertyChanged("NextAuctionPlayer");
                if (WithdrawnPlayers.Count == Players.Where(p => p.Digit != -1).Count() - 1)
                {
                    // Only one player remains in the auction.
                    Console.WriteLine("[DEBUG] It appears that " + Players[CurrentAuctionPlayerIndex].Name + " has won the auction.");
                    Bids.Add(Tuple.Create<Player, int>(null, 0));
                    ViewModel.SelectedProperty.Purchase(Players[CurrentAuctionPlayerIndex], MaximumBid);
                    // Wait three seconds before dismissing the flyout.
                    await Task.Delay(3000);
                    AuctionOngoing = false;
                    // The player would now be required to pay rent; however, this is not how Monopoly auctions work
                    // Manually override the rent payment - it is unnecessary here - and resolve the pending activity.
                    HasPaidRent = true;
                    if (Roll.Item1 != Roll.Item2)
                    {
                        ActionsUnresolved = 0;
                    }
                    else
                    {
                        ActionsUnresolved = 1;
                    }
                }
                Console.WriteLine("[DEBUG] Player " + Players[CurrentAuctionPlayerIndex].Name + " has withdrawn from auction.");
                return;
            }
            else
            { 
            Console.WriteLine("Attempting to increase bid: " + newBid.Item1.Name + " with bid " + newBid.Item2);
            Bids.Add(newBid);
            }
            OnPropertyChanged("Bids");
            OnPropertyChanged("MaximumBid");
            OnPropertyChanged("NextAuctionPlayer");
        }
        public async void _Trade(object sender)
        {
            // Ask if the user really wants to do this first:
            var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "Are you ready to proceed with this trade? Make sure that both players agree with the trade on the table!", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
            // and then check for the answer:
            if (DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            }
            // Make the trade!
            Player partner = sender as Player;
            // First, transfer applicable properties from the current player to the trading partner.
            List<Property> transferredTo = new List<Property>();
            foreach(KeyValuePair<string, ObservableCollection<Property>> set in CurrentPlayer.Inventory) {
                foreach(Property property in set.Value)
                {
                    if(property.IsSelectedForTrade)
                    {
                        property.IsSelectedForTrade = false;
                        transferredTo.Add(property);
                    }
                }
            }
            foreach(Property property in transferredTo)
            {
                property.Purchase(partner, 0);
                if(property == Board[partner.Location] && partner == CurrentPlayer) // if the property being traded is the one the player is on, then
                {
                    HasPaidRent = true;
                }
            }
            CurrentPlayer.Balance -= FromCurrentPlayerMoneyTrade;
            partner.Balance += FromCurrentPlayerMoneyTrade;
            FromCurrentPlayerMoneyTrade = 0;
            // Then, transfer applicable properties from the trading partner to the current player.
            List<Property> transferredFrom = new List<Property>();
            foreach (KeyValuePair<string, ObservableCollection<Property>> set in partner.Inventory)
            {
                foreach (Property property in set.Value)
                {
                    if (property.IsSelectedForTrade)
                    {
                        property.IsSelectedForTrade = false;
                        transferredFrom.Add(property);
                    }
                }
            }
            foreach (Property property in transferredFrom)
            {
                property.Purchase(CurrentPlayer, 0);
            }
            CurrentPlayer.Balance += ToCurrentPlayerMoneyTrade;
            partner.Balance -= ToCurrentPlayerMoneyTrade;
            ToCurrentPlayerMoneyTrade = 0;
            // Update graphically
            ViewModel.ForcePropertyChanged();
            // Force change in ActionsUnresolved
            if(Roll.Item1 != Roll.Item2 && HasPaidRent)
            {
                ActionsUnresolved = 0;
            }
        }
        public async void _DeclareBankruptcy(object sender)
        {
            // Ask if the user really wants to do this first:
            var DialogResult = await ViewModel.Dialogs.ShowMessageAsync(this, "", "The bank has recieved your bankruptcy application and is ready to accept it. But hold on: are you absolutely confident you want to declare bankruptcy? This will remove you from the game, forfeitting all your properties and money, and CANNOT be undone.", MessageDialogStyle.AffirmativeAndNegative, MainMenuViewModel.YesNoSettings);
            // and then check for the answer:
            if (DialogResult == MessageDialogResult.Negative)
            {
                // The player has cancelled the operation. Return.
                return;
            }
            // Handle the bankruptcy!
            // Declare the player bankrupt.
            CurrentPlayer.Digit = -1;
            Board[CurrentPlayer.Location].Depart(CurrentPlayer);
            // Has the game ended yet?
            int playersLeft = 0;
            foreach (Player player in Players)
            {
                if (player.Digit != -1) { playersLeft++; }
            }
            if (playersLeft == 1)
            {
                // The game is over! Congratulations!
                foreach(Player player in Players)
                {
                    if(player.Digit != -1) { CurrentPlayer = player; break; }
                }
                Console.WriteLine("The game is over!");
                GameOver = true; return;
            }
            // First, we need to determine whether or not this player is bankrupt to the bank or another player. This is simple.
            Player playerBankruptTo = null;
            if(Board[CurrentPlayer.Location] is Property property && !HasPaidRent)
            {
                playerBankruptTo = (property.Owner == null || property.Owner == CurrentPlayer) ? null : property.Owner;
            }
            // We can check that variable to determine whether or not the properties are sent to the bank (null) or not.
            // First, regardless of who is bankrupt, we must wipe every single property of houses and add half the house price to balance.
            foreach (KeyValuePair<string, ObservableCollection<Property>> pair in CurrentPlayer.Inventory)
            {
                foreach (Property p in pair.Value)
                {
                    if(p is Residence res)
                    {
                        CurrentPlayer.Balance += (res.HouseIncrementationPrice * res.Houses) / 2;
                        res.Houses = 0;
                    }
                }
            }
            // Now we can reallocate the properties.
            if (playerBankruptTo == null)
            {
                // All the properties and cash are seized by the bank. Declare the player bankrupt and begin auctions on the remaining cards.
                foreach (KeyValuePair<string, ObservableCollection<Property>> pair in CurrentPlayer.Inventory)
                {
                    List<Property> auctionProperties = new List<Property>();
                    foreach (Property prop in pair.Value)
                    {
                        auctionProperties.Add(prop);
                    }
                    foreach(Property auctionTarget in auctionProperties)
                    {
                        ViewModel.SelectedProperty = auctionTarget;
                        AuctionOngoing = true;
                        Bids = new ObservableCollection<Tuple<Player, int>>();
                        WithdrawnPlayers = new List<Player>();
                        // The current player cannot participate: force them to drop out.
                        CurrentAuctionPlayerIndex = Players.IndexOf(CurrentPlayer);
                        _IncreaseBid(0);
                        OnPropertyChanged("MaximumBid");
                        OnPropertyChanged("NextAuctionPlayer");
                        do {
                            // Now we need to wait until the auction is complete; this involves hanging around until the Owner changes.
                            // But don't hang the UI thread! Check every second.
                            await Task.Delay(1000);
                        } while (auctionTarget.Owner == CurrentPlayer);
                        await Task.Delay(3000);
                        // Start the next auction.
                    }
                    // The auctions are complete, and the property transferrals complete too.
                }
            } 
            else
            {
                // Transfer all the properties and money to the new player.
                List<Property> properties = new List<Property>();
                foreach(KeyValuePair<string, ObservableCollection<Property>> pair in CurrentPlayer.Inventory)
                {
                    foreach (Property transfer in pair.Value)
                    {
                        properties.Add(transfer);
                    } 
                }
                foreach(Property p in properties)
                {
                    p.Purchase(playerBankruptTo, 0);
                }
                playerBankruptTo.Balance += CurrentPlayer.Balance;
                CurrentPlayer.Balance = 0;
                // Declare the current player bankrupt.
            }
            // The bankruptcy process is complete! 
            // Let's begin a new turn, and turn over a new leaf.
            _NextTurn();
        }
        public void Jail(bool isJailed, Player player)
        {
            player.IsJailed = isJailed;
            if (isJailed)
            {
                Board[player.Location].Depart(player);
                MonopolyWindowViewModel.Handler.MovePlayer(player, player.Location, 10);
                ActionsUnresolved = 0; _RollsRemaining = 0; OnPropertyChanged("RollsComplete");
            }
        }
        #endregion
        #region Background functionality
        private List<Player> GetListOrder()
        {
            var bankruptcies = new List<Player>();
            var remaining_players = new List<Player>();
            foreach(Player player in Players)
            {
                // Find the number of bankruptcies first and seperate them into a new list.
                if(player.Digit == -1)
                {
                    bankruptcies.Add(player);
                }
                else
                {
                    remaining_players.Add(player);
                }
            }
            // Time to check if the game is over yet.
            if(remaining_players.Count == 1)
            {
                // Game over!
                return null;
            }
            // If not, we must take the top player in the current players list and shuffle them to the bottom.
            var player_moving = remaining_players[0];
            remaining_players.RemoveAt(0);
            remaining_players.Add(player_moving);
            // List assembly is complete. Concatenate the bankruptcies onto the very end and we are good to go.
            remaining_players.AddRange(bankruptcies);
            return remaining_players;
        }
        public int GetHousesAvailable()
        {
            int maxAvailable = 32;
            // This can be completed efficiently with the knowledge that only properties with owners can have houses on them.
            foreach(Player player in Players)
            {
                foreach(KeyValuePair<string, ObservableCollection<Property>> pair in player.Inventory)
                {
                    foreach(Property p in pair.Value)
                    {
                        if(p is Residence residence)
                        {
                            // Only subtract if it doesn't have a hotel.
                            maxAvailable = residence.Houses == 5 ? maxAvailable : (maxAvailable - residence.Houses);
                        }
                    }
                }
            }
            return maxAvailable;
        }
        public int GetHotelsAvailable()
        {
            int maxAvailable = 12;
            // This can be completed efficiently with the knowledge that only properties with owners can have hotels on them.
            foreach (Player player in Players)
            {
                foreach (KeyValuePair<string, ObservableCollection<Property>> pair in player.Inventory)
                {
                    foreach (Property p in pair.Value)
                    {
                        if(p is Residence residence)
                        {
                            // Only subtract if it doesn't have a hotel.
                            maxAvailable = residence.Houses == 5 ? (maxAvailable - 1) : maxAvailable;
                        }
                    }
                }
            }
            return maxAvailable;
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
