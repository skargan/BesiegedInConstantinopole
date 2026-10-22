using UnityEngine;
using Yarn.Unity;
public static class EmperorCombat
{
    public static int Skill=>EmperorProfile.Value(2)*10;
    public static int WallKills=>DailyLifeEvents.I("iEmperorAtWall")==1&&VariableSingleton.GetFloatVariable("fHealth")>0?1+Skill/10:0;
    public static void ConsumeWallPresence(){if(DailyLifeEvents.I("iEmperorAtWall")==1){VariableSingleton.ChangePercentFloat("fHealth",-.05f);VariableSingleton.SetInt("iEmperorAtWall",0);}}
    public static bool Train(out string report){report="Sword practice requires more than 10% health and Martial skill below 10.";if(EmperorProfile.Value(2)>=10||VariableSingleton.GetFloatVariable("fHealth")<=.10f)return false;EmperorProfile.FencingPractice();VariableSingleton.ChangePercentFloat("fHealth",-.05f);report="You practise footwork, distance and recovery with the guard. Martial skill +25 XP; health −5 percentage points. Major action spent. Current Martial skill: "+EmperorProfile.Value(2)+" / 10; "+EmperorProfile.XP(2)+" / "+EmperorProfile.NextLevel(2)+" XP toward the next level.";return true;}

    public static string ToggleWall(){bool lead=DailyLifeEvents.I("iEmperorAtWall")==0;VariableSingleton.SetInt("iEmperorAtWall",lead?1:0);return lead?"You plan to join the next wall assault. Confirm or change your position in its command briefing. Personal combat uses Martial skill; the briefing explains health risks.":"You will direct the defence from safety.";}
    [YarnFunction("MartialSkill")]public static int MartialSkill()=>EmperorProfile.Value(2);
    [YarnFunction("Fencing")]public static int Fencing()=>Skill;
    [YarnCommand("FightAssassin")]public static void Fight(int style){int damage;bool win;if(style==0){win=Random.Range(0,100)<35+Skill/2;damage=win?Mathf.Max(2,Random.Range(5,16)-Skill/20):Mathf.Max(10,Random.Range(25,46)-Skill/10);}else{win=true;damage=Random.Range(10,26);}VariableSingleton.SetInt("iCombatAssassinWon",win?1:0);VariableSingleton.SetInt("iCombatLastDamage",damage);VariableSingleton.ChangePercentFloat("fHealth",-damage/100f);if(VariableSingleton.GetFloatVariable("fHealth")<=0)VariableSingleton.SetInt("iCombatAssassinationFatal",1);}
    [YarnFunction("AssassinDamage")]public static int Damage()=>DailyLifeEvents.I("iCombatLastDamage");
}
