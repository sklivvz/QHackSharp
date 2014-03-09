using System;

namespace HackSharp
{
    internal class Dungeon
    {
        public static byte ExistenceChance;
        public static byte[,] Map = new byte[Config.MapW, Config.MapH];
        public Complex TheComplex = new Complex();
        private Monsters monsters;

        /// <summary>
        /// Define all the basic dungeon structures and create the complete dungeon map.
        /// </summary>
        /// <param name="monsters"></param>
        /// <param name="player"></param>
        public void InitDungeon(Monsters monsters, Player player)
        {
            if (monsters == null) throw new ArgumentNullException("monsters");
            if (player == null) throw new ArgumentNullException("player");
            this.monsters = monsters;
            TheComplex.ThePlayer = player;
            CreateCompleteDungeon();
        }

        /// <summary>
        /// Create all the levels in the dungeon.
        /// </summary>
        private void CreateCompleteDungeon()
        {
            for (TheComplex.DungeonLevel = 0; TheComplex.DungeonLevel < Config.MaxDungeonLevel; TheComplex.DungeonLevel++)
            {
                /* Basic initialization. */

                /* Create the current level map. */
                DigLevel();

                /* Note the current level as unvisited. */
                TheComplex.Visited[TheComplex.DungeonLevel] = false;
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
            {
                DigSection(new Position(sectx[index[i]], secty[index[i]]));
            }

            /* Build some stairs. */
            DigStairs();
        }


        private void DigSection(Position p)
        {
            if (Terminal.RandByte(100) + 1 >= ExistenceChance)
            {
                /* No room here. */
                TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].Exists = false;

                /* Decrease the chance for further empty rooms. */
                ExistenceChance += 3;
            }
            else
            {
                Direction dir;

                /* Yeah :-) ! */
                TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].Exists = true;

                /*
                 * Dig a room.
                 *
                 * Rooms are at least 4x4 tiles in size.
                 */

                do
                {
                    TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].TopLeft = new Position(p.X * Config.SectW + Terminal.RandByte(3) + 1, p.Y * Config.SectH + Terminal.RandByte(3) + 1);
                    TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].BottomRight = new Position((p.X + 1) * Config.SectW + Terminal.RandByte(3) - 2, (p.Y + 1) * Config.SectH + Terminal.RandByte(3) - 2);
                } while (TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].BottomRight.X - TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].TopLeft.X
                         < 3 ||
                         TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].BottomRight.Y - TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].TopLeft.Y
                         < 3);

                /*
                 * Create doors.
                 *
                 * XXX: At some point it would be nice to create doors only for
                 *      some directions to make the dungeon less regular.
                 */

                for (dir = Direction.N; dir <= Direction.E; dir++)
                {
                    var curSec = TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y];
                    if (IsDirectionPossible(p, dir))
                    {
                        switch (dir)
                        {
                            case Direction.N:
                                curSec.Doors[(int)dir] = new Position(
                                        curSec.TopLeft.X + Terminal.RandByte((byte)(RoomWidth(p) - 1)) + 1, 
                                        curSec.TopLeft.Y);
                                break;

                            case Direction.S:
                                curSec.Doors[(int)dir] = new Position(
                                        curSec.TopLeft.X + Terminal.RandByte((byte)(RoomWidth(p) - 1)) + 1,
                                        curSec.BottomRight.Y);
                                break;

                            case Direction.E:
                                curSec.Doors[(int)dir] = new Position(
                                        curSec.BottomRight.X,
                                        curSec.TopLeft.Y + Terminal.RandByte((byte)(RoomHeight(p) - 1)) + 1);
                                break;

                            case Direction.W:
                                curSec.Doors[(int)dir] = new Position(
                                        curSec.TopLeft.X,
                                        curSec.TopLeft.Y + Terminal.RandByte((byte)(RoomHeight(p) - 1)) + 1);
                                break;
                        }
                        curSec.dt[(int)dir] = RandDoor();
                    }
                    else
                        curSec.dt[(int)dir] = Tiles.NoDoor;
                }
            }
        }


        /// <summary>
        /// Calculate the room width for a specific room section at p.
        /// </summary>
        /// <returns></returns>
        private byte RoomWidth(Position p)
        {
            return (byte)(TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].BottomRight.X - TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].TopLeft.X - 1);
        }

        /// <summary>
        /// Calculate the room height for a specific room section at p.
        /// </summary>
        /// <returns></returns>
        private byte RoomHeight(Position p)
        {
            return (byte)(TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].BottomRight.Y - TheComplex.s[TheComplex.DungeonLevel, p.X, p.Y].TopLeft.Y - 1);
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
            if (roll < 90)
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
                    var section = TheComplex.s[TheComplex.DungeonLevel, sx, sy];
                    /* Handle each section. */
                    if (!section.Exists) continue;
                    /* Paint existing room. */
                    for (x = section.TopLeft.X + 1; x < section.BottomRight.X; x++)
                        for (y = section.TopLeft.Y + 1; y < section.BottomRight.Y; y++)
                            Map[x, y] = (byte)Tiles.Floor;

                    /* Paint doors. */
                    for (var dir = Direction.N; dir <= Direction.E; dir++)
                        if (section.dt[(byte) dir] != Tiles.NoDoor)
                            Map[section.Doors[(byte) dir].X, section.Doors[(byte) dir].Y] = (byte) section.dt[(byte)dir];
                }


            /* Connect each section. */
            for (sx = 0; sx < Config.NsectW; sx++)
                for (sy = 0; sy < Config.NsectH; sy++)
                {
                    var position = new Position(sx, sy);
                    if (IsDirectionPossible(position, Direction.E))
                        ConnectSections(sx, sy, sx + 1, sy, Direction.E);
                    if (IsDirectionPossible(position, Direction.S))
                        ConnectSections(sx, sy, sx, sy + 1, Direction.S);
                }

            /* Place the stairways. */
            Map[TheComplex.StairsUp[TheComplex.DungeonLevel].X, TheComplex.StairsUp[TheComplex.DungeonLevel].Y] = (byte)Tiles.StairUp;
            if (TheComplex.DungeonLevel < Config.MaxDungeonLevel - 1)
                Map[TheComplex.StairsDown[TheComplex.DungeonLevel].X, TheComplex.StairsDown[TheComplex.DungeonLevel].Y] = (byte)Tiles.StairDown;
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
            /* Get the start byteinates from section #1. */
            var sec1 = TheComplex.s[TheComplex.DungeonLevel, sx1, sy1];
            var c1 = sec1.Exists
                ? (dir == Direction.S
                    ? sec1.Doors[(byte) Direction.S]
                    : sec1.Doors[(byte) Direction.E])
                : new Position(
                    sx1*Config.SectW + (Config.SectW/2),
                    sy1*Config.SectH + (Config.SectH/2)
                    );

            /* Get the end byteinates from section #2. */
            var sec2 = TheComplex.s[TheComplex.DungeonLevel, sx2, sy2];
            var c2 = sec2.Exists
                ? (dir == Direction.S
                    ? sec2.Doors[(byte) Direction.N]
                    : sec2.Doors[(byte) Direction.W])
                : new Position(
                    sx2*Config.SectW + (Config.SectW/2),
                    sy2*Config.SectH + (Config.SectH/2)
                    );

            /* Get the middle of the section. */
            int mx = (c1.X + c2.X) / 2;
            int my = (c1.Y + c2.Y) / 2;

            /* Draw the tunnel. */
            int x = c1.X;
            int y = c1.Y;
            const byte rock = (byte)Tiles.Rock;
            const byte floor = (byte)Tiles.Floor;
            if (dir == Direction.E)
            {
                /* Part #1. */
                while (x < mx)
                {
                    if (Map[x, y] == rock)
                        Map[x, y] = floor;
                    x++;
                }

                /* Part #2. */
                if (y < c2.Y)
                    while (y < c2.Y)
                    {
                        if (Map[x, y] == rock)
                            Map[x, y] = floor;
                        y++;
                    }
                else
                    while (y > c2.Y)
                    {
                        if (Map[x, y] == rock)
                            Map[x, y] = floor;
                        y--;
                    }

                /* Part #3. */
                while (x < c2.X)
                {
                    if (Map[x, y] == rock)
                        Map[x, y] = floor;
                    x++;
                }
                if (Map[x, y] == rock)
                    Map[x, y] = floor;
            }
            else
            {
                /* Part #1. */
                while (y < my)
                {
                    if (Map[x, y] == rock)
                        Map[x, y] = floor;
                    y++;
                }
                if (Map[x, y] == rock)
                    Map[x, y] = floor;

                /* Part #2. */
                if (x < c2.X)
                    while (x < c2.X)
                    {
                        if (Map[x, y] == rock)
                            Map[x, y] = floor;
                        x++;
                    }
                else
                    while (x > c2.X)
                    {
                        if (Map[x, y] == rock)
                            Map[x, y] = floor;
                        x--;
                    }

                /* Part #3. */
                while (y < c2.Y)
                {
                    if (Map[x, y] == rock)
                        Map[x, y] = floor;
                    y++;
                }
            }
            if (Map[x, y] == rock)
                Map[x, y] = floor;
        }

        /// <summary>
        /// Determine whether a given section is set on a border.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static bool IsDirectionPossible(Position p, Direction direction)
        {
            return ((direction == Direction.N && p.Y > 0) ||
                    (direction == Direction.S && p.Y < Config.NsectH - 1) ||
                    (direction == Direction.W && p.X > 0) ||
                    (direction == Direction.E && p.X < Config.NsectW - 1));
        }

        /// <summary>
        /// Each level requires at least one stair!
        /// </summary>
        private void DigStairs()
        {
            /* Dig stairs upwards. */

            /* Find a section. */
            var s1 = GetRandomSection();

            TheComplex.StairsUp[TheComplex.DungeonLevel] = new Position(
                    (TheComplex.s[TheComplex.DungeonLevel, s1.X, s1.Y].TopLeft.X + Terminal.RandByte((byte)(RoomWidth(s1) - 1)) + 1),
                    (TheComplex.s[TheComplex.DungeonLevel, s1.X, s1.Y].TopLeft.Y + Terminal.RandByte((byte)(RoomHeight(s1) - 1)) + 1)
                );

            /* Dig stairs downwards. */
            if (TheComplex.DungeonLevel < Config.MaxDungeonLevel - 1)
            {
                /* Find a section. */
                var s2 = GetRandomSection();

                Position position;
                /* Find a good location. */
                do
                {
                    position = new Position(
                        TheComplex.s[TheComplex.DungeonLevel, s2.X, s2.Y].TopLeft.X + Terminal.RandByte((byte)(RoomWidth(s2) - 1)) + 1,
                        TheComplex.s[TheComplex.DungeonLevel, s2.X, s2.Y].TopLeft.Y + Terminal.RandByte((byte)(RoomHeight(s2) - 1)) + 1);
                } while (TheComplex.DungeonLevel != 0 && position.Equals(TheComplex.StairsUp[TheComplex.DungeonLevel]));

                /* Place the stairway. */
                TheComplex.StairsDown[TheComplex.DungeonLevel] = position;
            }
        }


        /// <summary>
        /// Find a random section on the current dungeon level.
        /// </summary>
        private Position GetRandomSection()
        {
            int sx, sy;
            do
            {
                sx = Terminal.RandInt(Config.NsectW);
                sy = Terminal.RandInt(Config.NsectH);
            } while (!TheComplex.s[TheComplex.DungeonLevel, sx, sy].Exists);
            return new Position(sx, sy);
        }

        /// <summary>
        /// Check whether a given position is accessible.
        /// </summary>
        internal bool IsOpen(Position p)
        {
            switch ((char)Map[p.X, p.Y])
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
        internal bool MightBeOpen(Position p)
        {
            return (char)Map[p.X, p.Y] != Tiles.Rock;
        }


        /// <summary>
        /// Memorize a new location.
        /// 
        /// This has two effects: the position is known to you (and will be for
        /// the rest of the game barring magical effects) and it will be displayed
        /// on the screen.
        /// </summary>
        internal void Know(Position p)
        {
            TheComplex.Known[TheComplex.DungeonLevel, p.X, p.Y] = true;
            PrintTile(p);
        }


        /// <summary>
        /// This function prints the tile at position (x, y) on the screen.
        /// If necessary the map will be scrolled in 'map_cursor'.
        /// </summary>
        internal void PrintTile(Position p)
        {
            MapCursor(p);
            PrintTileAtPosition(p);
        }

        private bool EnsureCoordinates(Position p)
        {
            return p.X < Config.MapW && p.Y < Config.MapH && p.X >= 0 && p.Y >= 0;
        }

        /// <summary>
        ///  Print the tile at position (x, y) to the current screen position.
        /// </summary>
        /// <remarks>Monsters and items also need to be considered in this function.</remarks>
        private void PrintTileAtPosition(Position p)
        {
            if (!EnsureCoordinates(p) || !TheComplex.Known[TheComplex.DungeonLevel, p.X, p.Y])
            {
                Terminal.SetColor(ConsoleColor.Black);
                Terminal.PrintChar(' ');
            }
            else
            {
                if (monsters.IsMonsterAt(p) && monsters.LineOfSight(p))
                {
                    Monster m = monsters.GetMonsterAt(p);

                    Terminal.SetColor(m.Color);
                    Terminal.PrintChar(m.Tile);
                }
                else
                {
                    set_color_for_tile((char)Map[p.X, p.Y]);
                    Terminal.PrintChar((char)Map[p.X, p.Y]);
                }
            }
        }

        /// <summary>
        /// Makes a complete scetion known.
        /// 
        /// This function is usually called when a room is entered. 
        /// </summary>
        internal void KnowSection(Position s)
        {
            Position topLeft = new Position(TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].BottomRight.X,TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].BottomRight.Y);
            Position bottomRight = new Position(TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].TopLeft.X, TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].TopLeft.Y);
            for (int y = bottomRight.Y; y <= topLeft.Y; y++)
                for (int x = topLeft.X; x <= bottomRight.X; x++)
                {
                    Know(new Position(x,y));
                }
        }


        /// <summary>
        /// Calculate the current section coordinates.
        /// </summary>
        internal Position GetCurrentSectionCoordinates(Position p)
        {
            return new Position(p.X / Config.SectW, p.Y / Config.SectH);
        }

        /// <summary>
        /// Calculate the current section coordinates *if* the current section contains a room and the given position is in that room.
        /// </summary>
        internal Position GetCurrentSection(Position p)
        {
            var s = GetCurrentSectionCoordinates(p);

            if (!TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].Exists ||
                p.X < TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].TopLeft.X ||
                p.X > TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].BottomRight.X ||
                p.Y < TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].TopLeft.Y ||
                p.Y > TheComplex.s[TheComplex.DungeonLevel, s.X, s.Y].BottomRight.Y)
            {
                return Position.Empty;
            }
            return s;
        }

        /// <summary>
        /// Completely redraw the map.  
        /// Take also care of the visible panel area. 
        /// </summary>
        /// <remarks>it's important that 'map_cursor' is not called in this function since 'map_cursor' scrolls the screen if this is necessary.  Scrolling the screen entails a call to 'paint_map' and you'Complex have an endless loop.</remarks>
        internal void PaintMap()
        {
            /* Paint the map line by line. */
            for (int y = TheComplex.PanelPos.Y * Config.SectH; y < TheComplex.PanelPos.Y * Config.SectH + Config.VmapH; y++)
            {
                Terminal.Cursor(0, 1 + y - TheComplex.PanelPos.Y * Config.SectH);
                int x;
                for (x = TheComplex.PanelPos.X * Config.SectW; x < TheComplex.PanelPos.X * Config.SectW + Config.VmapW; x++)
                {
                    PrintTileAtPosition(new Position(x, y));
                }
            }

            /* Update the screen. */
            Terminal.Update();
        }

        /// <summary>
        /// Set a color determined by the type of tile to be printed.
        /// </summary>
        /// <param name="tile"></param>
        private static void set_color_for_tile(char tile)
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
        internal void MapCursor(Position p)
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
                xp = p.X - TheComplex.PanelPos.X * Config.SectW;
                yp = p.Y - TheComplex.PanelPos.Y * Config.SectH + 1;

                /* Check scrolling to the right. */
                if (yp < 1)
                {
                    TheComplex.PanelPos = TheComplex.PanelPos.North();
                    change = true;
                }
                /* Check scrolling to the left. */
                else if (yp >= Config.VmapH)
                {
                    TheComplex.PanelPos = TheComplex.PanelPos.South();
                    change = true;
                }
                /* Check scrolling downwards. */
                if (xp < 1)
                {
                    TheComplex.PanelPos = TheComplex.PanelPos.West();
                    change = true;
                }
                /* Check scrolling upwards. */
                else if (xp >= Config.VmapW)
                {
                    TheComplex.PanelPos = TheComplex.PanelPos.East();
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
        internal void ChangeDoor(Position p, char door)
        {
            byte i;

            var coordinates = GetCurrentSectionCoordinates(p);

            for (i = 0; i < 4; i++)
            {
                var section = TheComplex.s[TheComplex.DungeonLevel, coordinates.X, coordinates.Y];
                if (!section.Doors[i].Equals(p)) continue;
                section.dt[i] = (byte)door;
                Map[p.X, p.Y] = (byte)door;
                TheComplex.Known[TheComplex.DungeonLevel, p.X, p.Y] = false;
                Know(p);
            }
        }


    }
}