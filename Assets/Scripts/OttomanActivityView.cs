using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A read-only view of the siege works. Artwork uses its own random stream.
public sealed class OttomanActivityView
{
    public static readonly string[] ArtPaths={"UI/SiegeActivity/calm-watch","UI/SiegeActivity/moat-assault","UI/SiegeActivity/varangian-defence"};
    static readonly System.Random artRandom=new System.Random();
    static int lastArt=-1;
    readonly GameObject panel;
    readonly RawImage painting;
    readonly TMP_Text day,report;
    readonly ScrollRect scroll;
    readonly TMP_Text[] statuses=new TMP_Text[7];
    readonly Image[] fills=new Image[7];
    public GameObject Panel=>panel;
    public int ArtworkIndex {get;private set;}
    public static int NextArtwork(){lastArt=lastArt<0?artRandom.Next(3):(lastArt+artRandom.Next(1,3))%3;return lastArt;}

    public OttomanActivityView(Transform root,Button proceed)
    {
        // Retire the old centred presentation, retaining its result/decision objects.
        foreach(Transform child in root)child.gameObject.SetActive(false);
        var back=new GameObject("Ottoman siege painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));
        back.transform.SetParent(root,false);back.transform.SetAsFirstSibling();
        painting=back.GetComponent<RawImage>();painting.raycastTarget=false;
        var fit=back.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=16f/9;
        panel=CampaignHUD.Panel(root,"Ottoman activity report",new Vector2(.555f,.035f),new Vector2(.985f,.965f));ImperialTheme.Panel(panel,false);
        var title=CampaignHUD.Label(panel.transform,"OTTOMAN SIEGE ACTIVITY",new Vector2(.045f,.905f),new Vector2(.955f,.975f),30);title.alignment=TextAlignmentOptions.Center;
        day=CampaignHUD.Label(panel.transform,"",new Vector2(.045f,.855f),new Vector2(.955f,.91f),23);day.alignment=TextAlignmentOptions.Center;day.color=ImperialTheme.Gold;
        var viewport=CampaignHUD.Panel(panel.transform,"Daily report viewport",new Vector2(.045f,.535f),new Vector2(.925f,.845f));viewport.GetComponent<Image>().color=Color.clear;viewport.AddComponent<RectMask2D>();
        report=CampaignHUD.Label(viewport.transform,"",Vector2.zero,Vector2.one,25);
        report.rectTransform.anchorMin=new Vector2(0,1);report.rectTransform.anchorMax=Vector2.one;report.rectTransform.pivot=new Vector2(.5f,1);
        scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=report.rectTransform;scroll.horizontal=false;scroll.scrollSensitivity=32;scroll.movementType=ScrollRect.MovementType.Clamped;
        CampaignHUD.AddScrollbar(scroll,panel.transform,new Vector2(.94f,.535f),new Vector2(.953f,.845f));
        var works=CampaignHUD.Label(panel.transform,"SIEGE ENGINES & WORKS",new Vector2(.045f,.465f),new Vector2(.955f,.525f),24);works.color=ImperialTheme.Gold;
        string[] names={"Siege ramp","Siege tower","Battering ram","Arrow shields","Overland slipways","Chain anchorage damage","Moat protection remaining"};
        for(int i=0;i<names.Length;i++){
            float y=.423f-i*.047f;
            CampaignHUD.Label(panel.transform,names[i],new Vector2(.045f,y),new Vector2(.64f,y+.039f),21);
            statuses[i]=CampaignHUD.Label(panel.transform,"",new Vector2(.65f,y),new Vector2(.955f,y+.039f),21);statuses[i].alignment=TextAlignmentOptions.Right;
            var track=CampaignHUD.Panel(panel.transform,names[i]+" progress",new Vector2(.045f,y-.005f),new Vector2(.955f,y-.001f));track.GetComponent<Image>().color=ImperialTheme.Surface;
            var fill=CampaignHUD.Panel(track.transform,"Progress",Vector2.zero,Vector2.one);fills[i]=fill.GetComponent<Image>();fills[i].color=ImperialTheme.Gold;fills[i].raycastTarget=false;
        }
        proceed.transform.SetParent(panel.transform,false);var r=proceed.GetComponent<RectTransform>();r.anchorMin=new Vector2(.045f,.025f);r.anchorMax=new Vector2(.955f,.092f);r.offsetMin=r.offsetMax=Vector2.zero;
        proceed.gameObject.SetActive(true);ImperialTheme.Button(proceed);
    }
    public void Show(bool assault){
        panel.SetActive(true);ArtworkIndex=NextArtwork();painting.texture=Resources.Load<Texture2D>(ArtPaths[ArtworkIndex]);
        if(painting.texture!=null)painting.GetComponent<AspectRatioFitter>().aspectRatio=painting.texture.width/(float)painting.texture.height;
        var button=panel.GetComponentInChildren<Button>(true);var caption=button.GetComponentInChildren<TMP_Text>(true);caption.text=assault?"Prepare the defence":"Continue";
        Refresh();scroll.verticalNormalizedPosition=1;
    }
    public void Hide()=>panel.SetActive(false);
    public static float[] Progress()=>new[]{Mathf.Clamp01(VariableSingleton.GetFloatVariable("fSiegeRamp")),VariableSingleton.GetBoolVariable("bSiegeTower")?1f:0f,VariableSingleton.GetBoolVariable("bBatteringRam")?1f:0f,DailyLifeEvents.I("iOttArrowScreens")>0?1f:0f,Mathf.Clamp01(DailyLifeEvents.I("iOttHornWorks")/100f),Mathf.Clamp01(DailyLifeEvents.I("iOttChainDamage")/100f),Mathf.Clamp01(VariableSingleton.GetFloatVariable("fMoat"))};
    public void Refresh(){
        day.text="Siege day "+VariableSingleton.SiegeDay;
        report.text=AI.Instance.LastReport;
        Canvas.ForceUpdateCanvases();var viewport=scroll.viewport.rect;
        report.rectTransform.sizeDelta=new Vector2(0,Mathf.Max(viewport.height,report.GetPreferredValues(report.text,viewport.width,99999).y+12));report.rectTransform.anchoredPosition=Vector2.zero;
        var values=Progress();for(int i=0;i<values.Length;i++){
            statuses[i].text=i>=1&&i<=3?(values[i]>0?"Ready":"Not built"):Mathf.RoundToInt(values[i]*100)+"%";
            fills[i].rectTransform.anchorMax=new Vector2(values[i],1);
            statuses[i].color=values[i]>=1?ImperialTheme.Gold:ImperialTheme.Ivory;
        }
    }
}
