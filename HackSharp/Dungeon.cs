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
            this.monsters = monsters;
            TheComplex.ThePlayer = player;
            TheComplex.Dig();
        }

        /// <summary>
        /// Build a map for the current dungeon level.
        /// 
        /// This function is very important for QHack in general.  Levels are only
        /// stored by their section descriptions.  The actual map is created when the
        /// level is entered.  The positive thing about this is that it requires much
        /// less space to save a level in this way (since you only need the outline
        /// descriptions).  The negative thing is that tunneling and other additions
        /// are not possible since the level descriptions have now way of recording
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
                    var section = TheComplex.CurrentLevel.Sections[sx, sy];
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
            Map[TheComplex.CurrentLevel.StairsUp.X, TheComplex.CurrentLevel.StairsUp.Y] = (byte)Tiles.StairUp;
            if (!TheComplex.CurrentLevel.IsBottom)
                Map[TheComplex.CurrentLevel.StairsDown.X, TheComplex.CurrentLevel.StairsDown.Y] = (byte)Tiles.StairDown;
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
            var sec1 = TheComplex.CurrentLevel.Sections[sx1, sy1];
            var c1 = sec1.Exists
                ? (dir == Direction.S
                    ? sec1.Doors[(byte) Direction.S]
                    : sec1.Doors[(byte) Direction.E])
                : new Position(
                    sx1*Config.SectW + (Config.SectW/2),
                    sy1*Config.SectH + (Config.SectH/2)
                    );

            /* Get the end byteinates from section #2. */
            var sec2 = TheComplex.CurrentLevel.Sections[sx2, sy2];
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
                    if (Map[x, y] == rock) Map[x, y] = floor;
                    x++;
                }

                /* Part #2. */
                if (y < c2.Y)
                    while (y < c2.Y)
                    {
                        if (Map[x, y] == rock) Map[x, y] = floor;
                        y++;
                    }
                else
                    while (y > c2.Y)
                    {
                        if (Map[x, y] == rock) Map[x, y] = floor;
                        y--;
                    }

                /* Part #3. */
                while (x < c2.X)
                {
                    if (Map[x, y] == rock) Map[x, y] = floor;
                    x++;
                }
                if (Map[x, y] == rock) Map[x, y] = floor;
            }
            else
            {
                /* Part #1. */
                while (y < my)
                {
                    if (Map[x, y] == rock) Map[x, y] = floor;
                    y++;
                }
                if (Map[x, y] == rock) Map[x, y] = floor;

                /* Part #2. */
                if (x < c2.X)
                    while (x < c2.X)
                    {
                        if (Map[x, y] == rock) Map[x, y] = floor;
                        x++;
                    }
                else
                    while (x > c2.X)
                    {
                        if (Map[x, y] == rock) Map[x, y] = floor;
                        x--;
                    }

                /* Part #3. */
                while (y < c2.Y)
                {
                    if (Map[x, y] == rock) Map[x, y] = floor;
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
        public static bool IsDirectionPossible(Position p, Direction direction)
        {
            return ((direction == Direction.N && p.Y > 0) ||
                    (direction == Direction.S && p.Y < Config.NsectH - 1) ||
                    (direction == Direction.W && p.X > 0) ||
                    (direction == Direction.E && p.X < Config.NsectW - 1));
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
            TheComplex.CurrentLevel.Known[p.X, p.Y] = true;
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
            if (!EnsureCoordinates(p) || !TheComplex.CurrentLevel.Known[p.X, p.Y])
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
        /// Makes a complete section known.
        /// 
        /// This function is usually called when a room is entered. 
        /// </summary>
        internal void KnowSection(Level level, Position s)
        {
            var section = level.Sections[s.X, s.Y];
            Position topLeft = new Position(section.BottomRight.X,section.BottomRight.Y);
            Position bottomRight = new Position(section.TopLeft.X, section.TopLeft.Y);
            for (int y = bottomRight.Y; y <= topLeft.Y; y++)
                for (int x = topLeft.X; x <= bottomRight.X; x++)
                {
                    Know(new Position(x,y));
                }
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

            var coordinates = Level.GetCurrentSectionCoordinates(p);

            for (i = 0; i < 4; i++)
            {
                var section = TheComplex.CurrentLevel.Sections[coordinates.X, coordinates.Y];
                if (!section.Doors[i].Equals(p)) continue;
                section.dt[i] = (byte)door;
                Map[p.X, p.Y] = (byte)door;
                TheComplex.CurrentLevel.Known[p.X, p.Y] = false;
                Know(p);
            }
        }


    }
}