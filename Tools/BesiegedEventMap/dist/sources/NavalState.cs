using UnityEngine;
using Yarn.Unity;

public static class NavalState
{
    public const int StartingShips=48; // One third of Barbaro's 145 vessels, rounded down, as agreed.
    public static int EnemyShips=>CouncilActions.Int("iTurkShips");
    public static bool Uncontested=>CouncilActions.Int("iShips")>EnemyShips;
    public static string SeaControl=>Uncontested?"Uncontested seas":"Contested seas";
    public static void Initialise(){VariableSingleton.SetInt("iTurkShips",StartingShips);VariableSingleton.SetInt("iNavalLastDay",0);VariableSingleton.SetInt("iNavalBattles",0);VariableSingleton.SetInt("iGalataCastles",0);VariableSingleton.SetInt("iNavalOurLoss",0);VariableSingleton.SetInt("iNavalEnemyLoss",0);VariableSingleton.SetInt("iArtilleryConvoy",0);}
    public static void CompleteDay(){int day=TrainingPool.Day(VariableSingleton.GetDate());if(CouncilActions.Int("iNavalLastDay")==day)return;VariableSingleton.SetInt("iNavalLastDay",day);VariableSingleton.ChangeInt("iTurkShips",1);Sync();}
    public static void Sync()=>VariableSingleton.ChangeBool("bTurkFleet",EnemyShips>0);
    public static int VictoryChance=>Mathf.Clamp(10+CouncilActions.Int("iShips")+Mathf.RoundToInt((StartingShips-EnemyShips)*.5f),5,95);
    public static string LastReport=>"Our ships lost: "+CouncilActions.Int("iNavalOurLoss")+" · Ottoman ships lost: "+CouncilActions.Int("iNavalEnemyLoss")+"\nSurviving ships: "+CouncilActions.Int("iShips")+" ours · "+EnemyShips+" Ottoman · "+SeaControl;
    [YarnFunction("NavalChance")] public static int Chance()=>VictoryChance;
    [YarnFunction("NavalReport")] public static string Report()=>LastReport;
    [YarnCommand("NavalBattle")] public static void Battle(bool victory)
    {
        int ours=CouncilActions.Int("iShips"),enemy=EnemyShips;if(ours<=0||enemy<=0)return;
        int ourLoss=Mathf.Clamp(Mathf.RoundToInt(ours*Random.Range(victory?.05f:.25f,victory?.15f:.50f)),0,ours);
        int enemyLoss=Mathf.Clamp(Mathf.RoundToInt(enemy*Random.Range(victory?.40f:.10f,victory?.65f:.25f)),0,enemy);
        VariableSingleton.ChangeInt("iShips",-ourLoss);VariableSingleton.ChangeInt("iTurkShips",-enemyLoss);
        VariableSingleton.SetInt("iNavalOurLoss",ourLoss);VariableSingleton.SetInt("iNavalEnemyLoss",enemyLoss);VariableSingleton.ChangeInt("iNavalBattles",1);
        if(!victory){VariableSingleton.SetInt("iFleet",3);VariableSingleton.SetInt("iFleetPrepared",0);}Sync();
    }
}
