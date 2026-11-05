using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public partial class CouncilUI
{
    TMP_Text ionianSpeech;
    public void IonianCourt()=>IonianPage("main");
    void IonianPage(string page)
    {
        Open("The Ionian Isles","");
        ControlSingleton.Instance.ReturnAction=page=="main"?(Action)DiplomaticRelations:IonianCourt;
        foreach(Transform child in screen.transform)child.gameObject.SetActive(false);
        var art=new GameObject("Ionian coast",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));art.transform.SetParent(screen.transform,false);
        var rect=art.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Diplomacy/Ionian/Coast");art.GetComponent<RawImage>().texture=texture;art.GetComponent<RawImage>().raycastTarget=false;
        var fit=art.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?1672f/941f:(float)texture.width/texture.height;
        bool visit=IonianDiplomacy.Resident;
        ionianSpeech=null;
        if(visit&&IonianDiplomacy.Resident){IonianDiplomacy.Introduce();IonianFigure(art.transform);}
        var panel=CampaignHUD.Panel(art.transform,"Ionian diplomatic panel",new Vector2(.63f,.035f),new Vector2(.985f,.965f));ImperialTheme.Panel(panel,false);
        panel.GetComponent<Image>().color=new Color(.018f,.035f,.05f,.96f);
        var root=panel.transform;
        notice=IonianLabel(root,"",.085f,.15f,18);notice.color=new Color(1f,.76f,.48f);
        IonianLabel(root,page=="main"?"THE IONIAN ISLES":page=="realm"?"The realm and its interests":page=="envoy"?"An envoy to the islands":"Raimondina Ventimiglia",.875f,.975f,27);
        if(page=="main") {
            IonianLabel(root,"The Tocco lordships\n"+CountryDiplomacy.EnvoyStatus(IonianDiplomacy.Country),.735f,.86f,20);
            IonianLabel(root,"These islands offer shelter and private understandings. They do not send an ordinary relief expedition.",.57f,.72f,21);
            IonianButton(root,"The realm and its interests",.46f,.55f,()=>IonianPage("realm"));
            if(IonianDiplomacy.Resident) {
                IonianDiplomacy.Introduce();
                IonianButton(root,"Speak with Raimondina",.35f,.44f,()=>IonianPage("visit"));
                IonianButton(root,"Envoy · review assignment",.24f,.33f,()=>IonianPage("envoy"));
            } else if(IonianDiplomacy.Introduced) {
                IonianButton(root,"Envoy · review assignment",.35f,.44f,()=>IonianPage("envoy"));
            } else {
                IonianButton(root,"Reestablish contact with my first wife's family",.35f,.44f,()=>IonianPage("visit"));
            }
            IonianLabel(root,IonianDiplomacy.Standing,.155f,.23f,19);
        } else if(page=="realm")IonianReading(root,IonianText.Data.realm,.17f,.86f);
        else if(page=="envoy")IonianEnvoy(root);
        else if(page=="shelter"||page=="exile")IonianAgreement(root,page=="exile");
        else if(page=="about"||page=="siege") {
            IonianReading(root,page=="about"?IonianText.Data.about:IonianText.Data.siege,.27f,.86f);
            IonianButton(root,"Return to our conversation",.17f,.25f,()=>IonianPage("visit"));
        } else IonianVisit(root);
        IonianButton(root,page=="main"?"Return to diplomacy":"Return to the Ionian Isles",.02f,.08f,()=>{if(page=="main")DiplomaticRelations();else IonianCourt();},19);
    }
    void IonianVisit(Transform root)
    {
        if(!IonianDiplomacy.Resident) {
            IonianReading(root,"The terrace is quiet. Beyond it, a fishing boat moves between the islands.\n\nA resident envoy can reestablish contact with your first wife’s family and introduce you to Raimondina, who speaks for the island lords. There is nothing further to arrange here until your envoy arrives.",.32f,.86f);
            bool assigned=CountryDiplomacy.EnvoyState(IonianDiplomacy.Country)!=CountryDiplomacy.EnvoyAssignment.None;
            var send=CampaignHUD.MakeTurnButton(root,assigned?"Review envoy assignment":"Send envoy",new Vector2(.05f,.19f),new Vector2(.95f,.29f),()=>{
                if(assigned){IonianPage("envoy");return;}
                bool ok=CountryDiplomacy.Dispatch(IonianDiplomacy.Country,out var report);Finish(ok,report);
            });
            string reason=assigned?null:CountryDiplomacy.DispatchReason(IonianDiplomacy.Country);
            send.interactable=reason==null;notice.text=reason??"";return;
        }
        IonianLabel(root,"A private correspondence through your envoy",.80f,.87f,18);
        IonianButton(root,"Tell me about yourself",.70f,.78f,()=>IonianPage("about"));
        IonianButton(root,"What do you make of our siege?",.60f,.68f,()=>IonianPage("siege"));
        var shelter=IonianButton(root,IonianDiplomacy.Shelter?"Refugee shelter · agreed":"Secure shelter for refugees",.50f,.58f,()=>IonianPage("shelter"));shelter.interactable=!IonianDiplomacy.Shelter;
        CampaignChoiceTooltip.Attach(shelter,"1,000 gold · support falls by 3 points in each constituency.");
        var exile=IonianButton(root,IonianDiplomacy.Exile?"Personal exile · agreed":"Arrange personal exile",.40f,.48f,()=>IonianPage("exile"));exile.interactable=!IonianDiplomacy.Exile;
        CampaignChoiceTooltip.Attach(exile,"1,000 gold · support falls by 10 points in each constituency.");
        IonianButton(root,"Court Raimondina · under development",.30f,.38f,()=>{}).interactable=false;
        IonianButton(root,"A naval base · under development",.20f,.28f,()=>{}).interactable=false;
    }
    void IonianEnvoy(Transform root)
    {
        int country=IonianDiplomacy.Country;
        bool assigned=CountryDiplomacy.EnvoyState(country)!=CountryDiplomacy.EnvoyAssignment.None;
        string reason=assigned?CountryDiplomacy.RecallReason(country):CountryDiplomacy.DispatchReason(country);
        IonianReading(root,CountryDiplomacy.EnvoyStatus(country)+"\n\n"+(assigned?"Recall your envoy. Existing shelter and exile agreements remain in force; new negotiations wait for a resident envoy.":"Assign one available envoy to introduce your interests to the island lords. A private audience and agreements become available after arrival.")+"\n\nJourney: "+CountryDiplomacy.Definitions[country].EnvoyDays+" calendar days. No gold or support cost. Confirming uses your major action.",.29f,.86f);
        var confirm=CampaignHUD.MakeTurnButton(root,assigned?"Recall envoy":"Send envoy",new Vector2(.05f,.17f),new Vector2(.95f,.26f),()=>{string report;bool ok=assigned?CountryDiplomacy.Recall(country,out report):CountryDiplomacy.Dispatch(country,out report);Finish(ok,report);});
        confirm.interactable=reason==null;notice.text=reason??"";
    }
    void IonianAgreement(Transform root,bool personal)
    {
        int penalty=personal?10:3;
        IonianReading(root,(personal?IonianText.Data.exile:IonianText.Data.shelter)+"\n\nThis agreement is permanent and can be made only once.",.29f,.86f);
        string reason=IonianDiplomacy.AgreementReason(personal);notice.text=reason??"";
        var confirm=CampaignHUD.MakeTurnButton(root,"Confirm agreement",new Vector2(.05f,.17f),new Vector2(.95f,.26f),()=>{bool ok=IonianDiplomacy.Agree(personal,out var report);Finish(ok,report);});confirm.interactable=reason==null;CampaignChoiceTooltip.Attach(confirm,"1,000 gold · one major action · Army, Public, Clergy and Noble support −"+penalty+" points each.");
    }
    void IonianFigure(Transform art)
    {
        var holder=CampaignHUD.Panel(art,"Raimondina sprite position",new Vector2(.13f,.035f),new Vector2(.58f,.98f));holder.GetComponent<Image>().color=Color.clear;holder.GetComponent<Image>().raycastTarget=false;
        var figure=new GameObject("Raimondina separate sprite",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));figure.transform.SetParent(holder.transform,false);
        var r=figure.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);
        var image=figure.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Diplomacy/Ionian/Raimondina");image.raycastTarget=false;
        var fit=figure.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=2f/3f;
        var shape=new[]{new Vector2(.43f,.985f),new Vector2(.60f,.99f),new Vector2(.67f,.90f),new Vector2(.73f,.82f),new Vector2(.75f,.66f),new Vector2(.80f,.48f),new Vector2(.81f,.08f),new Vector2(.74f,.015f),new Vector2(.13f,.015f),new Vector2(.10f,.07f),new Vector2(.23f,.36f),new Vector2(.23f,.63f),new Vector2(.26f,.80f),new Vector2(.36f,.92f)};
        var hotspot=LocationHotspot.Create(figure.transform,"Raimondina Ventimiglia",shape,()=>IonianSpeak());
        var trigger=hotspot.gameObject.AddComponent<CharacterChatterTrigger>();trigger.actor=CharacterChatter.Raimondina;trigger.speak=IonianSpeak;trigger.hotspot=hotspot;
        var caption=CampaignHUD.Panel(art,"Raimondina remark",new Vector2(.025f,.025f),new Vector2(.60f,.30f));ImperialTheme.Panel(caption,false);caption.GetComponent<Image>().color=new Color(.015f,.035f,.05f,.94f);
        // Opening another page restores the current remark without spending another one.
        string current=CharacterChatter.Current(CharacterChatter.Raimondina,IonianText.Lines,CharacterChatter.RaimondinaFarewell);
        ionianSpeech=CampaignHUD.Label(caption.transform,current.Length==0?"Speak with Raimondina, or move the pointer over her.":"“"+current+"”",new Vector2(.04f,.32f),new Vector2(.96f,.96f),22);ionianSpeech.enableAutoSizing=true;ionianSpeech.fontSizeMin=18;ionianSpeech.fontSizeMax=22;
        var speak=CampaignHUD.MakeButton(caption.transform,"Speak with Raimondina",new Vector2(.15f,.04f),new Vector2(.85f,.28f),()=>IonianSpeak());
        var buttonTrigger=speak.gameObject.AddComponent<CharacterChatterTrigger>();buttonTrigger.actor=CharacterChatter.Raimondina;buttonTrigger.speak=IonianSpeak;
    }
    void IonianSpeak(){if(ionianSpeech!=null)ionianSpeech.text="“"+CharacterChatter.Request(CharacterChatter.Raimondina,IonianText.Lines,CharacterChatter.RaimondinaFarewell)+"”";}
    static TMP_Text IonianLabel(Transform parent,string text,float bottom,float top,float size)
    {
        var t=CampaignHUD.Label(parent,text,new Vector2(.05f,bottom),new Vector2(.95f,top),size);t.enableAutoSizing=true;t.fontSizeMin=16;t.fontSizeMax=size;t.alignment=TextAlignmentOptions.MidlineLeft;return t;
    }
    static Button IonianButton(Transform parent,string text,float bottom,float top,UnityEngine.Events.UnityAction action,float size=21)
    {
        var b=CampaignHUD.MakeButton(parent,text,new Vector2(.05f,bottom),new Vector2(.95f,top),action);var t=b.GetComponentInChildren<TMP_Text>();t.enableAutoSizing=true;t.fontSizeMin=16;t.fontSizeMax=size;return b;
    }
    static void IonianReading(Transform parent,string text,float bottom,float top)
    {
        var view=CampaignHUD.Panel(parent,"Ionian readable passage",new Vector2(.05f,bottom),new Vector2(.92f,top));view.GetComponent<Image>().color=Color.clear;view.AddComponent<RectMask2D>();
        var scroll=view.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=25;scroll.viewport=view.GetComponent<RectTransform>();
        var label=CampaignHUD.Label(view.transform,text,new Vector2(.02f,1),new Vector2(.98f,1),21);label.alignment=TextAlignmentOptions.TopLeft;label.rectTransform.pivot=new Vector2(.5f,1);label.margin=new Vector4(0,5,0,15);label.overflowMode=TextOverflowModes.Overflow;
        label.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=label.rectTransform;
        CampaignHUD.AddScrollbar(scroll,parent,new Vector2(.94f,bottom),new Vector2(.96f,top));
    }
}
