using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shared cursor state is owned by the active drag and released on every exit path.
public sealed class TechnologyPanelRail:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
{
    RectTransform panel,parent;float startX,startAnchor;bool dragged;
    public static void Attach(RectTransform desk){
        var ring=new GameObject("Drag technology panel",typeof(RectTransform),typeof(TechnologyPullRing));ring.transform.SetParent(desk,false);
        var rect=(RectTransform)ring.transform;rect.anchorMin=rect.anchorMax=Vector2.one;
        rect.pivot=new Vector2(.70f,.70f);rect.sizeDelta=new Vector2(44,44);rect.anchoredPosition=new Vector2(-4,-4);
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

// The generated bronze hardware overlaps the desk's upper-right goldwork.
public sealed class TechnologyPullRing:MaskableGraphic,IPointerEnterHandler,IPointerExitHandler
{
    bool lit;
    Texture2D ring;
    public override Texture mainTexture { get { if(ring==null)ring=Resources.Load<Texture2D>("UI/MidnightMosaic/technology-pull-ring");return ring==null?Texture2D.whiteTexture:ring; } }
    public void OnPointerEnter(PointerEventData e){lit=true;SetVerticesDirty();GetComponentInParent<TechnologyDiscoveryRoom>()?.HoverRail(true);}
    public void OnPointerExit(PointerEventData e){lit=false;SetVerticesDirty();GetComponentInParent<TechnologyDiscoveryRoom>()?.HoverRail(false);}
    protected override void OnPopulateMesh(VertexHelper vh){
        vh.Clear();var r=rectTransform.rect;Color tint=lit?new Color(1f,.94f,.76f):Color.white;
        vh.AddVert(new Vector2(r.xMin,r.yMin),tint,new Vector2(0,0));
        vh.AddVert(new Vector2(r.xMax,r.yMin),tint,new Vector2(1,0));
        vh.AddVert(new Vector2(r.xMax,r.yMax),tint,new Vector2(1,1));
        vh.AddVert(new Vector2(r.xMin,r.yMax),tint,new Vector2(0,1));
        vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
    }
}
