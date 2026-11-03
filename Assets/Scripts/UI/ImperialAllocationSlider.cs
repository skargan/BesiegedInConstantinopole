using UnityEngine;
using UnityEngine.UI;

// Native goldwork stays crisp without stretching a texture or the drag handle.
public static class ImperialAllocationSlider
{
    public static Slider Create(Transform parent, int value, int maximum, UnityEngine.Events.UnityAction<float> changed)
    {
        var root=CampaignHUD.Panel(parent,"Research monk allocation",new Vector2(.08f,.385f),new Vector2(.92f,.46f));
        root.GetComponent<Image>().color=Color.clear;
        var slider=root.AddComponent<Slider>();
        slider.wholeNumbers=true;slider.minValue=0;slider.maxValue=maximum;
        var rail=Graphic(root.transform,"Engraved allocation rail",false);
        var area=new GameObject("Handle area",typeof(RectTransform));area.transform.SetParent(root.transform,false);
        var areaRect=(RectTransform)area.transform;areaRect.anchorMin=new Vector2(0,.5f);areaRect.anchorMax=new Vector2(1,.5f);
        areaRect.offsetMin=new Vector2(18,0);areaRect.offsetMax=new Vector2(-18,0);
        var handle=Graphic(area.transform,"Sapphire allocation seal",true);
        handle.rectTransform.anchorMin=handle.rectTransform.anchorMax=new Vector2(0,.5f);
        handle.rectTransform.sizeDelta=new Vector2(36,36);
        slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
        var colors=slider.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.18f,1.18f,1.12f);
        colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(.78f,.85f,.95f);
        colors.disabledColor=new Color(.42f,.46f,.5f);colors.fadeDuration=.12f;slider.colors=colors;
        rail.Slider=slider;slider.onValueChanged.AddListener(_=>rail.SetVerticesDirty());
        slider.SetValueWithoutNotify(value);slider.onValueChanged.AddListener(changed);slider.interactable=maximum>0;
        return slider;
    }
    static ImperialAllocationGoldwork Graphic(Transform parent,string name,bool seal)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(ImperialAllocationGoldwork));obj.transform.SetParent(parent,false);
        var graphic=obj.GetComponent<ImperialAllocationGoldwork>();graphic.Seal=seal;graphic.raycastTarget=false;
        graphic.rectTransform.anchorMin=Vector2.zero;graphic.rectTransform.anchorMax=Vector2.one;
        graphic.rectTransform.offsetMin=graphic.rectTransform.offsetMax=Vector2.zero;
        return graphic;
    }
}

public sealed class ImperialAllocationGoldwork : MaskableGraphic
{
    public bool Seal;
    public Slider Slider;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;float y=r.center.y;
        var gold=ImperialTheme.Gold;var light=new Color(.96f,.81f,.48f);var dark=new Color(.24f,.17f,.08f);
        if(Seal){
            var p=r.center;
            Diamond(vh,p+Vector2.down*2,15,new Color(0,0,0,.5f));
            Diamond(vh,p,14,dark);Diamond(vh,p,12.5f,gold);Diamond(vh,p,10.3f,light);
            Diamond(vh,p,8.5f,new Color(.025f,.12f,.21f));
            Quad(vh,p+new Vector2(-7,0),p+new Vector2(0,7),p+new Vector2(0,0),p+new Vector2(0,-7),new Color(.12f,.33f,.43f));
            Diamond(vh,p+new Vector2(-2,2),2.1f,ImperialTheme.Ivory);
            return;
        }
        float left=r.xMin+18,right=r.xMax-18;if(right<=left)return;
        Bar(vh,left,y-5,right-left,10,dark);Bar(vh,left,y-4,right-left,8,gold);
        Bar(vh,left,y-2.8f,right-left,5.6f,ImperialTheme.Navy);
        float t=Slider==null?0:Slider.normalizedValue;
        Bar(vh,left,y-1.5f,(right-left)*t,3,new Color(.80f,.62f,.29f));
        Bar(vh,left,y+.2f,(right-left)*t,1,light);
        for(int i=0;i<=4;i++){
            float x=Mathf.Lerp(left,right,i/4f);
            Bar(vh,x-.6f,y-8,1.2f,2.5f,gold);Bar(vh,x-.6f,y+5.5f,1.2f,2.5f,gold);
        }
        Diamond(vh,new Vector2(left,y),5,gold);Diamond(vh,new Vector2(right,y),5,gold);
        Diamond(vh,new Vector2(left,y),2,ImperialTheme.Navy);Diamond(vh,new Vector2(right,y),2,ImperialTheme.Navy);
    }
    void Diamond(VertexHelper vh,Vector2 p,float s,Color c){Quad(vh,p+Vector2.left*s,p+Vector2.down*s,p+Vector2.right*s,p+Vector2.up*s,c);}
    void Bar(VertexHelper vh,float x,float y,float w,float h,Color c){if(w<=0)return;Quad(vh,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),c);}
    void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint){int i=vh.currentVertCount;vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);vh.AddVert(c,tint,Vector2.zero);vh.AddVert(d,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
}
