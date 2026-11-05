using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Candar is a discreet market, not an allied court. The diplomacy prefix participates
// in existing campaign capture and reset. No separate state model is introduced.
public static class CandarDiplomacy
{
    const string Prefix = "iDiplomacyCandar";
    public const int RansomCost = 3000, SecretCost = 2000, DisruptionCost = 2500, ReplenishmentDays = 7;
    static int I(string key) => DailyLifeEvents.I(Prefix + key);
    static void Put(string key, int value) => VariableSingleton.SetInt(Prefix + key, value);
    public static int Day => TrainingPool.Day(VariableSingleton.GetDate());
    static DateTime Date(int day) => new DateTime((long)Math.Max(0, day) * TimeSpan.TicksPerDay);
    public static bool IsCandar(int index) => index >= 0 && index < CountryDiplomacy.Definitions.Length && CountryDiplomacy.Definitions[index].Id == "candar";
    public static bool Unlocked => I("Unlocked") == 1;
    public static bool StraitOpen => VariableSingleton.GetBoolVariable("bStraitsOpen");
    public static bool OrhanInService => CourtRoster.InService(3);
    public static string UnlockReason => Unlocked ? "Secret negotiations are already open." :
        !StraitOpen || !OrhanInService ? "Requires an open strait and Orhan Çelebi in your service." : null;
    public static DateTime RansomAvailableDate => Date(I("RansomDue"));
    public static DateTime DisruptionAvailableDate => Date(I("DisruptionDue"));
    public static int SecretsRemaining {
        get { int count = 0; for(int i = 0; i < SecretNames.Length; i++) if((I("SecretMask") & (1 << i)) == 0) count++; return count; }
    }
    public static readonly string[] SecretNames = {"The quartermaster's duplicate ledger", "The harbour factor's private accounts", "The courier's concealed correspondence"};
    static readonly string[] SecretReports = {
        "A copied ledger exposes a quartermaster's concealed payments. One secret is now available to your intelligence network.",
        "A harbour factor's hidden accounts reveal an Ottoman contact's private business. One secret is now available to your intelligence network.",
        "A courier's concealed correspondence supplies a fresh source of leverage. One secret is now available to your intelligence network."
    };
    static string AccessReason => Unlocked ? null : "Open secret negotiations first.";
    static bool CanAdd(string key, int quantity) => (long)DailyLifeEvents.I(key) + quantity <= int.MaxValue;
    public static string RansomReason => AccessReason ??
        (Day < I("RansomDue") ? "Next captive convoy: " + RansomAvailableDate.ToString("d MMM yyyy") + "." :
        DailyLifeEvents.I("iMoney") < RansomCost ? "Requires 3,000 gold." :
        !RansomFits ? "There is no room to record the complete convoy." : null);
    static bool RansomFits => CanAdd("iPopulation",300) && CanAdd("iInfantry",60) && CanAdd("iArchers",30) &&
        CanAdd("iCavalry",5) && CanAdd("iEngineers",5) && CanAdd("iHealers",5) && CanAdd("iEnvoys",1) && CanAdd("iVarangians",3);
    public static string SecretReason => AccessReason ??
        (SecretsRemaining == 0 ? "All three secrets have been purchased." :
        DailyLifeEvents.I("iMoney") < SecretCost ? "Requires 2,000 gold." :
        !CanAdd("iSecrets",1) ? "Your intelligence ledger is full." : null);
    public static bool DelayPending => I("DelayCredit") == 1 || I("Deferred") == 1;
    public static string DisruptionReason => AccessReason ??
        (Day < I("DisruptionDue") ? "Next available: " + DisruptionAvailableDate.ToString("d MMM yyyy") + "." :
        DelayPending ? "The previous disruption is still awaiting its operation." :
        DailyLifeEvents.I("iMoney") < DisruptionCost ? "Requires 2,500 gold." : null);

    public static bool OpenNegotiations(out string report)
    {
        report = UnlockReason; if(report != null) return false;
        Put("Unlocked",1);
        report = "A discreet understanding opens with İsmail Bey. Orhan has provided an introduction, not proclaimed a claim. No alliance or rebellion has begun.";
        return true;
    }
    public sealed class RansomArrival
    {
        public int Infantry, Archers, Cavalry, Engineers, Monks, Envoy, Guards, Population;
        public int Volunteers => Infantry + Archers + Cavalry + Engineers + Monks + Envoy + Guards;
        public int OtherResidents => Population - Volunteers;
    }
    // Every row has its own independent chance draw, including the guaranteed rows.
    // The random delegate follows Unity's [minimum, exclusiveMaximum) convention.
    public static RansomArrival RollRansom(Func<int,int,int> random)
    {
        if(random == null) throw new ArgumentNullException(nameof(random));
        return new RansomArrival {
            Infantry = Roll(random,100,20,60), Archers = Roll(random,100,15,30),
            Cavalry = Roll(random,50,1,5), Engineers = Roll(random,50,1,5),
            Monks = Roll(random,50,1,5), Envoy = Roll(random,5,1,1),
            Guards = Roll(random,10,1,3), Population = Roll(random,100,150,300)
        };
    }
    static int Roll(Func<int,int,int> random, int chance, int minimum, int maximum)
        => random(0,100) < chance ? random(minimum,maximum+1) : 0;
    public static bool Ransom(out string report)
    {
        report = RansomReason; if(report != null) return false;
        var arrival = RollRansom(UnityEngine.Random.Range);
        VariableSingleton.ChangeInt("iMoney",-RansomCost);
        Put("RansomDue",Day+ReplenishmentDays);
        // Total rescued population includes every volunteer below. Never add them twice.
        PopulationAccounting.Add(arrival.Population);
        string[] keys = {"iInfantry","iArchers","iCavalry","iEngineers","iHealers","iEnvoys","iVarangians"};
        int[] counts = {arrival.Infantry,arrival.Archers,arrival.Cavalry,arrival.Engineers,arrival.Monks,arrival.Envoy,arrival.Guards};
        for(int n=0;n<keys.Length;n++) if(counts[n]>0) VariableSingleton.ChangeInt(keys[n],counts[n]);
        // Regular soldiers already appear in RecruitmentLogistics.Serving. Reserve only
        // volunteers outside that count so they cannot also be recruited as civilians.
        SpecialistPersonnel.CommitLocalManpower(arrival.Engineers+arrival.Monks+arrival.Envoy+arrival.Guards);
        var text = new StringBuilder("Ransom paid: 3,000 gold.\n"+arrival.Population+" people arrive free. Of these, volunteers enter imperial service:\n");
        text.AppendLine("Infantry "+arrival.Infantry+" · Archers "+arrival.Archers);
        text.AppendLine("Cavalry "+arrival.Cavalry+" · Engineers "+arrival.Engineers+" · Monks "+arrival.Monks);
        text.AppendLine("Envoys "+arrival.Envoy+" · Varangian guards "+arrival.Guards);
        if(arrival.Guards>0) text.AppendLine("Freed veterans and northerners volunteer for the imperial guard.");
        text.AppendLine(arrival.OtherResidents+" other rescued residents settle in the City.");
        text.Append("Total population gained: "+arrival.Population+" (volunteers included).\nNext captive convoy: "+RansomAvailableDate.ToString("d MMM yyyy")+".");
        report=text.ToString(); return true;
    }
    public static bool PurchaseSecret(out string report)
    {
        report=SecretReason; if(report!=null) return false;
        var available=new List<int>();
        for(int i=0;i<SecretNames.Length;i++) if((I("SecretMask")&(1<<i))==0) available.Add(i);
        int chosen=available[UnityEngine.Random.Range(0,available.Count)];
        VariableSingleton.ChangeInt("iMoney",-SecretCost);
        VariableSingleton.ChangeInt("iSecrets",1);
        Put("SecretMask",I("SecretMask")|(1<<chosen));
        report=SecretNames[chosen]+"\n"+SecretReports[chosen]+"\n\nPaid: 2,000 gold. Secrets remaining: "+SecretsRemaining+".";
        return true;
    }
    public static bool Disrupt(out string report)
    {
        report=DisruptionReason; if(report!=null) return false;
        VariableSingleton.ChangeInt("iMoney",-DisruptionCost);
        Put("DisruptionDue",Day+ReplenishmentDays); Put("DelayCredit",1);
        report="Paid: 2,500 gold. Misplaced instructions will postpone the next eligible Ottoman operation by one calendar day. Resolved attacks, provincial supply schedules and the calendar are unchanged.\nNext available: "+DisruptionAvailableDate.ToString("d MMM yyyy")+".";
        return true;
    }

    // Supply emergencies, already-delayed orders, no-op reinforcement messages and
    // a compulsory move through a fallen wall are not paid tactical opportunities.
    public static bool Eligible(OttomanDirector.Operation op) => op != OttomanDirector.Operation.Resupply &&
        op != OttomanDirector.Operation.AwaitOrders && op != OttomanDirector.Operation.Reinforce && op != OttomanDirector.Operation.Advance &&
        op != OttomanDirector.Operation.NavalAssault && op != OttomanDirector.Operation.OverlandShips && op != OttomanDirector.Operation.ChainBattery;
    public static bool DeferredOperation(out OttomanDirector.Operation op)
    {
        op=(OttomanDirector.Operation)I("DeferredOperation");
        if(I("Deferred")!=1 || Day<I("DeferredDue")) return false;
        // A target may cease to exist while orders are held. Never force an illegal action.
        if(!Enum.IsDefined(typeof(OttomanDirector.Operation),op) || !Eligible(op) || !OttomanDirector.Legal(op)) { Put("Deferred",0);return false; }
        return true;
    }
    public static bool DelayOperation(OttomanDirector.Operation op, out string report)
    {
        report=null;
        // Naval pressure and non-tactical/emergency orders do not consume this purchase.
        if(!Eligible(op)) return false;
        if(I("Deferred")==1) {
            if(Day<I("DeferredDue")) { report="İsmail Bey's intermediaries have held the Ottoman orders until "+Date(I("DeferredDue")).ToString("d MMM yyyy")+"."; return true; }
            Put("Deferred",0); return false;
        }
        if(I("DelayCredit")!=1 || !Eligible(op)) return false;
        Put("DelayCredit",0); Put("Deferred",1); Put("DeferredOperation",(int)op); Put("DeferredDue",Day+1);
        report="İsmail Bey's intermediaries misdirect the orders. The next Ottoman operation is postponed until "+Date(Day+1).ToString("d MMM yyyy")+". Food consumption and naval pressure continue.";
        return true;
    }
}
