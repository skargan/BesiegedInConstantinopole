using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Theodoro encounter; agreements use existing goodwill, quality and sea contracts.
public partial class CouncilUI
{
    Transform theodoroDesk;
    public void TheodoroCourt()
    {
        int country=CountryDiplomacy.Index("theodoro");if(country<0){DiplomaticRelations();return;}
        Open("Theodoro · Mangup","");ControlSingleton.Instance.ReturnAction=DiplomaticRelations;
        foreach(Transform child in screen.transform)child.gameObject.SetActive(false);
        var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/theodoro-mangup");
        var painting=new GameObject("Theodoro painting",typeof(RectTransform),typeof(UnityEngine.UI.RawImage),typeof(UnityEngine.UI.AspectRatioFitter));painting.transform.SetParent(screen.transform,false);
        var rect=(RectTransform)painting.transform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        var image=painting.GetComponent<UnityEngine.UI.RawImage>();image.texture=texture;image.raycastTarget=false;
        var fit=painting.GetComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?16f/9:texture.width/(float)texture.height;
        var pane=CampaignHUD.Panel(painting.transform,"Theodoro encounter",new Vector2(.575f,.025f),new Vector2(.985f,.975f));ImperialTheme.Panel(pane,false);theodoroDesk=pane.transform;
        TheodoroText("THEODORO · MANGUP",.90f,.985f,28);
        TheodoroText(CountryDiplomacy.Attitude(country)+" · "+CountryDiplomacy.EnvoyStatus(country),.82f,.90f,19);
        TheodoroButton("Realm and its interests",.72f,.81f,()=>TheodoroConversation(0));
        TheodoroButton("Speak of old imperial ties",.61f,.70f,()=>TheodoroAgreement(0));
        TheodoroButton("Discuss cooperation in archery",.50f,.59f,()=>TheodoroAgreement(1));
        var incorporation=TheodoroButton("Reincorporation into the Empire · In development",.40f,.48f,()=>{});incorporation.interactable=false;
        TheodoroButton("A cavalry contingent · 80 riders",.30f,.38f,()=>TheodoroAgreement(2));
        // Preserve access to ordinary existing business without introducing a second aid/envoy system.
        var aid=DailyLifeEvents.I("iAidState"+country)==1?DiplomaticOrder.Expedite:DiplomaticOrder.RequestAid;
        var envoy=CountryDiplomacy.EnvoyState(country)==CountryDiplomacy.EnvoyAssignment.None?DiplomaticOrder.Dispatch:DiplomaticOrder.Recall;
        var aidButton=CampaignHUD.MakeButton(theodoroDesk,OrderTitle(aid),new Vector2(.04f,.19f),new Vector2(.485f,.28f),()=>ReviewDiplomaticOrder(country,aid));aidButton.interactable=OrderReason(country,aid)==null;
        var envoyButton=CampaignHUD.MakeButton(theodoroDesk,OrderTitle(envoy),new Vector2(.515f,.19f),new Vector2(.96f,.28f),()=>ReviewDiplomaticOrder(country,envoy));envoyButton.interactable=OrderReason(country,envoy)==null;
        TheodoroText("Archery cooperation: "+(TheodoroDiplomacy.Archery?"agreed · +5% lethality":"not yet agreed")+".",.105f,.18f,18);
        TheodoroButton("Return",.025f,.095f,DiplomaticRelations);
    }
    void TheodoroAgreement(int action)
    {
        TheodoroCourt();foreach(Transform child in theodoroDesk){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        string[] titles={"Renew imperial ties","Archery cooperation","The Mangup Retinue"};
        string[] terms={"Letters and assurances would renew the old understanding between Mangup and Constantinople.\n\nTheodoro goodwill +10 points. No gold payment. Once per campaign.","Theodoro offers instructors to work with the City's archers.\n\nCost: 1,500 gold.\nArcher lethality +5% for this campaign. This agreement cannot be repeated.","Eighty mounted retainers offer their service under a captain from Mangup. They assemble for Black Sea passage through Caffa.\n\n80 cavalry · 24-day baseline journey.\nTotal price: 10,800 gold. Pay 5,400 now and 5,400 on arrival.\nRequires Dismounted Champions. Arrival without incident: "+MercenaryContracts.SafeChance(TheodoroDiplomacy.Company)+"%. Existing voyage hazards, delays and arrival-payment rules apply. One contract per campaign."};
        TheodoroText(titles[action],.87f,.975f,27);TheodoroText(terms[action],.30f,.85f,22);
        var status=TheodoroText(TheodoroDiplomacy.Reason(action)??"Review the terms before confirming.",.20f,.29f,18);
        bool submitted=false;var confirm=TheodoroButton("Confirm agreement",.105f,.19f,()=>{if(submitted)return;if(TheodoroDiplomacy.Agree(action,out var report)){submitted=true;VariableSingleton.Instance.Save();TheodoroCourt();CampaignHUD.Instance.ShowReport("News from Theodoro",report,()=>CampaignHUD.Instance.DismissReport());}else status.text=report;});confirm.interactable=TheodoroDiplomacy.Reason(action)==null;
        TheodoroButton("Cancel",.025f,.095f,TheodoroCourt);ControlSingleton.Instance.ReturnAction=TheodoroCourt;
    }
    void TheodoroConversation(int topic)
    {
        TheodoroCourt();foreach(Transform child in theodoroDesk){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        string[] titles={"The realm and its interests","Old imperial ties","The bow and the saddle"};
        string[] passages={
            "Mangup stands above the roads and valleys of southwestern Crimea. Its cliffs give a small principality what a larger country might buy with an army: time to see trouble coming.\n\nTheodoro's Orthodox court belongs to the same religious world as Constantinople, but its neighbours include Genoese trading settlements and the Crimean Khanate. Sympathy for the emperor must share the table with local needs.\n\nThe palace looks familiar until you notice how much has travelled here from elsewhere. Merchants, soldiers and neighbours have all left something behind. This is a court with old connections to your world, and a life of its own.",
            "Your envoy presents the imperial letter. The reply is courteous, and rather less ceremonial.\n\n‘Our priests will understand your prayers without an interpreter. That does not mean our neighbours will let us answer them.’\n\nThe official sets the letter beside a map of the coast. ‘Tell us what you need, Majesty. Then tell us which promises you could still keep if your City survives. We shall have a more useful conversation.’\n\nA renewed understanding may become possible through a resident envoy and an open passage across the Black Sea. Your envoy will need more than a courteous letter to bring the court to an agreement.",
            "A captain turns a short bow in his hands before passing it to the envoy.\n\n‘The horse is not an inconvenience to be left at the gate. It carries the archer out of reach of the man who would cut him down.’\n\nHe asks about your cavalry, the ground outside the walls, and whether a sortie would find an open road home. These questions matter more to him than an elegant shooting display.\n\nInstructors and cooperation in mounted archery could give your cavalry another purpose. The captain is willing to discuss it. An agreement has yet to be settled."
        };
        TheodoroText(titles[topic],.87f,.975f,27);
        TheodoroText(passages[topic],.18f,.85f,22);
        TheodoroButton("Return to the court",.04f,.13f,TheodoroCourt);
        ControlSingleton.Instance.ReturnAction=TheodoroCourt;
    }
    TMP_Text TheodoroText(string text,float low,float high,int size)
    {
        var label=CampaignHUD.Label(theodoroDesk,text,new Vector2(.04f,low),new Vector2(.96f,high),size);label.enableAutoSizing=true;label.fontSizeMin=18;label.fontSizeMax=size;label.alignment=TextAlignmentOptions.TopLeft;return label;
    }
    UnityEngine.UI.Button TheodoroButton(string text,float low,float high,UnityEngine.Events.UnityAction action)
        =>CampaignHUD.MakeButton(theodoroDesk,text,new Vector2(.04f,low),new Vector2(.96f,high),action);
}
