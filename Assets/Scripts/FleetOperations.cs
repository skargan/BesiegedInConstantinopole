using System;
// Operational income has its own cadence and never consumes a narrative event slot.
public static class FleetOperations
{
    public const int ReportInterval=14;
    public static int CurrentInterval=>Math.Max(14,Math.Min(21,DailyLifeEvents.I("iFleetReportInterval",14)));
    public static int DaysRemaining=>CurrentInterval-DailyLifeEvents.I("iFleetReportProgress");
    public static void OrdersChanged(int mission)
    {
        VariableSingleton.SetInt("iFleetReportMission",mission);
        VariableSingleton.SetInt("iFleetReportProgress",0);
        VariableSingleton.SetInt("iFleetReportInterval",UnityEngine.Random.Range(14,22));
    }
    public static string CompleteDay()
    {
        int day=TrainingPool.Day(VariableSingleton.GetDate());
        if(DailyLifeEvents.I("iFleetReportDay")==day)return "";
        VariableSingleton.SetInt("iFleetReportDay",day);
        int mission=CouncilActions.Int("iFleet");
        if(CouncilActions.Int("iShips")<=0 || mission<0 || mission>4){OrdersChanged(mission);return "";}
        if(DailyLifeEvents.I("iFleetReportMission",-1)!=mission)OrdersChanged(mission);
        int progress=DailyLifeEvents.I("iFleetReportProgress")+1;
        VariableSingleton.SetInt("iFleetReportProgress",progress);
        if(progress<CurrentInterval)return "";
        VariableSingleton.SetInt("iFleetReportProgress",0);
        VariableSingleton.SetInt("iFleetReportInterval",UnityEngine.Random.Range(14,22));
        var before=VariableSingleton.Instance.CaptureState();
        RecurringEvents.FleetReport();
        return "Fleet operations — routine report\n"+EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState())+"\nNext routine report in "+CurrentInterval+" active days. Urgent reports and harbour defence do not wait for this report.\n\n";
    }
}
