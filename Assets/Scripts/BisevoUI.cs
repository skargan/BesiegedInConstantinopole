using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BisevoUI : MonoBehaviour
{
    static GameObject view;
    string scene;
    Transform painting;
    TMP_Text description;
    public static bool IsOpen => view != null;
    public static void Show() => Open(BisevoDiscovery.Pending ? "monastery" : "landing");
    static void Open(string scene)
    {
        if(view != null) { view.SetActive(false); Destroy(view); }
        view = new GameObject("Biševo visit",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(BisevoUI));
        var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=270;
        var scaler=view.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var ui=view.GetComponent<BisevoUI>();ui.scene=scene;ui.Draw();
    }
    void Draw()
    {
        var backdrop=CampaignHUD.Panel(transform,"Island backdrop",Vector2.zero,Vector2.one);backdrop.GetComponent<Image>().color=Color.black;
        var art=new GameObject("Island painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));art.transform.SetParent(backdrop.transform,false);painting=art.transform;
        var rect=(RectTransform)painting;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/Bisevo/"+scene);art.GetComponent<RawImage>().texture=texture;art.GetComponent<RawImage>().raycastTarget=false;
        var fit=art.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?16f/9:texture.width/(float)texture.height;
        if(scene=="landing") {
            Hot("The old sanctuary",()=>Open("sanctuary"),.468f,.254f,.475f,.17f,.491f,.139f,.524f,.12f,.56f,.14f,.595f,.195f,.603f,.256f,.552f,.271f);
            Hot("A narrow sea grotto",()=>Open("cave"),.865f,.625f,.872f,.597f,.887f,.575f,.904f,.575f,.921f,.6f,.922f,.632f);
            var monastery=Hot(BisevoDiscovery.CanEnter?"Fresh footprints lead through the broken gate.":"A warship is needed to land a party here.",()=>{if(BisevoDiscovery.Enter())Open("monastery");},.047f,.225f,.05f,.174f,.072f,.155f,.082f,.134f,.111f,.148f,.134f,.124f,.192f,.117f,.192f,.041f,.208f,.037f,.208f,.031f,.22f,.034f,.222f,.046f,.239f,.049f,.24f,.13f,.266f,.157f,.29f,.174f,.312f,.223f,.315f,.262f,.245f,.268f,.17f,.251f,.113f,.25f);
            var caption=monastery.transform.Find("Hover label");if(caption!=null){((RectTransform)caption).sizeDelta=new Vector2(220,80);caption.GetComponentInChildren<TMP_Text>().fontSize=17;}
        }
        if(scene=="sanctuary") {
            var flowers=Hot("Fresh flowers",()=>{
                description.text=Reflection;
                if(BisevoDiscovery.Discover())StartCoroutine(Reward());
            },.740f,.636f,.749f,.615f,.768f,.606f,.781f,.614f,.794f,.602f,.811f,.614f,.829f,.622f,.835f,.642f,.818f,.655f,.789f,.66f,.760f,.65f);
            flowers.SetSpent(BisevoDiscovery.Discovered);
            var reflection=flowers.gameObject.AddComponent<BisevoFlowerReflection>();reflection.ShowReflection=()=>description.text=Reflection;
        }
        if(scene=="monastery" && BisevoDiscovery.Pending) {
            var pirates=new GameObject("Pirates",typeof(RectTransform),typeof(RawImage));pirates.transform.SetParent(painting,false);
            var r=(RectTransform)pirates.transform;r.anchorMin=new Vector2(.12f,.15f);r.anchorMax=new Vector2(.61f,.91f);r.offsetMin=r.offsetMax=Vector2.zero;
            var image=pirates.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Maps/Bisevo/pirates");image.raycastTarget=false;
            pirates.AddComponent<AspectRatioFitter>().aspectMode=AspectRatioFitter.AspectMode.FitInParent;pirates.GetComponent<AspectRatioFitter>().aspectRatio=image.texture.width/(float)image.texture.height;
        }
        bool ransom=scene=="monastery" && BisevoDiscovery.Pending;
        var card=CampaignHUD.Panel(transform,"Island account",new Vector2(.025f,.025f),new Vector2(ransom?.975f:.76f,ransom?.25f:.16f));ImperialTheme.Panel(card,false);
        string title=scene=="landing"?"Biševo":scene=="cave"?"The Blue Cave":scene=="sanctuary"?"The old sanctuary":"Saint Sylvester's monastery";
        CampaignHUD.Label(card.transform,title,new Vector2(.025f,.66f),new Vector2(.97f,.96f),26);
        string text=scene=="landing"?"A quiet shore beneath the island's old ruins.":scene=="cave"?"Blue light rises through the water and moves across the vault of stone.":scene=="sanctuary"?(BisevoDiscovery.Discovered?Reflection:"Weathered gods look out across the sea."):ransom?
            (BisevoDiscovery.State==1?"Startled men reach for their weapons. Your envoy is seized. Their captain demands 5,000 gold for his life.":"The men sheltering here are pirates. They have taken your landing party and demand 5,000 gold, or they will keep your warship."):
            "The courtyard is empty. Only the wind moves through the broken arches.";
        description=CampaignHUD.Label(card.transform,text,new Vector2(.025f,ransom?.37f:.06f),new Vector2(.97f,.65f),21);description.enableAutoSizing=true;description.fontSizeMin=16;description.fontSizeMax=21;
        if(ransom) {
            var pay=CampaignHUD.MakeButton(card.transform,"Pay 5,000 gold",new Vector2(.025f,.045f),new Vector2(.46f,.3f),()=>{if(BisevoDiscovery.Resolve(true))Open("monastery");});
            pay.interactable=DailyLifeEvents.I("iMoney")>=BisevoDiscovery.Ransom;
            CampaignHUD.MakeButton(card.transform,BisevoDiscovery.State==1?"Refuse · lose the envoy":"Refuse · lose one warship",new Vector2(.52f,.045f),new Vector2(.97f,.3f),()=>{if(BisevoDiscovery.Resolve(false))Open("monastery");});
        } else CampaignHUD.MakeButton(transform,scene=="landing"?"Return to the map":"Return to the shore",new Vector2(.79f,.045f),new Vector2(.975f,.115f),()=>Back());
    }
    const string Reflection="Fresh flowers beneath an ancient god. More than a thousand years after Constantine, someone still brings offerings here.";
    LocationHotspot Hot(string label,UnityEngine.Events.UnityAction click,params float[] coords)
    {
        var points=new Vector2[coords.Length/2];for(int i=0;i<points.Length;i++)points[i]=new Vector2(coords[i*2],1-coords[i*2+1]);
        return LocationHotspot.Create(painting,label,points,click);
    }
    IEnumerator Reward()
    {
        var label=CampaignHUD.Label(painting,"+1 secret",new Vector2(.72f,.29f),new Vector2(.9f,.35f),28);label.color=new Color(.2f,.85f,1);label.fontStyle=FontStyles.Bold;label.gameObject.AddComponent<Shadow>().effectColor=new Color(0,.5f,1,.7f);
        Vector2 start=label.rectTransform.anchoredPosition;
        for(float t=0;t<3;t+=Time.unscaledDeltaTime){label.rectTransform.anchoredPosition=start+new Vector2(65,70)*t/3;label.alpha=Mathf.Min(1,(3-t)/.8f);yield return null;}
        Destroy(label.gameObject);
    }
    public static bool Back()
    {
        if(view==null)return false;
        if(BisevoDiscovery.Pending)return true;
        if(view.GetComponent<BisevoUI>().scene!="landing")Open("landing");
        else {view.SetActive(false);Destroy(view);view=null;}
        return true;
    }
}

// The reflection is a hover discovery; claiming the reward remains an explicit click.
public sealed class BisevoFlowerReflection : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler
{
    public System.Action ShowReflection;
    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e) => ShowReflection?.Invoke();
}
