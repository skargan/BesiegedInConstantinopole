#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InfirmaryDeskCapture : MonoBehaviour
{
    string output; CouncilUI council; readonly List<string> results=new List<string>();
    VariableSingleton.CampaignSave baseline;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-infirmary-desk-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/InfirmaryDesk"));Directory.CreateDirectory(folder);
        SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Infirmary verification");DontDestroyOnLoad(go);go.AddComponent<InfirmaryDeskCapture>().output=folder;}
    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    GameObject Root()=>GameObject.Find("Council screen");
    Button Button(string name)=>Root().GetComponentsInChildren<Button>().Single(b=>b.name==name);
    void Layout(string context){Canvas.ForceUpdateCanvases();var panel=GameObject.Find("Infirmary desk").GetComponent<RectTransform>();
        Check(panel.anchorMin.x>=.5f,context+" half the background remains clear");
        var art=Root().GetComponentInChildren<RawImage>();Check(art!=null&&art.texture!=null&&art.texture.name=="infirmary",context+" new background loaded");
        Check(Root().GetComponentsInChildren<ScrollRect>().Length==0,context+" no long scrolling page");
        foreach(var text in Root().GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,context+" text fits: "+text.text);}
        var buttons=panel.GetComponentsInChildren<Button>();
        foreach(var button in buttons){var r=(RectTransform)button.transform;Check(r.rect.width>60&&r.rect.height>25,context+" usable button "+button.name);}
        for(int i=0;i<buttons.Length;i++)for(int j=i+1;j<buttons.Length;j++){
            var a=new Vector3[4];var b=new Vector3[4];((RectTransform)buttons[i].transform).GetWorldCorners(a);((RectTransform)buttons[j].transform).GetWorldCorners(b);
            Check(!new Rect(a[0],a[2]-a[0]).Overlaps(new Rect(b[0],b[2]-b[0])),context+" buttons do not overlap");
        }
    }
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        string report="INFIRMARY_DESK_CHECKS "+results.Count+" passed; two resolutions, care and research states, recruitment, preview, apply, persistence.";
        File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    void Fixture(int kind){VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline)));
        VariableSingleton.SetInt("iMoney",5000);VariableSingleton.SetInt("iHealers",100);VariableSingleton.SetInt("iProvisions",1000);
        MedicalRecovery.ResetForRestore();MonasticResearch.AfterRestore();
        if(kind>0)MedicalRecovery.ApplyCombatLosses(true,80,40,20,10);
        if(kind==2){VariableSingleton.SetInt("iHealers",30);VariableSingleton.SetInt("iProvisions",0);VariableSingleton.SetInt("iMoney",999);}
        if(kind==3){MonasticResearch.ApplyAllocation(100,out _);MonasticResearch.AllocationsForDay(VariableSingleton.GetDate().AddDays(1));}
        if(kind==4)MonasticResearch.ApplyAllocation(90,out _);
        if(kind==5){VariableSingleton.SetInt("iHealers",0);VariableSingleton.SetInt("iMoney",0);}
        MonasticResearch.AfterRestore();
    }
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;Check(SaveGames.Automated,"isolated archives");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+20;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());baseline=VariableSingleton.Instance.CaptureState();
        Check(MonasticResearchChecks.Run()>0,"existing monastic research regression suite");
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);
            Check(Screen.width==size.x&&Screen.height==size.y,"resolution applied");
            for(int kind=0;kind<6;kind++){
                Fixture(kind);string before=State();council.Infirmary();yield return new WaitForSecondsRealtime(.2f);Layout(size+" care state "+kind);
                Check(before==State(),"opening care preserves state and random sequence");
                Check(Button("Recruit five monks").interactable==(DailyLifeEvents.I("iMoney")>=1000),"recruitment affordability");
                if(kind==1||kind==2)yield return Capture("infirmary-"+kind+"-"+size.x);
                Button("Medical tab 1").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);Layout(size+" research state "+kind);
                Check(before==State(),"switching tabs preserves state and random sequence");
                var slider=Root().GetComponentInChildren<Slider>();Check(slider!=null&&slider.maxValue==MonasticResearch.TotalMonks,"slider uses actual monk roster");
                slider.value=Math.Max(0,MonasticResearch.TotalMonks-5);yield return null;Layout("allocation preview");
                Check(before==State(),"allocation preview never commits");
                if(kind==1||kind==2)yield return Capture("research-"+kind+"-"+size.x);
                Button("Medical tab 0").onClick.Invoke();yield return null;Check(before==State(),"leaving preview cancels unconfirmed change");
            }
        }
        Fixture(1);council.Infirmary();yield return null;Button("Recruit five monks").onClick.Invoke();yield return null;
        Check(DailyLifeEvents.I("iMoney")==4000&&MedicalRecovery.QueuedMonks==5,"recruitment pays and queues exact existing order");Layout("recruitment feedback");
        Button("Medical tab 1").onClick.Invoke();yield return null;Root().GetComponentInChildren<Slider>().value=70;Button("Apply monk allocation").onClick.Invoke();yield return null;
        Check(MonasticResearch.HasPending&&MonasticResearch.PendingResearchMonks==70&&MonasticResearch.ResearchMonks==0,"apply schedules tomorrow without taking care places early");Layout("pending allocation");
        string saved=State();var archive=SaveGames.Snapshot("Infirmary layout check");string path=Path.Combine(SaveGames.TestRoot,"infirmary.besieged.json");SaveGames.AtomicWrite(path,JsonUtility.ToJson(archive,true));
        Fixture(0);Check(SaveGames.Load(path,out var error),"archive loads: "+error);
        Check(MonasticResearch.HasPending&&MonasticResearch.PendingResearchMonks==70&&MedicalRecovery.QueuedMonks==5,"allocation and recruits survive save/load");
        council.Infirmary();yield return null;Button("Infirmary latest report").onClick.Invoke();yield return null;
        Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("Hold the position and monks can rescue")),"details retain rescue rule and report");
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
