namespace HackSharp
{
    public class Complex
    {
        /* The current level number. */
        public Level[] Levels { get; private set; }

        /* The player data. */
        public Player ThePlayer;


        public int DungeonLevel { get; private set; }

        /* Last player Coordinates. */
        public Position OldPlayerPos { get; set; }

        /* The panel position. */
        public Position PanelPos { get; set; }

        //Player
        public Position PlayerPos { get; set; }


        //Stairs

        public Level CurrentLevel
        {
            get { return Levels[DungeonLevel]; }
        }

        public void Play()
        {
            DungeonLevel = 0;
            CurrentLevel.Visited = true;

            /* Initial player position. */
            PlayerPos = OldPlayerPos = CurrentLevel.StairsUp;

            /* Initial panel position. */
            PanelPos = new Position(0, 0);
        }

        /// <summary>
        ///     Continue one level downwards.
        /// </summary>
        public void DescendLevel()
        {
            DungeonLevel++;
            PlayerPos = CurrentLevel.StairsUp;
        }

        /// <summary>
        ///     Continue one level upwards.
        /// </summary>
        public void AscendLevel()
        {
            if (DungeonLevel > 0)
            {
                DungeonLevel--;
                PlayerPos = CurrentLevel.StairsDown;
            }
            else
            {
                /* Leave the dungeon. */
                DungeonLevel = -1;
            }
        }

        /// <summary>
        ///     Create all the levels in the dungeon.
        /// </summary>
        public void Dig()
        {
            Levels = new Level[Config.MaxDungeonLevel];
            for (int i = 0; i<Config.MaxDungeonLevel; i++) Levels[i] = new Level();
            Levels[Config.MaxDungeonLevel - 1].IsBottom = true;
            Levels[0].IsTop = true;
            for (int i = 0; i < Config.MaxDungeonLevel; i++)
            {
                /* Basic initialization. */
                /* Create the current level map. */
                Levels[i].Dig();

                /* Note the current level as unvisited. */
                Levels[i].Visited = false;
            }
        }


 


        /// <summary>
        /// Calculate the current section coordinates of the player*if* the current section contains a room and the given position is in that room.
        /// </summary>
        internal Position GetPlayerSection()
        {
            return CurrentLevel.GetSection(PlayerPos);
        }
    }
}

// original notice

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
