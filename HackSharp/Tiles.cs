namespace HackSharp
{
    /// <summary>
    /// Tiles for the dungeon.
    /// </summary>
    public static class Tiles
    {
        public const byte NO_DOOR = 255;
        public const char OPEN_DOOR = '/';
        public const char CLOSED_DOOR = '+';
        public const char LOCKED_DOOR = '\\';
        public const char ROCK = '#';
        public const char FLOOR = '.';
        public const char STAIR_DOWN = '>';
        public const char STAIR_UP = '<';
    }
}