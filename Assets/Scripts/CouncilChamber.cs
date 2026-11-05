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
    static Texture2D soft,flame,vignette;RawImage shade,sailingShip;RawImage[] clouds=new RawImage[6];bool painted;
    public static CouncilChamber Create(Transform parent){
        var panel=CampaignHUD.Panel(parent,"Council chamber",new Vector2(.29f,.025f),new Vector2(.71f,.84f));panel.GetComponent<Image>().color=ImperialTheme.Navy;panel.GetComponent<Image>().raycastTarget=false;panel.AddComponent<RectMask2D>();
        var image=new GameObject("Chamber painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));image.transform.SetParent(panel.transform,false);
        var rect=(RectTransform)image.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;
        var painting=image.GetComponent<RawImage>();painting.raycastTarget=false;
        painting.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/council-chamber");bool dedicated=painting.texture!=null;
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
        for(int i=0;i<clouds.Length;i++)clouds[i]=Layer(field.transform,"Window haze "+i,soft);
        for(int i=0;i<2;i++){
            shadows[i]=Layer(field.transform,"Candle shadow "+i,soft);lights[i]=Layer(field.transform,"Table light "+i,soft);glows[i]=Layer(field.transform,"Candle glow "+i,soft);flames[i]=Layer(field.transform,"Anchored flame "+i,flame);
        }
        var water=new GameObject("Passing ship water mask",typeof(RectTransform),typeof(RectMask2D));water.transform.SetParent(field.transform,false);
        var wr=(RectTransform)water.transform;wr.anchorMin=new Vector2(.762f,.593f);wr.anchorMax=new Vector2(.974f,.705f);wr.offsetMin=wr.offsetMax=Vector2.zero;
        sailingShip=Layer(water.transform,"Passing merchantman",Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/council-passing-ship"));
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
        // Small soft wisps stay within the painted sky, clear of arches and skyline.
        for(int i=0;i<clouds.Length;i++){
            bool left=i<3;int n=i%3;float drift=Mathf.Repeat(Elapsed*.011f+n*.058f,.17f);
            Place(clouds[i],new Vector2((left?.535f:.777f)+drift,(left?.846f:.81f)+n*.018f),new Vector2(.095f,.035f),new Color(.76f,.68f,.78f,.21f));
        }
        if(sailingShip!=null){var sr=sailingShip.rectTransform;sr.anchorMin=sr.anchorMax=new Vector2(-.18f+Mathf.Repeat(Elapsed/38f,1)*1.4f,.33f);sr.pivot=new Vector2(.5f,0);sr.anchoredPosition=Vector2.zero;var area=((RectTransform)transform).rect.size;sr.sizeDelta=new Vector2(area.x*.055f,area.y*.042f);sailingShip.enabled=EffectsAvailable&&sailingShip.texture!=null;}
        for(int i=0;i<2;i++){
        float phase=i*2.73f,t=Elapsed;float signal=.55f*Mathf.Sin(t*2.37f+phase)+.3f*Mathf.Sin(t*4.11f+phase*.7f)+.15f*Mathf.Sin(t*.63f+phase*1.9f);
        float gust=Mathf.Exp(-Mathf.Pow((Mathf.Repeat(t+i*.32f,27f)-18f)/.8f,2));
        float strength=1+signal*.10f-gust*.28f;Vector2 wick=i==0?settings.left:settings.right;
        Place(glows[i],wick+new Vector2(i==0?.015f:-.015f,.015f),i==0?new Vector2(.34f,.40f):new Vector2(.27f,.30f),new Color(1,.62f,.23f,settings.glowIntensity*strength));
        Place(lights[i],i==0?new Vector2(.25f,.47f):new Vector2(.79f,.36f),i==0?new Vector2(.47f,.34f):new Vector2(.48f,.32f),new Color(1,.68f,.31f,settings.glowIntensity*.36f*strength));
        Place(shadows[i],i==0?new Vector2(.045f,.64f):new Vector2(.43f,.10f),i==0?new Vector2(.24f,.87f):new Vector2(.98f,.24f),new Color(.015f,.012f,.025f,settings.shadowIntensity*(1+gust*.12f)));
        Place(flames[i],wick,new Vector2(.013f*(1+signal*.06f+gust*.18f),settings.flameHeight*strength),Color.white,true);flames[i].rectTransform.localRotation=Quaternion.Euler(0,0,signal*3+gust*(i==0?-15:-12));
    }painted=true;}
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
