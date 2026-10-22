using UnityEngine;
using TMPro;
public static class AssaultPractice {
 static AssaultPractice(){UnityEngine.SceneManagement.SceneManager.sceneUnloaded+=scene=>{practising=false;manager=null;council=null;};}
 static ManagerUI manager;static CouncilUI council;static bool practising;static UnityEngine.Random.State savedRandom;public static int ForcedIncident=-1;
 public static void Show(ManagerUI owner){if(!practising){savedRandom=UnityEngine.Random.state;practising=true;}manager=owner;council=CouncilUI.For(owner);council.AssaultPracticeMenu();}
 public static void Launch(int kind){
  council.Hide();manager.gameObject.SetActive(false);string condition=ForcedIncident>=0?AssaultIncidents.All[ForcedIncident].Condition:"";var random=new System.Random();var b=new PhasedAssault(random.Next());
  b.Practice=true;b.Type=kind==2?2:1;b.Attackers=b.EnemyLeft=8000;b.InitialInfantry=b.Infantry=2500;b.InitialArchers=b.Archers=1500;b.InitialCavalry=b.Cavalry=100;b.InitialGuards=b.Guards=180;b.InitialHealth=b.Health=100;b.Wall=kind==2?.3f:.7f;b.Breach=kind==2?35:10;b.ArcherBudget=1500;b.FootBudget=1000;b.CavalryBudget=80;b.EnemyLethality=.12f;b.GuardPower=4;b.PersonalBudget=4;b.Tower=kind==1||condition=="tower";b.Ram=kind==1||condition=="ram";b.Cannons=kind==3||condition=="cannon"?3:0;b.Ammo=b.Cannons>0?3:0;b.GunBudget=b.Cannons>0?500:0;b.Frontline=kind==2;
  PhasedAssaultUI.Open(b,manager.transform.parent);
 }
 public static void Duel(){council.Hide();AssaultDuelUI.Open(manager.transform.parent,(won,wounds)=>{Show(manager);});}
 public static void Exit(){if(practising)UnityEngine.Random.state=savedRandom;practising=false;council.Hide();manager.gameObject.SetActive(true);}
 public static void Return(){if(manager==null)return;manager.gameObject.SetActive(true);Show(manager);}
}
