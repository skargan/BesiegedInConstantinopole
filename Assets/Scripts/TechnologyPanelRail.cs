using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shared cursor state is owned by the active drag and released on every exit path.
public sealed class TechnologyPanelRail:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    RectTransform panel,parent;float startX,startAnchor;bool dragged;
    public static void Attach(RectTransform desk){
        var ring=new GameObject("Drag technology panel",typeof(RectTransform),typeof(TechnologyPullRing));ring.transform.SetParent(desk,false);
        var rect=(RectTransform)ring.transform;rect.anchorMin=new Vector2(.595f,.90f);rect.anchorMax=new Vector2(.675f,.98f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        var rail=ring.AddComponent<TechnologyPanelRail>();rail.panel=desk;rail.parent=desk.parent as RectTransform;
    }
    public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left)return;startX=e.position.x;startAnchor=panel.anchorMin.x;dragged=false;ByzantineCursor.SetGripping(this,true);}
    public void OnDrag(PointerEventData e){
        if(Mathf.Abs(e.position.x-startX)>4)dragged=true;
        Vector2 a,b;RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,new Vector2(startX,e.position.y),e.pressEventCamera,out a);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.position,e.pressEventCamera,out b);
        Move(startAnchor+(b.x-a.x)/parent.rect.width);
    }
    void Move(float x){x=Mathf.Clamp(x,.015f,.505f);panel.anchorMin=new Vector2(x,panel.anchorMin.y);panel.anchorMax=new Vector2(x+.48f,panel.anchorMax.y);}
    public void OnPointerUp(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left)return;if(!dragged)Move(panel.anchorMin.x>.26f?.015f:.505f);ByzantineCursor.SetGripping(this,false);}
    void OnDisable(){ByzantineCursor.SetGripping(this,false);}
}

// A small bronze door-ring, drawn crisply at the current resolution.
public sealed class TechnologyPullRing:MaskableGraphic,IPointerEnterHandler,IPointerExitHandler
{
    bool lit;
    public void OnPointerEnter(PointerEventData e){lit=true;SetVerticesDirty();GetComponentInParent<TechnologyDiscoveryRoom>()?.HoverRail(true);}
    public void OnPointerExit(PointerEventData e){lit=false;SetVerticesDirty();GetComponentInParent<TechnologyDiscoveryRoom>()?.HoverRail(false);}
    protected override void OnPopulateMesh(VertexHelper vh){
        vh.Clear();var r=rectTransform.rect;var centre=r.center;float radius=Mathf.Min(r.width,r.height)*.31f;
        for(int n=0;n<40;n++){
            float a=n*Mathf.PI*2/40,b=(n+1)*Mathf.PI*2/40;int v=vh.currentVertCount;
            Color tint=Color.Lerp(new Color(.38f,.20f,.07f),new Color(1,.83f,.42f),(.5f+.5f*Mathf.Sin(a))*(lit?1f:.8f));
            foreach(var p in new[]{centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(radius*.65f),centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(radius*.65f),centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius})vh.AddVert(p,tint,Vector2.zero);
            vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v,v+2,v+3);
        }
    }
}
