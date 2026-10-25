using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public partial class CouncilUI
{
    private void EnemyTroopsLink(GameObject card,UnityAction back)
    {
        var button=card.AddComponent<Button>();button.targetGraphic=card.GetComponent<Image>();
        var colours=button.colors;colours.highlightedColor=new Color(1f,.87f,.58f);button.colors=colours;
        button.onClick.AddListener(()=>EnemyTroops(back));
        foreach(var text in card.GetComponentsInChildren<TMPro.TMP_Text>())text.raycastTarget=false;
    }
    public void EnemyTroops(UnityAction back)
    {
        Open("The Ottoman field army","Troop types, remaining strength and assault values");
        ControlSingleton.Instance.ReturnAction=()=>back();
        Canvas.ForceUpdateCanvases();BodyHeight(Mathf.Max(650,((RectTransform)body.parent).rect.height));
        bool known=CouncilActions.Int("iSpies")>=1;
        var ai=AI.Instance;
        string[] names={"Christian Balkan contingents","Bashi-bazouks · irregulars","Janissaries"};
        string[] keys={"iBalkans","iBazouks","iJanissaries"};
        string[] descriptions={"Troops drawn from the Sultan's Balkan subjects. Their assault pressure differs from that of the other formations.","Irregular troops used in mass attacks. They share the Balkan contingents' per-man lethality in the current battle rules.","The Sultan's elite infantry. Their assault pressure and per-man lethality are tracked separately."};
        CampaignHUD.Label(body,known?"REPORTED TOTAL  ·  "+EspionageSystem.IntelligenceValue(0):"NUMBERS UNKNOWN  ·  Establish at least one agent to reveal enemy strength",new Vector2(.03f,.92f),new Vector2(.97f,.99f),24);
        for(int i=0;i<3;i++)
        {
            float top=.90f-i*.235f;
            var row=CampaignHUD.Panel(body,names[i],new Vector2(.025f,top-.22f),new Vector2(.975f,top));
            row.GetComponent<Image>().color=ImperialTheme.Surface;
            CampaignHUD.Label(row.transform,names[i],new Vector2(.025f,.72f),new Vector2(.97f,.98f),25);
            int count=CouncilActions.Int(keys[i]);
            string numbers=known?"Remaining: "+count.ToString("N0"):"Remaining: unknown";
            string stats=ai==null?"Combat values unavailable outside a campaign.":
                "Assault pressure: "+ai.TroopAssaultRating(i)+" points  ·  Lethality: "+ai.TroopLethality(i).ToString("0.###")+" per attacker reaching the wall\n"+
                "Standard wave: "+ai.TroopWaveSize(i).ToString("N0")+(known?"  ·  Available wave: "+Mathf.Min(count,ai.TroopWaveSize(i)).ToString("N0"):"");
            CampaignHUD.Label(row.transform,numbers+"\n"+stats,new Vector2(.025f,.28f),new Vector2(.97f,.73f),19);
            var detail=CampaignHUD.Explanation(row.transform,descriptions[i],new Vector2(.025f,.015f),new Vector2(.97f,.28f),19);detail.enableAutoSizing=true;detail.fontSizeMin=15;detail.fontSizeMax=17;
        }
        var explanation=CampaignHUD.Explanation(body,"<b>Reading the values</b>\nAssault pressure contributes to the breach test; it is not a success percentage. Lethality is the base casualty contribution of each attacker who reaches the wall. Wall protection and battle variation then modify losses. Standard waves are capped by the troops still available. These are the current campaign's combat rules.",new Vector2(.035f,.075f),new Vector2(.965f,.19f),20);explanation.enableAutoSizing=true;explanation.fontSizeMin=16;explanation.fontSizeMax=20;
        CampaignHUD.MakeButton(body,"Return",new Vector2(.35f,.005f),new Vector2(.65f,.065f),back);
    }
}

