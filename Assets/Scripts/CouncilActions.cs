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

    public static readonly Ally[] Allies = {

        new Ally("Hungary", "A potential landward counterweight to the Ottomans; aid is uncertain.",70,27,5000,0,500,0,0,0),

        new Ally("Venice", "Maritime interests and the defenders already in the City do not guarantee a state expedition.",70,18,500,500,0,5,0,0),

        new Ally("Genoa", "Giustiniani's volunteers, Galata and the Genoese government have distinct interests.",70,22,500,500,0,5,0,0),

        new Ally("The Papacy", "Church Union strengthens this appeal; an Orthodox ruler can still request modest aid.",50,20,0,200,0,2,20000,200),

        new Ally("Albania", "A shared enemy offers an opening for cooperation.",70,14,300,0,100,0,0,0),

        new Ally("Knights Hospitaller", "A small maritime reinforcement motivated by faith and regional defence.",35,6,100,100,50,2,0,0),

        new Ally("Naples", "Royal ambitions and resources compete with other commitments.",35,17,0,750,0,0,30000,0),

        new Ally("Holy Roman Empire", "Imperial sympathy does not ensure a coordinated expedition.",10,40,1000,300,100,0,10000,0),
        new Ally("France", "An appeal for a royal expedition and funds for the defence.",15,48,1500,500,250,2,10000,0),
        new Ally("Athens", "Seek troops and provisions from the Frankish court.",15,12,300,100,50,1,0,100),
        new Ally("Georgia", "Ask for warriors and archers to cross the Black Sea.",25,28,500,500,150,0,0,150),
        new Ally("Trebizond", "Request ships carrying soldiers and grain.",30,18,200,200,0,2,0,300),
        new Ally("Cyprus", "Seek an expedition from the island kingdom.",20,16,200,150,50,2,2000,100),
        new Ally("Poland and Lithuania", "Appeal for cavalry, volunteers and a war subsidy.",15,42,500,250,500,0,5000,0),
        new Ally("Teutonic Order", "Request an armed brotherhood and its supporting troops.",15,50,500,250,200,1,0,0),
        new Ally("England", "Ask for archers, ships and funds for the City's defence.",15,60,300,750,100,2,5000,0),
        new Ally("Karaman", "Ibrahim Bey's rivalry with the Sultan opens a route for discreet funds and supplies.",20,25,0,0,0,0,3000,300)

    };

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

        report=Report(before,FactionOrders[index]+". Your personal attention improves relations. This uses the major action.");

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
        report=Report(before,explanation+" This uses the major action.");

        return true;

    }

    public static int AidChance(int index)

    {

        int chance=Allies[index].Chance;

        if(index==3) chance=Int("iReligion")==1?90:Int("iReligion")==2?70:35;

        if(Int("iFocus")==4) chance+=10;
        if(PoliticalSystem.Passive(2)&&PoliticalSystem.Western(index))chance+=5;

        chance+=Mathf.RoundToInt(CourtRoster.Bonus(4,10,20));

        return Mathf.Clamp(chance,0,95);

    }

    public static bool RequestAid(int index, out string report)

    {

        report="";

        if(index<0 || index>=Allies.Length) return false;

        if(Int("iAidState"+index)!=0) {report="This ally has already answered your appeal this campaign.";return false;}

        if(Int("iMoney")<1000 || (AidRouting.BySea(index) && Int("iShips")<1)) {report="An appeal requires 1,000 gold. Sea appeals also require an available ship; it is not consumed.";return false;}

        var before=VariableSingleton.Instance.CaptureState();

        VariableSingleton.ChangeInt("iMoney",-1000);

        bool accepted=UnityEngine.Random.Range(0,100)<AidChance(index);

        VariableSingleton.SetInt("iAidWarships"+index,0);
        VariableSingleton.SetInt("iAidState"+index,accepted?1:3);

        if(accepted) VariableSingleton.SetInt("iAidDays"+index,Allies[index].Days);

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

    public static string ResolveEnemySupplies()

    {

        VariableSingleton.ChangeInt("iFood",-1);

        if(Int("iFood")>0) return "";

        var before=VariableSingleton.Instance.CaptureState();

        int deserters=Mathf.CeilToInt(Int("iBazouks")*.01f);

        VariableSingleton.ChangeInt("iBazouks",-deserters);

        VariableSingleton.SetInt("iFood",15);

        return Report(before,"Ottoman stores run out. The army spends today securing provisions instead of attacking. Hunger and disrupted pay drive away "+deserters.ToString("N0")+" irregulars. The new convoy brings 15 days of stores.");

    }



    public static string Intelligence()

    {

        int spies=Int("iSpies");

        var text=new StringBuilder($"Agents in the enemy camp: {spies}\nSecrets: {Int("iSecrets")}\nEnemy camp alert: {EspionageSystem.Alert}%\n\n");
        for(int i=0;i<8;i++) text.AppendLine(EspionageSystem.IntelligenceLabels[i]+": "+EspionageSystem.IntelligenceValue(i));

        return text.ToString();

    }

}

