using System;
using System.Linq;
using UnityEngine;

// Uses the existing saved diplomacy store and resident-envoy calendar.
public static class IonianDiplomacy
{
    const string Prefix="iDiplomacyIonian";
    public static int Country=>CountryDiplomacy.Index("ionian_tocco");
    public static bool IsCountry(int index)=>index>=0&&index==Country;
    public static bool Resident=>CountryDiplomacy.EnvoyState(Country)==CountryDiplomacy.EnvoyAssignment.Resident;
    public static bool Shelter=>DailyLifeEvents.I(Prefix+"Shelter")>0;
    public static bool Exile=>DailyLifeEvents.I(Prefix+"Exile")>0;
    public static bool Introduced=>DailyLifeEvents.I(Prefix+"Introduced")>0;
    // Marriage is not yet playable in this checkout. The saved flag is the explicit
    // integration contract for future marriage content; courtship here never sets it.
    public static bool Married=>DailyLifeEvents.I("iEmperorMarried")>0;
    public static readonly string[] SupportKeys={"fTroopRep","fPeopleRep","fPriestRep","fNobleRep"};
    public static string Standing=>Shelter&&Exile?"Refugee shelter and personal exile agreed":Exile?"Personal exile agreed":Shelter?"Refugee shelter agreed":"Shelter and exile negotiations";
    public static void Introduce()
    {
        if(!Resident||Introduced)return;
        VariableSingleton.SetInt(Prefix+"Introduced",1);
        DecisionJournal.Record("ionian-introduction","Letters from the Ionian Isles","Your resident envoy has secured an introduction to Raimondina Ventimiglia. She offers hospitality, not troops.");
    }
    public static string AgreementReason(bool personal)
    {
        if(personal?Exile:Shelter)return "This agreement is already secured";
        return Resident?null:"Requires a resident envoy in the Ionian Isles";
    }
    public static bool Agree(bool personal,out string report)
    {
        report=AgreementReason(personal);if(report!=null)return false;
        Introduce();
        VariableSingleton.SetInt(Prefix+(personal?"Exile":"Shelter"),1);
        VariableSingleton.SetInt(Prefix+(personal?"ExileDay":"ShelterDay"),CountryDiplomacy.Day);
        foreach(string key in SupportKeys)VariableSingleton.ChangeFloat(key,personal?-.10f:-.03f);
        report=personal
            ? "Raimondina's answer is brief: a house will be kept ready, and your name will be enough at the harbour. In the palace, the news is received less kindly. Men who have pledged their lives to the walls dislike learning that their emperor has found another shore.\n\nPersonal refuge in the Ionian Isles secured. Army, Public, Clergy and Noble support: −10 percentage points each. You remain in Constantinople; no departure has been ordered."
            : "The island stewards receive instructions to find lodgings for fugitives from Constantinople. Raimondina asks that families carry whatever papers they possess: a name is easier to lose than a bundle. At home, even this modest precaution is taken for an admission that the walls may fail.\n\nRefugee shelter in the Ionian Isles secured. Army, Public, Clergy and Noble support: −3 percentage points each. Passage through the blockade is not guaranteed.";
        DecisionJournal.Record(personal?"ionian-exile":"ionian-shelter",personal?"A house beyond the siege":"Shelter for the City's refugees",report,true);
        return true;
    }
    public static string EndingReflection(bool victory)
    {
        if(!Shelter&&!Exile)return "";
        string text="\n\nTHE IONIAN AGREEMENTS\n";
        if(victory)return text+"The City survives. "+(Shelter?"The island lodgings reserved for its refugees are not called upon. ":"")+(Exile?"Raimondina's house remains an invitation you have no need to accept.":"");
        if(Shelter)text+="Raimondina's promise of shelter remains for those who can reach the islands. The agreement provided a destination, not safe passage through the siege. ";
        if(Exile)text+=DailyLifeEvents.I("iSurrenderEnding")==1&&SurrenderTerms.Fate(2)==1
            ?"Your negotiated exile now has a destination: the Ionian Isles, where Raimondina has promised you a household."
            :"A household was reserved for you in the Ionian Isles. The promise did not secure your release or carry you out of Constantinople.";
        return text;
    }
}

public static class IonianText
{
    [Serializable] public class Remark {public int id;public string text;public bool unmarriedOnly,beforeShelter;}
    [Serializable] public class Content {public string realm,about,siege,shelter,exile;public Remark[] remarks;}
    static Content content;
    public static Content Data=>content??(content=JsonUtility.FromJson<Content>(Resources.Load<TextAsset>("Content/IonianCourt").text));
    public static string[] Lines=>Data.remarks.Select(r=>r.text).ToArray();
    public static bool Eligible(Remark r)=>!(r.unmarriedOnly&&IonianDiplomacy.Married)&&!(r.beforeShelter&&IonianDiplomacy.Shelter);
    public static string NextRemark()=>CharacterChatter.Request(CharacterChatter.Raimondina,Lines,CharacterChatter.RaimondinaFarewell);
}
