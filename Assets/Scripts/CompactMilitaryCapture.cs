#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CompactMilitaryCapture : MonoBehaviour
{
    string output;int passed;int oldWidth,oldHeight;FullScreenMode oldMode;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-compact-military-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/CompactMilitary"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");
        Directory.CreateDirectory(SaveGames.TestRoot);
        var go=new GameObject("Maritime verification");DontDestroyOnLoad(go);go.AddComponent<CompactMilitaryCapture>().output=path;
    }
    void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add("PASS "+name);passed++;}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    void Capture(string filename)
    {
        Canvas.ForceUpdateCanvases();var go=new GameObject("Maritime capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,filename),shot.EncodeToPNG());}
        finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);}
    }
    IEnumerator Start()
    {
        oldWidth=Screen.width;oldHeight=Screen.height;oldMode=Screen.fullScreenMode;
        var stack=new Stack<IEnumerator>();stack.Push(Run());Exception failure=null;
        while(stack.Count>0)
        {
            object next=null;bool moved=false;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null)break;
            if(!moved){stack.Pop();continue;}if(next is IEnumerator child)stack.Push(child);else yield return next;
        }
        // Do not leave a QA-only resolution behind for the next normal launch.
        Screen.SetResolution(oldWidth,oldHeight,oldMode);yield return new WaitForSecondsRealtime(.4f);
        File.WriteAllLines(Path.Combine(output,"details.txt"),checks);
        File.WriteAllText(Path.Combine(output,"checks.txt"),failure==null?"COMPACT_MILITARY_CHECKS "+passed+" passed":"FAILED "+failure);
        if(failure!=null)Debug.LogException(failure);Application.Quit(failure==null?0:1);
    }

    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;
        SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+25;
        while(!VariableSingleton.IsInitialized&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"Campaign initialised");yield return new WaitForSecondsRealtime(.6f);CampaignHUD.Instance.DismissReport();
        foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
        foreach(var gate in FindObjectsOfType<ProvincialConflictGate>())gate.enabled=false;
        var council=FindObjectOfType<CouncilUI>(true);var baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720),new Vector2Int(1920,1080)})
        {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);VariableSingleton.Instance.RestoreValidated(baseline);
            passed+=IntelligenceSpySummaryChecks.Run(council);foreach(int spies in new[]{7,8}){VariableSingleton.SetInt("iSpies",spies);council.Spies();yield return null;Capture("intelligence-"+spies+"-"+size.x+".png");}VariableSingleton.Instance.RestoreValidated(baseline);
            council.TroopUpgrades();yield return new WaitForSecondsRealtime(.8f);Canvas.ForceUpdateCanvases();
            Check(!FindObjectsOfType<ScrollRect>().Any(s=>s.isActiveAndEnabled),"Upgrades has no scrolling at "+size.x);
            Check(FindObjectsOfType<Button>().Count(b=>b.name.StartsWith("Review training "))==3,"All three training actions visible");
            Check(Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/troop-upgrades")!=null,"Troop artwork imported");
            Capture("upgrades-"+size.x+".png");CheckLayout();
            int money=CouncilActions.Int("iMoney");
            FindObjectsOfType<Button>().First(b=>b.name=="Review training 0").onClick.Invoke();yield return null;
            Check(CouncilActions.Int("iMoney")==money&&TroopTraining.Due(0)==0,"Review training does not spend or start drills");
            Check(FindObjectsOfType<Button>().Any(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Confirm training"),"Original training confirmation retained");
            ControlSingleton.Instance.ReturnAction();yield return null;
            Check(FindObjectsOfType<Button>().Count(b=>b.name.StartsWith("Review training "))==3,"Return restores complete upgrades screen");
            TroopTraining.Order(0,out _);council.TroopUpgrades();yield return null;
            Check(!FindObjectsOfType<Button>().First(b=>b.name=="Review training 0").interactable,"Pending training cannot be ordered twice");
            Capture("upgrades-pending-"+size.x+".png");
            VariableSingleton.Instance.RestoreValidated(baseline);council.Fleet();yield return new WaitForSecondsRealtime(.8f);
            Check(!FindObjectsOfType<ScrollRect>().Any(s=>s.isActiveAndEnabled),"Fleet overview has no scrolling");
            foreach(var label in new[]{"Change mission","Plan naval battle","Build warships","All shipyard orders"})Check(FindObjectsOfType<Button>().Any(b=>b.GetComponentInChildren<TMP_Text>()?.text==label),"Fleet action retained: "+label);
            Check(FindObjectsOfType<Button>().Any(b=>b.GetComponentInChildren<TMP_Text>()?.text.StartsWith("Repair ships")??false),"Repair remains available");
            Capture("fleet-"+size.x+".png");CheckLayout();
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text=="All shipyard orders").onClick.Invoke();yield return null;
            Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text=="No shipyard orders in progress."),"Shipyard details reachable");ControlSingleton.Instance.ReturnAction();yield return null;
            VariableSingleton.SetInt("iShips",0);VariableSingleton.SetInt("iSpies",0);council.Fleet();yield return null;
            Check(!FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Plan naval battle").interactable,"No fleet cannot launch battle");
            Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text=="Unknown"),"Enemy fleet hidden without intelligence");Capture("fleet-empty-"+size.x+".png");council.Hide();
        }
    }
    void CheckLayout()
    {
        Canvas.ForceUpdateCanvases();var pane=GameObject.Find("Command desk").GetComponent<RectTransform>();
        var corners=new Vector3[4];pane.GetWorldCorners(corners);var min=corners[0];var max=corners[2];
        foreach(var b in pane.GetComponentsInChildren<Button>())
        {
            var c=new Vector3[4];b.GetComponent<RectTransform>().GetWorldCorners(c);
            Check(c[0].x>=min.x-1&&c[0].y>=min.y-1&&c[2].x<=max.x+1&&c[2].y<=max.y+1,"Button inside command panel: "+b.name);
        }
        foreach(var t in pane.GetComponentsInChildren<TMP_Text>())
        {
            t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Text fits ("+t.rectTransform.rect+", font "+t.fontSize+"): "+t.text.Replace("\n"," / "));
        }
    }
}
#endif
