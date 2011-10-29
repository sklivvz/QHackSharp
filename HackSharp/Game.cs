/*                               -*- Mode: C -*- 
 * game.h -- 
 * ITIID           : $ITI$ $Header $__Header$
 * Author          : Thomas Biskup
 * Created On      : Mon Dec 30 00:11:37 1996
 * Last Modified By: Thomas Biskup
 * Last Modified On: Thu Jan  9 19:19:39 1997
 * Update Count    : 6
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
    internal class Game
    {
        private bool walk_in_room;
        private bool walk_mode;
        private int walk_steps;

        private readonly DungeonComplex d;
        private readonly Dungeon _dungeon;
        private Monsters _monsters;

        public Game(Dungeon dungeon, Monsters monsters)
        {
            if (dungeon == null) throw new ArgumentNullException("dungeon");
            if (monsters == null) throw new ArgumentNullException("monsters");

            _dungeon = dungeon;
            d = dungeon.d;
            _monsters = monsters;
        }

        /// <summary>
        /// The main function.
        /// </summary>
        internal void play()
        {
            char c = '\0';

            /*
            * Build the current level.
            *
            * XXXX: Once it is possible to save/restore the map this no longer
            *       must be done if the game was started by restoring a save file.
            */
            d.dl = 0;
            _dungeon.build_map();
            _monsters.create_population();
            _monsters.build_monster_map();
            d.visited[0] = true;

            /* Initial player position. */
            d.px = d.opx = d.stxu[0];
            d.py = d.opy = d.styu[0];

            /* Initial panel position. */
            d.psx = d.psy = 0;

            /*
            * Standard stuff.
            */

            /* Setup the screen. */
            Terminal.clear_screen();

            do
            {
                /* Print all the new things. */
                update_screen(d.px, d.py);
                update_player_status();

                /* Display the player and center the cursor. */
                map_cursor(d.px, d.py);
                Terminal.set_color(ConsoleColor.White);
                Terminal.prtchar('@');
                map_cursor(d.px, d.py);

                /* Refresh the IO stuff. */
                Terminal.update();

                /* Continue to walk or read a key. */
                if (!walk_mode)
                {
                    walk_steps = 0;
                    c = Terminal.getkey();
                }

                /* The message line should be cleared in any case. */
                Misc.clear_messages();

                /* Memorize the old PC position. */
                int opx = d.px;
                int opy = d.py;

                /* Act depending on the last key received. */
                switch (c)
                {
                    case 'T':
                        adjust_training();
                        break;

                    case 'o':
                        open_door();
                        break;

                    case '>':
                        descend_level();
                        break;

                    case '<':
                        ascend_level();
                        /* Quit if necessary. */
                        if (d.dl == -1)
                            c = 'Q';
                        break;

                    case 'R':
                        redraw();
                        break;

                    case 'J':
                        activate_walk_mode();
                        Try(Direction.W);
                        break;

                    case 'K':
                        activate_walk_mode();
                        Try(Direction.S);
                        break;

                    case 'L':
                        activate_walk_mode();
                        Try(Direction.E);
                        break;

                    case 'I':
                        activate_walk_mode();
                        Try(Direction.N);
                        break;

                    case 'j':
                        if (_dungeon.is_open(d.px - 1, d.py))
                            d.px--;
                        break;

                    case 'l':
                        if (_dungeon.is_open(d.px + 1, d.py))
                            d.px++;
                        break;

                    case 'i':
                        if (_dungeon.is_open(d.px, d.py - 1))
                            d.py--;
                        break;

                    case 'k':
                        if (_dungeon.is_open(d.px, d.py + 1))
                            d.py++;
                        break;

                    default:
                        break;
                }

                d.opx = opx;
                d.opy = opy;

                /* Remove the player character from the screen. */
                _dungeon.print_tile(opx, opy);
            } while (c != 'Q');
        }


        /// <summary>
        /// Update the screen based upon the current player position.  Panel scrolling is also handled in this function.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void update_screen(int x, int y)
        {
            int sx, sy, px, py, opsx, opsy;

            /* Find the current general section. */
            _dungeon.get_current_section_coordinates(d.px, d.py, out sx, out sy);

            /* Memorize the old panel view. */
            opsx = d.psx;
            opsy = d.psy;

            /* Adjust the panel view. */
            while (sx < d.psx)
                d.psx--;
            while (d.psx + 4 < sx)
                d.psx++;
            while (sy < d.psy)
                d.psy--;
            while (d.psy + 1 < sy)
                d.psy++;

            /* Repaint the whole screen map if necessary. */
            if (opsx != d.psx || opsy != d.psy)
                _dungeon.paint_map();

            /* Make the immediate surroundings known. */
            for (px = x - 1; px <= x + 1; px++)
                for (py = y - 1; py <= y + 1; py++)
                    _dungeon.know(px, py);

            /* Check whether the PC is in a room or not. */
            _dungeon.get_current_section(d.px, d.py, out sx, out sy);

            /* Make rooms known. */
            if (sx != -1 && sy != -1)
                _dungeon.know_section(sx, sy);
        }

        /// <summary>
        /// Try to walk into a given direction.  This is used by the walk-mode when the player tries to run into a given direction.
        /// </summary>
        /// <param name="dir"></param>
        /// <remarks>Walk-mode will be deactivated if...
        ///...the surroundings change in a major way (more or less obstacles around the player compared to the last step).
        ///...an intersection is reached in a tunnel.
        ///...a room is entered or left.</remarks>
        private void Try(Direction dir)
        {
            int sx1;
            int sy1;
            int sx2;
            int sy2;

            _dungeon.get_current_section(d.px, d.py, out sx1, out sy1);

            /* 
            * Check whether running should be stopped.
            */

            if (walk_steps>0 || walk_in_room)
            {
                /* Count the possible ways. */
                int cn = (_dungeon.might_be_open(d.px, d.py - 1) && (d.py - 1 != d.opy)) ? 1 : 0;
                int cs = (_dungeon.might_be_open(d.px, d.py + 1) && (d.py + 1 != d.opy)) ? 1 : 0;
                int cw = (_dungeon.might_be_open(d.px - 1, d.py) && (d.px - 1 != d.opx)) ? 1 : 0;
                int ce = (_dungeon.might_be_open(d.px + 1, d.py) && (d.px + 1 != d.opx)) ? 1 : 0;

                /* Check... */
                if (walk_in_room)
                {
                    /* In rooms we simply check the general look of the surroundings. */
                    int oldCn = 0;
                    int oldCw = 0;
                    int oldCe = 0;
                    int oldCs = 0;
                    switch (walk_steps)
                    {
                        case 0:
                            /* One free step in any case. */
                            break;

                        default:
                            /* Check the surroundings. */
                            walk_mode &= (cn == oldCn && cs == oldCs && cw == oldCw && ce == oldCe);
                            /* Memorize the surroundings. */
                            oldCn = cn;
                            oldCs = cs;
                            oldCw = cw;
                            oldCe = ce;
                            break;

                        case 1:
                            /* Memorize the surroundings. */
                            oldCn = cn;
                            oldCs = cs;
                            oldCw = cw;
                            oldCe = ce;
                            break;
                    }

                    /* Check whether we are still in a room. */
                    walk_mode &= (sx1 != -1 && sy1 != -1);

                    /* Check for special features. */
                    if (walk_steps > 0)
                        walk_mode = walk_mode && (_dungeon.tile_at(d.px, d.py) == Tiles.FLOOR);
                }
                else
                    /* Check for intersections. */
                    switch (dir)
                    {
                        case Direction.N:
                            walk_mode &= (cw + ce < 2 && cn + cw < 2 && cn + ce < 2);
                            break;
                        case Direction.S:
                            walk_mode &= (cw + ce < 2 && cs + cw < 2 && cs + ce < 2);
                            break;
                        case Direction.W:
                            walk_mode &= (cn + cs < 2 && cw + cs < 2 && cw + cn < 2);
                            break;
                        case Direction.E:
                            walk_mode &= (cn + cs < 2 && ce + cs < 2 && ce + cn < 2);
                            break;
                    }

                if (!walk_mode)
                    return;
            }

            /*
            * Walk.  This function also manages to walk around corners in a tunnel.
            */

            switch (dir)
            {
                case Direction.N:
                    if (_dungeon.is_open(d.px, d.py - 1))
                        d.py--;
                    else if (_dungeon.is_open(d.px - 1, d.py) && d.px - 1 != d.opx)
                        d.px--;
                    else if (_dungeon.is_open(d.px + 1, d.py) && d.px + 1 != d.opx)
                        d.px++;
                    else
                        walk_mode = false;
                    break;

                case Direction.S:
                    if (_dungeon.is_open(d.px, d.py + 1))
                        d.py++;
                    else if (_dungeon.is_open(d.px - 1, d.py) && d.px - 1 != d.opx)
                        d.px--;
                    else if (_dungeon.is_open(d.px + 1, d.py) && d.px + 1 != d.opx)
                        d.px++;
                    else
                        walk_mode = false;
                    break;

                case Direction.E:
                    if (_dungeon.is_open(d.px + 1, d.py))
                        d.px++;
                    else if (_dungeon.is_open(d.px, d.py + 1) && d.py + 1 != d.opy)
                        d.py++;
                    else if (_dungeon.is_open(d.px, d.py - 1) && d.py - 1 != d.opy)
                        d.py--;
                    else
                        walk_mode = false;
                    break;

                case Direction.W:
                    if (_dungeon.is_open(d.px - 1, d.py))
                        d.px--;
                    else if (_dungeon.is_open(d.px, d.py + 1) && d.py + 1 != d.opy)
                        d.py++;
                    else if (_dungeon.is_open(d.px, d.py - 1) && d.py - 1 != d.opy)
                        d.py--;
                    else
                        walk_mode = false;
                    break;
            }

            /* Find the new section. */
            _dungeon.get_current_section(d.px, d.py, out sx2, out sy2);

            /* Entering/leaving a room will deactivate walk-mode. */
            if (walk_steps>0)
                walk_mode &= (sx1 == sx2 && sy1 == sy2);

            /* Increase the number of steps actually walked. */
            walk_steps++;
        }

        /// <summary>
        /// Redraw the whole screen.
        /// </summary>
        private void redraw()
        {
            clear_messages();
            _dungeon.paint_map();
            update_necessary = true;
            update_player_status();
            Terminal.update();
        }

        /// <summary>
        /// Switch between dungeon levels.
        /// </summary>
        /// <param name="mod"></param>
        private void modify_dungeon_level(int mod)
        {
            /* Modify the actual dungeon level. */
            d.dl += mod;

            /* Build the current dungeon map from the general description. */
            _dungeon.build_map();

            /* Determine monster frequencies for the current dungeon level. */
            _monsters.initialize_monsters();

            /*
            * If a level is entered for the first time a new monster population
            * will be generated and the player receives a little bit of experience
            * for going where nobody went before (or at least managed to come back).
            */
            if (!d.visited[d.dl])
            {
                _monsters.create_population();
                d.visited[d.dl] = true;

                /* Score some experience for exploring unknown depths. */
                score_exp(d.dl);
            }

            /* Place monsters in the appropriate positions. */
            _monsters.build_monster_map();

            /* Paint the new map. */
            _dungeon.paint_map();
        }

        /// <summary>
        /// Continue one level downwards.
        /// </summary>
        private void descend_level()
        {
            if (_dungeon.tile_at(d.px, d.py) != Tiles.STAIR_DOWN)
                Misc.you("don't see any stairs leading downwards.");
            else
            {
                modify_dungeon_level(+1);
                d.px = d.stxu[d.dl];
                d.py = d.styu[d.dl];
            }
        }

        /// <summary>
        /// Continue one level upwards.
        /// </summary>
        private void ascend_level()
        {
            if (_dungeon.tile_at(d.px, d.py) != Tiles.STAIR_UP)
                Misc.you("don't see any stairs leading upwards.");
            else
            {
                if (d.dl >0)
                {
                    modify_dungeon_level(-1);
                    d.px = d.stxd[d.dl];
                    d.py = d.styd[d.dl];
                }
                else
                    /* Leave the dungeon. */
                    d.dl = -1;
            }
        }


        /// <summary>
        /// Handle doors.
        /// </summary>
        private void open_door()
        {
            int tx, ty;

            /* Find the door. */
            Misc.get_target(d.px, d.py, out tx, out ty);

            /* Command aborted? */
            if (tx == -1 || ty == -1)
                return;

            /* Check the door. */
            switch (_dungeon.tile_at(tx, ty))
            {
                case Tiles.OPEN_DOOR:
                    Misc.message("This door is already open.");
                    break;

                case Tiles.CLOSED_DOOR:
                    Misc.you("open the door.");
                    _dungeon.change_door(tx, ty, Tiles.OPEN_DOOR);
                    break;

                case Tiles.LOCKED_DOOR:
                    Misc.message("This door seems to be locked.");
                    break;

                default:
                    Misc.message("Which door?");
                    break;
            }
        }

        /// <summary>
        /// Activate the walk mode and determine whether we are walking through a room.
        /// </summary>
        private void activate_walk_mode()
        {
            int x, y;

            /* Activate walking. */
            walk_mode = true;

            /* Check for a room. */
            _dungeon.get_current_section(d.px, d.py, out x, out y);
            walk_in_room = (x != -1 && y != -1);
        }
    }
}