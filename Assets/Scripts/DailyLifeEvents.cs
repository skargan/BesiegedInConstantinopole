using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
public static class DailyLifeEvents
{
    public static int I(string key,int fallback=0){try{return VariableSingleton.GetIntVariable(key);}catch(KeyNotFoundException){return fallback;}}
    static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    public static readonly string[] Roots={"lifeSails","lifeBread","lifeWell","lifeGhost","lifeBarrel","lifeCobbler","lifeDrawing","lifeCandles","lifeCoin","lifeWedding","lifeOnions","lifeQuiet","lifeInsult","lifeStones","lifeSaints","lifeRoof","lifeSong","lifeGenerosity","lifeGunner","lifeLights"};
    public static IEnumerable<string> Available(){if(!VariableSingleton.HasSiegeStarted())yield break;foreach(var n in Roots)if(Day>=I("iLifeCooldown"+n)&&(n!="lifeSails"||I("iLifeSailsDue")==0)&&(n!="lifeBarrel"||I("iLifeBarrelDue")==0))yield return n;}
    [YarnCommand("LifeSeen")] public static void Seen(string node)=>VariableSingleton.SetInt("iLifeCooldown"+node,Day+30);
    public static string DueNode {get {if(I("iLifeSailsDue")>0&&Day>=I("iLifeSailsDue"))return new[]{"lifeFalseHope","lifeMorea","lifeMercenaries","lifeRefugees"}[I("iLifeSailsResult")];if(I("iLifeBarrelDue")>0&&Day>=I("iLifeBarrelDue"))return "lifeBarrelOwner";if(I("iLifeMentorDue")>0&&Day>=I("iLifeMentorDue"))return "lifeGunnerResult";return null;}}
    public static int SailsOutcome(int roll)=>roll<15?1:roll<30?2:roll<45?3:0;
    [YarnCommand("LifeSails")] public static void Sails(int announced){VariableSingleton.SetInt("iLifeAnnounced",announced);VariableSingleton.SetInt("iLifeSailsResult",SailsOutcome(Random.Range(0,100)));VariableSingleton.SetInt("iLifeSailsDue",Day+1);if(announced==1)VariableSingleton.ChangePercentFloat("fPeopleRep",.05f);}
    [YarnFunction("LifeInt")] public static int Read(string key)=>I(key);
    [YarnCommand("LifeBarrel")] public static void Barrel(int use){VariableSingleton.SetInt("iLifeBarrelUse",use);VariableSingleton.SetInt("iLifeBarrelDue",Day+3);}
    [YarnCommand("MerchantFavor")] public static void Merchant(int delta)=>VariableSingleton.SetInt("iMerchantFavor",Mathf.Clamp(I("iMerchantFavor",50)+delta,0,100));
    [YarnCommand("LifePoison")] public static void Poison(){VariableSingleton.SetInt("iLifePoisonEnding",1);VariableSingleton.ChangePercentFloat("fHealth",-1f);}
    [YarnFunction("LifeCanSurprise")] public static bool CanSurprise()=>I("iEnemyDelay")==0&&I("iBazouks")>0;
    [YarnCommand("LifeSurprise")] public static void Surprise()=>VariableSingleton.SetInt("iLifeSurprise",1);
    [YarnCommand("LifeMentor")] public static void Mentor(){if(I("iLifeMentorDue")>0||I("iEngineers")<1||I("iInfantry")<1)return;VariableSingleton.ChangeInt("iEngineers",-1);VariableSingleton.ChangeInt("iInfantry",-1);VariableSingleton.SetInt("iLifeMentorDue",Day+5);VariableSingleton.SetInt("iLifeMentorSuccess",Random.Range(0,100)<80?1:0);}
    [YarnCommand("LifeRepair")] public static void Repair()=>VariableSingleton.ChangePercentFloat("f"+VariableSingleton.Instance.EnduringWall.GetCodeName(),.01f);
}
public static class PopulationAccounting
{
    public static long People=>DailyLifeEvents.I("iPopulation");
    [YarnCommand("AddResidents")] public static void Add(int people)=>VariableSingleton.SetInt("iPopulation",(int)System.Math.Min(int.MaxValue,System.Math.Max(0,People+people)));
    public static int RoundedFood(long people)=>(int)((System.Math.Max(0,people)+500)/1000);
    public static int Food(bool advance)=>RoundedFood(People);
    // Preserve the existing three-gold-per-thousand tax rate and fractional carry.
    public static int Tax(){long amount=People*3+DailyLifeEvents.I("iPopulationTaxCarry");VariableSingleton.SetInt("iPopulationTaxCarry",(int)(amount%1000));return (int)(amount/1000);}
    public static long Count(VariableSingleton.CampaignSave s){long population=s.ints.FirstOrDefault(v=>v!=null&&v.key=="iPopulation")?.value??0;return s.version<2?population*1000+(s.ints.FirstOrDefault(v=>v!=null&&v.key=="iPopulationRemainder")?.value??0):population;}
    public static string Report(VariableSingleton.CampaignSave before,VariableSingleton.CampaignSave after){long a=Count(before),b=Count(after);int ma=before.ints.FirstOrDefault(v=>v!=null&&v.key=="iMerchantFavor")?.value??50,mb=after.ints.FirstOrDefault(v=>v!=null&&v.key=="iMerchantFavor")?.value??50;return(a==b?"":"City population: "+(b>a?"+":"")+(b-a).ToString("N0")+" → now "+b.ToString("N0")+"\n")+(ma==mb?"":"Merchant favour: "+(mb>ma?"+":"")+(mb-ma)+" → now "+mb+"\n");}
}
