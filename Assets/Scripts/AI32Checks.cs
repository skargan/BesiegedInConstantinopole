#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
public static class AI32Checks
{
    static int count;
    static void Check(bool ok,string text){if(!ok)throw new Exception("AI32 CHECK: "+text);count++;}
    static void Set(string key,int value)=>VariableSingleton.SetInt(key,value);
    static int I(string key)=>DailyLifeEvents.I(key);
    static void F(string key,float value)=>VariableSingleton.ChangePercentFloat(key,value-VariableSingleton.GetFloatVariable(key));
    static void Move(int day)=>typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,16+day);
    public static int Run()
    {
        count=0;var original=VariableSingleton.Instance.CaptureState();var rng=UnityEngine.Random.state;
        try{
            var broken=JsonUtility.FromJson<OttomanDirector.Catalogue>(JsonUtility.ToJson(OttomanDirector.Rules));broken.strategies[2].operations=new OttomanDirector.Rule[0];bool rejected=false;try{OttomanDirector.Validate(broken);}catch(InvalidOperationException){rejected=true;}Check(rejected,"empty action table rejected");
            OttomanDirector.Validate(OttomanDirector.Rules);Check(OttomanDirector.Rules.strategies.Length==4,"four populated catalogues");
            Move(0);Set("iOttDirectorVersion",0);OttomanDirector.PrepareTurn();Check(OttomanDirector.CurrentStrategy==OttomanDirector.Strategy.RapidAssault,"campaign opens with rapid assault");
            string plan=OttomanDirector.PlanReport;var state=UnityEngine.Random.state;float expected=UnityEngine.Random.value;UnityEngine.Random.state=state;
            for(int i=0;i<20;i++){OttomanDirector.PrepareTurn();Check(OttomanDirector.PlanReport==plan,"inspection preserves plan"+i);}
            Check(UnityEngine.Random.value==expected,"inspection preserves random stream");UnityEngine.Random.state=state;
            SaveGames.Checkpoint();string persistedPlan=OttomanDirector.PlanReport;int persistedPerson=I("iOttPersonality");Set("iOttPlan",0);Check(SaveGames.Load(VariableSingleton.TestSavePath,out var loadError)&&OttomanDirector.PlanReport==persistedPlan&&I("iOttPersonality")==persistedPerson,"actual archive restores plan and personality");
            int personality=I("iOttPersonality"),serial=I("iOttStrategySerial");Move(90);OttomanDirector.PrepareTurn();Check(I("iOttStrategySerial")==serial+1,"completed strategy reassesses");Check(I("iOttPersonality")==personality,"personality survives strategy change");
            var saved=VariableSingleton.Instance.CaptureState();string metadata=JsonUtility.ToJson(saved);OttomanDirector.TryExecute(out var operation);VariableSingleton.Instance.RestoreValidated(saved);Check(OttomanDirector.PlanReport==plan||I("iOttPlanTurn")==VariableSingleton.Turn,"saved turn plan retained");Check(saved.ints.Where(v=>v.key.StartsWith("iOtt")).All(v=>I(v.key)==v.value),"saved estimates retained");
            Check(OttomanDirector.TryExecute(out operation)&&!OttomanDirector.TryExecute(out operation),"operation executes once");
            Move(91);Set("iEnemyDelay",2);Set("iFood",50);AI.Instance.TakeAIAction();int food=I("iFood");AI.Instance.TakeAIAction();Check(I("iEnemyDelay")==1&&I("iFood")==food,"delays and enemy food not consumed twice");
            for(int i=0;i<10;i++){Set("iSpies",i);Check(EspionageSystem.IntelligenceValue(i)=="Unknown","intelligence threshold "+i);}
            Set("iSpies",10);Check(EspionageSystem.IntelligenceLabels[5]=="Enemy morale"&&EspionageSystem.IntelligenceValue(6)==OttomanDirector.PlanReport,"levels6and7");Check(EspionageSystem.IntelligenceValue(7).Contains("infiltrators")&&EspionageSystem.IntelligenceValue(8).Contains(OttomanDirector.StrategyNames[(int)OttomanDirector.CurrentStrategy]),"levels8and9");Check(EspionageSystem.IntelligenceValue(9).Contains("Reserved"),"level10 honest placeholder");
            foreach(string key in CourtRoster.Keys.Take(8))VariableSingleton.ChangeBool(key,false);VariableSingleton.ChangeBool("bTechRomanConcrete",false);
            Check(WallRepairRules.BaseMaterials("fOuterWall")==30&&WallRepairRules.BaseMaterials("fMiddleWall")==20&&WallRepairRules.BaseMaterials("fInnerWall")==10,"documented material rates");Check(WallRepairRules.Labour==500,"labour cost500");
            Set("iMoney",1000);Set("iMaterials",29);F("fOuterWall",.5f);Check(!WallRepairRules.TryRepair("fOuterWall",out var report)&&I("iMoney")==1000,"insufficient materials no partial debit");
            Set("iMaterials",30);Check(WallRepairRules.TryRepair("fOuterWall",out report)&&I("iMoney")==500&&I("iMaterials")==0,"labour and materials consumed once");Check(VariableSingleton.GetFloatVariable("fOuterWall")>=.5799f&&VariableSingleton.GetFloatVariable("fOuterWall")<=.6101f,"8to11repair points");
            VariableSingleton.ChangeBool("bTechRomanConcrete",true);Check(WallRepairRules.Materials("fOuterWall")==23&&WallRepairRules.Materials("fMiddleWall")==15&&WallRepairRules.Materials("fInnerWall")==8,"masonry discounts materials only");Check(WallRepairRules.Labour==500,"masonry leaves labour price unchanged");
            var screened=new PhasedAssault(10){Attackers=5000,EnemyLeft=5000,InitialArchers=2000,Archers=2000,ArcherBudget=1200,ArrowScreens=true,Cannons=1,Ammo=3,GunBudget=100};screened.Volley(0);int first=screened.ArcherApplied;screened.FireAtScreens();screened.Volley(0);Check(!screened.ArrowScreens&&screened.GunResolved&&screened.ArcherApplied-first>first,"cannon breaks screens and restores subsequent arrows");int dead=screened.Dead;screened.FireAtScreens();Check(screened.Dead==dead,"screens cannot yield repeated cannon damage");
            for(int j=0;j<20;j++){FleetOperations.OrdersChanged(1);Check(FleetOperations.CurrentInterval>=14&&FleetOperations.CurrentInterval<=21,"fleet cadence range "+j);}
            Check(PreparationCampaign.CareerCount==Enumerable.Range(0,Achievements.Names.Length).Count(AchievementPlatform.Earned),"one career and Ironman achievement source");
            // Evaluate every strategy/personality across wall bands using the actual saved planner.
            int attacks=0;for(int seed=0;seed<32;seed++){
                UnityEngine.Random.InitState(32000+seed);Set("iOttDirectorVersion",0);OttomanDirector.Ensure();Set("iOttPersonality",1+seed%3);Set("iOttStrategy",seed%4);Set("iOttBuildUntil",0);Set("iOttReassess",999);Set("iTurkCannons",5);Set("iTurkShips",48);Set("iBalkans",20000);Set("iBazouks",50000);Set("iJanissaries",10000);Set("iTurkSpies",seed%11);Set("iProvisions",5000);Set("iOttAmmo",999);Set("iEnemyDelay",0);Set("iFood",75);
                for(int day=0;day<60;day++){
                    Move(day);F("fOuterWall",new[]{.9f,.65f,.35f,.1f}[day%4]);Set("iOttPlanTurn",-1);Set("iOttExecutedTurn",-1);OttomanDirector.PrepareTurn();Check(OttomanDirector.Legal(OttomanDirector.Plan),"legal planned operation "+seed+"/"+day);if(OttomanDirector.Plan>=OttomanDirector.Operation.BalkanAssault&&OttomanDirector.Plan<=OttomanDirector.Operation.JanissaryAssault)attacks++;
                }
            }
            Check(attacks>100,"land strategies produce assault plans without empty wall bands");
            Debug.Log("AI32_PLANNER_SAMPLE 1920 turns; assault plans="+attacks);return count;
        }finally{VariableSingleton.Instance.RestoreValidated(original);UnityEngine.Random.state=rng;}
    }
}
#endif
