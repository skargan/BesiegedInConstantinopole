using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

// A transcript, never a saved executable dialogue. Stored inside the existing journal payload.
public static class EventDecisionHistory
{
    public const string Prefix="event-history-";
    [Serializable] public class Choice {public string text;public bool chosen;}
    [Serializable] public class Stage {public string narrative;public List<Choice> choices=new List<Choice>();}
    [Serializable] public class Record {public int format=1;public string node,title,parent,ending;public List<Stage> stages=new List<Stage>();}
    public sealed class Recorder
    {
        readonly Record record;string prose="";Stage pending;string finishedId;
        public Recorder(string node,string parent=null){record=new Record{node=node,parent=parent};}
        public void Line(string text){prose+=(prose.Length>0?"\n\n":"")+text;}
        public void Options(DialogueOption[] options){
            pending=new Stage{narrative=prose};prose="";
            foreach(var option in options)pending.choices.Add(new Choice{text=option.Line.Text.Text});
            record.stages.Add(pending);
        }
        public void Choose(string text){
            if(pending==null){pending=new Stage{narrative=prose};prose="";pending.choices.Add(new Choice{text=text});record.stages.Add(pending);}
            bool found=false;foreach(var choice in pending.choices){choice.chosen=!found&&choice.text==text;found|=choice.chosen;}
            if(!found)pending.choices.Add(new Choice{text=text,chosen=true});pending=null;
        }
        public string Finish(string title,string publicOutcome){
            if(finishedId!=null)return finishedId;
            record.title=title;record.ending=prose+(string.IsNullOrEmpty(publicOutcome)?"":"\n\n"+publicOutcome);
            if(record.stages.Count==0&&string.IsNullOrWhiteSpace(record.ending))return null;
            string id=Prefix+VariableSingleton.Turn+"-"+record.node+"-"+DecisionJournal.Entries.Count;
            DecisionJournal.Record(id,title,JsonUtility.ToJson(record),true);finishedId=id;return id;
        }
    }
    public static Record Read(DecisionJournal.Entry entry){
        if(entry==null||entry.id==null||!entry.id.StartsWith(Prefix))return null;
        try{
            var record=JsonUtility.FromJson<Record>(entry.text);
            if(record==null||record.format!=1||string.IsNullOrWhiteSpace(record.node)||record.stages==null||record.stages.Any(s=>s==null||s.choices==null||s.choices.Any(c=>c==null)))return null;
            return record;
        }catch(ArgumentException){return null;}
    }
    public static string ChoiceSummary(Record record)=>string.Join(" → ",record.stages.SelectMany(s=>s.choices??new List<Choice>()).Where(c=>c.chosen).Select(c=>Plain(c.text)));
    public static string Plain(string value)=>System.Text.RegularExpressions.Regex.Replace(value??"","<[^>]+>","");
    public static DecisionJournal.Entry Followup(DecisionJournal.Entry entry)=>DecisionJournal.Entries.FirstOrDefault(e=>Read(e)?.parent==entry.id);
    public static string LatestId(string node)=>DecisionJournal.Entries.LastOrDefault(e=>Read(e)?.node==node)?.id;
}
