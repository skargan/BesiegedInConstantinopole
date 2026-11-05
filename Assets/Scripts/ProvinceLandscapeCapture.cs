#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class ProvinceLandscapeCapture:MonoBehaviour
{
 string folder;int passed;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-province-landscape-capture"))return;var path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/ProvinceLandscapes"));Directory.CreateDirectory(path);SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var go=new GameObject("Province landscape verification");DontDestroyOnLoad(go);go.AddComponent<ProvinceLandscapeCapture>().folder=path;}
 void Check(bool ok,string note){if(!ok)throw new Exception(note);passed++;}
 IEnumerator Start(){var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+passed+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),"PROVINCE_LANDSCAPE_CHECKS "+passed+" passed");Application.Quit(0);}
 IEnumerator Run(){Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();ProvinceCampaign.Ensure();
 string[] ids={"thrace","bulgaria","macedonia","bithynia","karasi","aydin","mentese","germiyan","anatolia","candar","armeniakon"};
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080)}){
 Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);
 foreach(string id in ids){ProvincialConflictUI.Show(id);yield return null;Canvas.ForceUpdateCanvases();var root=GameObject.Find("Provincial command");Check(root.transform.Find("Scene artwork").GetComponent<RawImage>().texture==Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/Provinces/"+id),"correct art "+id);foreach(var t in root.GetComponentsInChildren<TMP_Text>())Check(!t.isTextOverflowing,"overflow "+id+": "+t.text);if(id=="thrace"||id=="mentese"){ScreenCapture.CaptureScreenshot(Path.Combine(folder,id+"-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);}Check(ProvincialConflictUI.Back(),"return handled");yield return null;Check(!ProvincialConflictUI.Open,"inspection closed");}
 }
 typeof(VariableSingleton).GetField("turn",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn);
 var p=ProvincialWar.Find("thrace");p.Resistance=45;ProvinceCampaign.Set("Agent_thrace",2);ProvincialWar.Launch(p.Id,out _);p.Rebels.Troops=4000;
 ProvincialConflictUI.Show("thrace");yield return null;Canvas.ForceUpdateCanvases();foreach(var t in GameObject.Find("Provincial command").GetComponentsInChildren<TMP_Text>())Check(!t.isTextOverflowing,"army controls fit "+t.text);
 ScreenCapture.CaptureScreenshot(Path.Combine(folder,"thrace-army.png"));yield return new WaitForSecondsRealtime(.3f);
 }
}
#endif
