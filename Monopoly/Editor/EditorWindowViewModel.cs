using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Monopoly.Game;
using Newtonsoft.Json;

namespace Monopoly.Editor
{
    class EditorWindowViewModel : INotifyPropertyChanged
    {
        #region Constructor
        public EditorWindowViewModel(IDialogCoordinator instance)
        {
            Dialogs = instance;
            StationRents = new ObservableCollection<int>();
            UtilityRents = new ObservableCollection<int>();
        }
        #endregion

        // To allow for the creation of dialogs in the View from the ViewModel.
        private IDialogCoordinator Dialogs;

        #region Public attributes
        public Action Close { get; set; }
        public ObservableCollection<GenericEditorItem> Board { get; set; }
        public ObservableCollection<int> StationRents { get; set; }
        public ObservableCollection<int> UtilityRents { get; set; }
        /// <summary>
        /// Using a set dictionary allows for both the monitoring of sets available and the colour codes for each set.
        /// </summary>
        public Dictionary<string, string> SetDictionary = new Dictionary<string, string>();
        public GenericEditorItem SelectedProperty { get
            {
                return _SelectedProperty;
            }
            set
            {
                if (value != _SelectedProperty)
                {
                    _SelectedProperty = value;
                    OnPropertyChanged("SelectedProperty");
                }
            }
        }
        public string BoardName
        {
            get
            {
                return _BoardName;
            }
            set
            {
                if(value != _BoardName)
                {
                    _BoardName = value;
                    OnPropertyChanged("BoardName");
                }
            }
        }
        public string Creator
        {
            get
            {
                return _Creator;
            }
            set
            {
                if (value != _Creator)
                {
                    _Creator = value;
                    OnPropertyChanged("Creator");
                }
            }
        }
        public string Language
        {
            get
            {
                return _Language;
            }
            set
            {
                if (value != _Language)
                {
                    _Language = value;
                    OnPropertyChanged("Language");
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand NewBoard
        {
            get
            {
                return new RelayCommand(_NewBoard);
            }
        }
        public RelayCommand SaveBoard
        {
            get
            {
                return new RelayCommand(_SaveBoard);
            }
        }
        public RelayCommand LoadSavedBoard
        {
            get
            {
                return new RelayCommand(_LoadSavedBoard);
            }
        }
        public RelayCommand SelectProperty
        {
            get
            {
                return new RelayCommand(_SelectProperty);
            }
        }
        public RelayCommand InvokeSelectionUpdates
        {
            get
            {
                return new RelayCommand(_UpdateStationUtilityRentCounts);
            }
        }
        #endregion

        #region Private attributes
        private GenericEditorItem _SelectedProperty;
        private string _BoardName;
        private string _Creator;
        private string _Language;
        #endregion

        #region Private subroutines
        private void _NewBoard(object sender)
        {
            Board = new ObservableCollection<GenericEditorItem>();
            for (int i = 0; i < 40; i++)
            {
                Board.Add(new GenericEditorItem() { Index = i });
                if (i % 10 == 0)
                {
                    Board[i].Name = "Protected";
                }
            }
            BoardName = "New board";
            Creator = "Anonymous";
            Language = "English";
            OnPropertyChanged("Board");
        }
        private void _SaveBoard(object sender)
        {
            // Fetch the gameboard to save
            Gameboard saveTarget = CovertGenericListToGameboard(Board);
            saveTarget.Name = BoardName;
            saveTarget.Creator = Creator;
            saveTarget.Language = Language;
            // Find where they'd like to save it
            SaveFileDialog saveBoard = new SaveFileDialog();
            saveBoard.Title = "Choose where to save your Monopoly board";
            saveBoard.DefaultExt = "monopoly";
            saveBoard.Filter = "Monopoly board files (*.mboard)|*.mboard";
            // Ensure a target directory has been created.
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Monopoly\\Boards");
            saveBoard.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Monopoly\\Boards";
            string serialisedGameboard = JsonConvert.SerializeObject(saveTarget);
            if (saveBoard.ShowDialog() == true)
            {
                if(saveBoard.FileName == null)
                {
                    return;
                }
                File.WriteAllText(saveBoard.FileName, serialisedGameboard);
            }
        }
        private async void _LoadSavedBoard(object sender)
        {
            // We are loading an old game.
            // Open the file dialog within a try-catch (to detect for improper closure).
            try
            {
                OpenFileDialog openBoard = new OpenFileDialog();
                openBoard.Filter = "Monopoly board files (*.mboard)|*.mboard";
                openBoard.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Monopoly\\Boards";
                openBoard.ShowDialog();
                if (openBoard.FileName == null)
                {
                    return;
                }
                else
                {
                    Gameboard newGameboard = MonopolyWindowViewModel.DeserialisePathIntoGameboard(openBoard.FileName);
                    BoardName = newGameboard.Name;
                    Creator = newGameboard.Creator;
                    Language = newGameboard.Language;
                    // Configure rents
                    StationRents = new ObservableCollection<int>();
                    foreach (int value in newGameboard.StationsRent)
                    {
                        StationRents.Add(value);
                    }
                    UtilityRents = new ObservableCollection<int>();
                    foreach (int value in newGameboard.UtilityMultipliers)
                    {
                        UtilityRents.Add(value);
                    }
                    // Load board
                    Board = ConvertGameboardToGenericList(newGameboard);
                    OnPropertyChanged("Board");
                    _UpdateStationUtilityRentCounts(null);
                }
            }
            catch (ArgumentException)
            {
                await Dialogs.ShowMessageAsync(this, "", "Please ensure you select Monopoly board file compatible with this version of Monopoly.", MessageDialogStyle.Affirmative);
            }
        }
        private void _SelectProperty(object sender)
        {
            SelectedProperty = (GenericEditorItem)sender;
        }
        private void _UpdateStationUtilityRentCounts(object sender)
        {
            Console.WriteLine("Refreshing station and utility rent counters");
            int stations = 0; int utilities = 0;
            foreach (GenericEditorItem item in Board)
            {
                if (item.Type == 1)
                {
                    // Station
                    Console.WriteLine("Found a station at index: " + item.Index);
                    stations++;
                }
                else if (item.Type == 2)
                {
                    // Utility
                    Console.WriteLine("Found a utility at index: " + item.Index);
                    utilities++;
                }
            }
            Console.WriteLine("Counted utilities " + utilities + ", and stations " + stations);
            // Change the numbers where needed
            if (stations > StationRents.Count())
            {
                // Need to add an additional integer to StationsRent
                for (int i = StationRents.Count(); i < stations; i++)
                {
                    Console.WriteLine("Adding a station to a list of " + StationRents.Count());
                    StationRents.Add(0);
                }
            }
            else if (stations < StationRents.Count())
            {
                // Need to remove the unnecessary integers from the end!
                ObservableCollection<int> newList = new ObservableCollection<int>();
                for (int i = 0; i < stations; i++)
                {
                    newList.Add(StationRents[i]);
                    Console.WriteLine("Adding stations, index " + i + ": " + StationRents[i]);
                }
                StationRents = newList;
            }
            // Repeat such a process for the utilities
            // Change the numbers where needed
            if (utilities > UtilityRents.Count())
            {
                // Need to add an additional integer to UtilityRent
                for (int i = UtilityRents.Count(); i < utilities; i++)
                {
                    UtilityRents.Add(0);
                }
            }
            else if (utilities < UtilityRents.Count())
            {
                // Need to remove the unnecessary integers from the end!
                ObservableCollection<int> newList = new ObservableCollection<int>();
                for (int i = 0; i < utilities; i++)
                {
                    newList.Add(UtilityRents[i]);
                }
                UtilityRents = newList;
            }
            Console.WriteLine("Two route done");
            // Force visual updates
            OnPropertyChanged("StationRents");
            OnPropertyChanged("UtilityRents");
            Console.WriteLine("Returning");
        }
        #endregion

        #region Conversion subroutines
        private ObservableCollection<GenericEditorItem> ConvertGameboardToGenericList(Gameboard gameboard)
        {
            // We can first order this passed-in gameboard to formatforview, which converts it into a list of easily explorable Locations.
            var locationList = gameboard.FormatForView();
            ObservableCollection<GenericEditorItem> outputList = new ObservableCollection<GenericEditorItem>();
            foreach (Location location in locationList)
            {
                // Go through individually and convert to a GenericEditorItem...
                if (location is Residence res)
                {
                    var editorItem = new GenericEditorItem
                    {
                        Type = 0,
                        Index = res.Position,
                        Name = res.Name,
                        Set = res.Set,
                        Price = res.Price,
                        HouseIncrementationCost = res.HouseIncrementationPrice,
                        PropertyHex = res.Hex,
                        Rent = new ObservableCollection<int>()
                    };
                    foreach (int rentItem in res.Rent)
                    {
                        editorItem.Rent.Add(rentItem);
                    }
                    outputList.Add(editorItem);
                    // Error checking and fixing where needed
                    if (SetDictionary.ContainsKey(res.Set))
                    {
                        if (SetDictionary[res.Set] != res.Hex)
                        {
                            res.Hex = SetDictionary[res.Set];
                        }
                    } else
                    {
                        SetDictionary.Add(res.Set, res.Hex);
                    }
                }
                else if (location is Station station)
                {
                    outputList.Add(new GenericEditorItem
                    {
                        Type = 1,
                        Index = station.Position,
                        Name = station.Name,
                        Set = station.Set,
                        Price = station.Price,
                        PropertyHex = station.Hex
                    });
                }
                else if (location is Utility utility)
                {
                    outputList.Add(new GenericEditorItem
                    {
                        Type = 2,
                        Index = utility.Position,
                        Name = utility.Name,
                        Set = utility.Set,
                        Price = utility.Price,
                        PropertyHex = utility.Hex
                    });
                }
                else if (location is Chance chance)
                {
                    outputList.Add(new GenericEditorItem
                    {
                        Type = 3,
                        Index = chance.Position
                    });
                }
                else if (location is CommunityChest cchest)
                {
                    outputList.Add(new GenericEditorItem
                    {
                        Type = 4,
                        Index = cchest.Position
                    });
                }
                else if (location is Tax taxation)
                {
                    outputList.Add(new GenericEditorItem
                    {
                        Type = 5,
                        Name = taxation.Name,
                        Index = taxation.Position,
                        Price = taxation.Cost
                    });
                }
            }
            for(int i = 0; i < 31; i += 10)
            {
                outputList.Add(new GenericEditorItem { Name = "Protected", Index = i });
            }
            return outputList;
        }
        private Gameboard CovertGenericListToGameboard(ObservableCollection<GenericEditorItem> list)
        {
            // Create defining lists for collation later
            List<Residence> residentialList = new List<Residence>();
            List<string> residentialSets = new List<string>();
            List<Station> stationsList = new List<Station>();
            List<Utility> utilitiesList = new List<Utility>();
            List<Tax> taxationIndexes = new List<Tax>();
            List<int> chanceIndexes = new List<int>();
            List<int> cChestIndexes = new List<int>();
            foreach(GenericEditorItem item in list)
            {
                switch(item.Type)
                {
                    case 0: // Residential
                        if(item.Name == "Protected") { break; }
                        residentialList.Add(new Residence(null)
                        {
                            Position = item.Index,
                            Name = item.Name,
                            Set = item.Set,
                            Price = item.Price,
                            HouseIncrementationPrice = item.HouseIncrementationCost,
                            Hex = item.PropertyHex,
                            Rent = item.Rent.ToArray()
                        });
                        if(!residentialSets.Contains(item.Set))
                        {
                            residentialSets.Add(item.Set);
                        }
                        break;
                    case 1: // Station
                        stationsList.Add(new Station(null)
                        {
                            Position = item.Index,
                            Name = item.Name,
                            Set = item.Set,
                            Price = item.Price,
                            Hex = item.PropertyHex,
                        });
                        break;
                    case 2: // Utility
                        utilitiesList.Add(new Utility(null)
                        {
                            Position = item.Index,
                            Name = item.Name,
                            Set = item.Set,
                            Price = item.Price,
                            Hex = item.PropertyHex,
                        });
                        break;
                    case 3: // Chance tile
                        chanceIndexes.Add(item.Index);
                        break;
                    case 4: // Community chest tile
                        cChestIndexes.Add(item.Index);
                        break;
                    case 5: // Chance tile
                        taxationIndexes.Add(new Tax(null)
                        {
                            Name = item.Name,
                            Position = item.Index,
                            Cost = item.Price
                        });
                        break;
                }
            }
            // Now we need to assemble these collections into an output gameboard
            Gameboard outputGameboard = new Gameboard(null);
            // Do the residences
            outputGameboard.Residences = new Dictionary<string, List<Residence>>();
            foreach(string set in residentialSets)
            {
                List<Residence> residencesOfSet = new List<Residence>();
                foreach(Residence res in residentialList)
                {
                    if(res.Set == set)
                    {
                        residencesOfSet.Add(res);
                    }
                }
                outputGameboard.Residences.Add(set, residencesOfSet);
            }
            // Residences done! Onto stations and utilities
            outputGameboard.Stations = stationsList;
            outputGameboard.StationsRent = StationRents.ToArray();
            outputGameboard.Utilities = utilitiesList;
            outputGameboard.UtilityMultipliers = UtilityRents.ToArray();
            // Properties done! Deal with taxation, chance and community chest
            outputGameboard.TaxIndexes = taxationIndexes;
            outputGameboard.ChanceIndexes = chanceIndexes.ToArray();
            outputGameboard.ChestIndexes = cChestIndexes.ToArray();
            return outputGameboard;
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
            var e = new PropertyChangedEventArgs(propertyName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        #endregion
    }
}
