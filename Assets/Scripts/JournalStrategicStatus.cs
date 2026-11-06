using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A read-only projection of existing campaign state, never a second simulation.
public static class JournalStrategicStatus
{
    public sealed class Indicator { public string Id,Name,Status; public Indicator(string id,string name,string status){Id=id;Name=name;Status=status;} }
    public static string UnionPosition {
        get {
            int policy=DailyLifeEvents.I("iReligion");
            if(policy==1)return "Implemented";
            if(policy==2)return "Symbolic";
            bool considered=DecisionJournal.Entries.Any(e=>e.id=="union-of-florence")||VariableSingleton.HasSiegeStarted()||VariableSingleton.Turn>11;
            return considered?"Rejected":"Undecided";
        }
    }
    public static List<Indicator> Read(){
        int galata=PreparationCampaign.GalataState;
        var rows=new List<Indicator>{
            new Indicator("bosphorus","Bosphorus Passage",VariableSingleton.GetBoolVariable("bStraitsOpen")?"Open":"Closed"),
            new Indicator("chain","Golden Horn Chain",VariableSingleton.GetBoolVariable("bGoldenHornChain")?"Present":"Absent"),
            new Indicator("galata","Galata Alignment",galata==3?"Pro-Ottoman":galata==1||galata==2?"Pro-Emperor":"Neutral"),
            new Indicator("union","Union of Florence",UnionPosition),
            new Indicator("sea","Sea Dominance",NavalState.Dominance==NavalState.Control.Imperial?"Imperial Fleet":NavalState.Dominance==NavalState.Control.Ottoman?"Ottoman Fleet":"Contested")
        };
        bool battalion=DailyLifeEvents.I("iTurkishBattalionCreated")>0;
        if(battalion||DecisionJournal.Entries.Any(e=>e.id=="turkish_battalion_created"))rows.Add(new Indicator("battalion","Turkish Mercenary Battalion",battalion?"Active":"Inactive"));
        if(IonianDiplomacy.Shelter)rows.Add(new Indicator("shelter","Ionian Refugee Shelter","Secured"));
        if(IonianDiplomacy.Exile)rows.Add(new Indicator("refuge","Personal Refuge","Secured"));
        return rows;
    }
}

public sealed class JournalStatusRefresh:MonoBehaviour
{
    string previous;
    void Update(){string current=string.Join("|",JournalStrategicStatus.Read().Select(r=>r.Id+":"+r.Status));if(current==previous)return;previous=current;DecisionJournal.RefreshStatus();}
}
