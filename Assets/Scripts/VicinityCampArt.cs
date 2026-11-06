using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Visual districts only: campaign siege state determines whether the camps exist.
public sealed class VicinityCampArt : MonoBehaviour, ICanvasRaycastFilter, IPointerEnterHandler, IPointerExitHandler
{
    RawImage picture; Outline glow;
    public static void Add(RectTransform map, Action<ConstantinopleVicinityUI.Site> select)
    {
        var names=new[]{"Siege works","The Sultan’s camp","Camp followers","Janissary camp","Mercenary camp","Balkan auxiliaries"};
        var descriptions=new[]{
            "Ditches, timber screens and barricades creep towards the land walls. The siege crews work behind them, preparing the approaches for the next assault.",
            "Orders and messengers converge upon the Sultan’s pavilion west of the city. Around it stretches the machinery of a great army: guards, stores, horses and clerks.",
            "Beyond the fighting camps, traders, cooks, craftsmen, entertainers and preachers have made a settlement of carts and canvas. An army draws many people who never carry a weapon.",
            "The Janissaries keep their tents in ordered rows. Weapons are inspected, rations counted and men summoned when the Sultan requires them.",
            "Men serving for pay gather around their captains, horses and equipment. Their fires burn among the greater host, close enough to hear its orders.",
            "The Balkan contingents occupy their own quarters among the besiegers. Supply wagons and tethered animals crowd the spaces between their tents."
        };
        var points=new[]{new Vector2(.39f,.49f),new Vector2(.30f,.43f),new Vector2(.20f,.34f),new Vector2(.35f,.35f),new Vector2(.25f,.53f),new Vector2(.31f,.23f)};
        var texture=Resources.Load<Texture2D>("UI/Maps/VicinityLocations/camps");
        if(texture==null)return;
        for(int i=0;i<names.Length;i++)
        {
            var site=new ConstantinopleVicinityUI.Site("camp-"+i,names[i],points[i].x,points[i].y,descriptions[i]);
            var go=new GameObject(names[i],typeof(RectTransform),typeof(RawImage),typeof(Outline),typeof(Button),typeof(VicinityCampArt));go.transform.SetParent(map,false);
            var rect=(RectTransform)go.transform;rect.anchorMin=site.Point-new Vector2(.045f,.045f);rect.anchorMax=site.Point+new Vector2(.045f,.045f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            var art=go.GetComponent<VicinityCampArt>();art.picture=go.GetComponent<RawImage>();art.picture.texture=texture;art.picture.uvRect=new Rect((i%3)/3f,i<3?.5f:0,1f/3,.5f);
            art.glow=go.GetComponent<Outline>();art.glow.effectColor=new Color(1,.75f,.2f,.9f);art.glow.effectDistance=new Vector2(1,-1);art.glow.enabled=false;
            var button=go.GetComponent<Button>();button.targetGraphic=art.picture;button.transition=Selectable.Transition.None;button.onClick.AddListener(()=>select(site));
        }
    }
    public bool IsRaycastLocationValid(Vector2 screenPoint,Camera eventCamera)
    {
        if(picture==null||picture.texture==null)return false;
        Vector2 point;var rect=(RectTransform)transform;
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,screenPoint,eventCamera,out point))return false;
        var bounds=rect.rect;float x=(point.x-bounds.xMin)/bounds.width,y=(point.y-bounds.yMin)/bounds.height;
        var uv=picture.uvRect;return ((Texture2D)picture.texture).GetPixelBilinear(uv.x+x*uv.width,uv.y+y*uv.height).a>.15f;
    }
    public void OnPointerEnter(PointerEventData data){glow.enabled=true;}
    public void OnPointerExit(PointerEventData data){glow.enabled=false;}
}
