using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    void VisitScene(string asset)
    {
        TheodoroBusiness();
        foreach(Transform child in theodoroDesk){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        var image=theodoroDesk.parent.GetComponent<RawImage>();
        image.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+asset);
    }
    LocationHotspot TheodoroHot(string label,UnityEngine.Events.UnityAction action,params float[] xy)
    {
        var points=new Vector2[xy.Length/2];
        for(int i=0;i<points.Length;i++)points[i]=new Vector2(xy[i*2],1-xy[i*2+1]);
        return LocationHotspot.Create(theodoroDesk.parent,label,points,action);
    }
    void TheodoroGate()
    {
        VisitScene("theodoro-gate");
        TheodoroText("THE GATE OF MANGUP",.88f,.98f,27);
        TheodoroText(TheodoroVisit.Admitted?"The captain recognises you and signals to the gatekeeper.":"The young captain bows without leaving the gateway. His companion watches your escort.\n\n‘My lady receives few visitors. Give me an answer, Majesty, and I shall announce you. Or leave a gift for the household, and spare us both the examination.’",.54f,.85f,22);
        if(TheodoroVisit.Admitted)TheodoroButton("Enter the palace",.40f,.50f,MariaAudience);
        else {
            TheodoroButton("Let me hear your question",.40f,.50f,TheodoroRiddle);
            TheodoroButton("Offer 3,000 gold",.28f,.38f,TheodoroGift);
        }
        TheodoroButton("Speak to the Tatar guard",.16f,.26f,TheodoroRetainer);
        TheodoroButton("Return to the map",.035f,.125f,ReturnToTheatre);
        ControlSingleton.Instance.ReturnAction=ReturnToTheatre;
        // Visible doorway and both figures have independent contours, in painting coordinates.
        TheodoroHot("The palace gate",()=>{if(TheodoroVisit.Admitted)MariaAudience();else TheodoroRiddle();},
            .257f,.639f,.256f,.163f,.270f,.140f,.294f,.124f,.327f,.116f,.357f,.122f,.386f,.140f,.410f,.165f,.433f,.201f,.451f,.244f,.463f,.292f,.470f,.346f,.473f,.639f);
        TheodoroHot("The captain",TheodoroRiddle,
            .028f,.886f,.021f,.852f,.029f,.768f,.028f,.704f,.020f,.647f,.026f,.594f,.034f,.507f,.047f,.421f,.054f,.350f,.067f,.280f,.085f,.227f,.107f,.203f,.129f,.194f,.128f,.169f,.135f,.153f,.133f,.131f,.146f,.103f,.164f,.091f,.181f,.101f,.190f,.127f,.187f,.153f,.181f,.178f,.181f,.198f,.203f,.214f,.218f,.247f,.224f,.303f,.222f,.373f,.233f,.423f,.231f,.487f,.226f,.525f,.230f,.596f,.239f,.675f,.243f,.764f,.232f,.807f,.214f,.829f,.204f,.881f,.224f,.914f,.226f,.929f,.203f,.940f,.177f,.927f,.165f,.895f,.157f,.862f,.142f,.849f,.132f,.896f,.127f,.948f,.116f,.966f,.096f,.968f,.088f,.951f,.089f,.908f,.076f,.896f,.050f,.896f);
        TheodoroHot("Hoş keldiñiz.",TheodoroRetainer,
            .253f,.882f,.264f,.825f,.267f,.773f,.256f,.737f,.258f,.675f,.251f,.613f,.260f,.562f,.271f,.530f,.266f,.489f,.248f,.449f,.245f,.412f,.258f,.340f,.277f,.294f,.295f,.282f,.299f,.259f,.296f,.228f,.303f,.195f,.308f,.165f,.309f,.137f,.326f,.167f,.350f,.188f,.361f,.213f,.356f,.248f,.355f,.276f,.374f,.300f,.382f,.350f,.390f,.410f,.409f,.459f,.432f,.471f,.437f,.497f,.422f,.515f,.407f,.500f,.391f,.481f,.389f,.553f,.399f,.617f,.399f,.679f,.410f,.746f,.405f,.810f,.400f,.865f,.408f,.889f,.406f,.904f,.378f,.907f,.356f,.891f,.346f,.848f,.337f,.807f,.315f,.812f,.308f,.855f,.295f,.899f,.276f,.918f,.260f,.916f);
    }
    void TheodoroRiddle()
    {
        if(TheodoroVisit.Admitted){MariaAudience();return;}
        VisitScene("theodoro-gate");
        TheodoroText("THE CAPTAIN'S QUESTION",.88f,.98f,27);
        TheodoroText("‘What is broken the moment you speak its name?’\n\nHe waits. The other guard appears to have heard this question rather often.",.64f,.84f,23);
        var box=CampaignHUD.Panel(theodoroDesk,"Theodoro riddle answer",new Vector2(.04f,.50f),new Vector2(.96f,.60f));ImperialTheme.Panel(box,false);
        var input=box.AddComponent<TMP_InputField>();
        var value=CampaignHUD.Label(box.transform,"",new Vector2(.03f,.1f),new Vector2(.97f,.9f),24);value.richText=false;value.raycastTarget=false;
        input.textViewport=(RectTransform)box.transform;input.textComponent=(TextMeshProUGUI)value;input.targetGraphic=box.GetComponent<Image>();input.characterLimit=60;
        input.customCaretColor=true;input.caretColor=ImperialTheme.Gold;
        var response=TheodoroText("",.30f,.45f,20);
        UnityEngine.Events.UnityAction submit=()=>{if(TheodoroVisit.Enter(false,input.text,out string reply))TheodoroGate();else response.text=reply;};
        input.onSubmit.AddListener(_=>submit());
        TheodoroButton("Answer",.18f,.28f,submit);
        TheodoroButton("Return",.035f,.125f,TheodoroGate);ControlSingleton.Instance.ReturnAction=TheodoroGate;
    }
    void TheodoroGift()
    {
        VisitScene("theodoro-gate");TheodoroText("A GIFT FOR THE HOUSEHOLD",.88f,.98f,26);
        TheodoroText("The captain names a sum without looking at his companion.\n\n3,000 gold will secure admission to the palace.",.55f,.82f,23);
        var message=TheodoroText("",.32f,.48f,21);
        var pay=TheodoroButton("Pay 3,000 gold",.18f,.28f,()=>{if(TheodoroVisit.Enter(true,null,out string reply))TheodoroGate();else message.text=reply;});
        pay.interactable=DailyLifeEvents.I("iMoney")>=TheodoroVisit.Gift&&!TheodoroVisit.Admitted;
        TheodoroButton("Return",.035f,.125f,TheodoroGate);ControlSingleton.Instance.ReturnAction=TheodoroGate;
    }
    void TheodoroRetainer()
    {
        VisitScene("theodoro-gate");TheodoroText("THE CAPTAIN'S COMPANION",.88f,.98f,26);
        TheodoroText("‘Hoş keldiñiz.’\nWelcome.\n\nThe greeting is civil. His glance at the captain is less so.\n\nHe says something else in his own language. Your interpreter hesitates, then leans towards you. ‘He says the young lord has a question for every visitor, and a purse for every answer.’\n\nThe captain smiles. ‘Our friend has travelled widely. He has acquired some unfortunate manners.’",.24f,.84f,22);
        TheodoroButton("Return",.035f,.125f,TheodoroGate);ControlSingleton.Instance.ReturnAction=TheodoroGate;
    }
    void TheodoroPalaceDoors()
    {
        TheodoroHot("The wall garden",TheodoroGarden,0f,.41f,0f,.069f,.021f,.019f,.040f,0f,.077f,0f,.109f,.022f,.131f,.060f,.140f,.108f,.137f,.166f,.131f,.221f,.133f,.285f,.115f,.310f,.088f,.361f,.079f,.417f);
        TheodoroHot("The chamber of seals",TheodoroSeals,.478f,.614f,.478f,.241f,.483f,.179f,.495f,.139f,.513f,.112f,.531f,.086f,.549f,.112f,.567f,.138f,.575f,.175f,.575f,.615f);
    }
    void TheodoroGarden()
    {
        VisitScene("theodoro-garden");
        TheodoroText("THE WALL GARDEN",.88f,.98f,27);
        TheodoroText("A traveller draws circles in the dust with the butt of his spear. Beyond the wall, the roads disappear between pale cliffs.\n\nHe looks up before you speak.",.61f,.83f,22);
        TheodoroButton("Tell me about yourself",.45f,.55f,()=>TheodoroTraveller(-1));
        TheodoroButton("Tell me about your faith",.32f,.42f,()=>TheodoroTraveller(0));
        TheodoroButton("Return to Maria",.035f,.125f,MariaAudience);ControlSingleton.Instance.ReturnAction=MariaAudience;
        TheodoroHot("Return to the palace",MariaAudience,.057f,.640f,.057f,.220f,.065f,.163f,.081f,.125f,.103f,.105f,.126f,.095f,.148f,.116f,.168f,.158f,.183f,.214f,.193f,.288f,.195f,.590f,.162f,.612f,.106f,.638f);
        TheodoroHot("Tenzin Dorje",()=>TheodoroTraveller(-1),.149f,.845f,.151f,.816f,.173f,.761f,.185f,.703f,.198f,.661f,.208f,.601f,.210f,.535f,.226f,.479f,.251f,.446f,.269f,.442f,.276f,.424f,.279f,.399f,.293f,.379f,.314f,.375f,.330f,.388f,.336f,.412f,.336f,.443f,.329f,.461f,.327f,.479f,.349f,.491f,.376f,.510f,.396f,.538f,.404f,.583f,.413f,.648f,.411f,.692f,.402f,.732f,.407f,.779f,.401f,.817f,.380f,.828f,.350f,.805f,.341f,.787f,.329f,.814f,.315f,.851f,.291f,.855f,.262f,.861f,.229f,.855f,.195f,.862f,.170f,.853f);
    }
    void TheodoroTraveller(int step)
    {
        VisitScene("theodoro-garden");TheodoroText("TENZIN DORJE",.88f,.98f,27);
        string[] words={
            "‘Tenzin Dorje. I was born east of the great mountains. I have crossed more roads than I care to count, and the sea you call Black.’\n\nHe brushes a stone from the circle.\n\n‘A merchant brought me here. The lady gave me a bed, and this little patch of earth. I am looking for a place where I might learn to leave suffering behind. So far, I have brought rather a lot of it with me.’",
            "‘I have heard that yours is a faith of great peace.’\n\n‘That is one way to put it.’ He keeps drawing. ‘We are taught to cease harming living things. To see how anger and desire make us suffer, and how readily we pass that suffering on.’\n\nHe turns the spear in his hands. ‘I have killed eighteen people. Eleven after I took the robe.’",
            "‘And how do you reconcile that with your vows?’\n\n‘Badly.’\n\nFor the first time his hand stops. ‘I told myself that some men spread suffering wherever they go, and that ending their lives would end the harm. It is a convenient thought when one already holds a weapon.’\n\n‘Do your teachers accept it?’\n\n‘Mine would have sent me away. You asked what I have done, Majesty. I would not have you mistake it for what I was taught.’",
            "‘Then what peace have you found?’\n\n‘There were rulers in the east who honoured our teachers and still sent armies to kill. Their roads grew quiet when the fighting ended. Men called that peace.’\n\nHe draws another circle around the first.\n\n‘Sometimes a great death is followed by a great calm. A graveyard is a quiet place. I am still learning whether I have ever known anything better.’\n\nThe spear moves through the dust again."
        };
        TheodoroText(words[step+1],.24f,.84f,22);
        if(step>=0&&step<2)TheodoroButton(step==0?"And your vows?":"What peace have you found?",.15f,.235f,()=>TheodoroTraveller(step+1));
        TheodoroButton("Return to the garden",.035f,.12f,TheodoroGarden);ControlSingleton.Instance.ReturnAction=TheodoroGarden;
    }
    void TheodoroSeals()
    {
        VisitScene("theodoro-seals");TheodoroText("THE CHAMBER OF SEALS",.88f,.98f,27);
        TheodoroText("Greek letters, Genoese accounts and leather packets from the steppe lie on the same table. The seals outlast the men who pressed them into wax.\n\nAn old clerk has left three documents open: a marriage settlement, a disputed harbour toll and a pledge of cavalry. The same tract of land appears in all three.\n\nBeneath the table stands a locked chest. The door beyond it is barred from the other side.",.26f,.84f,22);
        TheodoroButton("Return to Maria",.035f,.125f,MariaAudience);ControlSingleton.Instance.ReturnAction=MariaAudience;
        TheodoroHot("Return to the palace",MariaAudience,.006f,.531f,.006f,.190f,.015f,.145f,.032f,.114f,.052f,.094f,.075f,.098f,.096f,.126f,.109f,.167f,.114f,.220f,.114f,.524f);
        // The second door is deliberately scenery for later expansion, not a dead button.
    }
}
