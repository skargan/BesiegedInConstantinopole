using System;
using System.Linq;
using UnityEngine;

// One saved planner. Regional logistics supplies the siege through its own adapter.
// Intelligence reads this plan; it never rolls a new one just because a menu opens.
public static class OttomanDirector
{
    public enum Strategy { RapidAssault, MethodicalAssault, GoldenHorn, Strangulation }
    public enum Temperament { Rash, Aggressive, Cautious, Balanced }
    public enum Operation { Reconnaissance, FillMoat, Bombard, Ramp, Tower, Ram, BalkanAssault, IrregularAssault, JanissaryAssault, Reinforce, ArrowScreens, OverlandShips, ChainBattery, NavalAssault, SabotageFood, Carcasses, Advance, GalataAttack, Infiltrate, Resupply, AwaitOrders }
    [Serializable] public class Rule {public string action;public int weight;}
    [Serializable] public class StrategyRules {public string strategy;public Rule[] buildup,operations;}
    [Serializable] public class Catalogue {public int version;public StrategyRules[] strategies;}
    static Catalogue catalogue;
    static int I(string key,int fallback=0)=>DailyLifeEvents.I("iOtt"+key,fallback);
    static void Set(string key,int value)=>VariableSingleton.SetInt("iOtt"+key,value);
    static int Own(string key)=>DailyLifeEvents.I(key);
    public static bool ExecutedThisTurn=>Active&&I("ExecutedTurn",-1)==VariableSingleton.Turn;
    public static bool Active=>I("DirectorVersion")==1;
    public static Strategy CurrentStrategy=>(Strategy)Mathf.Clamp(I("Strategy"),0,3);
    public static Temperament Personality=>(Temperament)Mathf.Clamp(I("Personality"),0,3);
    public static Operation Plan=>(Operation)Mathf.Clamp(I("Plan"),0,Enum.GetValues(typeof(Operation)).Length-1);
    public static bool BuildingUp=>VariableSingleton.CompletedSiegeDays<I("BuildUntil");
    public static readonly string[] StrategyNames={"Rapid assault","Methodical assault","Seize the Golden Horn","Strangulation"};
    public static readonly string[] OperationNames={"Reconnoitre the defences","Fill the moat","Bombard the walls","Extend the siege ramp","Build a siege tower","Build a battering ram","Balkan assault","Irregular assault","Janissary assault","Bring imperial reinforcements","Build arrow shields","Haul ships overland","Bombard the chain towers","Prepare a coastal assault","Sabotage the City's stores","Hurl carcasses over the walls","Enter the next defensive line","Attack the Galatan approaches","Send infiltrators into Constantinople","Secure the next supply convoy","Untangle delayed orders"};
    public static string StrategyReport=>StrategyNames[(int)CurrentStrategy]+" · "+(BuildingUp?"buildup":"operations")+" · "+Personality+" Mehmet";
    public static string BeliefReport=>"Approximately "+I("EstimateLand").ToString("N0")+" main-wall defenders; wall integrity about "+I("EstimateWall")+"%; coastal defence about "+I("EstimateSea")+". Confidence "+I("Confidence")+"%. These are Ottoman estimates and may be wrong.";
    public static string PlanReport=>Active&&I("PlanTurn",-1)==VariableSingleton.Turn?OperationNames[(int)Plan]+" (prepared for the coming Ottoman operation; disruption may force a change)":"The siege plan has not yet been established.";
    public static Catalogue Rules {get {if(catalogue==null){var text=Resources.Load<TextAsset>("Content/OttomanStrategies");if(text==null)throw new InvalidOperationException("Missing Ottoman strategy catalogue");catalogue=JsonUtility.FromJson<Catalogue>(text.text);Validate(catalogue);}return catalogue;}}
    public static void Validate(Catalogue c){if(c==null||c.version!=1||c.strategies==null||c.strategies.Length!=4)throw new InvalidOperationException("Four Ottoman strategies required");var names=new System.Collections.Generic.HashSet<string>();foreach(var s in c.strategies){if(!Enum.TryParse(s.strategy,out Strategy strategy)||!Enum.IsDefined(typeof(Strategy),strategy)||!names.Add(s.strategy))throw new InvalidOperationException("Invalid/duplicate Ottoman strategy");foreach(var table in new[]{s.buildup,s.operations})if(table==null||table.Length==0||table.Any(r=>r==null||r.weight<=0||(!Enum.TryParse(r.action,out Operation op)||!Enum.IsDefined(typeof(Operation),op)))||table.Sum(r=>r.weight)<=0)throw new InvalidOperationException("Invalid Ottoman operation table");}}
    public static void Ensure()
    {
        if(Active)return;
        Set("DirectorVersion",1);Set("Personality",UnityEngine.Random.Range(0,4));Set("Strategy",0);Set("StrategyStart",VariableSingleton.CompletedSiegeDays);Set("StrategySerial",1);Set("Reassess",VariableSingleton.CompletedSiegeDays+UnityEngine.Random.Range(14,29));Set("BuildUntil",VariableSingleton.CompletedSiegeDays+3);Set("PlanTurn",-1);Set("ExecutedTurn",-1);Set("LastObservationTurn",-1);Set("LastLandAssault",-100);Set("Ammo",30);Set("ArrowScreens",0);Set("HornWorks",0);Set("ChainDamage",0);Set("GrandCannon",0);Set("LastAssaultDead",0);Set("LastAssaultCommitted",0);
        Set("EstimateLand",Mathf.Max(1,Mathf.RoundToInt((DefencePosture.LandInfantry+DefencePosture.LandArchers)*UnityEngine.Random.Range(.55f,1.45f))));Set("EstimateWall",Mathf.Clamp(Mathf.RoundToInt(VariableSingleton.Instance.EnduringWall.GetHealth()*100*UnityEngine.Random.Range(.65f,1.35f)),5,100));Set("EstimateSea",Mathf.RoundToInt(DefencePosture.Strength*UnityEngine.Random.Range(.6f,1.4f)));Set("Confidence",15);
    }
    public static void PrepareTurn()
    {
        if(!VariableSingleton.HasSiegeStarted())return;Ensure();if(I("PlanTurn",-1)==VariableSingleton.Turn)return;
        Observe(false);
        int day=VariableSingleton.CompletedSiegeDays;
        if(day>=I("Reassess")||(CurrentStrategy==Strategy.GoldenHorn&&DefencePosture.HornOpen&&day>I("StrategyStart")+7))Reassess();
        var table=Rules.strategies.First(s=>s.strategy==CurrentStrategy.ToString());var weights=(BuildingUp?table.buildup:table.operations).Select(r=>new {Op=(Operation)Enum.Parse(typeof(Operation),r.action),Weight=Weight((Operation)Enum.Parse(typeof(Operation),r.action),r.weight)}).Where(r=>r.Weight>0&&Legal(r.Op)).ToArray();
        Operation chosen=Operation.Reconnaissance;
        if(weights.Length>0){int roll=UnityEngine.Random.Range(0,weights.Sum(r=>r.Weight));foreach(var r in weights){roll-=r.Weight;if(roll<0){chosen=r.Op;break;}}}
        if(VariableSingleton.Instance.EnduringWall.GetHealth()<=0)chosen=Operation.Advance;else if(Own("iFood")<=1)chosen=Operation.Resupply;else if(Own("iEnemyDelay")>0)chosen=Operation.AwaitOrders;
        Set("Plan",(int)chosen);Set("PlanTurn",VariableSingleton.Turn);
    }
    static void Reassess()
    {
        int[] weights={30,25,20,25};if(Personality==Temperament.Rash){weights[0]+=35;weights[3]-=15;}if(Personality==Temperament.Aggressive){weights[0]+=20;weights[1]+=10;}if(Personality==Temperament.Cautious){weights[1]+=15;weights[3]+=25;weights[0]-=15;}
        if(I("EstimateWall")<40)weights[0]+=25;if(DefencePosture.HornOpen)weights[2]=8;if(Own("iTurkShips")==0)weights[2]=0;if(Own("iTurkSpies")>5)weights[3]+=10;
        if(I("LastAssaultDead")>I("LastAssaultCommitted")*.6f&&I("LastAssaultCommitted")>0){weights[0]=Mathf.Max(5,weights[0]-20);weights[1]+=15;weights[3]+=10;}
        int roll=UnityEngine.Random.Range(0,weights.Sum());int pick=0;for(int i=0;i<4;i++){roll-=weights[i];if(roll<0){pick=i;break;}}
        int day=VariableSingleton.CompletedSiegeDays;Set("Strategy",pick);Set("StrategyStart",day);Set("StrategySerial",I("StrategySerial")+1);Set("Reassess",day+UnityEngine.Random.Range(pick==1?30:pick==0?14:21,pick==1?61:pick==0?29:43));Set("BuildUntil",day+BuildupDays(pick));
    }
    static int BuildupDays(int strategy){
        int days=UnityEngine.Random.Range(strategy==1?7:strategy==2?5:3,strategy==1?16:strategy==2?11:8);
        if(Personality==Temperament.Cautious)days+=strategy==1?7:3;
        if(I("LastAssaultCommitted")>0&&I("LastAssaultDead")>I("LastAssaultCommitted")*.6f)days+=strategy==0?4:7;
        return Mathf.Min(strategy==0?10:30,days);
    }
    static int Weight(Operation op,int weight)
    {
        bool attack=op==Operation.BalkanAssault||op==Operation.IrregularAssault||op==Operation.JanissaryAssault;
        if(attack){int estimate=I("EstimateLand");if(Personality==Temperament.Rash)weight*=2;if(Personality==Temperament.Aggressive)weight=Mathf.RoundToInt(weight*1.5f);if(Personality==Temperament.Cautious&&I("EstimateWall")>60)weight=Mathf.Max(1,weight/3);if(I("EstimateWall")<50)weight*=2;if(estimate>10000)weight=Mathf.Max(1,weight/2);if(VariableSingleton.CompletedSiegeDays-I("LastLandAssault")>=7)weight*=2;}
        return weight;
    }
    public static bool Legal(Operation op)
    {
        switch(op){case Operation.Resupply:return Own("iFood")<=1;case Operation.AwaitOrders:return Own("iEnemyDelay")>0;case Operation.FillMoat:return VariableSingleton.GetFloatVariable("fMoat")>0;case Operation.ChainBattery:return !DefencePosture.HornOpen&&Own("iTurkCannons")>0&&I("Ammo")>0;case Operation.Bombard:case Operation.Carcasses:return Own("iTurkCannons")>0&&I("Ammo")>0;
            case Operation.Tower:return !VariableSingleton.GetBoolVariable("bSiegeTower");case Operation.Ram:return !VariableSingleton.GetBoolVariable("bBatteringRam");case Operation.Ramp:return VariableSingleton.GetFloatVariable("fSiegeRamp")<1;case Operation.ArrowScreens:return I("ArrowScreens")==0;
            case Operation.BalkanAssault:return Own("iBalkans")>0&&VariableSingleton.CompletedSiegeDays-I("LastLandAssault")>=2;case Operation.IrregularAssault:return Own("iBazouks")>0&&VariableSingleton.CompletedSiegeDays-I("LastLandAssault")>=2;case Operation.JanissaryAssault:return Own("iJanissaries")>0&&VariableSingleton.CompletedSiegeDays-I("LastLandAssault")>=2;
            case Operation.OverlandShips:return Own("iTurkShips")>0&&!DefencePosture.HornOpen;case Operation.NavalAssault:return Own("iTurkShips")>0&&DailyLifeEvents.I("iSeaRaidDue")==0;case Operation.SabotageFood:return Own("iTurkSpies")>0&&Own("iProvisions")>0;case Operation.Infiltrate:return Own("iTurkSpies")<10;case Operation.GalataAttack:return PreparationCampaign.CanAttackGalata;case Operation.Advance:return VariableSingleton.Instance.EnduringWall.GetHealth()<=0;default:return true;}
    }
    public static void Observe(bool combat)
    {
        Ensure();if(!combat&&I("LastObservationTurn",-1)==VariableSingleton.Turn)return;Set("LastObservationTurn",VariableSingleton.Turn);
        float accuracy=Mathf.Clamp(.2f+Own("iTurkSpies")*.055f+(combat?.2f:0),.2f,.9f);Set("Confidence",Mathf.RoundToInt(accuracy*100));
        Set("EstimateLand",Estimate(I("EstimateLand"),DefencePosture.LandInfantry+DefencePosture.LandArchers,accuracy));Set("EstimateWall",Mathf.Clamp(Estimate(I("EstimateWall"),Mathf.RoundToInt(VariableSingleton.Instance.EnduringWall.GetHealth()*100),accuracy),0,100));Set("EstimateSea",Estimate(I("EstimateSea"),DefencePosture.Strength,accuracy));
    }
    static int Estimate(int previous,int actual,float accuracy){float report=actual*UnityEngine.Random.Range(1-(1-accuracy)*.65f,1+(1-accuracy)*.65f);return Mathf.Max(0,Mathf.RoundToInt(Mathf.Lerp(previous,report,.2f+accuracy*.6f)));}
    public static bool TryExecute(out Operation op){PrepareTurn();op=Plan;if(I("ExecutedTurn",-1)==VariableSingleton.Turn)return false;Set("ExecutedTurn",VariableSingleton.Turn);if(!Legal(op))op=Own("iTurkCannons")==0||I("Ammo")==0?Operation.Reinforce:Operation.Reconnaissance;return true;}
    public static void MarkAssault(){Set("LastLandAssault",VariableSingleton.CompletedSiegeDays);}
    public static void AfterAssault(int committed,int dead,bool tookLine=false){Set("LastAssaultCommitted",committed);Set("LastAssaultDead",dead);if(tookLine||(committed>0&&dead>committed*.6f))Set("Reassess",Mathf.Min(I("Reassess"),VariableSingleton.CompletedSiegeDays+1));Observe(true);}
    public static void UseAmmo()=>Set("Ammo",Mathf.Max(0,I("Ammo")-1));
    public static void AddAmmo(int count)=>Set("Ammo",I("Ammo")+count);
}
