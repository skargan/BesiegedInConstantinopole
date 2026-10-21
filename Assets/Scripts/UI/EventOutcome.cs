using System.Collections.Generic;

using System.Linq;

using System.Text;

using UnityEngine;



// Reports actual state changes after Yarn commands run, including random outcomes.

public static class EventOutcome

{

    private static readonly Dictionary<string,string> Names = new Dictionary<string,string> {

        {"iCampAlert","Enemy camp alert (points)"},{"iIncome","Daily merchant levy (gold)"},{"fSultanRage","Sultan's fury"},{"iEnemyDelay","Enemy operations delayed (days)"},{"iTurkCannons","Ottoman cannons"},{"iTurkSpies","Ottoman spies in Constantinople"},{"iPopulation","City population"},{"iMoney","Gold"},{"iInfantry","Infantry"},{"iArchers","Archers"},{"iCavalry","Cavalry"},

        {"iVarangians","Varangian Guard"},{"iShips","Ships"},{"iCannons","Cannons"},{"iProvisions","Provisions"},{"iMaterials","Materials"},

        {"iEngineers","Engineers"},{"iSpies","Spies"},{"iSecrets","Secrets"},{"iAmmunition","Ammunition"},{"iHealers","Healers"},

        {"iBalkans","Ottoman Balkan troops"},{"iBazouks","Ottoman irregulars"},{"iJanissaries","Janissaries"},{"iFood","Ottoman provisions"},

        {"fHealth","Emperor's health"},{"fOuterWall","Theodosian Wall"},{"fMiddleWall","Constantinian Wall"},{"fInnerWall","Severan Wall"},

        {"fMoat","Moat integrity"},{"fNobleRep","Noble support"},{"fPeopleRep","Public morale"},{"fPriestRep","Clergy support"},{"fTroopRep","Troop morale"},

        {"fGalataRep","Galata relations"},{"fInfantryQuality","Infantry quality"},{"fArchersQuality","Archer quality"},{"fCavalryQuality","Cavalry quality"},

        {"bCharWarrior0","Giovanni Giustiniani"},{"bCharAdmiral1","Alvise Diedo"},{"bCharDiplomat4","George Sphrantzes"},{"bCharAdministrator2","Loukas Notaras"},{"bCharEnemy8","Hasan the Janissary"},{"bCharNomad3","Prince Orhan"},{"bCharArtillerist5","Orban"},{"bCharArchitect6","Johannes Grant"},{"bCharCardinal7","Cardinal Isidore"},

        {"bGalataIndependence","Galata independence"},{"bStraitsOpen","Bosphorus passage"},{"bMoreanDistraction","Morean diversion"},

        {"bTechGreekFire","Greek fire"},{"bTechCounterCannons","Counter-battery artillery"},{"bTechGrapeshot","Grapeshot"}

    };

    private static string Name(string key) => Names.TryGetValue(key,out var value) ? value : System.Text.RegularExpressions.Regex.Replace(key.Substring(1),"([a-z])([A-Z])","$1 $2");

    public static string Describe(VariableSingleton.CampaignSave before, VariableSingleton.CampaignSave after)

    {

        var report=new StringBuilder();
        report.Append(PopulationAccounting.Report(before,after));

        var ints=before.ints.ToDictionary(v=>v.key,v=>v.value);

        foreach(var v in after.ints)

        {

            if(v.key.StartsWith("iHard") || v.key.StartsWith("iCombat") || v.key=="iEmperorAtWall" || v.key.StartsWith("iLife") || v.key.StartsWith("iPopulation") || v.key=="iMerchantFavor")continue;
            if(v.key.StartsWith("iPrepDone") || v.key.StartsWith("iGalataEvent") || v.key=="iPreparationVersion" || v.key.StartsWith("iShah") || v.key=="iFoodShortfall" || v.key=="iHungerDays" || v.key=="iHungerOfferedDay") continue;
            if(v.key.StartsWith("iSultanVisit") || v.key.StartsWith("iSultanTalk") || v.key.StartsWith("iSultanSeen") || v.key=="iSultanConversationDay" || v.key=="iTurkPolicy" || v.key.StartsWith("iAdventurer")) continue;
            if(v.key=="iTurkSpies" && before.ints.FirstOrDefault(x=>x.key=="iTurkPolicy")?.value!=after.ints.FirstOrDefault(x=>x.key=="iTurkPolicy")?.value)continue;
            if(v.key.StartsWith("iPol")) continue;
            if(v.key.StartsWith("iBloc") || v.key.StartsWith("iAllegiance") || v.key=="iBanishmentUsed") continue;
            if(v.key.StartsWith("iCourtState")) continue;

            if(v.key.StartsWith("iAidState") || v.key.StartsWith("iAidDays")) continue;

            if(!ints.TryGetValue(v.key,out var old) || old==v.value) continue;

            if(v.key=="iFocus") {report.AppendLine("Siege Focus: "+FocusIcon.Names[Mathf.Clamp(v.value,0,4)]);continue;}

            if(v.key=="iGrandCannon") {report.AppendLine("Grand Cannon: "+new[]{"None in service","Ottoman possession","City possession"}[Mathf.Clamp(v.value,0,2)]);continue;}
            if(v.key=="iFleet") {report.AppendLine("Fleet orders: "+new[]{"Raid Ottoman supplies","Raid the coast and shipping","Bring grain from Crimea","Defend the Golden Horn","Bring volunteers","Fleet destroyed"}[Mathf.Clamp(v.value,0,5)]);continue;}
            if(v.key=="iCommander" && v.value<0) {report.AppendLine("Commander: vacant");continue;}
            if(v.key=="iCommander") {report.AppendLine("Commander: "+CourtRoster.CommanderName(v.value));continue;}

            if(v.key=="iReligion") {report.AppendLine("Religious policy: "+new[]{"Orthodoxy retained","Full Union","Partial Union"}[Mathf.Clamp(v.value,0,2)]);continue;}

            if(v.key=="iHarvestDay") {report.AppendLine(v.value==0?"The garden harvest has been collected.":"First garden harvest expected "+new System.DateTime((long)v.value*System.TimeSpan.TicksPerDay).ToString("d MMM yyyy")+".");continue;}
            long factor=v.key=="iPopulation" ? 1000 : 1;

            long change=(v.value-(long)old)*factor;

            report.AppendLine(Name(v.key)+": "+(change>0 ? "+" : "")+change.ToString("N0")+"  →  now "+(v.value*factor).ToString("N0"));

        }

        var floats=before.floats.ToDictionary(v=>v.key,v=>v.value);

        foreach(var v in after.floats)

        {

            if(!floats.TryGetValue(v.key,out var old) || Mathf.Abs(v.value-old)<.0001f) continue;

            if(v.key.EndsWith("Quality")) {
                float improvement=v.value-old;
                report.AppendLine(Name(v.key)+": "+(improvement>0?"+":"")+improvement.ToString("0.##")+" → now "+v.value.ToString("0.##")+"× lethality");continue;
            }
            float delta=(v.value-old)*100;

            report.AppendLine(Name(v.key)+": "+(delta>0?"+":"")+delta.ToString("0.#")+" points  →  now "+(v.value*100).ToString("0.#")+"%");

        }

        var bools=before.bools.ToDictionary(v=>v.key,v=>v.value);

        foreach(var v in after.bools)

        {

            if(!bools.TryGetValue(v.key,out var old) || old==v.value) continue;

            string state=v.key.StartsWith("bChar") ? (v.value?"available":"no longer available") : (v.value?"active":"inactive");

            report.AppendLine(Name(v.key)+": "+state);

        }

        return report.Length>0 ? report.ToString() : "";

    }

}

