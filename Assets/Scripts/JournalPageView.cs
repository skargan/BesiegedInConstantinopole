using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// All reader callbacks navigate recorded data. No Yarn runner or gameplay callback is reachable here.
public static class JournalPageView
{
    static Transform card;
    static GameObject reader;
    public static void Draw(Transform page,DecisionJournal.Tab tab){
        card=page;reader=null;
        var viewport=CampaignHUD.Panel(page,"Journal entries",new Vector2(.045f,.15f),new Vector2(.93f,.75f));
        viewport.AddComponent<RectMask2D>();
        var content=CampaignHUD.Panel(viewport.transform,"Chronological entries",Vector2.zero,Vector2.one);content.GetComponent<Image>().color=Color.clear;
        var rect=(RectTransform)content.transform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);
        var entries=DecisionJournal.Entries.Where(e=>e.decision==(tab==DecisionJournal.Tab.Decisions))
            .Where(e=>{var record=EventDecisionHistory.Read(e);return string.IsNullOrEmpty(record?.parent)||!DecisionJournal.Entries.Any(p=>p.id==record.parent);}).Reverse().ToArray();
        float width=((RectTransform)page).rect.width*.885f;float used=8;
        string lastDate=null;
        if(entries.Length==0)AddText(content.transform,"No entries have yet been recorded.",ref used,width,24);
        foreach(var entry in entries){
            if(lastDate!=entry.date){AddText(content.transform,PalaceAdministration.Period(entry.date),ref used,width,25,true);lastDate=entry.date;}
            var record=EventDecisionHistory.Read(entry);
            if(record!=null){
                var button=CampaignHUD.MakeButton(content.transform,entry.title+"\n"+EventDecisionHistory.ChoiceSummary(record),Vector2.zero,Vector2.one,()=>Open(entry,0,false));
                var caption=button.GetComponentInChildren<TMP_Text>();caption.fontSize=24;caption.enableAutoSizing=false;caption.alignment=TextAlignmentOptions.MidlineLeft;caption.margin=new Vector4(15,8,15,8);
                float height=Mathf.Max(85,caption.GetPreferredValues(caption.text,width-36,99999).y+26);Place((RectTransform)button.transform,used,height);used+=height+12;
            }else{
                AddText(content.transform,"<b>"+entry.title+"</b>\n"+entry.text,ref used,width,24);
            }
        }
        Canvas.ForceUpdateCanvases();rect.sizeDelta=new Vector2(0,Mathf.Max(used,((RectTransform)viewport.transform).rect.height));rect.anchoredPosition=Vector2.zero;
        var scroll=viewport.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=(RectTransform)viewport.transform;scroll.content=rect;scroll.scrollSensitivity=40;
        CampaignHUD.AddScrollbar(scroll,page,new Vector2(.938f,.15f),new Vector2(.952f,.75f));
    }
    static void Place(RectTransform rect,float top,float height){rect.anchorMin=new Vector2(0,1);rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,1);rect.offsetMin=new Vector2(8,-top-height);rect.offsetMax=new Vector2(-8,-top);}
    static void AddText(Transform parent,string body,ref float used,float width,int size,bool heading=false){
        var text=CampaignHUD.Label(parent,body,Vector2.zero,Vector2.one,size);text.enableAutoSizing=false;
        if(heading)text.color=new Color(.69f,.82f,.86f);
        float height=text.GetPreferredValues(body,Mathf.Max(200,width-20),99999).y+18;Place(text.rectTransform,used,height);used+=height+10;
    }
    public static bool CloseReader(){if(reader==null)return false;reader.SetActive(false);UnityEngine.Object.Destroy(reader);reader=null;return true;}
    public static void Open(DecisionJournal.Entry entry,int stage,bool outcome){
        CloseReader();if(card==null)return;
        reader=CampaignHUD.Panel(card,"Recorded event reader",new Vector2(.025f,.015f),new Vector2(.975f,.985f));ImperialTheme.Panel(reader,false);
        var title=CampaignHUD.Label(reader.transform,entry.title,new Vector2(.04f,.88f),new Vector2(.96f,.98f),32);title.alignment=TextAlignmentOptions.Center;
        CampaignHUD.Label(reader.transform,entry.date,new Vector2(.04f,.82f),new Vector2(.96f,.88f),21).alignment=TextAlignmentOptions.Center;
        var record=EventDecisionHistory.Read(entry);
        if(record==null)return;
        bool final=outcome||stage>=record.stages.Count;
        string prose=final?record.ending:record.stages[stage].narrative;
        var viewport=CampaignHUD.Panel(reader.transform,"Recorded narrative",new Vector2(.035f,.19f),new Vector2(final?.95f:.48f,.81f));viewport.AddComponent<RectMask2D>();
        var text=CampaignHUD.Label(viewport.transform,prose,Vector2.zero,Vector2.one,24);text.margin=new Vector4(12,10,22,10);var r=text.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);
        Canvas.ForceUpdateCanvases();r.sizeDelta=new Vector2(0,Mathf.Max(((RectTransform)viewport.transform).rect.height,text.GetPreferredValues(prose,((RectTransform)viewport.transform).rect.width-40,99999).y+28));r.anchoredPosition=Vector2.zero;
        var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=(RectTransform)viewport.transform;scroll.content=r;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=40;
        CampaignHUD.AddScrollbar(scroll,reader.transform,new Vector2(final?.95f:.48f,.19f),new Vector2(final?.962f:.491f,.81f));
        if(!final){
            var choices=CampaignHUD.Panel(reader.transform,"Recorded choices",new Vector2(.51f,.19f),new Vector2(.965f,.81f));choices.AddComponent<RectMask2D>();
            var content=CampaignHUD.Panel(choices.transform,"Choice transcript",Vector2.zero,Vector2.one);content.GetComponent<Image>().color=Color.clear;
            var cr=(RectTransform)content.transform;cr.anchorMin=new Vector2(0,1);cr.anchorMax=Vector2.one;cr.pivot=new Vector2(.5f,1);float y=8,width=((RectTransform)choices.transform).rect.width;
            foreach(var choice in record.stages[stage].choices){
                var b=CampaignHUD.MakeButton(content.transform,choice.text,Vector2.zero,Vector2.one,()=>Open(entry,stage+1,stage+1>=record.stages.Count));b.name=choice.chosen?"Recorded chosen decision":"Recorded unchosen decision";b.interactable=choice.chosen;
                var label=b.GetComponentInChildren<TMP_Text>();label.enableAutoSizing=false;label.fontSize=23;label.alignment=TextAlignmentOptions.MidlineLeft;label.margin=new Vector4(14,8,14,8);if(!choice.chosen)label.color=new Color(.48f,.49f,.47f);
                float h=Mathf.Max(76,label.GetPreferredValues(choice.text,width-40,99999).y+24);Place((RectTransform)b.transform,y,h);y+=h+12;
            }
            cr.sizeDelta=new Vector2(0,Mathf.Max(y,((RectTransform)choices.transform).rect.height));cr.anchoredPosition=Vector2.zero;
            var choiceScroll=choices.AddComponent<ScrollRect>();choiceScroll.viewport=(RectTransform)choices.transform;choiceScroll.content=cr;choiceScroll.horizontal=false;choiceScroll.movementType=ScrollRect.MovementType.Clamped;CampaignHUD.AddScrollbar(choiceScroll,reader.transform,new Vector2(.967f,.19f),new Vector2(.98f,.81f));
        }else{
            // Outcome takes the full reading width. A linked follow-up can only continue the actual path.
            ((RectTransform)viewport.transform).anchorMax=new Vector2(.95f,.81f);Canvas.ForceUpdateCanvases();r.sizeDelta=new Vector2(0,Mathf.Max(((RectTransform)viewport.transform).rect.height,text.GetPreferredValues(prose,((RectTransform)viewport.transform).rect.width-40,99999).y+28));
            var followup=EventDecisionHistory.Followup(entry);if(followup!=null)CampaignHUD.MakeButton(reader.transform,"Continue recorded decision",new Vector2(.52f,.04f),new Vector2(.95f,.14f),()=>Open(followup,0,false));
        }
        CampaignHUD.MakeButton(reader.transform,"Back to decisions",new Vector2(.055f,.04f),new Vector2(.46f,.14f),()=>CloseReader());
    }
}
