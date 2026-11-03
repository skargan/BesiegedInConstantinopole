using System;
using System.Linq;
using UnityEngine;

// Compatibility boundary for existing preparation, intelligence and turn-flow callers.
// Old automatic rebellion and finite wave simulation are replaced, not run in parallel.
public static class ProvinceCampaign
{
    public static readonly int[] FoodQuota={24,24,24,15,10,12,6,10,10,9,6};
    static readonly string[] Edges={"thrace|bulgaria","thrace|macedonia","bulgaria|macedonia","bithynia|karasi","bithynia|germiyan","bithynia|anatolia","bithynia|candar","karasi|aydin","karasi|germiyan","aydin|germiyan","aydin|mentese","mentese|germiyan","germiyan|anatolia","anatolia|candar","anatolia|armeniakon","candar|armeniakon"};
    public static int Get(string k)=>DailyLifeEvents.I("iWbw34"+k);
    public static void Set(string k,int v)=>VariableSingleton.SetInt("iWbw34"+k,v);
    public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    public static int Authority(string id)=>ProvincialWar.Find(id).Authority;
    public static void Authority(string id,int value){var p=ProvincialWar.Find(id);p.Authority=value;if(ProvincialWar.Ready)ProvincialWar.Collapse(p);}
    public static int Resistance(string id)=>Mathf.Clamp(Get("R_"+id),0,100);
    public static bool Free(string id)=>ProvincialWar.Ready?ProvincialWar.Find(id).Owner==ProvincialWar.Imperial:DailyLifeEvents.I("iWbwCaptured_"+id)>0;
    public static bool Rebellion(string id)=>ProvincialWar.Ready?ProvincialWar.Find(id).Rebels.Troops>0:Get("Battle_"+id)>0;
    public static int Agent(string id)=>Get("Agent_"+id);
    public static int Assigned=>WarTheatre.ProvinceIds.Count(id=>Agent(id)>0);
    public static bool Neighbour(string a,string b)=>Edges.Contains(a+"|"+b)||Edges.Contains(b+"|"+a);
    public static string Name(string id)=>WarTheatre.Find(id)?.name??id;
    public static void Ensure()=>ProvincialWar.Ensure();
    public static string Status(string id)
    {
        var p=ProvincialWar.Find(id);int day=ProvincialWar.Ready?ProvincialWar.I("LastDay"):Day;
        string rule=p.Owner==ProvincialWar.Chaos?"Chaos · "+Mathf.Max(0,p.Get("ChaosUntil")-day)+" days remain":p.Owner==ProvincialWar.Imperial?"Imperial control":p.Owner>=ProvincialWar.AllyOwnerOffset?CountryDiplomacy.Definitions[p.Owner-ProvincialWar.AllyOwnerOffset].Name+" controls this province":p.Get("Settlement")>0?"Settlement awaiting your decision":p.Get("BattleStage")>0?"Battle awaiting your command":Rebellion(id)?"Rebellion in progress":"Ottoman-held";
        string agent=Agent(id)==2?"Resident imperial agent":Agent(id)==1?"Agent travelling · "+Mathf.Max(0,Get("Arrival_"+id)-Day)+" days":Agent(id)==3?"Agent returning · "+Mathf.Max(0,Get("Arrival_"+id)-Day)+" days":"No resident agent";
        return rule+" · "+agent;
    }
    public static string DispatchReason(string id)
    {
        Ensure();return !VariableSingleton.HasSiegeStarted()?"Available during the siege":Agent(id)>0?"Agent already assigned":Get("Blocked_"+id)>Day?"Contacts temporarily closed":CouncilActions.Int("iSpies")<1?"Requires one agent":CouncilActions.Int("iMoney")<500?"Requires 500 gold":null;
    }
    public static bool Dispatch(string id,out string report)
    {
        report=DispatchReason(id);if(report!=null)return false;VariableSingleton.ChangeInt("iSpies",-1);VariableSingleton.ChangeInt("iMoney",-500);Set("Agent_"+id,1);Set("Arrival_"+id,Day+7);report="Your agent travels to "+Name(id)+"; arrival in seven days. Exact enemy intelligence becomes available on arrival.";return true;
    }
    public static bool Recall(string id,out string report)
    {
        report="Requires a resident agent.";if(Agent(id)!=2)return false;Set("Agent_"+id,3);Set("Arrival_"+id,Day+7);report="Your agent begins the seven-day return journey. Exact local enemy intelligence is no longer available; the rebel army remains.";return true;
    }
    public static void Mode(string id,int mode){if(Agent(id)==2)Set("Mode_"+id,Mathf.Clamp(mode,0,1));}
    public static int PendingFood(string id)=>ProvincialWar.Find(id).Config.FoodDays;
    public static string Reason(string id,int op)
    {
        Ensure();var p=ProvincialWar.Find(id);
        return !p.Hostile?"Province is no longer Ottoman-held":Agent(id)!=2?"Requires a resident agent":op==1&&p.Get("Plans")>0?"Enemy plans already acquired":op==2&&Authority(id)>70?"Requires Authority at 70 or below":CouncilActions.Int("iMoney")<(op==0?500:1000)?"Insufficient gold":null;
    }
    public static bool Act(string id,int op,out string report)
    {
        report=Reason(id,op);if(report!=null||op<0||op>2)return false;
        if(op==1)return ProvincialWar.Plans(id,out report);
        VariableSingleton.ChangeInt("iMoney",op==0?-500:-1000);var p=ProvincialWar.Find(id);int amount=ProvincialWar.Roll(1,10);
        if(op==0){p.Authority-=amount;report=Name(id)+": Ottoman Authority falls by "+amount+" points.";ProvincialWar.Collapse(p);}
        else {int before=p.Resistance;p.Resistance+=amount;report=Name(id)+": Resistance gains "+(p.Resistance-before)+" points.";}
        return true;
    }
    // Kept for old diagnostic code only; gameplay settlements now require the explicit event.
    public static string Resolve(string id,bool rebels,int day)=>"Legacy automatic rebellion resolution has been replaced by Who Shall Govern?";
    public static string Advance(DateTime date)=>ProvincialWar.Advance(date);
    public static string TakeDeliveryReport()=>"";
    public static string SupplyDay()=>ProvincialWar.SupplyDay();
}
