using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Reusable native UI medallion: imperial purple, gilded rings and the capital's monogram.
public sealed class CapitalSeal:MaskableGraphic
{
    public bool Lit;
    public static void Add(Transform parent){
        var background=parent.GetComponent<Graphic>();if(background!=null)background.color=Color.clear;
        var button=parent.GetComponent<Button>();if(button!=null)button.transition=Selectable.Transition.None;
        foreach(Transform child in parent)if(child.name=="Imperial goldwork"||child.GetComponent<TMP_Text>()!=null)child.gameObject.SetActive(false);
        var go=new GameObject("Imperial capital seal",typeof(RectTransform),typeof(CapitalSeal));go.transform.SetParent(parent,false);
        var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        var seal=go.GetComponent<CapitalSeal>();seal.raycastTarget=false;
        var monogram=CampaignHUD.Label(go.transform,"C",new Vector2(.10f,.05f),new Vector2(.90f,.95f),44);monogram.alignment=TextAlignmentOptions.Center;monogram.fontStyle=FontStyles.Bold;monogram.color=ImperialTheme.Gold;monogram.enableAutoSizing=true;monogram.fontSizeMax=44;monogram.fontSizeMin=18;monogram.raycastTarget=false;monogram.margin=Vector4.zero;
        parent.gameObject.AddComponent<CapitalSealHover>().Seal=seal;
    }
    protected override void OnPopulateMesh(VertexHelper vh){
        vh.Clear();float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.49f;Vector2 centre=rectTransform.rect.center;
        Ring(vh,centre,radius,0,new Color(.095f,.035f,.17f,.98f));
        var gold=Lit?new Color(1f,.91f,.55f):ImperialTheme.Gold;
        Ring(vh,centre,radius,radius*.90f,gold);Ring(vh,centre,radius*.81f,radius*.79f,gold);
    }
    static void Ring(VertexHelper vh,Vector2 centre,float outer,float inner,Color colour){
        for(int i=0;i<80;i++){float a=i*Mathf.PI*2/80,b=(i+1)*Mathf.PI*2/80;int n=vh.currentVertCount;
            vh.AddVert(centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*inner,colour,Vector2.zero);vh.AddVert(centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*outer,colour,Vector2.zero);
            vh.AddVert(centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*outer,colour,Vector2.zero);vh.AddVert(centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*inner,colour,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
}
public sealed class CapitalSealHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,ICanvasRaycastFilter
{
    public CapitalSeal Seal;
    public void OnPointerEnter(PointerEventData data){if(Seal!=null){Seal.Lit=true;Seal.SetVerticesDirty();}}
    public void OnPointerExit(PointerEventData data){if(Seal!=null){Seal.Lit=false;Seal.SetVerticesDirty();}}
    public bool IsRaycastLocationValid(Vector2 point,Camera camera){var rect=(RectTransform)transform;return RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,point,camera,out var local)&&(local-rect.rect.center).magnitude<=Mathf.Min(rect.rect.width,rect.rect.height)*.5f;}
}
