using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
public static class SiegeHardships
{
    static int I(string key)=>DailyLifeEvents.I("iHard"+key);
    static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    public static readonly string[] Roots={"hardStorm","hardCatastrophicStorm","hardCistern","hardHorse","hardRats","hardDead","hardGrave","hardItch","hardBrothel","hardWoman","hardFavourite","hardChild","hardCoin","hardSand","hardHead","hardScreams","hardMissing","sermonPride"};
    public static IEnumerable<string> Available(){if(!VariableSingleton.HasSiegeStarted())yield break;foreach(var n in Roots){if(Day<DailyLifeEvents.I("iLifeCooldown"+n))continue;if(n=="hardGrave"&&I("GraveStage")!=0||n=="hardSand"&&I("SandStage")!=0||n=="hardWoman"&&I("WomanStage")!=0||n=="hardMissing"&&(I("MissingStage")!=0||DailyLifeEvents.I("iInfantry")<12)||n=="sermonPride"&&I("SermonStage")!=0)continue;yield return n;}}
    public static string DueNode {get{string best=null;int earliest=int.MaxValue;foreach(var chain in new[]{"Grave","Sand","Woman","Missing","Sermon"}){int stage=I(chain+"Stage"),due=I(chain+"Due");if(stage<=0||stage==99||due<=0||due>Day||due>=earliest)continue;string node=chain=="Sermon"?new[]{"","sermonPride","sermonGreed","sermonLust","sermonEnvy","sermonGluttony","sermonWrath","sermonSloth"}[Mathf.Clamp(stage,1,7)]:"hard"+chain+stage;best=node;earliest=due;}return best;}}
    [YarnCommand("HardNext")]public static void Next(string chain,int stage,int days){VariableSingleton.SetInt("iHard"+chain+"Stage",stage);VariableSingleton.SetInt("iHard"+chain+"Due",stage==99?0:Day+Mathf.Max(1,days));}
    [YarnCommand("SermonPressure")]public static void Pressure(int delta)=>VariableSingleton.SetInt("iHardPressure",Mathf.Clamp(I("Pressure")+delta,0,100));
    [YarnFunction("SermonPressure")]public static int PressureValue()=>I("Pressure");
    [YarnCommand("SermonClose")]public static void Close(){Next("Sermon",99,0);if(I("Pressure")>=50){VariableSingleton.ChangePercentFloat("fPeopleRep",-.08f);VariableSingleton.ChangePercentFloat("fPriestRep",-.05f);}else if(I("Pressure")<=15)VariableSingleton.ChangePercentFloat("fPeopleRep",.03f);}
    [YarnCommand("HardSpy")]public static void Recruit(){if(I("WomanSpy")!=0||DailyLifeEvents.I("iMoney")<1000)return;VariableSingleton.ChangeInt("iMoney",-1000);VariableSingleton.ChangeInt("iSpies",1);VariableSingleton.SetInt("iHardWomanSpy",1);}
}
