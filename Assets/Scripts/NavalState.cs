using UnityEngine;
using Yarn.Unity;

public static class NavalState
{
    public const int StartingShips=48; // One third of Barbaro's 145 vessels, rounded down, as agreed.
    public static string ReplacementSource=>DailyLifeEvents.I("iOttShipSource")==0?"Gallipoli":"Izmit";
    public static int EnemyShips=>CouncilActions.Int("iTurkShips");
    public static bool Uncontested=>CouncilActions.Int("iShips")>EnemyShips;
    public static string SeaControl=>Uncontested?"Uncontested seas":"Contested seas";
    public static void Initialise(){VariableSingleton.SetInt("iTurkShips",StartingShips);VariableSingleton.SetInt("iNavalLastDay",0);VariableSingleton.SetInt("iNavalBattles",0);VariableSingleton.SetInt("iGalataCastles",0);VariableSingleton.SetInt("iNavalOurLoss",0);VariableSingleton.SetInt("iNavalEnemyLoss",0);VariableSingleton.SetInt("iNavalDiplomacyReward",0);VariableSingleton.SetInt("iArtilleryConvoy",0);}
    public static void CompleteDay(){int day=TrainingPool.Day(VariableSingleton.GetDate());if(CouncilActions.Int("iNavalLastDay")==day)return;VariableSingleton.SetInt("iNavalLastDay",day);// Provincial departures now provide all replacement ships.
        Sync();}
    public static void Sync()=>VariableSingleton.ChangeBool("bTurkFleet",EnemyShips>0);
    public static int VictoryChance=>Mathf.Clamp(10+CouncilActions.Int("iShips")+Mathf.RoundToInt((StartingShips-EnemyShips)*.5f),5,95);
    public static string LastReport=>"Our ships lost: "+CouncilActions.Int("iNavalOurLoss")+" · Ottoman ships lost: "+CouncilActions.Int("iNavalEnemyLoss")+"\nCrews lost: "+(CouncilActions.Int("iNavalOurLoss")*SpecialistPersonnel.ShipCrew)+" (including "+CouncilActions.Int("iNavalOurLoss")+" ship engineers).\nSurviving ships: "+CouncilActions.Int("iShips")+" ours · "+EnemyShips+" Ottoman · "+SeaControl+(DailyLifeEvents.I("iNavalDiplomacyReward")>0?"\nVenice, Genoa, the Hospitallers, Cyprus and Naples gain +10 goodwill after this major victory.":"");
    // Only destroyed operational hulls use this adapter. Withdrawals and transfers
    // to the damaged-ship pool keep their distinct accounting.
    public static int SinkShips(int requested)
    {
        int lost=Mathf.Clamp(requested,0,Mathf.Max(0,CouncilActions.Int("iShips")));
        if(lost==0)return 0;
        VariableSingleton.ChangeInt("iShips",-lost);SpecialistPersonnel.RecordSunkShips(lost);return lost;
    }
    [YarnCommand("SinkShips")]public static void SinkShipsCommand(int ships)=>SinkShips(ships);
    [YarnFunction("NavalChance")] public static int Chance()=>VictoryChance;
    [YarnFunction("NavalReport")] public static string Report()=>LastReport;
    [YarnCommand("NavalBattle")] public static void Battle(bool victory)
    {
        int ours=CouncilActions.Int("iShips"),enemy=EnemyShips;if(ours<=0||enemy<=0)return;
        int ourLoss=Mathf.Clamp(Mathf.RoundToInt(ours*Random.Range(victory?.05f:.25f,victory?.15f:.50f)),0,ours);
        int enemyLoss=Mathf.Clamp(Mathf.RoundToInt(enemy*Random.Range(victory?.40f:.10f,victory?.65f:.25f)),0,enemy);
        SinkShips(ourLoss);VariableSingleton.ChangeInt("iTurkShips",-enemyLoss);
        VariableSingleton.SetInt("iNavalOurLoss",ourLoss);VariableSingleton.SetInt("iNavalEnemyLoss",enemyLoss);VariableSingleton.ChangeInt("iNavalBattles",1);
        if(!victory){VariableSingleton.SetInt("iFleet",3);VariableSingleton.SetInt("iFleetPrepared",0);}Sync();
        VariableSingleton.SetInt("iNavalDiplomacyReward",DiplomaticBattleEvents.NavalVictory(victory,enemy,enemyLoss)?1:0);
    }
}
