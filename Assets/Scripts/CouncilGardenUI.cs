using UnityEngine;

public partial class CouncilUI
{
    public void Garden()=>Location(1);
    private void GardenTalk(int person,int topic)
    {
        if(!GardenConversations.Available(person)){Location(conversationLocation);return;}
        Open(GardenConversations.Name(person),new[]{"In the Palace","In the palace garden","In Hagia Sophia"}[conversationLocation]);ControlSingleton.Instance.ReturnAction=()=>Location(conversationLocation);FitConversation();
        CharacterPortraits.Draw(body,person,new Vector2(.025f,.35f),new Vector2(.29f,.97f));
        string text=topic==0?GardenConversations.Introduction(person):topic==1?GardenConversations.Siege(person):GardenConversations.Advice(person);
        ConversationText(text,new Vector2(.32f,.32f),new Vector2(.97f,.96f));
        string[] topics={"Tell me about yourself","What do you make of the siege?","What should I attend to?","Was there something you wished to ask me?"};
        for(int i=0;i<4;i++){int selected=i;float left=i%2==0?.03f:.515f,top=i<2?.27f:.14f;CampaignHUD.MakeButton(body,topics[i],new Vector2(left,top-.11f),new Vector2(left+.455f,top),()=>{if(selected==3)PersonalQuestion(person);else GardenTalk(person,selected);});}
    }
    private void PersonalQuestion(int person)
    {
        if(!GardenConversations.Available(person)){Location(conversationLocation);return;}
        var exchange=CharacterQuestions.For(person);if(exchange==null)return;
        int chosen=DailyLifeEvents.I("iLifeQuestionReply"+person)-1;
        Open(GardenConversations.Name(person),"Was there something you wished to ask me?");
        ControlSingleton.Instance.ReturnAction=()=>GardenTalk(person,0);FitConversation();
        CharacterPortraits.Draw(body,person,new Vector2(.025f,.44f),new Vector2(.27f,.97f));
        string passage=exchange.Question;
        if(chosen>=0&&chosen<exchange.Replies.Length)passage+="\n\n<b>Your reply</b>\n"+exchange.Replies[chosen]+"\n\n"+exchange.Responses[chosen];
        ConversationText(passage,new Vector2(.30f,chosen>=0?.20f:.42f),new Vector2(.97f,.97f));
        if(chosen<0)for(int i=0;i<3;i++){
            int answer=i;float top=.37f-i*.11f;
            CampaignHUD.MakeButton(body,exchange.Replies[i],new Vector2(.05f,top-.09f),new Vector2(.95f,top),()=>{VariableSingleton.SetInt("iLifeQuestionReply"+person,answer+1);PersonalQuestion(person);});
        }
        else CampaignHUD.MakeButton(body,"Return to our conversation",new Vector2(.15f,.05f),new Vector2(.85f,.15f),()=>GardenTalk(person,0));
    }

    private void FitConversation(){Canvas.ForceUpdateCanvases();BodyHeight(((RectTransform)body.parent).rect.height);}
    private void ConversationText(string text,Vector2 min,Vector2 max){var label=CampaignHUD.Explanation(body,text,min,max,25);label.enableAutoSizing=true;label.fontSizeMin=18;label.fontSizeMax=25;}

    private void FocusCard(float top)
    {
        var row=SpyPanel("Strategic focus",top,220);
        ImperialTheme.Icon(row.transform,Mathf.Clamp(CouncilActions.Int("iFocus"),0,4),new Vector2(.02f,.10f),new Vector2(.14f,.90f));
        CampaignHUD.Label(row.transform,"STRATEGIC FOCUS\n"+FocusIcon.Names[Mathf.Clamp(CouncilActions.Int("iFocus"),0,4)],new Vector2(.17f,.45f),new Vector2(.70f,.94f),26);
        CampaignHUD.Explanation(row.transform,FocusReorganisation.AdviserAvailable?"Sphrantzes: 2,000 gold · major action · 30 days between changes":"One change per campaign · 5,000 gold · major action",new Vector2(.17f,.08f),new Vector2(.70f,.43f),21);
        CampaignHUD.MakeButton(row.transform,"Review focus",new Vector2(.74f,.20f),new Vector2(.97f,.80f),ReviewFocus);
    }
    public void ReviewFocus()
    {
        Open("Change strategic focus","Completed preparations keep their rewards. Future focus-dependent choices use the new focus.");ControlSingleton.Instance.ReturnAction=Overview;BodyHeight(1000);
        for(int i=0;i<5;i++)
        {
            int target=i;var row=Row(i,200);ImperialTheme.Icon(row.transform,i,new Vector2(.025f,.12f),new Vector2(.14f,.88f));
            CampaignHUD.Label(row.transform,FocusIcon.Names[i],new Vector2(.17f,.40f),new Vector2(.68f,.90f),26);
            string reason=FocusReorganisation.Unavailable(i);
            var button=CampaignHUD.MakeButton(row.transform,reason??("Review · "+FocusReorganisation.Cost.ToString("N0")+" gold"),new Vector2(.71f,.15f),new Vector2(.98f,.85f),()=>ConfirmFocus(target));button.interactable=reason==null;
        }
    }
    public void IntelligenceBriefing()
    {
        Open("Reading the enemy's actions","The reported action is what happened. The council assessment explains its implications.");
        ControlSingleton.Instance.ReturnAction=Overview;BodyHeight(700);
        CampaignHUD.Explanation(body,SiegeIntelligenceBriefing.Text(),new Vector2(.04f,.08f),new Vector2(.96f,.96f),26);
    }
    private void ConfirmFocus(int focus)
    {
        Open("Confirm strategic change",FocusIcon.Names[Mathf.Clamp(CouncilActions.Int("iFocus"),0,4)]+" → "+FocusIcon.Names[focus]);ControlSingleton.Instance.ReturnAction=ReviewFocus;BodyHeight(440);
        CampaignHUD.Explanation(body,"Spend "+FocusReorganisation.Cost.ToString("N0")+" gold and this turn's major action. "+(FocusReorganisation.AdviserAvailable?"Sphrantzes allows another change after 30 days while he remains available.":"This uses your one ordinary focus change for the campaign.")+" Completed preparations are retained.",new Vector2(.05f,.38f),new Vector2(.95f,.93f),28);
        bool submitted=false;CampaignHUD.MakeButton(body,"Confirm change",new Vector2(.53f,.08f),new Vector2(.95f,.30f),()=>{if(submitted)return;submitted=true;bool ok=FocusReorganisation.Change(focus,out var report);if(!ok)submitted=false;Finish(ok,report);});
        CampaignHUD.MakeButton(body,"Keep current focus",new Vector2(.05f,.08f),new Vector2(.47f,.30f),ReviewFocus);
    }
}
