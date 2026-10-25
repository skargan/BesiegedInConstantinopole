using System;
using UnityEngine;

// Stable wbw.thessaloniki.v1 cell. Existing campaign dictionaries persist every assignment and roll.
public static class ProvincialIntelligence
{
    public enum CellState { Unassigned, Travelling, Resident, Operating, Returning, Lost }
    public const int TravelDays=7,RepeatDays=30;
    // Zero in Version32 archives denotes the original Thessaloniki/Macedonia cell.
    public static string ProvinceId=>I("Province")==0?"macedonia":WarTheatre.CanonicalProvince(WarTheatre.SavedProvinceIds[Mathf.Clamp(I("Province")-1,0,WarTheatre.SavedProvinceIds.Length-1)]);
    public static string ProvinceName=>WarTheatre.Find(ProvinceId).name;
    public static int Ledger=>Mathf.Max(I("Ledger_"+ProvinceId),ProvinceId=="macedonia"?I("Ledger"):ProvinceId=="aydin"?I("Ledger_saruhan"):0);
    public static int I(string suffix)=>DailyLifeEvents.I("iWbw"+suffix);
    static void Set(string suffix,int value)=>VariableSingleton.SetInt("iWbw"+suffix,value);
    public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    public static CellState State=>(CellState)I("State");
    public static int Remaining=>Mathf.Max(0,I("Due")-Day);
    public static int Cooldown=>Mathf.Max(0,I("Cooldown")-Day);
    public static int Assigned=>State==CellState.Travelling||State==CellState.Resident||State==CellState.Operating||State==CellState.Returning?1:0;
    public static int GetSupplyPressure()=>ImperialSupply.GetSupplyPressure();
    // Call before applying a restored save, so older saves cannot inherit a newer cell.
    public static void ResetForRestore()
    {
        foreach(var pair in VariableSingleton.Instance.CaptureState().ints)
            if(pair.key.StartsWith("iWbw",StringComparison.Ordinal))VariableSingleton.SetInt(pair.key,0);
    }
    public static string Status=>State==CellState.Travelling?"Travelling to "+ProvinceName+" · "+Remaining+" days":State==CellState.Resident?"Resident in "+ProvinceName:State==CellState.Operating?"Operation in progress · "+Remaining+" days":State==CellState.Returning?"Returning to the camp · "+Remaining+" days":State==CellState.Lost?"The last agent was lost":"No provincial agent assigned";
    public static string LastReport {
        get {
            switch(I("Report")) {
                case 1:return "A letter arrives folded inside a merchant's account. Your agent has reached the provincial contacts. A grain clerk will talk, provided the names of his family never reach the Sultan's men.";
                case 2:return "The clerk's figures tell a second story beneath the first: missing grain, invented carts, a captain paid twice. Your agent sends copies. One secret gained; the depot's routine is now known.";
                case 3:return "Before dawn, the forwarding yard burns. The miller saves his daughter; the quartermaster saves his seal. Neither saves the grain. The loss will be felt when the next convoy reaches the siege camp.";
                case 4:return "The rendezvous is empty. A boy delivers the agreed sign, but it has been drawn backwards. Your agent abandons the operation and remains in hiding. Nothing has been gained.";
                case 5:return "A merchant returns with a ring sewn into his cuff. The agent who wore it will send no more letters. The grain clerk and his family have vanished from their lodgings.";
                case 6:return "Your agent returns, thinner and quieter. The provincial assignment is closed; one agent is restored to the camp network.";
                default:return "No report received. Send an agent to establish provincial contacts.";
            }
        }
    }
    public static string DispatchUnavailable=>!VariableSingleton.HasSiegeStarted()?"Available after the siege begins":Assigned>0?"Recall or finish the current assignment first":CouncilActions.Int("iSpies")<1?"Requires one camp agent":CouncilActions.Int("iMoney")<500?"Requires 500 gold":null;
    public static bool Dispatch(out string report)=>Dispatch("macedonia",out report);
    public static bool Dispatch(string province,out string report)
    {
        province=WarTheatre.CanonicalProvince(province);
        int index=Array.IndexOf(WarTheatre.SavedProvinceIds,province);
        if(index<0){report="Unknown province.";return false;}
        report=DispatchUnavailable;if(report!=null)return false;
        Set("Province",index+1);VariableSingleton.ChangeInt("iSpies",-1);VariableSingleton.ChangeInt("iMoney",-500);
        Set("State",(int)CellState.Travelling);Set("Due",Day+TravelDays);Set("Report",0);
        report="One agent leaves the Ottoman camp network for "+ProvinceName+". Travel: 7 calendar days. Passage and contacts: 500 gold. Camp intelligence falls immediately; the agent cannot serve in both places.";return true;
    }
    public static string OperationUnavailable(int operation)
    {
        if(operation<0||operation>1)return "Unknown operation";
        if(State!=CellState.Resident)return "Requires a resident agent";
        if(Cooldown>0)return "Contacts need "+Cooldown+" more days before another operation";
        if(operation==0&&Ledger>0)return "The depot ledger has already been obtained";
        if(operation==1&&GetSupplyPressure()>=ImperialSupply.MaximumDisruption)return "A successful disruption is already awaiting delivery";
        return CouncilActions.Int("iMoney")<(operation==0?1000:1500)?"Insufficient gold":null;
    }
    public static bool Begin(int operation,out string report)
    {
        report=OperationUnavailable(operation);if(report!=null)return false;
        VariableSingleton.ChangeInt("iMoney",operation==0?-1000:-1500);
        Set("Operation",operation);Set("Success",UnityEngine.Random.Range(0,100)<(operation==0?75:65+(Ledger>0?15:0))?1:0);
        Set("Lost",I("Success")==0&&UnityEngine.Random.Range(0,100)<25?1:0);
        Set("State",(int)CellState.Operating);Set("Due",Day+(operation==0?3:4));Set("Cooldown",Day+RepeatDays);
        report=operation==0?"Your agent approaches the grain clerk. Payment: 1,000 gold; report expected in 3 days. Success can reveal one secret and improve a later depot operation.":"Your agent prepares to sabotage the forwarding depot. Payment: 1,500 gold; report expected in 4 days. Success reduces the next Ottoman convoy by three days of stores. Failure can cost the agent.";
        return true;
    }
    public static bool Recall(out string report)
    {
        report="Recall is available after arrival, once any operation has finished.";
        if(State!=CellState.Resident)return false;
        Set("State",(int)CellState.Returning);Set("Due",Day+TravelDays);
        report="The recall is sent. Your agent needs 7 calendar days to return and will rejoin the camp network only on arrival.";return true;
    }
    public static string Advance(DateTime date)
    {
        int day=TrainingPool.Day(date);
        if(I("Due")==0||day<I("Due"))return "";
        if(State==CellState.Travelling){Set("State",(int)CellState.Resident);Set("Report",1);}
        else if(State==CellState.Returning){Set("State",(int)CellState.Unassigned);VariableSingleton.ChangeInt("iSpies",1);Set("Report",6);}
        else if(State==CellState.Operating) {
            if(I("Success")>0) {
                if(I("Operation")==0){Set("Ledger_"+ProvinceId,1);VariableSingleton.ChangeInt("iSecrets",1);Set("Report",2);}
                else {ImperialSupply.QueueDisruption(3);Set("Report",3);}
                Set("State",(int)CellState.Resident);
            } else {Set("State",(int)(I("Lost")>0?CellState.Lost:CellState.Resident));Set("Report",I("Lost")>0?5:4);}
        } else return "";
        Set("Due",0);return "\n\nWar Beyond the Walls · "+ProvinceName+"\n"+LastReport+"\n";
    }
}
