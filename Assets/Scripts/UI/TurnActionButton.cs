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
        var rect=icon.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(1,.5f);
        rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition=new Vector2(-12-width*.5f,0);
        for(int i=0;i<captions.Length;i++)if(captions[i]!=null)captions[i].rectTransform.offsetMax=new Vector2(captionOffsets[i].x-width-18,captionOffsets[i].y);
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
        var canvas=GetComponentInParent<Canvas>();if(canvas==null)return;
        canvas=canvas.rootCanvas;
        tip=new GameObject("Turn meaning",typeof(RectTransform),typeof(Canvas),typeof(Image));tip.transform.SetParent(canvas.transform,false);
        var layer=tip.GetComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=700;
        tip.GetComponent<Image>().color=ImperialTheme.Navy;tip.GetComponent<Image>().raycastTarget=false;ImperialTheme.Frame(tip);
        var rect=tip.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(300,44);
        var corners=new Vector3[4];((RectTransform)transform).GetWorldCorners(corners);
        var parent=(RectTransform)canvas.transform;var local=(Vector2)parent.InverseTransformPoint((corners[0]+corners[3])*.5f);
        local.y-=30;var area=parent.rect;
        rect.anchoredPosition=new Vector2(Mathf.Clamp(local.x,area.xMin+158,area.xMax-158),Mathf.Clamp(local.y,area.yMin+30,area.yMax-30));
        var label=CampaignHUD.Label(tip.transform,Hint,new Vector2(.04f,.08f),new Vector2(.96f,.92f),19);label.alignment=TextAlignmentOptions.Center;
    }
    void CloseHint(){if(tip!=null){tip.SetActive(false);Destroy(tip);tip=null;}}
}
