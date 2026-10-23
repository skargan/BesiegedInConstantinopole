using System;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

public static class PoliticalDemands
{
    public static int Active=>DailyLifeEvents.I("iPolDemand")-1;
    public static string PendingNode=>Active>=0&&!PoliticalSystem.Banned(Active)?"politicalDemand":null;
    public static int Mature(int bloc)=>Enumerable.Range(DailyLifeEvents.I("iPolSettled"+bloc)+1,Math.Max(0,DailyLifeEvents.I("iPolIssued"+bloc)-DailyLifeEvents.I("iPolSettled"+bloc))).Count(n=>PoliticalSystem.Day-DailyLifeEvents.I("iPolDebtDay"+bloc+"_"+n)>=30);
    public static void Schedule()
    {
        if(Active>=0)return;
        if(PoliticalSystem.Day-DailyLifeEvents.I("iPolDemandDay")<7)return;
        var eligible=Enumerable.Range(0,4).Where(i=>!PoliticalSystem.Banned(i)&&DailyLifeEvents.I("iPolBlocked"+i)<=PoliticalSystem.Day&&Mature(i)>0).ToArray();
        int total=eligible.Sum(Mature);
        // One roll per council turn; each mature obligation adds ten percentage points, capped at fifty.
        if(total==0||UnityEngine.Random.Range(0,100)>=Math.Min(50,total*10))return;
        int pick=UnityEngine.Random.Range(0,total);
        foreach(int bloc in eligible){pick-=Mature(bloc);if(pick<0){VariableSingleton.SetInt("iPolDemand",bloc+1);VariableSingleton.SetInt("iPolDemandDay",PoliticalSystem.Day);return;}}
    }
    static int Bloc=>Mathf.Clamp(Active,0,3);
    static readonly int[] Payments={500,500,1000,3500};
    static readonly string[] Concessions={"Reserve honours for loyal officers. Noble support −5 points.","Restrict Latin public ceremonies. Public support −5 points.","Guarantee Latin worship. Clergy support −5 points.","Publicly endorse negotiations with the Sultan. Army morale −5 points."};
    static readonly string[] Keys={"fNobleRep","fPeopleRep","fPriestRep","fTroopRep"};
    [YarnFunction("PoliticalCreditor")]public static string Creditor()=>PoliticalSystem.Names[Bloc];
    [YarnFunction("PoliticalPayment")]public static float Payment()=>Payments[Bloc];
    [YarnFunction("PoliticalCanPay")]public static bool CanPay()=>CouncilActions.Int("iMoney")>=Payments[Bloc];
    [YarnFunction("PoliticalConcession")]public static string Concession()=>Concessions[Bloc];
    [YarnFunction("PoliticalCanEmpower")]public static bool CanEmpower()=>PoliticalSystem.Share(Bloc)<=95;
    [YarnCommand("ResolvePoliticalDemand")]public static void Resolve(string choice)
    {
        if(Active<0)return;int bloc=Active;
        if(choice=="pay"){if(!CanPay())return;VariableSingleton.ChangeInt("iMoney",-Payments[bloc]);}
        else if(choice=="concede")VariableSingleton.ChangePercentFloat(Keys[bloc],-.05f);
        else if(choice=="empower"){if(!CanEmpower())return;PoliticalSystem.Gain(bloc,5);}
        else if(choice=="refuse")VariableSingleton.SetInt("iPolBlocked"+bloc,PoliticalSystem.Day+15);
        else return;
        if(choice!="refuse"){
            VariableSingleton.SetInt("iPolSettled"+bloc,DailyLifeEvents.I("iPolSettled"+bloc)+1);
            VariableSingleton.SetInt("iPolOwed"+bloc,Math.Max(0,PoliticalSystem.Owed(bloc)-1));
        }
        VariableSingleton.SetInt("iPolDemand",0);
    }
}
