#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Reflection;
using UnityEngine;
public static class ProvincialChecks
{
    static int count;
    static void Check(bool ok,string why){if(!ok)throw new Exception(why);count++;}
    static void Set(string suffix,int n)=>VariableSingleton.SetInt("iWbw"+suffix,n);
    static void Move(int n)=>typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn+n);
    static void Reload(){var saved=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));ProvincialIntelligence.ResetForRestore();VariableSingleton.Instance.RestoreValidated(saved);}
    public static int Run()
    {
        var original=VariableSingleton.Instance.CaptureState();var random=UnityEngine.Random.state;count=0;
        try {
            Move(1);VariableSingleton.SetInt("iSpies",7);VariableSingleton.SetInt("iMoney",20000);
            foreach(string s in new[]{"State","Due","Report","Ledger","Cooldown","SupplyLoss"})Set(s,0);
            Check(ProvincialIntelligence.Dispatch(out var report),"Cannot dispatch valid camp agent");
            Check(CouncilActions.Int("iSpies")==6&&ProvincialIntelligence.Assigned==1,"Assignment duplicates/loses camp agent");
            Check(CouncilActions.Int("iMoney")==19500,"Dispatch cost wrong");
            Check(!ProvincialIntelligence.Dispatch(out report)&&CouncilActions.Int("iSpies")==6,"Duplicate dispatch");
            Check(ProvincialIntelligence.Advance(VariableSingleton.GetDate().AddDays(6))=="","Early provincial arrival");
            Reload();Move(8);Check(ProvincialIntelligence.Advance(VariableSingleton.GetDate()).Contains("grain clerk"),"Arrival missing after reload");
            Check(ProvincialIntelligence.Advance(VariableSingleton.GetDate())=="","Arrival repeated");
            Check(ProvincialIntelligence.Begin(0,out report),"Ledger operation unavailable");
            int success=ProvincialIntelligence.I("Success"),lost=ProvincialIntelligence.I("Lost");Reload();
            Check(ProvincialIntelligence.I("Success")==success&&ProvincialIntelligence.I("Lost")==lost,"Reload rerolled operation");
            Check(!ProvincialIntelligence.Begin(0,out report),"Duplicate operation payment");
            Check(!ProvincialIntelligence.Recall(out report),"Recall bypasses committed risk");
            Set("Success",1);Set("Lost",0);int secrets=CouncilActions.Int("iSecrets");Move(11);
            Check(ProvincialIntelligence.Advance(VariableSingleton.GetDate()).Contains("One secret"),"Ledger result missing");
            Check(CouncilActions.Int("iSecrets")==secrets+1&&ProvincialIntelligence.I("Ledger")==1,"Ledger reward not applied");
            Reload();ProvincialIntelligence.Advance(VariableSingleton.GetDate());Check(CouncilActions.Int("iSecrets")==secrets+1,"Reload duplicates reward");
            Check(!ProvincialIntelligence.Begin(1,out report),"Cooldown not enforced");
            Move(38);Check(ProvincialIntelligence.OperationUnavailable(0)!=null,"Ledger can be farmed twice");
            Check(ProvincialIntelligence.Begin(1,out report),"Sabotage never unlocks");
            Set("Success",1);Set("Lost",0);int food=CouncilActions.Int("iFood");Move(42);
            Check(ProvincialIntelligence.Advance(VariableSingleton.GetDate()).Contains("forwarding yard"),"Depot result missing");
            Check(ProvincialIntelligence.GetSupplyPressure()==3&&CouncilActions.Int("iFood")==food,"Operation ticks food or loses pressure");
            Reload();Check(ImperialSupply.Deliver(15,out report)==12&&report.Contains("3 days"),"Convoy ignores provincial result");
            Check(ImperialSupply.Deliver(15,out report)==15&&report=="","One operation hits multiple convoys");
            Check(ImperialSupply.QueueDisruption(99)==6&&ImperialSupply.QueueDisruption(3)==0,"Disruption cap broken");
            Check(ImperialSupply.Deliver(0,out report)==0&&ImperialSupply.GetSupplyPressure()==6,"Empty convoy consumes operation");
            Check(ImperialSupply.Deliver(2,out report)==1&&ImperialSupply.GetSupplyPressure()==5,"Delivery creates negative stores");
            Check(ProvincialIntelligence.Recall(out report),"Resident cannot recall");
            Move(48);Check(ProvincialIntelligence.Advance(VariableSingleton.GetDate())==""&&CouncilActions.Int("iSpies")==6,"Agent returns early");
            Reload();Move(49);ProvincialIntelligence.Advance(VariableSingleton.GetDate());
            Check(CouncilActions.Int("iSpies")==7&&ProvincialIntelligence.Assigned==0,"Recall loses/duplicates agent");
            ProvincialIntelligence.Advance(VariableSingleton.GetDate());Check(CouncilActions.Int("iSpies")==7,"Repeated recall creates agent");
            Check(ProvincialIntelligence.Dispatch(out report),"Cannot dispatch after recall");Move(56);ProvincialIntelligence.Advance(VariableSingleton.GetDate());
            Move(68);Check(ProvincialIntelligence.Begin(1,out report),"Repeat sabotage unavailable after cooldown");
            Set("Success",0);Set("Lost",1);Move(72);ProvincialIntelligence.Advance(VariableSingleton.GetDate());
            Check(ProvincialIntelligence.State==ProvincialIntelligence.CellState.Lost&&CouncilActions.Int("iSpies")==6&&ProvincialIntelligence.Assigned==0,"Dead provincial agent deducted from camp twice");
            Check(!ProvincialIntelligence.Recall(out report),"Dead agent can return");
            Set("State",0);VariableSingleton.SetInt("iSpies",0);Check(!ProvincialIntelligence.Dispatch(out report),"Dispatch without agent");
            VariableSingleton.SetInt("iSpies",1);VariableSingleton.SetInt("iMoney",499);Check(!ProvincialIntelligence.Dispatch(out report),"Dispatch without passage money");
            Move(-1);Check(ProvincialIntelligence.DispatchUnavailable=="Available after the siege begins","Siege-only restriction broken");
            ProvincialIntelligence.ResetForRestore();Check(ProvincialIntelligence.Assigned==0&&ProvincialIntelligence.GetSupplyPressure()==0,"Old-save reset retains provincial state");
            return count;
        } finally {ProvincialIntelligence.ResetForRestore();VariableSingleton.Instance.RestoreValidated(original);UnityEngine.Random.state=random;}
    }
}
#endif
