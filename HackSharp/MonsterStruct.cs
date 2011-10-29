namespace HackSharp
{
    class MonsterStruct
    {
        /* Index to the first empty monster slow; later ones use 'midx'. */
        internal byte[] Eidx = new byte[Config.MAX_DUNGEON_LEVEL];

        /* The monster slots for each level. */
        internal Monster[,] MonsterSlots = new Monster[Config.MAX_DUNGEON_LEVEL, Config.MONSTERS_PER_LEVEL];
    };
}