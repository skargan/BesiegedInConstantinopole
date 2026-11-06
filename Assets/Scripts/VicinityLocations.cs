using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Illustrated visits share the vicinity map's lifetime and never advance campaign time.
public sealed class VicinityLocations : MonoBehaviour
{
    static GameObject view;
    public static bool IsOpen => view != null;
    public static bool Close() { if(view == null) return false; view.SetActive(false); Destroy(view); view=null; return true; }
    public static void Show(ConstantinopleVicinityUI.Site site)
    {
        Close();
        view=new GameObject("Visit "+site.Id,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        view.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        view.GetComponent<Canvas>().sortingOrder=260;
        var scaler=view.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1600,900); scaler.matchWidthOrHeight=.5f;
        var backdrop=CampaignHUD.Panel(view.transform,"Backdrop",Vector2.zero,Vector2.one); backdrop.GetComponent<Image>().color=Color.black;
        var painting=new GameObject("Location painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter)); painting.transform.SetParent(backdrop.transform,false);
        var rect=(RectTransform)painting.transform; rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/VicinityLocations/"+site.Id); painting.GetComponent<RawImage>().texture=texture; painting.GetComponent<RawImage>().raycastTarget=false;
        var fit=painting.GetComponent<AspectRatioFitter>(); fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio=texture==null?16f/9:texture.width/(float)texture.height;
        var card=CampaignHUD.Panel(view.transform,"Location introduction",new Vector2(.69f,.10f),new Vector2(.975f,.90f)); ImperialTheme.Panel(card,false);
        var title=CampaignHUD.Label(card.transform,site.Name,new Vector2(.06f,.82f),new Vector2(.94f,.96f),31); title.alignment=TextAlignmentOptions.Center;
        var prose=CampaignHUD.Label(card.transform,site.Text,new Vector2(.07f,.23f),new Vector2(.93f,.79f),24); prose.enableAutoSizing=true; prose.fontSizeMin=20; prose.fontSizeMax=24;
        CampaignHUD.MakeButton(card.transform,"Return to the map",new Vector2(.07f,.06f),new Vector2(.93f,.15f),()=>Close());
    }
}
