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

public sealed class ProvincialBattleArtCapture : MonoBehaviour
{
    string output;int passed;int oldWidth,oldHeight;FullScreenMode oldMode;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-provincial-art-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/ProvincialArt"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");
        Directory.CreateDirectory(SaveGames.TestRoot);
        var go=new GameObject("Provincial art verification");DontDestroyOnLoad(go);go.AddComponent<ProvincialBattleArtCapture>().output=path;
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
        typeof(VariableSingleton).GetField("turn",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn);
        ProvincialIntelligence.ResetForRestore();ProvinceCampaign.Ensure();
        var p=ProvincialWar.Find("thrace");p.Resistance=45;ProvincialWar.Launch(p.Id,out _);
        p.Set("MusterUntil",0);ProvinceCampaign.Set("Agent_thrace",2);p.Rebels.Troops=4000;ProvincialBattle.Begin(p,0,true);
        var fixture=VariableSingleton.Instance.CaptureState();
        // Exhaustive presentation routing: all stages, terrains and aftermath combinations.
        for(int stage=1;stage<=6;stage++)for(int terrain=0;terrain<3;terrain++)for(int result=-1;result<=1;result++)for(int choice=0;choice<4;choice++)for(int renewed=0;renewed<=1;renewed++)for(int routed=0;routed<=1;routed++)for(int counter=0;counter<=1;counter++)
        {
            p.Set("BattleStage",stage);p.Set("BTerrain",terrain);p.Set("BResult",result);p.Set("BAfterChoice",choice);p.Set("BRenewed",renewed);p.Set("BRouted",routed);p.Set("BCounter",counter);
            string before=State(),key=ProvincialBattleArt.Key(p);
            if(State()!=before)throw new Exception("Artwork changed campaign state");
            bool pursuit=stage==6&&result>0&&choice==0&&renewed==0&&counter==0;
            bool retreat=stage==6&&result<0&&choice<=1&&renewed==0&&routed==0;
            string expected=stage==1?new[]{"01-encounter-plains","02-encounter-woodland","03-encounter-mountain"}[terrain]:stage==2?"04-manoeuvres":stage<=4?"05-clash":pursuit?"06-pursuit":retreat?"07-fighting-withdrawal":"08-aftermath";
            if(key!=expected)throw new Exception("Incorrect art routing: "+key);
        }
        Check(true,"1728 state combinations: correct routing and no campaign mutations");
        p.Set("BRenewed",0);p.Set("BRouted",0);p.Set("BCounter",0);
        VariableSingleton.Instance.RestoreValidated(fixture);p.Set("BattleStage",1);ProvincialBattle.Maneuver(p,0);
        p.Set("BApproach",0);p.Set("BEnemyApproach",1);p.Set("BTactic",1);p.Set("BShownEnemyTactic",2);
        p.Set("BTotalOurLoss",275);p.Set("BTotalEnemyLoss",430);p.Set("BAfterOurLoss",50);p.Set("BAfterEnemyLoss",125);p.Set("BAfterAuthority",85);p.Set("BAfterControl",1000);
        string[] keys={"01-encounter-plains","02-encounter-woodland","03-encounter-mountain","04-manoeuvres","05-clash","06-pursuit","07-fighting-withdrawal","08-aftermath"};
        foreach(string key in keys)Check(Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/ProvincialBattles/"+key)!=null,"Texture loads: "+key);
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
            for(int i=0;i<8;i++)
            {
                p.Set("BattleStage",i<3?1:i==3?2:i==4?3:i<7?6:5);p.Set("BTerrain",i<3?i:0);p.Set("BResult",i==6?-1:1);p.Set("BAfterChoice",0);
                string before=State();ProvincialConflictUI.Show(p.Id);yield return null;Canvas.ForceUpdateCanvases();
                var report=GameObject.Find("Battle report").GetComponent<TMP_Text>();report.ForceMeshUpdate();
                Check(!report.isTextOverflowing,"Report fits "+i+" at "+size.x);
                Check(State()==before,"Displaying art preserves state "+i+" at "+size.x);
                Check(GameObject.Find("Provincial command").GetComponentsInChildren<RawImage>().Any(r=>r.texture!=null&&r.texture.name==keys[i]),"Correct visible texture "+i);
                foreach(var label in GameObject.Find("Provincial command").GetComponentsInChildren<Button>().Select(b=>b.GetComponentInChildren<TMP_Text>())){label.ForceMeshUpdate();Check(!label.isTextOverflowing,"Button fits "+label.text+" at "+size.x);}
                Capture("art-"+i+"-"+size.x+".png");ProvincialConflictUI.Close();yield return null;
            }
        }
        VariableSingleton.Instance.RestoreValidated(fixture);ProvincialConflictUI.Show(p.Id);yield return null;
        Click("Caution");yield return null;Check(p.Get("BattleStage")==2,"Caution button advances to manoeuvre report");
        Click("Continue");yield return null;Check(p.Get("BattleStage")==3,"Continue advances to clash");
        Click("Hold Formation");yield return null;Check(p.Get("BattleStage")==4,"Tactic button resolves clash");
        Capture("clash-report-1280.png");
        Click("Continue");yield return null;Check(p.Get("BattleStage")==5,"Continue advances to aftermath choice");
        p.Set("BEnemyAfter",0);p.Set("BEnemyRenews",0);
        Click(p.Get("BResult")>0?"Consolidate":p.Get("BResult")<0?"Withdraw":"Disengage");yield return null;
        Check(p.Get("BattleStage")==6,"Aftermath button resolves battle");
        Click("Acknowledge result");yield return null;Check(p.Get("BattleStage")==0&&!ProvincialConflictUI.Open,"Acknowledgement returns to map");
    }
    void Click(string prefix)
    {
        var button=GameObject.Find("Provincial command").GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text.StartsWith(prefix));
        Check(button.interactable,"Button enabled: "+prefix);button.onClick.Invoke();
    }
}
#endif
