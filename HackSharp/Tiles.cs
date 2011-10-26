namespace HackSharp
{
    /// <summary>
    /// Tiles for the dungeon.
    /// </summary>
    public static class Tiles
    {
        public static readonly byte NO_DOOR = 255;
        public static readonly char OPEN_DOOR = '/';
        public static readonly char CLOSED_DOOR = '+';
        public static readonly char LOCKED_DOOR = '\\';
        public static readonly char ROCK = '#';
        public static readonly char FLOOR = '.';
        public static readonly char STAIR_DOWN = '>';
        public static readonly char STAIR_UP = '<';
    }
}