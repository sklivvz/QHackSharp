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

            stdprtstr("Setting up the game...");

            /* Initialize everything. */
            stdprtstr(".");
            Terminal.init_rand();
            stdprtstr(".");
            var player = new Player();
            var dungeon = new Dungeon();
            var monsters = new Monsters();
            var game = new Game(dungeon, monsters);
            monsters.InitMonsters(dungeon);
            stdprtstr(".");
            dungeon.InitDungeon(monsters, player);
            stdprtstr(".");
            player.InitPlayer(game);
            stdprtstr(".");
            Terminal.init_io();
            InitScreen();

            /* Play the game. */
            game.play();

            /* Clean up. */
            Terminal.clean_up_io();

            /* Be done. */
            return;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                Console.ReadLine();
            }

        }

        private static void stdprtstr(string message)
        {
            Console.Write(message);
        }


        /// <summary>
        /// Draw a title screen.
        /// </summary>
        private static void InitScreen()
        {
            Terminal.clear_screen();
            string s = String.Format("-----====<<<<< QHack {0}.{1} >>>>>====-----", Config.MAJOR_VERSION, Config.MINOR_VERSION);
            Terminal.cursor((80 - s.Length) >> 1, 3);
            Terminal.prtstr("{0}", s);
            Terminal.cursor(16, 5);
            Terminal.prtstr("(The Quickest Roguelike Gaming Hack on the Net)");
            Terminal.cursor(19, 8);
            Terminal.prtstr("(C) Copyright 1996, 1997 by Thomas Biskup.");
            Terminal.cursor(30, 9);
            Terminal.prtstr("All Rights Reserved.");
            Terminal.cursor(0, 24);
            Terminal.prtstr("Email comments/suggestions/bug reports to ............... rpg@saranxis.ruhr.de");
            Terminal.getkey();
        }
    }
}