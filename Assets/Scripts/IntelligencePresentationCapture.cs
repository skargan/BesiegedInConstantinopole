#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class IntelligencePresentationCapture:MonoBehaviour
{
 int checks;string folder;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-intelligence-presentation-capture"))return;var go=new GameObject("Intelligence presentation checks");DontDestroyOnLoad(go);go.AddComponent<IntelligencePresentationCapture>();}
 void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;}
 IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/IntelligencePresentation"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+checks+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),checks+" intelligence presentation checks passed");Application.Quit(0);}
 IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();var c=FindObjectOfType<CouncilUI>(true);VariableSingleton.SetInt("iTurkSpies",77);VariableSingleton.SetInt("iCampAlert",70);
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);foreach(int agents in new[]{2,8,10}){VariableSingleton.SetInt("iSpies",agents);c.Spies();yield return null;Canvas.ForceUpdateCanvases();var root=GameObject.Find("Council screen");var network=GameObject.Find("Intelligence network");Check(network.transform.childCount>=7,"six framed statistics");string all=string.Join(" ",network.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));Check(!all.Contains("GOLD")&&!all.Contains("Falls")&&!all.Contains("revealed"),"notes and duplicate gold absent");Check(all.Contains(agents<8?"Enemy spies · ?":"Enemy spies · 77"),"enemy spy gate "+agents);foreach(var label in root.GetComponentsInChildren<TMP_Text>())Check(!label.isTextOverflowing,"text fits "+label.text);
 var locked=GameObject.Find("Select intelligence report 9").GetComponent<CampaignChoiceTooltip>();locked.OnPointerEnter(null);yield return null;var tip=GameObject.Find("Campaign choice explanation");Check(tip!=null,"tier hover shown");if(agents<10)Check(tip.GetComponentInChildren<TMP_Text>().text.Contains("10 agents"),"requirement in hover");locked.OnPointerExit(null);yield return null;Check(GameObject.Find("Campaign choice explanation")==null,"hover clears");
 var alert=GameObject.Find("Ottoman camp alert").GetComponent<CampaignChoiceTooltip>();alert.OnPointerEnter(null);yield return null;tip=GameObject.Find("Campaign choice explanation");Check(tip.GetComponentInChildren<TMP_Text>().text.Contains("risk")&&tip.GetComponentInChildren<TMP_Text>().text.Contains("1–10"),"alert hover explains effect and decay");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"alert-hover-"+size.x+"-"+agents+".png"));yield return new WaitForSecondsRealtime(.2f);alert.OnPointerExit(null);yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"reports-"+size.x+"-"+agents+".png"));yield return new WaitForSecondsRealtime(.2f);
 var button=GameObject.Find("Select intelligence report 8").GetComponent<Button>();button.onClick.Invoke();yield return null;Check(!GameObject.Find("Selected intelligence report").GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Requires")),"locked requirement not printed in report");ControlSingleton.Instance.GoBack();yield return null;Check(GameObject.Find("Campaign choice explanation")==null,"navigation clears tooltip");}}
 }
}
#endif
