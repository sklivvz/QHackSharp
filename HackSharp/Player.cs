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
        internal const int MaxSkills = 11;
        private const int MaxAttribute = 6;

        /* Name. */
        string name;

        /* Attribute scores and maximum attribute scores ever reached. */
        byte[] attribute = new byte[MaxAttribute];
        byte[] max_attribute = new byte[MaxAttribute];

        /* Hitpoints (current and maximum). */
        int hits;
        int max_hits;

        /* Magical power. */
        int power;
        int max_power;

        /* Experience scores. */
        int experience;
        int[] tskill_exp = new int[MaxSkills];

        /* Training adjustment. */
        int[] tskill_training = new int[MaxSkills];

        /* Searching skill. */
        byte searching;

        /* Combat related stuff. */
        int to_hit;
        int to_damage;


        /* Update the player status line? */
        internal bool update_necessary = true;

        /* String constants for the training skills. */
        static string[] tskill_s =
{
  "Strength", "Intelligence", "Dexterity", "Toughness", "Mana",
  "Hitpoints", "Magical Power", "To-Hit Bonus", "To-Damage Bonus",
  "Searching"
};

        private Dungeon _dungeon;
        private DungeonComplex d;

        /// <summary>
        /// Set up all the data for the player.
        /// </summary>
        internal void init_player(Dungeon dungeon)
        {
            if (dungeon == null) throw new ArgumentNullException("dungeon");
            _dungeon = dungeon;
            d = _dungeon.d;
            int i;

            /* Initial attributes. */
            for (i = 0; i < (int)attributes.MAX_ATTRIBUTE; i++)
                set_attribute((attributes)i, Misc.dice("6d3"));

            /* Initial hitpoints. */
            d.pc.hits = d.pc.max_hits = (get_attribute(attributes.TOUGHNESS) +
                             (get_attribute(attributes.STRENGTH) >> 1) +
                             Misc.dice("1d6"));

            /* Initial magical power. */
            d.pc.power = d.pc.max_power = (get_attribute(attributes.MANA) +
                           (get_attribute(attributes.INTELLIGENCE) >> 2) +
                           Misc.dice("1d6"));

            /* Initial experience. */
            d.pc.experience = 0;
            for (i = 0; i < MaxSkills; i++)
                d.pc.tskill_exp[i] = 0;

            /* The number of training units initially used. */
            for (i = (int)tskills.T_MANA + 1; i < MaxSkills; i++)
                d.pc.tskill_training[i] = Config.TUNITS / (MaxSkills - (int)tskills.T_MANA - 1);

            /* Searching skill. */
            d.pc.searching = (byte) (get_attribute(attributes.INTELLIGENCE) +
                                     (get_attribute(attributes.MANA) / 5));

            /* Combat bonusses. */
            d.pc.to_hit = d.pc.to_damage = 0;

            /* Default name. */
            d.pc.name = "brak";
        }


        /// <summary>
        /// Set a PC attribute.
        /// </summary>
        /// <param name="theAttribute"></param>
        /// <param name="value"></param>
        void set_attribute(attributes theAttribute, int value)
        {
            d.pc.attribute[(int)theAttribute] = d.pc.max_attribute[(int)theAttribute] = (byte) value;
        }

        /// <summary>
        /// Get the effective value of an attribute.
        /// </summary>
        /// <param name="theAttribute"></param>
        /// <returns></returns>
        byte get_attribute(attributes theAttribute)
        {
            return d.pc.attribute[(int)theAttribute];
        }

        /// <summary>
        /// Draw the status line.
        /// </summary>
        internal void update_player_status()
        {
            if (update_necessary)
            {
                Terminal.cursor(0, 24);
                Terminal.set_color(ConsoleColor.Gray);
                Terminal.prtstr("%s   St:%d  In:%d  Dx:%d  To:%d  Ma:%d  H:%d(%d)  P:%d(%d)  X:%ld"
                   , d.pc.name
                   , (int)d.pc.attribute[(int)attributes.STRENGTH]
                   , (int)d.pc.attribute[(int)attributes.INTELLIGENCE]
                   , (int)d.pc.attribute[(int)attributes.DEXTERITY]
                   , (int)d.pc.attribute[(int)attributes.TOUGHNESS]
                   , (int)d.pc.attribute[(int)attributes.MANA]
                   , d.pc.hits
                   , d.pc.max_hits
                   , d.pc.power
                   , d.pc.max_power
                   , (long)d.pc.experience);
                Terminal.clear_to_eol();

                update_necessary = false;
            }
        }

        /// <summary>
        /// This function provides the main menu for adjusting the available training levels.  Everything important happens here.
        /// </summary>
        internal void adjust_training()
{
  char c;
            int i;
            int exp_length;
            int unit_length;
            byte pos = 0;  /* Initial menu position. */
  
  /*
   * Determine the maximum training skill length.  This could be a hard-coded
   * constants but by doing this dynamically it's a lot simpler and less
   * error-prone to change the specific training skills.
   *
   * In the same run we count the number of training units spent.
   */
  int length = exp_length = unit_length = 0;
  int remainingUnits = Config.TUNITS;
  for (i = 0; i < MaxSkills; i++)
  {
    length = Misc.imax(length, (tskill_s[i]).Length);
    exp_length = Misc.imax(exp_length, (d.pc.tskill_exp[i] / Config.TUNITS).ToString().Length);
    unit_length = Misc.imax(unit_length, (required_exp((tskills) i) / Config.TUNITS).ToString().Length);
    remainingUnits -= d.pc.tskill_training[i];
  }

  /* Main loop.  Draw the menu and react on commands. */
  bool doRedraw = true;

  do
  {
    /* Draw the menu. */
    if (doRedraw)
    {
      Terminal.set_color(ConsoleColor.Gray);
      
      int trainingLength = 0;
      for (i = 0; i < MaxSkills; i++)
	trainingLength = Misc.imax(trainingLength, d.pc.tskill_training[i].ToString().Length);
      for (i = 0; i < MaxSkills; i++)
      {
          Terminal.cursor(3, i);
          Terminal.prtstr("    %*s: %*ld of %*ld [%*d]: %d   "
	       , length
	       , tskill_s[i]
	       , exp_length
	       , (long) d.pc.tskill_exp[i] / Config.TUNITS
	       , unit_length
	       , required_exp((tskills) i) / Config.TUNITS
	       , trainingLength
	       , d.pc.tskill_training[i]
	       , current_level((tskills) i));
      }
      Terminal.cursor(0, 24);
      Terminal.prtstr(" [iI] Up -- [kK] Down -- [jJ] Decrease -- [lL] Increase");
      Terminal.prtstr(" -- Units: %d", (int)remainingUnits);
      Terminal.clear_to_eol();
      doRedraw = false;
    }

    Terminal.cursor(4, pos);
    Terminal.prtstr("->");
    Terminal.update();
    c = Terminal.getkey();
    Terminal.cursor(4, pos);
    Terminal.prtstr("  ");
    Terminal.update();
      
    switch (c)
    {
      case 'L':
	if (remainingUnits>0)
	{
	  d.pc.tskill_training[pos] += remainingUnits;
	  remainingUnits = 0;
	  doRedraw = true;
	}
	break;
	
      case 'l':
	if (remainingUnits>0)
	{
	  remainingUnits--;
	  d.pc.tskill_training[pos]++;
	  doRedraw = true;
	}
	break;
	
      case 'J':
	if (d.pc.tskill_training[pos]>0)
	{
	  remainingUnits += d.pc.tskill_training[pos];
	  d.pc.tskill_training[pos] = 0;
	  doRedraw = true;
	}
	break;
	
      case 'j':
	if (d.pc.tskill_training[pos]>0)
	{
	  d.pc.tskill_training[pos]--;
	  remainingUnits++;
	  doRedraw = true;
	}
	break;

      case 'I':
	pos = 0;
	break;
	
      case 'i':
	if (pos>0)
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
  }
  while (c != 27 && c != 'Q' && c != 32);

  /* Clean up. */
  _redraw();
}

        /// <summary>
        /// Determine the required amount of experience for a specific skill.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        int required_exp(tskills i)
        {
            switch (i)
            {
                case tskills.T_STRENGTH:
                    return (d.pc.attribute[(int)attributes.STRENGTH] + 1) * 65 * Config.TUNITS;

                case tskills.T_INTELLIGENCE:
                    return (d.pc.attribute[(int)attributes.INTELLIGENCE] + 1) * 60 * Config.TUNITS;

                case tskills.T_DEXTERITY:
                    return (d.pc.attribute[(int)attributes.DEXTERITY] + 1) * 60 * Config.TUNITS;

                case tskills.T_TOUGHNESS:
                    return (d.pc.attribute[(int)attributes.TOUGHNESS] + 1) * 60 * Config.TUNITS;

                case tskills.T_MANA:
                    return (d.pc.attribute[(int)attributes.MANA] + 1) * 55 * Config.TUNITS;

                case tskills.T_HITS:
                    return (d.pc.max_hits + 1) * Config.TUNITS;

                case tskills.T_POWER:
                    return (d.pc.max_power + 1) * Config.TUNITS;

                case tskills.T_2HIT:
                    return (((d.pc.to_hit + 1) * (d.pc.to_hit + 2)) >> 1) * 5 * Config.TUNITS;

                case tskills.T_2DAMAGE:
                    return (((d.pc.to_damage + 1) * (d.pc.to_damage + 2)) >> 1)
                  * 25 * Config.TUNITS;

                case tskills.T_SEARCHING:
                    return (d.pc.searching + 1) * Config.TUNITS;
            }

            return 0;
        }

        /// <summary>
        /// Determine the current value of a training skill.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        int current_level(tskills i)
        {
            switch (i)
            {
                case tskills.T_STRENGTH:
                case tskills.T_INTELLIGENCE:
                case tskills.T_DEXTERITY:
                case tskills.T_TOUGHNESS:
                case tskills.T_MANA:
                    return d.pc.attribute[(int)i];

                case tskills.T_HITS:
                    return d.pc.max_hits;

                case tskills.T_POWER:
                    return d.pc.max_power;

                case tskills.T_2HIT:
                    return d.pc.to_hit;

                case tskills.T_2DAMAGE:
                    return d.pc.to_damage;

                case tskills.T_SEARCHING:
                    return d.pc.searching;
            }

            return 0;
        }

        /// <summary>
        /// Increase a specified training skill by +1.
        /// </summary>
        /// <param name="i"></param>
        void increase_training_skill(tskills i)
        {
            switch (i)
            {
                case tskills.T_STRENGTH:
                case tskills.T_INTELLIGENCE:
                case tskills.T_DEXTERITY:
                case tskills.T_TOUGHNESS:
                case tskills.T_MANA:
                    d.pc.attribute[(int) i]++;
                    break;

                case tskills.T_HITS:
                    d.pc.max_hits++;
                    d.pc.hits++;
                    break;

                case tskills.T_POWER:
                    d.pc.max_power++;
                    d.pc.power++;
                    break;

                case tskills.T_2HIT:
                    d.pc.to_hit++;
                    break;

                case tskills.T_2DAMAGE:
                    d.pc.to_damage++;
                    break;

                case tskills.T_SEARCHING:
                    d.pc.searching++;
                    break;
            }
            update_necessary = true;
        }

        /// <summary>
        /// Score a specified number of experience points.
        /// </summary>
        /// <param name="x"></param>
        internal void score_exp(int x)
        {
            int i;

            /* Overall adjustment. */
            d.pc.experience += x;

            /* Divided adjustment. */
            for (i = 0; i < MaxSkills; i++)
            {
                d.pc.tskill_exp[i] += (x * d.pc.tskill_training[i]);

                /* Check advancement. */
                while (d.pc.tskill_exp[i] >= required_exp((tskills)i))
                {
                    d.pc.tskill_exp[i] -= required_exp((tskills)i);
                    increase_training_skill((tskills) i);
                    Misc.message("Your {0} increases to {1}.", tskill_s[i], current_level((tskills)i));
                }
            }

            /* Update the changes. */
            update_necessary = true;
        }

    }
}