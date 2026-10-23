using System;

using UnityEngine;



// Technology definitions follow Core Mechanics 1.3. Benefits are derived from flags,

// so event unlocks and restored saves cannot apply a bonus twice.

public static class TechnologySystem

{

    public sealed class Technology

    {

        public readonly string Key, Name, Effect, History;

        public readonly int Cost;

        public Technology(string key, string name, int cost, string effect, string history)

        { Key="bTech"+key; Name=name; Cost=cost; Effect=effect; History=history; }

    }

    public static readonly Technology[] All = {

        new Technology("GreekFire","Greek Fire",3,"An additional 5% of an assault's attackers are stopped before they reach the defenders.","Pitch catches on the water. The artificers deliver sealed vessels of incendiary mixture, and the crews practise passing them along the battlements."),

        new Technology("CounterCannons","Counter-battery artillery",3,"Unlock a major action: spend 3 ammunition to destroy 1–5 Ottoman cannons.","Gunners mark the enemy batteries on their charts. Powder is measured, ranges called and the first counter-battery crews take their stations."),

        new Technology("RomanConcrete","Repair mortar and masonry",3,"Repair gold costs -25%.","Lime kilns burn through the night. Masons prepare fresh mortar while labourers sort stone for the breaches."),

        new Technology("RomanTraining","Roman Tactics",3,"Infantry trained lethality +25%.","Veterans take charge of the drill yards. By evening, recruits hold formation through the press of shields and the noise of shouted orders."),

        new Technology("ArtilleryShips","Artillery Ships",2,"Doubles ship-based damage in defensive fleet reports.","Shipwrights reinforce the gun platforms. Captains bring their vessels into position to cover the harbour approaches."),

        new Technology("Grenades","Incendiary grenades",2,"Infantry lethality +0.25.","Clay vessels arrive from the workshops, packed and sealed. Selected troops learn to light their fuses and cast them into a crowded approach."),

        new Technology("Handcannons","Handcannons",2,"Ranged lethality +0.5.","Smiths deliver the first handcannons. Crews practise loading under cover and firing in succession."),

        new Technology("Grapeshot","Anti-personnel cannon shot",2,"Cannon lethality increases by 250% (3.5 times the base).", "Gunners replace the heavy ball with smaller shot. At close range, each discharge will sweep the ground before the breach."),

        new Technology("RomanTaxation","Emergency fiscal administration",3,"Daily siege income: 3 gold per 1,000 residents; fractional revenue carries forward.", "The treasury opens a new register. Collectors receive their districts, exemptions are reviewed and overdue revenues are called into the defence of the City."),

        new Technology("RomanArchitecture","Defensive engineering",2,"All defensive lines gain +5 wall defence bonus.","Engineers walk the defensive lines with chalk and measuring cords. Reinforced approaches and new firing positions give the garrison better ground to hold."),

        new Technology("UpgradedCrossbows","Upgraded Crossbows",2,"Archer trained lethality +25%.","Bowyers fit stronger prods and new winding mechanisms. The crossbowmen test their weapons before returning to the walls."),

        new Technology("PlateArmour","Cataphract Armour",2,"Cavalry trained lethality +25%.","Armourers fit the new harness piece by piece. Cavalry reserves enter the fighting at the breaches with stronger protection."),
        new Technology("VarangianDrills","Varangian Drills",2,"Varangian lethality +20% when deployed.","The guard rehearses its advance through a narrow breach. Axe-men take turns at the front, and the ranks close before an enemy can force them apart."),
        new Technology("DismountedChampions","Dismounted Champions",2,"Cavalry uses its full lethality when defending the walls.","The cavalry leaves its horses below the ramparts. Veterans practise fighting in heavy armour among rubble and broken stairways, bringing their strength to the breach.")

    };

    public static bool Has(string name) => VariableSingleton.GetBoolVariable("bTech"+name);

    public static bool Unlock(int index, out string report)

    {

        report="";

        if(index<0 || index>=All.Length) return false;

        if(index==2) {report="This workshop is unavailable.";return false;}

        var item=All[index];

        if(VariableSingleton.GetBoolVariable(item.Key)) {report="Already active; no secrets spent.";return false;}

        if(VariableSingleton.GetIntVariable("iSecrets")<item.Cost) {report="Not enough secrets.";return false;}

        VariableSingleton.ChangeInt("iSecrets",-item.Cost);

        VariableSingleton.ChangeBool(item.Key,true);

        report=item.Name+" adopted.\n\n"+item.History+"\n\n"+item.Effect+"\n\nSecrets spent: "+item.Cost+". Remaining: "+VariableSingleton.GetIntVariable("iSecrets")+".";

        return true;

    }

    public static float Quality(string unit)

    {

        float value=VariableSingleton.GetFloatVariable("f"+unit+"Quality");

        string tech=unit=="Infantry"?"RomanTraining":unit=="Archers"?"UpgradedCrossbows":"PlateArmour";

        return value*(Has(tech)?1.25f:1f);
    }


    public static float RepairMultiplier => Has("RomanConcrete")?.75f:1f;

    public static int FleetDefence => VariableSingleton.GetIntVariable("iFleet")==3 ? VariableSingleton.GetIntVariable("iShips")*(Has("ArtilleryShips")?2:1):0;

    public static string ArtilleryUnavailable(bool grand)
    {
        if(grand && CouncilActions.Int("iGrandCannon")!=2) return "Grand Cannon required";
        if(!grand && !Has("CounterCannons")) return "Counter Cannons required";
        if(!VariableSingleton.HasSiegeStarted()) return "Available during siege";
        if(!grand && CouncilActions.Int("iCannons")==0) return "Requires a cannon";
        if(CouncilActions.Int("iTurkCannons")==0) return "No enemy cannons";
        if(CouncilActions.Int("iAmmunition")<3) return "Requires 3 ammunition";
        return null;
    }

    public static bool FireGrandCannon(out string report)
    {
        report=ArtilleryUnavailable(true);if(report!=null) return false;
        int losses=Mathf.Min(CouncilActions.Int("iTurkCannons"),UnityEngine.Random.Range(1,6));
        VariableSingleton.ChangeInt("iAmmunition",-3);VariableSingleton.ChangeInt("iTurkCannons",-losses);
        report="The Grand Cannon shakes the parapet. Across the field, "+losses+" Ottoman cannons are shattered.\n\nAmmunition spent: 3. Enemy cannons remaining: "+CouncilActions.Int("iTurkCannons")+".";
        return true;
    }

    public static bool CounterBattery(out string report)

    {

        report="";

        if(!Has("CounterCannons") || !VariableSingleton.HasSiegeStarted()) {report="Requires counter-battery artillery and an active siege.";return false;}

        if(VariableSingleton.GetIntVariable("iAmmunition")<3 || VariableSingleton.GetIntVariable("iTurkCannons")==0 || VariableSingleton.GetIntVariable("iCannons")==0)

        {report="Requires a cannon, 3 ammunition and an enemy cannon target.";return false;}

        int losses=Mathf.Min(VariableSingleton.GetIntVariable("iTurkCannons"),UnityEngine.Random.Range(1,6));

        VariableSingleton.ChangeInt("iAmmunition",-3);VariableSingleton.ChangeInt("iTurkCannons",-losses);

        report="Counter-battery fire destroys "+losses+" Ottoman cannons.\nAmmunition -3; now "+VariableSingleton.GetIntVariable("iAmmunition")+".\nEnemy cannons remaining: "+VariableSingleton.GetIntVariable("iTurkCannons")+".";

        return true;

    }

}

