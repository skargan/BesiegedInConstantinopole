using UnityEngine;
using UnityEngine.UI;
// Two aligned arched openings. A stencil clips every exterior child to the painting.
public sealed class ChamberWindowMask:MaskableGraphic
{
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();Arch(vh,.475f,.714f,.594f,.831f,.112f);Arch(vh,.747f,.973f,.594f,.832f,.114f);}
 void Arch(VertexHelper vh,float l,float r,float bottom,float spring,float rise){int first=vh.currentVertCount;Add(vh,new Vector2((l+r)/2,bottom));Add(vh,new Vector2(l,bottom));for(int i=0;i<=28;i++){float a=Mathf.PI-i*Mathf.PI/28;Add(vh,new Vector2((l+r)/2+Mathf.Cos(a)*(r-l)/2,spring+Mathf.Sin(a)*rise));}Add(vh,new Vector2(r,bottom));int last=vh.currentVertCount-1;for(int i=first+1;i<last;i++)vh.AddTriangle(first,i,i+1);}
 void Add(VertexHelper vh,Vector2 p){var rect=rectTransform.rect;vh.AddVert(new Vector3(rect.xMin+p.x*rect.width,rect.yMin+p.y*rect.height),Color.white,p);}
}
// UVs sample the original boat itself. No regenerated replacement or rectangular water patch.
public sealed class ChamberShipGraphic:RawImage
{
 static readonly Vector2[] Outline={new Vector2(999,484),new Vector2(1017,487),new Vector2(1024,471),new Vector2(1029,456),new Vector2(1032,444),new Vector2(1037,449),new Vector2(1038,480),new Vector2(1043,478),new Vector2(1044,450),new Vector2(1048,447),new Vector2(1053,468),new Vector2(1060,479),new Vector2(1078,480),new Vector2(1072,490),new Vector2(1061,496),new Vector2(1025,500),new Vector2(1008,494)};
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var rect=rectTransform.rect;var vertices=new System.Collections.Generic.List<int>();for(int i=0;i<Outline.Length;i++){var p=Outline[i];vh.AddVert(new Vector3(rect.xMin+(p.x-996)/84*rect.width,rect.yMin+(503-p.y)/64*rect.height),color,new Vector2(p.x/1254,1-p.y/1254));vertices.Add(i);}int guard=0;while(vertices.Count>2&&guard++<200){bool found=false;for(int n=0;n<vertices.Count;n++){int a=vertices[(n+vertices.Count-1)%vertices.Count],b=vertices[n],c=vertices[(n+1)%vertices.Count];if(Cross(Outline[a],Outline[b],Outline[c])<=0)continue;bool inside=false;foreach(int j in vertices)if(j!=a&&j!=b&&j!=c&&Cross(Outline[a],Outline[b],Outline[j])>=0&&Cross(Outline[b],Outline[c],Outline[j])>=0&&Cross(Outline[c],Outline[a],Outline[j])>=0){inside=true;break;}if(inside)continue;vh.AddTriangle(a,b,c);vertices.RemoveAt(n);found=true;break;}if(!found)break;}}
 static float Cross(Vector2 a,Vector2 b,Vector2 c)=>(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
}
