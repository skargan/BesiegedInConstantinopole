using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One existing assigned visitor. This view does not choose visitors or alter campaign state.
public sealed class PalaceCourtFigure : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public const float FootX=.285f,FootY=.07f,Height=.61f,MaximumWidth=.16f;
    RectTransform painting,figure,shadow,plaque;
    Image picture;Outline outline;TMP_Text caption;
    Vector2 lastSize;float aspect;string personName;int personId=-1;Vector4 placement;
    public int Location {get;private set;}
    // Coordinates are local to each fitted painting: x, feet y, height, maximum width.
    public static Vector4 Placement(int location)=>location==1?new Vector4(.29f,.09f,.59f,.16f):location==2?new Vector4(.43f,.075f,.57f,.16f):new Vector4(FootX,FootY,Height,MaximumWidth);

    public static bool TryCreate(Transform painting,int person,UnityAction speak)=>TryCreate(painting,person,0,speak);
    public static bool TryCreate(Transform painting,int person,int location,UnityAction speak)
    {
        var art=PalaceFigureArt.Acquire(person);if(art==null)return false;
        var ground=new GameObject("Palace contact shadow "+person,typeof(RectTransform),typeof(PalaceFigureShadow));ground.transform.SetParent(painting,false);
        ground.GetComponent<PalaceFigureShadow>().raycastTarget=false;
        var go=new GameObject("Palace court figure "+person,typeof(RectTransform),typeof(Image),typeof(Button),typeof(Outline),typeof(PalaceCourtFigure));go.transform.SetParent(painting,false);
        var view=go.GetComponent<PalaceCourtFigure>();view.personId=person;view.painting=(RectTransform)painting;view.figure=go.GetComponent<RectTransform>();view.shadow=ground.GetComponent<RectTransform>();
        view.Location=Mathf.Clamp(location,0,2);view.placement=Placement(view.Location);
        view.personName=GardenConversations.Name(person);view.aspect=art.rect.width/art.rect.height;
        view.picture=go.GetComponent<Image>();view.picture.sprite=art;view.picture.preserveAspect=true;view.picture.color=Color.white;
        // Readable source textures permit silhouette hit-testing. Tight imports bound the fallback area.
        if(art.texture.isReadable)view.picture.alphaHitTestMinimumThreshold=.12f;
        view.outline=go.GetComponent<Outline>();view.outline.effectColor=new Color(1f,.80f,.36f,.65f);view.outline.effectDistance=new Vector2(1.25f,1.25f);view.outline.useGraphicAlpha=true;view.outline.enabled=false;
        var button=go.GetComponent<Button>();button.targetGraphic=view.picture;button.onClick.AddListener(speak);button.transition=Selectable.Transition.ColorTint;
        var colours=button.colors;colours.normalColor=Color.white;colours.highlightedColor=new Color(1,1,.94f);colours.selectedColor=colours.highlightedColor;colours.pressedColor=new Color(.8f,.8f,.73f);colours.fadeDuration=.1f;button.colors=colours;go.AddComponent<ButtonAudioBound>();button.onClick.AddListener(ButtonAudio.Play);

        // The nameplate is a sibling so the figure's alpha filter cannot reject its clicks.
        var plate=CampaignHUD.MakeButton(painting,view.personName,Vector2.zero,Vector2.zero,speak);plate.gameObject.name="Palace figure name "+person;
        view.plaque=plate.GetComponent<RectTransform>();view.caption=plate.GetComponentInChildren<TMP_Text>(true);
        view.caption.enableAutoSizing=true;view.caption.fontSizeMin=14;view.caption.fontSizeMax=19;
        view.caption.margin=new Vector4(10,0,10,0);
        view.caption.enableWordWrapping=false;view.caption.overflowMode=TextOverflowModes.Ellipsis;
        view.Arrange();return true;
    }
    void LateUpdate(){if(painting!=null && painting.rect.size!=lastSize)Arrange();}
    void Arrange()
    {
        if(painting==null)return;lastSize=painting.rect.size;
        float height=Mathf.Min(lastSize.y*placement.z,lastSize.x*placement.w/Mathf.Max(.05f,aspect));
        float width=height*aspect;
        figure.anchorMin=figure.anchorMax=new Vector2(placement.x,placement.y);figure.pivot=new Vector2(.5f,0);figure.anchoredPosition=Vector2.zero;figure.sizeDelta=new Vector2(width,height);
        shadow.anchorMin=shadow.anchorMax=new Vector2(placement.x,placement.y);shadow.pivot=new Vector2(.5f,.5f);shadow.anchoredPosition=new Vector2(0,-height*.003f);shadow.sizeDelta=new Vector2(width*.72f,height*.038f);
        plaque.anchorMin=plaque.anchorMax=new Vector2(placement.x,Mathf.Max(.008f,placement.y-.062f));plaque.pivot=new Vector2(.5f,0);plaque.anchoredPosition=Vector2.zero;plaque.sizeDelta=new Vector2(Mathf.Min(300,lastSize.x*.35f),Mathf.Clamp(lastSize.y*.042f,22,30));
    }
    void Highlight(bool active){if(outline!=null)outline.enabled=active;if(caption!=null)caption.text=active?personName+" · Speak":personName;}
    public void OnPointerEnter(PointerEventData e)=>Highlight(true);
    public void OnPointerExit(PointerEventData e)=>Highlight(false);
    public void OnSelect(BaseEventData e)=>Highlight(true);
    public void OnDeselect(BaseEventData e)=>Highlight(false);
    void OnDisable()=>Highlight(false);
    void OnDestroy(){if(personId>=0)PalaceFigureArt.Release(personId);}
}

// A small soft ellipse joins the cutout's feet to the painted floor without an extra texture.
public sealed class PalaceFigureShadow : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var rect=rectTransform.rect;var centre=rect.center;
        vh.AddVert(centre,new Color(.015f,.012f,.009f,.42f),Vector2.zero);
        const int steps=32;
        for(int i=0;i<=steps;i++){float a=i*Mathf.PI*2/steps;vh.AddVert(centre+new Vector2(Mathf.Cos(a)*rect.width*.5f,Mathf.Sin(a)*rect.height*.5f),Color.clear,Vector2.zero);}
        for(int i=1;i<=steps;i++)vh.AddTriangle(0,i,i+1);
    }
}
