using System;
using UnityEngine;

public static class SurrenderTerms
{
    public static readonly string[] Groups={"Inhabitants","Military","Emperor","Imperial household","Nobility","Religious worship","Territories"};
    public static readonly string[] Fates={"Spared under Ottoman rule","Exile","Enslavement","Execution","Vassalage"};
    public static bool HasOffer=>CouncilActions.Int("iTermsDay")>0;
    public static bool Valid=>HasOffer && CouncilActions.Int("iTermsDay")==TrainingPool.Day(VariableSingleton.GetDate()) && CouncilActions.Int("iSurrenderEnding")==0;
    public static int Fate(int group)=>CouncilActions.Int("iTermsFate"+group);
    public static bool Severe(int group)=>Fate(group)==2 || Fate(group)==3;
    public static int AssassinationRisk=>(Severe(0)?10:0)+(Severe(1)?50:0)+(Severe(3)?20:0);
    public static void Request()
    {
        if(Valid)return;
        int fury=Mathf.RoundToInt(VariableSingleton.GetFloatVariable("fSultanRage")*100),alert=EspionageSystem.Alert;
        int walls=Mathf.RoundToInt(VariableSingleton.Instance.EnduringWall.GetHealth()*100);
        int pressure=Mathf.RoundToInt(fury*.4f+alert*.2f+(100-walls)*.4f);
        int[] fates;
        if(pressure<25) fates=new[]{0,4,4,1,4,0,0};
        else if(pressure<50) fates=new[]{fury>=50?1:0,1,1,1,1,1,1};
        else if(pressure<75) fates=alert>=60?new[]{0,3,2,1,1,1,1}:new[]{0,4,3,2,1,1,1};
        else fates=new[]{fury>=95 && walls<=10?3:2,3,3,alert>=90?3:2,3,2,2};
        for(int i=0;i<fates.Length;i++) VariableSingleton.SetInt("iTermsFate"+i,fates[i]);
        VariableSingleton.SetInt("iTermsDay",TrainingPool.Day(VariableSingleton.GetDate()));
        VariableSingleton.SetInt("iTermsFury",fury);VariableSingleton.SetInt("iTermsAlert",alert);VariableSingleton.SetInt("iTermsWalls",walls);
        VariableSingleton.SetInt("iTermsPressure",pressure);
    }
    public static string Detail(int group)
    {
        int fate=Fate(group);
        if(group==5) return fate==0?"Christian worship continues under Ottoman authority; the City pays tribute.":fate==1?"Worship continues in designated churches. Hagia Sophia and selected foundations pass to the Sultan.":"The principal churches are seized. Public Christian worship is suspended until the Sultan grants permission.";
        if(group==6) return fate==0?"The City and its dependencies pass under Ottoman sovereignty. Local administration survives by the Sultan's appointment.":fate==1?"The City's territories, forts and revenues pass directly to Ottoman governors. Imperial claims are renounced.":"The imperial domains are confiscated and divided among the Sultan's servants. The dynasty retains no territorial rights.";
        if(fate==4) return group==2?"You keep a subordinate title, swear obedience, pay tribute and supply men when summoned.":group==1?"The troops swear service to the Sultan and may be sent to fight in his wars.":"Nobles retain selected estates in return for tribute and service to the Sultan.";
        if(fate==0) return "Lives and homes are spared. The inhabitants become Ottoman subjects and owe taxes to their new ruler.";
        if(fate==1) return group==1?"The soldiers surrender their weapons and leave under escort; their units are dissolved.":"Safe passage into exile, with personal possessions. Estates and offices in the City are forfeited.";
        if(fate==2) return "Freedom and property are forfeited. Those surrendered become captives at the Sultan's disposal.";
        return "Those covered by this term are put to death after the gates are opened.";
    }
    public static string Heading(int group)=>group<5?Fates[Fate(group)]:group==5?"Worship and churches":"Sovereignty and land";
    public static bool Accept(out string report)
    {
        report="The offer has expired. Request new terms.";if(!Valid)return false;
        int risk=AssassinationRisk;
        bool assassinated=risk>0 && UnityEngine.Random.Range(0,100)<risk;
        VariableSingleton.SetInt("iSurrenderEnding",assassinated?2:1);
        VariableSingleton.SetInt("iSurrenderDays",VariableSingleton.CompletedSiegeDays);
        if(assassinated) Achievements.Unlock(1); else Achievements.Unlock(2);
        VariableSingleton.Instance.Save();report=EndingText;return true;
    }
    public static string EndingText {
        get {
            if(CouncilActions.Int("iSurrenderEnding")==2) return "Before your envoys can deliver acceptance, men of your own City strike you down. The surrender is repudiated. Another council takes command, and the siege continues without you.\n\nYour reign ends in assassination.";
            string result="The seals are exchanged. Ottoman officers enter the gates, and Constantinople passes into the Sultan's hands.\n";
            for(int i=0;i<Groups.Length;i++) result+="\n"+Groups[i]+": "+Heading(i)+". "+Detail(i)+"\n";
            return result;
        }
    }
}
