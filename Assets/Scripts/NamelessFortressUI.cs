using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Atmospheric landmark only. Opening it never advances time or changes campaign state.
public sealed class NamelessFortressUI:MonoBehaviour
{
    static GameObject view;
    public const string Message="You are far from home, friend. There is nothing for you here. For now…";
    RectTransform bird;float opened;
    public static void Show()
    {
        Close();view=new GameObject("Nameless Fortress visit",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(NamelessFortressUI));
        var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=260;
        var scaler=view.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var backdrop=CampaignHUD.Panel(view.transform,"Mountain backdrop",Vector2.zero,Vector2.one);backdrop.GetComponent<UnityEngine.UI.Image>().color=Color.black;
        var painting=new GameObject("Nameless ruins painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));painting.transform.SetParent(backdrop.transform,false);
        var rect=(RectTransform)painting.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/nameless-fortress");painting.GetComponent<RawImage>().texture=texture;painting.GetComponent<RawImage>().raycastTarget=false;
        var fit=painting.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?16f/9:texture.width/(float)texture.height;
        var vulture=new GameObject("Circling vulture",typeof(RectTransform),typeof(RawImage));vulture.transform.SetParent(painting.transform,false);
        var image=vulture.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/nameless-vulture");image.raycastTarget=false;
        var behaviour=view.GetComponent<NamelessFortressUI>();behaviour.bird=(RectTransform)vulture.transform;behaviour.opened=Time.unscaledTime;
        var card=CampaignHUD.Panel(view.transform,"Nameless message",new Vector2(.035f,.04f),new Vector2(.72f,.205f));ImperialTheme.Panel(card,false);
        var title=CampaignHUD.Label(card.transform,"Nameless Fortress",new Vector2(.035f,.57f),new Vector2(.965f,.96f),29);
        var text=CampaignHUD.Label(card.transform,Message,new Vector2(.035f,.08f),new Vector2(.965f,.56f),23);text.fontStyle=FontStyles.Italic;text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=23;
        CampaignHUD.MakeButton(view.transform,"Return",new Vector2(.79f,.045f),new Vector2(.965f,.12f),()=>Close());
    }
    public static bool Close(){if(view==null)return false;view.SetActive(false);Destroy(view);view=null;return true;}
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){Close();return;}
        if(bird==null)return;
        float angle=(Time.unscaledTime-opened)*.32f;
        var at=new Vector2(.39f+Mathf.Cos(angle)*.12f,.845f+Mathf.Sin(angle)*.052f);
        bird.anchorMin=bird.anchorMax=at;bird.anchoredPosition=Vector2.zero;
        float width=((RectTransform)bird.parent).rect.width*.035f;
        bird.sizeDelta=new Vector2(width,width*.7f);bird.localRotation=Quaternion.Euler(0,0,Mathf.Sin(angle)*18f);
        bird.localScale=new Vector3(Mathf.Cos(angle)>=0?1:-1,.85f+.15f*Mathf.Sin(angle),1);
    }
}
