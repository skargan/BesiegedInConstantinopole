using System;
using System.Collections.Generic;

// Patients are removed from the army once at combat settlement. Remaining treatment
// days, unit identities and totals use the campaign's existing saved integer store.
public static class MedicalRecovery
{
    public const int TreatmentDays=15, MonkCost=200, RecruitmentDays=5;
    // Keep the first four indices stable: existing saves store patients by index.
    public static readonly string[] Units={"iInfantry","iArchers","iCavalry","iVarangians","iEngineers"};
    static int Day => TrainingPool.Day(VariableSingleton.GetDate());
    static int I(string key) => Math.Max(0,DailyLifeEvents.I(key));
    static void Put(string key,int value) => VariableSingleton.SetInt(key,Math.Max(0,value));
    static string PatientKey(int unit,int days) => "iMedicalPatient"+unit+"_"+days;
    static string ReturnedKey(int unit) => "iMedicalReturned"+unit;
    static int Bounded(long count) => (int)Math.Min(int.MaxValue,Math.Max(0L,count));
    public static int Monks => I("iHealers");
    public static int Capacity => MonasticResearch.InfirmaryMonks;
    public static int Total { get {long n=0;foreach(string unit in Units)n+=CountFor(unit);return Bounded(n);} }
    public static int RegularWounded => Bounded((long)CountFor(Units[0])+CountFor(Units[1])+CountFor(Units[2]));
    public static int FreePlaces => Math.Max(0,Capacity-Total);
    // Ordinary meals are already counted in city population; this is the extra treatment ration.
    public static double DailyProvisionNeed => Math.Min(Total,Capacity)/20d;
    public static int TreatableToday => Math.Min(Math.Min(Total,Capacity),(int)Math.Min(int.MaxValue,Math.Floor(SharedProvisions.Stock(SharedProvisions.City)*20d+.0000001)));
    public static double LastProvisionCost => SharedProvisions.Stock("medicalLastCost");
    public static int QueuedMonks { get {long n=0;foreach(var o in TrainingPool.Orders)if(o.unit=="iHealers")n+=o.quantity;return Bounded(n);} }
    public static int SoonestReturnDays { get {for(int d=1;d<=TreatmentDays;d++)for(int u=0;u<Units.Length;u++)if(I(PatientKey(u,d))>0)return d;return 0;} }
    public static int CountFor(string unit) {int u=Array.IndexOf(Units,unit);if(u<0)return 0;long n=0;for(int d=1;d<=TreatmentDays;d++)n+=I(PatientKey(u,d));return Bounded(n);}
    public static int RecoveredFor(string unit) {int u=Array.IndexOf(Units,unit);return u<0?0:I(ReturnedKey(u));}
    public static string TreatmentSummary => Total==0?"The infirmary has no wounded patients. Monks assigned here stand ready to receive casualties.":
        "Recovery takes 15 treatment days. "+
        (Capacity<Total? (Total-Capacity).ToString("N0")+" patients must wait each day: there are too few monks. ":"")+
        "Extra care uses one provision per 20 patients treated each day, in addition to their ordinary city ration. Treatment pauses when food is insufficient. " +
        "Patients closest to recovery are treated first. Waiting days do not count towards recovery.";
    public static string LastReport => I("iMedicalReportDay")==0?"No treatment report yet.":
        "Last treatment report · "+new DateTime((long)I("iMedicalReportDay")*TimeSpan.TicksPerDay).ToString("d MMM")+"\n"+
        I("iMedicalLastTreated").ToString("N0")+" treated · "+I("iMedicalLastReturned").ToString("N0")+" returned · "+SharedProvisions.Format(LastProvisionCost)+" extra provisions"+
        (I("iMedicalLastWaiting")>0?" · "+I("iMedicalLastWaiting").ToString("N0")+" waiting":"");

    public static void ResetForRestore()
    {
        for(int u=0;u<Units.Length;u++){Put(ReturnedKey(u),0);for(int d=1;d<=TreatmentDays;d++)Put(PatientKey(u,d),0);}
        foreach(string k in new[]{"LastDay","ReportDay","LastTreated","LastReturned","LastCost","LastWaiting"})Put("iMedical"+k,0);
        SharedProvisions.Set("medicalLastCost",0);
    }

    // Hamilton allocation preserves the actual casualty mix and exact total, including
    // small mixed groups. No additional gameplay RNG is consumed by medical accounting.
    static int[] Allocate(int target,int[] counts)
    {
        var result=new int[counts.Length];long total=0;foreach(int n in counts)total+=n;if(target<=0||total==0)return result;
        var remainder=new long[counts.Length];int assigned=0;
        for(int u=0;u<counts.Length;u++){long share=(long)target*counts[u];result[u]=(int)(share/total);remainder[u]=share%total;assigned+=result[u];}
        while(assigned<target){int best=-1;for(int u=0;u<counts.Length;u++)if(result[u]<counts[u]&&(best<0||remainder[u]>remainder[best]))best=u;if(best<0)break;result[best]++;remainder[best]=-1;assigned++;}
        return result;
    }

    // Only explicit combat adapters call this. Desertion, transfers, executions,
    // ship losses and narrative deaths must not become recoverable wounded.
    public static string ApplyCombatLosses(bool held,int infantry,int archers,int cavalry,int guards)
    {
        int[] requested={infantry,archers,cavalry,guards};var losses=new int[4];long total=0;
        for(int u=0;u<4;u++){losses[u]=Math.Min(I(Units[u]),Math.Max(0,requested[u]));total+=losses[u];}
        int rescued=held?Math.Min(FreePlaces,Bounded(total/2)):0;
        int[] admitted=Allocate(rescued,losses);
        for(int u=0;u<4;u++){
            if(losses[u]>0)VariableSingleton.ChangeInt(Units[u],-losses[u]);
            if(admitted[u]>0)Put(PatientKey(u,TreatmentDays),Bounded((long)I(PatientKey(u,TreatmentDays))+admitted[u]));
        }
        if(I("iMedicalLastDay")==0)Put("iMedicalLastDay",Day);
        if(total==0)return "";
        if(!held)return "No wounded could be brought back from the abandoned position. "+total.ToString("N0")+" permanent losses.";
        string details="";for(int u=0;u<4;u++)if(admitted[u]>0)details+=(details.Length>0?", ":"")+admitted[u]+" "+TrainingPool.Name(Units[u]).ToLowerInvariant();
        return rescued>0?"Monks receive "+rescued.ToString("N0")+" wounded ("+details+"). They are out of action for 15 treatment days. "+(total-rescued).ToString("N0")+" permanent losses.":
            (FreePlaces==0?"No free medical places. ":"No recoverable wounded were brought back. ")+total.ToString("N0")+" permanent losses; no wounded admitted.";
    }

    // The repair task has already removed this engineer from available duty.
    // A wounded survivor waits if all beds are occupied; no patient is evicted,
    // and treatment needs both an infirmary monk and its additional care ration.
    public static void AdmitWoundedEngineers(int count)
    {
        if(count<=0)return;
        int u=Array.IndexOf(Units,"iEngineers");
        Put(PatientKey(u,TreatmentDays),Bounded((long)I(PatientKey(u,TreatmentDays))+count));
        if(I("iMedicalLastDay")==0)Put("iMedicalLastDay",Day);
    }

    public static string Advance(DateTime date)
    {
        int target=TrainingPool.Day(date),last=I("iMedicalLastDay");
        if(last==0){Put("iMedicalLastDay",target);return "";}
        if(target<=last)return "";
        int returned=0;double cost=0;bool hadPatients=Total>0;
        for(int day=last+1;day<=target&&Total>0;day++){
            int patients=Total,limit=TreatableToday;
            int treated=0,todayReturned=0;
            // Ascending remaining days means moved patients cannot be processed twice.
            for(int d=1;d<=TreatmentDays&&treated<limit;d++){
                var group=new int[Units.Length];long groupTotal=0;for(int u=0;u<Units.Length;u++){group[u]=I(PatientKey(u,d));groupTotal+=group[u];}
                var chosen=Allocate(Math.Min(limit-treated,Bounded(groupTotal)),group);
                for(int u=0;u<Units.Length;u++){
                    int n=chosen[u];if(n==0)continue;Put(PatientKey(u,d),group[u]-n);treated+=n;
                    if(d>1)Put(PatientKey(u,d-1),Bounded((long)I(PatientKey(u,d-1))+n));
                    else{VariableSingleton.ChangeInt(Units[u],n);Put(ReturnedKey(u),Bounded((long)RecoveredFor(Units[u])+n));todayReturned+=n;}
                }
            }
            double dailyCost=SharedProvisions.Take(SharedProvisions.City,treated/20d);
            SharedProvisions.Set("medicalLastCost",dailyCost);
            Put("iMedicalReportDay",day);Put("iMedicalLastTreated",treated);Put("iMedicalLastReturned",todayReturned);Put("iMedicalLastCost",(int)Math.Floor(dailyCost));Put("iMedicalLastWaiting",patients-treated);
            returned+=todayReturned;cost+=dailyCost;
            if(treated==0)break;
        }
        Put("iMedicalLastDay",target);
        if(!hadPatients)return "";
        return "Infirmary: "+(returned>0?returned.ToString("N0")+" recovered patients return to duty. ":"")+
            Total.ToString("N0")+" remain wounded. Extra care used "+SharedProvisions.Format(cost)+" provisions."+
            (I("iMedicalLastWaiting")>0?" Treatment paused for "+I("iMedicalLastWaiting").ToString("N0")+" patients: insufficient monks or provisions.":"")+"\n\n";
    }

    public static bool RecruitMonks(out string report)
    {
        if(!TrainingPool.Place("iHealers",5,MonkCost)){report="You need 1,000 gold to call five monks into the medical service.";return false;}
        report="Five monks will join the medical service in five days. Gold −1,000. You may continue issuing orders.";return true;
    }
}
