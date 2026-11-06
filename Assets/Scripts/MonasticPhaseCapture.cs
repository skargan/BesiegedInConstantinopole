#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

// Explicit isolated-player test for the preparation production gate and its UI.
public sealed class MonasticPhaseCapture:MonoBehaviour
{
    string folder;int checks;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot(){if(!Environment.GetCommandLineArgs().Contains("-monastic-phase-capture"))return;Application.runInBackground=true;string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/MonasticPhase"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var go=new GameObject("Monastic phase checks");DontDestroyOnLoad(go);go.AddComponent<MonasticPhaseCapture>().folder=folder;}
    void Check(bool value,string note){if(!value)throw new Exception(note);checks++;}
    IEnumerator Start(){AudioListener.volume=0;File.WriteAllText(Path.Combine(folder,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());while(stack.Count>0){object next=null;bool more=false;Exception error=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){error=e;}if(error!=null){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+checks+"\n"+error);Application.Quit(1);yield break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),checks+" phase/UI checks passed; MonasticResearchChecks included");Application.Quit(0);}
    IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(1);CampaignHUD.Instance.DismissReport();Check(VariableSingleton.IsInitialized,"campaign ready");Check(MonasticResearchChecks.Run()>0,"dated production/save/medical regression");
        Check(!MonasticResearch.ProductionEnabled&&MonasticResearch.DailyProgress==0,"new campaign production paused");Check(MonasticResearch.ResearchMonks==50&&MonasticResearch.InfirmaryMonks==50,"allocation remains fifty-fifty");
        var original=VariableSingleton.Instance.CaptureState();int secrets=DailyLifeEvents.I("iSecrets");
        while(!VariableSingleton.HasSiegeStarted()){VariableSingleton.IncrementTurn();if(!VariableSingleton.HasSiegeStarted())Check(MonasticResearch.Progress==0&&DailyLifeEvents.I("iSecrets")==secrets,"actual preparation turn produces no research");}
        Check(MonasticResearch.Progress==50&&DailyLifeEvents.I("iSecrets")==secrets,"first siege boundary adds one day, no preparation windfall");
        VariableSingleton.Instance.RestoreValidated(original);var council=CouncilUI.For(FindObjectOfType<ManagerUI>(true));
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);council.MonasticResearchView();yield return null;Canvas.ForceUpdateCanvases();var desk=GameObject.Find("Infirmary desk");Check(desk.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Research begins with the siege")),"pause explained "+size.x);Check(!desk.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Next discovery in")),"no misleading ETA "+size.x);foreach(var t in desk.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"research text fits "+size.x+" "+t.text);}ScreenCapture.CaptureScreenshot(Path.Combine(folder,"paused-research-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);council.Hide();}
    }
}
#endif
