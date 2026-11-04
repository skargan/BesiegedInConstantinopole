using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Warm side lanterns (approved concept B); centre, captions and click targets stay unchanged.
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
        vh.Clear();if(amount<=0)return;var r=rectTransform.rect;float width=Mathf.Min(r.width*.24f,110);
        const int columns=12,rows=8;
        for(int side=0;side<2;side++){
            int start=vh.currentVertCount;
            for(int row=0;row<=rows;row++)for(int column=0;column<=columns;column++){
                float u=(float)column/columns,v=(float)row/rows;
                // Rounded pools of amber light fade completely before reaching the caption centre.
                float horizontal=Mathf.Pow(1-u,2)*Mathf.Exp(-2*u*u);
                float vertical=Mathf.Pow(Mathf.Sin(v*Mathf.PI),1.4f);
                var tint=new Color(1f,.72f,.30f,.34f*amount*horizontal*vertical);
                float x=side==0?r.xMin+u*width:r.xMax-u*width;
                vh.AddVert(new Vector2(x,Mathf.Lerp(r.yMin,r.yMax,v)),tint,Vector2.zero);
            }
            for(int row=0;row<rows;row++)for(int column=0;column<columns;column++){
                int a=start+row*(columns+1)+column,b=a+1,c=a+columns+2,d=a+columns+1;
                vh.AddTriangle(a,b,c);vh.AddTriangle(a,c,d);
            }
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
