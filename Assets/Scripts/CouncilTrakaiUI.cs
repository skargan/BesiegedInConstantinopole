using TMPro;
using UnityEngine;

public partial class CouncilUI
{
    const string TrakaiHistory="The Mother of God image is still venerated at Trakai Basilica. A later inscription links it to a gift from Emperor Manuel II to Grand Duke Vytautas. Research dates the surviving painting to the late fifteenth century and identifies later alterations; an earlier Byzantine gift may have been a different image. The tradition nevertheless recalls genuine contacts between Lithuania and Constantinople.\n\nThis game's mission to Casimir in 1452–53 is an imagined renewal of that connection. Casimir was Grand Duke of Lithuania from 1440 and King of Poland from 1447. The lakeside meeting and his words are fictional.";
    void ReviewTrakaiGift(int index)
    {
        var pane=MilitaryCanvas("An Icon for Trakai","trakai-icon","A gift for the church of Vytautas.");
        screen.transform.Find("Scene heading").gameObject.SetActive(false);ControlSingleton.Instance.ReturnAction=()=>AdvancedDiplomacy(index);
        var painting=screen.transform.Find("Scene artwork");var background=painting.GetComponent<UnityEngine.UI.RawImage>();
        painting.GetComponent<SceneBackdrop>().enabled=false;background.uvRect=new Rect(0,0,1,1);
        var artRect=(RectTransform)painting;artRect.anchorMin=artRect.anchorMax=new Vector2(.5f,.5f);
        var artFit=painting.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();artFit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;artFit.aspectRatio=background.texture.width/(float)background.texture.height;
        var rect=(RectTransform)pane;rect.anchorMin=new Vector2(.025f,.025f);rect.anchorMax=new Vector2(.975f,.32f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        CompactText(pane,"AN ICON FOR TRAKAI",.025f,.76f,.63f,.97f,27);
        CompactText(pane,"The Mother of God watches over Constantinople in painted wood and gold. At Trakai, Vytautas founded a church in her honour. Send Casimir an icon and a letter asking that the City be remembered in prayer. A Greek holy image may find welcome even where emperors' appeals have met with silence.",.025f,.28f,.63f,.76f,21);
        CompactText(pane,"1,750 gold · +15 goodwill (maximum 95) · Once per campaign · Major action",.025f,.035f,.63f,.27f,19);
        CampaignHUD.MakeButton(pane,"Historical connection",new Vector2(.66f,.66f),new Vector2(.845f,.94f),()=>CampaignHUD.Instance.ShowReport("The Mother of God of Trakai",TrakaiHistory,()=>CampaignHUD.Instance.DismissReport()));
        CampaignHUD.MakeButton(pane,"View painting",new Vector2(.855f,.66f),new Vector2(.98f,.94f),TrakaiPaintingPreview.Show);
        var send=CampaignHUD.MakeTurnButton(pane,TrakaiIconGift.Unavailable(index)??"Send the icon to Trakai",new Vector2(.66f,.32f),new Vector2(.98f,.61f),()=>{bool ok=TrakaiIconGift.Send(index,out var report);Finish(ok,report);});send.interactable=TrakaiIconGift.Unavailable(index)==null;
        CampaignHUD.MakeButton(pane,"Return",new Vector2(.66f,.035f),new Vector2(.98f,.27f),()=>AdvancedDiplomacy(index));
    }
}

public sealed class TrakaiPaintingPreview:MonoBehaviour
{
    public static void Show(){
        var root=new GameObject("Trakai painting preview",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster),typeof(TrakaiPaintingPreview));
        root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=450;
        var scale=root.GetComponent<UnityEngine.UI.CanvasScaler>();scale.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);
        var bg=CampaignHUD.Panel(root.transform,"Painting backdrop",Vector2.zero,Vector2.one);bg.GetComponent<UnityEngine.UI.Image>().color=Color.black;
        var art=new GameObject("Trakai handover painting",typeof(RectTransform),typeof(UnityEngine.UI.RawImage),typeof(UnityEngine.UI.AspectRatioFitter));art.transform.SetParent(bg.transform,false);var rt=(RectTransform)art.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.55f);
        var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/trakai-icon");art.GetComponent<UnityEngine.UI.RawImage>().texture=texture;art.GetComponent<UnityEngine.UI.RawImage>().raycastTarget=false;
        var fit=art.GetComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture.width/(float)texture.height;
        CampaignHUD.MakeButton(root.transform,"Close",new Vector2(.39f,.025f),new Vector2(.61f,.10f),()=>Destroy(root));
    }
    void Update(){if(Input.GetKeyDown(KeyCode.Escape))Destroy(gameObject);}
}
