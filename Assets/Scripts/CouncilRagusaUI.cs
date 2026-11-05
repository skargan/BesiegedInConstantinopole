using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public partial class CouncilUI
{
    // Cosmetic counters deliberately do not consume campaign random numbers or write to saves.
    readonly int[] ragusaRemarkCounter=new int[2];
    int ragusaSpeaking=-1;
    TMP_Text ragusaRemark;
    LocationHotspot[] ragusaFigures;

    public void RagusanCourt()=>RagusanPage("main");

    void RagusanPage(string page)
    {
        bool resident=RagusaDiplomacy.EnvoyPresent;
        if(!resident&&page!="main"&&page!="realm")page="main";
        Open("Ragusa","");
        ControlSingleton.Instance.ReturnAction=page=="main"?(Action)RagusaReturnToMap:RagusanCourt;
        foreach(Transform child in screen.transform)child.gameObject.SetActive(false);
        int representative=RagusaDiplomacy.ActiveRepresentative;
        var scene=RagusaText.Painting(representative==0?"gazulus":"cotrugli");
        var painting=new GameObject("Ragusan court painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));
        painting.transform.SetParent(screen.transform,false);
        var paintingRect=painting.GetComponent<RectTransform>();
        paintingRect.anchorMin=paintingRect.anchorMax=new Vector2(.5f,.5f);paintingRect.pivot=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>(!resident?"UI/Diplomacy/Ragusa/WaitingTerrace":scene==null?"UI/Diplomacy/Ragusa/GazulusForeground":scene.resource);
        var image=painting.GetComponent<RawImage>();image.texture=texture;image.raycastTarget=false;
        var fit=painting.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
        fit.aspectRatio=texture==null?1672f/941f:(float)texture.width/texture.height;

        ragusaSpeaking=-1;ragusaFigures=new LocationHotspot[2];
        if(resident&&scene!=null&&scene.characters!=null) {
            var figures=(RagusaText.Figure[])scene.characters.Clone();
            Array.Sort(figures,(a,b)=>a.hitPriority.CompareTo(b.hitPriority));
            foreach(var figure in figures) {
                if(figure.polygon==null||figure.polygon.Length<3)continue;
                int index=figure.id=="gazulus"?0:1;if(index!=representative)continue;string id=figure.id;
                var points=new Vector2[figure.polygon.Length];
                for(int n=0;n<points.Length;n++)points[n]=new Vector2(figure.polygon[n].x,figure.polygon[n].y);
                var hotspot=LocationHotspot.Create(painting.transform,RagusaText.Person(id).name,points,()=>RagusanPage(id));
                ragusaFigures[index]=hotspot;
                var speech=hotspot.gameObject.AddComponent<RagusaHoverSpeech>();
                speech.enter=()=>RagusaSpeak(index);speech.leave=()=>RagusaStopSpeaking(index);
            }
        }
        ragusaRemark=null;
        if(resident){
            var caption=CampaignHUD.Panel(painting.transform,"Ragusan conversation",new Vector2(.025f,.10f),new Vector2(.585f,.26f));
            caption.GetComponent<Image>().color=new Color(.015f,.035f,.05f,.94f);ImperialTheme.Panel(caption,false);
            ragusaRemark=RagusaLabel(caption.transform,"",new Vector2(.035f,.08f),new Vector2(.965f,.94f),20);caption.SetActive(false);
            int index=representative;string id=index==0?"gazulus":"cotrugli";
            var speak=RagusaButton(painting.transform,RagusaDiplomacy.ActiveRepresentativeName,new Vector2(.10f,.035f),new Vector2(.56f,.085f),()=>RagusanPage(id),18);
            speak.name="Ragusan current negotiator";
            var accessibility=speak.gameObject.AddComponent<RagusaHoverSpeech>();
            accessibility.enter=()=>{RagusaSpeak(index);if(ragusaFigures[index]!=null)ragusaFigures[index].SetHighlighted(true);};
            accessibility.leave=()=>{RagusaStopSpeaking(index);if(ragusaFigures[index]!=null)ragusaFigures[index].SetHighlighted(false);};
        }

        var desk=CampaignHUD.Panel(painting.transform,"Ragusan diplomatic desk",new Vector2(.62f,.035f),new Vector2(.985f,.965f));
        ImperialTheme.Panel(desk,false);desk.GetComponent<Image>().color=new Color(.018f,.035f,.055f,.96f);
        var root=desk.transform;
        notice=RagusaLabel(root,"",new Vector2(.045f,.085f),new Vector2(.955f,.145f),17);
        notice.color=new Color(1f,.72f,.45f);
        if(page=="main")RagusaMain(root);
        else if(page=="food"||page=="war")RagusaReview(root,page=="war");
        else {
            string title,text;
            if(page=="realm") {title=RagusaText.Data.realmTitle??"Ragusa — the realm and its interests";text=RagusaText.Data.realmText??"";}
            else {var person=RagusaText.Person(page);title=person.name;text=person.role+"\n\n"+person.description+"\n\n"+RagusaText.Get(page+"_opening")+"\n\n"+RagusaText.Get("chair_"+page);}
            RagusaLabel(root,title,new Vector2(.045f,.865f),new Vector2(.955f,.965f),27);
            RagusaReadingPanel(root,text,new Vector2(.045f,.17f),new Vector2(.955f,.84f),22);
        }
        RagusaButton(root,page=="main"?"Return to War Beyond the Walls":page=="food"||page=="war"?"Cancel and return to Ragusa":"Return to Ragusa",new Vector2(.045f,.02f),new Vector2(.955f,.08f),()=>{if(page=="main")RagusaReturnToMap();else RagusanCourt();},20);
    }

    void RagusaReturnToMap(){Hide();WarTheatreUI.Show();}

    void RagusaMain(Transform root)
    {
        int country=CountryDiplomacy.Index("ragusa");
        RagusaLabel(root,"THE REPUBLIC OF RAGUSA",new Vector2(.045f,.91f),new Vector2(.955f,.975f),27);
        if(RagusaDiplomacy.EnvoyPresent)RagusaLabel(root,RagusaDiplomacy.ActiveRepresentativeName,new Vector2(.045f,.815f),new Vector2(.955f,.905f),22);
        string goodwill="Ragusan goodwill "+RagusaDiplomacy.Relations.ToString("0")+" · "+CountryDiplomacy.Attitude(country);
        if(RagusaDiplomacy.EnvoyPresent)goodwill+="\nPersonal favour "+RagusaDiplomacy.Favour(RagusaDiplomacy.ActiveRepresentative);
        RagusaLabel(root,goodwill,new Vector2(.045f,.71f),new Vector2(.955f,.81f),20);
        if(RagusaDiplomacy.EnvoyPresent)RagusaButton(root,"Arrange a private provision shipment",new Vector2(.045f,.615f),new Vector2(.955f,.695f),()=>RagusanPage("food"),21);
        RagusaLabel(root,RagusaDiplomacy.FoodStatus,new Vector2(.045f,.505f),new Vector2(.955f,.605f),19);
        if(RagusaDiplomacy.EnvoyPresent)RagusaButton(root,"Hire out troops to Ragusa",new Vector2(.045f,.41f),new Vector2(.955f,.49f),()=>RagusanPage("war"),21);
        RagusaLabel(root,RagusaDiplomacy.ContractStatus,new Vector2(.045f,.255f),new Vector2(.955f,.40f),19);
        RagusaButton(root,"The realm and its interests",new Vector2(.045f,.175f),new Vector2(.955f,.245f),()=>RagusanPage("realm"),20);
        bool assigned=CountryDiplomacy.EnvoyState(country)!=CountryDiplomacy.EnvoyAssignment.None;
        var order=assigned?DiplomaticOrder.Recall:DiplomaticOrder.Dispatch;
        var envoy=RagusaButton(root,assigned?"Envoy: "+CountryDiplomacy.EnvoyStatus(country)+" · review":"Send an envoy · review",new Vector2(.045f,.095f),new Vector2(.955f,.165f),()=>ReviewDiplomaticOrder(country,order),18);
        envoy.interactable=OrderReason(country,order)==null;
        // Dedicated agreements replace the generic request-aid / request-war controls for Ragusa.
    }

    void RagusaReview(Transform root,bool war)
    {
        string reason=war?RagusaDiplomacy.ContractReason():RagusaDiplomacy.FoodRequestReason();
        string title=war?"A Ragusan service contract":"A private provision shipment";
        string terms;
        if(war)terms="Commit 500 infantry, 500 archers and 200 cavalry. They leave the city's available forces immediately.\n\nReceive "+RagusaDiplomacy.SigningGold.ToString("N0")+" gold now, +"+RagusaDiplomacy.SigningRelations+" Ragusan goodwill and +"+RagusaDiplomacy.SigningRelations+" favour with the current representative.\n\nThe contract is resolved after "+RagusaDiplomacy.ContractDays+" calendar days: an equal chance of victory or defeat.\n\nVICTORY\nReceive "+RagusaDiplomacy.VictoryGold.ToString("N0")+" more gold, +"+RagusaDiplomacy.VictoryRelations+" goodwill and +"+RagusaDiplomacy.VictoryRelations+" favour with the representative who signed. Each troop type independently loses 1–20%: 5–100 infantry, 5–100 archers, 2–40 cavalry.\n\nDEFEAT\nNo further payment. Each troop type independently loses 1–50%: 5–250 infantry, 5–250 archers, 2–100 cavalry.\n\nSurvivors take another "+RagusaDiplomacy.ReturnDays+" calendar days to return. A second contract must wait for them.\n\nConfirming uses your major action and advances the turn.";
        else terms="Pay "+RagusaDiplomacy.FoodPrice.ToString("N0")+" gold for "+RagusaDiplomacy.FoodQuantity+" provisions, including freight.\n\nRagusa formally refuses state assistance. A private merchant nevertheless arranges delivery: this agreement has no acceptance roll.\n\nExpected arrival: "+VariableSingleton.GetDate().AddDays(RagusaDiplomacy.FoodTravelDays).ToString("d MMM yyyy")+" ("+RagusaDiplomacy.FoodTravelDays+" calendar days). The ship travels by sea and can be delayed or lost in a storm. Its transport vessel does not join your fleet.\n\nOne arrangement per calendar month; an earlier shipment must arrive or be lost before another can be agreed.\n\nConfirming uses your major action and advances the turn.";
        RagusaLabel(root,title,new Vector2(.045f,.87f),new Vector2(.955f,.965f),27);
        RagusaReadingPanel(root,terms,new Vector2(.045f,.27f),new Vector2(.955f,.85f),21);
        notice.text=reason??"";
        bool submitted=false;
        var confirm=CampaignHUD.MakeTurnButton(root,"Confirm agreement",new Vector2(.045f,.17f),new Vector2(.955f,.25f),()=>{
            if(submitted)return;
            string report;bool success=war?RagusaDiplomacy.StartContract(out report):RagusaDiplomacy.RequestFood(out report);
            submitted=success;Finish(success,report);
        });
        confirm.interactable=reason==null;ImperialTheme.Button(confirm);
        var label=confirm.GetComponentInChildren<TMP_Text>();label.fontSizeMax=22;label.fontSizeMin=18;
    }

    void RagusaSpeak(int index)
    {
        if(!RagusaDiplomacy.EnvoyPresent||index!=RagusaDiplomacy.ActiveRepresentative||ragusaSpeaking==index||ragusaRemark==null)return;
        ragusaSpeaking=index;var person=RagusaText.Person(index==0?"gazulus":"cotrugli");
        string line=person.description;
        if(person.hover!=null&&person.hover.Length>0)line=person.hover[(ragusaRemarkCounter[index]++)%person.hover.Length];
        ragusaRemark.transform.parent.gameObject.SetActive(true);
        ragusaRemark.text="<b>"+person.name+"</b>\n“"+line+"”";
    }
    void RagusaStopSpeaking(int index){if(ragusaSpeaking==index){ragusaSpeaking=-1;if(ragusaRemark!=null)ragusaRemark.transform.parent.gameObject.SetActive(false);}}

    static TMP_Text RagusaLabel(Transform root,string text,Vector2 min,Vector2 max,float size)
    {
        var label=CampaignHUD.Label(root,text,min,max,size);label.enableAutoSizing=true;label.fontSizeMax=size;label.fontSizeMin=Mathf.Max(16,size-4);
        label.alignment=TextAlignmentOptions.MidlineLeft;label.overflowMode=TextOverflowModes.Ellipsis;return label;
    }
    static Button RagusaButton(Transform root,string text,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action,float size)
    {
        var button=CampaignHUD.MakeButton(root,text,min,max,action);ImperialTheme.Button(button);
        var label=button.GetComponentInChildren<TMP_Text>();label.fontSizeMax=size;label.fontSizeMin=Mathf.Max(16,size-4);return button;
    }
    static void RagusaReadingPanel(Transform parent,string text,Vector2 min,Vector2 max,float size)
    {
        var view=CampaignHUD.Panel(parent,"Ragusan readable passage",min,max);view.GetComponent<Image>().color=new Color(.01f,.025f,.04f,.55f);
        view.AddComponent<RectMask2D>();var scroll=view.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
        scroll.viewport=view.GetComponent<RectTransform>();
        var content=CampaignHUD.Label(view.transform,text,new Vector2(.025f,1),new Vector2(.965f,1),size);
        content.alignment=TextAlignmentOptions.TopLeft;content.rectTransform.pivot=new Vector2(.5f,1);content.rectTransform.anchoredPosition=Vector2.zero;
        content.margin=new Vector4(0,6,0,12);content.overflowMode=TextOverflowModes.Overflow;
        var fit=content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        scroll.content=content.rectTransform;
        var bar=CampaignHUD.Panel(parent,"Ragusan passage scroll",new Vector2(max.x-.012f,min.y),max);bar.GetComponent<Image>().color=new Color(.2f,.18f,.13f,.7f);
        var scrollbar=bar.AddComponent<Scrollbar>();scrollbar.direction=Scrollbar.Direction.BottomToTop;
        var handle=CampaignHUD.Panel(bar.transform,"Gold scroll handle",Vector2.zero,Vector2.one);handle.GetComponent<Image>().color=new Color(.72f,.55f,.25f,1);
        scrollbar.handleRect=handle.GetComponent<RectTransform>();scrollbar.targetGraphic=handle.GetComponent<Image>();scroll.verticalScrollbar=scrollbar;
        scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
    }
}

// Enter/select fires once; pointer movement never changes dialogue or campaign state.
public sealed class RagusaHoverSpeech : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler
{
    public Action enter,leave;
    public void OnPointerEnter(PointerEventData e)=>enter?.Invoke();
    public void OnPointerExit(PointerEventData e)=>leave?.Invoke();
    public void OnSelect(BaseEventData e)=>enter?.Invoke();
    public void OnDeselect(BaseEventData e)=>leave?.Invoke();
}
