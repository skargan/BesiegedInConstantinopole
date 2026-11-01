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
        ProvinceCampaign.Ensure();string id=provincialSelection;
        Open("War Beyond the Walls",WarTheatre.Find(id).name+" · Provincial networks and supplies");
        ControlSingleton.Instance.ReturnAction=WarTheatreUI.Show;BodyHeight(1080);
        var status=SpyPanel("Province",0,170);
        CampaignHUD.Label(status.transform,"Authority "+ProvinceCampaign.Authority(id)+"% · Resistance "+ProvinceCampaign.Resistance(id)+"%",new Vector2(.03f,.65f),new Vector2(.97f,.97f),27);
        CampaignHUD.Explanation(status.transform,ProvinceCampaign.Status(id)+"\nRebellion begins at 45% authority. Rebellions and liberated provinces stop exporting supplies.",new Vector2(.03f,.05f),new Vector2(.97f,.62f),23);
        var travel=SpyPanel("Agent orders",182,150);
        CampaignHUD.Explanation(travel.transform,"One agent per province · seven days of travel · camp agents available: "+CouncilActions.Int("iSpies"),new Vector2(.03f,.5f),new Vector2(.97f,.95f),22);
        var send=CampaignHUD.MakeTurnButton(travel.transform,"Send agent · 500 gold",new Vector2(.03f,.05f),new Vector2(.49f,.44f),()=>{bool ok=ProvinceCampaign.Dispatch(id,out var text);Finish(ok,text);});Availability(send,ProvinceCampaign.DispatchReason(id));
        var recall=CampaignHUD.MakeTurnButton(travel.transform,"Recall agent · seven days",new Vector2(.51f,.05f),new Vector2(.97f,.44f),()=>{bool ok=ProvinceCampaign.Recall(id,out var text);Finish(ok,text);});recall.interactable=ProvinceCampaign.Agent(id)==2&&!ProvinceCampaign.Rebellion(id);
        var mode=SpyPanel("Standing instructions",344,150);
        CampaignHUD.Explanation(mode.transform,"Free standing instruction · 1–2 points daily. Resistance organising is available at any authority.",new Vector2(.03f,.5f),new Vector2(.97f,.95f),22);
        for(int i=0;i<2;i++){int m=i;var button=CampaignHUD.MakeButton(mode.transform,(ProvinceCampaign.Get("Mode_"+id)==m?"Selected · ":"")+(m==0?"Weaken authority":"Build resistance"),new Vector2(.03f+i*.48f,.05f),new Vector2(.49f+i*.48f,.44f),()=>{ProvinceCampaign.Mode(id,m);ProvincialOperations();});button.interactable=ProvinceCampaign.Agent(id)==2&&!ProvinceCampaign.Rebellion(id)&&!ProvinceCampaign.Free(id);}
        string[] titles={"Weaken Ottoman authority","Attack provision convoys","Finance resistance"};
        string[] descriptions={"Undermine the officials and their local supporters. Authority falls by 1–10 points.","Destroy 3–7 days of scheduled provisions. Requires authority at 85% or below; losses are limited to remaining shipments.","Arm and organise willing fighters. Resistance increases by 1–10 points. Requires authority at 70% or below."};
        for(int i=0;i<3;i++){int op=i;var row=SpyPanel(titles[i],506+i*180,168);CampaignHUD.Explanation(row.transform,descriptions[i],new Vector2(.03f,.4f),new Vector2(.97f,.94f),22);var button=CampaignHUD.MakeTurnButton(row.transform,titles[i]+" · "+(i==0?"500":"1,000")+" gold",new Vector2(.03f,.05f),new Vector2(.97f,.35f),()=>{bool ok=ProvinceCampaign.Act(id,op,out var text);Finish(ok,text);});Availability(button,ProvinceCampaign.Reason(id,op));}
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
        string[] detail={CouncilActions.Int("iEngineers")+" available · "+mentor+" temporarily mentoring an apprentice.\nEngineers support construction and specialised assignments.",MedicalRecovery.Monks+" monks · "+MedicalRecovery.Total+" wounded in care.\n"+MedicalRecovery.FreePlaces+" free care places · 15 treatment days to return.",CouncilActions.Int("iSpies")+" in the Ottoman camp · "+ProvinceCampaign.Assigned+" assigned abroad.\n"+"Agents develop separate local networks in each province.","Resident-envoy assignments are not available yet.\nOrdinary diplomatic appeals remain available through messengers."};
        for(int i=0;i<4;i++){var row=SpyPanel(names[i],i*145,133);CampaignHUD.Label(row.transform,names[i],new Vector2(.03f,.68f),new Vector2(.97f,.98f),26);CampaignHUD.Explanation(row.transform,detail[i],new Vector2(.03f,.08f),new Vector2(i==1?.70f:.97f,.66f),22);if(i==1)CampaignHUD.MakeButton(row.transform,"Infirmary",new Vector2(.73f,.12f),new Vector2(.97f,.66f),Infirmary);}
    }
}
