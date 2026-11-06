using System;
using UnityEngine;
using UnityEngine.UI;

// Playback of the user's GIF frames; no campaign-clock or random-number calls.
public sealed class EagleFlight:MonoBehaviour
{
    [Serializable]public sealed class Frame {public int x,y,width,height;public float seconds;}
    [Serializable]public sealed class Clip {public int atlasWidth,atlasHeight;public Frame[] frames;}
    public struct Pose {public Vector2 anchor;public float depth;public int frame;}
    static Clip clip;static float duration;
    public static Clip Animation {get{if(clip==null){var data=Resources.Load<TextAsset>("UI/Maps/Landmarks/nameless-eagle-frames");if(data!=null){clip=JsonUtility.FromJson<Clip>(data.text);duration=0;foreach(var f in clip.frames)duration+=f.seconds;}}return clip;}}
    public static float CircuitSeconds {get{var data=Animation;return duration;}}
    public static int FrameAt(float seconds){var data=Animation;if(data==null||duration<=0)return 0;float time=Mathf.Repeat(seconds,duration);for(int i=0;i<data.frames.Length;i++){if(time<data.frames[i].seconds)return i;time-=data.frames[i].seconds;}return 0;}
    public static Pose Sample(float seconds){float angle=Mathf.Repeat(seconds/CircuitSeconds,1)*Mathf.PI*2;return new Pose{anchor=new Vector2(.26f+.075f*Mathf.Cos(angle),.94f+.021f*Mathf.Sin(angle)),depth=.88f-.12f*Mathf.Sin(angle),frame=FrameAt(seconds)};}
    RectTransform bird;RawImage image;float elapsed;
    public static EagleFlight Attach(RectTransform rect){var flight=rect.gameObject.AddComponent<EagleFlight>();flight.bird=rect;flight.image=rect.GetComponent<RawImage>();flight.image.texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/nameless-eagle-frames");flight.image.raycastTarget=false;flight.Present(0);return flight;}
    public void Present(float seconds){var data=Animation;if(data==null||data.frames.Length==0){image.enabled=false;return;}var p=Sample(seconds);var f=data.frames[p.frame];bird.anchorMin=bird.anchorMax=p.anchor;bird.anchoredPosition=Vector2.zero;float scale=((RectTransform)bird.parent).rect.width*.0011f*p.depth;bird.sizeDelta=new Vector2(f.width,f.height)*scale;bird.localScale=Vector3.one;bird.localRotation=Quaternion.identity;image.uvRect=new Rect(f.x/(float)data.atlasWidth,1-(f.y+f.height)/(float)data.atlasHeight,f.width/(float)data.atlasWidth,f.height/(float)data.atlasHeight);image.color=Color.white;}
    void Update(){if(PlayerPrefs.GetInt("Besieged.ReducedMotion",0)==0)elapsed+=Time.unscaledDeltaTime;Present(elapsed);}
}
