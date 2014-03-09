using System;
using System.Runtime.InteropServices;

namespace HackSharp
{
    internal class Monsters
    {
        /* The total number of monsters. */
        private const int MaxMonsters = 4;
        public MonsterCollection All;

        /* The complete monster list for the game. */

        /* The dynamic index map for one monster level. */
        private readonly byte[,] _midx = new byte[Config.MapW, Config.MapH];


        /* The total rarity for monsters; dependent on the current level. */
        private Dungeon _dungeon;
        private int _totalRarity;
        private int dungeonLevel;

        /// <summary>
        /// Create an initial monster population for a given level.
        /// </summary>
        internal void CreatePopulation()
        {
            byte m;

            /* Initialize the basic monster data. */
            InitializeMonsters();

            for (m = 0; m < Config.InitialMonsterNumber; m++)
            {
                /* Create a new monster. */
                var type = RandomMonsterType();
                var monster = new Monster(type, GetMonsterCoordinates());
                All.Add(_dungeon.TheComplex.DungeonLevel, monster);
            }
        }


        private int CalcMonsterRarity(int pMidx)
        {
            return MonsterDefinition.Manual[pMidx].EffectiveRarity(_dungeon.TheComplex.DungeonLevel, pMidx);
        }

        /// <summary>
        /// Calculate the frequencies for all available monsters based upon the current dungeon level.
        /// </summary>
        internal void InitializeMonsters()
        {
            _totalRarity = 0;
            var monsterTypesOnLevel = Math.Min(MaxMonsters, _dungeon.TheComplex.DungeonLevel * 2 + 4);
            for (int i = 0; i < monsterTypesOnLevel; i++)
                _totalRarity += CalcMonsterRarity(i);
        }

        /// <summary>
        /// Initialize the monster structures.  Basically we have to notify all slots that they are empty.  Also the general index map needs to be initialized.
        /// </summary>
        internal void InitMonsters(Dungeon dungeon)
        {
            _dungeon = dungeon;
            dungeonLevel = _dungeon.TheComplex.DungeonLevel;

            int i;
            int j;

            All = new MonsterCollection();

            /* Initialize the monster index map as 'empty'. */
            for (i = 0; i < Config.MapW; i++)
                for (j = 0; j < Config.MapH; j++)
                    _midx[i, j] = 255;
        }


        /// <summary>
        /// Create the monster map for a given dungeon level.
        /// </summary>
        internal void BuildMonsterMap()
        {
            int x, y;

            /* Initialize the monster index map as 'empty'. */
            for (x = 0; x < Config.MapW; x++)
                for (y = 0; y < Config.MapH; y++)
                    _midx[x, y] = 255;

            var monsters = All.MonstersOnLevel(dungeonLevel);
            for (x = 0; x < monsters.Length; x++)
            {
                Monster m = monsters[x];
                _midx[m.Position.X, m.Position.Y] = (byte) x;
            }
        }


        /// <summary>
        /// Determine the index number for a random monster on the current dungeon level.
        /// </summary>
        /// <returns></returns>
        private int RandomMonsterType()
        {
            long roll = Terminal.RandLong(_totalRarity) + 1;
            int i = 0;

            while (roll > CalcMonsterRarity(i))
            {
                roll -= CalcMonsterRarity(i);
                i++;
            }

            return i;
        }

        /// <summary>
        /// Find coordinates for a new monster.
        /// </summary>
        /// <returns>Position.</returns>
        /// <remarks>Some things need to be considered:
        /// 1. Monsters should only be created on 'floor' tiles.
        /// 2. New monsters should not be in LOS of the PC.
        /// 3. New monsters should not be created in spots where another monster is standing.</remarks>
        public Position GetMonsterCoordinates()
        {
            Position monsterCoordinates;
            do
            {
                monsterCoordinates = new Position(Terminal.RandInt(Config.MapW), Terminal.RandInt(Config.MapH));
            } while (
                Dungeon.Map[monsterCoordinates.X, monsterCoordinates.Y] != Tiles.Floor ||
                LineOfSight(monsterCoordinates) ||
                _midx[monsterCoordinates.X, monsterCoordinates.Y] != 255);
            return monsterCoordinates;
        }

        /// <summary>
        /// Check whether a PC is able to see a position.
        /// </summary>
        internal bool LineOfSight(Position p)
        {
            /* Adjacent to the PC? */
            if (Math.Abs(p.X - _dungeon.TheComplex.PlayerPos.X) <= 1 && Math.Abs(p.Y - _dungeon.TheComplex.PlayerPos.Y) <= 1)
                return true;

            /* In the same room section? */
            return _dungeon.TheComplex.CurrentLevel.GetSection(p).Equals(_dungeon.TheComplex.GetPlayerSection());
        }

        /// <summary>
        /// Get a monster at a specific position.
        /// </summary>
        internal Monster GetMonsterAt(Position p)
        {
            return All.GetMonster(dungeonLevel, _midx[p.X, p.Y]);
        }


        /// <summary>
        /// Determine whether a monster holds a given position.
        /// </summary>
        /// <returns></returns>
        internal bool IsMonsterAt(Position p)
        {
            return (_midx[p.X, p.Y] != 255);
        }

        /// <summary>
        /// Handle the monster turn: movement, combat, etc.
        /// </summary>
        internal void MoveMonsters()
        {
            //TODO: Implement this
        }
    }
}