using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad] public static class PoliticsSmokeTest {
 static PoliticsSmokeTest(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("PoliticsSmoke",false))new GameObject("Politics checks").AddComponent<PoliticsSmokeProbe>();};}
 public static void Run(){AssetDatabase.Refresh();SessionState.SetBool("PoliticsSmoke",true);EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");EditorApplication.EnterPlaymode();}
}
public class PoliticsSmokeProbe:MonoBehaviour {
 int checks;List<string> failures=new List<string>();
 void Check(bool ok,string text){checks++;if(!ok)failures.Add(text);}
 void Set(string k,int v)=>VariableSingleton.SetInt(k,v);
 int I(string k)=>CouncilActions.Int(k);
 IEnumerator Start(){DontDestroyOnLoad(gameObject);VariableSingleton.TestSavePath=System.IO.Path.GetFullPath("Logs/politics-test-save.json");yield return null;FindObjectOfType<MainMenuUI>().NewGame();yield return null;
 try {
 Set("iBloc0",40);Set("iBloc1",20);Set("iBloc2",20);Set("iBloc3",20);
 Check(PoliticalSystem.Passive(0)&&!PoliticalSystem.Passive(1),"strict21threshold");
 Check(TrainingPool.Duration("iArchers")==9&&TrainingPool.Duration("iShips")==30,"trainingonlyandrounding");
 Set("iMoney",10000);TrainingPool.Place("iArchers",1,10);int due=TrainingPool.Orders.Last().readyDay;Set("iBloc0",20);Check(TrainingPool.Orders.Last().readyDay==due,"existingorderunchanged");
 Set("iBloc1",21);Check(SupplyEconomy.FoodAfterPolitics(100)==95&&SupplyEconomy.FoodAfterPolitics(101)==96,"foodrounding");
 Set("iBloc3",21);Check(SupplyEconomy.Cost("iMaterials",100)==950&&SupplyEconomy.Cost("iMaterials",1)==10,"discounttotalrounding");
 int money=I("iMoney");SupplyEconomy.Buy("iMaterials",100,out _);Check(I("iMoney")==money-950,"actualpurchaseprice");
 Set("iMoney",949);Check(!SupplyEconomy.Buy("iMaterials",100,out _),"unaffordablepurchase");
 Set("iBloc2",20);int chance=CouncilActions.AidChance(1);Set("iBloc2",21);Check(CouncilActions.AidChance(1)==Math.Min(95,chance+5),"westernchance");
 Set("iBloc3",30);Check(PoliticalSystem.Aid(3,out _)&&PoliticalSystem.Owed(3)==1,"favourcreatesdebt");Check(!PoliticalSystem.Aid(3,out _),"weeklylimit");
 Check(PoliticalDemands.Mature(3)==0,"noearlydemand");Set("iPolDebtDay3_1",PoliticalSystem.Day-30);Check(PoliticalDemands.Mature(3)==1,"30dayeligibility");
 Set("iPolDemand",4);PoliticalDemands.Resolve("refuse");Check(PoliticalSystem.Owed(3)==1&&!PoliticalSystem.Passive(3)&&!PoliticalSystem.CanAid(3),"refusalretainsdebtblocksall");
 Check(I("iPolBlocked3")==PoliticalSystem.Day+15,"15dayban");Set("iPolBlocked3",PoliticalSystem.Day);Set("iPolLast3",PoliticalSystem.Day-7);Check(PoliticalSystem.Passive(3)&&PoliticalSystem.CanAid(3),"exactexpiry");
 Set("iPolDemand",4);Set("iMoney",3499);PoliticalDemands.Resolve("pay");Check(PoliticalDemands.Active==3&&PoliticalSystem.Owed(3)==1,"cannotpayinsufficient");
 Set("iMoney",3500);PoliticalDemands.Resolve("pay");Check(I("iMoney")==0&&PoliticalSystem.Owed(3)==0&&PoliticalDemands.Active==-1,"repaymentclearsone");
 Set("iBloc0",40);Set("iBloc1",20);Set("iBloc2",20);Set("iBloc3",20);Set("iPolDriftTurn",0);int[] before=Enumerable.Range(0,4).Select(PoliticalSystem.Share).ToArray();string drift=PoliticalSystem.AdvancePolitics();int gain=Enumerable.Range(0,4).Select(i=>PoliticalSystem.Share(i)-before[i]).Max();Check(gain>=1&&gain<=10&&Enumerable.Range(0,4).Sum(PoliticalSystem.Share)==100,"driftconserves100");Check(PoliticalSystem.AdvancePolitics()=="","driftonceperturn");
 VariableSingleton.ChangeBool(CourtRoster.Keys[0],true);PoliticalSystem.SetAllegiance(0,2);Set("iBanishmentUsed",0);Check(PoliticalSystem.Banish(1,out _)&&!CourtRoster.InService(0),"alignedcharacterbanished");
 VariableSingleton.Instance.Save();int share=PoliticalSystem.Share(0);Set("iBloc0",1);VariableSingleton.Instance.Load();Check(PoliticalSystem.Share(0)==share&&I("iPolBlocked3")==PoliticalSystem.Day,"savedpoliticalstate");
 }catch(Exception e){failures.Add(e.ToString());}
 Debug.Log("POLITICS_SMOKE checks="+checks+" failures="+failures.Count+"\n"+string.Join("\n",failures));SessionState.SetBool("PoliticsSmoke",false);EditorApplication.Exit(failures.Count==0?0:1);
 }
}
