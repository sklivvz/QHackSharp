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
        public int OldPlayerX;
        public int OldPlayerY;


        /* The panel positions. */
        public byte psx;
        public byte psy;

        //Player
        public int PlayerX;
        public int PlayerY;

        //Stairs
        public int[] StairsDownX = new int[Config.MaxDungeonLevel - 1];
        public int[] StairsUpX = new int[Config.MaxDungeonLevel];
        public int[] StairsDownY = new int[Config.MaxDungeonLevel - 1];
        public int[] StairsUpY = new int[Config.MaxDungeonLevel];

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

        /// <summary>
        /// Determine whether a given position is already known.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsKnown(int x, int y)
        {
            if (x < 0 || x >= Config.MapW)
                throw new ArgumentOutOfRangeException("x", x, string.Format("x must be more than zero and less than {0}", Config.MapW));
            if (y < 0 || y >= Config.MapH)
                throw new ArgumentOutOfRangeException("y", y, string.Format("y must be more than zero and less than {0}", Config.MapH));
            return Known[DungeonLevel, x, y];
        }

        /// <summary>
        /// Set or reset a knowledge bit in the knowledge map.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="known"></param>
        internal void SetKnowledge(int x, int y, bool known)
        {
            if (x < 0 || x >= Config.MapW)
                throw new ArgumentOutOfRangeException("x", x, string.Format("x must be more than zero and less than {0}", Config.MapW));
            if (y < 0 || y >= Config.MapH)
                throw new ArgumentOutOfRangeException("y", y, string.Format("y must be more than zero and less than {0}", Config.MapH));
            Known[DungeonLevel, x, y] = known;
        }
    }
}