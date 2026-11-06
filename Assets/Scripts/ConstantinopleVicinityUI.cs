using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A closer illustrated atlas and routes to existing screens, not a second siege simulation.
public sealed class ConstantinopleVicinityUI:MonoBehaviour
{
    public sealed class Site {public string Id,Name,Text,Route;public Vector2 Point;public int Size;public Site(string id,string name,float x,float y,string text,string route="",int size=24){Id=id;Name=name;Point=new Vector2(x,1-y);Text=text;Route=route;Size=size;}}
    // Anchors calibrated to the final painting; the source coast guide/projection is retained in documentation.
    public static readonly Site[] Sites={
        new Site("city","Constantinople",.472f,.524f,"The imperial city stands between the Golden Horn and the Sea of Marmara, behind its land and sea walls.","palace",40),
        new Site("galata","Galata",.489f,.455f,"The Genoese colony faces Constantinople across the sheltered water of the Golden Horn.","galata",27),
        new Site("rumeli","Strait-Cutter Fortress",.574f,.292f,"The new towers on the European shore command the narrow passage beside the older Anatolian fortress.","rumeli",25),
        new Site("anadolu","Anatolian Fortress",.616f,.307f,"Bayezid's fortress stands on the Asian shore, opposite the site chosen for Mehmed's new stronghold."),
        new Site("therapia","Therapia",.586f,.126f,"A fortified settlement overlooks a sheltered bay on the European bank of the Bosphorus."),
        new Site("studios","Studios",.427f,.566f,"The old district of Stoudios lies beside the southern walls, around the monastery of St John."),
        new Site("chrysopolis","Chrysopolis",.545f,.487f,"The Asian landing place faces the entrance to the Golden Horn across the Bosphorus."),
        new Site("chalcedon","Chalcedon",.565f,.575f,"The ancient settlement on the Asian shore looks west across the water towards Constantinople."),
        new Site("blachernae","Blachernae",.444f,.482f,"The imperial palace quarter occupies the northwestern edge of the city, where the land walls meet the Golden Horn."),
        new Site("horn","Golden Horn",.441f,.393f,"The long inlet shelters the city's shipping between Constantinople and Galata.","fleet",23)
    };
    static GameObject root;Transform detail;RectTransform map;Vector2 fit;float zoom=1;
    public static bool IsOpen=>root!=null;
    public static bool Close(){if(root==null)return false;root.SetActive(false);Destroy(root);root=null;return true;}
    public static void Show(){
        Close();root=new GameObject("Vicinity of Constantinople",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image),typeof(ConstantinopleVicinityUI));
        root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=235;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        ImperialTheme.Panel(root,true);var ui=root.GetComponent<ConstantinopleVicinityUI>();
        var title=CampaignHUD.Label(root.transform,"VICINITY OF CONSTANTINOPLE",new Vector2(.03f,.90f),new Vector2(.97f,.985f),34);title.alignment=TextAlignmentOptions.Center;
        CampaignHUD.MakeButton(root.transform,"Regional map",new Vector2(.80f,.035f),new Vector2(.975f,.12f),()=>Close());
        var frame=CampaignHUD.Panel(root.transform,"Local atlas frame",new Vector2(.022f,.075f),new Vector2(.785f,.895f));ImperialTheme.Panel(frame,false);
        var viewport=CampaignHUD.Panel(root.transform,"Local atlas viewport",new Vector2(.028f,.085f),new Vector2(.779f,.885f));viewport.AddComponent<RectMask2D>();
        var go=new GameObject("Bosphorus geography",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(viewport.transform,false);ui.map=(RectTransform)go.transform;ui.map.anchorMin=ui.map.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/ConstantinopleVicinity");go.GetComponent<RawImage>().texture=texture;
        Canvas.ForceUpdateCanvases();var bounds=((RectTransform)viewport.transform).rect;float aspect=(float)texture.width/texture.height;float width=Mathf.Min(bounds.width,bounds.height*aspect);ui.fit=new Vector2(width,width/aspect);ui.Resize(1);
        var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=(RectTransform)viewport.transform;scroll.content=ui.map;scroll.inertia=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=0;
        ui.detail=CampaignHUD.Panel(root.transform,"Local site information",new Vector2(.80f,.16f),new Vector2(.975f,.89f)).transform;ImperialTheme.Panel(ui.detail.gameObject,false);
        foreach(var site in Sites){if(site.Id=="rumeli"&&!MapLandmarks.FortressVisible)continue;ui.Marker(site);}
        var minus=CampaignHUD.MakeButton(root.transform,"−",new Vector2(.03f,.015f),new Vector2(.085f,.065f),()=>ui.Resize(ui.zoom/1.3f));
        CampaignHUD.MakeButton(root.transform,"Fit",new Vector2(.09f,.015f),new Vector2(.17f,.065f),()=>{ui.Resize(1);ui.map.anchoredPosition=Vector2.zero;});
        CampaignHUD.MakeButton(root.transform,"+",new Vector2(.175f,.015f),new Vector2(.23f,.065f),()=>ui.Resize(ui.zoom*1.3f));
        ui.Select(Sites[0]);
    }
    void Marker(Site site){
        var button=CampaignHUD.MakeButton(map,site.Id=="city"?"C":"",site.Point,site.Point,()=>Select(site));button.name="Local site "+site.Id;
        var rect=(RectTransform)button.transform;rect.sizeDelta=Vector2.one*site.Size;
        if(site.Id=="city")CapitalSeal.Add(button.transform);else{
            var icon=new GameObject("Site tower",typeof(RectTransform),typeof(RawImage));icon.transform.SetParent(button.transform,false);var ir=(RectTransform)icon.transform;ir.anchorMin=new Vector2(.08f,.08f);ir.anchorMax=new Vector2(.92f,.92f);ir.offsetMin=ir.offsetMax=Vector2.zero;
            var image=icon.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>(site.Id=="horn"?"UI/MidnightMosaic/Council/fleet":"UI/Maps/Landmarks/city");image.uvRect=site.Id=="horn"?new Rect(0,0,1,1):new Rect(.115f,.115f,.77f,.77f);image.raycastTarget=false;
        }
        var label=CampaignHUD.Label(map,site.Name,site.Point-new Vector2(.075f,.04f),site.Point+new Vector2(.075f,-.015f),16);label.name="Local label "+site.Id;label.alignment=TextAlignmentOptions.Center;label.fontStyle=FontStyles.Bold;label.enableAutoSizing=true;label.fontSizeMin=12;label.raycastTarget=false;
        if(site.Id=="rumeli"){rect.anchoredPosition=new Vector2(-15,14);label.rectTransform.anchoredPosition=new Vector2(-28,18);}
        if(site.Id=="anadolu"){rect.anchoredPosition=new Vector2(13,-8);label.rectTransform.anchoredPosition=new Vector2(35,-12);}
        if(site.Id=="blachernae")label.rectTransform.anchoredPosition=new Vector2(-32,20);
        if(site.Id=="horn")label.rectTransform.anchoredPosition=new Vector2(-42,14);
    }
    void Select(Site site){
        foreach(Transform child in detail){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        var title=CampaignHUD.Label(detail,site.Name,new Vector2(.06f,.78f),new Vector2(.94f,.97f),29);title.alignment=TextAlignmentOptions.Center;title.enableAutoSizing=true;title.fontSizeMin=21;title.fontSizeMax=29; if(site.Id=="city")DiplomaticNames.Bind(title,"constantinople");
        string text=site.Id=="rumeli"?MapLandmarks.FortressTitle+"\n\n"+site.Text:site.Text;
        var prose=CampaignHUD.Label(detail,text,new Vector2(.07f,.26f),new Vector2(.93f,.75f),23);prose.enableAutoSizing=true;prose.fontSizeMin=18;prose.fontSizeMax=23;
        if(site.Route!="")CampaignHUD.MakeButton(detail,site.Route=="palace"?"The Palace":site.Route=="galata"?"Galata relations":site.Route=="fleet"?"Fleet":"Inspect fortress",new Vector2(.07f,.055f),new Vector2(.93f,.19f),()=>Navigate(site.Route));
    }
    static void Navigate(string route){
        if(route=="rumeli"){MapLandmarks.ShowFortress();return;}
        Close();WarTheatreUI.Close();var council=FindObjectOfType<CouncilUI>(true);if(council==null)return;
        if(route=="palace")council.Palace();else if(route=="galata")council.Galata();else if(route=="fleet")council.Fleet();
    }
    void Resize(float amount){zoom=Mathf.Clamp(amount,1,3);map.sizeDelta=fit*zoom;}
    void Update(){if(MapLandmarks.ViewOpen)return;if(Input.mouseScrollDelta.y!=0&&RectTransformUtility.RectangleContainsScreenPoint(map.parent as RectTransform,Input.mousePosition))Resize(zoom*Mathf.Pow(1.12f,Input.mouseScrollDelta.y));}
}
