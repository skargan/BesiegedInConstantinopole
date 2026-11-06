using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A compact, read-only view of the three groups requested for the permanent header.
public sealed class CampaignStatusStrip : MonoBehaviour
{
    readonly TMP_Text[] amounts = new TMP_Text[14];
    readonly RectTransform[] cells = new RectTransform[14];
    readonly RectTransform[] symbols = new RectTransform[14];
    const float ValueSize=22, SymbolSize=22, PairGap=5, CellPadding=10;
    float lastWidth;
    bool needsLayout=true;
    float nextRefresh;
    public static readonly Color[] GroupColours = {new Color(.50f,.75f,.73f),new Color(.89f,.72f,.42f),new Color(.74f,.65f,.85f)};

    public static void Create(Transform bar)
    {
        var strip=bar.gameObject.AddComponent<CampaignStatusStrip>();
        strip.Group("IMPERIAL RESOURCES",0,6,.012f,.430f,0);
        strip.Group("MILITARY",6,4,.437f,.663f,1);
        strip.Group("SUPPORT",10,4,.670f,.878f,2);
    }
    void Group(string name,int first,int count,float left,float right,int group)
    {
        var panel=CampaignHUD.Panel(transform,name,new Vector2(left,.055f),new Vector2(right,.95f));
        panel.GetComponent<Image>().color=ImperialTheme.Navy;
        var line=CampaignHUD.Panel(panel.transform,"Group accent",new Vector2(0,.98f),Vector2.one);
        line.GetComponent<Image>().color=GroupColours[group];line.GetComponent<Image>().raycastTarget=false;
        var title=CampaignHUD.Label(panel.transform,name,new Vector2(.014f,.77f),new Vector2(.986f,.97f),12);
        title.color=GroupColours[group];title.characterSpacing=.8f;title.alignment=TextAlignmentOptions.Center;title.fontStyle=FontStyles.Normal;
        // Food needs room for a fractional stock; percentages need less than population.
        float[] widths=group==0?new[]{.85f,1.15f,1.18f,1f,1.32f,.8f}:null;
        float total=0,cursor=0;for(int n=0;n<count;n++)total+=widths==null?1:widths[n];
        for(int n=0;n<count;n++) {
            int index=first+n;
            float width=(widths==null?1:widths[n])/total;
            var cell=CampaignHUD.Panel(panel.transform,"Stat "+index,new Vector2(cursor,.06f),new Vector2(cursor+width,.75f));cursor+=width;
            cells[index]=(RectTransform)cell.transform;
            cell.GetComponent<Image>().color=Color.clear;
            cell.AddComponent<CampaignStatTooltip>().Index=index;
            if(n>0) {
                var divider=CampaignHUD.Panel(cell.transform,"Divider",new Vector2(0,.13f),new Vector2(.008f,.91f));
                divider.GetComponent<Image>().color=new Color(.48f,.43f,.32f,.4f);divider.GetComponent<Image>().raycastTarget=false;
            }
            symbols[index]=AddStatArt(cell.transform,index,group);
            amounts[index]=CampaignHUD.Label(cell.transform,"",new Vector2(.5f,.5f),new Vector2(.5f,.5f),ValueSize);
            amounts[index].name="Stat value "+index;
            amounts[index].alignment=TextAlignmentOptions.MidlineLeft;amounts[index].fontStyle=FontStyles.Normal;
            amounts[index].enableWordWrapping=false;amounts[index].enableAutoSizing=false;
        }
    }
    // Artwork replaces presentation only; the existing glyph remains a per-stat fallback.
    RectTransform AddStatArt(Transform parent,int index,int group)
    {
        var min=new Vector2(.06f,.11f);var max=new Vector2(.27f,.54f);
        // Citizens represent public support; the old raised hand is intentionally retired.
        var stat=index==11?CampaignStatArt.Stat.People:(CampaignStatArt.Stat)index;
        if(CampaignStatArt.Get(stat)==null) {
            var glyph=CampaignStatGlyph.Add(parent,index,min,max);glyph.color=GroupColours[group];
            return glyph.rectTransform;
        }
        var go=new GameObject("Stat symbol "+index,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;
        rect.offsetMin=rect.offsetMax=Vector2.zero;
        CampaignStatArt.Apply(go.GetComponent<Image>(),stat);
        if(index==11)go.GetComponent<Image>().color=new Color(.64f,.70f,.88f);
        return rect;
    }
    void Update()
    {
        if(!VariableSingleton.IsInitialized || Time.unscaledTime<nextRefresh)return;
        nextRefresh=Time.unscaledTime+.15f;
        for(int i=0;i<amounts.Length;i++) {
            string value=CampaignStatistics.Header[i].Value;
            if(amounts[i].text!=value){amounts[i].text=value;needsLayout=true;}
            bool low=(i==0||i>=10)&&VariableSingleton.GetFloatVariable(CampaignStatistics.Header[i].Key)<.25f;
            amounts[i].color=low?new Color(1,.60f,.45f):ImperialTheme.Ivory;
        }
    }
    void LateUpdate()
    {
        float width=((RectTransform)transform).rect.width;
        if(!needsLayout&&Mathf.Approximately(width,lastWidth))return;
        if(cells[0]==null||cells[0].rect.width<=0)return;
        lastWidth=width;needsLayout=false;
        float size=ValueSize;
        // Measure once when numbers or screen dimensions change. One font size keeps
        // every value on the same visual baseline, including the longest food balance.
        for(int i=0;i<amounts.Length;i++){
            amounts[i].fontSize=ValueSize;
            float measured=amounts[i].GetPreferredValues(amounts[i].text,Mathf.Infinity,Mathf.Infinity).x;
            float available=Mathf.Max(1,cells[i].rect.width-CellPadding-SymbolSize-PairGap);
            if(measured>available)size=Mathf.Min(size,ValueSize*available/measured);
        }
        size=Mathf.Clamp(size,16,ValueSize);
        for(int i=0;i<amounts.Length;i++){
            var amount=amounts[i];amount.fontSize=size;
            float textWidth=Mathf.Ceil(amount.GetPreferredValues(amount.text,Mathf.Infinity,Mathf.Infinity).x);
            float pairWidth=SymbolSize+PairGap+textWidth;
            var symbol=symbols[i];symbol.anchorMin=symbol.anchorMax=new Vector2(.5f,.5f);symbol.pivot=new Vector2(.5f,.5f);
            symbol.sizeDelta=new Vector2(SymbolSize,SymbolSize);symbol.anchoredPosition=new Vector2((SymbolSize-pairWidth)*.5f,0);
            var rect=amount.rectTransform;rect.pivot=new Vector2(0,.5f);rect.sizeDelta=new Vector2(textWidth+1,36);
            rect.anchoredPosition=new Vector2(-pairWidth*.5f+SymbolSize+PairGap,0);
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
