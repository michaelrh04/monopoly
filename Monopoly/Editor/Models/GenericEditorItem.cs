using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Editor
{
    /// <summary>
    /// This class contains all the attributes needed to control the editor exactly.
    /// </summary>
    class GenericEditorItem : INotifyPropertyChanged
    {
        #region Constructor
        public GenericEditorItem()
        {
            Index = -1;
        }
        #endregion
        
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    OnPropertyChanged("Index");
                }
            }
        }
        public int Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    OnPropertyChanged("Type");
                }
            }
        }
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public string Set
        {
            get
            {
                return _Set;
            }
            set
            {
                if (value != _Set)
                {
                    _Set = value;
                    OnPropertyChanged("Set");
                }
            }
        }
        public int Price
        {
            get
            {
                return _Price;
            }
            set
            {
                if (value != _Price)
                {
                    _Price = value;
                    OnPropertyChanged("Price");
                }
            }
        }
        public int HouseIncrementationCost
        {
            get
            {
                return _HouseIncrementationCost;
            }
            set
            {
                if (value != _HouseIncrementationCost)
                {
                    _HouseIncrementationCost = value;
                    OnPropertyChanged("HouseIncrementationCost");
                }
            }
        }
        public string PropertyHex
        {
            get
            {
                return _PropertyHex;
            }
            set
            {
                if(value != _PropertyHex)
                {
                    _PropertyHex = value;
                    OnPropertyChanged("PropertyHex");
                }
            }
        }
        public ObservableCollection<int> Rent { get; set; }

        private int _Index;
        private int _Type;
        private string _Name;
        private string _Set;
        private int _Price;
        private int _HouseIncrementationCost;
        private string _PropertyHex;

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
