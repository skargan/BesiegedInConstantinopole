using System;
using System.Linq;
using UnityEngine;

public static class CourtConversationData
{
    [Serializable]public class Catalogue {public int schema;public string status;public Character[] characters;}
    [Serializable]public class Character {public int person,stat,likes,hates,link;public string name,title,endingA,endingB;public Stage[] stages;}
    [Serializable]public class Stage {public string id,title,text,choiceA,choiceB,responseA,responseB;public int minStat,delayDays;}
    static Catalogue cache;
    public static Catalogue All=>cache??(cache=JsonUtility.FromJson<Catalogue>(Resources.Load<TextAsset>("Court/court-conversations").text));
    public static Character For(int person)=>All.characters.FirstOrDefault(c=>c.person==person);
    public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    static string Key(int p,int s)=>"iLifeQuestChoice"+p+"_"+s;
    public static int Choice(int p,int stage)=>DailyLifeEvents.I(Key(p,stage));
    public static bool CanRead(Character q,int stage)=>Locked(q,stage)==null;
    public static string Locked(Character q,int stage)
    {
        if(stage<0||stage>=q.stages.Length)return "Unavailable";
        if(Choice(q.person,stage)>0)return null; // A read passage remains readable after a stat loss.
        if(stage>0&&Choice(q.person,stage-1)==0)return "Continue the earlier conversation first";
        if(stage>0&&Day<DailyLifeEvents.I("iLifeQuestDay"+q.person+"_"+(stage-1))+q.stages[stage].delayDays)return "Return after "+q.stages[stage].delayDays+" days";
        int need=q.stages[stage].minStat;
        bool linked=stage>0&&Choice(q.link,1)>0;
        if(linked)need=Mathf.Min(5,need);
        return EmperorProfile.Value(q.stat)>=need?null:EmperorProfile.Names[q.stat]+" "+need+" required"+(stage==2?" (or 5 after "+GardenConversations.Name(q.link)+"’s disclosure)":"");
    }
    public static bool Decide(Character q,int stage,int answer)
    {
        if(answer<1||answer>2||!GardenConversations.Available(q.person)||!CanRead(q,stage)||Choice(q.person,stage)>0)return false;
        VariableSingleton.SetInt(Key(q.person,stage),answer);VariableSingleton.SetInt("iLifeQuestDay"+q.person+"_"+stage,Day);
        EmperorProfile.GainXP(q.stat,stage==0?15:stage==1?30:60);
        VariableSingleton.Instance.Save();return true;
    }
    static string ReadKey(int p,string topic)=>"iLifeConversationRead"+p+"_"+topic;
    public static int Token(int person,string topic)
    {
        string text=topic=="siege"?GardenConversations.Siege(person):topic=="advice"?GardenConversations.Advice(person):topic=="context"?SiegeCharacterContext.Text(person):null;
        if(text==null)return 1;
        // Stable across saves and launches; .NET's process-dependent string hash is unsuitable here.
        uint hash=2166136261;unchecked{foreach(char c in text)hash=(hash^c)*16777619;}
        return (int)(hash&0x3fffffff)+1;
    }
    public static bool Read(int p,string topic)=>DailyLifeEvents.I(ReadKey(p,topic))==Token(p,topic);
    public static void Mark(int p,string topic){int token=Token(p,topic);if(DailyLifeEvents.I(ReadKey(p,topic))==token)return;VariableSingleton.SetInt(ReadKey(p,topic),token);VariableSingleton.Instance.Save();}
    public static bool NewQuest(int p){var q=For(p);return q!=null&&Enumerable.Range(0,q.stages.Length).Any(s=>Choice(p,s)==0&&CanRead(q,s));}
    public static bool NewBiography(int p)=>!Read(p,"personal")||!Read(p,"arrival")||!Read(p,"bonds")||EmperorProfile.Value(0)>=4&&!Read(p,"city");
    public static bool SomethingNew(int p)=>NewBiography(p)||!Read(p,"siege")||!Read(p,"advice")||!Read(p,"context")||DailyLifeEvents.I("iLifeQuestionReply"+p)==0||NewQuest(p)||p==5&&SiegeCharacterContext.NewCannonConversation;
    public static string Journal()
    {
        var records=new System.Collections.Generic.List<string>();
        foreach(var q in All.characters)for(int i=0;i<q.stages.Length;i++){int c=Choice(q.person,i);if(c==0)continue;int day=DailyLifeEvents.I("iLifeQuestDay"+q.person+"_"+i);records.Add(new DateTime((long)day*TimeSpan.TicksPerDay).ToString("d MMM yyyy")+" · "+q.name+" · "+q.stages[i].title+"\nYou decided: "+(c==1?q.stages[i].choiceA:q.stages[i].choiceB));}
        return records.Count==0?"No court decisions recorded yet.":string.Join("\n\n",records);
    }
    public static string Recent(int person)
    {
        var q=For(person);if(q==null)return "";
        for(int s=q.stages.Length-1;s>=0;s--)if(Choice(person,s)>0&&Day-DailyLifeEvents.I("iLifeQuestDay"+person+"_"+s)<=14)return "\n\n<b>A recent matter between you</b>\n"+(Choice(person,s)==1?q.stages[s].responseA:q.stages[s].responseB);
        return "";
    }
}
