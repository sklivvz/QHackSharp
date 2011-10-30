/*                               -*- Mode: C -*- 
 * config.h -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Sun Dec 29 21:57:08 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 21:18:57 1997
 * Update Count    : 38
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
    internal static class Config
    {
        /*
         * Section I: General constants for meta information.
         *
         * A note on version information: the major version number is only increased
         * if a specific milestone in the game developement is reached.  The minor
         * version is increased very time I (Thomas Biskup) release a new QHack source
         * distribution.
         */

        public const int MajorVersion = 0;
        public const int MinorVersion = 1;


        /*
         * Section II: Constants governing the dungeon size.
         *
         * Many things are limited in scope because QHack uses 'byte' variables
         * in many places.  Changing this would be some work.  This limit was
         * arbitrarly introduced to keep the game simple.
         *
         * Note that there can't be more than 127 sections in either direction
         * right now -- this should nonetheless be sufficient.
         */

        /* Screen width. */
        public const int ScreenW = 80;

        /* Screen height. */
        public const int ScreenH = 25;

        /* Section width. */
        public const int SectW = 15;

        /* Section height. */
        public const int SectH = 12;

        /* Number of sections in E/W direction. */
        public const int NsectW = 7;

        /* Number of sections in N/S direction. */
        public const int NsectH = 4;

        /* Total number of sections per level. */
        public const int SectNumber = (NsectH*NsectW);

        /* Map width. */
        public const int MapW = (SectW*NsectW);

        /* Map height (must be divisable by NSECT_H). */
        public const int MapH = (SectH*NsectH);

        /* Visible map width. */
        public const int VmapW = 80;

        /* Visible map height. */
        public const int VmapH = 22;

        /* Maximum width for a room. */
        public const int RoomW = (SectW - 2);

        /* Maximum height for a room. */
        public const int RoomH = (SectH - 2);

        /* Bitmap width for the knowledge map. */
        public const int MapBitW = ((MapW >> 3) + 1);

        /* Maximum number of monsters per level. */
        public const int MonstersPerLevel = 64;

        /* The initial number of monsters on a new level. */
        public const int InitialMonsterNumber = 24;

        /*
         * Dungeon depth.
         *
         * Note: changing this variable is not a good idea since many parts of this
         * code depend on a dungeon with 26 levels.
         */

        public const int MaxDungeonLevel = 26;


        /*
         * Section III: Some constants for the player character.
         */

        /* The length of the name of your character. */
        public const int MaxPcNameLength = 20;

        /* The number of training units you can distribute. */
        public const int Tunits = 100;
    }
}