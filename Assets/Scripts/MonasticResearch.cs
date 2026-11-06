using System;
using UnityEngine;

// Allocation and progress use the ordinary saved campaign integer store.
// Only CampaignPersonnelCalendar advances this service, one dated boundary at a time.
public static class MonasticResearch
{
    [Serializable] public sealed class Balance { public int ProgressPerSecret=1000; public int ProgressPerMonkPerDay=1; }
    static Balance balance;
    public static Balance Rules { get {
        if(balance==null){var asset=Resources.Load<TextAsset>("Content/MonasticResearch");balance=asset==null?new Balance():JsonUtility.FromJson<Balance>(asset.text);}
        balance.ProgressPerSecret=Math.Max(1,balance.ProgressPerSecret);balance.ProgressPerMonkPerDay=Math.Max(1,balance.ProgressPerMonkPerDay);return balance;
    } }
    const string Prefix="iMonastic";
    static readonly string[] Fields={"Version","Active","Pending","PendingDay","Progress","LastDay"};
    static int I(string key)=>Math.Max(0,DailyLifeEvents.I(Prefix+key));
    static void Set(string key,int value)=>VariableSingleton.SetInt(Prefix+key,Math.Max(0,value));
    static int Clamp(long value)=>(int)Math.Min(int.MaxValue,Math.Max(0L,value));
    public static int TotalMonks=>Math.Max(0,DailyLifeEvents.I("iHealers"));
    public static int ResearchMonks {get {Ensure();ValidateAllocation();return I("Active");}}
    public static int InfirmaryMonks=>TotalMonks-ResearchMonks;
    public static int Progress {get {Ensure();return I("Progress");}}
    // Calendar work is evaluated against the day being resolved, not the already
    // advanced global turn (a preparation turn can cross into the siege).
    public static DateTime ResearchStart=>PreparationCampaign.Date(PreparationCampaign.SiegeTurn).Date;
    public static bool CanResearchOn(DateTime date)=>date.Date>=ResearchStart;
    public static bool ProductionEnabled=>VariableSingleton.HasSiegeStarted()&&CanResearchOn(VariableSingleton.GetDate());
    public static int DailyProgress=>ProductionEnabled?ProgressFor(ResearchMonks):0;
    public static int ProgressFor(int monks)=>Clamp((long)Math.Max(0,monks)*Rules.ProgressPerMonkPerDay);
    public static bool HasPending {get {Ensure();return I("PendingDay")>0;}}
    public static int PendingResearchMonks {get {Ensure();ValidateAllocation();return HasPending?I("Pending"):ResearchMonks;}}
    public static DateTime PendingDate=>new DateTime((long)I("PendingDay")*TimeSpan.TicksPerDay);
    public static int LastProcessedDay=>I("LastDay");
    public static int DaysToDiscovery(int monks) {
        if(!ProductionEnabled)return 0;
        long daily=ProgressFor(monks);return daily==0?0:(int)Math.Max(1,((long)Rules.ProgressPerSecret-Progress+daily-1)/daily);
    }
    public static void ResetForRestore(){foreach(string field in Fields)Set(field,0);}
    // New campaigns share the starting roster equally; an odd monk stays in care.
    // Restore/legacy migration deliberately keeps its existing allocation behaviour.
    public static void BeginCampaign(){ResetForRestore();Ensure();Set("Active",TotalMonks/2);}
    public static void Ensure() {
        if(I("Version")>0)return;
        ResetForRestore();Set("Version",1);Set("LastDay",TrainingPool.Day(VariableSingleton.GetDate()));
    }
    public static void AfterRestore(){Ensure();ValidateAllocation();}
    public static void ValidateAllocation() {
        Set("Active",Math.Min(TotalMonks,I("Active")));
        Set("Pending",Math.Min(TotalMonks,I("Pending")));
    }
    public static bool ApplyAllocation(int monks,out string report) {
        Ensure();ValidateAllocation();
        if(monks<0||monks>TotalMonks){report="Choose an allocation within the available monk roster.";return false;}
        if(monks==ResearchMonks){Set("PendingDay",0);Set("Pending",monks);report="Current duties retained. No change is pending.";return true;}
        Set("Pending",monks);Set("PendingDay",TrainingPool.Day(VariableSingleton.GetDate())+1);
        report="Allocation confirmed for "+PendingDate.ToString("d MMM yyyy")+": "+monks+" researching, "+(TotalMonks-monks)+" tending the wounded. No action or gold spent.";
        return true;
    }
    public static void AllocationsForDay(DateTime date) {
        Ensure();int day=TrainingPool.Day(date);
        if(day<=LastProcessedDay)return;
        if(I("PendingDay")>0&&I("PendingDay")<=day){Set("Active",I("Pending"));Set("PendingDay",0);}
        ValidateAllocation();
    }
    // No elapsed-days multiplication: arrivals and assignments must be settled first.
    public static int Advance(DateTime date) {
        Ensure();int day=TrainingPool.Day(date);if(day<=LastProcessedDay)return 0;
        AllocationsForDay(date);
        if(!CanResearchOn(date)){Set("LastDay",day);return 0;}
        long total=(long)Progress+ProgressFor(ResearchMonks);
        int discoveries=Clamp(total/Rules.ProgressPerSecret);
        Set("Progress",(int)(total%Rules.ProgressPerSecret));Set("LastDay",day);
        if(discoveries>0)VariableSingleton.SetInt("iSecrets",Clamp((long)DailyLifeEvents.I("iSecrets")+discoveries));
        return discoveries;
    }
    public static string DiscoveryReport(int discoveries,bool active) {
        if(discoveries==0&&!active)return "";
        return (discoveries>0?"Knowledge recovered: +"+discoveries+" Secret"+(discoveries==1?"":"s")+".\n":"")+
            "Monastic research continues: "+Progress+" / "+Rules.ProgressPerSecret+" progress.\n\n";
    }
    public static string Summary=>ResearchMonks+" assigned to research · "+InfirmaryMonks+" tending the wounded\n"+Progress+" / "+Rules.ProgressPerSecret+" toward the next Secret · "+(ProductionEnabled?"+"+DailyProgress+" progress/day":"Research begins with the siege")+
        (HasPending?"\nFrom "+PendingDate.ToString("d MMM")+": "+PendingResearchMonks+" researching":"");
}
