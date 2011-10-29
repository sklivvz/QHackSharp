using System;

namespace HackSharp
{
    /// <summary>
    /// The structure for a basic monster.
    /// </summary>
    class monster_def
    {
        /* The character symbol. */
        internal char symbol;

        /* The color of the monster. */
        internal ConsoleColor color;

        /* Its name. */
        internal string name;

        /* Armor class. */
        internal int ac;

        /* Initial hitpoints. */
        internal string hits;

        /* Number of attacks. */
        internal int attacks;

        /* To-hit bonus. */
        internal int to_hit;

        /* Damage dice. */
        internal string damage;

        /* Frequency for the basic level. */
        internal monster_rarity rarity;
    };
}