using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarTheatreUI : MonoBehaviour
{
    static GameObject root; RectTransform map; Transform overlays,detail; TMP_Text hover;
    readonly Dictionary<string,List<TerritoryGraphic>> powerShapes=new Dictionary<string,List<TerritoryGraphic>>();
    TerritoryGraphic hoveredShape;WarTheatre.Region hoveredRegion;DiplomaticNames.Style nameMode;
    void Register(TerritoryGraphic shape,WarTheatre.Region region) {
        string key=WarTheatre.HighlightKey(region);
        if(!powerShapes.TryGetValue(key,out var shapes)){shapes=new List<TerritoryGraphic>();powerShapes.Add(key,shapes);}
        shapes.Add(shape);shape.Click=()=>{if(region.id=="bisevo")BisevoUI.Show();else Select(region);};
        var power=WarTheatre.DiplomaticPower(region);
        int country=CountryDiplomacy.Index(string.IsNullOrEmpty(power.group)?power.id:power.group);
        if(region.id!="bisevo" && country>=0 && region.kind!="province" && region.kind!="capital")shape.DoubleClick=()=>{Close();FindObjectOfType<CouncilUI>(true).DiplomaticPower(country);};shape.Hover=on=>HoverPower(region,shape,on);
    }
    void ClearHover() {
        foreach(var shapes in powerShapes.Values)foreach(var shape in shapes)if(shape!=null)shape.SetHighlighted(false);
        hoveredShape=null;hoveredRegion=null;if(hover!=null)hover.text=HoverHint;
    }
    void HoverPower(WarTheatre.Region region,TerritoryGraphic source,bool on) {
        string key=WarTheatre.HighlightKey(region);
        if(!on){if(hoveredShape==source)ClearHover();return;}
        ClearHover();hoveredShape=source;hoveredRegion=region;
        if(powerShapes.TryGetValue(key,out var shapes))foreach(var shape in shapes)shape.SetHighlighted(true);
        var power=WarTheatre.DiplomaticPower(region);
        hover.text=TerritoryName(region);DiplomaticNames.ApplyTypeface(hover);
    }
    void OnDisable(){ClearHover();}
    bool provinces=true,allies=true; int viewMode; public int ViewMode=>viewMode; float zoom=1; Vector2 fit;
    const string HoverHint="";
    public static bool Close(){if(root==null)return false;root.SetActive(false);Destroy(root);root=null;return true;}
    public static void Show(){
        Close();root=new GameObject("War Beyond the Walls map",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image),typeof(WarTheatreUI));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=220;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        ImperialTheme.Panel(root,true);var ui=root.GetComponent<WarTheatreUI>();
        var title=CampaignHUD.Label(root.transform,"WAR BEYOND THE WALLS",new Vector2(.025f,.935f),new Vector2(.975f,.995f),30);
        title.alignment=TextAlignmentOptions.Center;
        CampaignHUD.MakeButton(root.transform,"Diplomacy",new Vector2(.025f,.94f),new Vector2(.15f,.99f),()=>{Close();FindObjectOfType<CouncilUI>(true).DiplomaticRelations();});
        CampaignHUD.MakeButton(root.transform,"Return",new Vector2(.16f,.94f),new Vector2(.25f,.99f),()=>Close());
        CampaignHUD.MakeButton(root.transform,"Campaign menu",new Vector2(.84f,.955f),new Vector2(.975f,.995f),()=>{Close();CampaignHUD.Instance.TogglePause();});
        var date=CampaignHUD.Label(root.transform,VariableSingleton.GetDate().ToString("d MMM yyyy"),new Vector2(.84f,.929f),new Vector2(.975f,.955f),18);date.alignment=TextAlignmentOptions.Center;
        var selector=CampaignHUD.MakeButton(root.transform,"All territories",new Vector2(.025f,.875f),new Vector2(.235f,.925f),()=>{});selector.name="Map view selector";
        selector.onClick.AddListener(()=>{ui.viewMode=(ui.viewMode+1)%4;ui.provinces=ui.viewMode!=2;ui.allies=ui.viewMode!=1;selector.GetComponentInChildren<TMP_Text>().text=new[]{"All territories","Ottoman provinces","Non-Ottoman Powers","Court religion"}[ui.viewMode];ui.Draw();ui.Clear(ui.detail);if(ui.viewMode==3)ui.ReligionLegend();});
        DiplomaticNames.Selector(root.transform,new Vector2(.245f,.875f),new Vector2(.365f,.925f));
        var zoomFrame=CampaignHUD.Panel(root.transform,"Zoom controls",new Vector2(.75f,.872f),new Vector2(.975f,.928f));ImperialTheme.Panel(zoomFrame,false);
        CampaignHUD.MakeButton(zoomFrame.transform,"−",new Vector2(.015f,.08f),new Vector2(.28f,.92f),()=>ui.Resize(ui.zoom/1.35f));
        CampaignHUD.MakeButton(zoomFrame.transform,"Fit",new Vector2(.30f,.08f),new Vector2(.70f,.92f),()=>{ui.Resize(1);ui.map.anchoredPosition=Vector2.zero;});
        CampaignHUD.MakeButton(zoomFrame.transform,"+",new Vector2(.72f,.08f),new Vector2(.985f,.92f),()=>ui.Resize(ui.zoom*1.35f));
        var atlasFrame=CampaignHUD.Panel(root.transform,"Atlas frame",new Vector2(.021f,.280f),new Vector2(.979f,.865f));ImperialTheme.Panel(atlasFrame,false);
        var area=CampaignHUD.Panel(root.transform,"Theatre viewport",new Vector2(.025f,.285f),new Vector2(.975f,.86f));area.AddComponent<RectMask2D>();
        var picture=new GameObject("Geographic base",typeof(RectTransform),typeof(RawImage));picture.transform.SetParent(area.transform,false);ui.map=picture.GetComponent<RectTransform>();ui.map.anchorMin=ui.map.anchorMax=new Vector2(.5f,.5f);
        var tex=Resources.Load<Texture2D>("UI/Maps/WarTheatreGeography");picture.GetComponent<RawImage>().texture=tex;
        var scroll=area.AddComponent<ScrollRect>();scroll.viewport=area.GetComponent<RectTransform>();scroll.content=ui.map;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=false;scroll.scrollSensitivity=0;
        Canvas.ForceUpdateCanvases();var bounds=scroll.viewport.rect;float aspect=tex==null?3:(float)tex.width/tex.height;float width=Mathf.Min(bounds.width,bounds.height*aspect);ui.fit=new Vector2(width,width/aspect);ui.Resize(1);
        ui.detail=CampaignHUD.Panel(root.transform,"Province information",new Vector2(.025f,.02f),new Vector2(.975f,.245f)).transform;
        ImperialTheme.Panel(ui.detail.gameObject,false);
        var hoverPlate=CampaignHUD.Panel(root.transform,"Map hover plaque",new Vector2(.375f,.875f),new Vector2(.735f,.925f));ImperialTheme.Panel(hoverPlate,false);
        ui.hover=CampaignHUD.Label(hoverPlate.transform,HoverHint,new Vector2(.04f,.08f),new Vector2(.96f,.92f),19);
        ui.hover.alignment=TextAlignmentOptions.Center;ui.hover.enableAutoSizing=true;ui.hover.fontSizeMin=13;ui.hover.fontSizeMax=21;ui.nameMode=DiplomaticNames.Mode;

        ui.Draw();
        CampaignHUD.Label(ui.detail,"Choose a territory",new Vector2(.02f,.72f),new Vector2(.98f,.97f),28);

    }
    void Resize(float value){zoom=Mathf.Clamp(value,1,3);map.sizeDelta=fit*zoom;}
    void Update(){if(nameMode!=DiplomaticNames.Mode){nameMode=DiplomaticNames.Mode;if(hoveredRegion!=null){hover.text=TerritoryName(hoveredRegion);DiplomaticNames.ApplyTypeface(hover);}}if(WorldMapUIOpen())return;float delta=Input.mouseScrollDelta.y;if(delta!=0&&RectTransformUtility.RectangleContainsScreenPoint(map.parent as RectTransform,Input.mousePosition))Resize(zoom*Mathf.Pow(1.12f,delta));}
    bool WorldMapUIOpen()=>GameObject.Find("World map viewer")!=null||ConstantinopleVicinityUI.IsOpen||BisevoUI.IsOpen;
    void Clear(Transform t){foreach(Transform child in t){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
    void Draw(){
        ClearHover();powerShapes.Clear();
        if(overlays!=null){overlays.gameObject.SetActive(false);Destroy(overlays.gameObject);}overlays=CampaignHUD.Panel(map,"Political overlays",Vector2.zero,Vector2.one).transform;overlays.GetComponent<Image>().color=Color.clear;overlays.GetComponent<Image>().raycastTarget=false;
        foreach(var r in WarTheatre.Data.regions){if(r.kind=="province"?!provinces:!allies&&r.kind!="capital")continue;
            if(r.points.Length>=3){var go=new GameObject(r.id,typeof(RectTransform),typeof(TerritoryGraphic));go.transform.SetParent(overlays,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;var shape=go.GetComponent<TerritoryGraphic>();shape.Points=r.points;shape.color=viewMode==3?ReligionColour(r):WarTheatre.Tint(r);Register(shape,r);}
        }
        foreach(var r in WarTheatre.Data.regions){if(r.kind=="province"?!provinces:!allies&&r.kind!="capital")continue;
            if(r.kind=="colony"){
                // Separate the Galata port marker from the Palace hotspot without moving its geographic anchor.
                Vector2 markerOffset=r.id=="galata"?new Vector2(-16,14):Vector2.zero;
                if(markerOffset!=Vector2.zero){
                    var leader=new GameObject("Galata leader",typeof(RectTransform),typeof(Image));leader.transform.SetParent(overlays,false);
                    var lr=leader.GetComponent<RectTransform>();lr.anchorMin=lr.anchorMax=r.label;lr.anchoredPosition=markerOffset*.5f;
                    lr.sizeDelta=new Vector2(markerOffset.magnitude,1);lr.localEulerAngles=new Vector3(0,0,Mathf.Atan2(markerOffset.y,markerOffset.x)*Mathf.Rad2Deg);
                    var ink=leader.GetComponent<Image>();ink.color=new Color(.87f,.78f,.48f,.8f);ink.raycastTarget=false;
                }
                var go=new GameObject(r.name,typeof(RectTransform),typeof(TerritoryGraphic));go.transform.SetParent(overlays,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=r.label;rt.anchoredPosition=markerOffset;rt.sizeDelta=Vector2.one*(r.kind=="capital"?8:5);var shape=go.GetComponent<TerritoryGraphic>();shape.Points=Enumerable.Range(0,16).Select(i=>new Vector2(.5f+.5f*Mathf.Cos(i*Mathf.PI/8),.5f+.5f*Mathf.Sin(i*Mathf.PI/8))).ToArray();shape.color=r.kind=="capital"?new Color(.96f,.78f,.38f):viewMode==3?ReligionColour(r):WarTheatre.Tint(r);Register(shape,r);}
            if(r.kind!="colony"&&r.kind!="island"&&r.id!="po_lordships_part2"){
                bool eastern=r.id=="aq_qoyunlu";
                bool compact=new[]{"candar_realm","savoy","milan","po_lordships","florence","siena","papacy","genoa","theodoro","sardinia","corsica","athens","ionian_tocco"}.Contains(r.id);
                string caption=eastern?"Turkmen\nConfederations":r.id=="milan"?"Milan":r.id=="athens"?"Athens":r.id=="ionian_tocco"?"Ionian\nLordships":r.id=="po_lordships"?"Po lords":r.id=="papacy"?"Papal\nStates":r.name;
                Vector2 half=eastern?new Vector2(.065f,.032f):compact?new Vector2(.027f,(r.id=="papacy"||r.id=="ionian_tocco")?.03f:.018f):new Vector2(.047f,.016f);
                // Edge possessions remain readable even when their territory continues beyond the atlas.
                Vector2 centre=new Vector2(Mathf.Clamp(r.label.x,half.x,1-half.x),Mathf.Clamp(r.label.y,half.y,1-half.y));
                if(r.id=="savoy")centre.y-=.028f;
                var text=CampaignHUD.Label(overlays,caption,centre-half,centre+half,r.kind=="capital"?14:compact?13:16);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;text.fontStyle=FontStyles.Bold;text.enableAutoSizing=true;text.fontSizeMin=10;text.fontSizeMax=compact?13:16;text.color=new Color(1,.95f,.78f);text.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1,-1);if(r.kind=="capital")text.rectTransform.anchoredPosition+=new Vector2(0,-19);}
            if(r.kind=="province")ProvincialMapMarkers.Add(overlays,r);
        }
        foreach(var r in WarTheatre.Data.regions.Where(r=>r.id==r.group&&CountryDiplomacy.Index(r.group)>=0&&WarTheatre.AtWar(r)))Icon(overlays,new[]{"venice","genoa","naples","cyprus"}.Contains(r.group)?5:4,r.front,38,()=>WarReport(r));
        MaritimeMapMedallions.Add(overlays,detail,hover,ClearHover);
        MapLandmarks.Add(overlays,hover,ClearHover);
        MercenaryMapMarkers.Add(overlays,hover,ClearHover);
    }
    static void Icon(Transform parent,int index,Vector2 point,float size,UnityEngine.Events.UnityAction action=null){
        var go=new GameObject("Theatre marker "+index,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=point;rt.sizeDelta=Vector2.one*size;var image=go.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Maps/WarTheatreSymbols");image.uvRect=new Rect(index%4*.25f,(3-index/4)*.25f,.25f,.25f);image.raycastTarget=action!=null;if(action!=null)go.AddComponent<Button>().onClick.AddListener(action);
    }
    void Select(WarTheatre.Region r){
        if(r.id=="candar_realm"){Close();FindObjectOfType<CouncilUI>(true).CandarCourt();return;}
        if(r.kind=="capital"){ConstantinopleVicinityUI.Show();return;}if(r.kind=="province"){ProvincialConflictUI.Show(r.id);return;}Clear(detail);
        var power=WarTheatre.DiplomaticPower(r);
        int country=CountryDiplomacy.Index(string.IsNullOrEmpty(power.group)?power.id:power.group);
        var heading=CampaignHUD.Label(detail,TerritoryName(r),new Vector2(.02f,.72f),new Vector2(.73f,.97f),28);
        if(country>=0)DiplomaticNames.Bind(heading,r.id=="sicily"?"sicily":r.id.StartsWith("sardinia")?"sardinia":CountryDiplomacy.Definitions[country].Id,TerritorySuffix(r));
        string summary=TerritorySummary(r);
        if(country>=0) {
            bool tributary=r.group=="serbia"&&!CountryDiplomacy.IsIntervening(country);
            DiplomaticSummary.Draw(detail,country,new Vector2(.02f,.44f),new Vector2(.73f,.70f),21,tributary?" · Ottoman vassal":"");
            CampaignHUD.MakeButton(detail,power.group=="naples"?"Alfonso’s court at Naples":"Diplomatic relations",new Vector2(.76f,.36f),new Vector2(.98f,.80f),()=>{Close();FindObjectOfType<CouncilUI>(true).DiplomaticPower(country);});
        }
        var body=CampaignHUD.Explanation(detail,summary,new Vector2(.02f,.08f),new Vector2(.73f,.40f),20);body.enableAutoSizing=true;body.fontSizeMin=17;body.fontSizeMax=20;
    }
    static string DisplayName(WarTheatre.Region r)=>r.id=="ionian_tocco"?"Ionian Lordships":r.name;
    public static string TerritorySuffix(WarTheatre.Region region){
        if(region.id=="sicily"||region.id.StartsWith("sardinia"))return "";
        var power=WarTheatre.DiplomaticPower(region);
        return region.id!=power.id&&(region.kind=="island"||region.kind=="colony")?" · "+DisplayName(region):"";
    }
    public static string TerritoryName(WarTheatre.Region region){
        var power=WarTheatre.DiplomaticPower(region);int country=CountryDiplomacy.Index(string.IsNullOrEmpty(power.group)?power.id:power.group);
        if(region.id=="sicily"||region.id.StartsWith("sardinia"))return DiplomaticNames.Name(region.id=="sicily"?"sicily":"sardinia");
        return country<0?DisplayName(region):DiplomaticNames.Name(CountryDiplomacy.Definitions[country].Id)+TerritorySuffix(region);
    }
    public static string TerritoryTitle(WarTheatre.Region region){
        if(region.id=="sicily")return "Sicily";
        if(region.id.StartsWith("sardinia"))return "Sardinia";
        return DisplayName(WarTheatre.DiplomaticPower(region));
    }
    public static string TerritorySummary(WarTheatre.Region region){
        if(region.id=="sicily")return "Sicily guards the passage between the western and eastern Mediterranean. Alfonso rules the island through a viceroy, while its barons defend privileges distinct from those of his mainland kingdom.";
        if(region.id.StartsWith("sardinia"))return "Sardinia lies across the western sea, its ports and fortresses held for the Crown of Aragon. Alfonso’s authority binds the island to his other realms, but its institutions and troubled history remain its own.";
        if(region.id=="naples")return "From Naples, Alfonso governs a mainland kingdom won by war and a court enriched by Mediterranean trade. His ambitions reach beyond Italy, while Sicily and Sardinia remain separate kingdoms beneath the same crown.";
        var power=WarTheatre.DiplomaticPower(region);return string.IsNullOrEmpty(power.summary)?power.name:power.summary;
    }

    void WarReport(WarTheatre.Region r){var power=WarTheatre.DiplomaticPower(r);Close();FindObjectOfType<CouncilUI>(true).DiplomaticRelationsFor(power.group);}
    public static string CourtReligion(WarTheatre.Region region) {
        if(region.kind=="province")return "Muslim";
        if(region.kind=="capital")return "Orthodox";
        var power=WarTheatre.DiplomaticPower(region);int country=CountryDiplomacy.Index(string.IsNullOrEmpty(power.group)?power.id:power.group);
        // Court classification is structured data, not an exact match on narrative labels
        // such as "Catholic court" or "Catholic dynasty · Orthodox Ruthenian lands".
        if(country>=0){
            if(CountryDiplomacy.Definitions[country].Catholic)return "Catholic";
            if(country<DiplomaticChronicles.All.Length){
                string faith=DiplomaticChronicles.All[country].Religion;
                if(faith.StartsWith("Orthodox",System.StringComparison.OrdinalIgnoreCase))return "Orthodox";
                if(faith.StartsWith("Muslim",System.StringComparison.OrdinalIgnoreCase))return "Muslim";
            }
        }
        switch(power.id){case "milan":case "savoy":case "po_lordships":case "florence":case "siena":case "sardinia":return "Catholic";case "candar_realm":return "Muslim";default:return "Unclassified";}
    }
    static Color ReligionColour(WarTheatre.Region r) {
        string religion=CourtReligion(r);
        var c=religion=="Catholic"?new Color(.88f,.64f,.25f):religion=="Orthodox"?new Color(.63f,.39f,.82f):religion.Contains("Muslim")?new Color(.18f,.66f,.47f):new Color(.58f,.60f,.63f);
        c.a=.48f;return c;
    }
    void ReligionLegend(){
        Clear(detail);
        var panel=CampaignHUD.Panel(detail,"Court religion legend",new Vector2(.012f,.06f),new Vector2(.28f,.94f));ImperialTheme.Panel(panel,false);
        CampaignHUD.Label(panel.transform,"Court religion",new Vector2(.06f,.73f),new Vector2(.94f,.96f),25);
        string[] names={"Catholic","Orthodox","Muslim"};string[] colours={"#E0A340","#A164D1","#2EA877"};
        for(int i=0;i<names.Length;i++){
            float top=.72f-i*.22f;
            var swatch=CampaignHUD.Panel(panel.transform,names[i]+" colour",new Vector2(.065f,top-.15f),new Vector2(.10f,top-.045f));
            Color colour;ColorUtility.TryParseHtmlString(colours[i],out colour);swatch.GetComponent<Image>().color=colour;swatch.GetComponent<Image>().raycastTarget=false;
            IllustratedIdentity.Faith(panel.transform,names[i],i==1,new Vector2(.13f,top-.19f),new Vector2(.23f,top));
            CampaignHUD.Label(panel.transform,"<color="+colours[i]+">"+names[i]+"</color>",new Vector2(.27f,top-.19f),new Vector2(.94f,top),21);
        }
    }
    void Legend(){Clear(detail);string[] names={"Agent","Network","Insurrection","Imperial control","Land battle","Naval battle","Army","Fleet","Provisions","Artillery","Garrison","Revenue","Sabotage","Envoy","Liberation","Order"};for(int i=0;i<16;i++){float x=.06f+(i%8)*.126f,y=i<8?.73f:.26f;Icon(detail,i,new Vector2(x,y),43);var label=CampaignHUD.Label(detail,names[i],new Vector2(x-.06f,y-.24f),new Vector2(x+.06f,y-.12f),14);label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;}}
}
