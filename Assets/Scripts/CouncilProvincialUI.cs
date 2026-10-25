using TMPro;
using UnityEngine;

public partial class CouncilUI
{
    // Release owner calls this once at the end of Spies(), preserving their intelligence layout.
    public void AddProvincialNavigation(float top)
    {
        BodyHeight(top+90);
        var row=SpyPanel("Beyond the siege lines",top,78);
        CampaignHUD.MakeButton(row.transform,"War Beyond the Walls",new Vector2(.02f,.12f),new Vector2(.65f,.88f),WarTheatreUI.Show);
        CampaignHUD.MakeButton(row.transform,"Specialists",new Vector2(.67f,.12f),new Vector2(.98f,.88f),Specialists);
    }
    string provincialSelection="macedonia";
    public void ProvincialOperationsFor(string province){provincialSelection=province;ProvincialOperations();}
    public void ProvincialOperations()
    {
        if(ProvincialIntelligence.Assigned>0)provincialSelection=ProvincialIntelligence.ProvinceId;
        Open("War Beyond the Walls",WarTheatre.Find(provincialSelection).name+" · Send a camp agent beyond the siege lines. Travel and operations continue while the siege goes on. Each order uses your major action.");
        ControlSingleton.Instance.ReturnAction=WarTheatreUI.Show;BodyHeight(1010);
        var status=SpyPanel("Provincial assignment",0,180);
        CampaignHUD.Label(status.transform,ProvincialIntelligence.Status,new Vector2(.03f,.69f),new Vector2(.97f,.97f),27);
        CampaignHUD.Explanation(status.transform,"Camp agents: "+CouncilActions.Int("iSpies")+" · Provincial assignment: "+ProvincialIntelligence.Assigned+"\nPending convoy disruption: "+ProvincialIntelligence.GetSupplyPressure()+" days of stores. This changes the next delivery, not today's food consumption.",new Vector2(.03f,.10f),new Vector2(.97f,.66f),23);
        var report=SpyPanel("Provincial correspondence",192,210);
        CampaignHUD.Explanation(report.transform,ProvincialIntelligence.LastReport,new Vector2(.03f,.10f),new Vector2(.97f,.91f),24);
        var travel=SpyPanel("Travel orders",414,150);
        CampaignHUD.Explanation(travel.transform,"One agent, seven days each way. An agent abroad no longer contributes to your knowledge of the Ottoman camp.",new Vector2(.03f,.44f),new Vector2(.97f,.96f),22);
        var dispatch=CampaignHUD.MakeButton(travel.transform,"Send agent · 500 gold",new Vector2(.03f,.06f),new Vector2(.49f,.40f),()=>{bool ok=ProvincialIntelligence.Dispatch(provincialSelection,out var text);Finish(ok,text);});
        Availability(dispatch,ProvincialIntelligence.DispatchUnavailable);
        var recall=CampaignHUD.MakeButton(travel.transform,"Recall agent · 7 days",new Vector2(.51f,.06f),new Vector2(.97f,.40f),()=>{bool ok=ProvincialIntelligence.Recall(out var text);Finish(ok,text);});
        recall.interactable=ProvincialIntelligence.State==ProvincialIntelligence.CellState.Resident;
        string[] titles={"The clerk's second ledger","Fire in the forwarding yard"};
        string[] prose={"A clerk keeps two accounts: one for the Sultan and one to protect himself. Buy a copy before fear closes his mouth. Success reveals one secret and makes the later depot operation safer.","A forwarding depot feeds the siege army. Pay local contacts to destroy its grain before it is loaded. Success removes three days of stores from the next convoy; it cannot stop all imperial supply."};
        for(int i=0;i<2;i++) {
            int operation=i;var row=SpyPanel(titles[i],576+i*210,198);
            CampaignHUD.Label(row.transform,titles[i],new Vector2(.025f,.76f),new Vector2(.975f,.99f),26);
            CampaignHUD.Explanation(row.transform,prose[i]+" Failed operations may lose the agent.",new Vector2(.025f,.30f),new Vector2(.975f,.74f),22);
            var button=CampaignHUD.MakeButton(row.transform,i==0?"Obtain ledger · 1,000 gold · 3 days":"Sabotage depot · 1,500 gold · 4 days",new Vector2(.025f,.04f),new Vector2(.975f,.27f),()=>{bool ok=ProvincialIntelligence.Begin(operation,out var text);Finish(ok,text);});
            Availability(button,ProvincialIntelligence.OperationUnavailable(i));
        }
    }
    static void Availability(UnityEngine.UI.Button button,string reason)
    {
        if(reason==null)return;button.interactable=false;button.GetComponentInChildren<TMP_Text>().text=reason;
    }
    public void Specialists()
    {
        Open("Specialists","Your people have different duties. An agent sent abroad cannot also work in the Ottoman camp.");BodyHeight(620);
        string[] names={"Engineers","Monks","Agents","Envoys"};
        int mentor=DailyLifeEvents.I("iLifeMentorDue")>0?1:0;
        string[] detail={CouncilActions.Int("iEngineers")+" available · "+mentor+" temporarily mentoring an apprentice.\nEngineers support construction and specialised assignments.",DailyLifeEvents.I("iHealers")+" in the medical service.\nWounded recovery is not available yet.",CouncilActions.Int("iSpies")+" in the Ottoman camp · "+ProvincialIntelligence.Assigned+" assigned abroad.\n"+ProvincialIntelligence.Status,"Resident-envoy assignments are not available yet.\nOrdinary diplomatic appeals remain available through messengers."};
        for(int i=0;i<4;i++){var row=SpyPanel(names[i],i*145,133);CampaignHUD.Label(row.transform,names[i],new Vector2(.03f,.68f),new Vector2(.97f,.98f),26);CampaignHUD.Explanation(row.transform,detail[i],new Vector2(.03f,.08f),new Vector2(.97f,.66f),22);}
    }
}
