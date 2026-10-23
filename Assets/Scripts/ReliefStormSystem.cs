using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
public static class ReliefStormSystem {
    static string lastReport="";
    public static bool Exposed(int mission)=>mission==0||mission==1||mission==2||mission==4;
    public static List<int> Incoming() {
        var result=new List<int>();
        for(int i=0;i<CouncilActions.Allies.Length;i++) if(AidRouting.BySea(i) && CouncilActions.Int("iAidState"+i)==1)result.Add(i);
        return result;
    }
    [YarnCommand("ReliefStorm")]public static void Resolve(int severity) {
        var incoming=Incoming();int target=incoming.Count==0?-1:incoming[Random.Range(0,incoming.Count)];
        Apply(severity,target,Random.Range(1,11),Random.Range(0,6));
    }
    public static void Apply(int severity,int target,int enemyRoll,int ourRoll) {
        var lines=new List<string>();
        if(target>=0 && target<CouncilActions.Allies.Length && AidRouting.BySea(target) && CouncilActions.Int("iAidState"+target)==1) {
            string name=CouncilActions.Allies[target].Name;
            if(severity==1){VariableSingleton.ChangeInt("iAidDays"+target,10);lines.Add(name+"'s relief fleet takes shelter far off its course. Arrival is delayed by 10 days; "+CouncilActions.Int("iAidDays"+target)+" days now remain.");}
            else {VariableSingleton.SetInt("iAidState"+target,4);VariableSingleton.SetInt("iAidDays"+target,0);lines.Add(name+"'s relief fleet is lost at sea. Its promised troops, ships and cargo will not arrive.");}
        } else lines.Add("For once, an empty horizon is a mercy: no relief fleet was on its way to us. No promised expedition is lost or delayed.");
        int enemyLoss=Mathf.Min(CouncilActions.Int("iTurkShips"),Mathf.Clamp(enemyRoll,1,10));
        VariableSingleton.ChangeInt("iTurkShips",-enemyLoss);NavalState.Sync();
        lines.Add(enemyLoss>0?"Ottoman losses: "+enemyLoss+" ships sunk.":"No Ottoman ships remain at sea to be lost.");
        int ours=CouncilActions.Int("iShips");
        if(Exposed(CouncilActions.Int("iFleet")) && ours>0) {
            int loss=Mathf.Min(ours,Mathf.Clamp(ourRoll,0,5));VariableSingleton.ChangeInt("iShips",-loss);
            lines.Add(loss>0?"Our ships were operating in open waters: "+loss+" lost, "+CouncilActions.Int("iShips")+" remain.":"Our captains ride out the storm in open waters. Every ship returns.");
        } else lines.Add(ours>0?"Our fleet is sheltered in the Golden Horn. None of our ships are lost.":"We have no ships at sea to lose.");
        lastReport=string.Join("\n\n",lines);
    }
    [YarnFunction("ReliefStormReport")]public static string Report()=>lastReport;
}
