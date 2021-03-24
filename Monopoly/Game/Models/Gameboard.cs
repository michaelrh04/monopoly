using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    /// <summary>
    /// The Gameboard class handles the loading in and storage of the gameboard. It is used to access information about speciifc properties/property sets,
    /// as well as containing the functions needed to turn the properties into a collection for display.
    /// </summary>
    [Serializable]
    public class Gameboard
    {
        [NonSerialized]
        private MonopolyWindowViewModel ViewModel;
        #region Constructor
        public Gameboard(MonopolyWindowViewModel viewmodel)
        {
            Console.WriteLine("[DEBUG] Constructing the Gameboard.");
            ViewModel = viewmodel;
        }
        #endregion

        #region Gameboard attributes
        public string Name { get; set; }
        public string Creator { get; set; }
        public string Language { get; set; }
        #endregion

        #region Gameboard Monopoly properties
        public Dictionary<string, List<Residence>> Residences;
        public List<Station> Stations;
        public List<Utility> Utilities;
        public int[] ChanceIndexes;
        public int[] ChestIndexes;
        public List<Tax> TaxIndexes;
        #endregion

        #region Board settings
        public int[] StationsRent;
        public int[] UtilityMultipliers;
        #endregion

        #region Subroutines
        /// <summary>
        /// FormatForView() converts and orders all properties on the board into a visual ObservableCollection. This should only have to be run once, when the board is first loaded in.
        /// </summary>
        /// <returns>An ObservableCollection of all properties, ordered.</returns>
        public ObservableCollection<Location> FormatForView()
        {
            ObservableCollection<Location> ReturnProduct = new ObservableCollection<Location>();
            // First, add 40 slots to the ObservableCollection: one for every board space.
            for(int i = 0; i < 40; i++)
            {
                ReturnProduct.Add(null);
            }
            // Sort through all 36 dynamic spaces (i.e. spaces set in the JSON file) and add them to the board
            foreach(KeyValuePair<string, List<Residence>> ResidenceSet in Residences)
            {
                foreach(Residence residence in ResidenceSet.Value)
                {
                    residence.Set = ResidenceSet.Key;
                    ReturnProduct[residence.Position] = residence;
                }
            }
            foreach(Station station in Stations)
            {
                station.Set = "Stations";
                ReturnProduct[station.Position] = station;
            }
            foreach(Utility utility in Utilities)
            {
                utility.Set = "Utilities";
                ReturnProduct[utility.Position] = utility;
            }
            // Properties complete, move onto more specific squares: chest indexes and taxation
            foreach(int chanceIndex in ChanceIndexes)
            {
                ReturnProduct[chanceIndex] = new Chance(chanceIndex, ViewModel);
            }
            foreach (int chestIndex in ChestIndexes)
            {
                ReturnProduct[chestIndex] = new CommunityChest(chestIndex, ViewModel);
            }
            foreach (Tax taxTile in TaxIndexes)
            {
                ReturnProduct[taxTile.Position] = taxTile;
            }
            // Finally, add the special corner squares
            ReturnProduct[0] = new Go(0, ViewModel);
            ReturnProduct[10] = new Jail(10, ViewModel);
            ReturnProduct[20] = new FreeParking(20, ViewModel);
            ReturnProduct[30] = new GoToJail(30, ViewModel);
            // Collection assembled, return!
            return ReturnProduct;
        }
        #endregion
    }
}
