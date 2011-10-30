using System;

namespace HackSharp
{
    internal class Monsters
    {
        /* The total number of monsters. */
        private const int MaxMonsters = 4;

        private static readonly int[] Lmod =
            {
                100, 90, 80, 72, 64, 56, 50, 42, 35, 28, 20, 12, 4, 1
            };

        private readonly MonsterStruct _m = new MonsterStruct();


        /* The complete monster list for the game. */

        /* The dynamic index map for one monster level. */
        private readonly byte[,] _midx = new byte[Config.MapW,Config.MapH];

        private readonly MonsterDefinition[] _monsterManual = new[]
                                                                  {
                                                                      new MonsterDefinition
                                                                          {
                                                                              Symbol = 'k',
                                                                              Color = ConsoleColor.Green,
                                                                              Name = "kobold",
                                                                              ArmorClass = 14,
                                                                              Hits = "1d4",
                                                                              Attacks = 1,
                                                                              ToHit = 0,
                                                                              Damage = "1d6",
                                                                              Rarity = MonsterRarity.Common
                                                                          },
                                                                      new MonsterDefinition
                                                                          {
                                                                              Symbol = 'r',
                                                                              Color = ConsoleColor.DarkYellow,
                                                                              Name = "rat",
                                                                              ArmorClass = 12,
                                                                              Hits = "1d3",
                                                                              Attacks = 1,
                                                                              ToHit = 0,
                                                                              Damage = "1d3",
                                                                              Rarity = MonsterRarity.Common
                                                                          },
                                                                      new MonsterDefinition
                                                                          {
                                                                              Symbol = 'g',
                                                                              Color = ConsoleColor.Cyan,
                                                                              Name = "goblin",
                                                                              ArmorClass = 13,
                                                                              Hits = "1d8",
                                                                              Attacks = 1,
                                                                              ToHit = 0,
                                                                              Damage = "1d6",
                                                                              Rarity = MonsterRarity.Common
                                                                          },
                                                                      new MonsterDefinition
                                                                          {
                                                                              Symbol = 'x',
                                                                              Color = ConsoleColor.Yellow,
                                                                              Name = "lightning bug",
                                                                              ArmorClass = 18,
                                                                              Hits = "2d3",
                                                                              Attacks = 1,
                                                                              ToHit = 1,
                                                                              Damage = "1d4",
                                                                              Rarity = MonsterRarity.Rare
                                                                          }
                                                                  };

        /* The total rarity for monsters; dependent on the current level. */
        private Complex _d;
        private Dungeon _dungeon;
        private int _totalRarity;

        /// <summary>
        /// Initialize the monster structures.  Basically we have to notify all slots that they are empty.  Also the general index map needs to be initialized.
        /// </summary>
        internal void InitMonsters(Dungeon dungeon)
        {
            _dungeon = dungeon;
            _d = _dungeon.Complex;

            int i;
            int j;

            for (i = 0; i < Config.MaxDungeonLevel; i++)
            {
                /* The first empty monster slot. */
                _m.Eidx[i] = 0;

                /* Initially all slots are empty. */
                for (j = 0; j < Config.MonstersPerLevel - 1; j++)
                {
                    _m.MonsterSlots[i, j].Used = false;
                    _m.MonsterSlots[i, j].Midx = j + 1;
                }

                /* The last one points to 'no more slots'. */
                _m.MonsterSlots[i, Config.MonstersPerLevel - 1].Midx = -1;
                _m.MonsterSlots[i, Config.MonstersPerLevel - 1].Used = false;
            }

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

            /* Setup all monster indices. */
            for (x = 0; x < Config.MonstersPerLevel; x++)
                if (_m.MonsterSlots[_d.DungeonLevel, x].Used)
                    _midx[_m.MonsterSlots[_d.DungeonLevel, x].X, _m.MonsterSlots[_d.DungeonLevel, x].Y] = (byte) x;
        }

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
                /* Find a monster index. */
                int index = GetMonsterIndex();

                /* Paranoia. */
                if (index == -1)
                    Error.die("Could not create the initial monster population");

                /* Create a new monster. */
                CreateMonsterIn(index);
            }
        }


        /// <summary>
        /// Return the maximum monster number for the current dungeon level.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Since the current monster list is somewhat limited only four monsters are available.</remarks>
        private int MaxMonster()
        {
            return Misc.imin(MaxMonsters, ((_d.DungeonLevel << 1) + 4));
        }


        /// <summary>
        /// Determine the frequency for a given monster.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        /// <remarks>This value is level-dependent.  If the monster is out-of-depth (for QHack this means 'has a lower minimum level than the current dungeon level) it's frequency will be reduced.</remarks>
        private int CalcMonsterRarity(int pMidx)
        {
            var rarity = (int) _monsterManual[pMidx].Rarity;
            int levelDiff = _d.DungeonLevel - monster_level(pMidx);

            return Misc.imax(1, (rarity*Lmod[Misc.imin(13, levelDiff)])/100);
        }


        /// <summary>
        /// Determine the minimum level for a given monster number.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        private int monster_level(int pMidx)
        {
            if (pMidx < 4)
                return 0;

            return (pMidx - 2) >> 1;
        }

        /// <summary>
        /// Calculate the frequencies for all available monsters based upon the current dungeon level.
        /// </summary>
        internal void InitializeMonsters()
        {
            byte i;

            _totalRarity = 0;

            for (i = 0; i < MaxMonster(); i++)
                _totalRarity += CalcMonsterRarity(i);
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
        /// Create a new monster in a given slot.
        /// </summary>
        /// <param name="pmidx"></param>
        private void CreateMonsterIn(int pmidx)
        {
            /* Adjust the 'empty' index. */
            if (_m.Eidx[_d.DungeonLevel] == pmidx)
                _m.Eidx[_d.DungeonLevel] = (byte) _m.MonsterSlots[_d.DungeonLevel, pmidx].Midx;

            /* Create the actual monster. */
            _m.MonsterSlots[_d.DungeonLevel, pmidx].Used = true;
            _m.MonsterSlots[_d.DungeonLevel, pmidx].Midx = RandomMonsterType();
            int x;
            int y;
            GetMonsterCoordinates(out x, out y);
            _m.MonsterSlots[_d.DungeonLevel, pmidx].X = x;
            _m.MonsterSlots[_d.DungeonLevel, pmidx].Y = y;
            _m.MonsterSlots[_d.DungeonLevel, pmidx].Hp =
                _m.MonsterSlots[_d.DungeonLevel, pmidx].MaxHp = MonsterHitPoints(_m.MonsterSlots[_d.DungeonLevel, pmidx].Midx);
            _m.MonsterSlots[_d.DungeonLevel, pmidx].State = MonsterState.Asleep;
        }


        /// <summary>
        /// Find coordinates for a new monster. 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <remarks>Some things need to be considered:
        /// 1. Monsters should only be created on 'floor' tiles.
        /// 2. New monsters should not be in LOS of the PC.
        /// 3. New monsters should not be created in spots where another monster is standing.
        /// </remarks>
        private void GetMonsterCoordinates(out int x, out int y)
        {
            do
            {
                x = Terminal.RandInt(Config.MapW);
                y = Terminal.RandInt(Config.MapH);
            } while (_dungeon.TileAt(x, y) != Tiles.Floor ||
                     LineOfSight(x, y) ||
                     _midx[x, y] != 255);
        }

        /// <summary>
        /// Return an initial hitpoint number for a monster of a given type.
        /// </summary>
        /// <param name="midx"></param>
        /// <returns></returns>
        private int MonsterHitPoints(int midx)
        {
            return Misc.dice(_monsterManual[midx].Hits);
        }

        /// <summary>
        /// Return the first potentially empty monster slot.
        /// </summary>
        /// <returns></returns>
        private int GetMonsterIndex()
        {
            return _m.Eidx[_d.DungeonLevel];
        }


        /// <summary>
        /// Check whether a PC is able to see a position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool LineOfSight(int x, int y)
        {
            int sx, sy, psx, psy;

            /* Adjacent to the PC? */
            if (Misc.iabs(x - _d.PlayerX) <= 1 && Misc.iabs(y - _d.PlayerY) <= 1)
                return true;

            /* Get the section for the given position. */
            _dungeon.GetCurrentSection(x, y, out sx, out sy);

            /* Get the section for the player. */
            _dungeon.GetCurrentSection(_d.PlayerX, _d.PlayerY, out psx, out psy);

            /* In the same room section? */
            return (sx == psx && sy == psy && sx != -1);
        }


        /// <summary>
        /// Get a monster at a specific position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal Monster GetMonsterAt(int x, int y)
        {
            /* Paranoia. */
            if (_midx[x, y] == 255)
                Error.die("No monster to retrieve");

            /* Return the requested monster. */
            return _m.MonsterSlots[_d.DungeonLevel, _midx[x, y]];
        }

        /// <summary>
        /// Return the color for an indexed monster.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        internal ConsoleColor MonsterColor(int pMidx)
        {
            return _monsterManual[_m.MonsterSlots[_d.DungeonLevel, pMidx].Midx].Color;
        }


        /// <summary>
        /// Return the picture for an indexed monster.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        internal char MonsterTile(int pMidx)
        {
            return _monsterManual[_m.MonsterSlots[_d.DungeonLevel, pMidx].Midx].Symbol;
        }

        /// <summary>
        /// Determine whether a monster holds a given position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsMonsterAt(int x, int y)
        {
            return (_midx[x, y] != 255);
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