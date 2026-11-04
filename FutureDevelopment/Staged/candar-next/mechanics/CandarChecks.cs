using System;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine {
 public static class Mathf {
  public static int Clamp(int n,int a,int b)=>Math.Max(a,Math.Min(b,n));public static float Clamp(float n,float a,float b)=>Math.Max(a,Math.Min(b,n));
  public static int Max(int a,int b)=>Math.Max(a,b);public static int Min(int a,int b)=>Math.Min(a,b);public static int RoundToInt(float n)=>(int)Math.Round(n);public static float Lerp(float a,float b,float t)=>a+(b-a)*t;
 }
 public static class Random {public static int Calls;public static Func<int,int,int> Choose=(a,b)=>a;public static int Range(int a,int b){Calls++;int v=Choose(a,b);if(v<a||v>=b)throw new Exception("Invalid roll");return v;}public static float Range(float a,float b)=>a;}
 public class TextAsset {public string text="";}
 public static class Resources {public static T Load<T>(string id) where T:new()=>new T();}
 public static class JsonUtility {public static T FromJson<T>(string text)=>(T)(object)new OttomanDirector.Catalogue{version=1,strategies=Enum.GetNames(typeof(OttomanDirector.Strategy)).Select(n=>new OttomanDirector.StrategyRules{strategy=n,buildup=new[]{new OttomanDirector.Rule{action="Reconnaissance",weight=1}},operations=new[]{new OttomanDirector.Rule{action="Reconnaissance",weight=1}}}).ToArray()};}
}
public static class DailyLifeEvents {public static int I(string key,int fallback=0)=>VariableSingleton.Ints.ContainsKey(key)?VariableSingleton.Ints[key]:fallback;}
public class VariableSingleton {
 public static Dictionary<string,int> Ints=new Dictionary<string,int>();public static Dictionary<string,bool> Bools=new Dictionary<string,bool>();public static Dictionary<string,float> Floats=new Dictionary<string,float>();
 public static DateTime Now;public static int Turn,CompletedSiegeDays;public static bool Siege=true;public static VariableSingleton Instance=new VariableSingleton();public Wall EnduringWall=new Wall();
 public static DateTime GetDate()=>Now;public static bool HasSiegeStarted()=>Siege;public static void SetInt(string k,int v)=>Ints[k]=v;
 public static int GetIntVariable(string k)=>DailyLifeEvents.I(k);public static void ChangeInt(string k,int d)=>Ints[k]=Math.Max(0,DailyLifeEvents.I(k)+d);
 public static bool GetBoolVariable(string k)=>Bools.ContainsKey(k)&&Bools[k];public static float GetFloatVariable(string k)=>Floats.ContainsKey(k)?Floats[k]:0;
 public class Wall {public float health=1;public float GetHealth()=>health;}
}
public static class CountryDiplomacy {public class Definition {public string Id;}public static Definition[] Definitions={new Definition{Id="ragusa"}};}
public static class CourtRoster {public static bool Orhan;public static bool InService(int id)=>id==3&&Orhan;}
public static class PopulationAccounting {public static void Add(int n)=>VariableSingleton.ChangeInt("iPopulation",n);}
public static class SpecialistPersonnel {public static void CommitLocalManpower(int n)=>VariableSingleton.ChangeInt("iSpecialistManpowerDrawn",n);}
public static class TrainingPool {public static int Day(DateTime date)=>(int)(date.Ticks/TimeSpan.TicksPerDay);}
public static class ProvinceCampaign {public static void Ensure(){}}
public static class SharedProvisions {public const string Camp="iFood";public static double Stock(string key)=>DailyLifeEvents.I(key);public static double OttomanDemand=>100;}
public static class DefencePosture {public static int LandInfantry=>1000;public static int LandArchers=>1000;public static int Strength=>100;public static bool HornOpen=>false;}
public static class PreparationCampaign {public static bool CanAttackGalata=>true;}
public static class CouncilActions {public static int SupplyCalls;public static string SupplyReport="";public static string ResolveEnemySupplies(){SupplyCalls++;VariableSingleton.ChangeInt("iFood",-100);return SupplyReport;}}
public static class OttomanSiegeOperations {public static List<OttomanDirector.Operation> Executed=new List<OttomanDirector.Operation>();public static string Execute(AI ai,OttomanDirector.Operation op){Executed.Add(op);return "Executed "+op;}}
public partial class AI {
 public object PendingPhased;public bool WasAssault,AwaitingBreachDecision;public string LastReport;public int Surprises;
 public Dummy assaultUI=new Dummy(),enemyLossDisplay=new Dummy(),assaultSystem=new Dummy();
 public class Dummy {public void SetAssaultResultButton(bool v){}public void SetLossesInfo(int a,int b){}public void ClearLossDisplay(){}}
 public void AIBashiBazoukAssault(){Surprises++;}
}
public static class CandarChecks {
 static int checks;static DateTime start=new DateTime(1453,4,6);
 static int I(string k)=>DailyLifeEvents.I(k);static void Put(string k,int n)=>VariableSingleton.SetInt(k,n);
 static void Check(bool value,string message){checks++;if(!value)throw new Exception(message);}
 static void Reset(){VariableSingleton.Ints.Clear();VariableSingleton.Bools.Clear();VariableSingleton.Floats.Clear();VariableSingleton.Now=start;VariableSingleton.Turn=1;VariableSingleton.CompletedSiegeDays=1;VariableSingleton.Siege=true;VariableSingleton.Instance.EnduringWall.health=1;CourtRoster.Orhan=false;UnityEngine.Random.Calls=0;UnityEngine.Random.Choose=(a,b)=>a;CouncilActions.SupplyCalls=0;CouncilActions.SupplyReport="";OttomanSiegeOperations.Executed.Clear();Put("iMoney",20000);Put("iPopulation",100000);Put("iFood",10000);Put("iTurkShips",50);Put("iTurkCannons",5);Put("iOttAmmo",30);VariableSingleton.Floats["fMoat"]=1;Put("iOttDirectorVersion",1);Put("iOttPersonality",1);Put("iOttReassess",10000);Put("iOttBuildUntil",10000);Put("iOttExecutedTurn",-1);Put("iOttPlanTurn",-1);}
 static void Unlock(){VariableSingleton.Bools["bStraitsOpen"]=true;CourtRoster.Orhan=true;string text;Check(CandarDiplomacy.OpenNegotiations(out text),"Open access");}
 static void Plan(OttomanDirector.Operation op){Put("iOttPlan",(int)op);Put("iOttPlanTurn",VariableSingleton.Turn);}
 static void Next(int day){VariableSingleton.Now=start.AddDays(day);VariableSingleton.Turn++;VariableSingleton.CompletedSiegeDays++;}
 public static int Main(){string report;Reset();
  Check(!CandarDiplomacy.OpenNegotiations(out report),"Locked without both prerequisites");VariableSingleton.Bools["bStraitsOpen"]=true;
  Check(!CandarDiplomacy.OpenNegotiations(out report),"Open straits alone insufficient");VariableSingleton.Bools["bStraitsOpen"]=false;CourtRoster.Orhan=true;
  Check(!CandarDiplomacy.OpenNegotiations(out report),"Orhan alone insufficient");
  Check(!CandarDiplomacy.Ransom(out report)&&!CandarDiplomacy.PurchaseSecret(out report)&&!CandarDiplomacy.Disrupt(out report)&&I("iMoney")==20000&&UnityEngine.Random.Calls==0,"Locked transactions cannot charge or roll");
  Unlock();Check(I("iMoney")==20000&&VariableSingleton.Ints.Where(p=>p.Key.StartsWith("iDiplomacyCandar")).Count()==1,"Opening changes only unlock state, no claim/rebellion/aid/alliance");
  Check(!CandarDiplomacy.OpenNegotiations(out report),"Repeat opening blocked");Put("iMoney",2999);
  Check(!CandarDiplomacy.Ransom(out report)&&I("iMoney")==2999&&UnityEngine.Random.Calls==0&&I("iDiplomacyCandarRansomDue")==0,"Unaffordable ransom does not mutate state or RNG");Put("iMoney",3000);
  Check(CandarDiplomacy.Ransom(out report)&&I("iMoney")==0,"Exact ransom funds accepted and charged once");
  Check(I("iInfantry")==20&&I("iArchers")==15&&I("iPopulation")==100150,"Guaranteed minimum rows and total population once");
  Check(I("iCavalry")==1&&I("iEngineers")==1&&I("iHealers")==1&&I("iEnvoys")==1&&I("iVarangians")==1,"Independent successful optional rows");
  Check(I("iSpecialistManpowerDrawn")==4&&UnityEngine.Random.Calls==16,"Only specialist/guard volunteers additionally committed; eight independent rows");
  int draws=UnityEngine.Random.Calls;Put("iMoney",20000);Check(!CandarDiplomacy.Ransom(out report)&&UnityEngine.Random.Calls==draws&&I("iMoney")==20000,"Same-day ransom cannot repeat");Next(6);Check(CandarDiplomacy.RansomReason!=null,"Day6 still replenishing");Next(7);
  UnityEngine.Random.Choose=(a,b)=>a==0&&b==100?0:b-1;Check(CandarDiplomacy.Ransom(out report),"Day7 replenished");
  Check(I("iInfantry")==80&&I("iArchers")==45&&I("iPopulation")==100450&&I("iMoney")==17000,"Inclusive maxima charged and added once");
  Check(I("iCavalry")==6&&I("iEngineers")==6&&I("iHealers")==6&&I("iEnvoys")==2&&I("iVarangians")==4,"Optional inclusive maxima");
  var threshold=CandarDiplomacy.RollRansom((a,b)=>b-1);Check(threshold.Infantry==60&&threshold.Archers==30&&threshold.Population==300&&threshold.Cavalry+threshold.Engineers+threshold.Monks+threshold.Envoy+threshold.Guards==0,"Guaranteed rows survive optional failures");
  var fifty=CandarDiplomacy.RollRansom((a,b)=>a==0&&b==100?50:a);var fortyNine=CandarDiplomacy.RollRansom((a,b)=>a==0&&b==100?49:a);Check(fifty.Cavalry+fifty.Engineers+fifty.Monks==0&&fortyNine.Cavalry==1&&fortyNine.Engineers==1&&fortyNine.Monks==1,"50percent boundary is exact for each specialist/cavalry row");
  var five=CandarDiplomacy.RollRansom((a,b)=>a==0&&b==100?5:a);var four=CandarDiplomacy.RollRansom((a,b)=>a==0&&b==100?4:a);Check(five.Envoy==0&&four.Envoy==1,"Envoy fivepercent boundary exact");
  var ten=CandarDiplomacy.RollRansom((a,b)=>a==0&&b==100?10:a);var nine=CandarDiplomacy.RollRansom((a,b)=>a==0&&b==100?9:a);Check(ten.Guards==0&&nine.Guards==1,"Guard tenpercent boundary exact");
  Reset();Unlock();Put("iPopulation",int.MaxValue-299);draws=UnityEngine.Random.Calls;Check(!CandarDiplomacy.Ransom(out report)&&I("iMoney")==20000&&UnityEngine.Random.Calls==draws,"Overflow preflight prevents partial purchase");
  Reset();Unlock();Put("iMoney",1999);Check(!CandarDiplomacy.PurchaseSecret(out report)&&I("iDiplomacyCandarSecretMask")==0&&UnityEngine.Random.Calls==0,"Unaffordable secret leaves stock and RNG");Put("iMoney",6000);var names=new HashSet<string>();
  for(int n=0;n<3;n++){Check(CandarDiplomacy.PurchaseSecret(out report),"Secret purchase succeeds");names.Add(report.Split('\n')[0]);}
  Check(names.Count==3&&I("iSecrets")==3&&I("iMoney")==0&&CandarDiplomacy.SecretsRemaining==0,"Three distinct finite secrets");Put("iMoney",10000);draws=UnityEngine.Random.Calls;Check(!CandarDiplomacy.PurchaseSecret(out report)&&I("iMoney")==10000&&UnityEngine.Random.Calls==draws,"Fourth secret cannot charge or reroll");
  Reset();Unlock();Put("iMoney",2499);Check(!CandarDiplomacy.Disrupt(out report)&&!CandarDiplomacy.DelayPending,"Unaffordable disruption grants no credit");Put("iMoney",2500);Check(CandarDiplomacy.Disrupt(out report)&&I("iMoney")==0&&CandarDiplomacy.DelayPending,"Disruption purchases one credit");Put("iMoney",20000);
  Check(!CandarDiplomacy.Disrupt(out report)&&I("iMoney")==20000,"No overlapping same-day purchase");Next(7);Check(!CandarDiplomacy.Disrupt(out report)&&I("iMoney")==20000,"Pending credit cannot stack after cooldown");
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);var ai=new AI();Plan(OttomanDirector.Operation.FillMoat);ai.TakeAIAction();
  Check(OttomanSiegeOperations.Executed.Count==0&&CandarDiplomacy.DelayPending&&ai.LastReport.Contains("postponed"),"Actual AI action postponed before execution");
  Check(CouncilActions.SupplyCalls==1&&I("iFood")==9900&&VariableSingleton.Now==start&&I("iTurkShips")==50,"Supply path still runs; disruption changes neither calendar nor navy");ai.TakeAIAction();Check(CouncilActions.SupplyCalls==1,"Same turn cannot consume or execute twice");
  VariableSingleton.Ints=new Dictionary<string,int>(VariableSingleton.Ints);Next(1);OttomanDirector.PrepareTurn();Check(OttomanDirector.Plan==OttomanDirector.Operation.FillMoat,"Actual planner restores saved deferred operation next calendar day");ai.TakeAIAction();
  Check(OttomanSiegeOperations.Executed.SequenceEqual(new[]{OttomanDirector.Operation.FillMoat})&&!CandarDiplomacy.DelayPending,"Deferred operation executes once and clears");ai.TakeAIAction();Check(OttomanSiegeOperations.Executed.Count==1,"No double execution on return day");Next(2);ai.TakeAIAction();Check(OttomanSiegeOperations.Executed.Count==2,"Normal action resumes next day");
  Next(6);Check(!CandarDiplomacy.Disrupt(out report),"Disruption day6 cooldown");Next(7);Check(CandarDiplomacy.Disrupt(out report),"Disruption day7 available after previous action");
  foreach(var op in new[]{OttomanDirector.Operation.NavalAssault,OttomanDirector.Operation.OverlandShips,OttomanDirector.Operation.ChainBattery,OttomanDirector.Operation.Resupply,OttomanDirector.Operation.AwaitOrders,OttomanDirector.Operation.Advance,OttomanDirector.Operation.Reinforce})Check(!CandarDiplomacy.DelayOperation(op,out report)&&CandarDiplomacy.DelayPending,"Excluded operation preserves credit: "+op);
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);ai=new AI();Plan(OttomanDirector.Operation.NavalAssault);ai.TakeAIAction();Check(OttomanSiegeOperations.Executed.Single()==OttomanDirector.Operation.NavalAssault&&CandarDiplomacy.DelayPending,"Naval action proceeds through actual AI hook");
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);ai=new AI();Plan(OttomanDirector.Operation.FillMoat);Put("iEnemyDelay",1);ai.TakeAIAction();Check(I("iEnemyDelay")==0&&I("iDiplomacyCandarDelayCredit")==1&&I("iDiplomacyCandarDeferred")==0,"Existing broad delay is processed first without spending Candar credit");
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);ai=new AI();Plan(OttomanDirector.Operation.FillMoat);CouncilActions.SupplyReport="Waiting for provisions";ai.TakeAIAction();Check(I("iDiplomacyCandarDelayCredit")==1&&ai.LastReport=="Waiting for provisions","Supply emergency consumes no paid tactical credit");
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);ai=new AI();Plan(OttomanDirector.Operation.FillMoat);Put("iLifeSurprise",1);Put("iBazouks",50);ai.TakeAIAction();Check(ai.Surprises==1&&I("iDiplomacyCandarDelayCredit")==1,"Immediate surprise consequence retains its priority");
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);ai=new AI();Plan(OttomanDirector.Operation.FillMoat);ai.TakeAIAction();Next(1);VariableSingleton.Instance.EnduringWall.health=0;OttomanDirector.PrepareTurn();Check(OttomanDirector.Plan==OttomanDirector.Operation.Advance,"Fallen-wall priority precedes deferred order");ai.TakeAIAction();Check(CandarDiplomacy.DelayPending,"Advance does not silently consume deferred order");
  Next(2);VariableSingleton.Instance.EnduringWall.health=1;Put("iFood",20);OttomanDirector.PrepareTurn();Check(OttomanDirector.Plan==OttomanDirector.Operation.Resupply&&CandarDiplomacy.DelayPending,"Current shared-food threshold precedes deferred order");
  Next(3);Put("iFood",10000);Put("iEnemyDelay",1);OttomanDirector.PrepareTurn();Check(OttomanDirector.Plan==OttomanDirector.Operation.AwaitOrders&&CandarDiplomacy.DelayPending,"Broad operation delay precedes deferred order");
  Next(4);Put("iEnemyDelay",0);OttomanDirector.PrepareTurn();Check(OttomanDirector.Plan==OttomanDirector.Operation.FillMoat,"Deferred legal order survives external emergencies");ai.TakeAIAction();Check(!CandarDiplomacy.DelayPending,"Recovered order clears after actual eligible execution");
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);ai=new AI();Plan(OttomanDirector.Operation.FillMoat);ai.TakeAIAction();Next(1);VariableSingleton.Floats["fMoat"]=0;OttomanDirector.PrepareTurn();Check(OttomanDirector.Plan==OttomanDirector.Operation.Reconnaissance&&!CandarDiplomacy.DelayPending,"No-longer-legal deferred order discarded without forcing obsolete action");
  Reset();Unlock();CandarDiplomacy.Disrupt(out report);VariableSingleton.Siege=false;ai=new AI();ai.TakeAIAction();Check(CouncilActions.SupplyCalls==0&&I("iDiplomacyCandarDelayCredit")==1,"Preparation phase does not consume purchased future delay");
  Console.WriteLine("CANDAR_CHECKS: "+checks+" passed; actual backend, patched full OttomanDirector and extracted actual AI.TakeAIAction; remaining services stubbed.");return 0;
 }
}
