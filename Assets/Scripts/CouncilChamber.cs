using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

// Decorative UI only. Normalised painting coordinates; no simulation or random stream access.
public sealed class CouncilChamber : MonoBehaviour
{
    [Serializable] public class Settings {public Vector2 left=new Vector2(.2153f,.7344f),right=new Vector2(.9147f,.508f);public float flameHeight=.028f,glowIntensity=.24f,shadowIntensity=.16f;}
    public Settings settings=new Settings();
    public bool ForceStill,EffectsAvailable=true;
    public float Elapsed {get;private set;}
    public bool IsStill=>ForceStill||platformStill||PlayerPrefs.GetInt("Besieged.ReducedMotion",0)!=0||!SceneParticles.Enabled;
    bool platformStill;float preferenceTimer;RawImage[] flames=new RawImage[2],glows=new RawImage[2],lights=new RawImage[2],shadows=new RawImage[2];
    static Texture2D soft,flame,vignette;RawImage shade,sailingShip,wake;Transform exterior;RawImage[] clouds=new RawImage[6];bool painted;
    public static CouncilChamber Create(Transform parent){
        var panel=CampaignHUD.Panel(parent,"Council chamber",new Vector2(.29f,.025f),new Vector2(.71f,.84f));panel.GetComponent<Image>().color=ImperialTheme.Navy;panel.GetComponent<Image>().raycastTarget=false;panel.AddComponent<RectMask2D>();
        var image=new GameObject("Chamber painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));image.transform.SetParent(panel.transform,false);
        var rect=(RectTransform)image.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;
        var painting=image.GetComponent<RawImage>();painting.raycastTarget=false;
        painting.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/council-chamber-clean");
        if(painting.texture==null)painting.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/council-chamber");bool dedicated=painting.texture!=null;
        if(!dedicated)painting.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Locations/palace-interior");
        var fit=image.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=painting.texture==null?1:(float)painting.texture.width/painting.texture.height;
        image.AddComponent<RectMask2D>();var component=image.AddComponent<CouncilChamber>();image.AddComponent<CouncilMusic>();
        var config=Resources.Load<TextAsset>("Content/CouncilChamber");if(config!=null)component.settings=JsonUtility.FromJson<Settings>(config.text);
        component.EffectsAvailable=dedicated;component.Initialise();return component;
    }
    void Initialise(){
        if(soft==null)soft=Texture(false);if(flame==null)flame=Texture(true);
        // A separate canvas keeps animated vertices away from the surrounding menu/quote geometry.
        var field=new GameObject("Candle effects",typeof(RectTransform),typeof(Canvas));field.transform.SetParent(transform,false);var r=(RectTransform)field.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
        if(vignette==null){
            vignette=new Texture2D(128,128,TextureFormat.RGBA32,false);vignette.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<128;y++)for(int x=0;x<128;x++){
                float u=x/127f,v=y/127f;
                float edge=Mathf.Clamp01(Mathf.Pow(Mathf.Abs(u-.5f)*2,2)+Mathf.Pow(Mathf.Abs(v-.5f)*2,2));
                float window=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.42f,.65f,u))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.52f,.75f,v));
                vignette.SetPixel(x,y,new Color(.008f,.012f,.025f,(.14f+.38f*edge)*(1-.92f*window)));
            }vignette.Apply(false,true);
        }
        shade=Layer(field.transform,"Room corner shade",vignette);
        var sr=shade.rectTransform;sr.anchorMin=Vector2.zero;sr.anchorMax=Vector2.one;sr.offsetMin=sr.offsetMax=Vector2.zero;
        var windows=new GameObject("Exterior window layers",typeof(RectTransform),typeof(CanvasRenderer),typeof(ChamberWindowMask),typeof(Mask));windows.transform.SetParent(field.transform,false);var windowRect=(RectTransform)windows.transform;windowRect.anchorMin=Vector2.zero;windowRect.anchorMax=Vector2.one;windowRect.offsetMin=windowRect.offsetMax=Vector2.zero;windows.GetComponent<ChamberWindowMask>().raycastTarget=false;windows.GetComponent<Mask>().showMaskGraphic=false;exterior=windows.transform;
        // Independent full-frame exterior plate: replacement views keep identical UV alignment.
        var view=Layer(exterior,"Exterior city and water",GetComponent<RawImage>().texture);view.rectTransform.anchorMin=Vector2.zero;view.rectTransform.anchorMax=Vector2.one;view.rectTransform.offsetMin=view.rectTransform.offsetMax=Vector2.zero;
        for(int i=0;i<clouds.Length;i++)clouds[i]=Layer(exterior,"Window haze "+i,Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/council-cloud-wisp")??soft);
        for(int i=0;i<2;i++){
            shadows[i]=Layer(field.transform,"Candle shadow "+i,soft);lights[i]=Layer(field.transform,"Table light "+i,soft);glows[i]=Layer(field.transform,"Candle glow "+i,soft);flames[i]=Layer(field.transform,"Anchored flame "+i,flame);
        }
        var water=new GameObject("Passing ship water mask",typeof(RectTransform),typeof(RectMask2D));water.transform.SetParent(exterior,false);
        var wr=(RectTransform)water.transform;wr.anchorMin=new Vector2(.747f,0);wr.anchorMax=Vector2.one;wr.offsetMin=wr.offsetMax=Vector2.zero;
        wake=Layer(water.transform,"Merchantman wake",soft);
        var boat=new GameObject("Passing merchantman",typeof(RectTransform),typeof(CanvasRenderer),typeof(ChamberShipGraphic));boat.transform.SetParent(water.transform,false);sailingShip=boat.GetComponent<ChamberShipGraphic>();sailingShip.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/council-chamber");sailingShip.raycastTarget=false;
        windows.transform.SetSiblingIndex(0);
        ReadPreference();Render();
    }
    RawImage Layer(Transform parent,string name,Texture texture){var go=new GameObject(name,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(parent,false);var im=go.GetComponent<RawImage>();im.texture=texture;im.raycastTarget=false;return im;}
    static Texture2D Texture(bool fire){
        int w=64,h=fire?128:64;var t=new Texture2D(w,h,TextureFormat.RGBA32,false);t.name=fire?"Chamber flame":"Chamber soft light";t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){
            float u=(x+.5f)/w,v=(y+.5f)/h,a;
            if(fire){float width=.36f*Mathf.Pow(Mathf.Sin(Mathf.PI*v),.7f)*(1-.65f*v);a=Mathf.Clamp01((1-Mathf.Abs(u-.5f)/Mathf.Max(.001f,width))*4)*Mathf.Clamp01(v*16)*Mathf.Clamp01((1-v)*12);t.SetPixel(x,y,new Color(1,Mathf.Lerp(.65f,.96f,1-v),Mathf.Lerp(.12f,.68f,1-v),a));}
            else {float d=Vector2.Distance(new Vector2(u,v),new Vector2(.5f,.5f))*2;a=Mathf.Pow(Mathf.Clamp01(1-d*d),3);t.SetPixel(x,y,new Color(1,1,1,a));}
        }t.Apply(false,true);return t;
    }
    void Place(RawImage im,Vector2 point,Vector2 size,Color tint,bool anchored=false){var r=im.rectTransform;Vector2 area=((RectTransform)transform).rect.size;r.anchorMin=r.anchorMax=point;r.pivot=anchored?new Vector2(.5f,0):new Vector2(.5f,.5f);r.anchoredPosition=Vector2.zero;r.sizeDelta=Vector2.Scale(size,area);im.color=tint;im.enabled=EffectsAvailable;}
    void Render(){if(flames[0]==null)return;
        shade.enabled=EffectsAvailable;
        // Clouds become completely invisible before recycling; no visible wrap or texture swaps.
        for(int i=0;i<clouds.Length;i++){
            bool left=i<3;int n=i%3;float phase=Mathf.Repeat(Elapsed/(42+i*3)+n*.333f,1);float fade=Mathf.Pow(Mathf.Sin(Mathf.PI*phase),2);
            Place(clouds[i],new Vector2((left?.456f:.725f)+phase*.255f,.855f+n*.023f),new Vector2(.145f,.045f),new Color(1,1,1,.28f*fade));
        }
        if(sailingShip!=null){float phase=Mathf.Repeat(Elapsed,64);float u=Mathf.Clamp01(phase/44);Vector2 point=ShipPoint(u);float scale=Mathf.Lerp(1,.57f,u);bool visible=phase<44;
            Place(sailingShip,new Vector2((point.x-.747f)/.253f,point.y),new Vector2(.067f,.051f)*scale,Color.white,true);sailingShip.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Elapsed*1.7f)*.5f);sailingShip.enabled=EffectsAvailable&&sailingShip.texture!=null&&visible;
            Place(wake,new Vector2((point.x-.747f-.019f*scale)/.253f,point.y+.001f),new Vector2(.046f,.003f)*scale,new Color(.96f,.75f,.46f,.12f));wake.enabled=sailingShip.enabled;
        }
        for(int i=0;i<2;i++){
        float phase=i*2.73f,t=Elapsed;float signal=.55f*Mathf.Sin(t*2.37f+phase)+.3f*Mathf.Sin(t*4.11f+phase*.7f)+.15f*Mathf.Sin(t*.63f+phase*1.9f);
        float gust=Mathf.Exp(-Mathf.Pow((Mathf.Repeat(t+i*.32f,27f)-18f)/.8f,2));
        float strength=1+signal*.10f-gust*.28f;Vector2 wick=i==0?settings.left:settings.right;
        Place(glows[i],wick+new Vector2(i==0?.015f:-.015f,.015f),i==0?new Vector2(.34f,.40f):new Vector2(.27f,.30f),new Color(1,.62f,.23f,settings.glowIntensity*strength));
        Place(lights[i],i==0?new Vector2(.25f,.47f):new Vector2(.79f,.36f),i==0?new Vector2(.47f,.34f):new Vector2(.48f,.32f),new Color(1,.68f,.31f,settings.glowIntensity*.36f*strength));
        Place(shadows[i],i==0?new Vector2(.045f,.64f):new Vector2(.43f,.10f),i==0?new Vector2(.24f,.87f):new Vector2(.98f,.24f),new Color(.015f,.012f,.025f,settings.shadowIntensity*(1+gust*.12f)));
        Place(flames[i],wick,new Vector2(.013f*(1+signal*.06f+gust*.18f),settings.flameHeight*strength),Color.white,true);flames[i].rectTransform.localRotation=Quaternion.Euler(0,0,signal*3+gust*(i==0?-15:-12));
    }painted=true;}
    // Art-profile hook: callers can replace the exterior without replacing the room or moving UI.
    public void SetExterior(Texture2D plate){if(exterior==null)return;var view=exterior.Find("Exterior city and water").GetComponent<RawImage>();view.texture=plate??GetComponent<RawImage>().texture;}
    public static Vector2 ShipPoint(float u){Vector2 a=new Vector2(.70f,.602f),b=new Vector2(.87f,.608f),c=new Vector2(1.055f,.637f);return (1-u)*(1-u)*a+2*(1-u)*u*b+u*u*c;}
    void Update(){preferenceTimer+=Time.unscaledDeltaTime;if(preferenceTimer>2){preferenceTimer=0;ReadPreference();}if(!IsStill&&EffectsAvailable){Elapsed+=Mathf.Min(Time.unscaledDeltaTime,.05f);Render();}else if(!painted)Render();}
    void OnEnable(){ReadPreference();painted=false;}
    void OnRectTransformDimensionsChange(){painted=false;if(flames[0]!=null)Render();}
    void ReadPreference(){
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        try{bool enabled;if(SystemParametersInfo(0x1042,0,out enabled,0))platformStill=!enabled;}catch{platformStill=false;}
#endif
    }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll",SetLastError=true)]static extern bool SystemParametersInfo(uint action,uint param,[MarshalAs(UnmanagedType.Bool)]out bool value,uint flags);
#endif
}
