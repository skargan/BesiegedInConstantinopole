using TMPro;
using UnityEngine;

// City visits lead to existing diplomacy; they do not move the emperor or advance time.
public sealed class MaritimeCityVisitUI:MonoBehaviour
{
    static GameObject view;
    public static string CurrentCity {get;private set;}
    public static string Description(string city)=>city=="genoa"?
        "Genoa crowds between the Ligurian hills and the sea, its houses climbing above a harbour filled with cargo, arguments and credit. The republic's merchants have interests far beyond these quays. In Constantinople, Genoese captains and traders have much to protect, but their purses do not all open at the same command. An appeal made here must find its way through family interests, commercial calculations and the uncomfortable question of who will pay.":
        "The Doge's Palace looks out across a lagoon crowded with vessels, while the councils within weigh the business of a maritime republic. Venice's wealth travels by sea, and the fate of Constantinople reaches into its accounts, harbours and merchant households. Concern is plentiful; a commitment requires more work. Your envoys must persuade the republic that helping the City is worth the ships, money and trouble that its councillors will certainly count.";
    public static Vector2 MapPosition(string city)=>city=="genoa"?new Vector2(72f/2048f,1-174f/683f):new Vector2(190f/2048f,1-110f/683f);
    public static void Show(string city)
    {
        if(city!="genoa"&&city!="venice")return;
        Close();CurrentCity=city;string name=city=="genoa"?"Genoa":"Venice";
        view=new GameObject("Maritime city visit",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster),typeof(MaritimeCityVisitUI));
        var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=260;
        var scaler=view.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var backdrop=CampaignHUD.Panel(view.transform,"City backdrop",Vector2.zero,Vector2.one);backdrop.GetComponent<UnityEngine.UI.Image>().color=Color.black;
        var art=new GameObject("City painting",typeof(RectTransform),typeof(UnityEngine.UI.RawImage),typeof(UnityEngine.UI.AspectRatioFitter));art.transform.SetParent(backdrop.transform,false);
        var rect=(RectTransform)art.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/"+city+"-harbour");var picture=art.GetComponent<UnityEngine.UI.RawImage>();picture.texture=texture;picture.raycastTarget=false;
        var fit=art.GetComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?16f/9:texture.width/(float)texture.height;
        var card=CampaignHUD.Panel(view.transform,"City introduction",new Vector2(.035f,.035f),new Vector2(.965f,.27f));ImperialTheme.Panel(card,false);
        var title=CampaignHUD.Label(card.transform,name,new Vector2(.025f,.76f),new Vector2(.765f,.97f),30);title.alignment=TextAlignmentOptions.MidlineLeft;
        var text=CampaignHUD.Label(card.transform,Description(city),new Vector2(.025f,.07f),new Vector2(.765f,.75f),22);text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=22;
        var council=FindObjectOfType<CouncilUI>(true);int country=CountryDiplomacy.Index(city);
        var diplomacy=CampaignHUD.MakeButton(card.transform,"Diplomacy with "+name,new Vector2(.79f,.51f),new Vector2(.975f,.82f),()=>{
            if(council==null||country<0)return;Close();WarTheatreUI.Close();council.DiplomaticPower(country);
        });diplomacy.interactable=council!=null&&country>=0;
        CampaignHUD.MakeButton(card.transform,"Return to the map",new Vector2(.79f,.12f),new Vector2(.975f,.43f),()=>Close());
    }
    // Shared navigation checks this before closing the map beneath it.
    public static bool Close(){if(view==null)return false;view.SetActive(false);Destroy(view);view=null;CurrentCity=null;return true;}
}
