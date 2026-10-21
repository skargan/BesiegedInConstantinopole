using System;
using System.Collections.Generic;
using UnityEngine;

public static class TroopTraining
{
    public static readonly string[] Units={"Infantry","Archers","Cavalry"};
    public static readonly string[] Ranks={"Untrained","Drilled","Trained","Veteran","Elite"};
    public static float[] Tiers(int unit)=>unit==0?new[]{.1f,.25f,.5f,.75f,1f}:new[]{.25f,.5f,1f,1.5f,3f};
    public static float Base(int unit)=>VariableSingleton.GetFloatVariable("f"+Units[unit]+"Quality");
    public static int Rank(int unit) {int rank=0;var tiers=Tiers(unit);for(int i=0;i<tiers.Length;i++) if(Base(unit)>=tiers[i]-.0001f) rank=i;return rank;}
    public static float Next(int unit) {foreach(float tier in Tiers(unit)) if(tier>Base(unit)+.0001f) return tier;return Base(unit);}
    public static int Due(int unit)=>CouncilActions.Int("iDrillDue"+unit);
    public static string Unavailable(int unit)
    {
        if(Due(unit)>0) return "Training underway";
        if(Rank(unit)==4) return "Elite training attained";
        if(CouncilActions.Int("i"+Units[unit])==0) return "Requires troops";
        if(CouncilActions.Int("iMoney")<5000) return "Requires 5,000 gold";
        return null;
    }
    public static bool Order(int unit,out string report)
    {
        report=Unavailable(unit);if(report!=null) return false;
        VariableSingleton.ChangeInt("iMoney",-5000);
        VariableSingleton.SetInt("iDrillDue"+unit,TrainingPool.Day(VariableSingleton.GetDate())+5);
        VariableSingleton.SetInt("iDrillTarget"+unit,Mathf.RoundToInt(Next(unit)*1000));
        report=Units[unit]+" begin five days of drills. They remain available for battle while their officers work with them.\n\nGold spent: 5,000. Training completes on "+VariableSingleton.GetDate().AddDays(5).ToString("d MMM yyyy")+".";
        return true;
    }
    public static string Complete(DateTime date)
    {
        string report="";
        for(int i=0;i<Units.Length;i++) if(Due(i)>0 && Due(i)<=TrainingPool.Day(date)) {
            float target=CouncilActions.Int("iDrillTarget"+i)/1000f;
            VariableSingleton.ChangeFloat("f"+Units[i]+"Quality",Mathf.Max(0,target-Base(i)));
            VariableSingleton.SetInt("iDrillDue"+i,0);VariableSingleton.SetInt("iDrillTarget"+i,0);
            report+="\n"+Units[i]+" complete their drills: "+Ranks[Rank(i)]+" (training lethality ×"+Base(i).ToString("0.##")+").";
        }
        return report;
    }
    public static string Pending()
    {
        string result="";
        for(int i=0;i<3;i++) if(Due(i)>0) result+=Units[i]+" drills · "+Mathf.Max(0,Due(i)-TrainingPool.Day(VariableSingleton.GetDate()))+" days remaining\n";
        return result;
    }
    public static float Lethality(int unit)=>TechnologySystem.Quality(Units[unit])+(unit==0&&TechnologySystem.Has("Grenades")?.25f:unit==1&&TechnologySystem.Has("Handcannons")?.5f:0);
    public static float WallCavalry=>Lethality(2)*(TechnologySystem.Has("DismountedChampions")?1f:.5f);
    public static string Breakdown(int unit)
    {
        float training=Base(unit),technology=Lethality(unit)-training;
        string text="Training ×"+training.ToString("0.##")+" + technology ×"+technology.ToString("0.##")+" = lethality ×"+Lethality(unit).ToString("0.##");
        if(unit==2) text+="\nAt the walls: ×"+WallCavalry.ToString("0.##")+(TechnologySystem.Has("DismountedChampions")?" · Dismounted Champions":" · half lethality when dismounted");
        if(unit==0) text+="\nInfantry advisers: +"+(CourtRoster.Bonus(0,.05f,.15f)*100).ToString("0")+"% in melee";
        if(unit==1) text+="\nAdmiral: +"+((CouncilActions.Int("iFleet")==3&&CouncilActions.Int("iShips")>0?CourtRoster.Bonus(1,.05f,.15f):0)*100).ToString("0")+"% while the fleet defends";
        return text;
    }
}
