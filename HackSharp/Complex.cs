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
        public int px;
        public int py;
        public byte[] stxd = new byte[Config.MAX_DUNGEON_LEVEL - 1];
        public byte[] stxu = new byte[Config.MAX_DUNGEON_LEVEL];
        public byte[] styd = new byte[Config.MAX_DUNGEON_LEVEL - 1];
        public byte[] styu = new byte[Config.MAX_DUNGEON_LEVEL];

        /* Level was already visited? */
        public bool[] Visited = new bool[Config.MAX_DUNGEON_LEVEL];

        /* The knowledge map. */
        public byte[, ,] Known = new byte[Config.MAX_DUNGEON_LEVEL, Config.MAP_BIT_W, Config.MAP_H];

        /* The player data. */
        public Player ThePlayer;

        /* NSECT_W * NSECT_H Sections for each level. */
        public Section[, ,] s = new Section[Config.MAX_DUNGEON_LEVEL, Config.NSECT_W, Config.NSECT_H];

        public Complex()
        {
            for (int i = 0; i < Config.MAX_DUNGEON_LEVEL; i++)
                for (int j = 0; j < Config.NSECT_W; j++)
                    for (int k = 0; k < Config.NSECT_H; k++)
                        s[i, j, k] = new Section();
        }
    }
}