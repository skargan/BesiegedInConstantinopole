using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Polygon coordinates belong to the fitted painting, not the window or its letterbox.
public class LocationHotspot:MaskableGraphic,ICanvasRaycastFilter,IPointerEnterHandler,IPointerExitHandler,IPointerMoveHandler,IPointerClickHandler
{
    public Vector2[] Shape;
    public UnityAction Action;
    private bool highlighted;
    private RectTransform caption;
    public static LocationHotspot Create(Transform painting,string label,Vector2[] polygon,UnityAction action)
    {
        var go=new GameObject("Hotspot · "+label,typeof(RectTransform),typeof(LocationHotspot));go.transform.SetParent(painting,false);
        var hotspot=go.GetComponent<LocationHotspot>();var r=hotspot.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
        hotspot.Shape=polygon;hotspot.Action=action;hotspot.raycastTarget=true;
        var box=CampaignHUD.Panel(go.transform,"Hover label",Vector2.zero,Vector2.zero);box.GetComponent<Image>().color=new Color(.025f,.045f,.07f,.95f);box.GetComponent<Image>().raycastTarget=false;
        hotspot.caption=box.GetComponent<RectTransform>();hotspot.caption.pivot=new Vector2(0,1);hotspot.caption.sizeDelta=new Vector2(220,43);
        var text=CampaignHUD.Label(box.transform,label,new Vector2(.05f,.05f),new Vector2(.95f,.95f),21);text.color=new Color(1f,.86f,.54f);text.raycastTarget=false;text.alignment=TMPro.TextAlignmentOptions.Center;
        box.SetActive(false);hotspot.SetVerticesDirty();return hotspot;
    }
    public bool Contains(Vector2 p)
    {
        bool inside=false;if(Shape==null||Shape.Length<3)return false;
        for(int i=0,j=Shape.Length-1;i<Shape.Length;j=i++){
            Vector2 a=Shape[i],b=Shape[j];
            if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
        }return inside;
    }
    public bool IsRaycastLocationValid(Vector2 point,Camera camera)
    {
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,point,camera,out var local))return false;
        Rect r=rectTransform.rect;return r.width>0&&r.height>0&&Contains(new Vector2((local.x-r.xMin)/r.width,(local.y-r.yMin)/r.height));
    }
    public void OnPointerEnter(PointerEventData e){SetHighlighted(true);PositionCaption(e);}
    public void OnPointerExit(PointerEventData e)=>SetHighlighted(false);
    public void OnPointerMove(PointerEventData e)=>PositionCaption(e);
    public void OnPointerClick(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&IsRaycastLocationValid(e.position,e.pressEventCamera))Action?.Invoke();}
    public void SetHighlighted(bool value){highlighted=value;if(caption!=null)caption.gameObject.SetActive(value);SetVerticesDirty();}
    private void PositionCaption(PointerEventData e)
    {
        if(caption==null)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.enterEventCamera,out var p);Rect r=rectTransform.rect;
        float x=p.x-r.xMin; float y=p.y-r.yMin; float labelX=x+40+220<=r.width?x+40:x-260;
        caption.anchoredPosition=new Vector2(Mathf.Clamp(labelX,4,Mathf.Max(4,r.width-224)),Mathf.Clamp(y+22,47,r.height-4));
    }
    protected override void OnDisable(){SetHighlighted(false);base.OnDisable();}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();Rect r=rectTransform.rect;
        // Keep the surface registered for hit-testing while visually transparent at rest.
        vh.AddVert(new Vector2(r.xMin,r.yMin),Color.clear,Vector2.zero);vh.AddVert(new Vector2(r.xMax,r.yMin),Color.clear,Vector2.zero);vh.AddVert(new Vector2(r.xMax,r.yMax),Color.clear,Vector2.zero);vh.AddVert(new Vector2(r.xMin,r.yMax),Color.clear,Vector2.zero);vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
        if(!highlighted||Shape==null)return;
        float scale=canvas==null?1:Mathf.Max(.1f,canvas.scaleFactor);
        Stroke(vh,r,7/scale,new Color(.95f,.63f,.18f,.10f));Stroke(vh,r,3.5f/scale,new Color(1f,.76f,.3f,.27f));Stroke(vh,r,1.5f/scale,new Color(1f,.87f,.52f,.95f));
    }
    private void Stroke(VertexHelper vh,Rect r,float width,Color tint)
    {
        for(int i=0;i<Shape.Length;i++){
            Vector2 a=new Vector2(r.xMin+Shape[i].x*r.width,r.yMin+Shape[i].y*r.height),b=new Vector2(r.xMin+Shape[(i+1)%Shape.Length].x*r.width,r.yMin+Shape[(i+1)%Shape.Length].y*r.height);
            Vector2 delta=b-a,normal=new Vector2(-delta.y,delta.x).normalized*width*.5f;int n=vh.currentVertCount;
            vh.AddVert(a-normal,tint,Vector2.zero);vh.AddVert(a+normal,tint,Vector2.zero);vh.AddVert(b+normal,tint,Vector2.zero);vh.AddVert(b-normal,tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
}
