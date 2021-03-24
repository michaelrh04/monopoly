using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly.Game
{
    public enum EnumCardType
    {
        /// <summary>
        /// This card advances to a new location.
        /// </summary>
        AdvanceTo,
        /// <summary>
        /// This card reverts to a new location.
        /// </summary>
        GoBackTo,
        /// <summary>
        /// This card imprisons player.
        /// </summary>
        Jail,
        /// <summary>
        /// This card orders the player to pay a certain amount of money.
        /// </summary>
        Pay,
        /// <summary>
        /// This card orders hte player to pay a certain amount of money OR draw a chance card.
        /// </summary>
        PayOrPenalty,
        /// <summary>
        /// This card allows the player to recieve money.
        /// </summary>
        Recieve
    }
}
