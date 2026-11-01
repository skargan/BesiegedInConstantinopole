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
 Set("iMoney",10000);
 for(int bloc=0;bloc<4;bloc++){Set("iBloc"+bloc,100);Set("iBlocBan"+bloc,1);Set("iPolBlocked"+bloc,PoliticalSystem.Day+100);Set("iPolOwed"+bloc,4);Set("iPolIssued"+bloc,4);}
 Set("iPolDemand",4);Set("iPolDebtDay3_1",PoliticalSystem.Day-100);Set("iAllegiance0",2);
 Check(Enumerable.Range(0,4).All(i=>!PoliticalSystem.Passive(i)&&!PoliticalSystem.Banned(i)&&!PoliticalSystem.CanAid(i)&&PoliticalSystem.Share(i)==0&&PoliticalSystem.Owed(i)==0),"legacyfactionsinert");
 Check(TrainingPool.Duration("iArchers")==10&&TrainingPool.Duration("iShips")==30,"basetrainingdurations");
 Check(SupplyEconomy.FoodAfterPolitics(100)==100&&SupplyEconomy.FoodAfterPolitics(101)==101,"noretiredfooddiscount");
 Check(SupplyEconomy.Cost("iMaterials",100)==1000&&SupplyEconomy.Cost("iMaterials",1)==10,"noretiredmarketdiscount");
 int chance=CouncilActions.AidChance(1);Set("iBloc2",0);Check(CouncilActions.AidChance(1)==chance,"noretiredaidbonus");
 string snapshot=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
 foreach(int bloc in Enumerable.Range(0,4)){PoliticalSystem.Gain(bloc,10);PoliticalSystem.SetAllegiance(0,bloc+1);Check(!PoliticalSystem.Aid(bloc,out _)&&!PoliticalSystem.Banish(bloc,out _),"retiredordersunavailable");}
 Check(PoliticalSystem.AdvancePolitics()=="","noretireddrift");PoliticalDemands.Schedule();foreach(string option in new[]{"pay","concede","empower","refuse"})PoliticalDemands.Resolve(option);
 Check(snapshot==JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()),"compatibilitycallschangeNeitherStateNorRng");
 Check(PoliticalDemands.Active==-1&&PoliticalDemands.PendingNode==null&&PoliticalDemands.Mature(3)==0,"nolegacydemandloop");
 VariableSingleton.ChangeBool(CourtRoster.Keys[0],false);VariableSingleton.ChangeBool(CourtRoster.Keys[0],true);Check(CourtRoster.InService(0),"legacybanallowsintroduction");
 var before=VariableSingleton.Instance.CaptureState();float[] civic=CouncilActions.FactionKeys.Select(VariableSingleton.GetFloatVariable).ToArray();
 VariableSingleton.Instance.RestoreValidated(before);
 Check(VariableSingleton.Instance.CaptureState().ints.Where(v=>v.key.StartsWith("iPol")||v.key.StartsWith("iBloc")||v.key.StartsWith("iAllegiance")||v.key=="iBanishmentUsed").All(v=>v.value==0),"restoreclearsretiredfactions");
 Check(civic.SequenceEqual(CouncilActions.FactionKeys.Select(VariableSingleton.GetFloatVariable))&&I("iMoney")==10000&&CourtRoster.InService(0),"restorepreservescivicmoneyandcourt");
 }catch(Exception e){failures.Add(e.ToString());}
 Debug.Log("POLITICS_SMOKE checks="+checks+" failures="+failures.Count+"\n"+string.Join("\n",failures));SessionState.SetBool("PoliticsSmoke",false);EditorApplication.Exit(failures.Count==0?0:1);
 }
}
