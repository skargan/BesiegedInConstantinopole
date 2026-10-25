using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Reference map: independent of campaign diplomacy state, so historical labels never imply live control.
public class WorldMapUI : MonoBehaviour
{
    static GameObject root;
    RectTransform viewport, content;
    Vector2 fit;
    float zoom=1;
    public static bool Close(){if(root==null)return false;Destroy(root);root=null;return true;}
    public static void Show()
    {
        Close();
        root=new GameObject("World map viewer",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image),typeof(WorldMapUI));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=225;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        root.GetComponent<Image>().color=new Color(.015f,.025f,.04f,1);
        var view=root.GetComponent<WorldMapUI>();
        CampaignHUD.Label(root.transform,"The wider world · Europe, c. 1450",new Vector2(.025f,.92f),new Vector2(.7f,.99f),30);
        var area=CampaignHUD.Panel(root.transform,"Map viewport",new Vector2(.02f,.10f),new Vector2(.98f,.91f));area.AddComponent<RectMask2D>();
        view.viewport=area.GetComponent<RectTransform>();
        var picture=new GameObject("Historical map",typeof(RectTransform),typeof(RawImage));picture.transform.SetParent(area.transform,false);
        view.content=picture.GetComponent<RectTransform>();view.content.anchorMin=view.content.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/Europe1450");picture.GetComponent<RawImage>().texture=texture;
        var scroll=area.AddComponent<ScrollRect>();scroll.viewport=view.viewport;scroll.content=view.content;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=false;scroll.scrollSensitivity=0;
        Canvas.ForceUpdateCanvases();
        float ratio=texture==null?16f/9f:(float)texture.width/texture.height;
        float width=Mathf.Min(view.viewport.rect.width,view.viewport.rect.height*ratio);view.fit=new Vector2(width,width/ratio);view.Resize(1);
        CampaignHUD.MakeButton(root.transform,"−",new Vector2(.70f,.925f),new Vector2(.755f,.985f),()=>view.Resize(view.zoom/1.4f));
        CampaignHUD.MakeButton(root.transform,"+",new Vector2(.76f,.925f),new Vector2(.815f,.985f),()=>view.Resize(view.zoom*1.4f));
        CampaignHUD.MakeButton(root.transform,"Fit",new Vector2(.82f,.925f),new Vector2(.89f,.985f),()=>{view.Resize(1);view.content.anchoredPosition=Vector2.zero;});
        CampaignHUD.MakeButton(root.transform,"Return",new Vector2(.895f,.925f),new Vector2(.98f,.985f),()=>Close());
        var caption=CampaignHUD.Label(root.transform,"Scroll to zoom · Drag to explore · Esc to return\nHistorical reference map — World History Encyclopedia. Borders shown are not live campaign control.",new Vector2(.025f,.01f),new Vector2(.975f,.09f),19);caption.alignment=TMPro.TextAlignmentOptions.Center;
    }
    void Resize(float value){zoom=Mathf.Clamp(value,1,4);content.sizeDelta=fit*zoom;}
    void Update(){float delta=Input.mouseScrollDelta.y;if(delta!=0&&RectTransformUtility.RectangleContainsScreenPoint(viewport,Input.mousePosition))Resize(zoom*Mathf.Pow(1.15f,delta));}
}
