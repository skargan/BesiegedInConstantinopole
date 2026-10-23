using UnityEngine;
using Yarn.Unity;
public static class EmperorCombat
{
    public static int Skill=>Mathf.Clamp(DailyLifeEvents.I("iFencing",25),0,100);
    public static int WallKills=>DailyLifeEvents.I("iEmperorAtWall")==1&&VariableSingleton.GetFloatVariable("fHealth")>0?1+Skill/10:0;
    public static void ConsumeWallPresence(){if(DailyLifeEvents.I("iEmperorAtWall")==1){VariableSingleton.ChangePercentFloat("fHealth",-.05f);VariableSingleton.SetInt("iEmperorAtWall",0);}}
    public static bool Train(out string report){report="Training requires more than 10 health and fencing below100.";if(Skill>=100||VariableSingleton.GetFloatVariable("fHealth")<=.10f)return false;int old=Skill;VariableSingleton.SetInt("iFencing",Mathf.Min(100,old+5));VariableSingleton.ChangePercentFloat("fHealth",-.05f);report="You drill with the guard: footwork, distance, recovery. Fencing +"+(Skill-old)+" → "+Skill+"; health -5. Major action spent.";return true;}
    public static string ToggleWall(){bool lead=DailyLifeEvents.I("iEmperorAtWall")==0;VariableSingleton.SetInt("iEmperorAtWall",lead?1:0);return lead?"You will join the next wall assault. Personal kills scale with fencing; fighting costs5health. You may cancel before the assault.":"You will direct the defence from safety.";}
    [YarnFunction("Fencing")]public static int Fencing()=>Skill;
    [YarnCommand("FightAssassin")]public static void Fight(int style){int damage;bool win;if(style==0){win=Random.Range(0,100)<35+Skill/2;damage=win?Mathf.Max(2,Random.Range(5,16)-Skill/20):Mathf.Max(10,Random.Range(25,46)-Skill/10);}else{win=true;damage=Random.Range(10,26);}VariableSingleton.SetInt("iCombatAssassinWon",win?1:0);VariableSingleton.SetInt("iCombatLastDamage",damage);VariableSingleton.ChangePercentFloat("fHealth",-damage/100f);if(VariableSingleton.GetFloatVariable("fHealth")<=0)VariableSingleton.SetInt("iCombatAssassinationFatal",1);}
    [YarnFunction("AssassinDamage")]public static int Damage()=>DailyLifeEvents.I("iCombatLastDamage");
}
