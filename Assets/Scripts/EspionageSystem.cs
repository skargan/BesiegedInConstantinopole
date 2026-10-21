using UnityEngine;

public static class EspionageSystem
{
    public static readonly int[] AlertGain = {3,15,30,0,25,20};
    public static readonly string[] Descriptions = {
        "A cook, a discontented captain, a beggar at the camp gates: each new contact sees something others miss. More agents reveal more of the enemy's plans.",
        "Ottoman bombards threaten the very stones of our walls. Send agents among the batteries to damage guns and put them out of action.",
        "An army marches on its stores. Burn the grain and fodder to force fresh supply runs; hunger and interrupted pay can drive soldiers to desert.",
        "Ottoman informants move among the people of Constantinople. Trace their contacts and uncover their hiding places before they can betray the City's defences.",
        "Bribe a clerk, copy a letter, listen beyond a tent flap. Gather the knowledge and contacts that can open doors for the Empire.",
        "Mislead the camp clerks and interrupt deliveries to the siege works. A day spent untangling false orders gives our defenders time to breathe."
    };
    public static readonly string[] IntelligenceLabels = {
        "Enemy soldiers", "Ottoman artillery and fleet", "Enemy food stores", "Food consumption",
        "Sultan's fury", "Enemy spies in Constantinople", "Enemy morale", "Likely tactics"
    };
    public static int Alert => Mathf.Clamp(CouncilActions.Int("iCampAlert"),0,100);
    public static float LossChance(int mission) => mission==3 ? 5f : 5f+Alert*.5f;
    public static string IntelligenceValue(int level)
    {
        if(CouncilActions.Int("iSpies")<level+1) return "Unknown";
        switch(level) {
            case 0: return (CouncilActions.Int("iBalkans")+CouncilActions.Int("iBazouks")+CouncilActions.Int("iJanissaries")).ToString("N0");
            case 1: return CouncilActions.Int("iTurkCannons")+" cannons · "+NavalState.EnemyShips+" vessels";
            case 2: return CouncilActions.Int("iFood")+" days";
            case 3: return "1 day of stores / siege day";
            case 4: return VariableSingleton.GetPercentFloat("fSultanRage");
            case 5: return CouncilActions.Int("iTurkSpies").ToString();
            case 6: return CouncilActions.Int("iMorale").ToString();
            default: return VariableSingleton.GetFloatVariable("fMoat")>0?"Preparations at the moat":VariableSingleton.Instance.EnduringWall.GetHealth()>.5f?"Artillery and probing attacks":"Assaults increasingly likely";
        }
    }
    public static bool FinishOperation(int mission, float risk)
    {
        VariableSingleton.SetInt("iCampAlert",Mathf.Min(100,Alert+AlertGain[mission]));
        bool lost=Random.value*100f<risk && CouncilActions.Int("iSpies")>0;
        if(lost) VariableSingleton.ChangeInt("iSpies",-1);
        return lost;
    }
    public static void AdvanceTurn()
    {
        if(Alert>0) VariableSingleton.SetInt("iCampAlert",Mathf.Max(0,Alert-Random.Range(1,11)));
    }
}
