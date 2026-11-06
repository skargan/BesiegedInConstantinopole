// Campaign state only. The first entry fixes the hostage; revisiting cannot reroll it.
public static class BisevoDiscovery
{
    public const int Ransom = 5000;
    public const string EncounterKey = "iBisevoEncounter";
    public const string SecretKey = "iBisevoSecret";
    // 0 unvisited, 1 envoy held, 2 ship held, 3 paid, 4 envoy lost, 5 ship lost.
    public static int State => DailyLifeEvents.I(EncounterKey);
    public static bool Pending => State == 1 || State == 2;
    public static bool Discovered => DailyLifeEvents.I(SecretKey) != 0;
    static bool Eligible => VariableSingleton.IsInitialized && !SaveGames.Terminal && !AssaultPractice.Active;
    public static bool CanEnter => Eligible && (Pending || DailyLifeEvents.I("iShips") > 0);
    public static bool Enter()
    {
        if(!CanEnter) return false;
        if(State != 0) return true;
        bool envoy = CountryDiplomacy.AvailableEnvoys > 0;
        if(envoy) VariableSingleton.ChangeInt("iEnvoys", -1);
        VariableSingleton.SetInt(EncounterKey, envoy ? 1 : 2);
        SaveGames.Checkpoint();
        return true;
    }
    public static bool Resolve(bool pay)
    {
        if(!Eligible || !Pending || (pay && DailyLifeEvents.I("iMoney") < Ransom)) return false;
        bool envoy = State == 1;
        // Commit the terminal state before notifying presentation or saving.
        VariableSingleton.SetInt(EncounterKey, pay ? 3 : envoy ? 4 : 5);
        if(pay) {
            VariableSingleton.ChangeInt("iMoney", -Ransom);
            if(envoy) VariableSingleton.ChangeInt("iEnvoys", 1);
        } else if(!envoy) {
            // Captured, not sunk: do not award the sinking system extra crew deaths.
            VariableSingleton.SetInt("iShips", System.Math.Max(0, DailyLifeEvents.I("iShips") - 1));
        }
        DecisionJournal.Record("bisevo_pirates", "Pirates at Saint Sylvester",
            pay ? "Paid 5,000 gold to the pirates sheltering in the abandoned monastery. Our party was released."
            : envoy ? "Refused the pirates' ransom. They killed our envoy."
            : "Refused the pirates' ransom. They seized the warship that brought our party to Biševo.", true);
        SaveGames.Checkpoint();
        return true;
    }
    public static bool Discover()
    {
        if(!Eligible || Discovered) return false;
        VariableSingleton.SetInt(SecretKey, 1);
        VariableSingleton.ChangeInt("iSecrets", 1);
        DecisionJournal.Record("bisevo_sanctuary", "Flowers for the old gods",
            "Fresh flowers lay beneath an ancient statue on Biševo. Someone still tends this secluded sanctuary. One secret gained.", true);
        SaveGames.Checkpoint();
        return true;
    }
}
