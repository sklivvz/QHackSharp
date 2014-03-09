using System;

namespace HackSharp
{
    /// <summary>
    /// The structure for a basic monster.
    /// </summary>
    internal class MonsterDefinition
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

        /// <summary>
        /// Determine the frequency for a given monster.
        /// </summary>
        /// <param name="dungeonLevel">The dungeon level.</param>
        /// <param name="pMidx">The application midx.</param>
        /// <returns>System.Int32.</returns>
        /// <remarks>This value is level-dependent.  If the monster is out-of-depth (for QHack this means 'has a lower minimum level than the current dungeon level) it's frequency will be reduced.</remarks>
        public int EffectiveRarity(int dungeonLevel, int pMidx)
        {
            // whatever this means...
            return Math.Max(1, ((int)Rarity * Lmod[Math.Min(13, dungeonLevel - (pMidx < 4 ? 0 : (pMidx - 2) / 2))]) / 100);
        }

        /// <summary>
        /// Return an initial hitpoint number for a monster of a given type.
        /// </summary>
        /// <returns></returns>
        public int RollHp()
        {
            return Misc.Dice(Hits);
        }

        private static readonly int[] Lmod =
        {
            100, 90, 80, 72, 64, 56, 50, 42, 35, 28, 20, 12, 4, 1
        };

        public static readonly MonsterDefinition[] Manual =
        {
            new MonsterDefinition
            {
                Symbol = 'k',
                Color = ConsoleColor.Green,
                Name = "kobold",
                ArmorClass = 14,
                Hits = "1d4",
                Attacks = 1,
                ToHit = 0,
                Damage = "1d6",
                Rarity = MonsterRarity.Common
            },
            new MonsterDefinition
            {
                Symbol = 'r',
                Color = ConsoleColor.DarkYellow,
                Name = "rat",
                ArmorClass = 12,
                Hits = "1d3",
                Attacks = 1,
                ToHit = 0,
                Damage = "1d3",
                Rarity = MonsterRarity.Common
            },
            new MonsterDefinition
            {
                Symbol = 'g',
                Color = ConsoleColor.Cyan,
                Name = "goblin",
                ArmorClass = 13,
                Hits = "1d8",
                Attacks = 1,
                ToHit = 0,
                Damage = "1d6",
                Rarity = MonsterRarity.Common
            },
            new MonsterDefinition
            {
                Symbol = 'x',
                Color = ConsoleColor.Yellow,
                Name = "lightning bug",
                ArmorClass = 18,
                Hits = "2d3",
                Attacks = 1,
                ToHit = 1,
                Damage = "1d4",
                Rarity = MonsterRarity.Rare
            }
        };
    }
}