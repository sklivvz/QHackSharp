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

namespace HackSharp
{
    internal class Game
    {
        private readonly Dungeon _dungeon;
        private readonly Monsters _monsters;
        private readonly Complex _d;
        private bool _walkInRoom;
        private bool _walkMode;
        private int _walkSteps;

        public Game(Dungeon dungeon, Monsters monsters)
        {
            if (dungeon == null) throw new ArgumentNullException("dungeon");
            if (monsters == null) throw new ArgumentNullException("monsters");

            _dungeon = dungeon;
            _d = dungeon.Complex;
            _monsters = monsters;
        }

        /// <summary>
        /// The main function.
        /// </summary>
        internal void Play()
        {
            char c = '\0';

            /*
            * Build the current level.
            *
            * XXXX: Once it is possible to save/restore the map this no longer
            *       must be done if the game was started by restoring a save file.
            */
            _d.DungeonLevel = 0;
            _dungeon.BuildMap();
            _monsters.CreatePopulation();
            _monsters.BuildMonsterMap();
            _d.Visited[0] = true;

            /* Initial player position. */
            _d.PlayerX = _d.OldPlayerX = _d.StairsUpX[0];
            _d.PlayerY = _d.OldPlayerY = _d.StairsUpY[0];

            /* Initial panel position. */
            _d.psx = _d.psy = 0;

            /*
            * Standard stuff.
            */

            /* Setup the screen. */
            Terminal.ClearScreen();

            do
            {
                /* Print all the new things. */
                UpdateScreen(_d.PlayerX, _d.PlayerY);
                _dungeon.Complex.ThePlayer.UpdatePlayerStatus();

                /* Display the player and center the cursor. */
                _dungeon.MapCursor(_d.PlayerX, _d.PlayerY);
                Terminal.SetColor(ConsoleColor.White);
                Terminal.PrintChar('@');
                _dungeon.MapCursor(_d.PlayerX, _d.PlayerY);

                /* Refresh the IO stuff. */
                Terminal.Update();

                /* Continue to walk or read a key. */
                if (!_walkMode)
                {
                    _walkSteps = 0;
                    c = Terminal.GetKey();
                }

                _monsters.MoveMonsters();

                /* The message line should be cleared in any case. */
                Misc.clear_messages();

                /* Memorize the old PC position. */
                int opx = _d.PlayerX;
                int opy = _d.PlayerY;

                /* Act depending on the last key received. */
                switch (c)
                {
                    case 'T':
                        _dungeon.Complex.ThePlayer.AdjustTraining();
                        break;

                    case 'o':
                        OpenDoor();
                        break;

                    case '>':
                        DescendLevel();
                        break;

                    case '<':
                        AscendLevel();
                        /* Quit if necessary. */
                        if (_d.DungeonLevel == -1)
                            c = 'Q';
                        break;

                    case 'R':
                        redraw();
                        break;

                    case 'J':
                        ActivateWalkMode();
                        Try(Direction.W);
                        break;

                    case 'K':
                        ActivateWalkMode();
                        Try(Direction.S);
                        break;

                    case 'L':
                        ActivateWalkMode();
                        Try(Direction.E);
                        break;

                    case 'I':
                        ActivateWalkMode();
                        Try(Direction.N);
                        break;

                    case 'j':
                        if (_dungeon.IsOpen(_d.PlayerX - 1, _d.PlayerY))
                            _d.PlayerX--;
                        break;

                    case 'l':
                        if (_dungeon.IsOpen(_d.PlayerX + 1, _d.PlayerY))
                            _d.PlayerX++;
                        break;

                    case 'i':
                        if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY - 1))
                            _d.PlayerY--;
                        break;

                    case 'k':
                        if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY + 1))
                            _d.PlayerY++;
                        break;

                }

                _d.OldPlayerX = opx;
                _d.OldPlayerY = opy;

                /* Remove the player character from the screen. */
                _dungeon.PrintTile(opx, opy);
            } while (c != 'Q');
        }


        /// <summary>
        /// Update the screen based upon the current player position.  Panel scrolling is also handled in this function.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void UpdateScreen(int x, int y)
        {
            int sx;
            int sy;
            int px;
            int py;

            /* Find the current general section. */
            _dungeon.GetCurrentSectionCoordinates(_d.PlayerX, _d.PlayerY, out sx, out sy);

            /* Memorize the old panel view. */
            int opsx = _d.psx;
            int opsy = _d.psy;

            /* Adjust the panel view. */
            while (sx < _d.psx)
                _d.psx--;
            while (_d.psx + 4 < sx)
                _d.psx++;
            while (sy < _d.psy)
                _d.psy--;
            while (_d.psy + 1 < sy)
                _d.psy++;

            /* Repaint the whole screen map if necessary. */
            if (opsx != _d.psx || opsy != _d.psy)
                _dungeon.PaintMap();

            /* Make the immediate surroundings known. */
            for (px = x - 1; px <= x + 1; px++)
                for (py = y - 1; py <= y + 1; py++)
                    _dungeon.Know(px, py);

            /* Check whether the PC is in a room or not. */
            _dungeon.GetCurrentSection(_d.PlayerX, _d.PlayerY, out sx, out sy);

            /* Make rooms known. */
            if (sx != -1 && sy != -1)
                _dungeon.KnowSection(sx, sy);
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

            _dungeon.GetCurrentSection(_d.PlayerX, _d.PlayerY, out sx1, out sy1);

            /* 
            * Check whether running should be stopped.
            */

            if (_walkSteps > 0 || _walkInRoom)
            {
                /* Count the possible ways. */
                int cn = (_dungeon.MightBeOpen(_d.PlayerX, _d.PlayerY - 1) && (_d.PlayerY - 1 != _d.OldPlayerY)) ? 1 : 0;
                int cs = (_dungeon.MightBeOpen(_d.PlayerX, _d.PlayerY + 1) && (_d.PlayerY + 1 != _d.OldPlayerY)) ? 1 : 0;
                int cw = (_dungeon.MightBeOpen(_d.PlayerX - 1, _d.PlayerY) && (_d.PlayerX - 1 != _d.OldPlayerX)) ? 1 : 0;
                int ce = (_dungeon.MightBeOpen(_d.PlayerX + 1, _d.PlayerY) && (_d.PlayerX + 1 != _d.OldPlayerX)) ? 1 : 0;

                /* Check... */
                if (_walkInRoom)
                {
                    /* In rooms we simply check the general look of the surroundings. */
                    int oldCn = 0;
                    int oldCw = 0;
                    int oldCe = 0;
                    int oldCs = 0;
                    switch (_walkSteps)
                    {
                        case 0:
                            /* One free step in any case. */
                            break;

                        default:
                            /* Check the surroundings. */
                            _walkMode &= (cn == oldCn && cs == oldCs && cw == oldCw && ce == oldCe);
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
                    _walkMode &= (sx1 != -1 && sy1 != -1);

                    /* Check for special features. */
                    if (_walkSteps > 0)
                        _walkMode = _walkMode && (_dungeon.TileAt(_d.PlayerX, _d.PlayerY) == Tiles.Floor);
                }
                else
                    /* Check for intersections. */
                    switch (dir)
                    {
                        case Direction.N:
                            _walkMode &= (cw + ce < 2 && cn + cw < 2 && cn + ce < 2);
                            break;
                        case Direction.S:
                            _walkMode &= (cw + ce < 2 && cs + cw < 2 && cs + ce < 2);
                            break;
                        case Direction.W:
                            _walkMode &= (cn + cs < 2 && cw + cs < 2 && cw + cn < 2);
                            break;
                        case Direction.E:
                            _walkMode &= (cn + cs < 2 && ce + cs < 2 && ce + cn < 2);
                            break;
                    }

                if (!_walkMode)
                    return;
            }

            /*
            * Walk.  This function also manages to walk around corners in a tunnel.
            */

            switch (dir)
            {
                case Direction.N:
                    if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY - 1))
                        _d.PlayerY--;
                    else if (_dungeon.IsOpen(_d.PlayerX - 1, _d.PlayerY) && _d.PlayerX - 1 != _d.OldPlayerX)
                        _d.PlayerX--;
                    else if (_dungeon.IsOpen(_d.PlayerX + 1, _d.PlayerY) && _d.PlayerX + 1 != _d.OldPlayerX)
                        _d.PlayerX++;
                    else
                        _walkMode = false;
                    break;

                case Direction.S:
                    if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY + 1))
                        _d.PlayerY++;
                    else if (_dungeon.IsOpen(_d.PlayerX - 1, _d.PlayerY) && _d.PlayerX - 1 != _d.OldPlayerX)
                        _d.PlayerX--;
                    else if (_dungeon.IsOpen(_d.PlayerX + 1, _d.PlayerY) && _d.PlayerX + 1 != _d.OldPlayerX)
                        _d.PlayerX++;
                    else
                        _walkMode = false;
                    break;

                case Direction.E:
                    if (_dungeon.IsOpen(_d.PlayerX + 1, _d.PlayerY))
                        _d.PlayerX++;
                    else if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY + 1) && _d.PlayerY + 1 != _d.OldPlayerY)
                        _d.PlayerY++;
                    else if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY - 1) && _d.PlayerY - 1 != _d.OldPlayerY)
                        _d.PlayerY--;
                    else
                        _walkMode = false;
                    break;

                case Direction.W:
                    if (_dungeon.IsOpen(_d.PlayerX - 1, _d.PlayerY))
                        _d.PlayerX--;
                    else if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY + 1) && _d.PlayerY + 1 != _d.OldPlayerY)
                        _d.PlayerY++;
                    else if (_dungeon.IsOpen(_d.PlayerX, _d.PlayerY - 1) && _d.PlayerY - 1 != _d.OldPlayerY)
                        _d.PlayerY--;
                    else
                        _walkMode = false;
                    break;
            }

            /* Find the new section. */
            _dungeon.GetCurrentSection(_d.PlayerX, _d.PlayerY, out sx2, out sy2);

            /* Entering/leaving a room will deactivate walk-mode. */
            if (_walkSteps > 0)
                _walkMode &= (sx1 == sx2 && sy1 == sy2);

            /* Increase the number of steps actually walked. */
            _walkSteps++;
        }

        /// <summary>
        /// Redraw the whole screen.
        /// </summary>
        internal void redraw()
        {
            Misc.clear_messages();
            _dungeon.PaintMap();
            _dungeon.Complex.ThePlayer.UpdateNecessary = true;
            _dungeon.Complex.ThePlayer.UpdatePlayerStatus();
            Terminal.Update();
        }

        /// <summary>
        /// Switch between dungeon levels.
        /// </summary>
        /// <param name="mod"></param>
        private void ModifyDungeonLevel(int mod)
        {
            /* Modify the actual dungeon level. */
            _d.DungeonLevel += mod;

            /* Build the current dungeon map from the general description. */
            _dungeon.BuildMap();

            /* Determine monster frequencies for the current dungeon level. */
            _monsters.InitializeMonsters();

            /*
            * If a level is entered for the first time a new monster population
            * will be generated and the player receives a little bit of experience
            * for going where nobody went before (or at least managed to come back).
            */
            if (!_d.Visited[_d.DungeonLevel])
            {
                _monsters.CreatePopulation();
                _d.Visited[_d.DungeonLevel] = true;

                /* Score some experience for exploring unknown depths. */
                _dungeon.Complex.ThePlayer.ScoreExp(_d.DungeonLevel);
            }

            /* Place monsters in the appropriate positions. */
            _monsters.BuildMonsterMap();

            /* Paint the new map. */
            _dungeon.PaintMap();
        }

        /// <summary>
        /// Continue one level downwards.
        /// </summary>
        private void DescendLevel()
        {
            if (_dungeon.TileAt(_d.PlayerX, _d.PlayerY) != Tiles.StairDown)
                Misc.you("don't see any stairs leading downwards.");
            else
            {
                ModifyDungeonLevel(1);
                _d.PlayerX = _d.StairsUpX[_d.DungeonLevel];
                _d.PlayerY = _d.StairsUpY[_d.DungeonLevel];
            }
        }

        /// <summary>
        /// Continue one level upwards.
        /// </summary>
        private void AscendLevel()
        {
            if (_dungeon.TileAt(_d.PlayerX, _d.PlayerY) != Tiles.StairUp)
                Misc.you("don't see any stairs leading upwards.");
            else
            {
                if (_d.DungeonLevel > 0)
                {
                    ModifyDungeonLevel(-1);
                    _d.PlayerX = _d.StairsDownX[_d.DungeonLevel];
                    _d.PlayerY = _d.StairsDownY[_d.DungeonLevel];
                }
                else
                    /* Leave the dungeon. */
                    _d.DungeonLevel = -1;
            }
        }


        /// <summary>
        /// Handle doors.
        /// </summary>
        private void OpenDoor()
        {
            int tx, ty;

            /* Find the door. */
            Misc.get_target(_d.PlayerX, _d.PlayerY, out tx, out ty);

            /* Command aborted? */
            if (tx == -1 || ty == -1)
                return;

            /* Check the door. */
            switch (_dungeon.TileAt(tx, ty))
            {
                case Tiles.OpenDoor:
                    Misc.message("This door is already open.");
                    break;

                case Tiles.ClosedDoor:
                    Misc.you("open the door.");
                    _dungeon.ChangeDoor(tx, ty, Tiles.OpenDoor);
                    break;

                case Tiles.LockedDoor:
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
        private void ActivateWalkMode()
        {
            int x, y;

            /* Activate walking. */
            _walkMode = true;

            /* Check for a room. */
            _dungeon.GetCurrentSection(_d.PlayerX, _d.PlayerY, out x, out y);
            _walkInRoom = (x != -1 && y != -1);
        }
    }
}