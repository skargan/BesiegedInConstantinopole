using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
}
