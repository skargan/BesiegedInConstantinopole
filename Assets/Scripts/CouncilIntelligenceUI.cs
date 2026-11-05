using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    // Presentation only: the existing mission and intelligence rules remain authoritative.
    private void IntelligenceMenu(bool operations)
    {
        var desk=MilitaryCanvas("Spies and intelligence","intelligence","Reports from the camp. Orders from the shadows.");
        int agents=CouncilActions.Int("iSpies");
        var network=CampaignHUD.Panel(desk.transform,"Intelligence network",new Vector2(.025f,.72f),new Vector2(.975f,.98f));
        ImperialTheme.Panel(network,false);
        IntelligenceText(network.transform,"CAMP AGENTS  "+agents+"     SECRETS  "+CouncilActions.Int("iSecrets")+"     GOLD  "+CouncilActions.Int("iMoney").ToString("N0"),new Vector2(.025f,.73f),new Vector2(.975f,.98f),23);
        IntelligenceText(network.transform,"Operations delay: "+CouncilActions.Int("iEnemyDelay")+" days · Agents abroad: "+ProvinceCampaign.Assigned,new Vector2(.025f,.50f),new Vector2(.975f,.73f),19);
        // Match intelligence tier VIII; do not even read the hidden count below the threshold.
        string enemySpies=agents>=8?CouncilActions.Int("iTurkSpies").ToString("N0"):"?";
        var counter=IntelligenceText(network.transform,"ENEMY SPIES  "+enemySpies,new Vector2(.025f,.27f),new Vector2(.42f,.50f),23);
        counter.name="Enemy spies summary";
        IntelligenceText(network.transform,"In the City · revealed at 8 agents",new Vector2(.025f,.02f),new Vector2(.42f,.27f),18);
        var alert=IntelligenceText(network.transform,"OTTOMAN CAMP ALERT  "+EspionageSystem.Alert+"%",new Vector2(.44f,.27f),new Vector2(.975f,.50f),23);
        alert.color=EspionageSystem.Alert>=50?new Color(1f,.65f,.49f):ImperialTheme.Ivory;
        IntelligenceText(network.transform,"Falls 1–10 points/turn"+(EspionageSystem.AlertFloor>0?" · Minimum "+EspionageSystem.AlertFloor+"%":""),new Vector2(.44f,.02f),new Vector2(.975f,.27f),19);

        var reports=IntelligenceButton(desk.transform,"Reports",new Vector2(.025f,.63f),new Vector2(.23f,.705f),()=>IntelligenceMenu(false));
        reports.name="Intelligence reports tab";
        var orders=IntelligenceButton(desk.transform,"Operations",new Vector2(.24f,.63f),new Vector2(.46f,.705f),()=>IntelligenceMenu(true));
        orders.name="Intelligence operations tab";
        var selected=operations?orders:reports;selected.interactable=false;var selectionColours=selected.colors;selectionColours.disabledColor=new Color(.72f,.58f,.34f);selected.colors=selectionColours;
        if(operations){
            IntelligenceText(desk.transform,"PAY WITH",new Vector2(.46f,.63f),new Vector2(.58f,.705f),18);
            var gold=IntelligenceButton(desk.transform,"1,000 gold",new Vector2(.59f,.63f),new Vector2(.79f,.705f),()=>{paySecret=false;IntelligenceMenu(true);});gold.name="Spy payment gold";
            var secrets=IntelligenceButton(desk.transform,"1 secret",new Vector2(.80f,.63f),new Vector2(1,.705f),()=>{paySecret=true;IntelligenceMenu(true);});secrets.name="Spy payment secrets";var payment=paySecret?secrets:gold;var paymentColours=payment.colors;paymentColours.normalColor=new Color(.72f,.58f,.34f);paymentColours.selectedColor=paymentColours.normalColor;payment.colors=paymentColours;
            IntelligenceOperations(desk.transform);
        }else{
            IntelligenceLedger(desk.transform,agents);
        }
        IntelligenceButton(desk,"War Beyond the Walls",new Vector2(.025f,.02f),new Vector2(.45f,.085f),WarTheatreUI.Show);
        IntelligenceButton(desk,"Specialists",new Vector2(.46f,.02f),new Vector2(.69f,.085f),Specialists);
        IntelligenceButton(desk,"Return",new Vector2(.70f,.02f),new Vector2(.975f,.085f),()=>ControlSingleton.Instance.GoBack());
        notice.rectTransform.anchorMin=new Vector2(.50f,.02f);notice.rectTransform.anchorMax=new Vector2(.77f,.09f);
    }

    private void IntelligenceLedger(Transform parent,int agents)
    {
        var ledger=CampaignHUD.Panel(parent,"Intelligence ledger",new Vector2(.025f,.10f),new Vector2(.975f,.61f));
        ImperialTheme.Panel(ledger,false);
        string[] tiers={"I","II","III","IV","V","VI","VII","VIII","IX","X"};
        var detail=CampaignHUD.Panel(ledger.transform,"Selected intelligence report",new Vector2(.49f,.02f),new Vector2(.98f,.98f));ImperialTheme.Panel(detail,false);
        System.Action<int> select=null;
        select=(i)=>{
            foreach(Transform child in detail.transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            bool known=agents>i;
            IntelligenceText(detail.transform,EspionageSystem.IntelligenceLabels[i],new Vector2(.04f,.80f),new Vector2(.96f,.98f),25);
            IntelligenceText(detail.transform,known?EspionageSystem.IntelligenceValue(i):"Requires "+(i+1)+" agents",new Vector2(.04f,.20f),new Vector2(.96f,.80f),23);
            if(i==0){var troops=IntelligenceButton(detail.transform,"View troops",new Vector2(.04f,.025f),new Vector2(.96f,.18f),()=>EnemyTroops(Spies));troops.name="Intelligence troop details";}
            if(i==9){var attempt=IntelligenceButton(detail.transform,"Review attempt",new Vector2(.04f,.025f),new Vector2(.96f,.18f),ReviewAssassination);attempt.name="Intelligence assassination";attempt.interactable=known;}
        };
        for(int i=0;i<10;i++){int tier=i;float top=.985f-i*.097f;
            var button=IntelligenceButton(ledger.transform,tiers[i]+"  "+EspionageSystem.IntelligenceLabels[i],new Vector2(.015f,top-.09f),new Vector2(.475f,top),()=>select(tier));
            button.name="Select intelligence report "+i;
        }
        select(0);
    }

    private void IntelligenceOperations(Transform parent)
    {
        var area=CampaignHUD.Panel(parent,"Spy operations",new Vector2(.025f,.10f),new Vector2(.975f,.61f));
        area.GetComponent<Image>().color=Color.clear;area.GetComponent<Image>().raycastTarget=false;
        for(int i=0;i<CouncilActions.SpyOrders.Length;i++){
            int mission=i;float left=i%2*.51f,top=1-i/2*.335f;
            var card=CampaignHUD.Panel(area.transform,"Spy mission "+i,new Vector2(left,top-.32f),new Vector2(left+.49f,top));
            ImperialTheme.Panel(card,false);
            IntelligenceText(card.transform,CouncilActions.SpyOrders[i],new Vector2(.025f,.76f),new Vector2(.975f,.97f),23);
            IntelligenceText(card.transform,CouncilActions.SpyEffects[i],new Vector2(.025f,.47f),new Vector2(.975f,.75f),20);
            IntelligenceText(card.transform,"Agent loss risk "+EspionageSystem.LossChance(i).ToString("0.#")+"% · Alert "+(i==3?"unchanged":"+"+EspionageSystem.AlertGain[i]),new Vector2(.025f,.30f),new Vector2(.975f,.46f),18);
            string reason=SpyMissionUnavailable(i);
            var review=IntelligenceButton(card.transform,reason??("Review · "+(paySecret?"1 secret":"1,000 gold")),new Vector2(.025f,.04f),new Vector2(.975f,.28f),()=>{if(mission==5)ReviewDisruption();else ReviewSpyMission(mission);});
            review.name="Review spy mission "+i;review.interactable=reason==null;
        }
    }

    private string SpyMissionUnavailable(int mission)
    {
        if(mission!=0&&CouncilActions.Int("iSpies")<1)return "Recruit an agent first";
        if(paySecret?CouncilActions.Int("iSecrets")<1:CouncilActions.Int("iMoney")<1000)return paySecret?"Requires 1 secret":"Requires 1,000 gold";
        if(mission==1&&CouncilActions.Int("iTurkCannons")==0||mission==2&&SharedProvisions.Stock(SharedProvisions.Camp)<=0||mission==3&&CouncilActions.Int("iTurkSpies")==0)return "No targets remain";
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
        IntelligenceText(review.transform,"Cost: "+(useSecret?"1 secret":"1,000 gold"),new Vector2(.04f,.40f),new Vector2(.96f,.56f),25);
        IntelligenceText(review.transform,"Risk of losing an agent: "+EspionageSystem.LossChance(mission).ToString("0.#")+"%\nCamp alert: "+(mission==3?"unchanged":"+"+EspionageSystem.AlertGain[mission]+" points")+". Higher alert increases the risk of future missions.",new Vector2(.04f,.09f),new Vector2(.96f,.39f),23);
        bool submitted=false;
        var cancel=IntelligenceButton(screen.transform,"Cancel",new Vector2(.10f,.115f),new Vector2(.46f,.205f),()=>IntelligenceMenu(true));cancel.name="Cancel spy mission";
        var confirm=IntelligenceButton(screen.transform,"Confirm operation",new Vector2(.54f,.115f),new Vector2(.90f,.205f),()=>{
            if(submitted)return;
            bool ok=CouncilActions.SpyMission(mission,useSecret,out var report);
            if(ok)submitted=true;
            Finish(ok,report);
        });TurnActionButton.Mark(confirm);confirm.name="Confirm spy mission "+mission;confirm.interactable=SpyMissionUnavailable(mission)==null;
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
