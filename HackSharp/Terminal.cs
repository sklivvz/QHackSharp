/*                               -*- Mode: C -*- 
 * sysdep.c -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Sun Dec 29 22:29:49 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 22:20:44 1997
 * Update Count    : 36
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
using System.Drawing;

namespace HackSharp
{
    internal static class Terminal
    {
        /// <summary>
        /// Set the cursor to a specified scren position.
        /// </summary>
        /// <param name="xp"></param>
        /// <param name="yp"></param>
        /// <remarks>Start at position (0, 0).</remarks>
        public static void cursor(int xp, int yp)
        {
            Console.SetCursorPosition(xp, yp);
        }

        public static void set_color(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        /// <summary>
        /// Print one character.
        /// </summary>
        /// <param name="c"></param>
        public static void prtchar(char c)
        {
            Console.Write(c);
        }

        /// <summary>
        /// Read one character from the keyboard without echoing it.
        /// </summary>
        /// <returns></returns>
        public static char getkey()
        {
            return Console.ReadKey(true).KeyChar;
        }

        /// <summary>
        /// Clear the screen.
        /// </summary>
        /// <remarks>The cursor is expected to be at position (0, 0) afterwards.</remarks>
        public static void clear_screen()
        {
            Console.Clear();
        }

        /// <summary>
        /// Clear the current line up to its end without moving the cursor.
        /// </summary>
        public static void clear_to_eol()
        {
            //meh, will do if needed
        }

        /// <summary>
        /// Print a string to the screen.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        public static void prtstr(string format, params object[] values)
        {
            Console.Write(format, values);
        }

        /// <summary>
        /// Return a random 64-bit number.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static long rand_long(long max)
        {
            return (long)random.NextDouble() * max;
        }

        /// <summary>
        /// Return a random 8-bit number.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static byte rand_byte(byte max)
        {
            return (byte)random.Next(max);
        }

        /// <summary>
        /// Return a random integer.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int rand_int(int max)
        {
            return random.Next(max);
        }

        private static Random random;

        /// <summary>
        /// Initialize the random number generator.
        /// </summary>
        public static void init_rand()
        {
            random = new Random();
        }

        /// <summary>
        /// Initialize the system-dependent I/O stuff.
        /// </summary>
        public static void init_io()
        {
            // Nothing to do
        }

        /// <summary>
        /// Clean up the /IO stuff.
        /// </summary>
        public static void clean_up_io()
        {
            set_color(ConsoleColor.Gray);
            clear_screen();
        }

        /// <summary>
        /// Update the screen.
        /// </summary>
        public static void update()
        {
            //Do nothing.
        }
    }
}