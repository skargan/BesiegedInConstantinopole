using TMPro;
using UnityEngine;

public partial class CouncilUI
{
    private void KjoldTalk(int topic=0)
    {
        if(!GardenConversations.Available(GardenConversations.Kjold)){Location(conversationLocation);return;}
        string text=topic==1?KjoldConversations.SecurityAssessment():topic==2?KjoldConversations.Text.guardHistory:KjoldConversations.Text.introduction;
        if(topic==1){KjoldConversations.HearSecurity();SaveGames.Checkpoint();}
        Open("Kjold",new[]{"In the Palace","In the Palace Garden","In Hagia Sophia"}[conversationLocation]);
        ControlSingleton.Instance.ReturnAction=()=>Location(conversationLocation);FitConversation();
        CharacterPortraits.Draw(body,GardenConversations.Kjold,new Vector2(.025f,.12f),new Vector2(.29f,.97f));
        KjoldReading(text);
        CampaignHUD.MakeButton(body,"Tell me about yourself",new Vector2(.32f,.22f),new Vector2(.635f,.31f),()=>KjoldTalk(0));
        CampaignHUD.MakeButton(body,"How do you assess my security?",new Vector2(.65f,.22f),new Vector2(.97f,.31f),()=>KjoldTalk(1));
        CampaignHUD.MakeButton(body,"Tell me about the Varangian Guard",new Vector2(.32f,.10f),new Vector2(.635f,.19f),()=>KjoldTalk(2));
        if(KjoldConversations.SecurityHeard)CampaignHUD.MakeButton(body,"I have orders",new Vector2(.65f,.10f),new Vector2(.97f,.19f),KjoldOrders);
    }
    private void KjoldReading(string passage)
    {
        var view=CampaignHUD.Panel(body,"Kjold reading viewport",new Vector2(.32f,.35f),new Vector2(.97f,.97f));
        view.AddComponent<UnityEngine.UI.RectMask2D>();
        var content=CampaignHUD.Panel(view.transform,"Kjold reading content",Vector2.zero,Vector2.one);
        content.GetComponent<UnityEngine.UI.Image>().color=Color.clear;content.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        var cr=content.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0,1);cr.anchorMax=Vector2.one;cr.pivot=new Vector2(.5f,1);cr.offsetMin=cr.offsetMax=Vector2.zero;
        var text=CampaignHUD.Explanation(content.transform,passage,Vector2.zero,Vector2.one,23);
        text.margin=new Vector4(16,12,24,12);text.enableWordWrapping=true;
        Canvas.ForceUpdateCanvases();float width=view.GetComponent<RectTransform>().rect.width;
        cr.sizeDelta=new Vector2(0,Mathf.Max(view.GetComponent<RectTransform>().rect.height,text.GetPreferredValues(passage,Mathf.Max(1,width-40),float.PositiveInfinity).y+28));
        var scroll=view.AddComponent<UnityEngine.UI.ScrollRect>();scroll.content=cr;scroll.viewport=view.GetComponent<RectTransform>();scroll.horizontal=false;scroll.scrollSensitivity=35;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
        CampaignHUD.AddScrollbar(scroll,view.transform,new Vector2(.98f,0),Vector2.one);
    }
    private void KjoldOrders()
    {
        if(!KjoldCourtOrders.Available){KjoldTalk(0);return;}
        Open("Kjold · Your orders","“Of course, my emperor. What are your orders?”");ControlSingleton.Instance.ReturnAction=()=>KjoldTalk(1);FitConversation();
        CharacterPortraits.Draw(body,GardenConversations.Kjold,new Vector2(.03f,.08f),new Vector2(.30f,.96f));
        var actions=new[]{KjoldCourtOrders.Sentence.Exile,KjoldCourtOrders.Sentence.Imprison,KjoldCourtOrders.Sentence.Execute};
        string[] labels={"Exile a courtier","Imprison a courtier","Execute a courtier"};
        for(int i=0;i<actions.Length;i++){var choice=actions[i];float top=.85f-i*.20f;CampaignHUD.MakeButton(body,labels[i],new Vector2(.36f,top-.15f),new Vector2(.95f,top),()=>KjoldTargets(choice));}
        CampaignHUD.MakeButton(body,"Nothing for now",new Vector2(.36f,.10f),new Vector2(.95f,.25f),()=>KjoldTalk(1));
    }
    private void KjoldTargets(KjoldCourtOrders.Sentence sentence)
    {
        if(!KjoldCourtOrders.Available){KjoldTalk(0);return;}
        Open("Kjold · "+sentence,"Choose a character currently serving at your court.");ControlSingleton.Instance.ReturnAction=KjoldOrders;
        var targets=KjoldCourtOrders.Targets();BodyHeight(Mathf.Max(200,targets.Count*140));
        for(int i=0;i<targets.Count;i++){
            int person=targets[i];var row=Row(i,140);IllustratedIdentity.Portrait(row.transform,person);
            CampaignHUD.Label(row.transform,KjoldCourtOrders.Name(person),new Vector2(.20f,.25f),new Vector2(.65f,.80f),26);
            CampaignHUD.MakeButton(row.transform,"Review sentence",new Vector2(.70f,.23f),new Vector2(.97f,.78f),()=>KjoldConfirm(person,sentence));
        }
        if(targets.Count==0)CampaignHUD.Label(body,"No eligible courtiers are in your service.",new Vector2(.05f,.25f),new Vector2(.95f,.8f),25);
    }
    private void KjoldConfirm(int person,KjoldCourtOrders.Sentence sentence)
    {
        Open(KjoldCourtOrders.Name(person)+" · "+sentence,"Review your order before it is carried out.");ControlSingleton.Instance.ReturnAction=()=>KjoldTargets(sentence);FitConversation();
        CharacterPortraits.Draw(body,person,new Vector2(.04f,.20f),new Vector2(.31f,.96f));
        ConversationText(KjoldCourtOrders.Review(person,sentence),new Vector2(.36f,.30f),new Vector2(.95f,.95f));
        bool submitted=false;
        CampaignHUD.MakeButton(body,"Confirm sentence",new Vector2(.65f,.08f),new Vector2(.95f,.23f),()=>{
            if(submitted)return;
            if(!KjoldCourtOrders.CarryOut(person,sentence,out var report)){notice.text=report;return;}
            submitted=true;SaveGames.Checkpoint();Hide();CampaignHUD.Instance.ShowReport("Order carried out",report,KjoldOrders);
        });
        CampaignHUD.MakeButton(body,"Cancel",new Vector2(.36f,.08f),new Vector2(.62f,.23f),()=>KjoldTargets(sentence));
    }
    private void CourtSentenceRow(int person,int position)
    {
        var row=Row(position,230);IllustratedIdentity.Portrait(row.transform,person);
        CampaignHUD.Label(row.transform,KjoldCourtOrders.Name(person),new Vector2(.20f,.68f),new Vector2(.97f,.94f),29);
        CampaignHUD.Explanation(row.transform,"Imperial service ended. Unavailable for conversation or command.",new Vector2(.20f,.10f),new Vector2(.61f,.60f),22);
        var stamp=CampaignHUD.Panel(row.transform,"Court sentence stamp",new Vector2(.65f,.25f),new Vector2(.95f,.63f));
        stamp.GetComponent<UnityEngine.UI.Image>().color=KjoldCourtOrders.For(person)==KjoldCourtOrders.Sentence.Execute?new Color(.28f,.055f,.045f):new Color(.12f,.16f,.20f);
        ImperialTheme.Frame(stamp);CampaignHUD.Label(stamp.transform,KjoldCourtOrders.Stamp(person),new Vector2(.03f,.05f),new Vector2(.97f,.95f),25).alignment=TextAlignmentOptions.Center;
    }
    private void CourtKjoldRow(int position)
    {
        var row=Row(position,230);IllustratedIdentity.Portrait(row.transform,GardenConversations.Kjold);
        CampaignHUD.Label(row.transform,"Kjold · Varangian officer",new Vector2(.20f,.72f),new Vector2(.97f,.96f),28);
        CampaignHUD.Explanation(row.transform,KjoldConversations.Text.biography,new Vector2(.20f,.06f),new Vector2(.97f,.69f),20).enableAutoSizing=true;
    }
}
