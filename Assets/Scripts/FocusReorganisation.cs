using System;

public static class FocusReorganisation
{
    public static bool AdviserAvailable=>CourtRoster.InService(4)&&DailyLifeEvents.I("iHardOfficerReturn4")<=TrainingPool.Day(VariableSingleton.GetDate());
    public static int Cost=>AdviserAvailable?2000:5000;
    public static int DaysRemaining=>DailyLifeEvents.I("iFocusChanges")==0?0:Math.Max(0,DailyLifeEvents.I("iFocusChangedDay")+30-TrainingPool.Day(VariableSingleton.GetDate()));
    public static string Unavailable(int focus)
    {
        if(focus<0||focus>=FocusIcon.Names.Length)return "Unknown focus.";
        if(focus==CouncilActions.Int("iFocus"))return "This is your current focus.";
        if(DailyLifeEvents.I("iFocusChanges")>0&&!AdviserAvailable)return "A focus change has already been used. Further changes require Sphrantzes in service and available.";
        if(DaysRemaining>0)return "Another focus change is available in "+DaysRemaining+" days.";
        if(CouncilActions.Int("iMoney")<Cost)return "Requires "+Cost.ToString("N0")+" gold.";
        return null;
    }
    public static bool Change(int focus,out string report)
    {
        report=Unavailable(focus);if(report!=null)return false;
        int old=CouncilActions.Int("iFocus"),cost=Cost;
        VariableSingleton.ChangeInt("iMoney",-cost);
        VariableSingleton.SetInt("iFocus",focus);
        VariableSingleton.SetInt("iFocusChanges",DailyLifeEvents.I("iFocusChanges")+1);
        VariableSingleton.SetInt("iFocusChangedDay",TrainingPool.Day(VariableSingleton.GetDate()));
        report="Strategic focus changed from "+FocusIcon.Names[old]+" to "+FocusIcon.Names[focus]+".\n\n"+cost.ToString("N0")+" gold spent. Further changes require Sphrantzes in service and available, and at least 30 days since this change. Completed preparations retain their rewards; future focus-dependent choices use your new focus.";
        return true;
    }
}
