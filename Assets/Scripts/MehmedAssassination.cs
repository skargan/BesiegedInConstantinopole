using System;
using UnityEngine;

// Campaign rules only. The council owns confirmation and spends the major action.
// All markers live in the campaign dictionaries and therefore survive save/load.
public static class MehmedAssassination
{
    public const int RequiredAgents = 10, SuccessPercent = 20, RepeatDays = 30;
    const string NextDayKey = "iMehmedAssassinationNextDay";
    const string FailedKey = "iMehmedAssassinationFailed";
    const string VictoryKey = "iMehmedAssassinationVictory";

    static int Today => TrainingPool.Day(VariableSingleton.GetDate());
    public static bool Victory => DailyLifeEvents.I(VictoryKey) > 0;
    public static bool HasFailed => DailyLifeEvents.I(FailedKey) > 0;
    public static int CooldownDays => Mathf.Max(0, DailyLifeEvents.I(NextDayKey) - Today);
    public static DateTime ReadyOn => new DateTime((long)DailyLifeEvents.I(NextDayKey) * TimeSpan.TicksPerDay);
    public static string Unavailable {
        get {
            if(Victory || SaveGames.Terminal) return "This campaign has ended.";
            if(!VariableSingleton.HasSiegeStarted()) return "Available once the siege begins.";
            if(CouncilActions.Int("iSpies") < RequiredAgents) return "Requires 10 agents in the enemy camp.";
            if(CooldownDays > 0) return "Next attempt: " + ReadyOn.ToString("d MMM yyyy") + " (" + CooldownDays + " days).";
            return null;
        }
    }
    public static bool Available => Unavailable == null;
    public static string Status => Victory ? "Mehmed is dead. The siege is broken." : Unavailable ?? "An attempt can be prepared · 20% chance of success";

    // Reset before applying an archive: an older save must not inherit later consequences.
    public static void ResetForRestore()
    {
        VariableSingleton.SetInt(NextDayKey, 0);
        VariableSingleton.SetInt(FailedKey, 0);
        VariableSingleton.SetInt(VictoryKey, 0);
    }

    public static bool Succeeds(int roll) => roll >= 0 && roll < SuccessPercent;

    // True means a valid attempt was resolved, including failure. No gold or agents are spent.
    public static bool Attempt(out string report)
    {
        report = Unavailable;
        if(report != null) return false;
        VariableSingleton.SetInt(NextDayKey, Today + RepeatDays);
        if(Succeeds(UnityEngine.Random.Range(0, 100))) {
            VariableSingleton.SetInt(VictoryKey, 1);
            report = "Before dawn, the signals from the Ottoman camp cease. A trusted messenger brings the news: Mehmed is dead. Commanders abandon their siege works as rival orders pass between the tents. The bells of Constantinople answer one another across the City. The siege is broken.";
        } else {
            VariableSingleton.SetInt(FailedKey, 1);
            VariableSingleton.ChangePercentFloat("fSultanRage", .5f);
            VariableSingleton.SetInt("iCampAlert", 100);
            report = "The attempt fails. Mehmed survives, and the camp closes around him. Guards search the roads and scrutinise every visitor; even when the alarm subsides, this vigilance will remain."
                + "\n\nSultan's fury: " + VariableSingleton.GetPercentFloat("fSultanRage")
                + "\nEnemy camp alert: 100% · permanent minimum 50%"
                + "\nNext attempt: " + ReadyOn.ToString("d MMM yyyy") + ".";
        }
        return true;
    }
}
