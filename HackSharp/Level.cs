namespace HackSharp
{
    public class Level
    {
        public Level()
        {
            Known = new bool[Config.MapW, Config.MapH];
            Sections = new Section[Config.NsectW, Config.NsectH];
            for (int j = 0; j < Config.NsectW; j++)
                for (int k = 0; k < Config.NsectH; k++)
                    Sections[j, k] = new Section();

            /* Nothing is known about the dungeon at this point. */
            for (int j = 0; j < Config.MapW; j++)
                for (int k = 0; k < Config.MapH; k++)
                    Known[j, k] = false;
        }

        public Position StairsDown { get; set; }
        public Position StairsUp { get; set; }
        /* Level was already visited? */
        public bool Visited { get; set; }
        public bool[,] Known { get; set; }
        public Section[,] Sections { get; set; }
        public bool IsBottom { get; set; }
        public bool IsTop { get; set; }

        /// <summary>
        ///     Calculate the room width for a specific room section at p.
        /// </summary>
        /// <returns></returns>
        public int RoomWidth(Position p)
        {
            Section section = Sections[p.X, p.Y];
            return section.BottomRight.X - section.TopLeft.X - 1;
        }

        /// <summary>
        ///     Calculate the room height for a specific room section at p.
        /// </summary>
        /// <returns></returns>
        public int RoomHeight(Position p)
        {
            Section section = Sections[p.X, p.Y];
            return section.BottomRight.Y - section.TopLeft.Y - 1;
        }

        private void DigSection(Position p)
        {
            Section section = Sections[p.X, p.Y];
            if (Terminal.RandByte(100) + 1 >= Dungeon.ExistenceChance)
            {
                /* No room here. */
                section.Exists = false;

                /* Decrease the chance for further empty rooms. */
                Dungeon.ExistenceChance += 3;
            }
            else
            {
                Direction dir;

                /* Yeah :-) ! */
                section.Exists = true;

                /*
                 * Dig a room.
                 *
                 * Rooms are at least 4x4 tiles in size.
                 */

                do
                {
                    section.TopLeft = new Position(p.X*Config.SectW + Terminal.RandByte(3) + 1,
                        p.Y*Config.SectH + Terminal.RandByte(3) + 1);
                    section.BottomRight = new Position((p.X + 1)*Config.SectW + Terminal.RandByte(3) - 2,
                        (p.Y + 1)*Config.SectH + Terminal.RandByte(3) - 2);
                } while (section.BottomRight.X - section.TopLeft.X < 3 || section.BottomRight.Y - section.TopLeft.Y < 3);

                /*
                 * Create doors.
                 *
                 * XXX: At some point it would be nice to create doors only for
                 *      some directions to make the dungeon less regular.
                 */

                for (dir = Direction.N; dir <= Direction.E; dir++)
                {
                    Section curSec = section;
                    if (Dungeon.IsDirectionPossible(p, dir))
                    {
                        switch (dir)
                        {
                            case Direction.N:
                                curSec.Doors[(int) dir] = new Position(
                                    curSec.TopLeft.X + Terminal.RandByte((byte) (RoomWidth(p) - 1)) + 1,
                                    curSec.TopLeft.Y);
                                break;

                            case Direction.S:
                                curSec.Doors[(int) dir] = new Position(
                                    curSec.TopLeft.X + Terminal.RandByte((byte) (RoomWidth(p) - 1)) + 1,
                                    curSec.BottomRight.Y);
                                break;

                            case Direction.E:
                                curSec.Doors[(int) dir] = new Position(
                                    curSec.BottomRight.X,
                                    curSec.TopLeft.Y + Terminal.RandByte((byte) (RoomHeight(p) - 1)) + 1);
                                break;

                            case Direction.W:
                                curSec.Doors[(int) dir] = new Position(
                                    curSec.TopLeft.X,
                                    curSec.TopLeft.Y + Terminal.RandByte((byte) (RoomHeight(p) - 1)) + 1);
                                break;
                        }
                        curSec.dt[(int) dir] = RandDoor();
                    }
                    else
                        curSec.dt[(int) dir] = Tiles.NoDoor;
                }
            }
        }


        /// <summary>
        ///     Determine a random door type.
        /// </summary>
        /// <returns></returns>
        private byte RandDoor()
        {
            byte roll = Terminal.RandByte(100);

            if (roll < 75)
                return (byte) Tiles.OpenDoor;
            if (roll < 90)
                return (byte) Tiles.ClosedDoor;

            return (byte) Tiles.LockedDoor;
        }

        /// <summary>
        ///     Each level requires at least one stair!
        /// </summary>
        private void DigStairs()
        {
            /* Dig stairs upwards. */

            /* Find a section. */
            Position s1 = GetRandomSection();

            Position sectionTopLeft1 = Sections[s1.X, s1.Y].TopLeft;
            StairsUp = new Position(
                (sectionTopLeft1.X + Terminal.RandByte((byte) (RoomWidth(s1) - 1)) + 1),
                (sectionTopLeft1.Y + Terminal.RandByte((byte) (RoomHeight(s1) - 1)) + 1)
                );

            /* Dig stairs downwards. */
            if (IsBottom) return;
            /* Find a section. */
            Position s2 = GetRandomSection();

            Position position;
            /* Find a good location. */
            do
            {
                Position sectionTopLeft2 = Sections[s2.X, s2.Y].TopLeft;
                position = new Position(
                    sectionTopLeft2.X + Terminal.RandByte((byte) (RoomWidth(s2) - 1)) + 1,
                    sectionTopLeft2.Y + Terminal.RandByte((byte) (RoomHeight(s2) - 1)) + 1);
            } while (!IsTop && position.Equals(StairsUp));

            /* Place the stairway. */
            StairsDown = position;
        }

        /// <summary>
        ///     Find a random section on the current dungeon level.
        /// </summary>
        private Position GetRandomSection()
        {
            int sx, sy;
            do
            {
                sx = Terminal.RandInt(Config.NsectW);
                sy = Terminal.RandInt(Config.NsectH);
            } while (!Sections[sx, sy].Exists);
            return new Position(sx, sy);
        }

        /// <summary>
        ///     Create one single dungeon level.
        /// </summary>
        public void Dig()
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
            Dungeon.ExistenceChance = 70;

            /* Dig each section. */
            for (i = 0; i < Config.SectNumber; i++)
            {
                DigSection(new Position(sectx[index[i]], secty[index[i]]));
            }

            /* Build some stairs. */
            DigStairs();
        }

        /// <summary>
        /// Calculate the current section coordinates.
        /// </summary>
        internal static Position GetCurrentSectionCoordinates(Position p)
        {
            return new Position(p.X/Config.SectW, p.Y/Config.SectH);
        }

        /// <summary>
        /// Calculate the current section coordinates *if* the current section contains a room and the given position is in that room.
        /// </summary>
        internal Position GetSection(Position p)
        {
            var s = GetCurrentSectionCoordinates(p);

            var section = Sections[s.X, s.Y];
            return !section.Exists ||
                   p.X < section.TopLeft.X || p.X > section.BottomRight.X ||
                   p.Y < section.TopLeft.Y || p.Y > section.BottomRight.Y
                ? Position.Empty
                : s;
        }
    }
}