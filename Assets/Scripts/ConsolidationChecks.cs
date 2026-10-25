#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;
public static class ConsolidationChecks
{
    static int checks;
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
    static void Set(string key,int value)=>VariableSingleton.SetInt(key,value);
    static int I(string key)=>CouncilActions.Int(key);
    public static int Run()
    {
        checks=0;var original=VariableSingleton.Instance.CaptureState();
        try {
            Set("iPrepDone8",0);Set("iCommander",0);
            VariableSingleton.ChangePercentFloat("fTroopRep",.5f-VariableSingleton.GetFloatVariable("fTroopRep"));
            PreparationCampaign.Complete("story14");Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fTroopRep")-.6f)<.001f,"Commander morale missing");
            PreparationCampaign.Complete("story14");Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fTroopRep")-.6f)<.001f,"Commander reward repeated");
            Check(SurrenderTerms.DefensiveLoss("InnerWall",100)>SurrenderTerms.DefensiveLoss("OuterWall",1),"Fresh inner wall improves bargaining over outer line");
            Check(SurrenderTerms.DefensiveLoss("MiddleWall",100)>=SurrenderTerms.DefensiveLoss("OuterWall",0),"Falling back erases pressure");
            Check(Mathf.Abs(SurrenderTerms.DefensiveLoss("InnerWall",0)-100)<.001f,"Final defence loss not total");
            Check(SurrenderTerms.DefensiveLoss("OuterWall",100)==0,"Intact defence counts as lost");
            Check(AidRouting.BySea(6)&&CouncilActions.Allies[6].Ships==0,"Naples sea route tied to awarded ships");
            Check(!AidRouting.BySea(0)&&!AidRouting.BySea(4),"Land expeditions exposed to storms");
            for(int i=0;i<CouncilActions.Allies.Length;i++)Set("iAidState"+i,0);
            Set("iMoney",10000);Set("iShips",0);string appeal;
            Check(CouncilActions.RequestAid(0,out appeal)&&I("iMoney")==9000,"Land appeal incorrectly requires ships");
            Check(!CouncilActions.RequestAid(1,out appeal)&&I("iMoney")==9000,"Sea appeal proceeds without available ship");
            Set("iAidState6",1);Set("iAidDays6",20);Set("iAidWarships6",0);
            Set("iAidState0",1);Set("iAidDays0",20);Set("iAidWarships0",0);
            Check(ReliefStormSystem.Incoming().Count==1&&ReliefStormSystem.Incoming()[0]==6,"Storm targets incorrect transport routes");
            ReliefStormSystem.Apply(1,6,1,0);Check(I("iAidDays6")==30&&I("iAidDays0")==20,"Storm delay hits land aid");
            ReliefStormSystem.Apply(2,0,1,0);Check(I("iAidState0")==1,"Storm destroys land army");
            Set("iAidState1",1);Set("iAidDays1",1);Set("iAidWarships1",0);int ships=I("iShips"),inf=I("iInfantry");
            CouncilActions.AdvanceAid(1);Check(I("iShips")==ships&&I("iInfantry")==inf+CouncilActions.Allies[1].Infantry,"Transports become player warships");
            CouncilActions.AdvanceAid(0);Check(I("iInfantry")==inf+CouncilActions.Allies[1].Infantry,"Aid delivered twice");
            Set("iAidState2",1);Check(AidRouting.Warships(2)==CouncilActions.Allies[2].Ships,"Older pending ship promise lost");
            Set("iAidWarships2",3);Check(AidRouting.Warships(2)==3,"Explicit warship quantity ignored");
            Set("iShips",10);Set("iFleet",1);FleetOperations.OrdersChanged(1);int money=I("iMoney");Set("iFleetReportInterval",14);
            for(int i=0;i<13;i++){Set("iFleetReportDay",0);Check(FleetOperations.CompleteDay()=="","Fleet rewards before14active days");}
            Check(I("iMoney")==money,"Early fleet income");Set("iFleetReportDay",0);
            Check(FleetOperations.CompleteDay().Contains("routine report")&&I("iMoney")>money,"Fleet report not delivered");
            int delivered=I("iMoney");Check(FleetOperations.CompleteDay()==""&&I("iMoney")==delivered,"Same day repeated fleet reward");
            FleetOperations.OrdersChanged(2);Check(FleetOperations.DaysRemaining>=14&&FleetOperations.DaysRemaining<=21,"New mission inherits completed progress");
            Set("iShips",0);Set("iFleetReportDay",0);Check(FleetOperations.CompleteDay()==""&&FleetOperations.DaysRemaining>=14&&FleetOperations.DaysRemaining<=21,"No fleet produces rewards");
            Check(!RecurringEvents.Eligible(18),"Fleet report remains in random pool");
            var saved=VariableSingleton.Instance.CaptureState();Set("iAidWarships2",9);Restore(saved);Check(AidRouting.Warships(2)==3,"Warship commitment did not restore");
            Restore(original);
            Set("iLifeSailsDue",TrainingPool.Day(VariableSingleton.GetDate()));Set("iLifeSailsResult",0);
            Check(DailyLifeEvents.DueNode!=null&&VariableSingleton.Instance.StoryScheduled(),"Milestone collision fixture invalid");
            UnityEngine.Object.FindObjectOfType<RestUI>(true).ContinueToEvent();
            var controller=UnityEngine.Object.FindObjectOfType<DialogueController>(true);
            string selected=(string)typeof(DialogueController).GetField("startNode",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(controller);
            Check(selected==PreparationCampaign.FixedNode(VariableSingleton.Turn),"Due narrative displaced fixed milestone");
            Check(DailyLifeEvents.DueNode!=null,"Milestone erased queued follow-up");
            return checks;
        } finally {Restore(original);}
    }
    static void Restore(VariableSingleton.CampaignSave state)=>typeof(VariableSingleton).GetMethod("RestoreState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(VariableSingleton.Instance,new object[]{state});
}
#endif
