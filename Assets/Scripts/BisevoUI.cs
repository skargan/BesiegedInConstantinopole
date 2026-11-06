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
            Hot("The old sanctuary",()=>Open("sanctuary"),0.447368f,0.253985f,0.463517f,0.235919f,0.472488f,0.229543f,0.472488f,0.165781f,0.477273f,0.162593f,0.479067f,0.226355f,0.494617f,0.227418f,0.498804f,0.160468f,0.497010f,0.156217f,0.496411f,0.134963f,0.501794f,0.131775f,0.501196f,0.126461f,0.510167f,0.124336f,0.537081f,0.136026f,0.539474f,0.148778f,0.535287f,0.151966f,0.534689f,0.231668f,0.545455f,0.238045f,0.547249f,0.212540f,0.553230f,0.210414f,0.553230f,0.241233f,0.560407f,0.244421f,0.561603f,0.170032f,0.559211f,0.164718f,0.561005f,0.157279f,0.569378f,0.157279f,0.572368f,0.162593f,0.569976f,0.167906f,0.571172f,0.243358f,0.584928f,0.251860f,0.586124f,0.192349f,0.590909f,0.191286f,0.592105f,0.257173f,0.604067f,0.265675f,0.605263f,0.279490f,0.581340f,0.276302f,0.556220f,0.266738f,0.529306f,0.262487f,0.502392f,0.251860f,0.480263f,0.251860f,0.465311f,0.259299f);
            Hot("A narrow sea grotto",()=>Open("cave"),0.873804f,0.621679f,0.875000f,0.611052f,0.879187f,0.600425f,0.883373f,0.589798f,0.888158f,0.583422f,0.894139f,0.580234f,0.899522f,0.584485f,0.907297f,0.586610f,0.911483f,0.598300f,0.913278f,0.611052f,0.913876f,0.622742f,0.899522f,0.622742f,0.888158f,0.624867f);
            var monastery=Hot(BisevoDiscovery.CanEnter?"Fresh footprints lead through the broken gate.":"A warship is needed to land a party here.",()=>{if(BisevoDiscovery.Enter())Open("monastery");},0.031100f,0.228480f,0.043062f,0.227418f,0.045455f,0.193411f,0.052033f,0.171095f,0.057416f,0.166844f,0.062799f,0.150903f,0.072368f,0.136026f,0.087321f,0.128587f,0.094498f,0.119022f,0.099880f,0.113709f,0.110048f,0.115834f,0.117225f,0.123273f,0.126794f,0.125399f,0.136962f,0.140276f,0.150718f,0.143464f,0.159091f,0.141339f,0.167464f,0.147715f,0.178230f,0.158342f,0.191986f,0.158342f,0.193182f,0.114772f,0.194976f,0.110521f,0.194976f,0.093518f,0.193780f,0.089267f,0.194378f,0.074389f,0.195574f,0.070138f,0.194976f,0.048884f,0.197368f,0.041445f,0.203349f,0.039320f,0.205742f,0.044633f,0.209330f,0.042508f,0.211722f,0.047821f,0.212919f,0.039320f,0.220096f,0.035069f,0.224880f,0.038257f,0.225478f,0.048884f,0.229067f,0.049947f,0.230263f,0.046759f,0.235646f,0.052072f,0.236842f,0.057386f,0.239833f,0.058448f,0.241627f,0.068013f,0.241627f,0.184910f,0.249402f,0.189160f,0.253589f,0.201913f,0.261364f,0.202976f,0.268541f,0.211477f,0.280502f,0.209352f,0.283493f,0.217853f,0.291866f,0.216791f,0.299641f,0.227418f,0.303230f,0.246546f,0.312201f,0.256111f,0.313397f,0.267800f,0.299641f,0.267800f,0.293062f,0.260361f,0.285287f,0.263549f,0.281100f,0.256111f,0.274522f,0.259299f,0.263756f,0.251860f,0.254187f,0.255048f,0.248206f,0.245484f,0.237440f,0.246546f,0.227871f,0.240170f,0.214713f,0.238045f,0.205742f,0.231668f,0.190789f,0.233794f,0.180622f,0.229543f,0.166268f,0.233794f,0.153110f,0.240170f,0.138158f,0.239107f,0.126196f,0.231668f,0.112440f,0.233794f,0.096890f,0.226355f,0.080742f,0.228480f,0.065789f,0.223167f,0.050239f,0.229543f);
            var caption=monastery.transform.Find("Hover label");if(caption!=null){((RectTransform)caption).sizeDelta=new Vector2(220,80);caption.GetComponentInChildren<TMP_Text>().fontSize=17;}
        }
        if(scene=="sanctuary") {
            var flowers=Hot("Fresh flowers",()=>{
                description.text=Reflection;
                if(BisevoDiscovery.Discover())StartCoroutine(Reward());
            },0.742225f,0.652497f,0.758373f,0.646121f,0.765550f,0.635494f,0.770933f,0.634431f,0.771531f,0.626993f,0.775718f,0.626993f,0.779306f,0.619554f,0.782895f,0.625930f,0.786483f,0.624867f,0.788278f,0.634431f,0.791866f,0.636557f,0.796053f,0.624867f,0.798445f,0.617428f,0.797249f,0.609989f,0.801435f,0.606801f,0.805024f,0.609989f,0.806220f,0.620616f,0.811603f,0.626993f,0.814593f,0.621679f,0.818182f,0.623804f,0.819976f,0.635494f,0.824163f,0.630181f,0.825957f,0.619554f,0.830144f,0.613177f,0.833732f,0.620616f,0.831340f,0.626993f,0.828349f,0.631243f,0.826555f,0.643996f,0.834330f,0.649309f,0.832536f,0.655685f,0.822368f,0.652497f,0.818182f,0.658874f,0.812201f,0.655685f,0.805622f,0.659936f,0.799641f,0.655685f,0.793660f,0.659936f,0.787081f,0.652497f,0.776316f,0.656748f,0.768541f,0.652497f,0.754187f,0.655685f);
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
