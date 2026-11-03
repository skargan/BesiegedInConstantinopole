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

public sealed class ProvincialWarCapture : MonoBehaviour
{
    string output;int passed;int oldWidth,oldHeight;FullScreenMode oldMode;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-provincial-war-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/ProvincialWar"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");
        Directory.CreateDirectory(SaveGames.TestRoot);
        var go=new GameObject("Maritime verification");DontDestroyOnLoad(go);go.AddComponent<ProvincialWarCapture>().output=path;
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
        File.WriteAllText(Path.Combine(output,"checks.txt"),failure==null?"PROVINCIAL_CHECKS "+passed+" passed":"FAILED "+failure);
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
        passed+=ProvincialWarChecks.Run();
        typeof(VariableSingleton).GetField("turn",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn);
        ProvincialIntelligence.ResetForRestore();ProvinceCampaign.Ensure();
        var baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
            VariableSingleton.Instance.RestoreValidated(baseline);var p=ProvincialWar.Find("thrace");p.Resistance=45;ProvincialWar.Launch(p.Id,out _);
            ProvincialConflictUI.Show(p.Id);yield return null;Canvas.ForceUpdateCanvases();Capture("province-"+size.x+".png");
            Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("Hidden Muster")),"Hidden muster visible");
            Check(FindObjectsOfType<Button>().Any(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Gather Strength"),"Orders available");
            ProvincialConflictUI.Close();p.Set("MusterUntil",0);ProvinceCampaign.Set("Agent_thrace",2);p.Rebels.Troops=4000;ProvincialBattle.Begin(p,0,true);ProvincialConflictUI.Show(p.Id);yield return null;Capture("battle-1-"+size.x+".png");
            Check(ProvincialConflictUI.Back()&&ProvincialConflictUI.Open,"Escape cannot skip pending encounter");
            ProvincialBattle.Maneuver(p,0);ProvincialConflictUI.Show(p.Id);yield return null;Capture("battle-2-"+size.x+".png");ProvincialBattle.Continue(p);ProvincialBattle.Clash(p,1);ProvincialBattle.Continue(p);ProvincialConflictUI.Show(p.Id);yield return null;Capture("battle-3-"+size.x+".png");
            ProvincialConflictUI.Close();p.Set("BattleStage",0);p.Authority=0;ProvincialWar.Collapse(p);ProvincialConflictUI.Show(p.Id);yield return null;Capture("settlement-"+size.x+".png");
            ProvincialConflictUI.Close();VariableSingleton.Instance.RestoreValidated(baseline);WarTheatreUI.Show();yield return null;Capture("map-"+size.x+".png");Check(WarTheatre.Find("dulkadir")==null&&WarTheatre.Find("ramadanids")==null,"Mamluk union removes minor subregions");WarTheatreUI.Close();
            var council=FindObjectOfType<CouncilUI>(true);council.ProvincialOperationsFor("thrace");yield return null;Capture("network-"+size.x+".png");council.Hide();
        }
    }
}
#endif
