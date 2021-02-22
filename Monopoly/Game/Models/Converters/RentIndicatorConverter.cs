using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using Monopoly.Game;

namespace Monopoly.Converters
{
    /// <summary>
    /// Converter to allow for control of the rent tier indicator: it should show the number of houses owned in a player's set, or stations/utilities owned by a player.
    /// </summary>
    class RentIndicator : IValueConverter
    {
        /// <summary>
        /// Converter will allow for the configuration of the housing indicator in the view, and should be used to control that accordingly.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>A group of elements (in a list), displayed in the dockpanel on the View.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The view will trigger this conversion immediately without a selected property, so ensure that this doesn't crash anything!
            if(value == null) { return null; }
            // If the property is unowned, how can anyone own a set that includes it? Return nothing if that is the result.
            if(((Property)value).Owner == null) { return null; }
            // Begin the creation of the additions to the collection.
            List<Dummy> Elements = new List<Dummy>();
            #region Full/nonfull set configuration
            // The display should also indicate whether or not all properties are owned. This will be done first.
            // However, owing to the different types of property there are, collecting the number of properties in any one set requires different methods.
            int NumberInSet = 0;
            if (value is Residence r) { NumberInSet = MonopolyWindowViewModel.Handler.BoardConfiguration.Residences[r.Set].Count(); }
            else if (value is Station) { NumberInSet = MonopolyWindowViewModel.Handler.BoardConfiguration.Stations.Count(); }
            else if (value is Utility) { NumberInSet = MonopolyWindowViewModel.Handler.BoardConfiguration.Utilities.Count(); }
            // Compare the number possible against the number the owner of this property owns.
            if (NumberInSet == ((Property)value).Owner.Inventory[((Property)value).Set].Count())
            {
                // The set is complete!
                Elements.Add(new Dummy { Type = DummyType.SetOwned });
            }
            else
            {
                // The set is incomplete.
                Elements.Add(new Dummy { Type = DummyType.SetUnowned });
            }
            #endregion
            #region Residence configuration
            if (value is Residence residence)
            {
                // This is a residence. The display needs to render houses, coloured appropriately for the number on the property.
                // Only the property (and not the owner) needs to be examined for this section.
                for(int i = 0; i < 4; i++)
                {
                    // Add four greyed out houses to the collection.
                    Elements.Add(new Dummy { Type = DummyType.HouseUnowned });
                }
                // Now remove an appropriate number of houses, and add new houses where needed.
                for(int i = 0; i < residence.Houses; i++)
                {
                    // As there is no fifth house, break the loop if necessary.
                    if(i + 1 == 5) { break; }
                    // Remove house.
                    Elements.RemoveAt(i + 1);
                    // Insert a new house at the beginning of the collection.
                    Elements.Insert(i + 1, new Dummy { Type = DummyType.HouseOwned });
                }
                // Now add a hotel, and make it dark gray or red depending on which is appropriate.
                Elements.Add(new Dummy { Type = (residence.Houses == 5) ? DummyType.HotelOwned : DummyType.HotelUnowned });
                // The creation is complete!
                return Elements;
            }
            #endregion
            #region Station configuration
            else if (value is Station station)
            {
                // This is a station. The display needs to render stations, coloured appropriately for how many the person owns.
                // First we must find out how many stations exist on the board. We can use the ViewModel's static gameboard property to do this.
                // This ensures that custom boards can be accounted for correctly.
                int StationsExisting = MonopolyWindowViewModel.Handler.BoardConfiguration.Stations.Count();
                // Now we can continue, adding the correct amount of unowned stations first.
                for (int i = 0; i < StationsExisting; i++)
                {
                    // Add greyed out stations to the collection.
                    Elements.Add(new Dummy { Type = DummyType.StationUnowned });
                }
                // And then looping through again to remove an appropriate number of stations and add their coloured variants.
                for (int i = 0; i < station.Owner.Inventory["Stations"].Count(); i++)
                {
                    // Remove unowned station.
                    Elements.RemoveAt(i + 1);
                    // Insert a new station at the beginning of the collection.
                    Elements.Insert(i + 1, new Dummy { Type = DummyType.StationOwned });
                }
                // The creation is complete!
                return Elements;
            }
            #endregion
            #region Utilities configuration
            else if (value is Utility utility)
            {
                // This is a utility. The display needs to render utility icons, coloured appropriately for how many the person owns.
                // First we must find out how many utilities exist on the board. We can use the ViewModel's static gameboard property to do this.
                // This ensures that custom boards can be accounted for correctly.
                int UtilitiesExisting = MonopolyWindowViewModel.Handler.BoardConfiguration.Utilities.Count();
                // Now we can continue, adding the correct amount of unowned utilities first.
                for (int i = 0; i < UtilitiesExisting; i++)
                {
                    // Add greyed out utilities to the collection.
                    Elements.Add(new Dummy { Type = DummyType.UtilityUnowned });
                }
                // And then looping through again to remove an appropriate number of utilities and add their coloured variants.
                for (int i = 0; i < utility.Owner.Inventory["Utilities"].Count(); i++)
                {
                    // Remove unowned utility.
                    Elements.RemoveAt(i + 1);
                    // Insert a new utility at the beginning of the collection.
                    Elements.Insert(i + 1, new Dummy { Type = DummyType.UtilityOwned });
                }
                // The creation is complete!
                return Elements;
            }
            #endregion
            throw new FormatException("Hang on! The property passed in didn't appear to be a residence, utility or station!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
