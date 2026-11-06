using UnityEngine;
// Standing cavalry operations; all persistent values use the campaign dictionaries.
public static class SallyOut {
 public static readonly string[] Missions={"Attack Ottoman troops","Skirmish and withdraw","Burn the siege works","Attack the batteries","Raid the provisions","Strike at Mehmed"};
 public static void EnsureLightCavalry(){if(DailyLifeEvents.I("iOttLightCavalryReady")!=0)return;VariableSingleton.SetInt("iOttLightCavalryReady",1);VariableSingleton.SetInt("iOttLightCavalry",3000);}
 public static int CutOffChance(int sent)=>sent<=50?0:Mathf.Clamp(sent/50,0,100);
 public static string LastInterception {get;private set;}
 public static bool Resolve(int sent,int mission,bool emperor,out string report){
  report="";if(!VariableSingleton.HasSiegeStarted()||SaveGames.Terminal||sent<1||sent>CouncilActions.Int("iCavalry")||mission<0||mission>=Missions.Length)return false;
  LastInterception=null;
  if(DailyLifeEvents.I("iSallyLastDay")==TrainingPool.Day(VariableSingleton.GetDate())){report="A contingent has already sallied today.";return false;}
  VariableSingleton.SetInt("iSallyLastDay",TrainingPool.Day(VariableSingleton.GetDate()));
  EnsureLightCavalry();
  int enemy=CouncilActions.Int("iBazouks"), defenders=mission==5?15000:Mathf.Min(enemy,Random.Range(60,301));
  float power=sent*TroopTraining.Lethality(2), enemyPower=defenders*(mission==5?1f:.5f);
  int killed=Mathf.Min(defenders,Mathf.FloorToInt(power*Random.Range(.65f,1.05f)));
  int lost=Mathf.Min(sent,Mathf.CeilToInt(enemyPower*Random.Range(.45f,.85f)*(mission==1?.35f:1f)));
  bool success=power*Random.Range(.7f,1.3f)>enemyPower;
  if(mission==5){lost=Mathf.Min(sent,Mathf.CeilToInt(defenders*Random.Range(.8f,1.2f)));success=killed>=defenders&&defenders==15000&&sent>lost;}
  int remaining=killed;foreach(var key in new[]{"iBazouks","iBalkans","iJanissaries"}){int n=Mathf.Min(CouncilActions.Int(key),remaining);VariableSingleton.ChangeInt(key,-n);remaining-=n;}
  report=Missions[mission]+". Ottoman troops killed: "+killed+".";
  int survivors=sent-lost;
  if(survivors>0&&Random.Range(0,100)<CutOffChance(sent)){
   int light=DailyLifeEvents.I("iOttLightCavalry");int intercept=Mathf.Min(light,Random.Range(100,501));int mutual=Mathf.Min(survivors,intercept);
   survivors-=mutual;lost+=mutual;VariableSingleton.SetInt("iOttLightCavalry",light-mutual);
   LastInterception="Ottoman light cavalry cut off the road to the gates. Your riders fight through them: "+mutual+" horsemen lost on each side.";report+="\n"+LastInterception;
  }
  VariableSingleton.ChangeInt("iCavalry",-lost);
  if(mission==5&&success&&survivors>0){VariableSingleton.SetInt("iMehmedAssassinationVictory",1);report+="\nYour riders reach Mehmed and kill him. The siege is broken.";}
  else if(mission==5&&emperor&&survivors==0){VariableSingleton.ChangePercentFloat("fHealth",-1);report+="\nThe Emperor falls with the last of his escort.";}
  if(success&&survivors>0){
   if(mission==2){bool engine=VariableSingleton.GetBoolVariable("bSiegeTower")||VariableSingleton.GetBoolVariable("bBatteringRam");if(engine){VariableSingleton.ChangeBool("bSiegeTower",false);VariableSingleton.ChangeBool("bBatteringRam",false);report+="\nThe siege engines burn.";}else{int days=Random.Range(1,4);VariableSingleton.ChangeInt("iEnemyDelay",days);report+="\nBurning tents and stores delay Ottoman operations by "+days+" days.";}}
   if(mission==3){int guns=Mathf.Min(CouncilActions.Int("iTurkCannons"),Random.Range(1,4));VariableSingleton.ChangeInt("iTurkCannons",-guns);report+="\nOttoman cannons destroyed: "+guns+".";}
   if(mission==4){double food=System.Math.Min(survivors,SharedProvisions.Stock(SharedProvisions.Camp));SharedProvisions.Add(SharedProvisions.Camp,-food);SharedProvisions.Add(SharedProvisions.City,food);report+="\nCaptured provisions: "+food.ToString("0.##")+".";}
  }
  report+="\nCavalry lost: "+lost+". Returned: "+survivors+".";return true;
 }
}
