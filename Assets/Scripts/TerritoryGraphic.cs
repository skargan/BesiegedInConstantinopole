using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// One mesh per territory. Polygon hit-testing prevents rectangular invisible click regions.
public class TerritoryGraphic : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Vector2[] Points; public System.Action Click; public System.Action<bool> Hover;
    bool over; public bool Selected;
    public static bool Contains(Vector2 p,Vector2[] points){bool inside=false;for(int i=0,j=points.Length-1;i<points.Length;j=i++)if((points[i].y>p.y)!=(points[j].y>p.y)&&p.x<(points[j].x-points[i].x)*(p.y-points[i].y)/(points[j].y-points[i].y)+points[i].x)inside=!inside;return inside;}
    public override bool Raycast(Vector2 screen,Camera camera){if(!base.Raycast(screen,camera)||Points==null)return false;RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screen,camera,out var p);var r=rectTransform.rect;return Contains(new Vector2((p.x-r.xMin)/r.width,(p.y-r.yMin)/r.height),Points);}
    public void OnPointerEnter(PointerEventData e){over=true;SetVerticesDirty();Hover?.Invoke(true);}
    public void OnPointerExit(PointerEventData e){over=false;SetVerticesDirty();Hover?.Invoke(false);}
    public void OnPointerClick(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)Click?.Invoke();}
    static float Cross(Vector2 a,Vector2 b,Vector2 c)=>(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
    static bool Triangle(Vector2 p,Vector2 a,Vector2 b,Vector2 c)=>Cross(a,b,p)>=0&&Cross(b,c,p)>=0&&Cross(c,a,p)>=0;
    protected override void OnPopulateMesh(VertexHelper vh){
        vh.Clear();if(Points==null||Points.Length<3)return;var r=rectTransform.rect;var p=new List<Vector2>();foreach(var v in Points)p.Add(new Vector2(r.xMin+v.x*r.width,r.yMin+v.y*r.height));
        float area=0;for(int i=0;i<p.Count;i++){var b=p[(i+1)%p.Count];area+=p[i].x*b.y-b.x*p[i].y;}if(area<0)p.Reverse();
        Color fill=color;if(over||Selected)fill.a=Mathf.Min(.6f,fill.a+.12f);foreach(var v in p)vh.AddVert(v,fill,Vector2.zero);
        var remaining=new List<int>();for(int i=0;i<p.Count;i++)remaining.Add(i);int guard=p.Count*p.Count;
        while(remaining.Count>2&&guard-->0){bool cut=false;for(int i=0;i<remaining.Count;i++){int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];if(Cross(p[a],p[b],p[c])<=0)continue;bool occupied=false;foreach(int j in remaining)if(j!=a&&j!=b&&j!=c&&Triangle(p[j],p[a],p[b],p[c])){occupied=true;break;}if(occupied)continue;vh.AddTriangle(a,b,c);remaining.RemoveAt(i);cut=true;break;}if(!cut)break;}
        Color line=over||Selected?new Color(1,.79f,.28f,1):new Color(.87f,.78f,.48f,.58f);float width=over||Selected?2.1f:.7f;
        for(int i=0;i<p.Count;i++){var a=p[i];var b=p[(i+1)%p.Count];var n=new Vector2(-(b-a).y,(b-a).x).normalized*width;int k=vh.currentVertCount;vh.AddVert(a-n,line,Vector2.zero);vh.AddVert(a+n,line,Vector2.zero);vh.AddVert(b+n,line,Vector2.zero);vh.AddVert(b-n,line,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);}
    }
}
