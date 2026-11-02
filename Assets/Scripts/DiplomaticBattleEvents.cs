using System.Linq;

// Eligibility thresholds are initial playtest defaults; combat resolution is unchanged.
public static class DiplomaticBattleEvents
{
    public const int MajorAssaultAttackers = 1000;
    public const int MajorEnemyFleet = 10;
    public const int MajorEnemyShipsSunk = 5;
    static readonly string[] MaritimeCourts = { "venice", "genoa", "hospitallers", "cyprus", "naples" };
    static readonly string[] SupportingCourts = { "papacy", "hospitallers", "cyprus", "morea" };
    public static int SuccessfulDefenceRewards => DailyLifeEvents.I("iDiplomacyMajorLandDefences");

    public static bool NavalVictory(bool victory, int enemyFleetBefore, int enemyShipsLost)
    {
        if (!victory || enemyFleetBefore < MajorEnemyFleet || enemyShipsLost < MajorEnemyShipsSunk || DailyLifeEvents.I("iDiplomacyMajorNavalVictory") > 0) return false;
        CountryDiplomacy.Ensure();
        foreach (string id in MaritimeCourts) CountryDiplomacy.ApplyEvent(id, "major_naval_victory", 10);
        VariableSingleton.SetInt("iDiplomacyMajorNavalVictory", 1);
        return true;
    }

    public static string HeldLandDefence(int attackers, bool held, bool emperorSurvives)
    {
        if (!VariableSingleton.HasSiegeStarted() || !held || !emperorSurvives || attackers < MajorAssaultAttackers || SuccessfulDefenceRewards >= 2) return "";
        int turnToken = VariableSingleton.Turn + 1;
        if (DailyLifeEvents.I("iDiplomacyLastDefenceTurn") == turnToken) return "";
        CountryDiplomacy.Ensure(); int chapter = SuccessfulDefenceRewards + 1;
        foreach (var country in CountryDiplomacy.Definitions.Where(d => d.WarEligible || SupportingCourts.Contains(d.Id)))
            CountryDiplomacy.ApplyEvent(country.Id, "major_defence_" + chapter, 5);
        VariableSingleton.SetInt("iDiplomacyMajorLandDefences", chapter);
        VariableSingleton.SetInt("iDiplomacyLastDefenceTurn", turnToken);
        return "\n\nNews of the successful defence strengthens our standing with anti-Ottoman courts: +5 goodwill. "
            + (chapter == 1 ? "One further major defence can earn this diplomatic gain." : "Both campaign gains from major defences have now been earned.");
    }
}
