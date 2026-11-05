using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Presentation state supplements the existing passage flag: an open route is not proof of ruins.
public sealed class MapLandmarks : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
{
    public const string OutcomeKey="iRumeliLandmark"; // 1 known; 2 ruined; 3 captured; 4 artillery disabled
    public static int Outcome=>DailyLifeEvents.I(OutcomeKey);
    public static bool FortressVisible=>Outcome>0||VariableSingleton.GetDate()>=new DateTime(1452,9,1);
    public static bool Building=>Outcome<=1&&VariableSingleton.GetDate()<new DateTime(1452,9,1);
    public static string FortressTitle=>Building?"Rumeli Fortress · under construction":Outcome==2?"The ruins of Rumeli":Outcome==3?"Rumeli Fortress · captured":Outcome==4?"Rumeli Fortress · guns silenced":"The Strait-Cutter Fortress";
    public static string FortressText=>Outcome==0&&VariableSingleton.GetBoolVariable("bStraitsOpen")?
        "Rumeli watches over the narrow water between Constantinople and the Black Sea. The passage is currently open to our shipping.\n\nThe surviving records do not establish whether the fortress was captured, damaged or otherwise deprived of its power to bar the route.":Building?
        "Stone barges gather beneath the European shore. Mehmed's masons raise towers above the narrow water, where ships from the Black Sea must pass on their way to Constantinople.\n\nThe works are not yet complete. When the guns are mounted, every vessel beneath these walls will sail under their threat.":Outcome==2?
        "Broken masonry slopes towards the water, and the gun platforms stand empty. The fortress that was meant to sever Constantinople from the Black Sea has been reduced to ruins.\n\nA merchantman makes northwards under full sail. These guns no longer threaten her passage; whatever dangers await at sea, they will not come from this shore.":Outcome==3?
        "The fortress has fallen into imperial hands. Its towers still command the narrow water, but the men who watch from them no longer serve Mehmed.\n\nThe costly assault has secured this stretch of the Bosphorus. Ships bound for the Black Sea can pass without facing Ottoman fire from these battlements.":Outcome==4?
        "The towers remain, but their artillery has been silenced. Fire and shattered powder stores have robbed the fortress of the weapons that made this narrow passage so dangerous.\n\nFor now, ships can pass beyond its broken gun platforms. The Sultan's masons may return one day; today the sea lies open beyond them.":
        "Mehmed's fortress grips the European shore where the Bosphorus narrows. From these high walls, the gunners watch the merchantmen that carry grain, men and news between Constantinople and the Black Sea.\n\nAs long as its guns remain in Ottoman hands, every ship passing beneath them is under threat. A captain may have wind in his sails and still find the strait closed before him.";
    string title; TMP_Text hint; Action clear; Outline glow;
    static GameObject view;
    public static void Add(Transform map,TMP_Text hover,Action clearHover)
    {
        Create(map,"Constantinople",WarTheatre.Find("constantinople").label,42,hover,clearHover,()=>OpenCourt(null));
        Create(map,"Ragusa",WarTheatre.Find("ragusa").label+new Vector2(0,-.035f),29,hover,clearHover,()=>OpenCourt("ragusa"));
        Create(map,"Rome",WarTheatre.Find("papacy").label+new Vector2(0,-.045f),29,hover,clearHover,RomeVisitUI.Show);
        // Isthmus of Corinth, at the Morea/Athens boundary in WarTheatre.json.
        Create(map,"The Hexamilion",new Vector2(.2985f,.3236f),25,hover,clearHover,HexamilionVisitUI.Show);
        // Alamut's Alborz location, aligned to the illustrated (non-georeferenced) map.
        Create(map,"Nameless Fortress",new Vector2(.925f,.255f),27,hover,clearHover,NamelessFortressUI.Show);
        Create(map,"Malta · Mercenary brokers",MercenaryMapMarkers.Malta,29,hover,clearHover,()=>MercenaryUI.Show());
        if(FortressVisible)Create(map,"Strait-Cutter Fortress",WarTheatre.Find("constantinople").label+new Vector2(.003f,.08f),32,hover,clearHover,ShowFortress);
    }
    static void OpenCourt(string id){var council=FindObjectOfType<CouncilUI>(true);if(council==null)return;int country=id==null?-1:CountryDiplomacy.Index(id);if(id!=null&&country<0)return;WarTheatreUI.Close();if(id==null)council.Palace();else council.DiplomaticPower(country);}
    static void Create(Transform parent,string name,Vector2 pos,float size,TMP_Text hover,Action clearHover,UnityEngine.Events.UnityAction click)
    {
        var go=new GameObject("Landmark · "+name,typeof(RectTransform),typeof(RawImage),typeof(Button),typeof(Outline),typeof(MapLandmarks));go.transform.SetParent(parent,false);
        var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=pos;rt.sizeDelta=Vector2.one*size;
        var image=go.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/city");image.uvRect=new Rect(.115f,.115f,.77f,.77f);
        var marker=go.GetComponent<MapLandmarks>();marker.title=name;marker.hint=hover;marker.clear=clearHover;marker.glow=go.GetComponent<Outline>();marker.glow.effectColor=new Color(1,.85f,.4f,1);marker.glow.effectDistance=new Vector2(2,2);marker.glow.enabled=false;
        var button=go.GetComponent<Button>();button.targetGraphic=image;button.transition=Selectable.Transition.None;button.onClick.AddListener(click);
    }
    public void OnPointerEnter(PointerEventData e){clear?.Invoke();glow.enabled=true;if(hint!=null)hint.text=title;}
    public void OnPointerExit(PointerEventData e){glow.enabled=false;if(hint!=null&&hint.text==title)hint.text="";}
    public bool IsRaycastLocationValid(Vector2 p,Camera c){var r=(RectTransform)transform;return RectTransformUtility.ScreenPointToLocalPointInRectangle(r,p,c,out var local)&&(local-r.rect.center).magnitude<=r.rect.width*.5f;}
    public static void CloseView(){if(view==null)return;view.SetActive(false);Destroy(view);view=null;}
    public static void ShowFortress()
    {
        CloseView();view=new GameObject("Rumeli fortress view",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(FortressViewEscape));
        var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=250;
        var scaler=view.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var bg=CampaignHUD.Panel(view.transform,"Fortress artwork",Vector2.zero,Vector2.one);bg.GetComponent<Image>().color=Color.black;
        var art=new GameObject("Painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));art.transform.SetParent(bg.transform,false);var rt=(RectTransform)art.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/rumeli-"+(Outcome==2||Outcome==4?"ruined":"intact"));art.GetComponent<RawImage>().texture=texture;art.GetComponent<RawImage>().raycastTarget=false;
        var aspect=art.GetComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;aspect.aspectRatio=(float)texture.width/texture.height;
        var panel=CampaignHUD.Panel(view.transform,"Fortress description",new Vector2(.035f,.045f),new Vector2(.55f,.38f));ImperialTheme.Panel(panel,false);
        CampaignHUD.Label(panel.transform,FortressTitle,new Vector2(.035f,.78f),new Vector2(.965f,.96f),28);
        var text=CampaignHUD.Label(panel.transform,FortressText,new Vector2(.035f,.06f),new Vector2(.965f,.76f),23);text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=23;
        CampaignHUD.MakeButton(view.transform,"Return",new Vector2(.79f,.045f),new Vector2(.965f,.12f),CloseView);
    }
}
public sealed class FortressViewEscape:MonoBehaviour {void Update(){if(Input.GetKeyDown(KeyCode.Escape))MapLandmarks.CloseView();}}
