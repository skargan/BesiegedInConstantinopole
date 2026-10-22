using System;
using UnityEngine;
using Yarn.Unity;
public static class DefencePosture {
 public static int SeaPercent=>DailyLifeEvents.I("iSeaGarrisonPercent",25);
 public static bool Dismounted=>DailyLifeEvents.I("iWallCavalry",1)==1;
 public static int SeaInfantry=>Mathf.RoundToInt(CouncilActions.Int("iInfantry")*SeaPercent/100f);
 public static int SeaArchers=>Mathf.RoundToInt(CouncilActions.Int("iArchers")*SeaPercent/100f);
 public static int LandInfantry=>CouncilActions.Int("iInfantry")-SeaInfantry;
 public static int LandArchers=>CouncilActions.Int("iArchers")-SeaArchers;
 public static int Integrity=>DailyLifeEvents.I("iSeaIntegrity",100);
 public static bool HornOpen=>DailyLifeEvents.I("iOttomanHornAccess")>0||!VariableSingleton.GetBoolVariable("bGoldenHornChain");
 public static int Required=>300+NavalState.EnemyShips*4+(HornOpen?150:0);
 public static int Strength=>SeaInfantry+SeaArchers+Integrity*2+(CouncilActions.Int("iFleet")==3?CouncilActions.Int("iShips")*5:0);
 public static string Summary=>"Main wall: "+LandInfantry+" infantry · "+LandArchers+" archers · "+(Dismounted?CouncilActions.Int("iCavalry"):0)+" dismounted cavalry\nSea walls: "+SeaInfantry+" infantry · "+SeaArchers+" archers · integrity "+Integrity+"%\nSea defence "+Strength+" / expected pressure "+Required+(DailyLifeEvents.I("iSeaRaidDue")>0?" · coastal attack warned for siege day "+DailyLifeEvents.I("iSeaRaidDue"):"");
 public static void Initialise(){VariableSingleton.SetInt("iSeaGarrisonPercent",25);VariableSingleton.SetInt("iWallCavalry",1);VariableSingleton.SetInt("iSeaIntegrity",100);foreach(string key in new[]{"iSeaRaidDue","iSeaNextWindow","iOttomanHornAccess"})VariableSingleton.SetInt(key,0);}
 [YarnCommand("OttomanHornAccess")]public static void SetHornAccess(){VariableSingleton.SetInt("iOttomanHornAccess",1);}
 public static string Advance(){
  int day=VariableSingleton.CompletedSiegeDays;if(day<=0)return "";int due=DailyLifeEvents.I("iSeaRaidDue");
  if(due>0&&day>=due){VariableSingleton.SetInt("iSeaRaidDue",0);VariableSingleton.SetInt("iSeaNextWindow",day+4);if(NavalState.Uncontested&&!HornOpen)return "Coastal watch: Ottoman boats withdraw before a landing. Our fleet controls the approaches.\n\n";
   if(Strength>=Required)return "Coastal attack repulsed. The sea-wall garrison holds without losses; the land operation remains separate.\n\n";
   int lost=Mathf.Min(SeaInfantry+SeaArchers,Mathf.Clamp((Required-Strength)/25,1,20));int foot=Mathf.Min(SeaInfantry,lost);VariableSingleton.ChangeInt("iInfantry",-foot);VariableSingleton.ChangeInt("iArchers",-Mathf.Min(SeaArchers,lost-foot));VariableSingleton.SetInt("iSeaIntegrity",Mathf.Max(0,Integrity-10));
   return "Coastal attack: "+lost+" sea-wall defenders lost; integrity −10 points, now "+Integrity+"%. "+(Integrity==0?"A coastal breach needs repair; it does not instantly end the campaign.":"Review the sea garrison before the next warning.")+"\n\n";
  }
  if(due==0&&day>=Mathf.Max(4,DailyLifeEvents.I("iSeaNextWindow"))&&(!NavalState.Uncontested||HornOpen)){VariableSingleton.SetInt("iSeaRaidDue",day+1);return "COASTAL WARNING: Ottoman landing boats gather for siege day "+(day+1)+". Review the sea-wall garrison. Land assaults may happen on the same day.\n\n";}return "";
 }
 public static bool Repair(out string report){report="Requires 500 gold, 40 materials and damaged sea walls.";if(Integrity>=100||CouncilActions.Int("iMoney")<500||CouncilActions.Int("iMaterials")<40)return false;VariableSingleton.ChangeInt("iMoney",-500);VariableSingleton.ChangeInt("iMaterials",-40);VariableSingleton.SetInt("iSeaIntegrity",Mathf.Min(100,Integrity+20));VariableSingleton.SetInt("iAchievementWallRepaired",1);report="Sea-wall repairs: integrity +20 points, now "+Integrity+"%. Gold −500; materials −40. Major action spent.";return true;}
 public static void Show(ManagerUI manager,Action after){
  var panel=CampaignHUD.Panel(manager.transform.parent,"Defence dispositions",new Vector2(.035f,.04f),new Vector2(.965f,.865f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
  CampaignHUD.Label(panel.transform,"Defence dispositions · land and sea",new Vector2(.04f,.88f),new Vector2(.96f,.98f),31);
  CampaignHUD.Explanation(panel.transform,Summary+"\nGolden Horn chain: "+(VariableSingleton.GetBoolVariable("bGoldenHornChain")?"intact":"absent")+" · "+NavalState.SeaControl+"\nThe two coastal sectors share one sea-wall garrison. Only the main-wall allocation fights the current land assault.",new Vector2(.04f,.51f),new Vector2(.96f,.86f),25);
  int[] values={0,25,50,75,100};for(int i=0;i<5;i++){int n=values[i];float x=.04f+i*.185f;CampaignHUD.MakeButton(panel.transform,(SeaPercent==n?"✓ ":"")+n+"% to sea walls",new Vector2(x,.36f),new Vector2(x+.175f,.47f),()=>{VariableSingleton.SetInt("iSeaGarrisonPercent",n);UnityEngine.Object.Destroy(panel);Show(manager,after);});}
  CampaignHUD.MakeButton(panel.transform,Dismounted?"Dismount cavalry: ON":"Dismount cavalry: OFF · kept in reserve",new Vector2(.04f,.19f),new Vector2(.63f,.31f),()=>{VariableSingleton.SetInt("iWallCavalry",Dismounted?0:1);UnityEngine.Object.Destroy(panel);Show(manager,after);});
  var repair=CampaignHUD.MakeButton(panel.transform,"Repair sea walls\n500 gold · 40 materials · major action",new Vector2(.66f,.19f),new Vector2(.96f,.31f),()=>{if(Repair(out var report)){UnityEngine.Object.Destroy(panel);ActionResults.Instance.Message=report;manager.CompleteAction();}});repair.interactable=after==null&&Integrity<100&&CouncilActions.Int("iMoney")>=500&&CouncilActions.Int("iMaterials")>=40;
  CampaignHUD.MakeButton(panel.transform,"Confirm dispositions",new Vector2(.30f,.035f),new Vector2(.70f,.14f),()=>{UnityEngine.Object.Destroy(panel);after?.Invoke();});
 }
}
