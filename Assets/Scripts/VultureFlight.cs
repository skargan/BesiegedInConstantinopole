using UnityEngine;
using UnityEngine.UI;

// A decorative, deterministic orbit. No sign flips, simulation clock or random calls.
public sealed class VultureFlight:MonoBehaviour
{
    public const float CircuitSeconds=32;
    public struct Pose {public Vector2 anchor;public float bank,depth,blend;public int first,second;}
    static readonly Rect[] Views={new Rect(0,.5f,.5f,.5f),new Rect(1,.5f,-.5f,.5f),new Rect(0,0,.5f,.5f),new Rect(.5f,0,.5f,.5f)};
    RectTransform bird;RawImage first,second;float opened,elapsed;
    public static VultureFlight Attach(RectTransform rect)
    {
        var flight=rect.gameObject.AddComponent<VultureFlight>();flight.bird=rect;flight.first=rect.GetComponent<RawImage>();
        var texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/nameless-vulture-views");
        flight.first.texture=texture;flight.first.raycastTarget=false;
        var other=new GameObject("Turning pose",typeof(RectTransform),typeof(RawImage));other.transform.SetParent(rect,false);var at=(RectTransform)other.transform;at.anchorMin=Vector2.zero;at.anchorMax=Vector2.one;at.offsetMin=at.offsetMax=Vector2.zero;
        flight.second=other.GetComponent<RawImage>();flight.second.texture=texture;flight.second.raycastTarget=false;
        flight.opened=Time.unscaledTime;flight.Present(0);return flight;
    }
    public static Pose Sample(float seconds)
    {
        float phase=Mathf.Repeat(seconds/CircuitSeconds,1),angle=phase*Mathf.PI*2,step=phase*4;
        int pose=Mathf.FloorToInt(step);float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.60f,1,step-pose));
        return new Pose{anchor=new Vector2(.26f+.09f*Mathf.Cos(angle),.94f+.025f*Mathf.Sin(angle)),bank=7*Mathf.Sin(angle+.35f),depth=.89f-.11f*Mathf.Sin(angle),first=pose,second=(pose+1)%4,blend=blend};
    }
    public void Present(float seconds)
    {
        var pose=Sample(seconds);bird.anchorMin=bird.anchorMax=pose.anchor;bird.anchoredPosition=Vector2.zero;
        float width=((RectTransform)bird.parent).rect.width*.036f*pose.depth;
        bird.sizeDelta=Vector2.one*width;bird.localScale=Vector3.one;bird.localRotation=Quaternion.Euler(0,0,pose.bank);
        first.uvRect=Views[pose.first];second.uvRect=Views[pose.second];
        first.color=new Color(1,1,1,1-pose.blend);second.color=new Color(1,1,1,pose.blend);
    }
    void Update(){if(PlayerPrefs.GetInt("Besieged.ReducedMotion",0)==0)elapsed=Time.unscaledTime-opened;else opened=Time.unscaledTime-elapsed;Present(elapsed);}
}
