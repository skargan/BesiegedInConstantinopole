using System;
using UnityEngine;

public static class CourtServiceBenefits
{
    public static bool KjeldServing => GardenConversations.Available(GardenConversations.Kjold);
    public static bool KjeldCommands => KjeldServing && DailyLifeEvents.I("iCommander")==200;
    public static bool BodyguardAvailable => KjeldServing && DailyLifeEvents.I("iCommander")!=200;
    public static int GuardAllowance => KjeldCommands?10:5;
    public static int GuardTrainingDays => KjeldCommands?15:30;
    // Calendar months in preparation; consecutive seven-day windows from siege day one.
    public static int RecruitmentPeriod => VariableSingleton.HasSiegeStarted() ? 1000000+(VariableSingleton.SiegeDay-1)/7 : VariableSingleton.GetDate().Year*12+VariableSingleton.GetDate().Month;
    public static int GuardUsed => DailyLifeEvents.I("iLifeGuardPeriod")==RecruitmentPeriod?DailyLifeEvents.I("iLifeGuardRecruited"):0;
    public static int GuardRemaining => Math.Max(0,GuardAllowance-GuardUsed);
    public static string GuardAvailability => GuardRemaining+" of "+GuardAllowance+" available this "+(VariableSingleton.HasSiegeStarted()?"week":"month");
    public static void RecordGuardRecruitment(int count)
    {
        int used=GuardUsed;
        VariableSingleton.SetInt("iLifeGuardPeriod",RecruitmentPeriod);
        VariableSingleton.SetInt("iLifeGuardRecruited",used+count);
    }
    public static bool TryRescue()
    {
        bool rescued=BodyguardAvailable && UnityEngine.Random.Range(0,100)<50;
        VariableSingleton.SetInt("iLifeKjeldRescue",rescued?1:0);
        return rescued;
    }
    public const string RescueText="Kjeld reaches you before the assassin can strike. His guards close around you and drag the attacker away.";
    public static void Sermon(DateTime date)
    {
        int day=TrainingPool.Day(date);
        if(DailyLifeEvents.I("iLifeSermonDay")>=day)return;
        VariableSingleton.SetInt("iLifeSermonDay",day);
        if(!ChurchCourt.GennadiosAvailable)return;
        string[] keys={"fTroopRep","fPeopleRep","fPriestRep","fNobleRep"};
        int chosen=-1;
        for(int i=0;i<keys.Length;i++){
            bool lowest=true;
            for(int j=0;j<keys.Length;j++)if(i!=j && VariableSingleton.GetFloatVariable(keys[i])>VariableSingleton.GetFloatVariable(keys[j])-.1f+.00001f)lowest=false;
            if(lowest){chosen=i;break;}
        }
        if(chosen<0)chosen=UnityEngine.Random.Range(0,keys.Length);
        VariableSingleton.ChangePercentFloat(keys[chosen],DailyLifeEvents.I("iCommander")==203?.03f:.01f);
    }
}
