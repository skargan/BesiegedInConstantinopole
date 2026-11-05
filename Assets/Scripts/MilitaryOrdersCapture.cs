#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class MilitaryOrdersCapture:MonoBehaviour
{
 int checks;string output;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-military-orders-capture"))return;var go=new GameObject("Military orders verification");DontDestroyOnLoad(go);go.AddComponent<MilitaryOrdersCapture>();}
 void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
 IEnumerator Start(){output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/MilitaryOrders"));Directory.CreateDirectory(output);SaveGames.TestRoot=Path.Combine(output,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED "+checks+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(output,"checks.txt"),checks+" military order checks passed");Application.Quit(0);}
 Button Find(string text)=>FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text==text);
 IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();var manager=FindObjectOfType<ManagerUI>(true);var council=CouncilUI.For(manager);VariableSingleton.SetInt("iMoney",1000000);VariableSingleton.SetInt("iMaterials",5000);var baseline=VariableSingleton.Instance.CaptureState();string[] names={"Infantry","Archers","Cavalry","Varangian Guard","Cannons","Warships"};
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);for(int i=0;i<6;i++){VariableSingleton.Instance.RestoreValidated(baseline);council.Hide();manager.BringUpTroopsUI();yield return null;FindObjectsOfType<Button>().First(b=>b.name=="Select military unit "+i).onClick.Invoke();yield return null;
 if(i==5){Check(GameObject.Find("Shipyard screen")!=null,"warships open shipyard");Find("Build warships").onClick.Invoke();}else Find((i<4?"Train ":"Build ")+names[i]).onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();Check(GameObject.Find("Military order screen")!=null,"order visible "+i);Check(!FindObjectsOfType<MilitarySelectionDesk>().Any(),"desk cannot cover order "+i);foreach(var t in GameObject.Find("Military order screen").GetComponentsInChildren<TMP_Text>())Check(!t.isTextOverflowing,"text fits "+i+" "+t.text);Check(TrainingPool.Orders.Count==0,"preview creates no order");int money=DailyLifeEvents.I("iMoney");ScreenCapture.CaptureScreenshot(Path.Combine(output,"order-"+i+"-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);Find("Cancel").onClick.Invoke();yield return null;Check(DailyLifeEvents.I("iMoney")==money&&TrainingPool.Orders.Count==0,"cancel free "+i);
 council.RecruitmentOrder(i,()=>manager.BringUpTroopsUI());yield return null;Check(Find("Confirm order").interactable,"one unit affordable "+i);Find("Confirm order").onClick.Invoke();yield return null;Check(TrainingPool.Orders.Count==1&&TrainingPool.Orders[0].quantity==1,"one exact queued unit "+i);Check(DailyLifeEvents.I("iMoney")<money,"payment applied "+i);CampaignHUD.Instance.DismissReport();}
 VariableSingleton.Instance.RestoreValidated(baseline);council.Shipyard();yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(output,"shipyard-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);ControlSingleton.Instance.GoBack();yield return null;}
 }
}
#endif
