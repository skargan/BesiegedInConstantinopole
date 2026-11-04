// Standalone logic checks: compiles the actual staged DailyLifeEvents.cs with narrow stubs.
using System;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine { public static class Random { public static int Range(int a,int b)=>a; } }
namespace Yarn.Unity {
    public class YarnCommandAttribute:Attribute {public YarnCommandAttribute(string s){} }
    public class YarnFunctionAttribute:Attribute {public YarnFunctionAttribute(string s){} }
}
public static class TrainingPool {public static int Day(DateTime date)=>(int)(date-new DateTime(1452,1,1)).TotalDays;}
public static class SpecialistPersonnel {public static bool BeginMentoring()=>true;public static void CompleteMentoring(){} }
public class VariableSingleton {
    public static VariableSingleton Instance=new VariableSingleton();
    public class Value {public string key;public int value;}
    public class CampaignSave {public List<Value> ints=new List<Value>();public int version=2;}
    public class Wall {public string GetCodeName()=>"OuterWall";}
    public Wall EnduringWall=new Wall();
    public static Dictionary<string,int> ints=new Dictionary<string,int>();
    public static Dictionary<string,float> floats=new Dictionary<string,float>();
    public static DateTime date=new DateTime(1453,4,7);public static bool siege=true;
    public static DateTime GetDate()=>date;
    public static bool HasSiegeStarted()=>siege;
    public static int GetIntVariable(string key)=>ints[key];
    public static void SetInt(string key,int val)=>ints[key]=val;
    public static void ChangeInt(string key,int val)=>ints[key]=DailyLifeEvents.I(key)+val;
    public static void ChangePercentFloat(string key,float val)=>floats[key]=(floats.ContainsKey(key)?floats[key]:0)+val;
}
public static class TavernChecks {
    static int checks;
    static void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
    public static int Main(){
        int day=TrainingPool.Day(VariableSingleton.GetDate());
        Check(DailyLifeEvents.Roots.Count(n=>n=="lifeTavern")==1,"One tavern root");
        VariableSingleton.siege=false;Check(!DailyLifeEvents.Available().Contains("lifeTavern"),"Not offered before siege");
        VariableSingleton.siege=true;Check(DailyLifeEvents.Available().Contains("lifeTavern"),"Offered in siege");
        DailyLifeEvents.Seen("lifeTavern");Check(DailyLifeEvents.I("iLifeCooldownlifeTavern")==day+30,"Thirty calendar day cooldown");
        Check(!DailyLifeEvents.Available().Contains("lifeTavern"),"Not offered during cooldown");
        DailyLifeEvents.TavernPledge();Check(DailyLifeEvents.I("iLifeTavernDue")==day+1,"Next day due");
        Check(DailyLifeEvents.DueNode==null,"No immediate morning reward");
        DailyLifeEvents.CompleteTavernPledge();Check(DailyLifeEvents.I("iInfantry")==0&&DailyLifeEvents.I("iLifeTavernDue")==day+1,"Early completion cannot award or consume");
        VariableSingleton.date=VariableSingleton.date.AddDays(1);
        DailyLifeEvents.TavernPledge();Check(DailyLifeEvents.I("iLifeTavernDue")==day+1,"Repeated schedule does not defer arrival");
        Check(DailyLifeEvents.DueNode=="lifeTavernPledge","Due followup selected");
        VariableSingleton.SetInt("iLifeSailsDue",day+1);Check(DailyLifeEvents.DueNode=="lifeFalseHope","Existing sails priority retained");
        VariableSingleton.SetInt("iLifeSailsDue",0);VariableSingleton.SetInt("iLifeBarrelDue",day+1);Check(DailyLifeEvents.DueNode=="lifeBarrelOwner","Existing barrel priority retained");
        VariableSingleton.SetInt("iLifeBarrelDue",0);VariableSingleton.SetInt("iLifeMentorDue",day+1);Check(DailyLifeEvents.DueNode=="lifeGunnerResult","Existing mentor priority retained");
        VariableSingleton.SetInt("iLifeMentorDue",0);
        var restored=new Dictionary<string,int>(VariableSingleton.ints);VariableSingleton.ints.Clear();VariableSingleton.ints=restored;
        Check(DailyLifeEvents.DueNode=="lifeTavernPledge","Due state needs no non-persistent local field");
        DailyLifeEvents.CompleteTavernPledge();
        Check(DailyLifeEvents.I("iInfantry")==5&&DailyLifeEvents.I("iArchers")==5,"Exactly five infantry and five archers");
        Check(Math.Abs(VariableSingleton.floats["fTroopRep"]-.01f)<.00001,"One army support point");
        Check(DailyLifeEvents.DueNode==null,"Followup removed after award");
        DailyLifeEvents.CompleteTavernPledge();Check(DailyLifeEvents.I("iInfantry")==5&&DailyLifeEvents.I("iArchers")==5,"Repeat completion cannot duplicate reward");
        VariableSingleton.SetInt("iMoney",12);DailyLifeEvents.TavernPay(25);Check(DailyLifeEvents.I("iMoney")==0,"Limited purse cannot go negative");
        DailyLifeEvents.TavernPay(20);Check(DailyLifeEvents.I("iMoney")==0,"Empty purse remains zero");
        VariableSingleton.SetInt("iMoney",100);DailyLifeEvents.TavernPay(25);Check(DailyLifeEvents.I("iMoney")==75,"Normal wager loss");
        DailyLifeEvents.TavernPay(-25);Check(DailyLifeEvents.I("iMoney")==75,"Invalid negative fee creates no gold");
        VariableSingleton.date=VariableSingleton.date.AddDays(29);Check(DailyLifeEvents.Available().Contains("lifeTavern"),"Root can repeat after thirty days");
        DailyLifeEvents.TavernPledge();Check(!DailyLifeEvents.Available().Contains("lifeTavern"),"Outstanding pledge blocks another visit even after cooldown");
        Console.WriteLine("TAVERN_LOGIC_CHECKS: "+checks+" passed. Actual staged C# with stubs; not Unity/runtime/Yarn compilation.");return 0;
    }
}
