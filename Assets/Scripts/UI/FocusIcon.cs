using UnityEngine;
using UnityEngine.UI;

// Carved-ivory emblems; retain vector fallbacks when an asset is unavailable.
[RequireComponent(typeof(CanvasRenderer))]
public class FocusIcon : MaskableGraphic
{
    public int Focus;
    public static readonly string[] Names = { "Fortifications and Artillery", "Mobilisation", "War Economy", "Faith and Unity", "Diplomacy and Spycraft" };
    public static readonly Color[] Colours = { new Color(.64f,.76f,.81f), new Color(.79f,.57f,.49f), new Color(.84f,.72f,.43f), new Color(.75f,.68f,.85f), new Color(.51f,.75f,.65f) };
    private static Texture2D ivory;
    public override Texture mainTexture { get { if(ivory==null) ivory=Resources.Load<Texture2D>("UI/MidnightMosaic/ivory-focus"); return ivory!=null?ivory:base.mainTexture; } }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if(mainTexture==ivory && ivory!=null) {
            int[] cells={4,1,0,3,2};int cell=cells[Mathf.Clamp(Focus,0,4)];
            var r=rectTransform.rect;float size=Mathf.Min(r.width,r.height);var c=r.center;
            float u=cell/5f,uu=(cell+1)/5f;
            vh.AddVert(c+new Vector2(-size/2,-size/2),Color.white,new Vector2(u,.18f));
            vh.AddVert(c+new Vector2(size/2,-size/2),Color.white,new Vector2(uu,.18f));
            vh.AddVert(c+new Vector2(size/2,size/2),Color.white,new Vector2(uu,.84f));
            vh.AddVert(c+new Vector2(-size/2,size/2),Color.white,new Vector2(u,.84f));
            vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);return;
        }
        switch (Focus)
        {
            case 0: // Battlement
                Box(vh,.16f,.15f,.84f,.68f); Box(vh,.16f,.68f,.3f,.86f); Box(vh,.43f,.68f,.57f,.86f); Box(vh,.7f,.68f,.84f,.86f); break;
            case 1: // Spear and shield
                Polygon(vh,new Vector2(.18f,.72f),new Vector2(.66f,.72f),new Vector2(.64f,.34f),new Vector2(.42f,.14f),new Vector2(.2f,.34f));
                Box(vh,.77f,.14f,.82f,.76f); Polygon(vh,new Vector2(.7f,.75f),new Vector2(.9f,.75f),new Vector2(.8f,.94f)); break;
            case 2: // Coin
                var pts=new Vector2[20]; for(int i=0;i<20;i++){float a=i*Mathf.PI/10;pts[i]=new Vector2(.5f+Mathf.Cos(a)*.36f,.5f+Mathf.Sin(a)*.36f);} Polygon(vh,pts); break;
            case 3: // Cross
                Box(vh,.43f,.12f,.57f,.9f); Box(vh,.2f,.56f,.8f,.7f); break;
            default: // Sealed letter
                Box(vh,.1f,.23f,.9f,.72f); Polygon(vh,new Vector2(.12f,.77f),new Vector2(.88f,.77f),new Vector2(.5f,.98f)); break;
        }
    }
    private void Box(VertexHelper vh,float x,float y,float xx,float yy) { Polygon(vh,new Vector2(x,y),new Vector2(xx,y),new Vector2(xx,yy),new Vector2(x,yy)); }
    private void Polygon(VertexHelper vh,params Vector2[] points)
    {
        int start=vh.currentVertCount; var rect=rectTransform.rect;
        foreach(var p in points) vh.AddVert(new Vector3(rect.x+p.x*rect.width,rect.y+p.y*rect.height),Colours[Mathf.Clamp(Focus,0,4)],Vector2.zero);
        for(int i=1;i<points.Length-1;i++) vh.AddTriangle(start,start+i,start+i+1);
    }
}
