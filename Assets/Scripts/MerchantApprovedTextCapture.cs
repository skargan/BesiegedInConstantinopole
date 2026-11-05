#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class MerchantApprovedTextCapture:MonoBehaviour
{
 string folder;int checks;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-merchant-approved-text-capture"))return;var go=new GameObject("Approved merchant text checks");DontDestroyOnLoad(go);go.AddComponent<MerchantApprovedTextCapture>();}
 void Check(bool yes,string message){if(!yes)throw new Exception(message);checks++;}
 IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/MerchantApproved"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+checks+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),checks+" approved merchant text checks passed");Application.Quit(0);}
 IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();var c=FindObjectOfType<CouncilUI>(true);var lines=JsonUtility.FromJson<MarketMerchant.Lines>(Resources.Load<TextAsset>("Content/MarketMerchant").text).remarks;
 Check(lines.Length==61&&lines.Distinct().Count()==61,"61 unique approved remarks");Check(lines.Contains("The finest minds in both churches have argued about reunion for years. My mother is of an opinion that they will not resolve this issue without consulting her."),"exact user reunion wording");Check(lines.Contains("A pickpocket tried to rob me yesterday. He looked in my purse and changed his mind."),"approved shortened punchline");Check(lines.Count(x=>x.Contains("\n"))==3,"three quote/source then comment entries");
 int money=CouncilActions.Int("iMoney");string rng=JsonUtility.ToJson(UnityEngine.Random.state);
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);c.Market();yield return null;var merchant=GameObject.Find("Council screen").GetComponentInChildren<MarketMerchant>();Check(merchant!=null&&MarketMerchant.RemarkCount==61,"pool loaded through merchant");merchant.Speak();var words=(TMP_Text)typeof(MarketMerchant).GetField("words",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(merchant);Canvas.ForceUpdateCanvases();
 foreach(string line in lines.Concat(new[]{CharacterChatter.MerchantFarewell})){words.text=line;words.ForceMeshUpdate();Check(!words.isTextOverflowing,"remark fits "+Array.IndexOf(lines,line));if(line.Contains("\n")){int end=line.IndexOf('\n')-1;Check(words.textInfo.characterInfo[end].lineNumber==0,"quote and source fit first line");}}
 foreach(string text in new[]{lines[4],lines.Last()}){words.text=text;words.ForceMeshUpdate();ScreenCapture.CaptureScreenshot(Path.Combine(folder,(text==lines.Last()?"quote-":"reunion-")+size.x+".png"));yield return new WaitForSecondsRealtime(.15f);}
 Check(CouncilActions.Int("iMoney")==money&&JsonUtility.ToJson(UnityEngine.Random.state)==rng,"presentation leaves treasury and RNG unchanged");c.Hide();yield return null;}
 }
}
#endif
