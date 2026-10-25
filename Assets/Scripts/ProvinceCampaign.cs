using System;
using System.Linq;
using System.Text;
using UnityEngine;

// All persistent fields share the iWbw prefix, cleared by the existing restore boundary.
public static class ProvinceCampaign {
 public static readonly int[] FoodQuota={24,24,24,15,10,12,6,10,10,9,6};
 static readonly string[] Troops={"iBalkans","iBazouks","iJanissaries"};
 static readonly string[] Edges={"thrace|bulgaria","thrace|macedonia","bulgaria|macedonia","bithynia|karasi","bithynia|germiyan","bithynia|anatolia","bithynia|candar","karasi|aydin","karasi|germiyan","aydin|germiyan","aydin|mentese","mentese|germiyan","germiyan|anatolia","anatolia|candar","anatolia|armeniakon","candar|armeniakon"};
 public static int Get(string k)=>DailyLifeEvents.I("iWbw34"+k);
 public static void Set(string k,int v)=>VariableSingleton.SetInt("iWbw34"+k,v);
 public static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
 public static int Authority(string id)=>Mathf.Clamp(100-DailyLifeEvents.I("iWbwControlLoss_"+id),0,100);
 public static void Authority(string id,int v)=>VariableSingleton.SetInt("iWbwControlLoss_"+id,100-Mathf.Clamp(v,0,100));
 public static int Resistance(string id)=>Get("R_"+id);
 public static bool Free(string id)=>DailyLifeEvents.I("iWbwCaptured_"+id)>0;
 public static bool Rebellion(string id)=>Get("Battle_"+id)>0;
 public static int Agent(string id)=>Get("Agent_"+id); // 0 absent,1 travelling,2 resident,3 returning
 public static int Assigned=>WarTheatre.ProvinceIds.Count(id=>Agent(id)>0);
 public static bool Neighbour(string a,string b)=>Edges.Contains(a+"|"+b)||Edges.Contains(b+"|"+a);
 public static string Name(string id)=>WarTheatre.Find(id).name;
 static int Roll(int min,int max)=>UnityEngine.Random.Range(min,max+1);
 public static void Ensure(){
  if(!VariableSingleton.HasSiegeStarted()||Get("Ready")>0)return;
  Set("Ready",1);Set("LastDay",Day);Set("FoodTick",VariableSingleton.CompletedSiegeDays-1);
  int elapsed=VariableSingleton.CompletedSiegeDays;
  OttomanDirector.Ensure();
  if(elapsed<=0){VariableSingleton.SetInt("iFood",Mathf.Max(0,90-Get("PreparationFoodLoss")));VariableSingleton.SetInt("iTurkCannons",Mathf.Max(0,12-Get("PreparationGunLoss")));VariableSingleton.SetInt("iOttAmmo",30);}
  // Older campaigns retain their present stores and force; only future deliveries survive migration.
  for(int t=0;t<3;t++)Set("Reserve"+t,Mathf.Max(0,CouncilActions.Int(Troops[t])));
  for(int i=0;i<WarTheatre.ProvinceIds.Length;i++)for(int wave=0;wave<5;wave++){
   string k=i+"_"+wave;int due=15+wave*30+Roll(0,wave==4?30:20);
   Set("Due"+k,due);Set("Food"+k,FoodQuota[i]/5+(wave<FoodQuota[i]%5?1:0));
   Set("Done"+k,due<=elapsed?1:0);
  }
  // Migrate the original single cell without charging or duplicating its agent.
  if(ProvincialIntelligence.Assigned>0){string id=ProvincialIntelligence.ProvinceId;int state=(int)ProvincialIntelligence.State;Set("Agent_"+id,state==3?4:state==4?3:state==1?1:2);Set("Arrival_"+id,Mathf.Max(Day,ProvincialIntelligence.I("Due")));}
  if(ProvincialIntelligence.State==ProvincialIntelligence.CellState.Operating)Set("LegacyPending",1);else {VariableSingleton.SetInt("iWbwState",0);VariableSingleton.SetInt("iWbwDue",0);}
 }
 public static string Status(string id){
  return Free(id)?"Liberated · monthly gifts":Rebellion(id)?"Open rebellion · exports stopped":Agent(id)==1?"Agent travelling · "+Mathf.Max(0,Get("Arrival_"+id)-Day)+" days":Agent(id)==3?"Agent returning · "+Mathf.Max(0,Get("Arrival_"+id)-Day)+" days":Agent(id)==4?"Previously commissioned operation in progress":Agent(id)==2?"Resident agent · "+(Get("Mode_"+id)==0?"undermining authority":"organising resistance"):Get("Blocked_"+id)>Day?"Contacts closed for "+(Get("Blocked_"+id)-Day)+" days":"No agent stationed";
 }
 public static string DispatchReason(string id){Ensure();return !VariableSingleton.HasSiegeStarted()?"Available during the siege":Free(id)?"Province already liberated":Rebellion(id)?"Battle in progress":Agent(id)>0?"Agent already assigned":Get("Blocked_"+id)>Day?"Contacts closed for "+(Get("Blocked_"+id)-Day)+" days":CouncilActions.Int("iSpies")<1?"Requires one agent":CouncilActions.Int("iMoney")<500?"Requires 500 gold":null;}
 public static bool Dispatch(string id,out string report){report=DispatchReason(id);if(report!=null)return false;VariableSingleton.ChangeInt("iSpies",-1);VariableSingleton.ChangeInt("iMoney",-500);Set("Agent_"+id,1);Set("Arrival_"+id,Day+7);report="An agent leaves for "+Name(id)+". Passage: 500 gold; arrival in seven days.";return true;}
 public static bool Recall(string id,out string report){report="Requires a resident agent outside an open rebellion.";if(Agent(id)!=2||Rebellion(id))return false;Set("Agent_"+id,3);Set("Arrival_"+id,Day+7);report="Your agent begins the seven-day return journey. Local resistance remains, but daily organising stops.";return true;}
 public static void Mode(string id,int mode){if(Agent(id)==2&&!Rebellion(id)&&!Free(id))Set("Mode_"+id,Mathf.Clamp(mode,0,1));}
 public static int PendingFood(string id){int i=Array.IndexOf(WarTheatre.ProvinceIds,id),sum=0;for(int w=0;w<5;w++)if(Get("Done"+i+"_"+w)==0)sum+=Get("Food"+i+"_"+w);return sum;}
 public static string Reason(string id,int op){Ensure();return Free(id)?"Province liberated":Rebellion(id)?"Battle in progress":Agent(id)!=2?"Requires a resident agent":op==1&&Authority(id)>85?"Requires authority at 85% or below":op==2&&Authority(id)>70?"Requires authority at 70% or below":op==1&&PendingFood(id)<3?"No substantial future convoy remains":CouncilActions.Int("iMoney")<(op==0?500:1000)?"Insufficient gold":null;}
 public static bool Act(string id,int op,out string report){report=Reason(id,op);if(report!=null||op<0||op>2)return false;VariableSingleton.ChangeInt("iMoney",op==0?-500:-1000);int amount=Roll(op==1?3:1,op==1?7:10);
  if(op==0){Authority(id,Authority(id)-amount);report=Name(id)+": Ottoman authority falls by "+amount+" points.";}
  else if(op==2){int old=Resistance(id);Set("R_"+id,Mathf.Min(100,old+amount));report=Name(id)+": resistance gains "+(Resistance(id)-old)+" points.";}
  else {int i=Array.IndexOf(WarTheatre.ProvinceIds,id),left=amount;for(int w=0;w<5&&left>0;w++){string k=i+"_"+w;if(Get("Done"+k)>0)continue;int loss=Mathf.Min(left,Get("Food"+k));Set("Food"+k,Get("Food"+k)-loss);left-=loss;}report="Convoy attacks in "+Name(id)+" destroy "+(amount-left)+" days of provisions intended for the siege.";}
  report+=StartRebellions();return true;
 }
 static string StartRebellions(){var s=new StringBuilder();foreach(string id in WarTheatre.ProvinceIds)if(!Free(id)&&!Rebellion(id)&&Authority(id)<=45){Set("Battle_"+id,1);s.Append("\n"+Name(id)+" rises in open rebellion. Exports cease.");}return s.ToString();}
 public static string Resolve(string id,bool rebels,int day){
  if(!Rebellion(id))return "";Set("Battle_"+id,0);
  if(rebels){VariableSingleton.SetInt("iWbwCaptured_"+id,1);Authority(id,0);Set("Gift_"+id,day+Roll(25,35));}
  else {Authority(id,100);Set("R_"+id,0);Set("Agent_"+id,0);Set("Blocked_"+id,day+100);}
  foreach(string other in WarTheatre.ProvinceIds)if(other!=id&&!Free(other))Authority(other,Authority(other)+(rebels?(Neighbour(id,other)?-10:-5):(Neighbour(id,other)?20:10)));
  return "\n"+Name(id)+(rebels?" is liberated. Ottoman authority falters across the provinces.":" is subdued. The agent is lost; contacts remain closed for 100 days. Ottoman authority strengthens elsewhere.");
 }
 public static string Advance(DateTime date){Ensure();if(Get("Ready")==0)return "";int now=TrainingPool.Day(date);var s=new StringBuilder();
  if(Get("LegacyPending")>0){string id=ProvincialIntelligence.ProvinceId;s.Append(ProvincialIntelligence.Advance(date));if(ProvincialIntelligence.State!=ProvincialIntelligence.CellState.Operating){Set("Agent_"+id,ProvincialIntelligence.State==ProvincialIntelligence.CellState.Lost?0:2);Set("LegacyPending",0);VariableSingleton.SetInt("iWbwState",0);}}
  for(int day=Get("LastDay")+1;day<=now;day++){
   var wins=new System.Collections.Generic.List<string>();var losses=new System.Collections.Generic.List<string>();
   foreach(string id in WarTheatre.ProvinceIds){
    if(Agent(id)==1&&Get("Arrival_"+id)<=day){Set("Agent_"+id,2);s.Append("\nAgent reaches "+Name(id)+".");}
    else if(Agent(id)==3&&Get("Arrival_"+id)<=day){Set("Agent_"+id,0);VariableSingleton.ChangeInt("iSpies",1);s.Append("\nAgent returns from "+Name(id)+".");}
    if(Free(id)){if(day>=Get("Gift_"+id)){int gold=Roll(300,900);VariableSingleton.ChangeInt("iMoney",gold);Set("Gift_"+id,day+Roll(25,35));s.Append("\nGift from "+Name(id)+": "+gold+" gold.");}continue;}
    if(Rebellion(id)){Authority(id,Authority(id)-Roll(1,5));Set("R_"+id,Mathf.Max(0,Resistance(id)-Roll(1,5)));if(Resistance(id)==0)losses.Add(id);else if(Authority(id)==0)wins.Add(id);continue;}
    if(Agent(id)==2){if(Get("Mode_"+id)==0)Authority(id,Authority(id)-Roll(1,2));else Set("R_"+id,Mathf.Min(100,Resistance(id)+Roll(1,2)));}
   }
   foreach(string id in losses)s.Append(Resolve(id,false,day));foreach(string id in wins)s.Append(Resolve(id,true,day));s.Append(StartRebellions());Set("LastDay",day);
  }return s.Length>0?"\n\nWar Beyond the Walls"+s+"\n":"";
 }
 public static string TakeDeliveryReport(){if(Get("DeliveryReport")==0)return "";Set("DeliveryReport",0);return "\nOttoman provincial deliveries: "+Get("DeliveryFood")+" food-days, "+Get("DeliveryMen")+" troops, "+Get("DeliveryGuns")+" cannons and "+Get("DeliveryAmmo")+" ammunition stocks.\n";}
 public static string SupplyDay(){Ensure();int day=VariableSingleton.CompletedSiegeDays;if(Get("FoodTick")>=day)return "";Set("FoodTick",day);int food=0,men=0,guns=0,ammo=0;
  for(int i=0;i<WarTheatre.ProvinceIds.Length;i++)for(int w=0;w<5;w++){string k=i+"_"+w,id=WarTheatre.ProvinceIds[i];if(Get("Done"+k)>0||Get("Due"+k)>day)continue;Set("Done"+k,1);if(Rebellion(id)||Free(id))continue;food+=Get("Food"+k);
   // Each of 55 parcels owns an exact portion of the finite reserve, including remainders.
   int parcel=i*5+w;for(int t=0;t<3;t++){int total=Get("Reserve"+t),n=total/55+(parcel<total%55?1:0);VariableSingleton.ChangeInt(Troops[t],n);men+=n;}
   if(i==0||i==3){guns++;ammo+=12;}
  }
  if(food>0){int loss=Mathf.Min(food,ImperialSupply.GetSupplyPressure());VariableSingleton.SetInt("iWbwSupplyLoss",ImperialSupply.GetSupplyPressure()-loss);food-=loss;}
  VariableSingleton.ChangeInt("iFood",food);VariableSingleton.ChangeInt("iTurkCannons",guns);OttomanDirector.AddAmmo(ammo);
  if(food+men+guns>0){Set("DeliveryReport",1);Set("DeliveryFood",food);Set("DeliveryMen",men);Set("DeliveryGuns",guns);Set("DeliveryAmmo",ammo);} if(guns>0&&day>=21&&DailyLifeEvents.I("iOttGrandCannon")==0)VariableSingleton.SetInt("iOttGrandCannon",1);
  VariableSingleton.SetInt("iFood",Mathf.Max(0,CouncilActions.Int("iFood")-1));if(CouncilActions.Int("iFood")>0)return "";
  int lost=Mathf.CeilToInt(Mathf.Max(0,CouncilActions.Int("iBazouks"))*.01f);VariableSingleton.ChangeInt("iBazouks",-lost);return "Ottoman stores are exhausted. Foraging interrupts today's attack; "+lost+" irregulars desert. No unscheduled supplies appear.";
 }
}
