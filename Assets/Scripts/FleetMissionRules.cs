using System;
using UnityEngine;
public static class FleetMissionRules
{
 public static readonly string[] Names={"Raid Ottoman supply ships","Raid Ottoman merchant shipping","Bring grain from Crimea","Defend the Golden Horn","Bring volunteers","Awaiting orders"};
 public static readonly string[] Descriptions={"Seize provisions from Ottoman supply ships and bring the captured cargo into the City.","Seek plunder from Ottoman merchantmen, with a chance of finding valuable intelligence.","Bring grain from Black Sea ports while the Bosphorus passage remains open.","Remain on the defensive and support the walls with arrows and cannon fire; no routine reports.","Bring willing soldiers and skilled travellers into the City's service.","Issue new orders to your ships."};
 public static int Defence=>CouncilActions.Int("iFleet")==3?RecurringEvents.FleetDefensiveDamage:0;
 public static string Resolve(int mission,Func<int,int,int> roll=null){
 if(CouncilActions.Int("iShips")<=0||mission<0||mission>4||mission==3)return "";
 if(mission==2&&!VariableSingleton.GetBoolVariable("bStraitsOpen"))return "The Bosphorus is closed; the grain mission cannot proceed.";
 if(roll==null)roll=(low,high)=>UnityEngine.Random.Range(low,high+1);
 if(mission==0){double food=SharedProvisions.Transfer(SharedProvisions.Camp,SharedProvisions.City,roll(0,400));return "Captured Ottoman provisions: "+SharedProvisions.Format(food)+".";}
 if(mission==1){int gold=roll(500,1500),materials=roll(0,100),secret=roll(1,100)<=2?1:0;VariableSingleton.ChangeInt("iMoney",gold);VariableSingleton.ChangeInt("iMaterials",materials);if(secret>0)VariableSingleton.ChangeInt("iSecrets",1);return "Merchant shipping raid: "+gold+" gold and "+materials+" materials."+(secret>0?" Captured papers reveal one secret.":"");}
 if(mission==2){SharedProvisions.Add(SharedProvisions.City,150);return "The grain ships deliver 150 provisions.";}
 int infantry=roll(50,100),archers=roll(25,50),cavalry=roll(0,10),envoy=roll(1,100)<=3?1:0,monks=roll(1,100)<=10?roll(1,4):0,engineers=roll(1,100)<=10?roll(1,2):0,guards=roll(1,100)<=3?1:0;
 string[] keys={"iInfantry","iArchers","iCavalry","iEnvoys","iHealers","iEngineers","iVarangians"};int[] values={infantry,archers,cavalry,envoy,monks,engineers,guards};int people=0;for(int i=0;i<keys.Length;i++){VariableSingleton.SetInt(keys[i],DailyLifeEvents.I(keys[i])+values[i]);people+=values[i];}PopulationAccounting.Add(people);
 VariableSingleton.SetInt("iMercForeignInfantry",DailyLifeEvents.I("iMercForeignInfantry")+infantry);VariableSingleton.SetInt("iMercForeignArchers",DailyLifeEvents.I("iMercForeignArchers")+archers);VariableSingleton.SetInt("iMercForeignCavalry",DailyLifeEvents.I("iMercForeignCavalry")+cavalry);
 return "Volunteers arrive: "+infantry+" infantry, "+archers+" archers and "+cavalry+" cavalry."+(envoy>0?" One envoy offers his service.":"")+(monks>0?" Monks: "+monks+".":"")+(engineers>0?" Engineers: "+engineers+".":"")+(guards>0?" One Varangian joins the Guard.":"");
 }
}
