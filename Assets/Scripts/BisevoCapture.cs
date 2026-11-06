#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public sealed class BisevoCapture:MonoBehaviour {
string output; readonly List<string> results=new List<string>();
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-bisevo-capture"))return;
string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Bisevo"));Directory.CreateDirectory(folder);
SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");Application.runInBackground=true;
var go=new GameObject("Bisevo verification");DontDestroyOnLoad(go);go.AddComponent<BisevoCapture>().output=folder;}
void Check(bool ok,string text){if(!ok)throw new Exception(text);results.Add("PASS "+text);}
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+results.Count+" checks\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"BISEVO_CHECKS "+results.Count+" passed");Application.Quit(0);}
IEnumerator Run(){AudioListener.volume=0;yield return null;SaveMenu.StartCampaign(false);
float deadline=Time.realtimeSinceStartup+20;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();
// Exercise the island through the live map raycaster, not a direct scene shortcut.
WarTheatreUI.Show();yield return null;Canvas.ForceUpdateCanvases();
var island=GameObject.Find("bisevo").GetComponent<TerritoryGraphic>();
var rect=island.rectTransform.rect;var uv=WarTheatre.Find("bisevo").label;
var screen=(Vector2)island.rectTransform.TransformPoint(new Vector3(rect.xMin+uv.x*rect.width,rect.yMin+uv.y*rect.height,0));
var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=screen,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,hits);
Check(hits.Count>0&&hits[0].gameObject==island.gameObject,"Biševo is the first live map hit at the painted island");
UnityEngine.EventSystems.ExecuteEvents.Execute(hits[0].gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);yield return null;
Check(BisevoUI.IsOpen,"map pointer click opens Biševo");BisevoUI.Back();yield return null;
var theatre=GameObject.Find("War Beyond the Walls map").GetComponent<WarTheatreUI>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
typeof(WarTheatreUI).GetMethod("Resize",flags).Invoke(theatre,new object[]{3f});
var map=(RectTransform)typeof(WarTheatreUI).GetField("map",flags).GetValue(theatre);
map.anchoredPosition=new Vector2((.5f-.17f)*map.rect.width,(.5f-.74f)*map.rect.height);
yield return Capture("adriatic-map");
var venice=GameObject.Find("brac").GetComponent<TerritoryGraphic>();venice.OnPointerEnter(pointer);yield return Capture("adriatic-venetian-highlight");
Check(GameObject.Find("hvar").GetComponent<TerritoryGraphic>().Highlighted,"Venetian islands highlight together");
Check(WarTheatre.Find("croatia")==null,"Hungarian internal seam removed");WarTheatreUI.Close();yield return null;

foreach(bool envoy in new[]{false,true})foreach(bool pay in new[]{false,true}) {
VariableSingleton.SetInt(BisevoDiscovery.EncounterKey,0);VariableSingleton.SetInt("iEnvoys",envoy?2:0);VariableSingleton.SetInt("iShips",3);VariableSingleton.SetInt("iMoney",6000);
Check(BisevoDiscovery.Enter(),"enter "+envoy+pay);int state=envoy?1:2;Check(BisevoDiscovery.State==state,"correct hostage");
Check(DailyLifeEvents.I("iEnvoys")== (envoy?1:0),"reserve only available envoy");Check(BisevoDiscovery.Enter(),"reopen pending");Check(DailyLifeEvents.I("iEnvoys")== (envoy?1:0),"no duplicate reservation");
var saved=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));VariableSingleton.SetInt(BisevoDiscovery.EncounterKey,0);VariableSingleton.Instance.RestoreValidated(saved);Check(BisevoDiscovery.State==state,"pending hostage survives save roundtrip");
Check(BisevoDiscovery.Resolve(pay),"resolve");Check(!BisevoDiscovery.Resolve(pay),"cannot duplicate resolution");
Check(DailyLifeEvents.I("iMoney")== (pay?1000:6000),"correct payment");Check(DailyLifeEvents.I("iEnvoys")== (envoy?(pay?2:1):0),"envoy result");Check(DailyLifeEvents.I("iShips")==(!envoy&&!pay?2:3),"ship loss only without envoy");
int final=BisevoDiscovery.State;Check(BisevoDiscovery.Enter()&&BisevoDiscovery.State==final,"repeat monastery stays resolved");
}
VariableSingleton.SetInt(BisevoDiscovery.EncounterKey,0);VariableSingleton.SetInt("iShips",0);Check(!BisevoDiscovery.Enter(),"no warship blocks entry even with envoy");VariableSingleton.SetInt("iShips",1);VariableSingleton.SetInt("iMoney",4999);Check(BisevoDiscovery.Enter(),"can be ambushed without ransom gold");Check(!BisevoDiscovery.Resolve(true)&&BisevoDiscovery.Pending,"insufficient funds preserve encounter");BisevoDiscovery.Resolve(false);
VariableSingleton.SetInt(BisevoDiscovery.SecretKey,0);int secrets=DailyLifeEvents.I("iSecrets");Check(BisevoDiscovery.Discover(),"sanctuary discovery");Check(!BisevoDiscovery.Discover()&&DailyLifeEvents.I("iSecrets")==secrets+1,"once-only secret");
var finalSave=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));VariableSingleton.SetInt(BisevoDiscovery.SecretKey,0);VariableSingleton.Instance.RestoreValidated(finalSave);Check(BisevoDiscovery.Discovered&&!BisevoDiscovery.Discover(),"reward persistence");
finalSave.ints.RemoveAll(p=>p.key.StartsWith("iBisevo"));VariableSingleton.Instance.RestoreValidated(finalSave);Check(BisevoDiscovery.State==0&&!BisevoDiscovery.Discovered,"older saves cannot inherit discoveries");
VariableSingleton.SetInt(BisevoDiscovery.EncounterKey,1);SaveGames.PendingResume=null;
new GameObject("Verify pending encounter resume").AddComponent<SaveResume>();
yield return null;yield return null;yield return null;yield return null;
Check(BisevoUI.IsOpen,"scene resume reopens pending ransom");
Check(GameObject.Find("Biševo visit").GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Pay 5,000 gold"),"resumed screen exposes ransom choice");
BisevoDiscovery.Resolve(false);BisevoUI.Back();BisevoUI.Back();yield return null;
foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}) {
Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);VariableSingleton.SetInt("iShips",3);VariableSingleton.SetInt("iEnvoys",1);VariableSingleton.SetInt("iMoney",6000);VariableSingleton.SetInt(BisevoDiscovery.EncounterKey,0);BisevoUI.Show();yield return null;
yield return Capture("landing-"+size.x);
foreach(var label in new[]{"The old sanctuary","A narrow sea grotto","Fresh footprints"}) {
var hotspot=GameObject.Find("Biševo visit").GetComponentsInChildren<LocationHotspot>().Single(h=>h.name.Contains(label));hotspot.Action.Invoke();yield return null;
Check(BisevoUI.IsOpen,"destination opens "+label);yield return Capture(label=="The old sanctuary"?"sanctuary-"+size.x:label=="A narrow sea grotto"?"cave-"+size.x:"pirates-"+size.x);
foreach(var text in GameObject.Find("Biševo visit").GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"text fits "+text.text);}
if(BisevoDiscovery.Pending){Check(BisevoUI.Back()&&BisevoDiscovery.Pending,"back cannot abandon ransom");BisevoDiscovery.Resolve(true);}
BisevoUI.Back();yield return null;
}
BisevoUI.Back();Check(!BisevoUI.IsOpen,"return to map");}
}
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif

