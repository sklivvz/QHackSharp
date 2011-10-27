using System;
using System.Drawing;

namespace HackSharp
{
    internal class Dungeon
    {
        public static byte existence_chance;
        public static byte[,] map = new byte[Config.MAP_W,Config.MAP_H];
        public DungeonComplex d = new DungeonComplex();

        /// <summary>
        /// Define all the basic dungeon structures and create the complete dungeon map.
        /// </summary>
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

        private byte rand_int(int sectNumber)
        {
            throw new NotImplementedException();
        }

        private void dig_section(byte x, byte y)
        {
            if (rand_byte(100) + 1 >= existence_chance)
            {
                /* No room here. */
                d.s[d.dl, x, y].exists = false;

                /* Decrease the chance for further empty rooms. */
                existence_chance += 3;
            }
            else
            {
                Direction dir;

                /* Yeah :-) ! */
                d.s[d.dl, x, y].exists = true;

                /*
                 * Dig a room.
                 *
                 * Rooms are at least 4x4 tiles in size.
                 */

                do
                {
                    d.s[d.dl, x, y].rx1 = (byte) (x*Config.SECT_W + rand_byte(3) + 1);
                    d.s[d.dl, x, y].ry1 = (byte) (y*Config.SECT_H + rand_byte(3) + 1);
                    d.s[d.dl, x, y].rx2 = (byte) ((x + 1)*Config.SECT_W - rand_byte(3) - 2);
                    d.s[d.dl, x, y].ry2 = (byte) ((y + 1)*Config.SECT_H - rand_byte(3) - 2);
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

                for (dir = Direction.N; dir <= Direction.E; dir++)
                    if (dir_possible(x, y, dir))
                    {
                        switch (dir)
                        {
                            case Direction.N:
                                d.s[d.dl, x, y].dx[(int) dir] =
                                    (byte) (d.s[d.dl, x, y].rx1 + rand_byte(room_width(x, y) - 1) + 1);
                                d.s[d.dl, x, y].dy[(int) dir] = d.s[d.dl, x, y].ry1;
                                break;

                            case Direction.S:
                                d.s[d.dl, x, y].dx[(int) dir] =
                                    (byte) (d.s[d.dl, x, y].rx1 + rand_byte(room_width(x, y) - 1) + 1);
                                d.s[d.dl, x, y].dy[(int) dir] = d.s[d.dl, x, y].ry2;
                                break;

                            case Direction.E:
                                d.s[d.dl, x, y].dy[(int) dir] =
                                    (byte) (d.s[d.dl, x, y].ry1 + rand_byte(room_height(x, y) - 1) + 1);
                                d.s[d.dl, x, y].dx[(int) dir] = d.s[d.dl, x, y].rx2;
                                break;

                            case Direction.W:
                                d.s[d.dl, x, y].dy[(int) dir] =
                                    (byte) (d.s[d.dl, x, y].ry1 + rand_byte(room_height(x, y) - 1) + 1);
                                d.s[d.dl, x, y].dx[(int) dir] = d.s[d.dl, x, y].rx1;
                                break;
                        }
                        d.s[d.dl, x, y].dt[(int) dir] = rand_door();
                    }
                    else
                        d.s[d.dl, x, y].dt[(int) dir] = Tiles.NO_DOOR;
            }
        }

        private byte rand_byte(int p0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculate the room width for a specific room section at (x, y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int room_width(int x, int y)
        {
            return (d.s[d.dl, x, y].rx2 - d.s[d.dl, x, y].rx1 - 1);
        }

        /// <summary>
        /// Calculate the room height for a specific room section at (x, y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int room_height(int x, int y)
        {
            return (d.s[d.dl, x, y].ry2 - d.s[d.dl, x, y].ry1 - 1);
        }

        /// <summary>
        /// Determine a random door type.
        /// </summary>
        /// <returns></returns>
        private byte rand_door()
        {
            byte roll = rand_byte(100);

            if (roll < 75)
                return (byte) Tiles.OPEN_DOOR;
            else if (roll < 90)
                return (byte) Tiles.CLOSED_DOOR;

            return (byte) Tiles.LOCKED_DOOR;
        }

        /// <summary>
        /// Build a map for the current dungeon level.
        /// 
        /// This function is very important for QHack in general.  Levels are only
        /// stored by their section descriptions.  The actual map is created when the
        /// level is entered.  The positive thing about this is that it requires much
        /// less space to save a level in this way (since you only need the outline
        /// descriptions).  The negative thing is that tunneling and other additions
        /// are not possible since the level desciptions have now way of recording
        /// them.
        /// </summary>
        private void build_map()
        {
            int x;
            int y;
            byte sx;
            byte sy;

            /* Basic initialization. */
            for (x = 0; x < Config.MAP_W; x++)
                for (y = 0; y < Config.MAP_H; y++)
                    map[x, y] = (byte) Tiles.ROCK;

            /* Build each section. */
            for (sx = 0; sx < Config.NSECT_W; sx++)
                for (sy = 0; sy < Config.NSECT_H; sy++)
                {
                    /* Handle each section. */
                    if (d.s[d.dl, sx, sy].exists)
                    {
                        /* Paint existing room. */
                        for (x = d.s[d.dl, sx, sy].rx1 + 1;
                             x < d.s[d.dl, sx, sy].rx2;
                             x++)
                            for (y = d.s[d.dl, sx, sy].ry1 + 1;
                                 y < d.s[d.dl, sx, sy].ry2;
                                 y++)
                                map[x, y] = (byte) Tiles.FLOOR;

                        /* Paint doors. */
                        byte dir;
                        for (dir = (byte) Direction.N; dir <= (byte) Direction.E; dir++)
                            if (d.s[d.dl, sx, sy].dt[dir] != Tiles.NO_DOOR)
                                map[d.s[d.dl, sx, sy].dx[dir], d.s[d.dl, sx, sy].dy[dir]]
                                    = d.s[d.dl, sx, sy].dt[dir];
                    }
                }


            /* Connect each section. */
            for (sx = 0; sx < Config.NSECT_W; sx++)
                for (sy = 0; sy < Config.NSECT_H; sy++)
                {
                    if (dir_possible(sx, sy, Direction.E))
                        connect_sections(sx, sy, sx + 1, sy, (int) Direction.E);
                    if (dir_possible(sx, sy, Direction.S))
                        connect_sections(sx, sy, sx, sy + 1, (int) Direction.S);
                }

            /* Place the stairways. */
            map[d.stxu[d.dl], d.styu[d.dl]] = (byte) Tiles.STAIR_UP;
            if (d.dl < Config.MAX_DUNGEON_LEVEL - 1)
                map[d.stxd[d.dl], d.styd[d.dl]] = (byte) Tiles.STAIR_DOWN;
        }


        /// <summary>
        /// Connect two sections of a level.
        /// </summary>
        /// <param name="sx1"></param>
        /// <param name="sy1"></param>
        /// <param name="sx2"></param>
        /// <param name="sy2"></param>
        /// <param name="dir"></param>
        private void connect_sections(int sx1, int sy1, int sx2, int sy2, int dir)
        {
            int cx1;
            int cy1;
            int cx2;
            int cy2;

            /* Get the start byteinates from section #1. */
            if (d.s[d.dl, sx1, sy1].exists)
            {
                if (dir == (byte) Direction.S)
                {
                    cx1 = d.s[d.dl, sx1, sy1].dx[(byte) Direction.S];
                    cy1 = d.s[d.dl, sx1, sy1].dy[(byte) Direction.S];
                }
                else
                {
                    cx1 = d.s[d.dl, sx1, sy1].dx[(byte) Direction.E];
                    cy1 = d.s[d.dl, sx1, sy1].dy[(byte) Direction.E];
                }
            }
            else
            {
                cx1 = sx1*Config.SECT_W + (Config.SECT_W/2);
                cy1 = sy1*Config.SECT_H + (Config.SECT_H/2);
            }

            /* Get the end byteinates from section #2. */
            if (d.s[d.dl, sx2, sy2].exists)
            {
                if (dir == (byte) Direction.S)
                {
                    cx2 = d.s[d.dl, sx2, sy2].dx[(byte) Direction.N];
                    cy2 = d.s[d.dl, sx2, sy2].dy[(byte) Direction.N];
                }
                else
                {
                    cx2 = d.s[d.dl, sx2, sy2].dx[(byte) Direction.W];
                    cy2 = d.s[d.dl, sx2, sy2].dy[(byte) Direction.W];
                }
            }
            else
            {
                cx2 = sx2*Config.SECT_W + (Config.SECT_W/2);
                cy2 = sy2*Config.SECT_H + (Config.SECT_H/2);
            }

            /* Get the middle of the section. */
            int mx = (cx1 + cx2)/2;
            int my = (cy1 + cy2)/2;

            /* Draw the tunnel. */
            int x = cx1;
            int y = cy1;
            if (dir == (byte) Direction.E)
            {
                /* Part #1. */
                while (x < mx)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    x++;
                }

                /* Part #2. */
                if (y < cy2)
                    while (y < cy2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        y++;
                    }
                else
                    while (y > cy2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        y--;
                    }

                /* Part #3. */
                while (x < cx2)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    x++;
                }
                if (map[x, y] == (byte) Tiles.ROCK)
                    map[x, y] = (byte) Tiles.FLOOR;
            }
            else
            {
                /* Part #1. */
                while (y < my)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    y++;
                }
                if (map[x, y] == (byte) Tiles.ROCK)
                    map[x, y] = (byte) Tiles.FLOOR;

                /* Part #2. */
                if (x < cx2)
                    while (x < cx2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        x++;
                    }
                else
                    while (x > cx2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        x--;
                    }

                /* Part #3. */
                while (y < cy2)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    y++;
                }
            }
            if (map[x, y] == (byte) Tiles.ROCK)
                map[x, y] = (byte) Tiles.FLOOR;
        }

        /// <summary>
        /// Determine whether a given section is set on a border.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool dir_possible(int x, int y, Direction dir)
        {
            return ((dir == Direction.N && y > 0) ||
                    (dir == Direction.S && y < Config.NSECT_H - 1) ||
                    (dir == Direction.W && x > 0) ||
                    (dir == Direction.E && x < Config.NSECT_W - 1));
        }


        /// <summary>
        /// Each level requires at least one stair!
        /// </summary>
        private void dig_stairs()
        {
            int sx;
            int sy;
            byte x;
            byte y;

            /* Dig stairs upwards. */

            /* Find a section. */
            get_random_section(out sx, out sy);

            d.stxu[d.dl] = (byte) (d.s[d.dl, sx, sy].rx1 + rand_byte(room_width(sx, sy) - 1) + 1);
            d.styu[d.dl] = (byte) (d.s[d.dl, sx, sy].ry1 + rand_byte(room_height(sx, sy) - 1) + 1);

            /* Dig stairs downwards. */
            if (d.dl < Config.MAX_DUNGEON_LEVEL - 1)
            {
                /* Find a section. */
                get_random_section(out sx, out sy);

                /* Find a good location. */
                do
                {
                    x = (byte) (d.s[d.dl, sx, sy].rx1 + rand_byte(room_width(sx, sy) - 1) + 1);
                    y = (byte) (d.s[d.dl, sx, sy].ry1 + rand_byte(room_height(sx, sy) - 1) + 1);
                } while (d.dl != 0 && x == d.stxu[d.dl] && y == d.styu[d.dl]);

                /* Place the stairway. */
                d.stxd[d.dl] = x;
                d.styd[d.dl] = y;
            }
        }


        /// <summary>
        /// Find a random section on the current dungeon level.
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        private void get_random_section(out int sx, out int sy)
        {
            do
            {
                sx = rand_int(Config.NSECT_W);
                sy = rand_int(Config.NSECT_H);
            } while (!d.s[d.dl, sx, sy].exists);
        }

        /// <summary>
        /// Check whether a given position is accessible.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool is_open(int x, int y)
        {
            switch ((char) map[x, y])
            {
                case Tiles.ROCK:
                case Tiles.LOCKED_DOOR:
                case Tiles.CLOSED_DOOR:
                    return false;

                default:
                    return true;
            }
        }


        /// <summary>
        /// Check whether a given position might be accessible.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool might_be_open(int x, int y)
        {
            switch ((char) map[x, y])
            {
                case Tiles.ROCK:
                    return false;

                default:
                    return true;
            }
        }


        /// <summary>
        /// Memorize a new location.
        /// 
        /// This has two effects: the position is known to you (and will be for
        /// the rest of the game barring magical effects) and it will be displayed
        /// on the screen.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void know(int x, int y)
        {
            if (is_known(x, y))
                return;

            set_knowledge((byte) x, (byte) y, 1);
            print_tile(x, y);
        }


        /// <summary>
        /// This function prints the tile at position (x, y) on the screen.
        /// If necessary the map will be scrolled in 'map_cursor'.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void print_tile(int x, int y)
        {
            map_cursor(x, y);
            print_tile_at_position(x, y);
        }


/*
 * Print the tile at position (x, y) to the current screen position.
 *
 * NOTE: Monsters and items also need to be considered in this function.
 */

        private void print_tile_at_position(int x, int y)
        {
            if (x < 0 || y < 0 || x > Config.MAP_W || y > Config.MAP_H || !is_known(x, y))
            {
                Terminal.set_color(Color.Black);
                Terminal.prtchar(' ');
            }
            else
            {
                if (is_monster_at(x, y) && los(x, y))
                {
                    monster m = get_monster_at(x, y);

                    Terminal.set_color(monster_color(m->midx));
                    Terminal.prtchar(monster_tile(m->midx));
                }
                else
                {
                    set_color_for_tile((char)map[x, y]);
                    Terminal.prtchar(map[x, y]);
                }
            }
        }

        /// <summary>
        /// Makes a complete scetion known.
        /// 
        /// This function is usually called when a room is entered. 
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        private void know_section(int sx, int sy)
        {
            int x, y;

            for (y = d.s[d.dl, sx, sy].ry1;
                 y <= d.s[d.dl, sx, sy].ry2;
                 y++)
                for (x = d.s[d.dl, sx, sy].rx1;
                     x <= d.s[d.dl, sx, sy].rx2;
                     x++)
                    know(x, y);
        }

        /// <summary>
        /// Calculate the current section coordinates.
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        private void get_current_section_coordinates(int px, int py, out int sx, out int sy)
        {
            sx = px/Config.SECT_W;
            sy = py/Config.SECT_H;
        }

        /// <summary>
        /// Calculate the current section coordinates *if* the current section contains a room and the given position is in that room.
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        private void get_current_section(int px, int py, out int sx, out int sy)
        {
            get_current_section_coordinates(px, py, out sx, out sy);

            if (!d.s[d.dl, sx, sy].exists ||
                px < d.s[d.dl, sx, sy].rx1 ||
                px > d.s[d.dl, sx, sy].rx2 ||
                py < d.s[d.dl, sx, sy].ry1 ||
                py > d.s[d.dl, sx, sy].ry2)
            {
                sx = -1;
                sy = -1;
            }
        }

        /// <summary>
        /// Return the tile at position (x, y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private char tile_at(int x, int y)
        {
            return (char) map[x, y];
        }

        /// <summary>
        /// Completely redraw the map.  
        /// Take also care of the visible panel area. 
        /// 
        /// Note: it's important that 'map_cursor' is not called in this function since 'map_cursor' scrolls the screen if this is necessary.  Scrolling the screen entails a call to 'paint_map' and you'd have an endless loop.
        /// </summary>
        private void paint_map()
        {
            int x, y;

            /* Paint the map line by line. */
            for (y = d.psy*Config.SECT_H; y < d.psy*Config.SECT_H + Config.VMAP_H; y++)
            {
                Terminal.cursor(0, 1 + y - d.psy*Config.SECT_H);
                for (x = d.psx*Config.SECT_W; x < d.psx*Config.SECT_W + Config.VMAP_W; x++)
                    print_tile_at_position(x, y);
            }

            /* Update the screen. */
            Terminal.update();
        }

        /// <summary>
        /// Set a color determined by the type of tile to be printed.
        /// </summary>
        /// <param name="tile"></param>
        private void set_color_for_tile(char tile)
        {
            switch (tile)
            {
                case Tiles.ROCK:
                    Terminal.set_color(Color.DarkGray);
                    break;
                case Tiles.FLOOR:
                    Terminal.set_color(Color.LightGray);
                    break;
                case Tiles.STAIR_UP:
                case Tiles.STAIR_DOWN:
                    Terminal.set_color(Color.White);
                    break;
                default:
                    Terminal.set_color(Color.Brown);
                    break;
            }
        }


        /// <summary>
        /// Set the screen cursor based upon the map coordinates. If necessary the screen map will be scrolled to show the current map position on the screen.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void map_cursor(int x, int y)
        {
            bool change = false;
            bool any_change = false;
            int xp, yp;

            do
            {
                /* Any display change necessary? */
                any_change |= change;
                change = false;

                /* Determine the screen coordinates for the map coordinates. */
                xp = x - d.psx*Config.SECT_W;
                yp = y - d.psy*Config.SECT_H + 1;

                /* Check scrolling to the right. */
                if (yp < 1)
                {
                    d.psy--;
                    change = true;
                }
                    /* Check scrolling to the left. */
                else if (yp >= Config.VMAP_H)
                {
                    d.psy++;
                    change = true;
                }
                /* Check scrolling downwards. */
                if (xp < 1)
                {
                    d.psx--;
                    change = true;
                }
                    /* Check scrolling upwards. */
                else if (xp >= Config.VMAP_W)
                {
                    d.psx++;
                    change = true;
                }
            } while (change);

            /* Scroll the map if required to do so. */
            if (any_change)
                paint_map();

            /* Set the cursor. */
            Terminal.cursor(xp, yp);
        }


        /// <summary>
        /// Change a door at a given position to another type of door.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="door"></param>
        private void change_door(int x, int y, byte door)
        {
            int sx, sy;
            byte i;

            get_current_section_coordinates(x, y, out sx, out sy);

            for (i = 0; i < 4; i++)
                if (d.s[d.dl, sx, sy].dx[i] == x && d.s[d.dl, sx, sy].dy[i] == y)
                {
                    d.s[d.dl, sx, sy].dt[i] = door;
                    map[x, y] = door;
                    set_knowledge((byte) x, (byte) y, 0);
                    know(x, y);
                }
        }

        /// <summary>
        /// Determine whether a given position is already known.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <remarks>NOTE: The knowledge map is saved in a bit field to save some memory.</remarks>
        private bool is_known(int x, int y)
        {
            return (d.known[d.dl, x >> 3, y] & (1 << (x%8))) > 0;
        }

        /// <summary>
        /// Set or reset a knowledge bit in the knowledge map.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="known"></param>
        private void set_knowledge(byte x, byte y, byte known)
        {
            if (known > 0)
                d.known[d.dl, x >> 3, y] |= (byte) (1 << (x%8));
            else
                d.known[d.dl, x >> 3, y] &= (byte) (~(1 << (x%8)));
        }
    }
}