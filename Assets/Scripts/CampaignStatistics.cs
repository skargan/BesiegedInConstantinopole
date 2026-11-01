using System;
using System.Globalization;

// Shared presentation definitions. Values are always read from the campaign, never copied into another state.
public static class CampaignStatistics
{
    public sealed class Stat
    {
        public readonly string Name, ShortName, Key, Meaning;
        public Stat(string name, string shortName, string key, string meaning) { Name=name; ShortName=shortName; Key=key; Meaning=meaning; }
        public string Value => Key=="population" ? Number(PopulationAccounting.People) : Key.StartsWith("f") ? VariableSingleton.GetPercentFloat(Key) : Number(CouncilActions.Int(Key));
    }
    public static string Number(long value) => value.ToString("N0", CultureInfo.InvariantCulture);
    public static readonly Stat[] Header = {
        new Stat("Emperor's health","Health","fHealth","The Emperor's health, not the health of the city's population."),
        new Stat("Gold","Gold","iMoney","Treasury available for purchases, labour and orders."),
        new Stat("Population","People","population","Individual inhabitants. Recruitment and daily food needs are based on this population."),
        new Stat("Materials","Materials","iMaterials","Construction supplies available for repairs and other works."),
        new Stat("Provisions","Food","iProvisions","Stored provisions. Daily consumption and days remaining are listed in City at a Glance."),
        new Stat("Secrets","Secrets","iSecrets","Knowledge and contacts used for research and intelligence operations."),
        new Stat("Archers","Archers","iArchers","Archers currently in service; excludes recruits awaiting deployment."),
        new Stat("Infantry","Infantry","iInfantry","Infantry currently in service; excludes recruits awaiting deployment."),
        new Stat("Cavalry","Cavalry","iCavalry","Cavalry currently in service, including any dismounted for wall defence."),
        new Stat("Varangian Guard","Guard","iVarangians","Varangian guards currently in service."),
        new Stat("Army morale","Army","fTroopRep","The army's willingness to continue the defence."),
        new Stat("Public morale","Public","fPeopleRep","The population's confidence in your rule."),
        new Stat("Clergy support","Clergy","fPriestRep","Support among the clergy. Separate from political faction influence."),
        new Stat("Noble support","Nobles","fNobleRep","Support among the nobility. Separate from political faction influence.")
    };
}
