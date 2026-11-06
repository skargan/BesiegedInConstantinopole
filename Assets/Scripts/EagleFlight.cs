using System;
using UnityEngine;
using UnityEngine.UI;

// The user's GIF poses, retimed and blended for display. No campaign-clock or random calls.
public sealed class EagleFlight:MonoBehaviour
{
    [Serializable]public sealed class Frame {public int x,y,width,height;public float seconds;}
    [Serializable]public sealed class Clip {public int atlasWidth,atlasHeight;public Frame[] frames;}
    public const float CircuitSeconds=18f,PlaybackRate=6.72f;
    public struct Pose {public Vector2 anchor;public float depth,blend;public int frame,nextFrame;}
    static Clip clip;static float duration;
    public static Clip Animation {get{if(clip==null){var data=Resources.Load<TextAsset>("UI/Maps/Landmarks/nameless-eagle-frames");if(data!=null){clip=JsonUtility.FromJson<Clip>(data.text);duration=0;foreach(var f in clip.frames)duration+=f.seconds;}}return clip;}}
    public static float SourceSeconds {get{var data=Animation;return duration;}}
    public static float AnimationSeconds=>SourceSeconds/PlaybackRate;
    static int FrameAt(float seconds,out float blend){var data=Animation;blend=0;if(data==null||duration<=0)return 0;float time=Mathf.Repeat(seconds*PlaybackRate,duration);for(int i=0;i<data.frames.Length;i++){if(time<data.frames[i].seconds){blend=Mathf.SmoothStep(0,1,time/data.frames[i].seconds);return i;}time-=data.frames[i].seconds;}return 0;}
    public static int FrameAt(float seconds)=>FrameAt(seconds,out _);
    public static Pose Sample(float seconds){float angle=Mathf.Repeat(seconds/CircuitSeconds,1)*Mathf.PI*2;int frame=FrameAt(seconds,out var blend);return new Pose{anchor=new Vector2(.79f+.115f*Mathf.Cos(angle),.83f+.035f*Mathf.Sin(angle)),depth=.88f-.12f*Mathf.Sin(angle),frame=frame,nextFrame=Animation==null?0:(frame+1)%Animation.frames.Length,blend=blend};}
    RectTransform bird;RawImage image,nextImage;float elapsed;
    public static EagleFlight Attach(RectTransform rect){var flight=rect.gameObject.AddComponent<EagleFlight>();flight.bird=rect;flight.image=rect.GetComponent<RawImage>();flight.image.texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/nameless-eagle-frames");flight.image.raycastTarget=false;
        var next=new GameObject("Blended eagle pose",typeof(RectTransform),typeof(RawImage));next.transform.SetParent(rect,false);flight.nextImage=next.GetComponent<RawImage>();flight.nextImage.texture=flight.image.texture;flight.nextImage.raycastTarget=false;var r=next.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=Vector2.zero;
        flight.Present(0);return flight;}
    public void Present(float seconds){var data=Animation;if(data==null||data.frames.Length==0){image.enabled=nextImage.enabled=false;return;}var p=Sample(seconds);var f=data.frames[p.frame];var next=data.frames[p.nextFrame];bird.anchorMin=bird.anchorMax=p.anchor;bird.anchoredPosition=Vector2.zero;float scale=((RectTransform)bird.parent).rect.width*.00072f*p.depth;bird.sizeDelta=new Vector2(f.width,f.height)*scale;nextImage.rectTransform.sizeDelta=new Vector2(next.width,next.height)*scale;bird.localScale=Vector3.one;bird.localRotation=Quaternion.identity;image.uvRect=UV(f,data);nextImage.uvRect=UV(next,data);
        // Ordinary reciprocal alpha dips to75% at mid-blend. Overlap-biased weights
        // keep the tiny opaque body above97% while adjacent wing edges transition.
        image.color=new Color(1,1,1,Mathf.Pow(1-p.blend,.25f));nextImage.color=new Color(1,1,1,Mathf.Pow(p.blend,.25f));}
    static Rect UV(Frame f,Clip data)=>new Rect(f.x/(float)data.atlasWidth,1-(f.y+f.height)/(float)data.atlasHeight,f.width/(float)data.atlasWidth,f.height/(float)data.atlasHeight);
    void Update(){if(PlayerPrefs.GetInt("Besieged.ReducedMotion",0)==0)elapsed+=Mathf.Min(Time.unscaledDeltaTime,.1f);Present(elapsed);}
}
