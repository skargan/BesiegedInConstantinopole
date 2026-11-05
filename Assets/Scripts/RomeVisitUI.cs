using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A visit leaves diplomacy and campaign time to their existing systems.
public sealed class RomeVisitUI:MonoBehaviour
{
    static GameObject view;
    public const string Description="Pilgrims pass beneath the mosaics of St Peter's, the ancient basilica raised by Constantine over the apostle's burial place. Beyond its colonnaded courtyard lies a church that has witnessed more than a thousand years of emperors, coronations and appeals for aid. To Constantinople, Rome offers both hope and a difficult bargain: the resources of Latin Christendom stand across a division that no desperate letter can simply erase. Here, the emperor's envoys must turn the language of Christian unity into ships, money and men.";
    public static void Show()
    {
        Close();view=new GameObject("Rome basilica visit",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(RomeVisitUI));
        var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=260;
        var scaler=view.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var shade=CampaignHUD.Panel(view.transform,"Rome backdrop",Vector2.zero,Vector2.one);shade.GetComponent<UnityEngine.UI.Image>().color=Color.black;
        var painting=new GameObject("Old Saint Peter's painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));painting.transform.SetParent(shade.transform,false);
        var rt=(RectTransform)painting.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/rome-old-st-peters");painting.GetComponent<RawImage>().texture=texture;painting.GetComponent<RawImage>().raycastTarget=false;
        var fit=painting.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?16f/9:texture.width/(float)texture.height;
        var card=CampaignHUD.Panel(view.transform,"Rome introduction",new Vector2(.58f,.045f),new Vector2(.97f,.49f));ImperialTheme.Panel(card,false);
        var title=CampaignHUD.Label(card.transform,"Rome · St Peter's",new Vector2(.05f,.81f),new Vector2(.95f,.97f),30);title.alignment=TextAlignmentOptions.Center;
        var text=CampaignHUD.Label(card.transform,Description,new Vector2(.055f,.23f),new Vector2(.945f,.81f),22);text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=22;
        CampaignHUD.MakeButton(card.transform,"Diplomacy with the Papacy",new Vector2(.045f,.045f),new Vector2(.955f,.20f),()=>{
            var council=FindObjectOfType<CouncilUI>(true);int country=CountryDiplomacy.Index("papacy");if(council==null||country<0)return;Close();WarTheatreUI.Close();council.DiplomaticPower(country);
        });
        CampaignHUD.MakeButton(view.transform,"Return to the map",new Vector2(.035f,.045f),new Vector2(.26f,.115f),()=>Close());
    }
    public static bool Close(){if(view==null)return false;view.SetActive(false);Destroy(view);view=null;return true;}
    void Update(){if(Input.GetKeyDown(KeyCode.Escape))Close();}
}
