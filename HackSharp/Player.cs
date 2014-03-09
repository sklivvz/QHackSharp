/*                               -*- Mode: C -*- 
 * player.h -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Mon Jan  6 11:42:10 1997
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 21:20:45 1997
 * Update Count    : 19
 * Status          : Unknown, Use with caution!
 *
 * (C) Copyright 1996, 1997 by Thomas Biskup.
 * All Rights Reserved.
 *
 * This software may be distributed only for educational, research and
 * other non-proft purposes provided that this copyright notice is left
 * intact.  If you derive a new game from these sources you also are
 * required to give credit to Thomas Biskup for creating them in the first
 * place.  These sources must not be distributed for any fees in excess of
 * $3 (as of January, 1997).
 */

using System;

namespace HackSharp
{
    /// <summary>
    /// The global data structure for the player character.
    /// </summary>
    public class Player
    {
        internal  int MaxSkills = TskillS.Length;
        private const int MaxAttribute = 6;

        private static readonly string[] TskillS =
            {
                "Strength", "Intelligence", "Dexterity", "Toughness", "Mana",
                "Hitpoints", "Magical Power", "To-Hit Bonus", "To-Damage Bonus",
                "Searching"
            };

        /* Name. */

        /* Attribute scores and maximum attribute scores ever reached. */
        private readonly byte[] _attributes = new byte[MaxAttribute];
        private readonly byte[] _maxAttributes = new byte[MaxAttribute];
        private readonly int[] _tskillExp = new int[TskillS.Length];

        /* Training adjustment. */
        private readonly int[] _tskillTraining = new int[TskillS.Length];
        private int _experience;
        private Game _game;

        /* Hitpoints (current and maximum). */
        private int _hits;
        private int _maxHits;

        /* Magical power. */
        private int _maxPower;
        private string _name;
        private int _power;

        /* Experience scores. */

        /* Searching skill. */
        private byte _searching;

        /* Combat related stuff. */
        private int _toDamage;
        private int toHit;


        /* Update the player status line? */

        public Player()
        {
            UpdateNecessary = true;
        }

        /* String constants for the training skills. */

        public bool UpdateNecessary { get; set; }

        /// <summary>
        /// Set up all the data for the player.
        /// </summary>
        internal void InitPlayer(Game game)
        {
            if (game == null) throw new ArgumentNullException("game");

            _game = game;
            int i;

            /* Initial attributes. */
            for (i = 0; i < (int) Attributes.MaxAttribute; i++)
                SetAttribute((Attributes) i, Misc.Dice("6d3"));

            /* Initial hitpoints. */
            _hits = _maxHits = (GetAttribute(Attributes.Toughness) +
                                                                      (GetAttribute(Attributes.Strength) >> 1) +
                                                                      Misc.Dice("1d6"));

            /* Initial magical power. */
            _power = _maxPower = (GetAttribute(Attributes.Mana) +
                                                                        (GetAttribute(Attributes.Intelligence) >> 2) +
                                                                        Misc.Dice("1d6"));

            /* Initial experience. */
            _experience = 0;
            for (i = 0; i < MaxSkills; i++)
                _tskillExp[i] = 0;

            /* The number of training units initially used. */
            for (i = (int) TrainingSkills.T_MANA + 1; i < MaxSkills; i++)
                _tskillTraining[i] = Config.Tunits/(MaxSkills - (int) TrainingSkills.T_MANA - 1);

            /* Searching skill. */
            _searching = (byte) (GetAttribute(Attributes.Intelligence) +
                                                    (GetAttribute(Attributes.Mana)/5));

            /* Combat bonusses. */
            toHit = _toDamage = 0;

            /* Default name. */
            _name = "brak";
        }


        /// <summary>
        /// Set a PC attribute.
        /// </summary>
        /// <param name="theAttribute"></param>
        /// <param name="value"></param>
        private void SetAttribute(Attributes theAttribute, int value)
        {
            _attributes[(int) theAttribute] = _maxAttributes[(int) theAttribute] = (byte) value;
        }

        /// <summary>
        /// Get the effective value of an attribute.
        /// </summary>
        /// <param name="theAttribute"></param>
        /// <returns></returns>
        private byte GetAttribute(Attributes theAttribute)
        {
            return _attributes[(int) theAttribute];
        }

        /// <summary>
        /// Draw the status line.
        /// </summary>
        internal void UpdatePlayerStatus()
        {
            if (UpdateNecessary)
            {
                Terminal.Cursor(0, 24);
                Terminal.SetColor(ConsoleColor.Gray);
                Terminal.PrintString(
                    "{0}   St:{1}  In:{2}  Dx:{3}  To:{4}  Ma:{5}  H:{6}({7})  P:{8}({9})  X:{10}"
                    , _name
                    , (int) _attributes[(int) Attributes.Strength]
                    , (int) _attributes[(int) Attributes.Intelligence]
                    , (int) _attributes[(int) Attributes.Dexterity]
                    , (int) _attributes[(int) Attributes.Toughness]
                    , (int) _attributes[(int) Attributes.Mana]
                    , _hits
                    , _maxHits
                    , _power
                    , _maxPower
                    , (long) _experience);
                Terminal.ClearToEol();

                UpdateNecessary = false;
            }
        }

        /// <summary>
        /// This function provides the main menu for adjusting the available training levels.  Everything important happens here.
        /// </summary>
        internal void AdjustTraining()
        {
            char c;
            int i;
            int expLength;
            int unitLength;
            int pos = 0; /* Initial menu position. */

            /*
             * Determine the maximum training skill length.  This could be a hard-coded
             * constants but by doing this dynamically it's a lot simpler and less
             * error-prone to change the specific training skills.
             *
             * In the same run we count the number of training units spent.
             */
            int length = expLength = unitLength = 0;
            int remainingUnits = Config.Tunits;
            for (i = 0; i < MaxSkills; i++)
            {
                int b = (TskillS[i]).Length;
                length = Math.Max(length, b);
                int b1 = (_tskillExp[i]/Config.Tunits).ToString().Length;
                expLength = Math.Max(expLength, b1);
                int b2 = (RequiredExp((TrainingSkills) i)/Config.Tunits).ToString().Length;
                unitLength = Math.Max(unitLength, b2);
                remainingUnits -= _tskillTraining[i];
            }

            /* Main loop.  Draw the menu and react on commands. */
            bool doRedraw = true;

            do
            {
                /* Draw the menu. */
                if (doRedraw)
                {
                    Terminal.SetColor(ConsoleColor.Gray);

                    int trainingLength = 0;
                    for (i = 0; i < MaxSkills; i++)
                    {
                        int b = _tskillTraining[i].ToString().Length;
                        trainingLength = Math.Max(trainingLength, b);
                    }
                    for (i = 0; i < MaxSkills; i++)
                    {
                        Terminal.Cursor(3, i);
                        
                        Terminal.PrintString("    {0}: {1} of {2} [{3}]: {4}"
                                        , TskillS[i].PadLeft(length)
                                        , ((long) _tskillExp[i] / Config.Tunits).ToString().PadLeft(expLength)
                                        , (RequiredExp((TrainingSkills)i) / Config.Tunits).ToString().PadLeft(unitLength)
                                        , (_tskillTraining[i]).ToString().PadLeft(trainingLength)
                                        , CurrentLevel((TrainingSkills) i));
                    }
                    Terminal.Cursor(0, 24);
                    Terminal.PrintString(" [iI] Up -- [kK] Down -- [jJ] Decrease -- [lL] Increase -- Units: {0}", remainingUnits);
                    Terminal.ClearToEol();
                    doRedraw = false;
                }

                Terminal.Cursor(4, pos);
                Terminal.PrintString("->");
                Terminal.Update();
                c = Terminal.GetKey();
                Terminal.Cursor(4, pos);
                Terminal.PrintString("  ");
                Terminal.Update();

                switch (c)
                {
                    case 'L':
                        if (remainingUnits > 0)
                        {
                            _tskillTraining[pos] += remainingUnits;
                            remainingUnits = 0;
                            doRedraw = true;
                        }
                        break;

                    case 'l':
                        if (remainingUnits > 0)
                        {
                            remainingUnits--;
                            _tskillTraining[pos]++;
                            doRedraw = true;
                        }
                        break;

                    case 'J':
                        if (_tskillTraining[pos] > 0)
                        {
                            remainingUnits += _tskillTraining[pos];
                            _tskillTraining[pos] = 0;
                            doRedraw = true;
                        }
                        break;

                    case 'j':
                        if (_tskillTraining[pos] > 0)
                        {
                            _tskillTraining[pos]--;
                            remainingUnits++;
                            doRedraw = true;
                        }
                        break;

                    case 'I':
                        pos = 0;
                        break;

                    case 'i':
                        if (pos > 0)
                            pos--;
                        else
                            pos = MaxSkills - 1;
                        break;

                    case 'K':
                        pos = MaxSkills - 1;
                        break;

                    case 'k':
                        if (pos < MaxSkills - 1)
                            pos++;
                        else
                            pos = 0;
                        break;
                }
            } while (c != 27 && c != 'Q' && c != 32);

            /* Clean up. */
            _game.Redraw();
        }

        /// <summary>
        /// Determine the required amount of experience for a specific skill.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private int RequiredExp(TrainingSkills i)
        {
            switch (i)
            {
                case TrainingSkills.T_STRENGTH:
                    return (_attributes[(int) Attributes.Strength] + 1)*65*Config.Tunits;

                case TrainingSkills.T_INTELLIGENCE:
                    return (_attributes[(int) Attributes.Intelligence] + 1)*60*Config.Tunits;

                case TrainingSkills.T_DEXTERITY:
                    return (_attributes[(int) Attributes.Dexterity] + 1)*60*Config.Tunits;

                case TrainingSkills.T_TOUGHNESS:
                    return (_attributes[(int) Attributes.Toughness] + 1)*60*Config.Tunits;

                case TrainingSkills.T_MANA:
                    return (_attributes[(int) Attributes.Mana] + 1)*55*Config.Tunits;

                case TrainingSkills.T_HITS:
                    return (_maxHits + 1)*Config.Tunits;

                case TrainingSkills.T_POWER:
                    return (_maxPower + 1)*Config.Tunits;

                case TrainingSkills.T_2HIT:
                    return (((toHit + 1)*(toHit + 2)) >> 1)*5*Config.Tunits;

                case TrainingSkills.T_2DAMAGE:
                    return (((_toDamage + 1)*(_toDamage + 2)) >> 1)
                           *25*Config.Tunits;

                case TrainingSkills.T_SEARCHING:
                    return (_searching + 1)*Config.Tunits;
            }

            return 0;
        }

        /// <summary>
        /// Determine the current value of a training skill.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private int CurrentLevel(TrainingSkills i)
        {
            switch (i)
            {
                case TrainingSkills.T_STRENGTH:
                case TrainingSkills.T_INTELLIGENCE:
                case TrainingSkills.T_DEXTERITY:
                case TrainingSkills.T_TOUGHNESS:
                case TrainingSkills.T_MANA:
                    return _attributes[(int) i];

                case TrainingSkills.T_HITS:
                    return _maxHits;

                case TrainingSkills.T_POWER:
                    return _maxPower;

                case TrainingSkills.T_2HIT:
                    return toHit;

                case TrainingSkills.T_2DAMAGE:
                    return _toDamage;

                case TrainingSkills.T_SEARCHING:
                    return _searching;
            }

            return 0;
        }

        /// <summary>
        /// Increase a specified training skill by +1.
        /// </summary>
        /// <param name="i"></param>
        private void IncreaseTrainingSkill(TrainingSkills i)
        {
            switch (i)
            {
                case TrainingSkills.T_STRENGTH:
                case TrainingSkills.T_INTELLIGENCE:
                case TrainingSkills.T_DEXTERITY:
                case TrainingSkills.T_TOUGHNESS:
                case TrainingSkills.T_MANA:
                    _attributes[(int) i]++;
                    break;

                case TrainingSkills.T_HITS:
                    _maxHits++;
                    _hits++;
                    break;

                case TrainingSkills.T_POWER:
                    _maxPower++;
                    _power++;
                    break;

                case TrainingSkills.T_2HIT:
                    toHit++;
                    break;

                case TrainingSkills.T_2DAMAGE:
                    _toDamage++;
                    break;

                case TrainingSkills.T_SEARCHING:
                    _searching++;
                    break;
            }
            UpdateNecessary = true;
        }

        /// <summary>
        /// Score a specified number of experience points.
        /// </summary>
        /// <param name="x"></param>
        internal void ScoreExp(int x)
        {
            int i;

            /* Overall adjustment. */
            _experience += x;

            /* Divided adjustment. */
            for (i = 0; i < MaxSkills; i++)
            {
                _tskillExp[i] += (x*_tskillTraining[i]);

                /* Check advancement. */
                while (_tskillExp[i] >= RequiredExp((TrainingSkills) i))
                {
                    _tskillExp[i] -= RequiredExp((TrainingSkills) i);
                    IncreaseTrainingSkill((TrainingSkills) i);
                    Misc.Message("Your {0} increases to {1}.", TskillS[i], CurrentLevel((TrainingSkills) i));
                }
            }

            /* Update the changes. */
            UpdateNecessary = true;
        }
    }
}