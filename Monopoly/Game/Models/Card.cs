using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    [Serializable]
    public class Card
    {
        /// <summary>
        /// Determines the type of card present
        /// </summary>
        public EnumCardType Type { get; set; }
        /// <summary>
        /// Determines whether or not this card targets players or the bank
        /// </summary>
        public bool TargetsPlayers { get; set; }
        /// <summary>
        /// Stores associated values required for the operation of this card
        /// </summary>
        public int[] Amounts { get; set; }
        /// <summary>
        /// Stores associated text (message on the card).
        /// </summary>
        public string Text { get; set; }
    }
}
