using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Reusable in conversations and future event choices. No success probability is disclosed.
public class EmperorAttributeBadge:MaskableGraphic
{
    public int Stat;
    static readonly Sprite[] art=new Sprite[3];
    static readonly string[] paths={"charisma","intellect","martial"};
    public static Color Colour(int stat)=>stat==2?new Color(.88f,.34f,.30f):stat==1?new Color(.36f,.64f,.94f):new Color(.94f,.73f,.31f);
    public static void DrawNew(Transform parent)
    {
        var go=new GameObject("Unread topic",typeof(RectTransform),typeof(EmperorAttributeBadge));go.transform.SetParent(parent,false);var mark=go.GetComponent<EmperorAttributeBadge>();mark.Stat=-1;mark.color=Colour(0);mark.raycastTarget=false;var r=mark.rectTransform;r.anchorMin=new Vector2(.025f,.30f);r.anchorMax=new Vector2(.05f,.70f);r.offsetMin=r.offsetMax=Vector2.zero;
    }
    public static void Draw(Transform parent,int stat,Vector2 min,Vector2 max,int required=0,bool enabled=true)
    {
        var go=new GameObject("Attribute · "+EmperorProfile.Names[stat],typeof(RectTransform));go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
        var texture=Resources.Load<Texture2D>("UI/Identity/Attributes/"+paths[stat]);
        if(texture!=null){if(art[stat]==null)art[stat]=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));var image=go.AddComponent<Image>();image.sprite=art[stat];image.preserveAspect=true;image.raycastTarget=false;image.color=enabled?Color.white:new Color(.38f,.38f,.40f,.60f);}
        else{var icon=go.AddComponent<EmperorAttributeBadge>();icon.Stat=stat;icon.color=enabled?Colour(stat):Color.Lerp(Colour(stat),new Color(.38f,.39f,.42f),.65f);icon.raycastTarget=false;}
        if(required>0){var t=CampaignHUD.Label(go.transform,required.ToString(),new Vector2(0,-.32f),new Vector2(1,.15f),17);t.alignment=TextAlignmentOptions.Center;t.color=enabled?Colour(stat):Color.Lerp(Colour(stat),new Color(.38f,.39f,.42f),.65f);}
    }
    VertexHelper mesh;
    Vector2 P(float x,float y){var r=rectTransform.rect;float d=Mathf.Min(r.width,r.height);return new Vector2(r.center.x+(x-.5f)*d,r.center.y+(y-.5f)*d);}
    void Line(float ax,float ay,float bx,float by,float width=.045f)
    {
        Vector2 a=P(ax,ay),b=P(bx,by),n=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*width*.5f;int i=mesh.currentVertCount;
        mesh.AddVert(a+n,color,Vector2.zero);mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);mesh.AddVert(b+n,color,Vector2.zero);mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
    }
    void Ring(float x,float y,float radius){for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Line(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius,.028f);}}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();mesh=vh;
        if(Stat<0){Line(.5f,.12f,.84f,.5f);Line(.84f,.5f,.5f,.88f);Line(.5f,.88f,.16f,.5f);Line(.16f,.5f,.5f,.12f);return;}
        if(Stat==2){Line(.20f,.18f,.78f,.80f,.055f);Line(.20f,.80f,.78f,.18f,.055f);Line(.12f,.30f,.33f,.12f);Line(.68f,.12f,.89f,.30f);Line(.70f,.78f,.80f,.91f);Line(.80f,.91f,.84f,.73f);Line(.14f,.73f,.20f,.91f);Line(.20f,.91f,.30f,.78f);}
        else if(Stat==1){Line(.23f,.35f,.36f,.20f);Line(.36f,.20f,.68f,.20f);Line(.68f,.20f,.81f,.43f);Line(.23f,.35f,.72f,.35f);Line(.72f,.35f,.87f,.51f);Line(.87f,.51f,.74f,.49f);Ring(.23f,.38f,.12f);Line(.48f,.20f,.48f,.12f);Line(.32f,.12f,.67f,.12f);Line(.73f,.52f,.66f,.68f);Line(.66f,.68f,.75f,.88f);Line(.75f,.88f,.82f,.68f);Line(.82f,.68f,.73f,.52f);}
        else{Ring(.5f,.52f,.23f);Ring(.5f,.52f,.17f);Line(.5f,.64f,.5f,.40f);Line(.42f,.55f,.58f,.55f);for(int i=0;i<5;i++){float y=.20f+i*.12f;float x=.20f-Mathf.Sin(i*.55f)*.06f;Line(x,y,x+.08f,y+.07f,.035f);Line(1-x,y,1-x-.08f,y+.07f,.035f);}Line(.38f,.14f,.62f,.14f);}
    }
}
