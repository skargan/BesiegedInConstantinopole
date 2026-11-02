using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Explicit presentation metadata: never infer time commitment from a caption.
[DisallowMultipleComponent]
public sealed class TurnActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public string Hint = "Time passes after this order.";
    static Sprite emblem;
    static TurnActionButton hintOwner;
    Button button;
    Image icon;
    TMP_Text[] captions;
    Vector2[] captionOffsets;
    GameObject tip;
    bool lastEnabled;

    public static void Mark(Button target,string hint=null)
    {
        if(target==null)return;
        var marker=target.GetComponent<TurnActionButton>();
        if(marker==null)marker=target.gameObject.AddComponent<TurnActionButton>();
        if(hint!=null)marker.Hint=hint;
        marker.Arrange();marker.Refresh();
    }
    public static void MarkBoundActions(Transform parent,string method,string hint=null)
    {
        if(parent==null)return;
        foreach(var target in parent.GetComponentsInChildren<Button>(true))
            for(int i=0;i<target.onClick.GetPersistentEventCount();i++)
                if(target.onClick.GetPersistentMethodName(i)==method)Mark(target,hint);
    }
    public static void Unmark(Button target)
    {
        var marker=target==null?null:target.GetComponent<TurnActionButton>();if(marker==null)return;
        marker.CloseHint();
        for(int i=0;marker.captions!=null&&i<marker.captions.Length;i++)if(marker.captions[i]!=null)marker.captions[i].rectTransform.offsetMax=marker.captionOffsets[i];
        if(marker.icon!=null){marker.icon.gameObject.SetActive(false);Destroy(marker.icon.gameObject);}
        marker.enabled=false;Destroy(marker);
    }
    public static Sprite Emblem {
        get {
            if(emblem==null){
                var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/turn-hourglass");
                if(texture!=null)emblem=Sprite.Create(texture,new Rect(texture.width*.185f,texture.height*.064f,texture.width*.63f,texture.height*.886f),new Vector2(.5f,.5f));
            }
            return emblem;
        }
    }
    void Awake()
    {
        button=GetComponent<Button>();captions=GetComponentsInChildren<TMP_Text>(true).Where(t=>t.gameObject.activeSelf&&t.enabled&&t.rectTransform.anchorMax.x>.8f).ToArray();
        captionOffsets=captions.Select(t=>t.rectTransform.offsetMax).ToArray();
        var go=new GameObject("Turn hourglass",typeof(RectTransform),typeof(Image),typeof(LayoutElement));
        go.transform.SetParent(transform,false);go.GetComponent<LayoutElement>().ignoreLayout=true;
        icon=go.GetComponent<Image>();icon.sprite=Emblem;icon.preserveAspect=true;icon.raycastTarget=false;
        Arrange();Refresh();
    }
    void OnRectTransformDimensionsChange(){Arrange();}
    void Arrange()
    {
        if(icon==null)return;
        icon.sprite=Emblem;
        var bounds=((RectTransform)transform).rect;
        float height=Mathf.Clamp(bounds.height*.66f,22,44),width=height*.60f;
        // Keep the emblem clear of the ornate frame at every button height.
        float inset=Mathf.Clamp(height*.65f,20,28),captionGap=10;
        var rect=icon.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(1,.5f);
        rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition=new Vector2(-inset-width*.5f,0);
        for(int i=0;i<captions.Length;i++)if(captions[i]!=null)captions[i].rectTransform.offsetMax=new Vector2(captionOffsets[i].x-width-inset-captionGap,captionOffsets[i].y);
    }
    void Refresh(){lastEnabled=button!=null&&button.IsInteractable();if(!lastEnabled)CloseHint();if(icon!=null)icon.color=lastEnabled?Color.white:new Color(.46f,.46f,.46f,.72f);}
    void LateUpdate(){if(button!=null&&button.IsInteractable()!=lastEnabled)Refresh();}
    public void OnPointerEnter(PointerEventData e){ShowHint();}
    public void OnPointerExit(PointerEventData e){CloseHint();}
    public void OnSelect(BaseEventData e){ShowHint();}
    public void OnDeselect(BaseEventData e){CloseHint();}
    void OnDisable(){CloseHint();}
    void OnDestroy(){CloseHint();}
    void ShowHint()
    {
        CloseHint();if(button==null||!button.IsInteractable())return;
        if(hintOwner!=null&&hintOwner!=this)hintOwner.CloseHint();
        var canvas=GetComponentInParent<Canvas>();if(canvas==null)return;
        canvas=canvas.rootCanvas;
        hintOwner=this;
        tip=new GameObject("Turn meaning",typeof(RectTransform),typeof(Canvas),typeof(Image));tip.transform.SetParent(canvas.transform,false);
        var layer=tip.GetComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=700;
        tip.GetComponent<Image>().color=ImperialTheme.Navy;tip.GetComponent<Image>().raycastTarget=false;ImperialTheme.Frame(tip);
        var rect=tip.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var parent=(RectTransform)canvas.transform;var area=parent.rect;
        const float margin=12,gap=10,padding=16;
        var label=CampaignHUD.Label(tip.transform,Hint,Vector2.zero,Vector2.one,19);label.alignment=TextAlignmentOptions.Center;
        label.enableWordWrapping=true;label.enableAutoSizing=false;label.raycastTarget=false;
        float maximumWidth=Mathf.Max(1,Mathf.Min(520,area.width-2*margin));
        float width=Mathf.Clamp(label.GetPreferredValues(Hint,float.PositiveInfinity,float.PositiveInfinity).x+2*padding,Mathf.Min(300,maximumWidth),maximumWidth);
        float wantedHeight=label.GetPreferredValues(Hint,Mathf.Max(1,width-2*padding),float.PositiveInfinity).y+24;
        float height=Mathf.Min(Mathf.Max(44,wantedHeight),Mathf.Max(1,area.height-2*margin));
        rect.sizeDelta=new Vector2(width,height);
        label.rectTransform.offsetMin=new Vector2(padding,12);label.rectTransform.offsetMax=new Vector2(-padding,-12);
        if(wantedHeight>height){label.enableAutoSizing=true;label.fontSizeMin=14;label.fontSizeMax=19;}
        var corners=new Vector3[4];((RectTransform)transform).GetWorldCorners(corners);
        var points=corners.Select(c=>(Vector2)parent.InverseTransformPoint(c)).ToArray();
        float left=points.Min(p=>p.x),right=points.Max(p=>p.x),bottom=points.Min(p=>p.y),top=points.Max(p=>p.y);
        float x=(left+right)*.5f,y=bottom-gap-height*.5f;
        // Prefer below; flip above when a low button leaves insufficient room.
        if(y-height*.5f<area.yMin+margin)y=top+gap+height*.5f;
        if(y+height*.5f>area.yMax-margin){
            // Very tall controls can leave more room beside them than above/below.
            y=(bottom+top)*.5f;
            x=right+gap+width<=area.xMax-margin?right+gap+width*.5f:left-gap-width*.5f;
        }
        rect.anchoredPosition=new Vector2(Mathf.Clamp(x,area.xMin+margin+width*.5f,area.xMax-margin-width*.5f),Mathf.Clamp(y,area.yMin+margin+height*.5f,area.yMax-margin-height*.5f))-area.center;
    }
    void CloseHint(){if(tip!=null){tip.SetActive(false);Destroy(tip);tip=null;}if(hintOwner==this)hintOwner=null;}
}
