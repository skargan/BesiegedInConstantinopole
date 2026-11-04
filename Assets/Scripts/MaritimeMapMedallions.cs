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
    const string DefaultHint="Hover for gold outline · Click a territory · Scroll to zoom · Drag to explore";

    // Preserve the established outnumbering rule. Equal forces remain contested.
    // Ottoman advantage is a description of the balance, not a new supremacy bonus.
    public static string SeaAsset(int ours,int theirs)=>ours>theirs?"sea-imperial":theirs>ours?"sea-ottoman":"sea-contested";
    public static string SeaTitle(int ours,int theirs)=>ours>theirs?"Uncontested seas":"Contested seas";
    public static bool PassageOpen=>VariableSingleton.GetBoolVariable("bStraitsOpen");
    public static string PassageTitle=>PassageOpen?"Passage open":"Passage interdicted";

    public static void Add(Transform map,Transform information,TMP_Text hover,Action clearHover)
    {
        Create(map,information,hover,clearHover,false,new Vector2(.185f,.20f));
        Create(map,information,hover,clearHover,true,new Vector2(.485f,.76f));
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
        badge.Refresh();
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
        if(Passage)return "Bosphorus · "+PassageTitle+" · Click for Straitcutter Fortress and route details";
        return NavalState.SeaControl+" · Fleet strength around Constantinople · Click for Fleet management";
    }
    void Highlight(bool on)
    {
        highlighted=on;if(glow!=null)glow.enabled=on;
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
        if(!Passage){OpenFleet();return;}
        foreach(Transform child in detail){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        clearTerritoryHover?.Invoke();
        CampaignHUD.Label(detail,"Bosphorus · "+PassageTitle,new Vector2(.02f,.72f),new Vector2(.73f,.97f),28);
        string body=PassageOpen
            ?"The Straitcutter Fortress no longer bars the route. Grain voyages from Crimea and Black Sea purchases are available when their other requirements are met.\nEnemy fleets can still threaten shipping. The Golden Horn chain protects a different entrance."
            :"The Straitcutter Fortress bars the Black Sea route. Grain voyages from Crimea and Black Sea purchases require the straits to be opened.\nThe selectable preparation Assault on the Straitcutter Fortress can open this passage. Naval superiority alone does not open it.";
        var text=CampaignHUD.Explanation(detail,body,new Vector2(.02f,.06f),new Vector2(.73f,.70f),20);text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=20;
        CampaignHUD.MakeButton(detail,"Fleet management",new Vector2(.76f,.44f),new Vector2(.98f,.83f),OpenFleet);
        CampaignHUD.Label(detail,"Inspecting the passage\ndoes not advance time.",new Vector2(.76f,.06f),new Vector2(.98f,.38f),17);
    }
    static void OpenFleet()
    {
        var council=FindObjectOfType<CouncilUI>(true);if(council==null)return;
        WarTheatreUI.Close();council.Fleet();
        ControlSingleton.Instance.ReturnAction=()=>{council.Hide();WarTheatreUI.Show();};
    }
}
