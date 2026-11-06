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
        if(!TheodoroVisit.Admitted){TheodoroGate();return;}
        TheodoroBusiness();
    }
    void TheodoroBusiness()
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
        DiplomaticNames.Bind(TheodoroText("THEODORO · MANGUP",.90f,.985f,28),"theodoro");
        TheodoroText(CountryDiplomacy.Attitude(country)+" · "+CountryDiplomacy.EnvoyStatus(country),.82f,.90f,19);
        TheodoroButton("An audience with Maria Asanina",.72f,.81f,MariaAudience);
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
    void MariaAudience()
    {
        TheodoroBusiness();
        foreach(Transform child in theodoroDesk){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        var art=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/theodoro-maria-palace");
        if(art!=null)theodoroDesk.parent.GetComponent<UnityEngine.UI.RawImage>().texture=art;
        TheodoroText("MARIA ASANINA",.88f,.97f,28);
        TheodoroText("Mistress of the palace",.80f,.87f,20);
        TheodoroButton("Tell me about yourself",.65f,.75f,()=>MariaConversation(0));
        TheodoroButton("Tell me about your people",.52f,.62f,()=>MariaConversation(1));
        TheodoroButton("What do you think of the Genoese?",.39f,.49f,()=>MariaConversation(2));
        TheodoroButton("The affairs of Theodoro",.27f,.36f,TheodoroCourt);
        TheodoroButton("The wall garden",.16f,.24f,TheodoroGarden);
        TheodoroButton("The chamber of seals",.06f,.14f,TheodoroSeals);
        TheodoroPalaceDoors();
        ControlSingleton.Instance.ReturnAction=TheodoroCourt;
    }
    void MariaConversation(int topic)
    {
        MariaAudience();
        foreach(Transform child in theodoroDesk){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        string[] titles={"Tell me about yourself","Tell me about your people","What do you think of the Genoese?"};
        string[] words={
            "“I am Maria Asanina, a humble servant of my lord Olubey. For three years he has lain under a dark witch’s spell: without a word, without a movement. I attend to his affairs until his strength returns.”\n\nShe rests one hand upon the arm of her chair.\n\n“The petitions cannot wait beside his bed. Nor can the harvest, or the men who guard our roads. Someone must hear them. His seal remains here, and his household is cared for.”\n\nFor a moment she looks towards the inner door. “When he wakes, I intend that he shall find everything in order.”",
            "“In winter the wind finds every loose shutter in this palace. My attendants blame the carpenters; the carpenters blame the mountain. Both ask me for money.”\n\n“Our priests pray in Greek. Down in the market you will hear other tongues, and a man who cannot bargain in his neighbour’s language soon learns to count on his fingers. Families keep their customs. They also borrow one another’s recipes and quarrel over the same springs.”\n\n“From these heights the coast looks very close. It is a long road when you must bring salt up it, or carry your wounded home. Remember that when someone tells you our cliffs make us safe.”",
            "“Those plague rats.”\n\nShe notices your expression. “Literally. Their ships brought the sickness into our ports.”\n\n“I had heard it travelled the other way—from Asia, through these ports.”\n\nMaria gives a short, angry laugh. “Then you have heard their account. They arrive from Europe with their holds and their vermin, and somehow the blame belongs to everyone else.”\n\nShe draws a finger along the coast on a small map. “A warehouse becomes a claim. A debt becomes a harbour. Now there is this Bank of Saint George. I wonder how much of our country its clerks have already promised one another.”"
        };
        TheodoroText(titles[topic],.88f,.98f,25);
        TheodoroText(words[topic],.17f,.86f,21);
        TheodoroButton("Return to Maria",.035f,.125f,MariaAudience);
        ControlSingleton.Instance.ReturnAction=MariaAudience;
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
