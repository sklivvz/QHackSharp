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
 * QHack uses one large structure for the complete dungeon.  There are
 * no pointers or other fancy stuff involved since this game should be
 * simple and easy to use.
 *
 * Naturally this prevents some useful things and is not the way a big
 * roguelike game should be written (you sacrifice too much in flexibility),
 * but since it's easy to use I headed into this direction.
 */

    public class DungeonComplex
    {
        public static DungeonComplex D = new DungeonComplex();

        /* The current level number. */
        public byte Dl;

        /* Last player Coordinates. */
        public Coord Opx;
        public Coord Opy;


        /* The panel positions. */
        public Coord Psx;
        public Coord Psy;
        public Coord Px;
        public Coord Py;
        public Coord[] Stxd; // [MAX_DUNGEON_LEVEL - 1];
        public Coord[] Stxu; // [MAX_DUNGEON_LEVEL];
        public Coord[] Styd; // [MAX_DUNGEON_LEVEL - 1];
        public Coord[] Styu; // [MAX_DUNGEON_LEVEL];

        /* Level was already visited? */
        public bool Visited; // [MAX_DUNGEON_LEVEL];

        /* The knowledge map. */
        private byte[,,] _known;

        /* The player data. */
        private Player _pc;

        /* NSECT_W * NSECT_H Sections for each level. */
        private Section[,,] _s;
    }
}