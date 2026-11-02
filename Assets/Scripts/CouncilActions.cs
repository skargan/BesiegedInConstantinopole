using System;

using System.Text;

using UnityEngine;



// Council actions share the existing campaign dictionaries, so checkpoints retain every consequence.

// Costs come from Core Mechanics 1.4. Aid packages/chances remain explicit playtest values.

public static class CouncilActions

{

    public sealed class Ally

    {

        public string Name, Description;

        public int Chance, Days, Infantry, Archers, Cavalry, Ships, Gold, Provisions;

        public Ally(string name, string description, int chance, int days, int infantry, int archers, int cavalry, int ships, int gold, int provisions)

        {

            Name=name; Description=description; Chance=chance; Days=days;

            Infantry=infantry; Archers=archers; Cavalry=cavalry; Ships=ships; Gold=gold; Provisions=provisions;

        }

        public string Cargo => CargoFor(0);
        public string CargoFor(int warships) {
            var items=new System.Collections.Generic.List<string>();
            int[] counts={Infantry,Archers,Cavalry,warships,Gold,Provisions};string[] names={"infantry","archers","cavalry","ships","gold","provisions"};
            for(int i=0;i<counts.Length;i++) if(counts[i]>0) items.Add(counts[i].ToString("N0")+" "+(i==3 && counts[i]==1?"ship":names[i]));
            return string.Join(", ",items);
        }

    }

    // Append only: numeric ally keys are retained in campaign saves.
    public const int AqQoyunluIndex = 17;
    public static readonly Ally[] Allies = CountryDiplomacy.CreateAllies();

    // Map possessions resolve to the same append-only country record as ordinary diplomacy.
    public static int AllyIndexForRealm(string group) => CountryDiplomacy.Index(group);

    public static readonly string[] FactionKeys = {"fTroopRep","fPeopleRep","fNobleRep","fPriestRep","fGalataRep"};

    public static readonly string[] FactionNames = {"Army morale","Public morale","Noble support","Clergy support","Galata relations"};

    public static readonly string[] FactionDescriptions = {

        "Roman troops and foreign contingents share the defence, but have different loyalties and needs.",

        "Residents, refugees, workers and merchants face the same siege with unequal resources.",

        "Old court families guard their offices; merchant households seek safe harbours and reliable coin. Their wealth and retainers can sustain the defence, but their political loyalties differ.",

        "Unionist clergy look to Western aid. Orthodox opponents fear the price of that alliance. Both have followers in the City, and both expect the Emperor to hear them.",

        "The Genoese community across the Golden Horn seeks security and trade while officially remaining neutral."

    };

    public static readonly string[] FactionOrders = {"Visit the walls and reward courage","Distribute relief to the people","Host the noble households","Commission icons for Hagia Sophia","Send a gift to Galata's podestà"};

    public static readonly string[] SpyOrders = {"Recruit an agent in the enemy camp","Sabotage Ottoman cannons","Burn enemy food stores","Hunt spies in Constantinople","Gather secrets","Disrupt Ottoman preparations"};

    public static readonly string[] SpyEffects = {"+1 spy", "Destroy 1–3 cannons", "Burn 1–10 days of stores; shortages force resupply and desertion", "50% chance to uncover one Ottoman spy in the City", "Gain 0, 1 or 2 secrets (25% / 25% / 50%)", "Delay Ottoman operations by 1 siege day"};

    public static int Int(string key) {
        try { return VariableSingleton.GetIntVariable(key); }
        catch(System.Collections.Generic.KeyNotFoundException) {
            if(key.StartsWith("iAidState") || key.StartsWith("iAidDays")) { VariableSingleton.SetInt(key,0);return 0; }
            throw;
        }
    }

    private static string Report(VariableSingleton.CampaignSave before, string explanation) => explanation + "\n\n" + EventOutcome.Describe(before, VariableSingleton.Instance.CaptureState());



    public static bool SupportFaction(int index, out string report)

    {

        report="";

        if(index<0 || index>=FactionKeys.Length) return false;

        if(Int("iMoney")<300) { report="You need 300 gold."; return false; }

        if(VariableSingleton.GetFloatVariable(FactionKeys[index])>=1) {report="Support is already at its maximum.";return false;}

        var before=VariableSingleton.Instance.CaptureState();

        VariableSingleton.ChangeInt("iMoney",-300);

        VariableSingleton.ChangePercentFloat(FactionKeys[index],.03f);

        report=Report(before,FactionOrders[index]+". Your personal attention improves relations.");

        return true;

    }

    public static bool SpyMission(int index, bool paySecret, out string report)

    {

        report="";

        if(index<0 || index>=SpyOrders.Length) return false;


        if(index!=0 && Int("iSpies")<1) {report="Recruit an agent first.";return false;}

        string resource=paySecret?"iSecrets":"iMoney";

        int cost=paySecret?1:1000;

        if(Int(resource)<cost) {report=paySecret?"You need one secret.":"You need 1,000 gold.";return false;}

        if(index==1 && Int("iTurkCannons")==0 || index==2 && Int("iFood")==0 || index==3 && Int("iTurkSpies")==0)

        {report="No targets remain for this mission.";return false;}

        var before=VariableSingleton.Instance.CaptureState();

        VariableSingleton.ChangeInt(resource,-cost);

        float risk=EspionageSystem.LossChance(index);
        string explanation=SpyOrders[index]+" completed.";

        switch(index)

        {

            case 0: VariableSingleton.ChangeInt("iSpies",1); break;

            case 1: VariableSingleton.ChangeInt("iTurkCannons",-UnityEngine.Random.Range(1,4)); break;

            case 2: VariableSingleton.ChangeInt("iFood",-UnityEngine.Random.Range(1,11)); break;

            case 3:

                bool found=UnityEngine.Random.Range(0,100)<50;

                if(found) VariableSingleton.ChangeInt("iTurkSpies",-1);

                explanation=found?"Your agents uncover an Ottoman operative.":"Your agents find no conclusive lead. The operation's cost is spent.";

                break;

            case 4: VariableSingleton.ChangeInt("iSecrets",Mathf.Min(2,UnityEngine.Random.Range(0,4))); break;

            case 5:
                VariableSingleton.ChangeInt("iEnemyDelay",1);
                explanation="Forged orders send labourers astray and leave siege equipment waiting for supplies. Ottoman preparations fall a day behind.";
                break;

        }

        bool lost=EspionageSystem.FinishOperation(index,risk);
        explanation+=lost?" One of your agents is lost during the operation.":" Your agents return safely.";
        report=Report(before,explanation);

        return true;

    }

    // Rounded legacy accessor retained for existing scripts; UI and rolls use the exact value.
    public static int AidChance(int index) => Mathf.RoundToInt(CountryDiplomacy.FinalChance(index));
    public static float ExactAidChance(int index) => CountryDiplomacy.FinalChance(index);

    public static bool RequestAid(int index, out string report)

    {

        report="";

        if(index<0 || index>=Allies.Length) return false;

        report=CountryDiplomacy.AidRequestReason(index);
        if(report!=null)return false;

        var before=VariableSingleton.Instance.CaptureState();

        VariableSingleton.ChangeInt("iMoney",-1000);

        CountryDiplomacy.ConsumeAidPermission(index);
        DiplomaticStoryAid.ClearLastPackage(index);
        bool accepted=UnityEngine.Random.Range(0f,100f)<ExactAidChance(index);

        VariableSingleton.SetInt("iAidWarships"+index,0);
        VariableSingleton.SetInt("iAidState"+index,accepted?1:3);

        VariableSingleton.SetInt("iAidDays"+index,accepted?Allies[index].Days:0);

        report=Report(before,accepted?$"{Allies[index].Name} commits aid. Expected in {Allies[index].Days} calendar days.\nPromised: {Allies[index].Cargo}. {AidRouting.Route(index)}; transports do not join our fleet.":$"{Allies[index].Name} declines your appeal. No reinforcements are promised.");
        report+=DiplomaticReplies.Format(Allies[index].Name,accepted,Int("iReligion"),index+VariableSingleton.Turn);

        return true;

    }

    public static bool HurryAid(int index,out string report)

    {

        report="";

        if(index<0 || index>=Allies.Length) return false;

        if(Int("iAidState"+index)!=1 || Int("iAidDays"+index)<=1 || Int("iMoney")<1000) {report="You need an expedition on the way and 1,000 gold.";return false;}

        var before=VariableSingleton.Instance.CaptureState();

        VariableSingleton.ChangeInt("iMoney",-1000);

        VariableSingleton.SetInt("iAidDays"+index,Mathf.Max(1,Int("iAidDays"+index)-5));

        report=Report(before,$"You expedite {Allies[index].Name}'s expedition. Arrival in {Int("iAidDays"+index)} calendar days.");

        return true;

    }

    // Called exactly once when the campaign calendar advances; pending aid survives save/load.

    public static string AdvanceAid(int elapsedDays)

    {

        var arrivals=new StringBuilder();

        for(int i=0;i<Allies.Length;i++)

        {

            if(Int("iAidState"+i)!=1) continue;

            VariableSingleton.SetInt("iAidDays"+i,Mathf.Max(0,Int("iAidDays"+i)-elapsedDays));

            if(Int("iAidDays"+i)>0) continue;

            var before=VariableSingleton.Instance.CaptureState();

            var ally=Allies[i];

            int promisedWarships=AidRouting.Warships(i);
            VariableSingleton.SetInt("iAidState"+i,2);

            string[] keys={"iInfantry","iArchers","iCavalry","iShips","iMoney","iProvisions"};

            int[] values={ally.Infantry,ally.Archers,ally.Cavalry,promisedWarships,ally.Gold,ally.Provisions};

            for(int j=0;j<keys.Length;j++) VariableSingleton.ChangeInt(keys[j],values[j]);

            arrivals.AppendLine(Report(before,ally.Name+"'s promised aid arrives."));

        }

        return arrivals.ToString();

    }

    // Provisional supply loop: shortages combine lost operational time with desertion.

    // Resolve before the day's tactic so a resupply day cannot also produce an assault.

    public static string ResolveEnemySupplies()=>ProvinceCampaign.SupplyDay();

    public static string Intelligence()

    {

        int spies=Int("iSpies");

        var text=new StringBuilder($"Agents in the enemy camp: {spies}\nSecrets: {Int("iSecrets")}\nEnemy camp alert: {EspionageSystem.Alert}%\n\n");
        for(int i=0;i<EspionageSystem.IntelligenceLabels.Length;i++) text.AppendLine(EspionageSystem.IntelligenceLabels[i]+": "+EspionageSystem.IntelligenceValue(i));

        return text.ToString();

    }

}

