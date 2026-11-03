using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only: the existing preparation service owns costs and consequences.
public partial class CouncilUI
{
    int galataTab;
    static readonly int[][] GalataGroups={new[]{8,9,1,0},new[]{2,3,4,5},new[]{6,7}};
    static readonly string[] GalataOrderNames={"Bribe the podestà","Gifts and negotiation","Request provisions","Arrange a loan","Request reinforcements","Strengthen Galata","Pay Giovanni's men","Requisition revenues","Demand immediate support","Respect neutrality"};

    public void Galata()=>GalataMenu(-1);

    string GalataTerms(int action)
    {
        switch(action){
            case 0:return "7,500 gold · Goodwill +30";
            case 1:return "2,000 gold · Goodwill +"+(DailyLifeEvents.I("iFocus")==3?"30":"10");
            case 2:return "1 claim · Receive 1,000 provisions";
            case 3:return "1 claim · Borrow 5,000 gold for ten years";
            case 4:return "1 claim · Receive 150 infantry and 100 archers";
            case 5:return "1 claim · Galata defences +10";
            case 6:return "3,000 gold · Giovanni's loyalty +10";
            case 7:return "Receive 3,000 gold · Giovanni's loyalty −20";
            case 8:return "Receive 300 infantry, 300 archers, 4,500 gold and 500 provisions. Goodwill −20.";
            default:return "Goodwill +10 · Once per campaign";
        }
    }
    string GalataDetail(int action)
    {
        switch(action){
            case 0:return "The podestà accepts your payment. Galata's political status remains unchanged.";
            case 1:return "Send gifts and negotiate goodwill with Galata's representatives.";
            case 2:return "Redeem one diplomatic claim for supplies from Galata's warehouses.";
            case 3:return "Redeem one diplomatic claim for ten-year credit. No repayment falls due during this siege.";
            case 4:return "Redeem one diplomatic claim for troops to reinforce the City's defence.";
            case 5:return "Redeem one diplomatic claim to strengthen Galata's harbour defences.";
            case 6:return "Pay Giovanni's soldiers. His personal loyalty is separate from Galatan goodwill.";
            case 7:return "Claim revenues from Giovanni's autonomous administration. Repeated demands may provoke a political crisis.";
            case 8:return "The podestà sends immediate support under protest. Galata remains neutral.";
            default:return "Publicly recognise Galata's friendly neutrality. Its gates remain its own.";
        }
    }
    static TMP_Text GalataText(Transform parent,string name,string value,float low,float high,int size=22)
    {
        var text=CampaignHUD.Label(parent,value,new Vector2(.045f,low),new Vector2(.955f,high),size);
        text.name=name;text.enableAutoSizing=true;text.fontSizeMin=17;text.fontSizeMax=size;text.alignment=TextAlignmentOptions.MidlineLeft;return text;
    }
    void GalataMenu(int review)
    {
        Open("Galatan diplomacy","");ControlSingleton.Instance.ReturnAction=review<0?(Action)Diplomacy:Galata;
        SceneBackdrop.Apply(screen,"galata");
        foreach(Transform child in screen.transform)if(child.name!="Scene artwork"&&child.name!="Imperial goldwork")child.gameObject.SetActive(false);
        var panel=CampaignHUD.Panel(screen.transform,"Galata desk",new Vector2(.54f,.025f),new Vector2(.985f,.975f));ImperialTheme.Panel(panel,false);
        var root=panel.transform;
        GalataText(root,"Galata title","GALATAN DIPLOMACY",.925f,.985f,29);
        int state=Mathf.Clamp(PreparationCampaign.GalataState,0,3);
        GalataText(root,"Galata control",PreparationCampaign.GalataStates[state],.875f,.925f,23);
        GalataText(root,"Galata measures","Goodwill "+VariableSingleton.GetPercentFloat("fGalataRep")+"  ·  Defences "+DailyLifeEvents.I("iGalataDefences")+"\nGiovanni's loyalty "+DailyLifeEvents.I("iLoyalty0")+"  ·  Claims "+DailyLifeEvents.I("iGalataClaims")+"\nTen-year credit "+DailyLifeEvents.I("iGalataDebt").ToString("N0")+" gold",.735f,.87f,22);
        if(review>=0){
            GalataText(root,"Galata review title",GalataOrderNames[review],.62f,.715f,29);
            GalataText(root,"Galata review terms",GalataTerms(review),.455f,.61f,25);
            GalataText(root,"Galata review explanation",GalataDetail(review),.29f,.445f,22);
            string reason=PreparationCampaign.GalataActionUnavailable(review);
            GalataText(root,"Galata unavailable",reason??"",.225f,.285f,20);
            bool submitted=false;
            var confirm=CampaignHUD.MakeTurnButton(root,"Confirm order",new Vector2(.12f,.13f),new Vector2(.88f,.215f),()=>{
                if(submitted)return;bool ok=PreparationCampaign.DiplomaticAction(review,out var report);submitted=ok;Finish(ok,report);
            });confirm.name="Galata confirm "+review;confirm.interactable=reason==null;MarketButtonText(confirm,23);
        }else{
            string[] tabs={"Relations","Claims","Giovanni"};
            for(int i=0;i<tabs.Length;i++){
                int tab=i;float left=.035f+i*.315f;
                var button=CampaignHUD.MakeButton(root,tabs[i],new Vector2(left,.65f),new Vector2(left+.30f,.715f),()=>{galataTab=tab;Galata();});
                button.name="Galata tab "+i;button.interactable=galataTab!=i;MarketButtonText(button,22);
            }
            var ordered=GalataGroups[Mathf.Clamp(galataTab,0,2)].OrderBy(a=>PreparationCampaign.GalataActionUnavailable(a)==null?0:1).ToArray();
            for(int pos=0;pos<ordered.Length;pos++){
                int action=ordered[pos];float top=.63f-pos*.112f;
                var row=CampaignHUD.Panel(root,"Galata order "+action,new Vector2(.025f,top-.103f),new Vector2(.975f,top));ImperialTheme.Panel(row,false);
                var text=CampaignHUD.Label(row.transform,GalataOrderNames[action]+"\n"+GalataTerms(action),new Vector2(.025f,.08f),new Vector2(.755f,.94f),21);
                text.name="Galata order terms";text.enableAutoSizing=true;text.fontSizeMin=17;text.fontSizeMax=21;text.alignment=TextAlignmentOptions.MidlineLeft;
                string reason=PreparationCampaign.GalataActionUnavailable(action);
                var button=CampaignHUD.MakeButton(row.transform,reason==null?"Review":"Details",new Vector2(.775f,.17f),new Vector2(.975f,.83f),()=>GalataMenu(action));
                button.name="Galata review "+action;MarketButtonText(button,20);
                // Locked orders may be inspected: their full requirement is shown, but confirmation remains disabled.
                if(reason!=null)text.color=new Color(.65f,.62f,.51f);
            }
            string explanation=state==3?"Galata is under Ottoman control. Orders are unavailable.":galataTab==1?"Redeem diplomatic claims for supplies, credit, soldiers or stronger defences.":galataTab==2?"Giovanni's personal loyalty is separate from Galatan goodwill.":"Goodwill shapes Galata's willingness to stand with the City.";
            GalataText(root,"Galata context",explanation,.10f,.175f,19);
        }
        var back=CampaignHUD.MakeButton(root,review>=0?"Back to Galata":"Return",new Vector2(.20f,.025f),new Vector2(.80f,.09f),()=>{if(review>=0)Galata();else Diplomacy();});
        back.name="Galata return";MarketButtonText(back,23);
    }
}
