using TMPro;
using UnityEngine;

public partial class CouncilUI
{
    public void AddProvincialNavigation(float top){BodyHeight(top+90);var row=SpyPanel("Beyond the siege lines",top,78);CampaignHUD.MakeButton(row.transform,"War Beyond the Walls",new Vector2(.02f,.12f),new Vector2(.98f,.88f),WarTheatreUI.Show);}
    string provincialSelection="macedonia";
    public void ProvincialOperationsFor(string province){provincialSelection=province;ProvincialOperations();}
    public void ProvincialOperations()
    {
        ProvinceCampaign.Ensure();string id=provincialSelection;var p=ProvincialWar.Find(id);
        Open(p.Name+" · Provincial network","Agents reveal enemy strength and support the local resistance.");ControlSingleton.Instance.ReturnAction=()=>{Hide();ProvincialConflictUI.Show(id);};BodyHeight(550);
        var status=SpyPanel("Province",0,85);
        CampaignHUD.Label(status.transform,"Authority "+p.Authority+" · Resistance "+p.Resistance+" · Wealth "+p.Wealth+"%",new Vector2(.03f,.50f),new Vector2(.97f,.97f),25);
        CampaignHUD.Label(status.transform,ProvinceCampaign.Status(id),new Vector2(.03f,.03f),new Vector2(.97f,.48f),20);
        var travel=SpyPanel("Agent orders",90,90);
        CampaignHUD.Label(travel.transform,"One resident per province · seven days each way · "+CouncilActions.Int("iSpies")+" camp agents available",new Vector2(.03f,.57f),new Vector2(.97f,.97f),20);
        var send=CampaignHUD.MakeTurnButton(travel.transform,"Send agent · 500 gold",new Vector2(.03f,.05f),new Vector2(.49f,.52f),()=>{bool ok=ProvinceCampaign.Dispatch(id,out var text);Finish(ok,text);});Availability(send,ProvinceCampaign.DispatchReason(id));
        var recall=CampaignHUD.MakeTurnButton(travel.transform,"Recall agent",new Vector2(.51f,.05f),new Vector2(.97f,.52f),()=>{bool ok=ProvinceCampaign.Recall(id,out var text);Finish(ok,text);});recall.interactable=ProvinceCampaign.Agent(id)==2;
        var mode=SpyPanel("Standing instructions",185,85);
        CampaignHUD.Label(mode.transform,"Free standing instruction · 1–2 points daily · continues during rebellion",new Vector2(.03f,.57f),new Vector2(.97f,.97f),20);
        for(int i=0;i<2;i++){int m=i;var button=CampaignHUD.MakeButton(mode.transform,(ProvinceCampaign.Get("Mode_"+id)==m?"Selected · ":"")+(m==0?"Weaken authority":"Build resistance"),new Vector2(.03f+i*.48f,.05f),new Vector2(.49f+i*.48f,.52f),()=>{ProvinceCampaign.Mode(id,m);SaveGames.Checkpoint();ProvincialOperations();});button.interactable=p.Known&&p.Hostile&&!p.Pending;}
        string[] titles={"Weaken Authority","Acquire enemy plans","Finance resistance"};
        string[] descriptions={"−1–10 Authority · 500 gold","+2 secrets · once per province · 1,000 gold","+1–10 Resistance · Authority ≤70 · 1,000 gold"};
        for(int i=0;i<3;i++){int op=i;var row=SpyPanel(titles[i],275+i*70,65);CampaignHUD.Label(row.transform,descriptions[i],new Vector2(.03f,.08f),new Vector2(.65f,.92f),19);var button=CampaignHUD.MakeTurnButton(row.transform,titles[i],new Vector2(.67f,.08f),new Vector2(.97f,.92f),()=>{bool ok=ProvinceCampaign.Act(id,op,out var text);Finish(ok,text);});Availability(button,ProvinceCampaign.Reason(id,op));}
        var command=SpyPanel("Armies",490,55);CampaignHUD.MakeButton(command.transform,"Provincial armies and standing orders",new Vector2(.03f,.04f),new Vector2(.97f,.96f),()=>{Hide();ProvincialConflictUI.Show(id);});
    }
    static void Availability(UnityEngine.UI.Button button,string reason){if(reason==null)return;button.interactable=false;button.GetComponentInChildren<TMP_Text>().text=reason;}
    public void Specialists()=>SpecialistRoster();
}
