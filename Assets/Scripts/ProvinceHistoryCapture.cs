#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public sealed class ProvinceHistoryCapture:MonoBehaviour
{
 string folder;int checks;List<string> errors=new List<string>();
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-province-history-capture"))return;var go=new GameObject("Province history checks");DontDestroyOnLoad(go);go.AddComponent<ProvinceHistoryCapture>();}
 void Check(bool yes,string label){if(!yes)throw new Exception(label);checks++;}
 void Log(string value,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(value+"\n"+stack);}
 IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/ProvinceHistory"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");Application.logMessageReceived+=Log;var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+checks+"\n"+ex+"\n"+string.Join("\n",errors));Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),checks+" province history/map checks passed");Application.Quit(0);}
 IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();ProvinceCampaign.Ensure();
 var foreign=WarTheatre.Find("candar_realm");var inland=WarTheatre.Find("candar");Check(foreign.kind=="ally"&&inland.kind=="province","separate identities");Check(WarTheatre.ProvinceIds.Length==11&&WarTheatre.Data.regions.Count(r=>r.kind=="province")==11,"eleven simulated provinces retained");Check(TerritoryGraphic.Contains(foreign.front,foreign.points),"Sinop marker is inside Candar land");Check(!TerritoryGraphic.Contains(foreign.front,inland.points),"Sinop not also Ottoman Paphlagonia");Check(TerritoryGraphic.Contains(foreign.label,foreign.points),"Candar label inside territory");Check(TerritoryGraphic.Contains(inland.label,inland.points),"inland label inside Paphlagonia");
 int money=CouncilActions.Int("iMoney");string rng=JsonUtility.ToJson(UnityEngine.Random.state);
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
 Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
 foreach(var id in WarTheatre.ProvinceIds){var entry=ProvinceHistoryUI.Find(id);Check(entry!=null&&entry.paragraphs.Length==3&&entry.sources.Length>0,"three sourced paragraphs "+id);ProvincialConflictUI.Show(id);yield return null;
 GameObject.Find("Province history button").GetComponent<Button>().onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();var history=GameObject.Find("Province history");Check(history!=null,"History opens "+id);foreach(var t in history.GetComponentsInChildren<TMP_Text>())Check(!t.isTextOverflowing,"history text fits "+id+": "+t.text.Substring(0,Math.Min(t.text.Length,35)));Check(CouncilActions.Int("iMoney")==money&&JsonUtility.ToJson(UnityEngine.Random.state)==rng,"history leaves money and RNG unchanged");
 if(id=="armeniakon"||id=="candar"){ScreenCapture.CaptureScreenshot(Path.Combine(folder,id+"-"+size.x+".png"));yield return new WaitForSecondsRealtime(.15f);}
 if(size.x==1280)ControlSingleton.Instance.GoBack();else GameObject.Find("History return").GetComponent<Button>().onClick.Invoke();yield return null;Check(GameObject.Find("Province history")==null&&ProvincialConflictUI.Open,"history returns to existing inspection "+id);Check(ProvincialConflictUI.Back(),"inspection returns to map");yield return null;WarTheatreUI.Close();}
 WarTheatreUI.Show();yield return null;Canvas.ForceUpdateCanvases();var marker=GameObject.Find("Landmark · Sinop · Candar").GetComponent<RectTransform>();Check(marker.anchorMin==foreign.front,"marker uses authoritative land anchor");
 var shape=GameObject.Find("candar_realm").GetComponent<TerritoryGraphic>();shape.SetHighlighted(true);var map=GameObject.Find("Geographic base").GetComponent<RectTransform>();map.sizeDelta*=3;map.anchoredPosition=Vector2.Scale(new Vector2(.5f,.5f)-foreign.label,map.sizeDelta);yield return null;Canvas.ForceUpdateCanvases();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"candar-map-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);
 var point=RectTransformUtility.WorldToScreenPoint(null,marker.TransformPoint(marker.rect.center));var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);Check(hits.Count>0&&hits[0].gameObject.name=="Landmark · Sinop · Candar","Sinop land marker receives click");
 shape.Click();yield return null;Check(GameObject.Find("Candar negotiations")!=null,"Candar territory opens existing court");FindObjectOfType<CouncilUI>(true).Hide();yield return null;
 }
 Check(errors.Count==0,"no runtime errors: "+string.Join(";",errors));
 }
}
#endif
