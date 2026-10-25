using System;

// Isolated test model: no campaign singleton, save, achievement or Unity RNG access.
public sealed class AlbaniaWarTrial {
 public enum Order { Hold, Advance, Raid, AidRebels }
 public readonly int Seed,Scenario; readonly Random random;
 int lastRaidDay=-100;
 public int Day,Front=50,Albanians=10000,Ottomans=15000,AlbanianFood=24,OttomanFood=30,Authority=65,Resistance,Support=3;
 public bool Rebellion,Liberated,Suppressed;
 public string Last="Choose an order. Each decision resolves one day; the trial ends after 30 days.";
 public string EnemyIntent=>Day-lastRaidDay<3?"Escort the supply road":OttomanFood<6?"Secure provisions":Rebellion&&Resistance>=35?"Suppress the revolt":Front>=60?"Counterattack the Albanian advance":"Press the frontier";
 public bool Finished=>Day>=30||Front<=0||Front>=100||Albanians<=0||Ottomans<=0;
 public string Outcome=>!Finished?"War in progress":Front>=100||Ottomans<=0?"Albanian breakthrough":Front<=0||Albanians<=0?"Ottoman breakthrough":Front>=65?"Albanian territorial advantage":Front<=35?"Ottoman territorial advantage":"Contested frontier — neither army has won";
 public AlbaniaWarTrial(int seed,int scenario){Seed=seed;Scenario=scenario;random=new Random(seed);if(scenario==1){Rebellion=true;Authority=45;Resistance=60;}if(scenario==2){Rebellion=true;Authority=45;Resistance=20;}if(scenario==3){AlbanianFood=8;OttomanFood=12;Authority=50;Resistance=35;}}
 int Roll(int min,int max)=>random.Next(min,max+1);
 static int Clamp(int v,int lo,int hi)=>Math.Max(lo,Math.Min(hi,v));
 public bool Can(Order order)=>!Finished&&(order!=Order.AidRebels||Support>0&&!Liberated&&!Suppressed);
 public bool Step(Order order){
  if(!Can(order))return false;
  string intent=EnemyIntent;Day++;int start=Front;bool revoltAtStart=Rebellion;
  AlbanianFood=Math.Max(0,AlbanianFood-1);OttomanFood=Math.Max(0,OttomanFood-1);
  // Local road/foraging assumptions; this bounded trial does not draw campaign resources.
  if(Day%7==0){AlbanianFood+=4;if(!Rebellion&&!Liberated)OttomanFood+=5;}
  if(intent=="Secure provisions")OttomanFood+=3;
  if(intent=="Escort the supply road")OttomanFood+=2;
  if(order==Order.Raid){OttomanFood=Math.Max(0,OttomanFood-(intent=="Escort the supply road"?1:3));lastRaidDay=Day;}
  if(order==Order.AidRebels){Support--;Resistance=Clamp(Resistance+8,0,100);Authority=Clamp(Authority-3,0,100);}
  double a=Albanians/10000.0*(AlbanianFood>0?1:.60),o=Ottomans/15000.0*(OttomanFood>0?1:.60);
  int change=Roll(-2,2)-1+(int)Math.Round((a-o)*5)+(order==Order.Advance?3:order==Order.Raid?1:order==Order.AidRebels?-1:0);
  if(intent=="Counterattack the Albanian advance")change-=2;
  if(intent=="Secure provisions")change+=2;
  if(intent=="Escort the supply road"&&Front>=60)change-=2;
  if(revoltAtStart)change+=1; // Garrison diverted away from the frontier.
  if(Liberated)change+=1;
  Front=Clamp(Front+change,0,100);
  int aLoss=Roll(30,90)+(order==Order.Advance?70:order==Order.Hold?-20:0)+(AlbanianFood==0?40:0);
  int oLoss=Roll(40,110)+(order==Order.Raid?35:0)+(revoltAtStart?35:0)+(OttomanFood==0?60:0);
  Albanians=Math.Max(0,Albanians-aLoss);Ottomans=Math.Max(0,Ottomans-oLoss);
  string eventText="";
  if(Rebellion){
   // Same simultaneous 1–5 erosion and Ottoman tie rule as the provincial system.
   int authorityLoss=Roll(1,5)+(order==Order.Advance?2:0),resistanceLoss=Roll(1,5);
   if(intent=="Suppress the revolt")resistanceLoss+=2;
   if(order==Order.Advance)resistanceLoss=Math.Max(1,resistanceLoss-1);
   Authority=Math.Max(0,Authority-authorityLoss);Resistance=Math.Max(0,Resistance-resistanceLoss);
   if(Resistance==0){Rebellion=false;Suppressed=true;Authority=100;Front=Math.Max(0,Front-8);eventText="The revolt is suppressed. Released garrison troops restore eight points of Ottoman frontier control.";}
   else if(Authority==0){Rebellion=false;Liberated=true;Front=Math.Min(100,Front+8);eventText="The province is liberated. Ottoman local exports remain closed; Albanian control gains eight points.";}
  }
  else if(!Liberated&&!Suppressed){if(order==Order.Advance)Authority=Math.Max(0,Authority-2);if(Authority<=45){Rebellion=true;eventText="A provincial revolt begins. The local Ottoman supply route closes.";}}
  Last="Day "+Day+" · Ottoman order: "+intent+"\nFront "+(Front-start>=0?"+":"")+(Front-start)+" points · Albanian losses "+aLoss+" · Ottoman losses "+oLoss+".\n"+(revoltAtStart?"Rebels divert troops: +1 frontier pressure. ":"")+(revoltAtStart&&order==Order.Advance?"Albanian pressure aids rebels: +2 authority damage and one less resistance loss. ":"")+eventText;
  return true;
 }
 public const string Assumptions="BOUNDED TEST — NOT A LIVE CAMPAIGN WAR\n30 days; one order per day. Seeded results; restart repeats the same scenario. Fictional balance strengths: 10,000 Albanians and 15,000 Ottomans. Front 0 = Ottoman breakthrough; 100 = Albanian breakthrough. At day 30, 35/65 mark territorial advantage; otherwise stalemate.\n\nSUPPLY AND CHOICES\nEach army uses one local food-day daily. Every seven days Albania receives four, Ottomans five if their province is neither rebelling nor liberated. Hungry armies operate at 60% strength. Ottoman foraging gains three food-days but concedes pressure. Ottoman pressure contributes one point daily. Advance gains pressure but increases losses; raid destroys three enemy food-days, reduced to one against escorted convoys. For three days after a raid, Ottomans prioritise escorts: two local food-days gained; counterattacks continue when Albanian pressure reaches 60; holding limits losses. Three aid packages each add eight resistance and remove three authority; aid yields ground.\n\nREVOLT AND WAR HELP EACH OTHER\nAn active revolt diverts Ottoman troops (+1 Albanian pressure) and interrupts local deliveries. An Albanian advance during revolt adds two authority damage and reduces resistance losses by one, minimum one. Ottomans prioritise suppression at resistance 35+, adding two resistance losses. Base rebellion losses remain independently 1–5 each day; simultaneous collapse favours Ottomans. Liberation shifts the front +8; suppression shifts it −8, once.\n\nBOUNDARIES\nOne abstract Macedonia/Thessaly frontier, not conquest of the entire province. No naval intervention, terrain subregions, relief army, diplomacy or second front. No campaign time, money, troops, agents, saves, achievements or random state are changed. Future integration must route reinforcements through the existing finite supply ledger and apply provincial neighbour effects exactly once.";
}
