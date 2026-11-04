using System;
using System.Linq;
using System.Collections.Generic;
namespace UnityEngine {
    public static class Mathf {public static int Clamp(int n,int a,int b)=>Math.Max(a,Math.Min(b,n));public static int Min(int a,int b)=>Math.Min(a,b);public static int Max(int a,int b)=>Math.Max(a,b);}
    public static class Random {
        public static Queue<int> Rolls=new Queue<int>();public static int Calls;
        public static int Range(int a,int b){Calls++;int n=Rolls.Count>0?Rolls.Dequeue():a;if(n<a||n>=b)throw new Exception("Roll outside requested range");return n;}
    }
}
namespace Yarn.Unity {public class YarnCommandAttribute:Attribute{public YarnCommandAttribute(string s){}}public class YarnFunctionAttribute:Attribute{public YarnFunctionAttribute(string s){}}}
public static class DailyLifeEvents {public static int I(string key,int fallback=0)=>VariableSingleton.Ints.ContainsKey(key)?VariableSingleton.Ints[key]:fallback;}
public class VariableSingleton {
    public static bool IsInitialized=>true;
    public static VariableSingleton Instance=new VariableSingleton();public static Dictionary<string,int> Ints=new Dictionary<string,int>();
    public List<TrainingPool.Order> TrainingOrders=new List<TrainingPool.Order>();public static DateTime Now=new DateTime(1452,1,1);
    public static DateTime GetDate()=>Now;public static void SetInt(string k,int n)=>Ints[k]=n;
    public static void ChangeInt(string k,int delta){RecruitmentLogistics.RecordLoss(k,Math.Min(DailyLifeEvents.I(k),Math.Max(0,-delta)));Ints[k]=DailyLifeEvents.I(k)+delta;}
    public static void ChangePercentFloat(string key,float delta){}
}
public static class CountryDiplomacy {
    public static float goodwill;public static void Ensure(){}public static int Index(string id)=>id=="ragusa"?25:-1;
    public static float Goodwill(int i)=>goodwill;public static float ChangeGoodwill(int i,float points){goodwill=Math.Max(0,Math.Min(95,goodwill+points));return points;}
    public static string Advance(DateTime date)=>"";
}
public static class RagusaText {public static string Get(string key,string fallback="")=>fallback;}
public static class Achievements {public static int losses;public static void TroopLoss(string key,int lost)=>losses+=lost;}
public static class PopulationAccounting {public static int People=>100000;}
public static class MedicalRecovery {public const int RecruitmentDays=7,MonkCost=100;public static int RegularWounded=>0;public static int Total=>0;public static string Advance(DateTime date)=>"";}
public static class MonasticResearch {public static void AllocationsForDay(DateTime date){}public static int ResearchMonks=>0;public static int Advance(DateTime date)=>0;public static string DiscoveryReport(int discoveries,bool active)=>"";}
public static class SpecialistPersonnel {
    public const int EngineerTrainingDays=10,EnvoyTrainingDays=10,EngineerPrice=100,EnvoyPrice=100,ShipRepairEngineers=2,ShipRepairManpower=10,ShipRepairPrice=250,ShipRepairMaterials=5,ShipRepairDays=5,ShipCrew=50;
    public static int LocalManpowerCommitted=>0;public static int AvailableDamagedShips=>0;public static int Available=>10;
    public static bool CanStart(string u,int q,out string reason){reason=null;return true;}public static int MaximumOrder(string u,int q)=>q;
    public static int EngineersPerUnit(string u)=>1;public static int OrdinaryManpowerPerUnit(string u)=>1;
    public static void EnsureInitialized(){}public static void ReserveForOrder(TrainingPool.Order o){}public static void CommitLocalManpower(int n){}
    public static string Advance(DateTime date)=>"";
}
public static class CouncilActions {public class Ally{public string Name;}public static Ally[] Allies={new Ally{Name="Legacy aid"}};public static int Int(string key)=>DailyLifeEvents.I(key);}
public static class AidRouting {public static bool BySea(int i)=>true;}
public static class NavalState {public static void Sync(){}public static int SinkShips(int n){int loss=Math.Min(n,DailyLifeEvents.I("iShips"));VariableSingleton.ChangeInt("iShips",-loss);return loss;}}
public static class RagusaChecks {
    static int checks;static DateTime start=new DateTime(1452,1,1);
    static void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
    static int I(string k)=>DailyLifeEvents.I(k);static void Put(string k,int v)=>VariableSingleton.SetInt(k,v);
    static void Reset(){VariableSingleton.Ints.Clear();TrainingPool.Orders.Clear();DecisionJournal.Entries.Clear();UnityEngine.Random.Rolls.Clear();UnityEngine.Random.Calls=0;CountryDiplomacy.goodwill=25;Achievements.losses=0;VariableSingleton.Now=start;Put("iMoney",20000);Put("iProvisions",200);Put("iInfantry",1000);Put("iArchers",1000);Put("iCavalry",400);Put("iShips",5);Put("iTurkShips",48);Put("iFleet",3);RecruitmentLogistics.BeginCampaign();}
    static string Move(int days){VariableSingleton.Now=start.AddDays(days);string s=TrainingPool.CompleteDueOrders(VariableSingleton.Now);return s+RagusaDiplomacy.Advance(VariableSingleton.Now);}
    public static int Main(){
        Reset();Check(RagusaDiplomacy.ActiveRepresentative==0&&RagusaDiplomacy.Favour(0)==50&&RagusaDiplomacy.Favour(1)==50,"Gazulus opens January; separate favours");
        VariableSingleton.Now=new DateTime(1452,2,1);Check(RagusaDiplomacy.ActiveRepresentative==1,"Cotrugli February");
        VariableSingleton.Now=new DateTime(1453,1,1);Check(RagusaDiplomacy.ActiveRepresentative==0,"Annual monthly parity");
        RagusaDiplomacy.ChangeFavour(0,70);Check(RagusaDiplomacy.Favour(0)==100&&RagusaDiplomacy.Favour(1)==50,"Independent capped favour");
        RagusaDiplomacy.ChangeFavour(1,-99);Check(RagusaDiplomacy.Favour(1)==0&&RagusaDiplomacy.Relations==25,"Favour separate from country goodwill");
        Check(UnityEngine.Random.Calls==0,"Viewing/alternation consumes no campaign RNG");
        Reset();string report;
        Check(RagusaDiplomacy.RequestFood(out report)&&report.Contains("cannot possibly")&&report.Contains("14 Jan")==false,"Comic refusal commits purchase");
        Check(I("iMoney")==17500&&I("iProvisions")==200&&I("iShips")==5,"Price charged now; cargo and ships not gifted now");
        Check(UnityEngine.Random.Calls==0,"No invented food refusal RNG");
        Check(!RagusaDiplomacy.RequestFood(out report)&&I("iMoney")==17500,"Only one arrangement per calendar month");
        Move(13);Check(I("iProvisions")==200,"Food not early");Move(14);Check(I("iProvisions")==700&&!RagusaDiplomacy.FoodAtSea&&I("iShips")==5,"Food arrives after fourteen days without ships");
        Move(14);Check(I("iProvisions")==700,"No duplicate food delivery");Move(31);Check(RagusaDiplomacy.RequestFood(out report),"Next calendar month available");
        Put("iAidState0",1);Put("iAidDays0",20);Check(ReliefStormSystem.Incoming().Contains(RagusaDiplomacy.FoodStormTarget)&&ReliefStormSystem.Incoming().Contains(0),"Private and state aid coexist in storm targets");
        ReliefStormSystem.Apply(1,RagusaDiplomacy.FoodStormTarget,1,0);Check(I("iAidDays0")==20&&I("iDiplomacyRagusaFoodDue")==TrainingPool.Day(start)+55,"Storm delays only targeted cargo by ten days");
        Move(45);Check(I("iProvisions")==700,"Original arrival cancelled by storm delay");Move(55);Check(I("iProvisions")==1200,"Delayed cargo arrives once");
        Move(60);Check(RagusaDiplomacy.RequestFood(out report),"March arrangement allowed");ReliefStormSystem.Apply(2,RagusaDiplomacy.FoodStormTarget,1,0);Move(100);Check(I("iProvisions")==1200&&!RagusaDiplomacy.FoodAtSea&&RagusaDiplomacy.FoodStatus.Contains("lost"),"Catastrophic storm permanently cancels cargo");
        Reset();Put("iMoney",2499);Check(!RagusaDiplomacy.RequestFood(out report)&&I("iDiplomacyRagusaFoodMonth")==0,"Insufficient gold does not consume monthly arrangement");
        Reset();Put("iCavalry",199);Check(!RagusaDiplomacy.StartContract(out report)&&I("iInfantry")==1000,"Unavailable contingent cannot partially deploy");
        Reset();int free=RecruitmentLogistics.AvailableManpower;
        Check(RagusaDiplomacy.StartContract(out report),"Contract accepted");
        Check(I("iInfantry")==500&&I("iArchers")==500&&I("iCavalry")==200&&I("iMoney")==25000,"Troops removed and signing payment delivered");
        Check(RagusaDiplomacy.CommittedTroops==1200&&RecruitmentLogistics.AvailableManpower==free,"Away troops remain reserved in recruitment pool");
        Check(RecruitmentLogistics.Losses("iInfantry")==0&&Achievements.losses==0,"Departure is not a death");
        Check(RagusaDiplomacy.Relations==30&&RagusaDiplomacy.Favour(0)==55,"Signing relation/favour gains");
        Check(!RagusaDiplomacy.StartContract(out report)&&I("iMoney")==25000,"Concurrent contract prevented");
        Move(89);Check(UnityEngine.Random.Calls==0&&TrainingPool.Orders.Count==0,"No early contract roll");
        foreach(int roll in new[]{0,1,20,20})UnityEngine.Random.Rolls.Enqueue(roll);
        Move(90);Check(TrainingPool.Orders.Count==3&&TrainingPool.Orders.All(TrainingPool.IsValidOrder),"Three valid saved return orders");
        Check(TrainingPool.Orders.Select(o=>o.quantity).SequenceEqual(new[]{495,400,160}),"Victory loss endpoints 1 to20percent per troop type");
        Check(RecruitmentLogistics.Queued==1055&&RagusaDiplomacy.CommittedTroops==0,"Returning troops counted once in logistics");
        Check(I("iMoney")==35000&&RagusaDiplomacy.Relations==40&&RagusaDiplomacy.Favour(0)==65&&RagusaDiplomacy.Favour(1)==50,"Victory pays original representative despite monthly change");
        Check(Achievements.losses==145&&RecruitmentLogistics.Losses("iInfantry")==5,"Real losses recorded once");
        Check(TrainingPool.Orders.All(o=>TrainingPool.OrderName(o).Contains("Ragusan")),"Return orders clearly labelled");
        int calls=UnityEngine.Random.Calls;Move(90);Check(UnityEngine.Random.Calls==calls&&TrainingPool.Orders.Count==3&&I("iMoney")==35000,"Repeat day cannot reroll/pay/queue twice");
        var save=new Dictionary<string,int>(VariableSingleton.Ints);var orders=TrainingPool.Orders.Select(o=>o.Copy()).ToList();VariableSingleton.Ints.Clear();TrainingPool.Orders.Clear();VariableSingleton.Ints=save;TrainingPool.Orders.AddRange(orders);Move(91);
        Check(UnityEngine.Random.Calls==calls&&TrainingPool.Orders.Count==3,"Restored saved state preserves fixed result");
        Move(103);Check(I("iInfantry")==500,"Survivors do not teleport home");Move(104);
        Check(I("iInfantry")==995&&I("iArchers")==900&&I("iCavalry")==360&&TrainingPool.Orders.Count==0&&!RagusaDiplomacy.ContractActive,"Survivors return after fourteen days");
        Move(104);Check(I("iInfantry")==995&&I("iMoney")==35000,"Repeated return day has no duplicate reward");
        Check(DecisionJournal.Entries.Count==3,"Decision/result/return journal records deduplicate");
        Reset();RagusaDiplomacy.StartContract(out report);foreach(int roll in new[]{1,50,1,50})UnityEngine.Random.Rolls.Enqueue(roll);Move(90);
        Check(TrainingPool.Orders.Select(o=>o.quantity).SequenceEqual(new[]{250,495,100})&&I("iMoney")==25000,"Defeat 1to50percent losses with no victory payment");
        Check(RagusaDiplomacy.Relations==30,"Defeat adds no unapproved relation penalty");
        Reset();RagusaDiplomacy.StartContract(out report);foreach(int roll in new[]{0,10,10,10})UnityEngine.Random.Rolls.Enqueue(roll);for(int d=1;d<=105;d++)Move(d);
        Check(!RagusaDiplomacy.ContractActive&&TrainingPool.Orders.Count==0&&I("iInfantry")==950,"Daily catch-up handles ninety-day result and return within long span");
        Reset();RagusaDiplomacy.StartContract(out report);foreach(int roll in new[]{0,10,10,10})UnityEngine.Random.Rolls.Enqueue(roll);
        var jan=start;var feb=new DateTime(1452,2,1);var mar=new DateTime(1452,3,1);var apr=new DateTime(1452,4,1);var may=new DateTime(1452,5,1);
        VariableSingleton.Now=feb;CampaignPersonnelCalendar.Advance(jan,feb);VariableSingleton.Now=mar;CampaignPersonnelCalendar.Advance(feb,mar);
        Check(I("iMoney")==25000&&UnityEngine.Random.Calls==0&&RagusaDiplomacy.CommittedTroops==1200,"Preparation month-jumps do not pay victory before day90");
        VariableSingleton.Now=apr;string april=CampaignPersonnelCalendar.Advance(mar,apr);
        Check(I("iMoney")==35000&&I("iInfantry")==500&&TrainingPool.Orders.Count==3&&april.Contains("victory"),"April month-jump shows outcome payment but no early return");
        Check(TrainingPool.Orders.All(o=>o.readyDay==TrainingPool.Day(start)+104),"Return based on logical day90 plus14, not month-end");
        Check(DecisionJournal.Entries.Single(e=>e.id=="ragusa-contract-result-1").date==new DateTime(1452,3,31).ToString("d MMM yyyy"),"March31 result retains logical date despite April1 campaign date");
        VariableSingleton.Now=may;string mayReport=CampaignPersonnelCalendar.Advance(apr,may);
        Check(I("iInfantry")==950&&TrainingPool.Orders.Count==0&&mayReport.Contains("surviving Ragusan contingent"),"May jump completes April14 return and preserves visible report");
        Check(UnityEngine.Random.Calls==4&&Achievements.losses==120,"Month jumps resolve exactly once and record only true casualties");
        Check(DecisionJournal.Entries.Single(e=>e.id=="ragusa-contract-return-1").date==new DateTime(1452,4,14).ToString("d MMM yyyy"),"April14 return retains logical date despite May1 campaign date");
        Check(DecisionJournal.Entries.Single(e=>e.id=="ragusa-contract-1").date==start.ToString("d MMM yyyy"),"Existing Record API still records signing on current date");
        DecisionJournal.RecordAt(start,"ragusa-contract-return-1","Duplicate","Must not replace original");
        Check(DecisionJournal.Entries.Count==3&&DecisionJournal.Copy().Single(e=>e.id=="ragusa-contract-return-1").date==new DateTime(1452,4,14).ToString("d MMM yyyy"),"Dated entries preserve deduplication and saved copy dates");
        Reset();RagusaDiplomacy.RequestFood(out report);VariableSingleton.Now=feb;CampaignPersonnelCalendar.Advance(jan,feb);
        Check(DecisionJournal.Entries.Single(e=>e.id=="ragusa-food-arrival-1").date==start.AddDays(14).ToString("d MMM yyyy"),"Private cargo arrival keeps January15 through February jump");
        Reset();SharedProvisions.Set(SharedProvisions.City,200.125);RagusaDiplomacy.RequestFood(out report);Move(13);
        Check(Math.Abs(SharedProvisions.Stock(SharedProvisions.City)-200.125)<.000001,"Shared fractional stock unchanged before cargo arrives");
        Move(14);Check(Math.Abs(SharedProvisions.Stock(SharedProvisions.City)-700.125)<.000001,"Cargo uses shared stock and preserves fractions");
        Move(14);Check(Math.Abs(SharedProvisions.Stock(SharedProvisions.City)-700.125)<.000001,"Shared cargo delivery occurs exactly once");
        Console.WriteLine("RAGUSA_CHECKS: "+checks+" passed; actual staged mechanics/TrainingPool/RecruitmentLogistics/ReliefStormSystem with narrow stubs. Unity, UI and actual save-file verification pending.");return 0;
    }
}
