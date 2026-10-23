using System;
// Operational income has its own cadence and never consumes a narrative event slot.
public static class FleetOperations
{
    public const int ReportInterval=14;
    public static int DaysRemaining=>ReportInterval-DailyLifeEvents.I("iFleetReportProgress");
    public static void OrdersChanged(int mission)
    {
        VariableSingleton.SetInt("iFleetReportMission",mission);
        VariableSingleton.SetInt("iFleetReportProgress",0);
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
        if(progress<ReportInterval)return "";
        VariableSingleton.SetInt("iFleetReportProgress",0);
        var before=VariableSingleton.Instance.CaptureState();
        RecurringEvents.FleetReport();
        return "Fleet operations — fortnightly report\n"+EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState())+"\nNext routine report in 14 active days. Urgent reports and harbour defence do not wait for this report.\n\n";
    }
}
