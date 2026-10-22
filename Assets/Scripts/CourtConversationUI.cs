using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CourtConversationUI:MonoBehaviour
{
    sealed class Option {public string text;public bool fresh,enabled;public Action action;public int stat=-1,required;}
    int person;Action closed;Action<int,int> lesson;GameObject panel;string passage;string page="root";int stage;bool closing;
    public static void Show(int person,Action closed,Action<int,int> lesson=null)
    {
        var root=new GameObject("Court conversation",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(CourtConversationUI));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=140;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var view=root.GetComponent<CourtConversationUI>();view.person=person;view.closed=closed;view.lesson=lesson;view.passage=GardenConversations.Personal(person)+CourtConversationData.Recent(person);CourtConversationData.Mark(person,"personal");view.Render();
        ControlSingleton.Instance.ClearHistory();ControlSingleton.Instance.LogToEsc(root,root);ControlSingleton.Instance.ReturnAction=view.Close;
    }
    void Close(){if(closing)return;closing=true;ControlSingleton.Instance.ClearHistory();Destroy(gameObject);closed?.Invoke();}
    Option O(string title,bool fresh,Action action,bool enabled=true,int stat=-1,int required=0)=>new Option{text=title,fresh=fresh,action=action,enabled=enabled,stat=stat,required=required};
    void Topic(string key,string text){CourtConversationData.Mark(person,key);passage=text;Render();}
    void Render()
    {
        if(!GardenConversations.Available(person)){Close();return;}
        if(panel!=null){panel.SetActive(false);Destroy(panel);}
        panel=CampaignHUD.Panel(transform,"Conversation",new Vector2(.015f,.025f),new Vector2(.985f,.87f));ImperialTheme.Panel(panel);
        var name=CampaignHUD.Label(panel.transform,GardenConversations.Name(person),new Vector2(.03f,.88f),new Vector2(.97f,.98f),34);name.alignment=TextAlignmentOptions.Center;
        CharacterPortraits.Draw(panel.transform,person,new Vector2(.025f,.25f),new Vector2(.285f,.85f));
        var role=CampaignHUD.Label(panel.transform,GardenConversations.Role(person),new Vector2(.025f,.14f),new Vector2(.285f,.24f),22);role.alignment=TextAlignmentOptions.Center;role.enableAutoSizing=true;role.fontSizeMin=16;
        var options=Options().OrderBy(o=>!o.enabled?2:o.fresh?0:1).ToArray();
        ScrollText(panel.transform,passage,new Vector2(.32f,.51f),new Vector2(.96f,.86f));
        var viewport=CampaignHUD.Panel(panel.transform,"Conversation options",new Vector2(.32f,.105f),new Vector2(.955f,.485f));viewport.GetComponent<Image>().color=Color.clear;viewport.AddComponent<RectMask2D>();
        var content=CampaignHUD.Panel(viewport.transform,"Topics",Vector2.zero,Vector2.one).GetComponent<RectTransform>();content.GetComponent<Image>().color=Color.clear;
        content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,Mathf.Max(250,options.Length*70));content.anchoredPosition=Vector2.zero;
        for(int i=0;i<options.Length;i++){var o=options[i];var button=CampaignHUD.MakeButton(content,o.text,Vector2.zero,Vector2.one,()=>o.action());button.interactable=o.enabled;var r=button.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,62);r.anchoredPosition=new Vector2(0,-i*70);var t=button.GetComponentInChildren<TMP_Text>();t.enableAutoSizing=true;t.fontSizeMin=16;t.fontSizeMax=23;t.color=!o.enabled?new Color(.5f,.5f,.52f):o.fresh?new Color(1,.84f,.47f):new Color(.62f,.67f,.70f);if(o.stat>=0){t.rectTransform.anchorMin=new Vector2(.10f,.04f);EmperorAttributeBadge.Draw(button.transform,o.stat,new Vector2(.02f,.25f),new Vector2(.085f,.91f),o.enabled?0:o.required,o.enabled);}else if(o.enabled&&o.fresh)EmperorAttributeBadge.DrawNew(button.transform);}
        var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=content;scroll.horizontal=false;scroll.scrollSensitivity=30;CampaignHUD.AddScrollbar(scroll,panel.transform,new Vector2(.96f,.105f),new Vector2(.973f,.485f));
        CampaignHUD.MakeButton(panel.transform,"Leave the conversation",new Vector2(.34f,.025f),new Vector2(.70f,.085f),Close);
    }
    static void ScrollText(Transform parent,string value,Vector2 min,Vector2 max)
    {
        var viewport=CampaignHUD.Panel(parent,"Speech",min,max);viewport.GetComponent<Image>().color=Color.clear;viewport.AddComponent<RectMask2D>();
        var t=CampaignHUD.Explanation(viewport.transform,value,Vector2.zero,Vector2.one,25);t.alignment=TextAlignmentOptions.TopLeft;t.margin=new Vector4(5,5,20,5);
        Canvas.ForceUpdateCanvases();var r=t.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,Mathf.Max(viewport.GetComponent<RectTransform>().rect.height,t.GetPreferredValues(value,viewport.GetComponent<RectTransform>().rect.width-25,9999).y+20));r.anchoredPosition=Vector2.zero;
        var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=r;scroll.horizontal=false;scroll.scrollSensitivity=30;CampaignHUD.AddScrollbar(scroll,parent,new Vector2(max.x,min.y),new Vector2(max.x+.012f,max.y));
    }
    List<Option> Options()
    {
        var list=new List<Option>();var q=CourtConversationData.For(person);
        if(page=="root"){
            list.Add(O("Tell me about yourself",CourtConversationData.NewBiography(person),()=>{page="bio";passage=GardenConversations.Personal(person);Render();}));
            list.Add(O("What do you make of the siege?",!CourtConversationData.Read(person,"siege"),()=>Topic("siege",GardenConversations.Siege(person))));
            list.Add(O("What should I attend to?",!CourtConversationData.Read(person,"advice"),()=>Topic("advice",GardenConversations.Advice(person))));
            list.Add(O("How is your part of the defence holding?",!CourtConversationData.Read(person,"context"),()=>Topic("context",SiegeCharacterContext.Text(person))));
            list.Add(O("Was there something you wished to ask me?",DailyLifeEvents.I("iLifeQuestionReply"+person)==0,()=>{page="question";passage=CharacterQuestions.For(person).Question;Render();}));
            if(q!=null)list.Add(O(q.title,CourtConversationData.NewQuest(person),()=>{page="quest";passage="There is a matter "+q.name+" has not brought before the full council.";Render();},true,q.stat));
            if(person==5){
                list.Add(O("How many cannons does a city really need?",!CourtConversationData.Read(5,"cannontheory"),()=>Topic("cannontheory",SiegeCharacterContext.CannonTheory),true,1));
                if(CourtConversationData.Read(5,"cannontheory")){bool ready=SiegeCharacterContext.CannonsSatisfied||CourtConversationData.Read(5,"cannonanswer");list.Add(O("Finally, do you think we have enough cannons now?"+(ready?"":" · Orban is not yet satisfied"),ready&&!CourtConversationData.Read(5,"cannonanswer"),()=>{if(!CourtConversationData.Read(5,"cannonanswer"))EmperorProfile.GainXP(1,15);Topic("cannonanswer",SiegeCharacterContext.CannonAnswer);VariableSingleton.Instance.Save();},ready,1));}
            }
            if(q!=null&&lesson!=null)list.Add(O("Will you instruct me? · "+EmperorProfile.Names[q.stat]+" +25 XP · major action",false,()=>{Destroy(gameObject);closed?.Invoke();lesson(q.stat,person);},EmperorProfile.Value(q.stat)<10));
            list.Add(O("Our court journal",false,()=>{page="journal";passage=CourtConversationData.Journal();Render();}));
        }else if(page=="bio"){
            list.Add(O("Tell me about yourself",!CourtConversationData.Read(person,"personal"),()=>Topic("personal",GardenConversations.Personal(person))));
            list.Add(O("How did you come to Constantinople?",!CourtConversationData.Read(person,"arrival"),()=>Topic("arrival",CourtCharacterWriting.Arrival(person))));
            bool unlocked=EmperorProfile.Value(0)>=4;
            list.Add(O("What does this City mean to you?",unlocked&&!CourtConversationData.Read(person,"city"),()=>Topic("city",CourtCharacterWriting.City(person)),unlocked,0,4));
            if(q!=null)list.Add(O("Whose judgement do you trust?",!CourtConversationData.Read(person,"bonds"),()=>Topic("bonds",CourtCharacterWriting.Bonds(person))));
        }else if(page=="question"){
            var exchange=CharacterQuestions.For(person);int chosen=DailyLifeEvents.I("iLifeQuestionReply"+person)-1;
            if(chosen<0)for(int i=0;i<exchange.Replies.Length;i++){int answer=i;list.Add(O(exchange.Replies[i],true,()=>{if(DailyLifeEvents.I("iLifeQuestionReply"+person)==0){VariableSingleton.SetInt("iLifeQuestionReply"+person,answer+1);EmperorProfile.GainXP(0,15);VariableSingleton.Instance.Save();}passage=exchange.Question+"\n\n"+exchange.Responses[answer];Render();}));}
            else passage=exchange.Question+"\n\n<b>You replied</b>\n"+exchange.Replies[chosen]+"\n\n"+exchange.Responses[chosen];
        }else if(page=="quest"){
            for(int i=0;i<q.stages.Length;i++){int k=i;string locked=CourtConversationData.Locked(q,i);list.Add(O(q.stages[i].title+(locked==null?"":" · "+locked),locked==null&&CourtConversationData.Choice(person,i)==0,()=>{stage=k;page="stage";passage=StagePassage(q,k);Render();},locked==null,q.stat,q.stages[i].minStat));}
        }else if(page=="stage"){
            int chosen=CourtConversationData.Choice(person,stage);var s=q.stages[stage];
            if(chosen==0){list.Add(O(s.choiceA,true,()=>Decide(q,1),true,q.stat));list.Add(O(s.choiceB,true,()=>Decide(q,2),true,q.stat));}
            list.Add(O("Return to this matter",false,()=>{page="quest";Render();}));
        }
        if(page!="root")list.Add(O("Return to our conversation",false,()=>{page="root";Render();}));
        return list;
    }
    string StagePassage(CourtConversationData.Character q,int s){int c=CourtConversationData.Choice(person,s);return q.stages[s].text+(c==0?"":"\n\n<b>Your decision</b>\n"+(c==1?q.stages[s].choiceA:q.stages[s].choiceB)+"\n\n"+(c==1?q.stages[s].responseA:q.stages[s].responseB));}
    void Decide(CourtConversationData.Character q,int answer){if(CourtConversationData.Decide(q,stage,answer)){passage=StagePassage(q,stage);Render();}}
}
