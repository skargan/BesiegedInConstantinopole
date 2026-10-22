using UnityEngine;
using Yarn.Unity;

// Health remains the authoritative existing fHealth percentage. Skills use a separate 0..10 scale.
public static class EmperorProfile
{
    public static readonly string[] Names={"Charisma","Intellect","Martial skill"};
    public static bool NeedsOpening;
    public static int Value(int stat)=>Mathf.Clamp(DailyLifeEvents.I("iLifeEmperorStat"+stat,stat==2?Mathf.Max(3,Mathf.RoundToInt(DailyLifeEvents.I("iFencing",30)/10f)):3),0,10);
    public static int XP(int stat)=>DailyLifeEvents.I("iLifeEmperorXP"+stat);
    public static int NextLevel(int stat)=>100+Value(stat)*25;
    public static bool Valid(int[] values)=>values!=null&&values.Length==3&&values[0]>=3&&values[1]>=3&&values[2]>=3&&values[0]<=10&&values[1]<=10&&values[2]<=10&&values[0]+values[1]+values[2]==16;
    public static bool Allocate(int[] values)
    {
        if(!NeedsOpening||!Valid(values))return false;
        for(int i=0;i<3;i++)VariableSingleton.SetInt("iLifeEmperorStat"+i,values[i]);
        VariableSingleton.SetInt("iLifeEmperorCreated",1);NeedsOpening=false;return true;
    }
    public static void Loaded(bool fresh)
    {
        NeedsOpening=fresh;
        for(int i=0;i<3;i++)if(DailyLifeEvents.I("iLifeEmperorStat"+i,-1)<0)VariableSingleton.SetInt("iLifeEmperorStat"+i,fresh?3:Value(i));
        // Loading an older campaign preserves health and fencing, without rerolling its beginnings.
        if(fresh)VariableSingleton.ChangePercentFloat("fHealth",1f);
    }
    public static void GainXP(int stat,int amount)
    {
        if(stat<0||stat>2||amount<=0||Value(stat)>=10)return;
        int xp=XP(stat)+amount;
        while(Value(stat)<10&&xp>=NextLevel(stat)){xp-=NextLevel(stat);VariableSingleton.SetInt("iLifeEmperorStat"+stat,Value(stat)+1);}
        VariableSingleton.SetInt("iLifeEmperorXP"+stat,Value(stat)>=10?0:xp);
    }
    public static void FencingPractice()
    {
        GainXP(2,25);
    }
    public static int HealthRoll(int kind)=>kind==0?Random.Range(1,11):kind==1?Random.Range(20,41):kind==2?Random.Range(10,31):kind==3?Random.Range(5,76):Random.Range(5,21);
    [YarnCommand("EmperorHealthEvent")]public static void HealthEvent(int kind)
    {
        int amount=HealthRoll(kind);VariableSingleton.SetInt("iLifeEmperorLastHealth",amount);
        VariableSingleton.SetInt("iLifeCooldownEmperor"+kind,TrainingPool.Day(VariableSingleton.GetDate())+30);
        VariableSingleton.ChangePercentFloat("fHealth",(kind==4?amount:-amount)/100f);
    }
    public static System.Collections.Generic.IEnumerable<string> HealthEvents()
    {
        if(!VariableSingleton.HasSiegeStarted())yield break;
        int day=TrainingPool.Day(VariableSingleton.GetDate());
        for(int i=0;i<5;i++)if(day>=DailyLifeEvents.I("iLifeCooldownEmperor"+i)&&(i!=0||DailyLifeEvents.I("iLifeDrankTonight")==1))yield return "emperorHealth"+i;
    }
    public static string Epilogue()
    {
        var lines=new System.Collections.Generic.List<string>();
        foreach(var q in CourtConversationData.All.characters){int verdict=DailyLifeEvents.I("iLifeQuestChoice"+q.person+"_2");if(verdict>0)lines.Add(q.name+": "+(verdict==1?q.endingA:q.endingB));}
        return lines.Count==0?"":"\n\nCOURT MATTERS\n"+string.Join("\n",lines);
    }
}
