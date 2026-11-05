using TMPro;
using UnityEngine;

// An illustrated visit only; campaign state and time remain with their existing systems.
public sealed class HexamilionVisitUI:MonoBehaviour
{
    static GameObject view;
    public const string Description="The Hexamilion once barred the Isthmus of Corinth, the narrow land bridge into the Morea. The 'six-mile wall' stretched between two seas, its towers guarding the road south. Before he became emperor, Constantine restored this ancient defence, hoping to give the Peloponnese room to breathe. In 1446, Murad II broke through it. Now wind moves through the breaches, and travellers pick their way past fallen masonry. To the men defending Constantinople, these ruins offer an unwelcome lesson: walls can buy time, but stone still needs soldiers, provisions and allies.";
    public static void Show()
    {
        Close();view=new GameObject("Hexamilion wall visit",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster),typeof(HexamilionVisitUI));
        var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=260;
        var scaler=view.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var backdrop=CampaignHUD.Panel(view.transform,"Hexamilion backdrop",Vector2.zero,Vector2.one);backdrop.GetComponent<UnityEngine.UI.Image>().color=Color.black;
        var painting=new GameObject("Hexamilion wall painting",typeof(RectTransform),typeof(UnityEngine.UI.RawImage),typeof(UnityEngine.UI.AspectRatioFitter));painting.transform.SetParent(backdrop.transform,false);
        var rect=(RectTransform)painting.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/hexamilion-wall");var image=painting.GetComponent<UnityEngine.UI.RawImage>();image.texture=texture;image.raycastTarget=false;
        var fit=painting.GetComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?16f/9:texture.width/(float)texture.height;
        var card=CampaignHUD.Panel(view.transform,"Hexamilion introduction",new Vector2(.035f,.035f),new Vector2(.965f,.27f));ImperialTheme.Panel(card,false);
        CampaignHUD.Label(card.transform,"The Hexamilion · Isthmus of Corinth",new Vector2(.025f,.75f),new Vector2(.975f,.97f),30);
        var text=CampaignHUD.Label(card.transform,Description,new Vector2(.025f,.07f),new Vector2(.79f,.74f),22);text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=22;
        CampaignHUD.MakeButton(card.transform,"Return to the map",new Vector2(.81f,.17f),new Vector2(.975f,.50f),()=>Close());
    }
    // ControlSingleton routes Escape here before considering the underlying map.
    public static bool Close(){if(view==null)return false;view.SetActive(false);Destroy(view);view=null;return true;}
}
