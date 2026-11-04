using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public sealed class FleetChainBackdrop : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    RawImage painting;LocationHotspot spot;GameObject caption;bool present;string key;Rect uv;
    Vector2[] shape;
    public static void Attach(GameObject screen)
    {
        var art=screen.transform.Find("Scene artwork");if(art==null)return;
        // Source pixels trace the chain and its floating supports, excluding shore towers.
        float[] top={160,256,250,272,272,266,288,271,338,273,354,266,368,272,415,273,434,266,449,272,481,270,496,262,513,269,541,267,557,260,571,266,602,263,617,253,630,261,703,256,719,246,733,254,757,250,774,240,789,246,840,240,900,241};
        float[] bottom={900,251,840,250,790,258,787,282,758,286,756,262,735,264,733,287,703,290,702,267,632,273,631,290,602,294,601,275,573,277,571,293,541,296,540,279,514,280,512,296,480,297,480,282,450,283,448,298,414,300,414,284,370,284,369,301,338,302,338,285,290,284,289,303,260,303,259,282,160,265};
        var points=new System.Collections.Generic.List<Vector2>();foreach(var list in new[]{top,bottom})for(int i=0;i<list.Length;i+=2)points.Add(new Vector2(list[i]/1672f,1-list[i+1]/941f));
        var spot=LocationHotspot.Create(art,"The Great Chain",points.ToArray(),null);
        var b=spot.gameObject.AddComponent<FleetChainBackdrop>();b.painting=art.GetComponent<RawImage>();b.spot=spot;b.shape=points.ToArray();
        b.caption=CampaignHUD.Panel(screen.transform,"Great Chain description",new Vector2(.025f,.40f),new Vector2(.425f,.56f));ImperialTheme.Panel(b.caption,false);
        var text=CampaignHUD.Label(b.caption.transform,"The great chain of the Golden Horn guards the harbour, barring the Ottoman fleet’s direct passage.",new Vector2(.045f,.10f),new Vector2(.955f,.90f),23);
        text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=23;text.alignment=TextAlignmentOptions.MidlineLeft;
        foreach(var g in b.caption.GetComponentsInChildren<Graphic>())g.raycastTarget=false;
        b.caption.SetActive(false);b.Refresh();
    }
    void LateUpdate(){Refresh();}
    void Refresh(){
        bool active=VariableSingleton.GetBoolVariable("bGoldenHornChain");
        string next=SceneBackdrop.Phase("fleet")+(active?"":"-no-chain");
        if(next!=key){key=next;var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+key);if(texture!=null)painting.texture=texture;present=active;spot.raycastTarget=active;if(!active)OnPointerExit(null);}
        if(uv!=painting.uvRect||spot.Shape==shape){uv=painting.uvRect;spot.Shape=shape.Select(p=>new Vector2((p.x-uv.x)/uv.width,(p.y-uv.y)/uv.height)).ToArray();spot.SetVerticesDirty();}
    }
    public void OnPointerEnter(PointerEventData e){if(!present)return;caption.SetActive(true);spot.SetHighlighted(true);transform.Find("Hover label").gameObject.SetActive(false);}
    public void OnPointerExit(PointerEventData e){if(caption!=null)caption.SetActive(false);if(spot!=null)spot.SetHighlighted(false);}
    void OnDisable(){OnPointerExit(null);}
}
