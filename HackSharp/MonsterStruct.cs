namespace HackSharp
{
    class MonsterStruct
    {
        /* Index to the first empty monster slow; later ones use 'midx'. */
        internal byte[] Eidx = new byte[Config.MaxDungeonLevel];

        /* The monster slots for each level. */
        internal Monster[,] MonsterSlots = new Monster[Config.MaxDungeonLevel, Config.MonstersPerLevel];
    };
}