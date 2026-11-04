using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

// Decorative UI only. Normalised painting coordinates; no simulation or random stream access.
public sealed class CouncilChamber : MonoBehaviour
{
    [Serializable] public class Settings {public Vector2 left=new Vector2(.2153f,.7344f),right=new Vector2(.9147f,.508f);public float flameHeight=.028f,glowIntensity=.16f,shadowIntensity=.09f;}
    public Settings settings=new Settings();
    public bool ForceStill,EffectsAvailable=true;
    public float Elapsed {get;private set;}
    public bool IsStill=>ForceStill||platformStill||PlayerPrefs.GetInt("Besieged.ReducedMotion",0)!=0||!SceneParticles.Enabled;
    bool platformStill;float preferenceTimer;RawImage[] flames=new RawImage[2],glows=new RawImage[2],lights=new RawImage[2],shadows=new RawImage[2];
    static Texture2D soft,flame;bool painted;
    public static CouncilChamber Create(Transform parent){
        var panel=CampaignHUD.Panel(parent,"Council chamber",new Vector2(.29f,.025f),new Vector2(.71f,.84f));panel.GetComponent<Image>().color=ImperialTheme.Navy;panel.GetComponent<Image>().raycastTarget=false;panel.AddComponent<RectMask2D>();
        var image=new GameObject("Chamber painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));image.transform.SetParent(panel.transform,false);
        var rect=(RectTransform)image.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;
        var painting=image.GetComponent<RawImage>();painting.raycastTarget=false;
        painting.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/council-chamber");bool dedicated=painting.texture!=null;
        if(!dedicated)painting.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Locations/palace-interior");
        var fit=image.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=painting.texture==null?1:(float)painting.texture.width/painting.texture.height;
        image.AddComponent<RectMask2D>();var component=image.AddComponent<CouncilChamber>();
        var config=Resources.Load<TextAsset>("Content/CouncilChamber");if(config!=null)component.settings=JsonUtility.FromJson<Settings>(config.text);
        component.EffectsAvailable=dedicated;component.Initialise();return component;
    }
    void Initialise(){
        if(soft==null)soft=Texture(false);if(flame==null)flame=Texture(true);
        // A separate canvas keeps animated vertices away from the surrounding menu/quote geometry.
        var field=new GameObject("Candle effects",typeof(RectTransform),typeof(Canvas));field.transform.SetParent(transform,false);var r=(RectTransform)field.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
        for(int i=0;i<2;i++){
            shadows[i]=Layer(field.transform,"Candle shadow "+i,soft);lights[i]=Layer(field.transform,"Table light "+i,soft);glows[i]=Layer(field.transform,"Candle glow "+i,soft);flames[i]=Layer(field.transform,"Anchored flame "+i,flame);
        }
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
    void Render(){if(flames[0]==null)return;for(int i=0;i<2;i++){
        float phase=i*2.73f,t=Elapsed;float signal=.55f*Mathf.Sin(t*1.37f+phase)+.3f*Mathf.Sin(t*2.11f+phase*.7f)+.15f*Mathf.Sin(t*.63f+phase*1.9f);
        float strength=1+signal*.10f;Vector2 wick=i==0?settings.left:settings.right;
        Place(glows[i],wick+new Vector2(0,.01f),new Vector2(.14f,.16f),new Color(1,.58f,.15f,settings.glowIntensity*strength));
        Place(lights[i],new Vector2(wick.x,.32f-i*.045f),new Vector2(.28f,.105f),new Color(1,.65f,.28f,settings.glowIntensity*.7f*strength));
        Place(shadows[i],new Vector2((i==0?.16f:.975f)+signal*.002f,i==0?.66f:.58f),new Vector2(.07f,.22f),new Color(.03f,.02f,.04f,settings.shadowIntensity*strength));
        Place(flames[i],wick,new Vector2(.013f*(1+signal*.06f),settings.flameHeight*strength),Color.white,true);flames[i].rectTransform.localRotation=Quaternion.Euler(0,0,signal*3);
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
