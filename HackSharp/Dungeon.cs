namespace HackSharp
{
    internal class Dungeon
    {
        public static byte existence_chance;
        public static byte[,] map = new byte[Config.MAP_W, Config.MAP_H];
        public DungeonComplex d;


        //Define all the basic dungeon structures and create the complete dungeon map.
        public void init_dungeon()
        {
            create_complete_dungeon();
        }

        /// <summary>
        /// Create all the levels in the dungeon.
        /// </summary>
        private void create_complete_dungeon()
        {
            for (d.dl = 0; d.dl < Config.MAX_DUNGEON_LEVEL; d.dl++)
            {
                /* Basic initialization. */

                /* Nothing is known about the dungeon at this point. */
                for (byte x = 0; x < Config.MAP_W; x++)
                    for (byte y = 0; y < Config.MAP_H; y++)
                        set_knowledge(x, y, 0);

                /* Create the current level map. */
                dig_level();

                /* Note the current level as unvisited. */
                d.visited[d.dl] = false;
            }
        }

        /// <summary>
        /// Create one single dungeon level.
        /// </summary>
        private void dig_level()
        {
            byte w;
            byte h;
            var sectx = new byte[Config.SECT_NUMBER];
            var secty = new byte[Config.SECT_NUMBER];
            var index = new short[Config.SECT_NUMBER];

            // Determine a random order for the section generation.
            /* Initial order. */
            short i = 0;
            for (w = 0; w < Config.NSECT_W; w++)
                for (h = 0; h < Config.NSECT_H; h++)
                {
                    index[i] = i;
                    sectx[i] = w;
                    secty[i] = h;
                    i++;
                }

            /* Randomly shuffle the initial order. */
            for (i = 0; i < Config.SECT_NUMBER; i++)
            {
                short j = rand_int(Config.SECT_NUMBER);
                short k = rand_int(Config.SECT_NUMBER);
                short dummy = index[j];
                index[j] = index[k];
                index[k] = dummy;
            }
            //Create each section separately.

            /* Initially there is a 30% chance for rooms to be non-existant. */
            existence_chance = 70;

            /* Dig each section. */
            for (i = 0; i < Config.SECT_NUMBER; i++)
                dig_section(sectx[index[i]], secty[index[i]]);

            /* Build some stairs. */
            dig_stairs();
        }

        private void dig_section(byte x, byte y)
        {
            if (rand_byte(100) + 1 >= existence_chance)
            {
                /* No room here. */
                d.s[d.dl][x][y].exists = false;

                /* Decrease the chance for further empty rooms. */
                existence_chance += 3;
            }
            else
            {
                byte dir;

                /* Yeah :-) ! */
                d.s[d.dl, x, y].exists = true;

                /*
                 * Dig a room.
                 *
                 * Rooms are at least 4x4 tiles in size.
                 */

                do
                {
                    d.s[d.dl, x, y].rx1 = x * Config.SECT_W + rand_byte(3) + 1;
                    d.s[d.dl, x, y].ry1 = y * Config.SECT_H + rand_byte(3) + 1;
                    d.s[d.dl, x, y].rx2 = (x + 1) * Config.SECT_W - rand_byte(3) - 2;
                    d.s[d.dl, x, y].ry2 = (y + 1) * Config.SECT_H - rand_byte(3) - 2;
                } while (d.s[d.dl, x, y].rx2 - d.s[d.dl, x, y].rx1
                         < 3 ||
                         d.s[d.dl, x, y].ry2 - d.s[d.dl, x, y].ry1
                         < 3);

                /*
                 * Create doors.
                 *
                 * XXX: At some point it would be nice to create doors only for
                 *      some directions to make the dungeon less regular.
                 */

                for (dir = (byte)Direction.N; dir <= (byte)Direction.E; dir++)
                    if (dir_possible(x, y, dir))
                    {
                        switch ((Direction)dir)
                        {
                            case Direction.N:
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx1 + rand_byte(room_width(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry1;
                                break;

                            case Direction.S:
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx1 + rand_byte(room_width(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry2;
                                break;

                            case Direction.E:
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry1 + rand_byte(room_height(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx2;
                                break;

                            case Direction.W:
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry1 + rand_byte(room_height(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx1;
                                break;

                            default:
                                break;
                        }
                        d.s[d.dl, x, y].dt[dir] = rand_door();
                    }
                    else
                        d.s[d.dl, x, y].dt[dir] = Tiles.NO_DOOR;
            }
        }
    }
}