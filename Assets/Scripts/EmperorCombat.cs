using UnityEngine;
using Yarn.Unity;

// Assassin encounters and explicit, encounter-local frontline choices remain active.
// Persistent fencing progression and queued palace wall orders are retired.
public static class EmperorCombat
{
    // Preserve the original new-campaign balance without reading legacy iFencing.
    public const int PersonalContribution = 3;
    public const int AssassinDuelSuccessChance = 47;

    public static void PrepareAssault(PhasedAssault battle)
    {
        ConsumeWallPresence();
        battle.Frontline = false;
        battle.PersonalBudget = PersonalContribution;
    }

    // Compatibility entry points for old scene callbacks; none grants an advantage,
    // trains a skill, spends health, or schedules participation in a future assault.
    public static int WallKills => 0;
    public static void ConsumeWallPresence() => VariableSingleton.SetInt("iEmperorAtWall", 0);
    public static bool Train(out string report)
    {
        report = "Personal training is unavailable.";
        return false;
    }
    public static string ToggleWall()
    {
        ConsumeWallPresence();
        return "Choose your command position when an assault begins.";
    }
    // Old compiled Yarn programs can still resolve the binding; no current text
    // presents this constant as a player statistic or progression system.
    [YarnFunction("Fencing")] public static int Fencing() => 25;

    [YarnCommand("FightAssassin")]
    public static void Fight(int style)
    {
        int damage;
        bool win;
        if (style == 0)
        {
            win = Random.Range(0,100) < AssassinDuelSuccessChance;
            damage = win ? Random.Range(4,15) : Random.Range(23,44);
        }
        else
        {
            win = true;
            damage = Random.Range(10,26);
        }
        VariableSingleton.SetInt("iCombatAssassinWon", win ? 1 : 0);
        VariableSingleton.SetInt("iCombatLastDamage", damage);
        VariableSingleton.ChangePercentFloat("fHealth", -damage/100f);
        if (VariableSingleton.GetFloatVariable("fHealth") <= 0)
            VariableSingleton.SetInt("iCombatAssassinationFatal", 1);
    }
    [YarnFunction("AssassinDamage")] public static int Damage() => DailyLifeEvents.I("iCombatLastDamage");
}
