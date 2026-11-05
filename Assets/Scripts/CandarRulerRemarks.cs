using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Approved prose is copied unchanged into Resources/Content/CandarRemarks.json.
public sealed class CandarRulerRemarks:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler,ISelectHandler,IDeselectHandler
{
    [Serializable] public sealed class Catalogue { public string speaker; public string[] lines; }
    [Serializable] public sealed class HoverShape { public Vector2[] points; }
    static Catalogue catalogue;
    static readonly System.Random random=new System.Random();
    static int last=-1;
    static float ready;
    TMP_Text words;LocationHotspot hotspot;float until;
    public static Catalogue Remarks {
        get {
            if(catalogue==null){var text=Resources.Load<TextAsset>("Content/CandarRemarks");catalogue=text==null?new Catalogue{speaker="İsmail Bey",lines=new string[0]}:JsonUtility.FromJson<Catalogue>(text.text);}
            return catalogue;
        }
    }
    public void Bind(TMP_Text label,LocationHotspot outline){words=label;hotspot=outline;}
    public void Speak()
    {
        if(words==null||Time.unscaledTime<ready||Remarks.lines==null||Remarks.lines.Length==0)return;
        int n;do{n=random.Next(Remarks.lines.Length);}while(Remarks.lines.Length>1&&n==last);
        last=n;words.text=Remarks.speaker+": “"+Remarks.lines[n]+"”";
        until=Time.unscaledTime+10f;ready=until;
    }
    public void Glow(bool on){if(hotspot!=null)hotspot.SetHighlighted(on);}
    public void OnPointerEnter(PointerEventData e){Glow(true);Speak();}
    public void OnPointerExit(PointerEventData e){Glow(false);}
    public void OnPointerClick(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)Speak();}
    public void OnSelect(BaseEventData e){Glow(true);Speak();}
    public void OnDeselect(BaseEventData e){Glow(false);}
    void Update(){if(words!=null&&until>0&&Time.unscaledTime>=until){words.text="";until=0;}}
}

// A conventional focusable button shares the painted character's conversation.
public sealed class CandarRemarkFocus:MonoBehaviour,ISelectHandler,IDeselectHandler
{
    public CandarRulerRemarks source;
    public void OnSelect(BaseEventData e){if(source!=null)source.OnSelect(e);}
    public void OnDeselect(BaseEventData e){if(source!=null)source.OnDeselect(e);}
}
