using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarTheatreUI : MonoBehaviour
{
    static GameObject root; RectTransform map; Transform overlays,detail; TMP_Text hover;
    bool provinces=true,allies=true; float zoom=1; Vector2 fit; string selected="macedonia";
    public static bool Close(){if(root==null)return false;root.SetActive(false);Destroy(root);root=null;return true;}
    public static void Show(){
        Close();root=new GameObject("War Beyond the Walls map",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image),typeof(WarTheatreUI));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=220;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        root.GetComponent<Image>().color=new Color(.018f,.035f,.049f,1);var ui=root.GetComponent<WarTheatreUI>();
        CampaignHUD.Label(root.transform,"WAR BEYOND THE WALLS",new Vector2(.025f,.935f),new Vector2(.56f,.995f),30);
        CampaignHUD.MakeButton(root.transform,"Historical atlas",new Vector2(.66f,.94f),new Vector2(.81f,.99f),WorldMapUI.Show);
        CampaignHUD.MakeButton(root.transform,"Return",new Vector2(.83f,.94f),new Vector2(.975f,.99f),()=>Close());
        CampaignHUD.MakeButton(root.transform,"Ottoman provinces",new Vector2(.025f,.875f),new Vector2(.20f,.925f),()=>{ui.provinces=!ui.provinces;ui.Draw();});
        CampaignHUD.MakeButton(root.transform,"Foreign powers",new Vector2(.21f,.875f),new Vector2(.37f,.925f),()=>{ui.allies=!ui.allies;ui.Draw();});
        CampaignHUD.MakeButton(root.transform,"Symbols",new Vector2(.38f,.875f),new Vector2(.49f,.925f),ui.Legend);
        CampaignHUD.MakeButton(root.transform,"−",new Vector2(.79f,.875f),new Vector2(.84f,.925f),()=>ui.Resize(ui.zoom/1.35f));
        CampaignHUD.MakeButton(root.transform,"+",new Vector2(.85f,.875f),new Vector2(.90f,.925f),()=>ui.Resize(ui.zoom*1.35f));
        CampaignHUD.MakeButton(root.transform,"Fit",new Vector2(.91f,.875f),new Vector2(.975f,.925f),()=>{ui.Resize(1);ui.map.anchoredPosition=Vector2.zero;});
        var area=CampaignHUD.Panel(root.transform,"Theatre viewport",new Vector2(.025f,.285f),new Vector2(.975f,.86f));area.AddComponent<RectMask2D>();
        var picture=new GameObject("Geographic base",typeof(RectTransform),typeof(RawImage));picture.transform.SetParent(area.transform,false);ui.map=picture.GetComponent<RectTransform>();ui.map.anchorMin=ui.map.anchorMax=new Vector2(.5f,.5f);
        var tex=Resources.Load<Texture2D>("UI/Maps/WarTheatreGeography");picture.GetComponent<RawImage>().texture=tex;
        var scroll=area.AddComponent<ScrollRect>();scroll.viewport=area.GetComponent<RectTransform>();scroll.content=ui.map;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=false;scroll.scrollSensitivity=0;
        Canvas.ForceUpdateCanvases();var bounds=scroll.viewport.rect;float aspect=tex==null?3:(float)tex.width/tex.height;float width=Mathf.Min(bounds.width,bounds.height*aspect);ui.fit=new Vector2(width,width/aspect);ui.Resize(1);
        ui.detail=CampaignHUD.Panel(root.transform,"Province information",new Vector2(.025f,.045f),new Vector2(.975f,.245f)).transform;
        ui.hover=CampaignHUD.Label(root.transform,"Hover for gold outline · Click a territory · Scroll to zoom · Drag to explore",new Vector2(.025f,.248f),new Vector2(.975f,.283f),19);
        CampaignHUD.Label(root.transform,"1453 theatre · Schematic frontier bands · Colour shows control; symbols show reports. Unmarked land is outside this theatre's current scope.",new Vector2(.025f,.002f),new Vector2(.975f,.04f),16);
        ui.Draw();ui.Select(WarTheatre.Find(ui.selected));
    }
    void Resize(float value){zoom=Mathf.Clamp(value,1,3);map.sizeDelta=fit*zoom;}
    void Update(){if(WorldMapUIOpen())return;float delta=Input.mouseScrollDelta.y;if(delta!=0&&RectTransformUtility.RectangleContainsScreenPoint(map.parent as RectTransform,Input.mousePosition))Resize(zoom*Mathf.Pow(1.12f,delta));}
    bool WorldMapUIOpen()=>GameObject.Find("World map viewer")!=null;
    void Clear(Transform t){foreach(Transform child in t){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
    void Draw(){
        if(overlays!=null){overlays.gameObject.SetActive(false);Destroy(overlays.gameObject);}overlays=CampaignHUD.Panel(map,"Political overlays",Vector2.zero,Vector2.one).transform;overlays.GetComponent<Image>().color=Color.clear;overlays.GetComponent<Image>().raycastTarget=false;
        foreach(var r in WarTheatre.Data.regions){if(r.kind=="province"?!provinces:!allies&&r.kind!="capital")continue;
            if(r.points.Length>=3){var go=new GameObject(r.id,typeof(RectTransform),typeof(TerritoryGraphic));go.transform.SetParent(overlays,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;var shape=go.GetComponent<TerritoryGraphic>();shape.Points=r.points;shape.color=WarTheatre.Tint(r);shape.Selected=r.id==selected;shape.Click=()=>Select(r);shape.Hover=on=>{if(on)hover.text=r.name+" · "+(r.kind=="province"?WarTheatre.Status(r):r.kind=="neutral"?"Independent principality · geographical context":"Select for relations and regional interests");};}
        }
        foreach(var r in WarTheatre.Data.regions){if(r.kind=="province"?!provinces:!allies&&r.kind!="capital")continue;
            if(r.kind=="colony"||r.kind=="capital"){var go=new GameObject(r.name,typeof(RectTransform),typeof(TerritoryGraphic));go.transform.SetParent(overlays,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=r.label;rt.sizeDelta=Vector2.one*(r.kind=="capital"?20:12);var shape=go.GetComponent<TerritoryGraphic>();shape.Points=new[]{new Vector2(.5f,0),new Vector2(1,.5f),new Vector2(.5f,1),new Vector2(0,.5f)};shape.color=r.kind=="capital"?new Color(.75f,.25f,.9f):WarTheatre.Tint(r);shape.Click=()=>Select(r);shape.Hover=on=>{if(on)hover.text=r.name+" · "+(r.kind=="capital"?"Enter the Palace":r.group+" connection");};}
            if(r.kind!="colony"&&r.kind!="island"){var text=CampaignHUD.Label(overlays,r.name,new Vector2(r.label.x-.047f,r.label.y-.016f),new Vector2(r.label.x+.047f,r.label.y+.016f),r.kind=="capital"?14:16);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;text.fontStyle=FontStyles.Bold;text.enableAutoSizing=true;text.fontSizeMin=10;text.fontSizeMax=16;text.color=new Color(1,.95f,.78f);text.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1,-1);if(r.kind=="capital")text.rectTransform.anchoredPosition+=new Vector2(0,-19);}
            if(r.kind=="province"){int icon=WarTheatre.Captured(r)?3:WarTheatre.Resistance(r)>=2?2:WarTheatre.Resistance(r)==1?1:ProvincialIntelligence.Assigned>0&&ProvincialIntelligence.ProvinceId==r.id?0:-1;if(icon>=0)Icon(overlays,icon,r.label+new Vector2(0,.046f),32,()=>Select(r));}
        }
        foreach(var r in WarTheatre.Data.regions.Where(r=>r.id==r.group&&r.kind=="ally"&&WarTheatre.AtWar(r)))Icon(overlays,new[]{"venice","genoa","naples","cyprus"}.Contains(r.group)?5:4,r.front,38,()=>WarReport(r));
    }
    static void Icon(Transform parent,int index,Vector2 point,float size,UnityEngine.Events.UnityAction action=null){
        var go=new GameObject("Theatre marker "+index,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=point;rt.sizeDelta=Vector2.one*size;var image=go.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Maps/WarTheatreSymbols");image.uvRect=new Rect(index%4*.25f,(3-index/4)*.25f,.25f,.25f);image.raycastTarget=action!=null;if(action!=null)go.AddComponent<Button>().onClick.AddListener(action);
    }
    void Select(WarTheatre.Region r){
        if(r.kind=="capital"){Close();FindObjectOfType<CouncilUI>(true).Palace();return;}selected=r.id;Draw();Clear(detail);
        CampaignHUD.Label(detail,r.name,new Vector2(.02f,.72f),new Vector2(.73f,.97f),28);
        string text;
        if(r.kind=="province")text=WarTheatre.Status(r)+" · Authority "+WarTheatre.Control(r)+"%\n"+r.referenceName+" · "+r.role+"\n"+(ProvincialIntelligence.Assigned>0&&ProvincialIntelligence.ProvinceId==r.id?ProvincialIntelligence.Status:"No imperial agent assigned here.");
        else if(r.kind=="neutral")text=r.referenceName+"\n"+r.note;
        else text=(r.kind=="colony"?"Maritime possession / associated lordship · "+r.group:"Potential regional participant · "+(WarTheatre.AtWar(r)?"War reported":"No regional war reported"))+"\n"+r.note;
        var body=CampaignHUD.Explanation(detail,text,new Vector2(.02f,.06f),new Vector2(.73f,.71f),20);body.enableAutoSizing=true;body.fontSizeMin=17;body.fontSizeMax=20;
        if(r.kind=="province")CampaignHUD.MakeButton(detail,"Provincial operations",new Vector2(.76f,.45f),new Vector2(.98f,.83f),()=>{Close();FindObjectOfType<CouncilUI>(true).ProvincialOperationsFor(r.id);});
        else if(r.kind!="neutral")CampaignHUD.MakeButton(detail,"Diplomatic relations",new Vector2(.76f,.45f),new Vector2(.98f,.83f),()=>{Close();FindObjectOfType<CouncilUI>(true).DiplomaticRelations();});
        CampaignHUD.Label(detail,r.kind=="province"?"Regional production and uprisings:\nnext development stage":"An aid expedition does not\nautomatically mean a foreign war.",new Vector2(.76f,.06f),new Vector2(.98f,.4f),17);
    }
    void WarReport(WarTheatre.Region r){Clear(detail);CampaignHUD.Label(detail,r.name+" · Ottoman front",new Vector2(.02f,.7f),new Vector2(.98f,.98f),28);CampaignHUD.Explanation(detail,"War reported on this front. Provincial control and independent commitments will be recorded here as the regional warfare system develops.\nNo battle strength or casualties have been invented for this report.",new Vector2(.02f,.08f),new Vector2(.98f,.66f),22);}
    void Legend(){Clear(detail);string[] names={"Agent","Network","Insurrection","Imperial control","Land battle","Naval battle","Army","Fleet","Provisions","Artillery","Garrison","Revenue","Sabotage","Envoy","Liberation","Order"};for(int i=0;i<16;i++){float x=.06f+(i%8)*.126f,y=i<8?.73f:.26f;Icon(detail,i,new Vector2(x,y),43);var label=CampaignHUD.Label(detail,names[i],new Vector2(x-.06f,y-.24f),new Vector2(x+.06f,y-.12f),14);label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;}}
}
