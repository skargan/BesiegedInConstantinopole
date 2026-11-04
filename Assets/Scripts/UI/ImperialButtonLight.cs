using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Extra edge light for the shared mosaic buttons; captions and click targets stay unchanged.
public sealed class ImperialButtonLight : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    Selectable owner;bool hovered,selected;float amount;
    public void SetSelected(bool value){selected=value;}
    public static void Attach(Selectable button)
    {
        if(button.GetComponent<ImperialButtonLightDriver>()!=null)return;
        var driver=button.gameObject.AddComponent<ImperialButtonLightDriver>();
        var go=new GameObject("Hover edge light",typeof(RectTransform),typeof(CanvasRenderer),typeof(ImperialButtonLight));
        go.transform.SetParent(button.transform,false);go.transform.SetAsFirstSibling();
        var r=(RectTransform)go.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(5,5);r.offsetMax=new Vector2(-5,-5);
        var light=go.GetComponent<ImperialButtonLight>();light.owner=button;light.raycastTarget=false;driver.Light=light;
    }
    public void OnPointerEnter(PointerEventData e){hovered=true;}
    public void OnPointerExit(PointerEventData e){hovered=false;}
    protected override void OnDisable(){base.OnDisable();hovered=false;selected=false;amount=0;}
    void Update(){float target=(hovered||selected)&&owner!=null&&owner.IsInteractable()?1:0;float next=Mathf.MoveTowards(amount,target,Time.unscaledDeltaTime/ .12f);if(next!=amount){amount=next;SetVerticesDirty();}}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();if(amount<=0)return;var r=rectTransform.rect;float width=Mathf.Min(r.width*.22f,70);
        for(int side=0;side<2;side++)for(int row=0;row<4;row++){
            float outer=side==0?r.xMin:r.xMax,inner=outer+(side==0?width:-width);int start=vh.currentVertCount;
            float y0=Mathf.Lerp(r.yMin,r.yMax,row/4f),y1=Mathf.Lerp(r.yMin,r.yMax,(row+1)/4f);
            float fade0=row==0?0:1,fade1=row==3?0:1;
            var low=new Color(1,.91f,.70f,.10f*amount*fade0);var high=new Color(1,.91f,.70f,.10f*amount*fade1);var clear=new Color(1,.91f,.70f,0);
            vh.AddVert(new Vector2(outer,y0),low,Vector2.zero);vh.AddVert(new Vector2(inner,y0),clear,Vector2.zero);
            vh.AddVert(new Vector2(inner,y1),clear,Vector2.zero);vh.AddVert(new Vector2(outer,y1),high,Vector2.zero);
            vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
        }
    }
}
public sealed class ImperialButtonLightDriver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public ImperialButtonLight Light;
    public void OnSelect(BaseEventData e){if(Light!=null)Light.SetSelected(true);}
    public void OnDeselect(BaseEventData e){if(Light!=null)Light.SetSelected(false);}
    public void OnPointerEnter(PointerEventData e){if(Light!=null)Light.OnPointerEnter(e);}
    public void OnPointerExit(PointerEventData e){if(Light!=null)Light.OnPointerExit(e);}
}
