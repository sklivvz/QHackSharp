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

using System;
using System.Linq;

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

    public class Complex
    {
        /* The current level number. */
        public int DungeonLevel;

        /* Last player Coordinates. */
        public Position OldPlayerPos { get; set; }

        /* The panel position. */
        public Position PanelPos { get; set; }

        //Player
        public Position PlayerPos { get; set; }


        //Stairs
        public Position[] StairsDown = new Position[Config.MaxDungeonLevel - 1];
        public Position[] StairsUp = new Position[Config.MaxDungeonLevel];

        /* Level was already visited? */
        public bool[] Visited = new bool[Config.MaxDungeonLevel];

        /* The knowledge map. */
        public bool[, ,] Known = new bool[Config.MaxDungeonLevel, Config.MapW, Config.MapH];

        /* The player data. */
        public Player ThePlayer;

        /* NSECT_W * NSECT_H Sections for each level. */
        public Section[, ,] s = new Section[Config.MaxDungeonLevel, Config.NsectW, Config.NsectH];

        public Complex()
        {
            for (int i = 0; i < Config.MaxDungeonLevel; i++)
                for (int j = 0; j < Config.NsectW; j++)
                    for (int k = 0; k < Config.NsectH; k++)
                        s[i, j, k] = new Section();

            /* Nothing is known about the dungeon at this point. */
            for (int i = 0; i < Config.MaxDungeonLevel; i++)
                for (int j = 0; j < Config.MapW; j++)
                    for (int k = 0; k < Config.MapH; k++)
                        Known[i, j, k] = false;
        }
    }
}