using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    static Transform statusParent;
    public enum Tab {DailyNotes,Decisions,StrategicStatus}
    public static void Show(bool decisions=false)=>Show(decisions?Tab.Decisions:Tab.DailyNotes);
    public static void Show(Tab tab){
        Close();root=new GameObject("Decision Journal",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=280;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
        var card=CampaignHUD.Panel(root.transform,"Journal page",new Vector2(.08f,.06f),new Vector2(.92f,.94f));ImperialTheme.Panel(card,false);
        CampaignHUD.Label(card.transform,"Decision Journal",new Vector2(.05f,.87f),new Vector2(.95f,.97f),38);
        string[] titles={"Daily Notes","Decisions","Strategic Status"};
        for(int i=0;i<3;i++){int index=i;var button=CampaignHUD.MakeButton(card.transform,titles[i],new Vector2(.05f+i*.305f,.77f),new Vector2(.34f+i*.305f,.86f),()=>Show((Tab)index));button.name="Journal tab "+titles[i];button.interactable=i!=(int)tab;}
        var viewport=CampaignHUD.Panel(card.transform,"Journal entries",new Vector2(.05f,.15f),new Vector2(.93f,.74f));viewport.AddComponent<RectMask2D>();
        string body=string.Join("\n\n",Entries.Where(e=>e.decision==(tab==Tab.Decisions)).Reverse().Select(e=>e.date+" · "+e.title+"\n"+e.text));
        if(string.IsNullOrEmpty(body))body="No entries have yet been recorded.";
        var text=CampaignHUD.Label(viewport.transform,body,Vector2.zero,Vector2.one,24);
        var r=text.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);
        Canvas.ForceUpdateCanvases();r.sizeDelta=new Vector2(0,Mathf.Max(((RectTransform)viewport.transform).rect.height,text.GetPreferredValues(body,((RectTransform)viewport.transform).rect.width,99999).y+30));
        var scroll=viewport.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=(RectTransform)viewport.transform;scroll.content=r;scroll.scrollSensitivity=35;
        if(tab!=Tab.StrategicStatus)CampaignHUD.AddScrollbar(scroll,card.transform,new Vector2(.935f,.15f),new Vector2(.95f,.74f));
        if(tab==Tab.StrategicStatus){UnityEngine.Object.Destroy(text.gameObject);scroll.content=null;statusParent=viewport.transform;RefreshStatus();root.AddComponent<JournalStatusRefresh>();}
        CampaignHUD.MakeButton(card.transform,"Return",new Vector2(.32f,.025f),new Vector2(.68f,.12f),()=>Close());
    }
    public static void RefreshStatus(){
        if(statusParent==null)return;
        foreach(Transform child in statusParent){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}
        var rows=JournalStrategicStatus.Read();float height=Mathf.Min(.14f,.96f/rows.Count);
        for(int i=0;i<rows.Count;i++){
            var row=rows[i];float top=1-i*height;var panel=CampaignHUD.Panel(statusParent,"Strategic indicator "+row.Id,new Vector2(0,top-height+.012f),new Vector2(1,top));ImperialTheme.Panel(panel,false);
            var label=CampaignHUD.Label(panel.transform,row.Name,new Vector2(.025f,.08f),new Vector2(.63f,.92f),25);label.alignment=TextAlignmentOptions.MidlineLeft;
            var value=CampaignHUD.Label(panel.transform,row.Status,new Vector2(.64f,.08f),new Vector2(.975f,.92f),25);value.alignment=TextAlignmentOptions.MidlineRight;value.color=new Color(.72f,.84f,.89f);value.name="Strategic status "+row.Id;
        }
    }
    public static bool Close(){statusParent=null;if(root==null)return false;root.SetActive(false);UnityEngine.Object.Destroy(root);root=null;return true;}
}
