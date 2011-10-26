namespace HackSharp
{
    /// <summary>
    /// Tiles for the dungeon.
    /// </summary>
    internal static class Tiles
    {
        public static readonly byte  NO_DOOR = 255;
        private static readonly char OPEN_DOOR = '/';
        private static readonly char CLOSED_DOOR = '+';
        private static readonly char LOCKED_DOOR = '\\';
        private static readonly char ROCK = '#';
        private static readonly char FLOOR = '.';
        private static readonly char STAIR_DOWN = '>';
        private static readonly char STAIR_UP = '<';
    }
}