namespace HackSharp
{
    /// <summary>
    /// Constants for the training skills used by QHack.
    /// </summary>
    /// <remarks>The first five ones always should be the attribute training skills. 
    /// Changing this could break several code parts in the game (especially in 'init_player').</remarks>
    enum tskills
    {
        T_STRENGTH, T_INTELLIGENCE, T_DEXTERITY, T_TOUGHNESS, T_MANA,
        T_HITS, T_POWER, T_2HIT, T_2DAMAGE, T_SEARCHING, MAX_T_SKILL
    };
}