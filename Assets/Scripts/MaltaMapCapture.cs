#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public sealed class MaltaMapCapture:MonoBehaviour
{
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-malta-map-capture"))return;var go=new GameObject("Malta marker checks");DontDestroyOnLoad(go);go.AddComponent<MaltaMapCapture>();}
 string folder;int count;
 void Check(bool ok,string text){if(!ok)throw new Exception(text);count++;}
 IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/MaltaMap"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var run=Run();File.WriteAllText(Path.Combine(folder,"checks.txt"),"RUNNING");while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+count+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),count+" Malta marker checks passed");Application.Quit(0);}
 IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
 Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);WarTheatreUI.Show();yield return null;yield return null;Check(Screen.width==size.x&&Screen.height==size.y,"exact resolution");
 var marker=GameObject.Find("Landmark · Malta · Mercenary brokers").GetComponent<RectTransform>();Check(marker.anchorMin==MercenaryMapMarkers.Malta&&marker.anchorMax==MercenaryMapMarkers.Malta,"shared island anchor");
 var map=GameObject.Find("Geographic base").GetComponent<RectTransform>();map.sizeDelta*=3;map.anchoredPosition=Vector2.Scale(new Vector2(.5f,.5f)-MercenaryMapMarkers.Malta,map.sizeDelta);yield return null;Canvas.ForceUpdateCanvases();
 var p=RectTransformUtility.WorldToScreenPoint(null,marker.TransformPoint(marker.rect.center));var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},hits);Check(hits.Count>0&&hits[0].gameObject==marker.gameObject,"zoomed Malta receives pointer");
 ScreenCapture.CaptureScreenshot(Path.Combine(folder,"malta-island-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);
 marker.GetComponent<Button>().onClick.Invoke();yield return null;Check(GameObject.Find("Mercenary brokers")!=null,"Malta opens marketplace");Check(MercenaryUI.Close(),"marketplace returns to map");yield return null;Check(GameObject.Find("War Beyond the Walls map")!=null,"map retained");
 int company=Array.FindIndex(MercenaryRoster.All,c=>c.Port=="Malta");MercenaryContracts.Put(company,"Travel",0);Check(Vector2.Distance(MercenaryMapMarkers.Position(company),MercenaryMapMarkers.Malta)<.00001f,"Malta voyage starts at island");WarTheatreUI.Close();yield return null;
 }
 }
}
#endif
