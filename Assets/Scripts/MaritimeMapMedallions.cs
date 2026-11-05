using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Presentation only: these indicators never alter missions, passage, RNG or time.
public sealed class MaritimeMapMedallions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler, ICanvasRaycastFilter
{
    public bool Passage;
    TMP_Text caption, hint;
    Transform detail;
    RawImage artwork;
    Outline glow;
    Action clearTerritoryHover;
    string currentAsset;
    bool highlighted;
    const string DefaultHint="";
    static GameObject popup;

    // Preserve the established outnumbering rule. Equal forces remain contested.
    // Ottoman advantage is a description of the balance, not a new supremacy bonus.
    public static string SeaAsset(int ours,int theirs)=>ours>theirs?"sea-imperial":theirs>ours?"sea-ottoman":"sea-contested";
    public static string SeaTitle(int ours,int theirs)=>ours>theirs?"Uncontested seas":"Contested seas";
    public static bool PassageOpen=>VariableSingleton.GetBoolVariable("bStraitsOpen");
    public static string PassageTitle=>PassageOpen?"Passage open":"Passage interdicted";

    public static void Add(Transform map,Transform information,TMP_Text hover,Action clearHover)
    {
        Create(map,information,hover,clearHover,false,new Vector2(.185f,.20f));
        Create(map,information,hover,clearHover,true,new Vector2(.505f,.76f));
    }
    static void Create(Transform parent,Transform information,TMP_Text hover,Action clearHover,bool passage,Vector2 anchor)
    {
        var go=new GameObject(passage?"Bosphorus medallion":"Sea control medallion",typeof(RectTransform),typeof(RawImage),typeof(Button),typeof(Outline),typeof(MaritimeMapMedallions));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=new Vector2(82,82);
        var badge=go.GetComponent<MaritimeMapMedallions>();badge.Passage=passage;badge.detail=information;badge.hint=hover;badge.clearTerritoryHover=clearHover;
        badge.artwork=go.GetComponent<RawImage>();badge.glow=go.GetComponent<Outline>();badge.glow.effectColor=new Color(1,.80f,.28f,.95f);badge.glow.effectDistance=new Vector2(2,2);badge.glow.enabled=false;
        var button=go.GetComponent<Button>();button.targetGraphic=badge.artwork;button.transition=Selectable.Transition.None;button.onClick.AddListener(badge.Activate);
        badge.caption=CampaignHUD.Label(go.transform,"",Vector2.zero,Vector2.one,16);
        var labelRect=badge.caption.rectTransform;labelRect.anchorMin=labelRect.anchorMax=new Vector2(.5f,0);labelRect.pivot=new Vector2(.5f,1);labelRect.anchoredPosition=new Vector2(0,-3);labelRect.sizeDelta=new Vector2(142,54);
        badge.caption.alignment=TextAlignmentOptions.Top;badge.caption.fontStyle=FontStyles.Bold;
        badge.caption.gameObject.AddComponent<Shadow>().effectColor=new Color(0,0,0,.95f);
        badge.Refresh();badge.caption.gameObject.SetActive(false);
    }
    void Update(){Refresh();}
    void Refresh()
    {
        if(artwork==null)return;
        int ours=CouncilActions.Int("iShips"),theirs=NavalState.EnemyShips;
        string asset=Passage?(PassageOpen?"strait-open":"strait-interdicted"):SeaAsset(ours,theirs);
        if(currentAsset!=asset){currentAsset=asset;artwork.texture=Resources.Load<Texture2D>("UI/Maps/Maritime/"+asset);}
        caption.text=Passage?"Bosphorus\n"+PassageTitle:"Sea control\n"+NavalState.SeaControl;
        if(highlighted&&hint!=null)hint.text=HintText();
    }
    string HintText()
    {
        if(Passage)return "Bosphorus · "+PassageTitle;
        return "Sea control · "+NavalState.SeaControl;
    }
    void Highlight(bool on)
    {
        highlighted=on;if(glow!=null)glow.enabled=on;if(caption!=null)caption.gameObject.SetActive(on);
        if(on){clearTerritoryHover?.Invoke();if(hint!=null)hint.text=HintText();}
        else if(hint!=null&&hint.text==HintText())hint.text=DefaultHint;
    }
    public void OnPointerEnter(PointerEventData e)=>Highlight(true);
    public void OnPointerExit(PointerEventData e)=>Highlight(false);
    public void OnSelect(BaseEventData e)=>Highlight(true);
    public void OnDeselect(BaseEventData e)=>Highlight(false);
    void OnDisable(){Highlight(false);}
    public bool IsRaycastLocationValid(Vector2 screen,Camera camera)
    {
        var rect=(RectTransform)transform;
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,screen,camera,out var p))return false;
        p-=rect.rect.center;return p.x*p.x/(rect.rect.width*rect.rect.width*.25f)+p.y*p.y/(rect.rect.height*rect.rect.height*.25f)<=1;
    }
    public void Activate()
    {
        if(popup!=null)Destroy(popup);
        popup=new GameObject("Maritime status",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=popup.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=240;
        var scaler=popup.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
        var dim=CampaignHUD.Panel(popup.transform,"Backdrop",Vector2.zero,Vector2.one);dim.GetComponent<Image>().color=new Color(0,0,0,.65f);
        var card=CampaignHUD.Panel(popup.transform,"Maritime status card",new Vector2(.28f,.16f),new Vector2(.72f,.84f));ImperialTheme.Panel(card,false);
        CampaignHUD.Label(card.transform,Passage?"Bosphorus Passage":"Control of the Sea",new Vector2(.06f,.86f),new Vector2(.94f,.97f),32).alignment=TextAlignmentOptions.Center;
        var picture=new GameObject("Enlarged maritime emblem",typeof(RectTransform),typeof(RawImage));picture.transform.SetParent(card.transform,false);
        var rect=(RectTransform)picture.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.63f);rect.sizeDelta=new Vector2(205,205);
        picture.GetComponent<RawImage>().texture=artwork.texture;picture.GetComponent<RawImage>().raycastTarget=false;
        CampaignHUD.Label(card.transform,Passage?PassageTitle:NavalState.SeaControl,new Vector2(.06f,.36f),new Vector2(.94f,.45f),28).alignment=TextAlignmentOptions.Center;
        string explanation=Passage?(PassageOpen?"The Black Sea passage is currently open.":"The Black Sea passage is currently interdicted. Opening it permits Black Sea voyages."):
            "Our fleet: "+CouncilActions.Int("iShips")+" ships · Ottoman fleet: "+NavalState.EnemyShips+" ships";
        var text=CampaignHUD.Explanation(card.transform,explanation,new Vector2(.08f,.20f),new Vector2(.92f,.35f),21);text.alignment=TextAlignmentOptions.Center;text.enableAutoSizing=true;text.fontSizeMin=17;
        CampaignHUD.MakeButton(card.transform,"Fleet management",new Vector2(.07f,.05f),new Vector2(.57f,.15f),()=>{Destroy(popup);popup=null;OpenFleet();});
        CampaignHUD.MakeButton(card.transform,"Return",new Vector2(.62f,.05f),new Vector2(.93f,.15f),()=>{Destroy(popup);popup=null;});
    }

    static void OpenFleet()
    {
        var council=FindObjectOfType<CouncilUI>(true);if(council==null)return;
        WarTheatreUI.Close();council.Fleet();
        ControlSingleton.Instance.ReturnAction=()=>{council.Hide();WarTheatreUI.Show();};
    }
}
