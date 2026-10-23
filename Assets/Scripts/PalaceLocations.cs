using System.Collections.Generic;
using UnityEngine;

public static class PalaceLocations
{
    private static int Token=>VariableSingleton.Turn+1;
    public static bool VisitedSanctuary=>DailyLifeEvents.I("iLifeSanctuaryVisit")==Token;
    public static string VisitSanctuary()
    {
        if(VisitedSanctuary)return "You have already found a moment of peace here this turn.";
        VariableSingleton.SetInt("iLifeSanctuaryVisit",Token);
        float before=VariableSingleton.GetFloatVariable("fHealth");
        VariableSingleton.ChangePercentFloat("fHealth",.01f);
        float gain=(VariableSingleton.GetFloatVariable("fHealth")-before)*100;
        return gain>.001f?"A moment beneath the great dome restores you. Health +"+gain.ToString("0.#")+" percentage point.":"You find a moment of peace beneath the great dome. Your health is already full.";
    }
    private static void Assign()
    {
        if(DailyLifeEvents.I("iLifeLocationTurn")==Token)return;
        var candidates=new List<int>();
        foreach(int person in CharacterQuestions.People)if(GardenConversations.Available(person))candidates.Add(person);
        // The priest belongs naturally in the cathedral. Other visitors rotate between the two secular spaces.
        int church=candidates.Contains(7)?7:-1;if(church>=0)candidates.Remove(church);
        VariableSingleton.SetInt("iLifeLocationVisitor2",church);
        for(int place=0;place<2;place++){
            int person=candidates.Count==0?-1:candidates[Random.Range(0,candidates.Count)];
            VariableSingleton.SetInt("iLifeLocationVisitor"+place,person);candidates.Remove(person);
        }
        VariableSingleton.SetInt("iLifeLocationTurn",Token);
    }
    public static int Visitor(int place)
    {
        Assign();int person=DailyLifeEvents.I("iLifeLocationVisitor"+place,-1);
        return person>=0&&GardenConversations.Available(person)?person:-1;
    }
}
