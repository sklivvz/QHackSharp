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
        public bool exists;

        /* Room Coordinates. */
        public byte rx1;
        public byte rx2;
        public byte ry1;
        public byte ry2;

        /* Door positions. */
        public byte[] dx = new byte[4]; //4
        public byte[] dy = new byte[4];

        /* Door types */
        public byte[] dt = new byte[4]; //4

    }
}