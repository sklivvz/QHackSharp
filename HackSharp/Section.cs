/*                               -*- Mode: C -*- 
 * qhack.h -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Mon Dec 30 00:25:24 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 20:50:10 1997
 * Update Count    : 30
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

using System.Runtime.Remoting.Messaging;

namespace HackSharp
{
    /*
     * One Section of a level: each dungeon map consists of NSECT_W * NSECT_H
     * Sections (see config.g).  A Section either contains one room with up
     * to four doors or a tunnel interSection.
     *
     * NOTE: The use of a Sectioning approach prevents things like digging, etc.
     */

    public class Section
    {
        /* Room available? */
        public bool Exists;

        /* Room Coordinates. */
        public Position TopLeft { get; set; }
        public Position BottomRight { get; set; }

        public Position[] Doors = new Position[4];

        /* Door positions. */
        public int[] dx {
            get { return new[] {Doors[0].X, Doors[1].X, Doors[2].X, Doors[3].X}; }
        }
        public int[] dy
        {
            get { return new[] { Doors[0].Y, Doors[1].Y, Doors[2].Y, Doors[3].Y }; }
        }

        /* Door types */
        public int[] dt = new int[4]; //4

    }
}