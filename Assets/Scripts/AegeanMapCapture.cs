#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
public sealed class AegeanMapCapture:MonoBehaviour {
    int passed;string output;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-aegean-capture"))return;
        var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Aegean"));Directory.CreateDirectory(folder);
        SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Aegean map verification");DontDestroyOnLoad(go);go.AddComponent<AegeanMapCapture>().output=folder;}
    void Check(bool value,string label){if(!value)throw new Exception(label);passed++;}
    IEnumerator Start(){var run=Run();while(true){object next=null;bool moved;try{moved=run.MoveNext();if(moved)next=run.Current;}catch(Exception e){File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED "+passed+" "+e);Debug.LogException(e);Application.Quit(1);yield break;}if(!moved)break;yield return next;}
        File.WriteAllText(Path.Combine(output,"checks.txt"),"AEGEAN_CHECKS "+passed+" passed");Application.Quit(0);}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+25;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"campaign");yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();
        foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
        Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.5f);WarTheatreUI.Show();yield return null;Canvas.ForceUpdateCanvases();
        WarTheatre.Validate(WarTheatre.Data);Check(WarTheatre.Data.regions.Count(r=>r.kind=="province")==11,"unchanged province economy");
        var ui=GameObject.Find("War Beyond the Walls map").GetComponent<WarTheatreUI>();var flags=BindingFlags.NonPublic|BindingFlags.Instance;
        typeof(WarTheatreUI).GetMethod("Resize",flags).Invoke(ui,new object[]{3f});
        var map=(RectTransform)typeof(WarTheatreUI).GetField("map",flags).GetValue(ui);map.anchoredPosition=new Vector2((.5f-.365f)*map.rect.width,(.5f-.245f)*map.rect.height);Canvas.ForceUpdateCanvases();yield return null;
        var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
        foreach(var r in WarTheatre.Data.regions.Where(r=>r.id.StartsWith("aegean_")||new[]{"chios","lesbos","negroponte","naxos","crete","kythera","hospitallers"}.Contains(r.id))){
            var shape=GameObject.Find(r.id).GetComponent<TerritoryGraphic>();Check(shape!=null,r.id+" rendered");
            var mesh=shape.canvasRenderer.GetMesh();Check(mesh!=null&&mesh.triangles.Length>0,r.id+" mesh");
            shape.OnPointerEnter(pointer);Check(shape.Highlighted,r.id+" hover");Check(GameObject.Find(WarTheatre.DiplomaticPower(r).id).GetComponent<TerritoryGraphic>().Highlighted,r.id+" diplomatic grouping");
            shape.OnPointerExit(pointer);Check(!shape.Highlighted,r.id+" clears");
            shape.OnPointerClick(pointer);Check(GameObject.Find("War Beyond the Walls map")!=null,r.id+" selection retains map");
        }
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"aegean-map.png"));yield return new WaitForSecondsRealtime(.4f);
        Check(WarTheatre.Find("aegean_75").group=="venice"&&WarTheatre.Find("aegean_80").group=="venice","Cornaro islands");
        Check(WarTheatre.Find("aegean_51").group=="hospitallers","Kos");Check(WarTheatre.Find("aegean_30").group=="genoa","Samos");
    }
}
#endif
