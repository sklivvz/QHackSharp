using System;

namespace HackSharp
{
    /// <summary>
    /// The structure for a basic monster.
    /// </summary>
    class MonsterDefinition
    {
        /* The character symbol. */
        public char Symbol { get; set; }

        /* The color of the monster. */
        internal ConsoleColor Color { get; set; }

        /* Its name. */
        internal string Name { get; set; }

        /* Armor class. */
        internal int ArmorClass { get; set; }

        /* Initial hitpoints. */
        internal string Hits { get; set; }

        /* Number of attacks. */
        internal int Attacks { get; set; }

        /* To-hit bonus. */
        internal int ToHit { get; set; }

        /* Damage dice. */
        internal string Damage { get; set; }

        /* Frequency for the basic level. */
        internal MonsterRarity Rarity { get; set; }
    };
}