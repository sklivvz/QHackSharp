namespace HackSharp
{
    class monster_struct
    {
        /* Index to the first empty monster slow; later ones use 'midx'. */
        internal byte[] eidx = new byte[Config.MAX_DUNGEON_LEVEL];

        /* The monster slots for each level. */
        internal monster[,] m = new monster[Config.MAX_DUNGEON_LEVEL, Config.MONSTERS_PER_LEVEL];
    };
}