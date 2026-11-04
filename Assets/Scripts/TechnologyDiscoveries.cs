using UnityEngine;

// Persistent discoveries use the existing campaign dictionary and journal/save pipeline.
// The unfinished experiment belongs to the screen, not to the save file.
public static class TechnologyDiscoveries
{
    public enum Kind { Letter, Alchemy, Skull, Treasure }
    public static string Key(Kind kind)=>"iDiscovery"+kind;
    public static bool Found(Kind kind)=>DailyLifeEvents.I(Key(kind))==1;
    public static bool Claim(Kind kind)
    {
        if(!VariableSingleton.IsInitialized || SaveGames.Terminal || AssaultPractice.Active || Found(kind))return false;
        VariableSingleton.SetInt(Key(kind),1);
        if(kind!=Kind.Skull)VariableSingleton.SetInt("iSecrets",DailyLifeEvents.I("iSecrets")+1);
        if(kind==Kind.Treasure)VariableSingleton.SetInt("iMoney",DailyLifeEvents.I("iMoney")+2500);
        string title=kind==Kind.Skull?"Dragon skull discovered":kind==Kind.Alchemy?"An alchemical discovery":kind==Kind.Letter?"The pigeon's letter":"A concealed treasury";
        string note=kind==Kind.Skull?"A great fossil skull rests among the scholars' curiosities. Someone has carved the Columns of Gediminas upon its brow.":kind==Kind.Alchemy?"The workshop's experiment yields a useful discovery. One secret gained.":kind==Kind.Letter?"An overlooked message brings useful intelligence. One secret gained.":"A concealed catch yields beneath your thumb. Inside lie coins wrapped in faded silk and a folded page of calculations. Whoever hid them here never returned. Gained 2,500 gold and one secret.";
        DecisionJournal.Record("discovery_"+kind.ToString().ToLowerInvariant(),title,note,true);
        if(kind==Kind.Alchemy)Achievements.Unlock(18);
        SaveGames.Checkpoint();
        return true;
    }
    public sealed class Experiment
    {
        public int Step {get;private set;}
        float lastInteraction;
        public void Reset(){Step=0;lastInteraction=0;}
        public void Tick(float now){if(Step>0 && now-lastInteraction>=60)Reset();}
        public bool Advance(int step,float now){Tick(now);if(step!=Step || Step>=6)return false;Step++;lastInteraction=now;return true;}
    }
}
