using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Public history only: never derive entries from hidden campaign-state differences.
public static class DecisionJournal
{
    [Serializable] public class Entry {public string id,date,title,text;public bool decision;}
    public static List<Entry> Entries=new List<Entry>();
    public static void Record(string id,string title,string text,bool decision=false){
        if(!VariableSingleton.IsInitialized)return;
        RecordAt(VariableSingleton.GetDate(),id,title,text,decision);
    }
    public static void RecordAt(DateTime date,string id,string title,string text,bool decision=false){
        if(!VariableSingleton.IsInitialized||Entries.Any(x=>x.id==id))return;
        Entries.Add(new Entry{id=id,date=date.ToString("d MMM yyyy"),title=title,text=text,decision=decision});
    }
    public static List<Entry> Copy()=>Entries.Select(e=>new Entry{id=e.id,date=e.date,title=e.title,text=e.text,decision=e.decision}).ToList();
    static GameObject root;
    public static void Show(bool decisions=false){
        Close();root=new GameObject("Decision Journal",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=280;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
        var card=CampaignHUD.Panel(root.transform,"Journal page",new Vector2(.08f,.06f),new Vector2(.92f,.94f));ImperialTheme.Panel(card,false);
        CampaignHUD.Label(card.transform,"Decision Journal",new Vector2(.05f,.87f),new Vector2(.95f,.97f),38);
        CampaignHUD.MakeButton(card.transform,"Daily notes",new Vector2(.05f,.77f),new Vector2(.48f,.86f),()=>Show(false));
        CampaignHUD.MakeButton(card.transform,"Decisions and affairs",new Vector2(.52f,.77f),new Vector2(.95f,.86f),()=>Show(true));
        var viewport=CampaignHUD.Panel(card.transform,"Journal entries",new Vector2(.05f,.15f),new Vector2(.93f,.74f));viewport.AddComponent<RectMask2D>();
        string body=string.Join("\n\n",Entries.Where(e=>e.decision==decisions).Reverse().Select(e=>e.date+" · "+e.title+"\n"+e.text));
        if(decisions)body="THE GOLDEN HORN\n"+(VariableSingleton.GetBoolVariable("bGoldenHornChain")?"The harbour chain is in place.":"The harbour chain is not in place.")+"\n\n"+body;
        if(string.IsNullOrEmpty(body))body="No entries have yet been recorded.";
        var text=CampaignHUD.Label(viewport.transform,body,Vector2.zero,Vector2.one,24);
        var r=text.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);
        Canvas.ForceUpdateCanvases();r.sizeDelta=new Vector2(0,Mathf.Max(((RectTransform)viewport.transform).rect.height,text.GetPreferredValues(body,((RectTransform)viewport.transform).rect.width,99999).y+30));
        var scroll=viewport.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=(RectTransform)viewport.transform;scroll.content=r;scroll.scrollSensitivity=35;
        CampaignHUD.AddScrollbar(scroll,card.transform,new Vector2(.935f,.15f),new Vector2(.95f,.74f));
        CampaignHUD.MakeButton(card.transform,"Return",new Vector2(.32f,.025f),new Vector2(.68f,.12f),()=>Close());
    }
    public static bool Close(){if(root==null)return false;UnityEngine.Object.Destroy(root);root=null;return true;}
}
