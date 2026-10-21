using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Presentation is isolated here so later skins can change independently of campaign rules.
public static class ImperialTheme
{
    public const string Name = "Midnight Mosaic";
    public static readonly Color Navy = new Color(.025f,.055f,.085f,1);
    public static readonly Color Surface = new Color(.045f,.095f,.14f,1);
    public static readonly Color Gold = new Color(.76f,.59f,.30f,1);
    public static readonly Color Ivory = new Color(.96f,.91f,.79f,1);
    private static Sprite mosaic;
    private static TMP_FontAsset font;
    public static TMP_FontAsset Font {
        get {
            if(font==null) {
                var source=Resources.Load<Font>("Fonts/CrimsonText-Regular");
                if(source==null) return TMP_Settings.defaultFontAsset;
                font=TMP_FontAsset.CreateFontAsset(source);
                font.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>();
                if(TMP_Settings.defaultFontAsset!=null)font.fallbackFontAssetTable.Add(TMP_Settings.defaultFontAsset);
            }
            return font;
        }
    }
    public static Sprite Mosaic {
        get {
            if(mosaic==null) {
                var tex=Resources.Load<Texture2D>("UI/MidnightMosaic/mosaic");
                if(tex!=null) mosaic=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,.5f));
            }
            return mosaic;
        }
    }
    private static readonly System.Collections.Generic.Dictionary<int,Sprite> crops=new System.Collections.Generic.Dictionary<int,Sprite>();
    private static Sprite MaterialFor(Image image) {
        var source=Mosaic;if(source==null)return null;
        var rect=image.rectTransform.rect;float aspect=rect.height>0?rect.width/rect.height:2;
        int key=Mathf.RoundToInt(aspect*20);Sprite result;if(crops.TryGetValue(key,out result))return result;
        var tex=source.texture;float width=tex.width,height=tex.height;
        if(width/height>aspect)width=height*aspect;else height=width/Mathf.Max(.1f,aspect);
        result=Sprite.Create(tex,new Rect((tex.width-width)/2,(tex.height-height)/2,width,height),new Vector2(.5f,.5f));crops[key]=result;return result;
    }
    public static void LegacySurface(GameObject root) {
        foreach(var image in root.GetComponentsInChildren<Image>(true)) {
            if(image.GetComponent<Selectable>()!=null || image.GetComponentInParent<Selectable>()!=null)continue;
            if(image.sprite==null && image.color.a>.25f) {
                image.color=Surface;
                if(image.rectTransform.rect.height>25)Frame(image.gameObject);
            }
        }
    }
    public static void Frame(GameObject obj) {
        if(obj.transform.Find("Imperial goldwork")!=null) return;
        var edge=new GameObject("Imperial goldwork",typeof(RectTransform),typeof(CanvasRenderer),typeof(ImperialFrame),typeof(LayoutElement));
        edge.transform.SetParent(obj.transform,false);
        var rect=edge.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        edge.GetComponent<LayoutElement>().ignoreLayout=true;
        edge.GetComponent<ImperialFrame>().raycastTarget=false;
    }
    public static void Panel(GameObject obj,bool textured=true) {
        var image=obj.GetComponent<Image>();if(image==null)return;
        image.sprite=textured?MaterialFor(image):null;image.type=Image.Type.Simple;
        image.color=textured?new Color(.40f,.49f,.57f,1):Navy;
        Frame(obj);
    }
    public static void Button(Selectable button) {
        var image=button.GetComponent<Image>();
        if(image!=null) {image.sprite=MaterialFor(image);image.type=Image.Type.Simple;image.color=Color.white;button.targetGraphic=image;}
        button.transition=Selectable.Transition.ColorTint;
        var colors=button.colors;
        colors.normalColor=new Color(.75f,.88f,1,1);
        colors.highlightedColor=new Color(1,1,1,1);
        colors.selectedColor=new Color(.94f,.94f,1,1);
        colors.pressedColor=new Color(.45f,.66f,.80f,1);
        var caption=button.GetComponentInChildren<TMP_Text>(true);
        var words=caption!=null?caption.text.ToLowerInvariant():"";
        if(words=="return" || words=="cancel" || words=="resume")colors.normalColor=new Color(.48f,.85f,.78f,1);
        if(words.StartsWith("accept terms") || words.StartsWith("confirm surrender"))colors.normalColor=new Color(1,.42f,.37f,1);
        colors.disabledColor=words=="active"?new Color(1f,.78f,.32f,1):new Color(.25f,.29f,.32f,1);colors.fadeDuration=.12f;button.colors=colors;
        Frame(button.gameObject);
        var outline=button.GetComponent<Outline>();if(outline!=null)outline.enabled=false;
        foreach(var label in button.GetComponentsInChildren<TMP_Text>(true)) { label.font=Font;label.color=Ivory; }
    }
    public static void Icon(Transform parent,int focus,Vector2 min,Vector2 max) {
        var obj=new GameObject("Ivory focus emblem",typeof(RectTransform),typeof(FocusIcon));obj.transform.SetParent(parent,false);
        var rect=obj.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
        var icon=obj.GetComponent<FocusIcon>();icon.Focus=focus;icon.raycastTarget=false;
    }
}

// Native geometry keeps delicate gilt corners crisp at every window size.
[RequireComponent(typeof(CanvasRenderer))]
public class ImperialFrame : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();var r=rectTransform.rect;float w=r.width,h=r.height;if(w<12||h<12)return;
        float inset=3;float x=r.x+inset,y=r.y+inset;w-=inset*2;h-=inset*2;
        Rect(vh,x,y,w,2);Rect(vh,x,y+h-2,w,2);Rect(vh,x,y,2,h);Rect(vh,x+w-2,y,2,h);
        float d=Mathf.Min(16,h*.22f);
        for(int i=0;i<4;i++) {
            float cx=i%2==0?x+4:x+w-4,cy=i<2?y+4:y+h-4;float sx=i%2==0?1:-1,sy=i<2?1:-1;
            Line(vh,new Vector2(cx,cy),new Vector2(cx+sx*d,cy),1.5f);
            Line(vh,new Vector2(cx,cy),new Vector2(cx,cy+sy*d),1.5f);
            var c=new Vector2(cx+sx*5,cy+sy*5);
            Line(vh,c+Vector2.up*3,c+Vector2.right*3,1);Line(vh,c+Vector2.right*3,c-Vector2.up*3,1);
            Line(vh,c-Vector2.up*3,c-Vector2.right*3,1);Line(vh,c-Vector2.right*3,c+Vector2.up*3,1);
        }
    }
    void Rect(VertexHelper vh,float x,float y,float w,float h) { Quad(vh,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)); }
    void Line(VertexHelper vh,Vector2 a,Vector2 b,float width) {var n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;Quad(vh,a-n,b-n,b+n,a+n);}
    void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d) {int i=vh.currentVertCount;foreach(var p in new[]{a,b,c,d})vh.AddVert(p,ImperialTheme.Gold,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
}