/*                               -*- Mode: C -*- 
 * monster.h -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Mon Dec 30 18:08:50 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 20:12:30 1997
 * Update Count    : 24
 * Status          : Unknown, Use with caution!
 *
 * (C) Copyright 1996, 1997 by Thomas Biskup.
 * All Rights Reserved.
 *
 * This software may be distributed only for educational, research and
 * other non-proft purposes provided that this copyright notice is left
 * intact.  If you derive a new game from these sources you also are
 * required to give credit to Thomas Biskup for creating them in the first
 * place.  These sources must not be distributed for any fees in excess of
 * $3 (as of January, 1997).
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HackSharp
{
    /// <summary>
    /// A structure for a given monster.
    /// </summary>
    struct Monster
    {
        /* Is the entry occupied by a monster? */
        internal bool Used { get; set; }

        /* Monster type. */
        internal int Midx { get; set; }

        /* Position on the map. */
        internal int X { get; set; }
        internal int Y { get; set; }

        /* Hitpoint data. */
        internal int Hp { get; set; }
        internal int MaxHp { get; set; }

        /* The current state (see above). */
        internal MonsterState State { get; set; }
    };
}
