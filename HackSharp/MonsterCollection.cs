using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HackSharp
{
    class MonsterCollection
    {
        /* Index to the first empty monster slow; later ones use 'midx'. */
        private readonly byte[] eidx = new byte[Config.MaxDungeonLevel];

        /* The monster slots for each level. */
        private readonly Monster[,] monsterSlots = new Monster[Config.MaxDungeonLevel, Config.MonstersPerLevel];

        public Monster GetMonster(int dungeonLevel, int pMidx)
        {
            return monsterSlots[dungeonLevel, pMidx];
        }

        public void Add(int dungeonLevel, Monster monster)
        {
            /* Find a monster index. */
            int index = eidx[dungeonLevel];

            /* Paranoia. */
            if (index == -1)
                Error.Die("Could not create the initial monster population");

            eidx[dungeonLevel] = (byte)monsterSlots[dungeonLevel, index].Midx;

            monsterSlots[dungeonLevel, index] = monster;
        }

        public MonsterCollection()
        {
            for (int i = 0; i < Config.MaxDungeonLevel; i++)
            {
                /* The first empty monster slot. */
                eidx[i] = 0;

                /* Initially all slots are empty. */
                for (int j = 0; j < Config.MonstersPerLevel - 1; j++)
                    monsterSlots[i, j] = new Monster { Used = false, Midx = j + 1 };

                /* The last one points to 'no more slots'. */
                monsterSlots[i, Config.MonstersPerLevel - 1] = new Monster { Midx = -1, Used = false };
            }
        }

        public Monster[] MonstersOnLevel(int dungeonLevel)
        {
            return MonstersOnLevelImpl(dungeonLevel).Where(m=>m.Used).ToArray();
        }

        private IEnumerable<Monster> MonstersOnLevelImpl(int dungeonLevel)
        {
            for (int i = 0; i < monsterSlots.GetLength(1);i++)
            {
                yield return monsterSlots[dungeonLevel, i];
            }
        }
    };
}