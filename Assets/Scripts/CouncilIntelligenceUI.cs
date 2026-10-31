using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    // Presentation only: the existing mission and intelligence rules remain authoritative.
    private void IntelligenceMenu(bool operations)
    {
        Open("Spies and intelligence", "");
        var scroll=body.parent.GetComponent<ScrollRect>();
        if(scroll.verticalScrollbar!=null)scroll.verticalScrollbar.gameObject.SetActive(false);
        body.parent.gameObject.SetActive(false);

        var desk=CampaignHUD.Panel(screen.transform,"Intelligence desk",new Vector2(.035f,.115f),new Vector2(.965f,.88f));
        desk.GetComponent<Image>().color=Color.clear;
        desk.GetComponent<Image>().raycastTarget=false;
        int agents=CouncilActions.Int("iSpies");
        var network=CampaignHUD.Panel(desk.transform,"Intelligence network",new Vector2(0,.845f),Vector2.one);
        ImperialTheme.Panel(network,false);
        IntelligenceText(network.transform,"CAMP AGENTS  "+agents+"     SECRETS  "+CouncilActions.Int("iSecrets")+"     GOLD  "+CouncilActions.Int("iMoney").ToString("N0"),new Vector2(.02f,.47f),new Vector2(.65f,.94f),23);
        IntelligenceText(network.transform,"One agent unlocks each level · Operations delay: "+CouncilActions.Int("iEnemyDelay")+" days · Agents abroad: "+ProvinceCampaign.Assigned,new Vector2(.02f,.06f),new Vector2(.65f,.49f),19);
        var alert=IntelligenceText(network.transform,"CAMP ALERT  "+EspionageSystem.Alert+"%",new Vector2(.68f,.47f),new Vector2(.98f,.94f),23);
        alert.color=EspionageSystem.Alert>=50?new Color(1f,.65f,.49f):ImperialTheme.Ivory;
        IntelligenceText(network.transform,"Falls 1–10 points/turn · Minimum "+EspionageSystem.AlertFloor+"%",new Vector2(.68f,.06f),new Vector2(.98f,.49f),19);

        var reports=IntelligenceButton(desk.transform,"Reports",new Vector2(0,.753f),new Vector2(.18f,.827f),()=>IntelligenceMenu(false));
        reports.name="Intelligence reports tab";
        var orders=IntelligenceButton(desk.transform,"Operations",new Vector2(.19f,.753f),new Vector2(.38f,.827f),()=>IntelligenceMenu(true));
        orders.name="Intelligence operations tab";
        var selected=operations?orders:reports;selected.interactable=false;var selectionColours=selected.colors;selectionColours.disabledColor=new Color(.72f,.58f,.34f);selected.colors=selectionColours;
        if(operations){
            IntelligenceText(desk.transform,"PAY WITH",new Vector2(.46f,.753f),new Vector2(.58f,.827f),18);
            var gold=IntelligenceButton(desk.transform,"1,000 gold",new Vector2(.59f,.753f),new Vector2(.79f,.827f),()=>{paySecret=false;IntelligenceMenu(true);});gold.name="Spy payment gold";
            var secrets=IntelligenceButton(desk.transform,"1 secret",new Vector2(.80f,.753f),new Vector2(1,.827f),()=>{paySecret=true;IntelligenceMenu(true);});secrets.name="Spy payment secrets";var payment=paySecret?secrets:gold;var paymentColours=payment.colors;paymentColours.normalColor=new Color(.72f,.58f,.34f);paymentColours.selectedColor=paymentColours.normalColor;payment.colors=paymentColours;
            IntelligenceOperations(desk.transform);
        }else{
            IntelligenceText(desk.transform,"REPORT / AGENTS REQUIRED",new Vector2(.42f,.753f),new Vector2(1,.827f),18);
            IntelligenceLedger(desk.transform,agents);
        }
        IntelligenceButton(screen.transform,"War Beyond the Walls",new Vector2(.035f,.02f),new Vector2(.30f,.09f),WarTheatreUI.Show);
        IntelligenceButton(screen.transform,"Specialists",new Vector2(.31f,.02f),new Vector2(.48f,.09f),Specialists);
        notice.rectTransform.anchorMin=new Vector2(.50f,.02f);notice.rectTransform.anchorMax=new Vector2(.77f,.09f);
    }

    private void IntelligenceLedger(Transform parent,int agents)
    {
        var ledger=CampaignHUD.Panel(parent,"Intelligence ledger",Vector2.zero,new Vector2(1,.73f));
        ImperialTheme.Panel(ledger,false);
        // Longer unlocked reports receive the space their actual contents require.
        float[] heights={1.35f,1.05f,.9f,.9f,.9f,.9f,1.8f,2.7f,1.25f,1.75f};
        float total=0;for(int i=0;i<10;i++){if(agents<=i)heights[i]=(i==0||i==9)?1.35f:1.05f;total+=heights[i];}
        float top=.985f;
        for(int i=0;i<10;i++){
            bool known=agents>i;float h=.97f*heights[i]/total;
            var row=CampaignHUD.Panel(ledger.transform,"Intelligence row "+i,new Vector2(.008f,top-h),new Vector2(.992f,top-.004f));
            row.GetComponent<Image>().color=i%2==0?ImperialTheme.Surface:ImperialTheme.Navy;
            var label=IntelligenceText(row.transform,(i+1)+"  "+EspionageSystem.IntelligenceLabels[i],new Vector2(.015f,.04f),new Vector2(.33f,.96f),21);
            bool link=i==0||i==9;
            var value=IntelligenceText(row.transform,known?EspionageSystem.IntelligenceValue(i):"Requires "+(i+1)+" agents",new Vector2(.35f,.04f),new Vector2(link?.80f:.985f,.96f),20);
            if(!known){label.color=new Color(.66f,.65f,.59f);value.color=label.color;}
            if(i==0){var troops=IntelligenceButton(row.transform,"View troops",new Vector2(.82f,.035f),new Vector2(.985f,.965f),()=>EnemyTroops(Spies));troops.name="Intelligence troop details";}
            if(i==9){var attempt=IntelligenceButton(row.transform,"Review attempt",new Vector2(.82f,.035f),new Vector2(.985f,.965f),ReviewAssassination);attempt.name="Intelligence assassination";attempt.interactable=known;}
            top-=h;
        }
    }

    private void IntelligenceOperations(Transform parent)
    {
        var area=CampaignHUD.Panel(parent,"Spy operations",Vector2.zero,new Vector2(1,.73f));
        area.GetComponent<Image>().color=Color.clear;area.GetComponent<Image>().raycastTarget=false;
        for(int i=0;i<CouncilActions.SpyOrders.Length;i++){
            int mission=i;float left=i%2*.51f,top=1-i/2*.335f;
            var card=CampaignHUD.Panel(area.transform,"Spy mission "+i,new Vector2(left,top-.32f),new Vector2(left+.49f,top));
            ImperialTheme.Panel(card,false);
            IntelligenceText(card.transform,CouncilActions.SpyOrders[i],new Vector2(.025f,.76f),new Vector2(.975f,.97f),23);
            IntelligenceText(card.transform,CouncilActions.SpyEffects[i],new Vector2(.025f,.47f),new Vector2(.975f,.75f),20);
            IntelligenceText(card.transform,"Agent loss risk "+EspionageSystem.LossChance(i).ToString("0.#")+"% · Alert "+(i==3?"unchanged":"+"+EspionageSystem.AlertGain[i])+" · Major action",new Vector2(.025f,.30f),new Vector2(.975f,.46f),18);
            string reason=SpyMissionUnavailable(i);
            var review=IntelligenceButton(card.transform,reason??("Review · "+(paySecret?"1 secret":"1,000 gold")),new Vector2(.025f,.04f),new Vector2(.975f,.28f),()=>{if(mission==5)ReviewDisruption();else ReviewSpyMission(mission);});
            review.name="Review spy mission "+i;review.interactable=reason==null;
        }
    }

    private string SpyMissionUnavailable(int mission)
    {
        if(mission!=0&&CouncilActions.Int("iSpies")<1)return "Recruit an agent first";
        if(paySecret?CouncilActions.Int("iSecrets")<1:CouncilActions.Int("iMoney")<1000)return paySecret?"Requires 1 secret":"Requires 1,000 gold";
        if(mission==1&&CouncilActions.Int("iTurkCannons")==0||mission==2&&CouncilActions.Int("iFood")==0||mission==3&&CouncilActions.Int("iTurkSpies")==0)return "No targets remain";
        return null;
    }

    private void ReviewSpyMission(int mission)
    {
        bool useSecret=paySecret;
        Open(CouncilActions.SpyOrders[mission],EspionageSystem.Descriptions[mission]);
        ControlSingleton.Instance.ReturnAction=()=>IntelligenceMenu(true);
        var scroll=body.parent.GetComponent<ScrollRect>();
        if(scroll.verticalScrollbar!=null)scroll.verticalScrollbar.gameObject.SetActive(false);
        body.parent.gameObject.SetActive(false);
        var review=CampaignHUD.Panel(screen.transform,"Spy mission review",new Vector2(.10f,.24f),new Vector2(.90f,.77f));
        ImperialTheme.Panel(review,false);
        IntelligenceText(review.transform,"EXPECTED EFFECT",new Vector2(.04f,.82f),new Vector2(.96f,.96f),24);
        IntelligenceText(review.transform,CouncilActions.SpyEffects[mission],new Vector2(.04f,.57f),new Vector2(.96f,.82f),28);
        IntelligenceText(review.transform,"Cost: "+(useSecret?"1 secret":"1,000 gold")+" · Uses your major action",new Vector2(.04f,.40f),new Vector2(.96f,.56f),25);
        IntelligenceText(review.transform,"Risk of losing an agent: "+EspionageSystem.LossChance(mission).ToString("0.#")+"%\nCamp alert: "+(mission==3?"unchanged":"+"+EspionageSystem.AlertGain[mission]+" points")+". Higher alert increases the risk of future missions.",new Vector2(.04f,.09f),new Vector2(.96f,.39f),23);
        bool submitted=false;
        var cancel=IntelligenceButton(screen.transform,"Cancel",new Vector2(.10f,.115f),new Vector2(.46f,.205f),()=>IntelligenceMenu(true));cancel.name="Cancel spy mission";
        var confirm=IntelligenceButton(screen.transform,"Confirm operation",new Vector2(.54f,.115f),new Vector2(.90f,.205f),()=>{
            if(submitted)return;
            bool ok=CouncilActions.SpyMission(mission,useSecret,out var report);
            if(ok)submitted=true;
            Finish(ok,report);
        });confirm.name="Confirm spy mission "+mission;confirm.interactable=SpyMissionUnavailable(mission)==null;
    }

    private TMP_Text IntelligenceText(Transform parent,string text,Vector2 min,Vector2 max,float size)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);label.alignment=TextAlignmentOptions.MidlineLeft;
        label.enableAutoSizing=true;label.fontSizeMin=Mathf.Min(18,size);label.fontSizeMax=size;label.raycastTarget=false;
        return label;
    }

    private Button IntelligenceButton(Transform parent,string text,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
    {
        var button=CampaignHUD.MakeButton(parent,text,min,max,action);ImperialTheme.Button(button);return button;
    }
}
