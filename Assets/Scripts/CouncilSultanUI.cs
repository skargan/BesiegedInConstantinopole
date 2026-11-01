using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    private void SultanAudience()
    {
        const string title="Messages to the Sultan";
        Open(title,"History, poetry and theology share one exchange per day, carried by your envoys.");
        ControlSingleton.Instance.ReturnAction=Diplomacy;

        // This audience uses the full page; the common scrolling list is not needed here.
        var scroll=body.parent.GetComponent<ScrollRect>();
        if(scroll.verticalScrollbar!=null)scroll.verticalScrollbar.gameObject.SetActive(false);
        body.parent.gameObject.SetActive(false);
        foreach(var label in screen.GetComponentsInChildren<TMP_Text>()) {
            if(label.transform.parent==screen.transform && label!=notice && label.text!="Return") {
                label.alignment=TextAlignmentOptions.Center;
                label.fontSizeMax=label.fontSize;
                label.enableAutoSizing=true;
                label.fontSizeMin=label.text==title?28:18;
            }
        }

        var audience=CampaignHUD.Panel(screen.transform,"Sultan audience",new Vector2(.035f,.115f),new Vector2(.965f,.805f));
        audience.GetComponent<Image>().color=Color.clear;
        audience.GetComponent<Image>().raycastTarget=false;
        var stage=audience.transform;
        SultanText(stage,"CONVERSATION",new Vector2(0,.925f),new Vector2(.32f,1),21,TextAlignmentOptions.Center);
        SultanText(stage,"DIPLOMATIC ACTIONS",new Vector2(.68f,.925f),Vector2.one,21,TextAlignmentOptions.Center);

        // Keep the opponent in the centre, with the same portrait used by his other screens.
        var identity=CampaignHUD.Panel(stage,"The Sultan",new Vector2(.34f,.31f),new Vector2(.66f,1));
        ImperialTheme.Panel(identity,false);
        CharacterPortraits.Draw(identity.transform,202,new Vector2(.04f,.18f),new Vector2(.96f,.97f));
        SultanText(identity.transform,"MEHMED II",new Vector2(.03f,.08f),new Vector2(.97f,.175f),29,TextAlignmentOptions.Center);
        SultanText(identity.transform,"Sultan and supreme commander",new Vector2(.03f,.015f),new Vector2(.97f,.08f),18,TextAlignmentOptions.Center);

        int[] conversations={1,3,4};
        int[] actions={2,5,6};
        for(int row=0;row<3;row++) {
            float top=.905f-row*.305f;
            SultanCard(stage,conversations[row],new Vector2(0,top-.29f),new Vector2(.32f,top));
            SultanCard(stage,actions[row],new Vector2(.68f,top-.29f),new Vector2(1,top));
        }
        SultanCard(stage,0,new Vector2(.34f,.005f),new Vector2(.66f,.295f));

        if(SurrenderTerms.HasOffer) {
            var offer=CampaignHUD.MakeButton(screen.transform,SurrenderTerms.Valid?"Review surrender terms":"Read expired offer",new Vector2(.035f,.02f),new Vector2(.37f,.09f),ReviewTerms);
            offer.name="Sultan offer review";
            ImperialTheme.Button(offer);
            notice.rectTransform.anchorMin=new Vector2(.39f,.02f);
            notice.rectTransform.anchorMax=new Vector2(.76f,.09f);
        }
    }

    private TMP_Text SultanText(Transform parent,string text,Vector2 min,Vector2 max,float size,TextAlignmentOptions alignment)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);
        label.alignment=alignment;
        label.enableAutoSizing=true;
        label.fontSizeMin=size-2;
        label.fontSizeMax=size;
        return label;
    }

    private void SultanCard(Transform parent,int choice,Vector2 min,Vector2 max)
    {
        bool submitted=false;
        var card=CampaignHUD.MakeButton(parent,"",min,max,()=>{
            if(submitted || !SultanNegotiations.Available(choice))return;
            submitted=true;
            if(choice==2){SultanGiftMenu();return;}
            bool ok=SultanNegotiations.Act(choice,out var report);
            if(!ok){submitted=false;notice.text=report;return;}
            if(choice==0){ReviewTerms();return;}
            if(SultanNegotiations.IsConversation(choice))
                CampaignHUD.Instance.ShowReport(SultanNegotiations.Titles[choice],report,()=>{CampaignHUD.Instance.DismissReport();Sultan();});
            else Finish(true,report);
        });
        card.name="Sultan choice "+choice;
        card.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);
        var heading=SultanText(card.transform,SultanNegotiations.Titles[choice],new Vector2(.045f,.73f),new Vector2(.955f,.96f),26,TextAlignmentOptions.Left);
        var description=SultanText(card.transform,SultanNegotiations.Effects[choice],new Vector2(.045f,.25f),new Vector2(.955f,.72f),20,TextAlignmentOptions.TopLeft);
        description.fontStyle=FontStyles.Italic;
        string unavailable=SultanNegotiations.Unavailable(choice);
        string action=unavailable ?? (choice==2?"Choose gift  ›":choice==0?"Request terms  ›":"Send message  ›");
        var status=SultanText(card.transform,action,new Vector2(.045f,.035f),new Vector2(.955f,.235f),19,TextAlignmentOptions.BottomLeft);
        card.interactable=unavailable==null;
        ImperialTheme.Button(card);
        if(choice==5||choice==6)TurnActionButton.Mark(card);
        status.color=unavailable==null?ImperialTheme.Gold:new Color(.64f,.64f,.61f);
        if(unavailable!=null){heading.color=new Color(.72f,.70f,.64f);description.color=new Color(.64f,.64f,.61f);}
    }
}
