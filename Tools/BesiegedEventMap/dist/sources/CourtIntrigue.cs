using System.Collections.Generic;
using System.Linq;
using Yarn.Unity;

public static class CourtIntrigue
{
    static int I(string key){try{return CouncilActions.Int(key);}catch(KeyNotFoundException){return 0;}}
    static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    // Fictional campaign matches: secular, friendly courtiers only. No historical marital-status claim.
    static int[] Candidates=>new[]{0,1,3,5,6}.Where(i=>VariableSingleton.GetBoolVariable(CourtRoster.Keys[i])&&I("iCourtState"+i)!=4).ToArray();
    public static IEnumerable<string> OptionalNodes()
    {
        if(!VariableSingleton.HasSiegeStarted())yield break;
        if(I("iLadySeen")==0)yield return "courtLady";
        if(I("iLadyFollowup")==0&&Day>=I("iLadyDue")) {
            if(I("iLadyRoute")==1)yield return "courtLadyLetters";
            if(I("iLadyRoute")==2)yield return "courtLadyMarriage";
        }
    }
    [YarnCommand("LadyBegin")]
    public static void Begin(){if(I("iLadySeen")>0)return;VariableSingleton.SetInt("iLadySeen",1);var c=Candidates;VariableSingleton.SetInt("iLadyMatch",c.Length==0?0:c[UnityEngine.Random.Range(0,c.Length)]+1);}
    [YarnFunction("LadyHasMatch")] public static bool HasMatch()=>I("iLadyMatch")>0&&VariableSingleton.GetBoolVariable(CourtRoster.Keys[I("iLadyMatch")-1]);
    [YarnFunction("LadyMatchName")] public static string MatchName()=>I("iLadyMatch")>0?CourtRoster.Names[I("iLadyMatch")-1]:"no available courtier";
    [YarnCommand("LadyRoute")] public static void Route(int route){VariableSingleton.SetInt("iLadyRoute",route);VariableSingleton.SetInt("iLadyDue",Day+5);}
    [YarnCommand("LadyLoyalty")] public static void Loyalty(int delta){if(HasMatch())PreparationCampaign.Loyalty(I("iLadyMatch")-1,delta);}
}
