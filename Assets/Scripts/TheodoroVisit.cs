using System;
using System.Linq;

// One campaign flag; opening dialogue never spends money or consumes randomness.
public static class TheodoroVisit
{
    public const string AccessKey="iTheodoroPalaceAccess";
    public const int Gift=3000;
    public static bool Admitted=>DailyLifeEvents.I(AccessKey)>0;
    public static bool Correct(string answer)
    {
        string word=new string((answer??"").Trim().ToLowerInvariant().Where(char.IsLetter).ToArray());
        return word=="silence" || word=="thesilence";
    }
    public static bool Enter(bool pay,string answer,out string reply)
    {
        if(!VariableSingleton.IsInitialized || SaveGames.Terminal || AssaultPractice.Active){reply="The audience is unavailable.";return false;}
        if(Admitted){reply="The guards stand aside.";return true;}
        if(pay && DailyLifeEvents.I("iMoney")<Gift){reply="You need 3,000 gold.";return false;}
        if(!pay && !Correct(answer)){reply="The captain shakes his head. ‘Listen to the question once more.’";return false;}
        VariableSingleton.SetInt(AccessKey,pay?2:1);
        if(pay)VariableSingleton.ChangeInt("iMoney",-Gift);
        reply=pay?"The captain accepts the gift and orders the gate opened.":"‘Silence. Yes.’ He steps aside. ‘You will find plenty of it in the prince's rooms.’";
        DecisionJournal.Record("theodoro-palace-entry","An audience at Mangup",pay?"Paid 3,000 gold to secure admission to Maria Asanina's palace.":"Answered the gate captain's riddle and entered Maria Asanina's palace.",true);
        SaveGames.Checkpoint();return true;
    }
}
