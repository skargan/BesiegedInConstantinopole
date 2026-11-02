using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A compact, read-only view of the three groups requested for the permanent header.
public sealed class CampaignStatusStrip : MonoBehaviour
{
    readonly TMP_Text[] amounts = new TMP_Text[14];
    float nextRefresh;
    public static readonly Color[] GroupColours = {new Color(.50f,.75f,.73f),new Color(.89f,.72f,.42f),new Color(.74f,.65f,.85f)};

    public static void Create(Transform bar)
    {
        var strip=bar.gameObject.AddComponent<CampaignStatusStrip>();
        strip.Group("CITY & STORES",0,6,.012f,.438f,0);
        strip.Group("GARRISON",6,4,.445f,.716f,1);
        strip.Group("SUPPORT",10,4,.723f,.988f,2);
    }
    void Group(string name,int first,int count,float left,float right,int group)
    {
        var panel=CampaignHUD.Panel(transform,name,new Vector2(left,.055f),new Vector2(right,.70f));
        panel.GetComponent<Image>().color=ImperialTheme.Navy;
        var line=CampaignHUD.Panel(panel.transform,"Group accent",new Vector2(0,.98f),Vector2.one);
        line.GetComponent<Image>().color=GroupColours[group];line.GetComponent<Image>().raycastTarget=false;
        var title=CampaignHUD.Label(panel.transform,name,new Vector2(.014f,.75f),new Vector2(.986f,.98f),12);
        title.color=GroupColours[group];title.characterSpacing=1.1f;
        for(int n=0;n<count;n++) {
            int index=first+n;
            var cell=CampaignHUD.Panel(panel.transform,"Stat "+index,new Vector2((float)n/count,.025f),new Vector2((float)(n+1)/count,.76f));
            cell.GetComponent<Image>().color=Color.clear;
            cell.AddComponent<CampaignStatTooltip>().Index=index;
            if(n>0) {
                var divider=CampaignHUD.Panel(cell.transform,"Divider",new Vector2(0,.13f),new Vector2(.008f,.91f));
                divider.GetComponent<Image>().color=new Color(.48f,.43f,.32f,.4f);divider.GetComponent<Image>().raycastTarget=false;
            }
            AddStatArt(cell.transform,index,group);
            var label=CampaignHUD.Label(cell.transform,CampaignStatistics.Header[index].ShortName,new Vector2(.28f,.53f),new Vector2(.98f,.99f),15);
            label.enableWordWrapping=false;label.enableAutoSizing=true;label.fontSizeMin=13;label.fontSizeMax=15;
            amounts[index]=CampaignHUD.Label(cell.transform,"",new Vector2(.07f,.015f),new Vector2(.98f,.59f),25);
            amounts[index].name="Stat value "+index;
            amounts[index].enableWordWrapping=false;amounts[index].enableAutoSizing=true;amounts[index].fontSizeMin=18;amounts[index].fontSizeMax=25;
        }
    }
    // Artwork replaces presentation only; the existing glyph remains a per-stat fallback.
    void AddStatArt(Transform parent,int index,int group)
    {
        var min=new Vector2(.07f,.56f);var max=new Vector2(.24f,.98f);
        var stat=(CampaignStatArt.Stat)index;
        if(CampaignStatArt.Get(stat)==null) {
            CampaignStatGlyph.Add(parent,index,min,max).color=GroupColours[group];
            return;
        }
        var go=new GameObject("Stat symbol "+index,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;
        rect.offsetMin=rect.offsetMax=Vector2.zero;
        CampaignStatArt.Apply(go.GetComponent<Image>(),stat);
    }
    void Update()
    {
        if(!VariableSingleton.IsInitialized || Time.unscaledTime<nextRefresh)return;
        nextRefresh=Time.unscaledTime+.15f;
        for(int i=0;i<amounts.Length;i++) {
            string value=CampaignStatistics.Header[i].Value;
            if(amounts[i].text!=value)amounts[i].text=value;
            bool low=(i==0||i>=10)&&VariableSingleton.GetFloatVariable(CampaignStatistics.Header[i].Key)<.25f;
            amounts[i].color=low?new Color(1,.60f,.45f):ImperialTheme.Ivory;
        }
    }
}

// Full names and exact meanings remain discoverable; no gameplay action is attached to a statistic.
public sealed class CampaignStatTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int Index;
    GameObject panel;
    public void OnPointerEnter(PointerEventData e) {Show();}
    public void OnPointerClick(PointerEventData e) {Show();}
    public void OnPointerExit(PointerEventData e) {Clear();}
    void OnDisable() {Clear();}
    void Clear() {if(panel!=null)Destroy(panel);panel=null;}
    void Show()
    {
        if(panel!=null||!VariableSingleton.IsInitialized||CampaignHUD.Instance.HasModal)return;
        var canvas=GetComponentInParent<Canvas>();
        var corners=new Vector3[4];((RectTransform)transform).GetWorldCorners(corners);
        float centre=(corners[0].x+corners[2].x)*.5f/Screen.width;
        float left=Mathf.Clamp(centre-.14f,.01f,.71f);
        panel=CampaignHUD.Panel(canvas.transform,"Statistic detail",new Vector2(left,.72f),new Vector2(left+.28f,.875f));
        ImperialTheme.Panel(panel,false);panel.GetComponent<Image>().raycastTarget=false;
        var stat=CampaignStatistics.Header[Index];
        var label=CampaignHUD.Label(panel.transform,"<b>"+stat.Name+" · "+stat.Value+"</b>\n"+stat.Meaning,new Vector2(.045f,.08f),new Vector2(.955f,.93f),21);
        label.enableAutoSizing=true;label.fontSizeMin=17;label.fontSizeMax=21;
    }
}
