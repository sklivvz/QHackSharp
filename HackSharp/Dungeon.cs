using System;

namespace HackSharp
{
    internal class Dungeon
    {
        public static byte ExistenceChance;
        public static byte[,] Map = new byte[Config.MapW, Config.MapH];
        public Complex Complex = new Complex();
        private Monsters _monsters;

        /// <summary>
        /// Define all the basic dungeon structures and create the complete dungeon map.
        /// </summary>
        /// <param name="monsters"></param>
        /// <param name="player"></param>
        public void InitDungeon(Monsters monsters, Player player)
        {
            if (monsters == null) throw new ArgumentNullException("monsters");
            if (player == null) throw new ArgumentNullException("player");
            _monsters = monsters;
            Complex.ThePlayer = player;
            CreateCompleteDungeon();
        }

        /// <summary>
        /// Create all the levels in the dungeon.
        /// </summary>
        private void CreateCompleteDungeon()
        {
            for (Complex.DungeonLevel = 0; Complex.DungeonLevel < Config.MaxDungeonLevel; Complex.DungeonLevel++)
            {
                /* Basic initialization. */

                /* Create the current level map. */
                DigLevel();

                /* Note the current level as unvisited. */
                Complex.Visited[Complex.DungeonLevel] = false;
            }
        }

        /// <summary>
        /// Create one single dungeon level.
        /// </summary>
        private void DigLevel()
        {
            byte w;
            byte h;
            var sectx = new byte[Config.SectNumber];
            var secty = new byte[Config.SectNumber];
            var index = new int[Config.SectNumber];

            // Determine a random order for the section generation.
            /* Initial order. */
            short i = 0;
            for (w = 0; w < Config.NsectW; w++)
                for (h = 0; h < Config.NsectH; h++)
                {
                    index[i] = i;
                    sectx[i] = w;
                    secty[i] = h;
                    i++;
                }

            /* Randomly shuffle the initial order. */
            for (i = 0; i < Config.SectNumber; i++)
            {
                int j = Terminal.RandInt(Config.SectNumber);
                int k = Terminal.RandInt(Config.SectNumber);
                int dummy = index[j];
                index[j] = index[k];
                index[k] = dummy;
            }
            //Create each section separately.

            /* Initially there is a 30% chance for rooms to be non-existant. */
            ExistenceChance = 70;

            /* Dig each section. */
            for (i = 0; i < Config.SectNumber; i++)
                DigSection(sectx[index[i]], secty[index[i]]);

            /* Build some stairs. */
            DigStairs();
        }


        private void DigSection(byte x, byte y)
        {
            if (Terminal.RandByte(100) + 1 >= ExistenceChance)
            {
                /* No room here. */
                Complex.s[Complex.DungeonLevel, x, y].exists = false;

                /* Decrease the chance for further empty rooms. */
                ExistenceChance += 3;
            }
            else
            {
                Direction dir;

                /* Yeah :-) ! */
                Complex.s[Complex.DungeonLevel, x, y].exists = true;

                /*
                 * Dig a room.
                 *
                 * Rooms are at least 4x4 tiles in size.
                 */

                do
                {
                    Complex.s[Complex.DungeonLevel, x, y].rx1 = (byte)(x * Config.SectW + Terminal.RandByte(3) + 1);
                    Complex.s[Complex.DungeonLevel, x, y].ry1 = (byte)(y * Config.SectH + Terminal.RandByte(3) + 1);
                    Complex.s[Complex.DungeonLevel, x, y].rx2 = (byte)((x + 1) * Config.SectW - Terminal.RandByte(3) - 2);
                    Complex.s[Complex.DungeonLevel, x, y].ry2 = (byte)((y + 1) * Config.SectH - Terminal.RandByte(3) - 2);
                } while (Complex.s[Complex.DungeonLevel, x, y].rx2 - Complex.s[Complex.DungeonLevel, x, y].rx1
                         < 3 ||
                         Complex.s[Complex.DungeonLevel, x, y].ry2 - Complex.s[Complex.DungeonLevel, x, y].ry1
                         < 3);

                /*
                 * Create doors.
                 *
                 * XXX: At some point it would be nice to create doors only for
                 *      some directions to make the dungeon less regular.
                 */

                for (dir = Direction.N; dir <= Direction.E; dir++)
                    if (IsDirectionPossible(x, y, dir))
                    {
                        switch (dir)
                        {
                            case Direction.N:
                                Complex.s[Complex.DungeonLevel, x, y].dx[(int)dir] =
                                    (byte)
                                    (Complex.s[Complex.DungeonLevel, x, y].rx1 + Terminal.RandByte((byte)(RoomWidth(x, y) - 1)) +
                                     1);
                                Complex.s[Complex.DungeonLevel, x, y].dy[(int)dir] = Complex.s[Complex.DungeonLevel, x, y].ry1;
                                break;

                            case Direction.S:
                                Complex.s[Complex.DungeonLevel, x, y].dx[(int)dir] =
                                    (byte)
                                    (Complex.s[Complex.DungeonLevel, x, y].rx1 + Terminal.RandByte((byte)(RoomWidth(x, y) - 1)) +
                                     1);
                                Complex.s[Complex.DungeonLevel, x, y].dy[(int)dir] = Complex.s[Complex.DungeonLevel, x, y].ry2;
                                break;

                            case Direction.E:
                                Complex.s[Complex.DungeonLevel, x, y].dy[(int)dir] =
                                    (byte)
                                    (Complex.s[Complex.DungeonLevel, x, y].ry1 + Terminal.RandByte((byte)(RoomHeight(x, y) - 1)) +
                                     1);
                                Complex.s[Complex.DungeonLevel, x, y].dx[(int)dir] = Complex.s[Complex.DungeonLevel, x, y].rx2;
                                break;

                            case Direction.W:
                                Complex.s[Complex.DungeonLevel, x, y].dy[(int)dir] =
                                    (byte)
                                    (Complex.s[Complex.DungeonLevel, x, y].ry1 + Terminal.RandByte((byte)(RoomHeight(x, y) - 1)) +
                                     1);
                                Complex.s[Complex.DungeonLevel, x, y].dx[(int)dir] = Complex.s[Complex.DungeonLevel, x, y].rx1;
                                break;
                        }
                        Complex.s[Complex.DungeonLevel, x, y].dt[(int)dir] = RandDoor();
                    }
                    else
                        Complex.s[Complex.DungeonLevel, x, y].dt[(int)dir] = Tiles.NoDoor;
            }
        }


        /// <summary>
        /// Calculate the room width for a specific room section at (x, y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private byte RoomWidth(int x, int y)
        {
            return (byte)(Complex.s[Complex.DungeonLevel, x, y].rx2 - Complex.s[Complex.DungeonLevel, x, y].rx1 - 1);
        }

        /// <summary>
        /// Calculate the room height for a specific room section at (x, y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private byte RoomHeight(int x, int y)
        {
            return (byte)(Complex.s[Complex.DungeonLevel, x, y].ry2 - Complex.s[Complex.DungeonLevel, x, y].ry1 - 1);
        }

        /// <summary>
        /// Determine a random door type.
        /// </summary>
        /// <returns></returns>
        private byte RandDoor()
        {
            byte roll = Terminal.RandByte(100);

            if (roll < 75)
                return (byte)Tiles.OpenDoor;
            else if (roll < 90)
                return (byte)Tiles.ClosedDoor;

            return (byte)Tiles.LockedDoor;
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
        internal void BuildMap()
        {
            int x;
            int y;
            byte sx;
            byte sy;

            /* Basic initialization. */
            for (x = 0; x < Config.MapW; x++)
                for (y = 0; y < Config.MapH; y++)
                    Map[x, y] = (byte)Tiles.Rock;

            /* Build each section. */
            for (sx = 0; sx < Config.NsectW; sx++)
                for (sy = 0; sy < Config.NsectH; sy++)
                {
                    /* Handle each section. */
                    if (Complex.s[Complex.DungeonLevel, sx, sy].exists)
                    {
                        /* Paint existing room. */
                        for (x = Complex.s[Complex.DungeonLevel, sx, sy].rx1 + 1;
                             x < Complex.s[Complex.DungeonLevel, sx, sy].rx2;
                             x++)
                            for (y = Complex.s[Complex.DungeonLevel, sx, sy].ry1 + 1;
                                 y < Complex.s[Complex.DungeonLevel, sx, sy].ry2;
                                 y++)
                                Map[x, y] = (byte)Tiles.Floor;

                        /* Paint doors. */
                        byte dir;
                        for (dir = (byte)Direction.N; dir <= (byte)Direction.E; dir++)
                            if (Complex.s[Complex.DungeonLevel, sx, sy].dt[dir] != Tiles.NoDoor)
                                Map[Complex.s[Complex.DungeonLevel, sx, sy].dx[dir], Complex.s[Complex.DungeonLevel, sx, sy].dy[dir]]
                                    = Complex.s[Complex.DungeonLevel, sx, sy].dt[dir];
                    }
                }


            /* Connect each section. */
            for (sx = 0; sx < Config.NsectW; sx++)
                for (sy = 0; sy < Config.NsectH; sy++)
                {
                    if (IsDirectionPossible(sx, sy, Direction.E))
                        ConnectSections(sx, sy, sx + 1, sy, Direction.E);
                    if (IsDirectionPossible(sx, sy, Direction.S))
                        ConnectSections(sx, sy, sx, sy + 1, Direction.S);
                }

            /* Place the stairways. */
            Map[Complex.StairsUpX[Complex.DungeonLevel], Complex.StairsUpY[Complex.DungeonLevel]] = (byte)Tiles.StairUp;
            if (Complex.DungeonLevel < Config.MaxDungeonLevel - 1)
                Map[Complex.StairsDownX[Complex.DungeonLevel], Complex.StairsDownY[Complex.DungeonLevel]] = (byte)Tiles.StairDown;
        }


        /// <summary>
        /// Connect two sections of a level.
        /// </summary>
        /// <param name="sx1"></param>
        /// <param name="sy1"></param>
        /// <param name="sx2"></param>
        /// <param name="sy2"></param>
        /// <param name="dir"></param>
        private void ConnectSections(int sx1, int sy1, int sx2, int sy2, Direction dir)
        {
            int cx1;
            int cy1;
            int cx2;
            int cy2;

            /* Get the start byteinates from section #1. */
            if (Complex.s[Complex.DungeonLevel, sx1, sy1].exists)
            {
                if (dir == Direction.S)
                {
                    cx1 = Complex.s[Complex.DungeonLevel, sx1, sy1].dx[(byte)Direction.S];
                    cy1 = Complex.s[Complex.DungeonLevel, sx1, sy1].dy[(byte)Direction.S];
                }
                else
                {
                    cx1 = Complex.s[Complex.DungeonLevel, sx1, sy1].dx[(byte)Direction.E];
                    cy1 = Complex.s[Complex.DungeonLevel, sx1, sy1].dy[(byte)Direction.E];
                }
            }
            else
            {
                cx1 = sx1 * Config.SectW + (Config.SectW / 2);
                cy1 = sy1 * Config.SectH + (Config.SectH / 2);
            }

            /* Get the end byteinates from section #2. */
            if (Complex.s[Complex.DungeonLevel, sx2, sy2].exists)
            {
                if (dir == Direction.S)
                {
                    cx2 = Complex.s[Complex.DungeonLevel, sx2, sy2].dx[(byte)Direction.N];
                    cy2 = Complex.s[Complex.DungeonLevel, sx2, sy2].dy[(byte)Direction.N];
                }
                else
                {
                    cx2 = Complex.s[Complex.DungeonLevel, sx2, sy2].dx[(byte)Direction.W];
                    cy2 = Complex.s[Complex.DungeonLevel, sx2, sy2].dy[(byte)Direction.W];
                }
            }
            else
            {
                cx2 = sx2 * Config.SectW + (Config.SectW / 2);
                cy2 = sy2 * Config.SectH + (Config.SectH / 2);
            }

            /* Get the middle of the section. */
            int mx = (cx1 + cx2) / 2;
            int my = (cy1 + cy2) / 2;

            /* Draw the tunnel. */
            int x = cx1;
            int y = cy1;
            if (dir == Direction.E)
            {
                /* Part #1. */
                while (x < mx)
                {
                    if (Map[x, y] == (byte)Tiles.Rock)
                        Map[x, y] = (byte)Tiles.Floor;
                    x++;
                }

                /* Part #2. */
                if (y < cy2)
                    while (y < cy2)
                    {
                        if (Map[x, y] == (byte)Tiles.Rock)
                            Map[x, y] = (byte)Tiles.Floor;
                        y++;
                    }
                else
                    while (y > cy2)
                    {
                        if (Map[x, y] == (byte)Tiles.Rock)
                            Map[x, y] = (byte)Tiles.Floor;
                        y--;
                    }

                /* Part #3. */
                while (x < cx2)
                {
                    if (Map[x, y] == (byte)Tiles.Rock)
                        Map[x, y] = (byte)Tiles.Floor;
                    x++;
                }
                if (Map[x, y] == (byte)Tiles.Rock)
                    Map[x, y] = (byte)Tiles.Floor;
            }
            else
            {
                /* Part #1. */
                while (y < my)
                {
                    if (Map[x, y] == (byte)Tiles.Rock)
                        Map[x, y] = (byte)Tiles.Floor;
                    y++;
                }
                if (Map[x, y] == (byte)Tiles.Rock)
                    Map[x, y] = (byte)Tiles.Floor;

                /* Part #2. */
                if (x < cx2)
                    while (x < cx2)
                    {
                        if (Map[x, y] == (byte)Tiles.Rock)
                            Map[x, y] = (byte)Tiles.Floor;
                        x++;
                    }
                else
                    while (x > cx2)
                    {
                        if (Map[x, y] == (byte)Tiles.Rock)
                            Map[x, y] = (byte)Tiles.Floor;
                        x--;
                    }

                /* Part #3. */
                while (y < cy2)
                {
                    if (Map[x, y] == (byte)Tiles.Rock)
                        Map[x, y] = (byte)Tiles.Floor;
                    y++;
                }
            }
            if (Map[x, y] == (byte)Tiles.Rock)
                Map[x, y] = (byte)Tiles.Floor;
        }

        /// <summary>
        /// Determine whether a given section is set on a border.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool IsDirectionPossible(int x, int y, Direction direction)
        {
            return ((direction == Direction.N && y > 0) ||
                    (direction == Direction.S && y < Config.NsectH - 1) ||
                    (direction == Direction.W && x > 0) ||
                    (direction == Direction.E && x < Config.NsectW - 1));
        }


        /// <summary>
        /// Each level requires at least one stair!
        /// </summary>
        private void DigStairs()
        {
            int sx;
            int sy;
            byte x;
            byte y;

            /* Dig stairs upwards. */

            /* Find a section. */
            GetRandomSection(out sx, out sy);

            Complex.StairsUpX[Complex.DungeonLevel] =
                (byte)(Complex.s[Complex.DungeonLevel, sx, sy].rx1 + Terminal.RandByte((byte)(RoomWidth(sx, sy) - 1)) + 1);
            Complex.StairsUpY[Complex.DungeonLevel] =
                (byte)(Complex.s[Complex.DungeonLevel, sx, sy].ry1 + Terminal.RandByte((byte)(RoomHeight(sx, sy) - 1)) + 1);

            /* Dig stairs downwards. */
            if (Complex.DungeonLevel < Config.MaxDungeonLevel - 1)
            {
                /* Find a section. */
                GetRandomSection(out sx, out sy);

                /* Find a good location. */
                do
                {
                    x =
                        (byte)
                        (Complex.s[Complex.DungeonLevel, sx, sy].rx1 + Terminal.RandByte((byte)(RoomWidth(sx, sy) - 1)) + 1);
                    y =
                        (byte)
                        (Complex.s[Complex.DungeonLevel, sx, sy].ry1 + Terminal.RandByte((byte)(RoomHeight(sx, sy) - 1)) + 1);
                } while (Complex.DungeonLevel != 0 && x == Complex.StairsUpX[Complex.DungeonLevel] && y == Complex.StairsUpY[Complex.DungeonLevel]);

                /* Place the stairway. */
                Complex.StairsDownX[Complex.DungeonLevel] = x;
                Complex.StairsDownY[Complex.DungeonLevel] = y;
            }
        }


        /// <summary>
        /// Find a random section on the current dungeon level.
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        private void GetRandomSection(out int sx, out int sy)
        {
            do
            {
                sx = Terminal.RandInt(Config.NsectW);
                sy = Terminal.RandInt(Config.NsectH);
            } while (!Complex.s[Complex.DungeonLevel, sx, sy].exists);
        }

        /// <summary>
        /// Check whether a given position is accessible.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsOpen(int x, int y)
        {
            switch ((char)Map[x, y])
            {
                case Tiles.Rock:
                case Tiles.LockedDoor:
                case Tiles.ClosedDoor:
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
        internal bool MightBeOpen(int x, int y)
        {
            switch ((char)Map[x, y])
            {
                case Tiles.Rock:
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
        internal void Know(int x, int y)
        {
            if (x < 0 || x >= Config.MapW || y < 0 || y >= Config.MapH || Complex.IsKnown(x, y))
                return;

            Complex.SetKnowledge(x, y, true);
            PrintTile(x, y);
        }


        /// <summary>
        /// This function prints the tile at position (x, y) on the screen.
        /// If necessary the map will be scrolled in 'map_cursor'.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void PrintTile(int x, int y)
        {
            MapCursor(x, y);
            PrintTileAtPosition(x, y);
        }

        /// <summary>
        ///  Print the tile at position (x, y) to the current screen position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <remarks>Monsters and items also need to be considered in this function.</remarks>
        private void PrintTileAtPosition(int x, int y)
        {
            if (x < 0 || y < 0 || x > Config.MapW || y > Config.MapH || !Complex.IsKnown(x, y))
            {
                Terminal.SetColor(ConsoleColor.Black);
                Terminal.PrintChar(' ');
            }
            else
            {
                if (_monsters.IsMonsterAt(x, y) && _monsters.LineOfSight(x, y))
                {
                    Monster m = _monsters.GetMonsterAt(x, y);

                    Terminal.SetColor(_monsters.MonsterColor(m.Midx));
                    Terminal.PrintChar(_monsters.MonsterTile(m.Midx));
                }
                else
                {
                    set_color_for_tile((char)Map[x, y]);
                    Terminal.PrintChar((char)Map[x, y]);
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
        internal void KnowSection(int sx, int sy)
        {
            int x, y;

            for (y = Complex.s[Complex.DungeonLevel, sx, sy].ry1;
                 y <= Complex.s[Complex.DungeonLevel, sx, sy].ry2;
                 y++)
                for (x = Complex.s[Complex.DungeonLevel, sx, sy].rx1;
                     x <= Complex.s[Complex.DungeonLevel, sx, sy].rx2;
                     x++)
                    Know(x, y);
        }

        /// <summary>
        /// Calculate the current section coordinates.
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        internal void GetCurrentSectionCoordinates(int px, int py, out int sx, out int sy)
        {
            sx = px / Config.SectW;
            sy = py / Config.SectH;
        }

        /// <summary>
        /// Calculate the current section coordinates *if* the current section contains a room and the given position is in that room.
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        internal void GetCurrentSection(int px, int py, out int sx, out int sy)
        {
            GetCurrentSectionCoordinates(px, py, out sx, out sy);

            if (!Complex.s[Complex.DungeonLevel, sx, sy].exists ||
                px < Complex.s[Complex.DungeonLevel, sx, sy].rx1 ||
                px > Complex.s[Complex.DungeonLevel, sx, sy].rx2 ||
                py < Complex.s[Complex.DungeonLevel, sx, sy].ry1 ||
                py > Complex.s[Complex.DungeonLevel, sx, sy].ry2)
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
        internal char TileAt(int x, int y)
        {
            return (char)Map[x, y];
        }

        /// <summary>
        /// Completely redraw the map.  
        /// Take also care of the visible panel area. 
        /// </summary>
        /// <remarks>it's important that 'map_cursor' is not called in this function since 'map_cursor' scrolls the screen if this is necessary.  Scrolling the screen entails a call to 'paint_map' and you'Complex have an endless loop.</remarks>
        internal void PaintMap()
        {
            int x, y;

            /* Paint the map line by line. */
            for (y = Complex.psy * Config.SectH; y < Complex.psy * Config.SectH + Config.VmapH; y++)
            {
                Terminal.Cursor(0, 1 + y - Complex.psy * Config.SectH);
                for (x = Complex.psx * Config.SectW; x < Complex.psx * Config.SectW + Config.VmapW; x++)
                    PrintTileAtPosition(x, y);
            }

            /* Update the screen. */
            Terminal.Update();
        }

        /// <summary>
        /// Set a color determined by the type of tile to be printed.
        /// </summary>
        /// <param name="tile"></param>
        private void set_color_for_tile(char tile)
        {
            switch (tile)
            {
                case Tiles.Rock:
                    Terminal.SetColor(ConsoleColor.DarkGray);
                    break;
                case Tiles.Floor:
                    Terminal.SetColor(ConsoleColor.Gray);
                    break;
                case Tiles.StairUp:
                case Tiles.StairDown:
                    Terminal.SetColor(ConsoleColor.White);
                    break;
                default:
                    Terminal.SetColor(ConsoleColor.DarkYellow);
                    break;
            }
        }


        /// <summary>
        /// Set the screen cursor based upon the map coordinates. If necessary the screen map will be scrolled to show the current map position on the screen.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void MapCursor(int x, int y)
        {
            bool change = false;
            bool anyChange = false;
            int xp;
            int yp;

            do
            {
                /* Any display change necessary? */
                anyChange |= change;
                change = false;

                /* Determine the screen coordinates for the map coordinates. */
                xp = x - Complex.psx * Config.SectW;
                yp = y - Complex.psy * Config.SectH + 1;

                /* Check scrolling to the right. */
                if (yp < 1)
                {
                    Complex.psy--;
                    change = true;
                }
                /* Check scrolling to the left. */
                else if (yp >= Config.VmapH)
                {
                    Complex.psy++;
                    change = true;
                }
                /* Check scrolling downwards. */
                if (xp < 1)
                {
                    Complex.psx--;
                    change = true;
                }
                /* Check scrolling upwards. */
                else if (xp >= Config.VmapW)
                {
                    Complex.psx++;
                    change = true;
                }
            } while (change);

            /* Scroll the map if required to do so. */
            if (anyChange)
                PaintMap();

            /* Set the cursor. */
            Terminal.Cursor(xp, yp);
        }


        /// <summary>
        /// Change a door at a given position to another type of door.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="door"></param>
        internal void ChangeDoor(int x, int y, char door)
        {
            int sx, sy;
            byte i;

            GetCurrentSectionCoordinates(x, y, out sx, out sy);

            for (i = 0; i < 4; i++)
                if (Complex.s[Complex.DungeonLevel, sx, sy].dx[i] == x && Complex.s[Complex.DungeonLevel, sx, sy].dy[i] == y)
                {
                    Complex.s[Complex.DungeonLevel, sx, sy].dt[i] = (byte)door;
                    Map[x, y] = (byte)door;
                    Complex.SetKnowledge(x, y, false);
                    Know(x, y);
                }
        }


    }
}