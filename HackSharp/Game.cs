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
 * other non-profit purposes provided that this copyright notice is left
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
            _dungeon = dungeon;
            _d = dungeon.TheComplex;
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
            _dungeon.BuildMap();
            _monsters.CreatePopulation();
            _monsters.BuildMonsterMap();

            _d.Play();

            /*
            * Standard stuff.
            */

            /* Setup the screen. */
            Terminal.ClearScreen();

            do
            {
                /* Print all the new things. */
                UpdateScreen(_d.PlayerPos);
                _dungeon.TheComplex.ThePlayer.UpdatePlayerStatus();

                /* Display the player and center the cursor. */
                _dungeon.MapCursor(_d.PlayerPos);
                Terminal.SetColor(ConsoleColor.White);
                Terminal.PrintChar('@');
                _dungeon.MapCursor(_d.PlayerPos);

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
                var oldPosition = _d.PlayerPos;

                /* Act depending on the last key received. */
                switch (c)
                {
                    case 'T':
                        _dungeon.TheComplex.ThePlayer.AdjustTraining();
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
                        Redraw();
                        break;

                    case 'H':
                        ActivateWalkMode();
                        Try(Direction.W);
                        break;

                    case 'J':
                        ActivateWalkMode();
                        Try(Direction.S);
                        break;

                    case 'L':
                        ActivateWalkMode();
                        Try(Direction.E);
                        break;

                    case 'K':
                        ActivateWalkMode();
                        Try(Direction.N);
                        break;

                    case 'h':
                    {
                        SafeMove(_d.PlayerPos.West());
                        break;
                    }

                    case 'l':
                    {
                        SafeMove(_d.PlayerPos.East());
                        break;
                    }

                    case 'k':
                    {
                        SafeMove(_d.PlayerPos.North());
                        break;
                    }

                    case 'j':
                    {
                        SafeMove(_d.PlayerPos.South());
                        break;
                    }

                }

                _d.OldPlayerPos = oldPosition;

                /* Remove the player character from the screen. */
                _dungeon.PrintTile(oldPosition);
            } while (c != 'Q');
        }

        private bool SafeMove(Position position)
        {
            if (_dungeon.IsOpen(position))
            {
                _d.PlayerPos = position;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update the screen based upon the current player position.  Panel scrolling is also handled in this function.
        /// </summary>
        /// <param name="p">The position.</param>
        private void UpdateScreen(Position p)
        {
            /* Find the current general section. */
            Position p1 = Level.GetCurrentSectionCoordinates(_d.PlayerPos);

            /* Memorize the old panel view. */
            var oldPos = _d.PanelPos;

            /* Adjust the panel view. */
            while (_d.PanelPos.X >= p1.X)
                _d.PanelPos = _d.PanelPos.West();
            while (_d.PanelPos.X + 4 < p1.X)
                _d.PanelPos = _d.PanelPos.East();
            while (_d.PanelPos.Y >= p1.Y)
                _d.PanelPos = _d.PanelPos.North();
            while (_d.PanelPos.Y + 1 < p1.Y)
                _d.PanelPos = _d.PanelPos.South();

            /* Repaint the whole screen map if necessary. */
            if (!oldPos.Equals(_d.PanelPos))
                _dungeon.PaintMap();

            /* Make the immediate surroundings known. */
            for (int px = p.X - 1; px <= p.X + 1; px++)
                for (int py = p.Y - 1; py <= p.Y + 1; py++)
                {
                    _dungeon.Know(new Position(px, py));
                }

            /* Check whether the PC is in a room or not. */
            var currentSection = _d.GetPlayerSection();

            /* Make rooms known. */
            if (!currentSection.Equals(Position.Empty))
            {
                _dungeon.KnowSection(_d.CurrentLevel,currentSection);
            }
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
            var orgSection = _d.GetPlayerSection();

            /* 
            * Check whether running should be stopped.
            */

            var north = _d.PlayerPos.North();
            var west = _d.PlayerPos.West();
            var east = _d.PlayerPos.East();
            var south = _d.PlayerPos.South();
            if (_walkSteps > 0 || _walkInRoom)
            {
                /* Count the possible ways. */
                int cn = (_dungeon.MightBeOpen(north) && (_d.PlayerPos.Y - 1 != _d.OldPlayerPos.Y)) ? 1 : 0;
                int cs = (_dungeon.MightBeOpen(south) && (_d.PlayerPos.Y + 1 != _d.OldPlayerPos.Y)) ? 1 : 0;
                int cw = (_dungeon.MightBeOpen(west) && (_d.PlayerPos.X - 1 != _d.OldPlayerPos.X)) ? 1 : 0;
                int ce = (_dungeon.MightBeOpen(east) && (_d.PlayerPos.X + 1 != _d.OldPlayerPos.X)) ? 1 : 0;

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
                    _walkMode &= (orgSection.X != -1 && orgSection.Y != -1);

                    /* Check for special features. */
                    if (_walkSteps > 0)
                    {
                        Position p = _d.PlayerPos;
                        _walkMode = _walkMode && ((char)Dungeon.Map[p.X, p.Y] == Tiles.Floor);
                    }
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
                    if (_dungeon.IsOpen(north))
                        _d.PlayerPos = north;
                    else if (_dungeon.IsOpen(west) && _d.PlayerPos.X - 1 != _d.OldPlayerPos.X)
                        _d.PlayerPos = west;
                    else if (_dungeon.IsOpen(east) && _d.PlayerPos.X + 1 != _d.OldPlayerPos.X)
                        _d.PlayerPos = east;
                    else
                        _walkMode = false;
                    break;

                case Direction.S:
                    if (_dungeon.IsOpen(south))
                        _d.PlayerPos= south;
                    else if (_dungeon.IsOpen(west) && _d.PlayerPos.X - 1 != _d.OldPlayerPos.X)
                        _d.PlayerPos = west;
                    else if (_dungeon.IsOpen(east) && _d.PlayerPos.X + 1 != _d.OldPlayerPos.X)
                        _d.PlayerPos = east;
                    else
                        _walkMode = false;
                    break;

                case Direction.E:
                    if (_dungeon.IsOpen(east))
                        _d.PlayerPos = east;
                    else if (_dungeon.IsOpen(south) && _d.PlayerPos.Y + 1 != _d.OldPlayerPos.Y)
                        _d.PlayerPos = south;
                    else if (_dungeon.IsOpen(north) && _d.PlayerPos.Y - 1 != _d.OldPlayerPos.Y)
                        _d.PlayerPos = north;
                    else
                        _walkMode = false;
                    break;

                case Direction.W:
                    if (_dungeon.IsOpen(west))
                        _d.PlayerPos = west;
                    else if (_dungeon.IsOpen(south) && _d.PlayerPos.Y + 1 != _d.OldPlayerPos.Y)
                        _d.PlayerPos = south;
                    else if (_dungeon.IsOpen(north) && _d.PlayerPos.Y - 1 != _d.OldPlayerPos.Y)
                        _d.PlayerPos = north;
                    else
                        _walkMode = false;
                    break;
            }

            /* Find the new section. */
            var playerSection = _dungeon.TheComplex.GetPlayerSection();

            /* Entering/leaving a room will deactivate walk-mode. */
            if (_walkSteps > 0)
                _walkMode &= (orgSection.X == playerSection.X && orgSection.Y == playerSection.Y);

            /* Increase the number of steps actually walked. */
            _walkSteps++;
        }

        /// <summary>
        /// Redraw the whole screen.
        /// </summary>
        internal void Redraw()
        {
            Misc.clear_messages();
            _dungeon.PaintMap();
            _dungeon.TheComplex.ThePlayer.UpdateNecessary = true;
            _dungeon.TheComplex.ThePlayer.UpdatePlayerStatus();
            Terminal.Update();
        }

        /// <summary>
        /// Switch between dungeon levels.
        /// </summary>
        private void InitLevel()
        {
            /* Build the current dungeon map from the general description. */
            _dungeon.BuildMap();

            /* Determine monster frequencies for the current dungeon level. */
            _monsters.InitializeMonsters();

            /*
            * If a level is entered for the first time a new monster population
            * will be generated and the player receives a little bit of experience
            * for going where nobody went before (or at least managed to come back).
            */
            if (!_d.CurrentLevel.Visited)
            {
                _monsters.CreatePopulation();
                _d.CurrentLevel.Visited = true;

                /* Score some experience for exploring unknown depths. */
                _d.ThePlayer.ScoreExp(_d.DungeonLevel);
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
            Position p = _d.PlayerPos;
            if ((char)Dungeon.Map[p.X, p.Y] != Tiles.StairDown)
                Misc.You("don't see any stairs leading downwards.");
            else
            {
                _d.DescendLevel();
                InitLevel();
            }
        }

        /// <summary>
        /// Continue one level upwards.
        /// </summary>
        private void AscendLevel()
        {
            Position p = _d.PlayerPos;
            if ((char)Dungeon.Map[p.X, p.Y] != Tiles.StairUp)
                Misc.You("don't see any stairs leading upwards.");
            else
            {
                _d.AscendLevel();
                if (_d.DungeonLevel > 0)
                {
                    InitLevel();
                }
            }
        }


        /// <summary>
        /// Handle doors.
        /// </summary>
        private void OpenDoor()
        {
            /* Find the door. */
            var target = Misc.GetTarget(_d.PlayerPos);

            /* Command aborted? */
            if (target.Equals(Position.Empty))
                return;

            /* Check the door. */
            Position p = target;
            switch ((char)Dungeon.Map[p.X, p.Y])
            {
                case Tiles.OpenDoor:
                    Misc.Message("This door is already open.");
                    break;

                case Tiles.ClosedDoor:
                    Misc.You("open the door.");
                    _dungeon.ChangeDoor(target,Tiles.OpenDoor);
                    break;

                case Tiles.LockedDoor:
                    Misc.Message("This door seems to be locked.");
                    break;

                default:
                    Misc.Message("Which door?");
                    break;
            }
        }

        /// <summary>
        /// Activate the walk mode and determine whether we are walking through a room.
        /// </summary>
        private void ActivateWalkMode()
        {
            /* Activate walking. */
            _walkMode = true;

            /* Check for a room. */
            _walkInRoom = !_dungeon.TheComplex.GetPlayerSection().Equals(Position.Empty);
        }
    }
}