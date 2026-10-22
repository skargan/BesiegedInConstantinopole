using System;
using UnityEngine;
public static class PhasedAssaultTests {
 static int checks;
 static void Check(bool value,string name){if(!value)throw new Exception("ASSAULT TEST: "+name);checks++;}
 static PhasedAssault Make(int seed){return new PhasedAssault(seed){Attackers=5000,EnemyLeft=5000,InitialInfantry=3000,Infantry=3000,InitialArchers=2000,Archers=2000,InitialCavalry=100,Cavalry=100,InitialGuards=200,Guards=200,InitialHealth=100,Health=100,Wall=.5f,ArcherBudget=1000,FootBudget=1500,CavalryBudget=100,EnemyLethality=.1f,GuardPower=3,Breach=20};}
 public static void Run(){
  Check(PhasedAssault.VolleyScore(.5f)==.25f,"perfect volley");Check(PhasedAssault.VolleyScore(0)==-.25f,"early volley");
  var perfect=Make(1);for(int i=0;i<5;i++)perfect.Volley(.25f);Check(perfect.Shots==3&&perfect.VolleyMultiplier==1.25f,"three volleys average, bounded and repeat safe");
  var ordinary=Make(1);ordinary.Volley(0);ordinary.Volley(0);ordinary.Volley(0);ordinary.Approach(0,0);perfect.Approach(0,0);Check(perfect.Dead-ordinary.Dead>=247&&perfect.Dead-ordinary.Dead<=250,"modifier applies only to archer budget");
  int dead=ordinary.Dead;ordinary.Approach(0,0);Check(dead==ordinary.Dead,"approach resolves once");
  for(int seed=0;seed<300;seed++){
   var b=Make(seed);b.Volley((seed%3-1)*.25f);b.Shelter(seed%2==0?.2f:1);b.Approach(.2f,100);b.Sortie(seed%3,.8f);b.AssignReserve(seed%2==0?25:75);
   for(int j=0;j<6;j++)b.Clash();Check(b.Round<=3&&b.Dead<=b.Attackers&&b.EnemyLeft>=0&&b.Infantry>=0&&b.Archers>=0&&b.Guards>=0,"bounded ledger "+seed);
   b.Emergency(false);Check(b.Health>=1,"nonfatal withdrawal "+seed);b.Resolve(true);int loss=b.OurDead;b.Resolve(true);Check(b.Finished&&b.OurDead==loss,"once-only resolution "+seed);
  }
  var budget=Make(8);budget.ArcherBudget=budget.GunBudget=budget.FootBudget=budget.CavalryBudget=0;budget.Wall=0;budget.EnemyLethality=.1f;budget.Approach(0,0);for(int i=0;i<3;i++)budget.Clash();Check(Mathf.Abs(budget.OurDead-515)<=2,"single enemy budget across three contacts");
  var deadEmperor=Make(9);deadEmperor.Health=0;deadEmperor.Emergency(false);Check(deadEmperor.Health==0,"escort cannot resurrect");
  var footOnly=Make(2);footOnly.Infantry=footOnly.InitialInfantry=100;footOnly.Archers=footOnly.Cavalry=0;footOnly.ArcherBudget=0;footOnly.Approach(0,0);footOnly.ContactBudget=300;footOnly.Wall=0;footOnly.Clash();Check(footOnly.Infantry==85,"all calculated casualties allocated to available infantry");
  var weak=Make(4);weak.Guards=0;weak.Approach(0,0);for(int i=0;i<3;i++)weak.Clash();weak.Resolve(true);Check(!weak.Finished,"unavailable guards rejected");weak.Resolve(false);Check(weak.Finished&&!weak.Held,"fallback available");
  var exhausted=Make(5);exhausted.EnemyLeft=0;exhausted.Resolve(false);Check(exhausted.Held,"no breach without attackers");
  Check(AssaultIncidents.All.Length==24,"24 assault incidents");
  for(int i=0;i<24;i++){bool free=false;foreach(var c in AssaultIncidents.All[i].Choices){if(c.Ammo==0&&c.Materials==0)free=true;var b=Make(i);b.Practice=true;b.Cannons=3;b.Ammo=3;Check(AssaultIncidents.Apply(c,b),"practice incident choice "+i);Check(b.ArcherFactor>=.75f&&b.ArcherFactor<=1.25f&&b.FootFactor>=.75f&&b.FootFactor<=1.25f&&b.Infantry>=0,"incident caps "+i);}Check(free,"unfunded fallback "+i);}
  Check(PhasedAssaultUI.VolleyDuration==7&&PhasedAssaultUI.CoverDuration==3,"approved short durations");
  Check(PhasedAssault.VolleyScore(.36f)==.25f&&PhasedAssault.VolleyScore(.64f)==.25f,"generous ideal plateau");
  var gun=Make(2);gun.GunBudget=500;gun.FireGun(.25f);int killed=gun.Dead;gun.FireGun(.25f);Check(gun.Dead==killed&&killed==625,"one bounded gun allocation");gun.ArcherBudget=0;gun.Approach(0,0);Check(gun.Dead==killed,"gun not charged twice in approach");
  var duel=new SwordDuel();Check(duel.Stroke(new Vector2(.4f,.5f),new Vector2(.6f,.5f)).Contains("ARMOUR"),"armour deflects");duel.Cooldown=0;duel.Cycle=.8f;var w=duel.Weapon;Check(duel.Stroke(w-new Vector2(.1f,0),w+new Vector2(.1f,0)).Contains("PARRY"),"weapon intercept");for(int i=0;i<3;i++){duel.Cooldown=0;duel.Cycle=1.4f;duel.Stroke(new Vector2(.4f,.82f),new Vector2(.6f,.82f));}Check(duel.Finished&&duel.Won,"three opponents defeated");
  ArchitectureChecks();
  Debug.Log("PHASED_ASSAULT_MODEL_CHECKS "+checks+" passed");
 }
 static void ArchitectureChecks(){
  var b=Make(123);b.Clash();b.Sortie(2,1);b.Resolve(true);b.Emergency(true);Check(b.Round==0&&!b.SortieResolved&&!b.Finished&&b.Health==100,"out-of-order actions rejected");
  int notifications=0;b.Resolved+=x=>notifications++;b.Shelter(.65f);for(int i=0;i<3;i++)b.Volley(0);b.Approach(0,0);b.Sortie(0,0);for(int i=0;i<3;i++)b.Clash();b.AssignReserve(50);b.Emergency(false);int health=b.Health;b.Emergency(true);Check(b.Health==health,"personal emergency resolves once");b.Resolve(true);
  Check(b.Finished&&b.Stage==PhasedAssault.Phase.Aftermath&&notifications==1&&b.Outcome!=null,"explicit completed phase with one frozen result");
  int dead=b.Dead,our=b.OurDead,lines=b.Journal.Count;b.FireGun(1);b.Shelter(1);b.Sortie(2,1);b.Clash();b.AssignReserve(100);b.CompleteDuel(true,20);b.Resolve(false);Check(b.Dead==dead&&b.OurDead==our&&b.Journal.Count==lines&&notifications==1,"finished encounter rejects further actions");
  Check(!AssaultIncidents.Apply(AssaultIncidents.All[0].Choices[0],b),"incident cannot alter finished battle");
  Check(b.TryCommit()&&!b.TryCommit(),"one campaign commit per encounter");var frozen=b.Outcome;b.Infantry=0;Check(frozen.InfantryDelta!=b.Infantry-b.InitialInfantry,"result remains frozen despite legacy public field mutation");
  for(int seed=0;seed<30;seed++){var first=Make(seed);var second=Make(seed);for(int i=0;i<3;i++){first.Volley(0);PresentationRandom.Range(-10,10);second.Volley(0);}first.Approach(0,0);second.Approach(0,0);first.Sortie(1,.7f);second.Sortie(1,.7f);for(int i=0;i<3;i++){first.Clash();second.Clash();}first.Resolve(true);second.Resolve(true);Check(first.Outcome.Ledger==second.Outcome.Ledger,"seeded repeatability with cosmetic noise "+seed);}
  var state=UnityEngine.Random.state;float expected=UnityEngine.Random.value;UnityEngine.Random.state=state;for(int i=0;i<200;i++)PresentationRandom.Range(0,1);Check(UnityEngine.Random.value==expected,"cosmetic samples leave campaign RNG intact");UnityEngine.Random.state=state;
  AssaultIncidents.Validate(AssaultIncidents.All);Check(true,"authoritative content schema valid");
  var incidentBattle=Make(88);incidentBattle.Practice=true;Check(AssaultIncidents.Choose(0,0,incidentBattle)&&!AssaultIncidents.Choose(0,1,incidentBattle),"one choice per incident even when UI repeats callback");
  Check(!AssaultIncidents.Choose(5,0,Make(89)),"contact incident rejected during approach");
  var broken=(AssaultIncidents.Incident[])AssaultIncidents.All.Clone();broken[1]=broken[0];bool rejected=false;try{AssaultIncidents.Validate(broken);}catch(InvalidOperationException){rejected=true;}Check(rejected,"duplicate content IDs rejected");
 }
 public static void Verify(){Run();SceneParticleTests.Run();ValidateStory.Run();}
 public static void Build(){Verify();ConfigurePlaytest.Build();}
}
