using System;
using UnityEngine;
public static class PhasedAssaultTests {
 static int checks;
 static void Check(bool value,string name){if(!value)throw new Exception("ASSAULT TEST: "+name);checks++;}
 static PhasedAssault Make(int seed){return new PhasedAssault(seed){Attackers=5000,EnemyLeft=5000,InitialInfantry=3000,Infantry=3000,InitialArchers=2000,Archers=2000,InitialCavalry=100,Cavalry=100,InitialGuards=200,Guards=200,InitialHealth=100,Health=100,Wall=.5f,ArcherBudget=1000,FootBudget=1500,CavalryBudget=100,EnemyLethality=.1f,GuardPower=3,Breach=20};}
 public static void Run(){
  Check(PhasedAssault.VolleyScore(.5f)==.25f,"perfect volley");Check(PhasedAssault.VolleyScore(0)==-.25f,"early volley");
  var perfect=Make(1);for(int i=0;i<5;i++)perfect.Volley(.25f);Check(perfect.Shots==3&&perfect.VolleyMultiplier==1.25f,"three volleys average, bounded and repeat safe");
  var ordinary=Make(1);ordinary.Volley(0);ordinary.Volley(0);ordinary.Volley(0);ordinary.Approach(0,0);perfect.Approach(0,0);Check(perfect.Dead-ordinary.Dead==250,"modifier applies only to archer budget");
  int dead=ordinary.Dead;ordinary.Approach(0,0);Check(dead==ordinary.Dead,"approach resolves once");
  for(int seed=0;seed<300;seed++){
   var b=Make(seed);b.Volley((seed%3-1)*.25f);b.Shelter(seed%2==0?.2f:1);b.Approach(.2f,100);b.Sortie(seed%3,.8f);b.AssignReserve(seed%2==0?25:75);
   for(int j=0;j<6;j++)b.Clash();Check(b.Round<=3&&b.Dead<=b.Attackers&&b.EnemyLeft>=0&&b.Infantry>=0&&b.Archers>=0&&b.Guards>=0,"bounded ledger "+seed);
   b.Emergency(false);Check(b.Health>=1,"nonfatal withdrawal "+seed);b.Resolve(true);int loss=b.OurDead;b.Resolve(true);Check(b.Finished&&b.OurDead==loss,"once-only resolution "+seed);
  }
  var budget=Make(8);budget.ArcherBudget=budget.GunBudget=budget.FootBudget=budget.CavalryBudget=0;budget.Wall=0;budget.EnemyLethality=.1f;budget.Approach(0,0);for(int i=0;i<3;i++)budget.Clash();Check(Mathf.Abs(budget.OurDead-500)<=2,"single enemy budget across three contacts");
  var deadEmperor=Make(9);deadEmperor.Health=0;deadEmperor.Emergency(false);Check(deadEmperor.Health==0,"escort cannot resurrect");
  var footOnly=Make(2);footOnly.Infantry=footOnly.InitialInfantry=100;footOnly.Archers=footOnly.Cavalry=0;footOnly.ContactBudget=300;footOnly.Wall=0;footOnly.Clash();Check(footOnly.Infantry==90,"all calculated casualties allocated to available infantry");
  var weak=Make(4);weak.Guards=0;weak.Resolve(true);Check(!weak.Finished,"unavailable guards rejected");weak.Resolve(false);Check(weak.Finished&&!weak.Held,"fallback available");
  var exhausted=Make(5);exhausted.EnemyLeft=0;exhausted.Resolve(false);Check(exhausted.Held,"no breach without attackers");
  Debug.Log("PHASED_ASSAULT_MODEL_CHECKS "+checks+" passed");
 }
 public static void Build(){Run();ConfigurePlaytest.Build();}
}
