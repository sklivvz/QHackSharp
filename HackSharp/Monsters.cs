using System;

namespace HackSharp
{
    class Monsters
    {

        monster_struct m = new monster_struct();

        /* The total number of monsters. */
        private const int MAX_MONSTER = 4;



        /* The complete monster list for the game. */
        readonly monster_def[] md = new monster_def[]
                                        {
                                            new monster_def{symbol='k', color = ConsoleColor.Green, name = "kobold", ac = 14, hits = "1d4", attacks = 1,to_hit = 0, damage = "1d6",rarity = monster_rarity.COMMON},
                                            new monster_def{symbol='r', color = ConsoleColor.DarkYellow, name = "rat", ac = 12, hits = "1d3", attacks = 1,to_hit = 0, damage = "1d3", rarity = monster_rarity.COMMON},
                                            new monster_def{symbol='g', color = ConsoleColor.Cyan, name = "goblin", ac = 13, hits = "1d8", attacks = 1,to_hit = 0, damage = "1d6", rarity = monster_rarity.COMMON},
                                            new monster_def{symbol='x', color = ConsoleColor.Yellow, name = "lightning bug", ac = 18, hits = "2d3", attacks = 1, to_hit = 1,damage = "1d4", rarity = monster_rarity.RARE}
                                        };

        /* The dynamic index map for one monster level. */
        byte[,] midx = new byte[Config.MAP_W,Config.MAP_H];

        /* The total rarity for monsters; dependent on the current level. */
        int total_rarity;

        private Dungeon _dungeon;
        private DungeonComplex d;

        /// <summary>
        /// Initialize the monster structures.  Basically we have to notify all slots that they are empty.  Also the general index map needs to be initialized.
        /// </summary>
        internal void init_monsters(Dungeon dungeon)
        {
            _dungeon = dungeon;
            d = _dungeon.d;

            int i;
            int j;

            for (i = 0; i < Config.MAX_DUNGEON_LEVEL; i++)
            {
                /* The first empty monster slot. */
                m.eidx[i] = 0;

                /* Initially all slots are empty. */
                for (j = 0; j < Config.MONSTERS_PER_LEVEL - 1; j++)
                {
                    m.m[i,j].used = false;
                    m.m[i,j].midx = j + 1;
                }

                /* The last one points to 'no more slots'. */
                m.m[i,Config.MONSTERS_PER_LEVEL - 1].midx = -1;
                m.m[i,Config.MONSTERS_PER_LEVEL - 1].used = false;
            }

            /* Initialize the monster index map as 'empty'. */
            for (i = 0; i < Config.MAP_W; i++)
                for (j = 0; j < Config.MAP_H; j++)
                    midx[i,j] = 255;
        }


        /// <summary>
        /// Create the monster map for a given dungeon level.
        /// </summary>
        internal void build_monster_map()
        {
            int x, y;

            /* Initialize the monster index map as 'empty'. */
            for (x = 0; x < Config.MAP_W; x++)
                for (y = 0; y < Config.MAP_H; y++)
                    midx[x,y] = 255;

            /* Setup all monster indices. */
            for (x = 0; x < Config.MONSTERS_PER_LEVEL; x++)
                if (m.m[d.dl,x].used)
                    midx[m.m[d.dl,x].x,m.m[d.dl,x].y] = (byte) x;
        }

        /// <summary>
        /// Create an initial monster population for a given level.
        /// </summary>
        internal void create_population()
        {
            byte m;

            /* Initialize the basic monster data. */
            initialize_monsters();

            for (m = 0; m < Config.INITIAL_MONSTER_NUMBER; m++)
            {
                int index;

                /* Find a monster index. */
                index = get_monster_index();

                /* Paranoia. */
                if (index == -1)
                    Error.die("Could not create the initial monster population");

                /* Create a new monster. */
                create_monster_in(index);
            }
        }



        /// <summary>
        /// Return the maximum monster number for the current dungeon level.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Since the current monster list is somewhat limited only four monsters are available.</remarks>
        int max_monster()
        {
            return Misc.imin(MAX_MONSTER, ((d.dl << 1) + 4));
        }


        static int[] lmod =
            {
                100, 90, 80, 72, 64, 56, 50, 42, 35, 28, 20, 12, 4, 1
            };
        
        /// <summary>
        /// Determine the frequency for a given monster.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        /// <remarks>This value is level-dependent.  If the monster is out-of-depth (for QHack this means 'has a lower minimum level than the current dungeon level) it's frequency will be reduced.</remarks>
        int calc_monster_rarity(int pMidx)
        {
            var rarity = (int) md[pMidx].rarity;
            int levelDiff = d.dl - monster_level(pMidx);

            return Misc.imax(1, (rarity * lmod[Misc.imin(13, levelDiff)]) / 100);
        }


        /// <summary>
        /// Determine the minimum level for a given monster number.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        int monster_level(int pMidx)
        {
            if (pMidx < 4)
                return 0;

            return (pMidx - 2) >> 1;
        }

        /// <summary>
        /// Calculate the frequencies for all available monsters based upon the current dungeon level.
        /// </summary>
        internal void initialize_monsters()
        {
            byte i;
  
            total_rarity = 0;

            for (i = 0; i < max_monster(); i++)
                total_rarity += calc_monster_rarity(i);
        }

        /// <summary>
        /// Determine the index number for a random monster on the current dungeon level.
        /// </summary>
        /// <returns></returns>
        int random_monster_type()
        {
            int roll;
            int i;

            roll = rand_long(total_rarity) + 1;
            i = 0;

            while (roll > calc_monster_rarity(i))
            {
                roll -= calc_monster_rarity(i);
                i++;
            }
  
            return i;
        }


        /// <summary>
        /// Create a new monster in a given slot.
        /// </summary>
        /// <param name="pmidx"></param>
        void create_monster_in(int pmidx)
        {
            /* Adjust the 'empty' index. */
            if (m.eidx[d.dl] == pmidx)
                m.eidx[d.dl] = (byte) m.m[d.dl,pmidx].midx; 

            /* Create the actual monster. */
            m.m[d.dl,pmidx].used = true;
            m.m[d.dl,pmidx].midx = random_monster_type();
            get_monster_coordinates(out m.m[d.dl,pmidx].x, out m.m[d.dl,pmidx].y);
            m.m[d.dl,pmidx].hp = m.m[d.dl,pmidx].max_hp = mhits(m.m[d.dl,pmidx].midx);
            m.m[d.dl,pmidx].state = monster_state.ASLEEP;
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
        void get_monster_coordinates(out int x,  out int y)
        {
            do
            {
                x = rand_int(Config.MAP_W);
                y = rand_int(Config.MAP_H);
            }
            while (_dungeon.tile_at(x, y) != Tiles.FLOOR ||
                   los(x, y) ||
                   midx[x,y] != 255);
        }

        /// <summary>
        /// Return an initial hitpoint number for a monster of a given type.
        /// </summary>
        /// <param name="midx"></param>
        /// <returns></returns>
        int mhits(int midx)
        {
            return Misc.dice(md[midx].hits);
        }

        /// <summary>
        /// Return the first potentially empty monster slot.
        /// </summary>
        /// <returns></returns>
        int get_monster_index()
        {
            return m.eidx[d.dl];
        }


        /// <summary>
        /// Check whether a PC is able to see a position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool los(int x, int y)
        {
            int sx, sy, psx, psy;

            /* Adjacent to the PC? */
            if (Misc.iabs(x - d.px) <= 1 && Misc.iabs(y - d.py) <= 1)
                return true;

            /* Get the section for the given position. */
            _dungeon.get_current_section(x, y, out sx, out sy);

            /* Get the section for the player. */
            _dungeon.get_current_section(d.px, d.py, out psx, out psy);

            /* In the same room section? */
            return (sx == psx && sy == psy && sx != -1);
        }


        /// <summary>
        /// Get a monster at a specific position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal monster get_monster_at(int x, int y)
        {
            /* Paranoia. */
            if (midx[x,y] == 255)
                Error.die("No monster to retrieve");

            /* Return the requested monster. */
            return m.m[d.dl,midx[x,y]];
        }

        /// <summary>
        /// Return the color for an indexed monster.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        internal ConsoleColor monster_color(int pMidx)
        {
            return md[m.m[d.dl,pMidx].midx].color;
        }



        /// <summary>
        /// Return the picture for an indexed monster.
        /// </summary>
        /// <param name="pMidx"></param>
        /// <returns></returns>
        internal char monster_tile(int pMidx)
        {
            return md[m.m[d.dl,pMidx].midx].symbol;
        }

        /// <summary>
        /// Determine whether a monster holds a given position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool is_monster_at(int x, int y)
        {
            return (midx[x,y] != 255);
        }

        /// <summary>
        /// Handle the monster turn: movement, combat, etc.
        /// </summary>
        void move_monsters()
        {
            //TODO: Implement this
        }

    }
}