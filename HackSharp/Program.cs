/*                               -*- Mode: C -*- 
 * qhack.c -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Sun Dec 29 22:55:15 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 21:25:59 1997
 * Update Count    : 61
 * Status          : Unknown, Use with caution!
 *
 * On the following times I worked on this game:
 *  12/29/96, 12/30/96 [9:45pm-1:30am, 10:35am-11:11am, 1:25pm-2:55pm]
 *                     [5:18pm-6:55pm]
 *  12/31/96           [1:25pm-3:16pm]
 *  01/06/97           [11:28am-11:55am, 10:37pm-10:56pm]
 *  01/08/97           [11:25pm-1:04am]
 *  09/01/97           [6:40pm-10:55pm]
 *
 * Total time so far: 959 minutes = 15 hours, 59 minutes
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
    internal class Program
    {
        /// <summary>
        /// The main function.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            try
            {

            /* Print startup message. */
            Console.WriteLine("\nQuickHack Version 0.1");
            Console.WriteLine("(C) Copyright 1996, 1997 by Thomas Biskup.\n");
            if (args.Length > 1)
                return;

            Terminal.StandardPrintString("Setting up the game...");

            /* Initialize everything. */
            Terminal.StandardPrintString(".");
            Terminal.InitRand();
            Terminal.StandardPrintString(".");
            var player = new Player();
            var dungeon = new Dungeon();
            var monsters = new Monsters();
            var game = new Game(dungeon, monsters);
            monsters.InitMonsters(dungeon);
            Terminal.StandardPrintString(".");
            dungeon.InitDungeon(monsters, player);
            Terminal.StandardPrintString(".");
            player.InitPlayer(game);
            Terminal.StandardPrintString(".");
            InitScreen();

            /* Play the game. */
            game.Play();

            /* Clean up. */
            Terminal.CleanUpIO();

            /* Be done. */
            return;
            }
            catch (Exception ex)
            {
                try
                {
                    Terminal.CleanUpIO();
                }
                catch
                {
                }

                Console.WriteLine(ex);
                Console.ReadLine();
            }

        }




        /// <summary>
        /// Draw a title screen.
        /// </summary>
        private static void InitScreen()
        {
            Terminal.ClearScreen();
            string s = String.Format("-----====<<<<< QHack {0}.{1} >>>>>====-----", Config.MajorVersion, Config.MinorVersion);
            Terminal.Cursor((80 - s.Length) >> 1, 3);
            Terminal.PrintString("{0}", s);
            Terminal.Cursor(16, 5);
            Terminal.PrintString("(The Quickest Roguelike Gaming Hack on the Net)");
            Terminal.Cursor(19, 8);
            Terminal.PrintString("(C) Copyright 1996, 1997 by Thomas Biskup.");
            Terminal.Cursor(30, 9);
            Terminal.PrintString("All Rights Reserved.");
            Terminal.Cursor(0, 24);
            Terminal.PrintString("Email comments/suggestions/bug reports to ............... rpg@saranxis.ruhr.de");
            Terminal.GetKey();
        }
    }
}