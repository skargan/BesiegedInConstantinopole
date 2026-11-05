#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class LandmarkCapture:MonoBehaviour
{
    string folder;int passed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-landmark-capture"))return;var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Landmarks"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");var go=new GameObject("Landmark verification");DontDestroyOnLoad(go);go.AddComponent<LandmarkCapture>().folder=folder;}
    void Check(bool ok,string note){if(!ok)throw new Exception(note);passed++;}
    IEnumerator Start(){var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+passed+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),"LANDMARK_CHECKS "+passed+" passed");Application.Quit(0);}
    IEnumerator Run(){Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();
        Check(SaveGames.Automated,"isolated saves");VariableSingleton.SetInt(MapLandmarks.OutcomeKey,0);Check(!MapLandmarks.FortressVisible,"absent before construction");
        VariableSingleton.SetInt(MapLandmarks.OutcomeKey,1);Check(MapLandmarks.FortressVisible&&MapLandmarks.Building,"construction revealed");
        foreach(int state in new[]{1,2,3,4}){VariableSingleton.SetInt(MapLandmarks.OutcomeKey,state);var saved=VariableSingleton.Instance.CaptureState();VariableSingleton.SetInt(MapLandmarks.OutcomeKey,0);VariableSingleton.Instance.RestoreValidated(saved);Check(MapLandmarks.Outcome==state,"saved outcome "+state);}
        Check(CountryDiplomacy.Index("ragusa")>=0&&CountryDiplomacy.Index("papacy")>=0,"diplomacy destinations");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);WarTheatreUI.Show();yield return null;Canvas.ForceUpdateCanvases();
            foreach(string name in new[]{"Constantinople","Ragusa","Rome","Strait-Cutter Fortress"})Check(GameObject.Find("Landmark · "+name)?.GetComponent<Button>()!=null,"marker "+name);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"map-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);
            foreach(int state in new[]{1,2,3,4}){VariableSingleton.SetInt(MapLandmarks.OutcomeKey,state);MapLandmarks.ShowFortress();yield return null;Canvas.ForceUpdateCanvases();var root=GameObject.Find("Rumeli fortress view");Check(root!=null,"view opened");foreach(var text in root.GetComponentsInChildren<TMP_Text>())Check(!text.isTextOverflowing,"text fits "+text.text);Check(root.GetComponentsInChildren<RawImage>().All(x=>x.texture!=null),"art loaded");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"fortress-"+state+"-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);root.GetComponentInChildren<Button>().onClick.Invoke();Check(!root.activeSelf,"return closes view");Check(GameObject.Find("War Beyond the Walls map")!=null,"map preserved");yield return null;}
            WarTheatreUI.Close();yield return null;
        }
    }
}
#endif
