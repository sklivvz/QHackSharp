/*                               -*- Mode: C -*- 
 * misc.c -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Mon Dec 30 17:21:49 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 21:18:23 1997
 * Update Count    : 26
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
using System.Text.RegularExpressions;

namespace HackSharp
{
    internal class Misc
    {
        /* Is the message buffer currently used? */
        private static bool mbuffer_full;

        /* What's the current x position in the message buffer? */
        private static int mbuffer_x;

        /// <summary>
        /// Display a message in the message line.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        /// <remarks>The color will be reset to light gray and the message buffer will be cleared if it was full.</remarks>
        internal static void message(string format, params object[] values)
        {

            string buffer = string.Format(format, values);

            /* Clear the message buffer if necessary. */
            if (mbuffer_full)
                more();

            /* Position the cursor. */
            Terminal.cursor(0, 0);

            /* Reset the color. */
            Terminal.set_color(ConsoleColor.Gray);

            /* Display the message. */
            Terminal.prtstr(buffer);

            /* Update the screen. */
            Terminal.update();

            /* Note the new message in the buffer. */
            mbuffer_full = true;
            mbuffer_x = buffer.Length + 1;
        }


        /// <summary>
        /// A simple convenience function for typical PC-related messages.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        internal static void you(string format, params object[] values)
        {
            message("You " + format, values);
        }

        /// <summary>
        /// Display a (more) prompt at the appropriate position in the message buffer and aftwards clear the message buffer.
        /// </summary>
        private static void more()
        {
            Terminal.cursor(mbuffer_x, 0);
            Terminal.set_color(ConsoleColor.White);
            Terminal.prtstr("(more)");
            while (Terminal.getkey() != ' ') ;
            clear_messages();
        }

        /// <summary>
        /// Clear the message buffer.
        /// </summary>
        internal static void clear_messages()
        {
            Terminal.cursor(0, 0);
            Terminal.clear_to_eol();
            mbuffer_full = false;
            mbuffer_x = 0;
        }

        /// <summary>
        /// Get a target position starting from a base position at (xp, yp).
        /// </summary>
        /// <param name="xp"></param>
        /// <param name="yp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal static void get_target(int xp, int yp, out int x, out int y)
        {
            char c;

            x = xp;
            y = yp;

            message("Which direction? ");
            c = Terminal.getkey();
            clear_messages();

            switch (c)
            {
                case 'i':
                    (y)--;
                    break;

                case 'j':
                    (x)--;
                    break;

                case 'k':
                    (y)++;
                    break;

                case 'l':
                    (x)++;
                    break;

                default:
                    (x) = (y) = -1;
                    break;
            }
        }

        /// <summary>
        /// Extract a number from a string in dice notation.  Dice may have up to 127 sides.
        /// </summary>
        /// <param name="dice"></param>
        /// <returns></returns>
        internal static int dice(string dice)
        {
            int roll = 0;
            int sides;

            int amount;
            int bonus = 0;
            dice = dice.Replace(" ", "");
            Match match = Regex.Match(dice, @"^(\d+)d(\d+)(([\+\-])(\d+)){0,1}$");
            if (!match.Success)
                Error.die("Invalid dice format.");

            int.TryParse(match.Groups[1].Value, out amount);
            int.TryParse(match.Groups[2].Value, out sides);
            if (match.Groups.Count > 2)
                int.TryParse(match.Groups[3].Value, out bonus);

            for (int i = 0; i < amount; i++)
                roll += rand_byte(sides) + 1;

            return roll + bonus;
        }

        /// <summary>
        /// Return the absolute value of a variable.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        internal static int iabs(int x)
        {
            return Math.Abs(x);
        }

        /// <summary>
        /// Return the maximum of two given values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static int imax(int a, int b)
        {
            return ((a > b) ? a : b);
        }

        /// <summary>
        /// Return the minimum of two given values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static int imin(int a, int b)
        {
            return ((a < b) ? a : b);
        }
    }
}