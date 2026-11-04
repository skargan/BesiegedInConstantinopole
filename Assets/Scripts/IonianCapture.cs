#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class IonianCapture:MonoBehaviour
{
    string output;int passed;CouncilUI council;ManagerUI manager;
    VariableSingleton.CampaignSave baseline;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-ionian-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Ionian"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");Directory.CreateDirectory(SaveGames.TestRoot);
        var go=new GameObject("Ionian checks");DontDestroyOnLoad(go);go.AddComponent<IonianCapture>().output=path;
    }
    void Check(bool ok,string message){if(!ok)throw new Exception(message);passed++;checks.Add("PASS "+message);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    GameObject Root=>GameObject.Find("Council screen");
    Button Button(string text)=>Root.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==text));
    void Page(string page)=>typeof(CouncilUI).GetMethod("IonianPage",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(council,new object[]{page});
    void Restore(){council.Hide();VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline)));CampaignHUD.Instance.DismissReport();}
    void Resident(){VariableSingleton.SetInt("iDiplomacyEnvoy_"+IonianDiplomacy.Country,2);}
    IEnumerator Start(){var stack=new Stack<IEnumerator>();stack.Push(Run());while(stack.Count>0){object next=null;bool moved=false;Exception error=null;
        try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){error=e;}
        if(error!=null){File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+passed+"\n"+error);File.WriteAllLines(Path.Combine(output,"details.txt"),checks);Application.Quit(1);yield break;}
        if(!moved){stack.Pop();continue;}if(next is IEnumerator routine)stack.Push(routine);else yield return next;
    }File.WriteAllText(Path.Combine(output,"checks.txt"),"IONIAN_CHECKS "+passed+" passed");File.WriteAllLines(Path.Combine(output,"details.txt"),checks);Application.Quit(0);}
    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.12f);Canvas.ForceUpdateCanvases();
        var obj=new GameObject("Art capture camera",typeof(Camera));var camera=obj.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{
            camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        }finally{
            RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
            camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(obj);Canvas.ForceUpdateCanvases();
        }
        yield return null;
    }
    void Layout(string page){Canvas.ForceUpdateCanvases();var panel=GameObject.Find("Ionian diplomatic panel").GetComponent<RectTransform>();
        foreach(var text in panel.GetComponentsInChildren<TMP_Text>()){
            if(text.GetComponentInParent<ScrollRect>()!=null)continue;
            text.ForceMeshUpdate();Check(!text.isTextOverflowing,page+" text fits: "+text.text.Replace("\n"," "));
        }
        foreach(var button in panel.GetComponentsInChildren<Button>()){
            var p=button.GetComponent<RectTransform>();var corners=new Vector3[4];p.GetWorldCorners(corners);
            Check(corners.All(v=>v.x>=0&&v.y>=0&&v.x<=Screen.width+1&&v.y<=Screen.height+1),page+" button inside viewport");
        }
    }
    IEnumerator Run(){Application.runInBackground=true;AudioListener.volume=0;yield return null;SaveMenu.StartCampaign(false);
        float until=Time.realtimeSinceStartup+30;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<until)yield return null;
        Check(VariableSingleton.IsInitialized,"Campaign initialized");yield return new WaitForSecondsRealtime(.5f);
        manager=FindObjectOfType<ManagerUI>();council=CouncilUI.For(manager);baseline=VariableSingleton.Instance.CaptureState();
        Check(IonianText.Data.remarks.Length==30,"All thirty canonical remarks loaded");
        Check(!IonianDiplomacy.Resident&&!IonianDiplomacy.Introduced,"Unintroduced initial state");
        var supports=IonianDiplomacy.SupportKeys.Select(VariableSingleton.GetFloatVariable).ToArray();
        Check(!IonianDiplomacy.Agree(false,out _),"No agreement without resident envoy");
        Check(IonianDiplomacy.SupportKeys.Select(VariableSingleton.GetFloatVariable).SequenceEqual(supports),"Unavailable negotiation cannot debit support");
        Check(CountryDiplomacy.AidRequestReason(IonianDiplomacy.Country)!=null,"Backend rejects ordinary aid");
        Check(CountryDiplomacy.RenewReason(IonianDiplomacy.Country,false)!=null,"Backend rejects ordinary aid renewal");
        int envoys=CountryDiplomacy.AvailableEnvoys;string rng=Rng();
        Check(CountryDiplomacy.Dispatch(IonianDiplomacy.Country,out _),"Existing envoy dispatch works");
        Check(CountryDiplomacy.AvailableEnvoys==envoys-1&&!IonianDiplomacy.Resident,"Envoy travels instead of teleporting");
        Check(IonianDiplomacy.SupportKeys.Select(VariableSingleton.GetFloatVariable).SequenceEqual(supports)&&Rng()==rng,"Dispatch costs no support or RNG");
        CountryDiplomacy.Advance(CountryDiplomacy.EnvoyDue(IonianDiplomacy.Country));Check(IonianDiplomacy.Resident,"Calendar arrival unlocks resident negotiations");
        IonianDiplomacy.Introduce();IonianDiplomacy.Introduce();Check(DecisionJournal.Entries.Count(e=>e.id=="ionian-introduction")==1,"Introduction recorded once");
        Check(IonianDiplomacy.Agree(false,out _),"Shelter succeeds");
        for(int i=0;i<4;i++)Check(Mathf.Abs(VariableSingleton.GetFloatVariable(IonianDiplomacy.SupportKeys[i])-Mathf.Clamp01(supports[i]-.03f))<.0001f,"Shelter penalty exactly three points "+i);
        supports=IonianDiplomacy.SupportKeys.Select(VariableSingleton.GetFloatVariable).ToArray();
        Check(!IonianDiplomacy.Agree(false,out _)&&IonianDiplomacy.SupportKeys.Select(VariableSingleton.GetFloatVariable).SequenceEqual(supports),"Repeated shelter cannot debit twice");
        Check(IonianDiplomacy.Agree(true,out _),"Personal refuge succeeds independently");
        for(int i=0;i<4;i++)Check(Mathf.Abs(VariableSingleton.GetFloatVariable(IonianDiplomacy.SupportKeys[i])-Mathf.Clamp01(supports[i]-.10f))<.0001f,"Personal refuge penalty exactly ten points "+i);
        Check(DailyLifeEvents.I("iSurrenderEnding")==0,"Agreement does not end campaign");
        var saved=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
        Restore();Check(!IonianDiplomacy.Shelter&&!IonianDiplomacy.Exile,"Restoring earlier state clears later agreements");
        VariableSingleton.Instance.RestoreValidated(saved);Check(IonianDiplomacy.Shelter&&IonianDiplomacy.Exile,"Both agreements survive save round trip");
        Check(DecisionJournal.Entries.Any(e=>e.id=="ionian-shelter"&&e.decision)&&DecisionJournal.Entries.Any(e=>e.id=="ionian-exile"&&e.decision),"Both dated decisions survive save round trip");
        CountryDiplomacy.Recall(IonianDiplomacy.Country,out _);Check(IonianDiplomacy.Exile&&IonianDiplomacy.Shelter&&!IonianDiplomacy.Resident,"Recall retains promises");
        VariableSingleton.SetInt("iSurrenderEnding",1);VariableSingleton.SetInt("iTermsFate2",1);Check(IonianDiplomacy.EndingReflection(false).Contains("Your negotiated exile now has a destination"),"Actual exile uses Ionian destination");
        VariableSingleton.SetInt("iTermsFate2",3);Check(!IonianDiplomacy.EndingReflection(false).Contains("Your negotiated exile now has a destination"),"Execution is not rewritten as escape");
        Check(IonianDiplomacy.EndingReflection(true).Contains("no need to accept"),"Victory remembers unused personal refuge");
        VariableSingleton.SetInt("iEmperorMarried",1);Check(IonianText.Data.remarks.Count(IonianText.Eligible)==30,"All approved remarks available regardless of marital or shelter state");
        rng=Rng();string previous=IonianText.NextRemark();Check(previous==IonianText.NextRemark(),"Repeated requests during cooldown keep the current remark");
        Check(rng==Rng(),"Remarks preserve campaign randomness");VariableSingleton.SetInt("iEmperorMarried",0);
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);Restore();
            council.DiplomaticPower(IonianDiplomacy.Country);yield return null;Layout("main");Check(GameObject.Find("Ionian diplomatic panel")!=null,"Diplomatic entry opens special court");yield return Capture("main-"+size.x);
            Button("Visit the isles").onClick.Invoke();yield return null;Check(GameObject.Find("Raimondina separate sprite")==null,"Coast is empty before envoy");Layout("empty");yield return Capture("empty-"+size.x);
            Resident();Page("visit");yield return null;Check(GameObject.Find("Raimondina separate sprite")!=null,"Resident envoy reveals separate sprite");Layout("visit");yield return Capture("visit-"+size.x);
            Check(!Button("Court Raimondina · under development").interactable&&!Button("A naval base · under development").interactable,"Future features explicitly disabled");
            foreach(string page in new[]{"realm","about","siege","shelter","exile","envoy"}){Page(page);yield return null;Layout(page);yield return Capture(page+"-"+size.x);}
        }
        Restore();Resident();Page("shelter");yield return null;
        var beforeConfirm=IonianDiplomacy.SupportKeys.Select(VariableSingleton.GetFloatVariable).ToArray();
        Button("Confirm agreement").onClick.Invoke();yield return null;
        Check(IonianDiplomacy.Shelter,"Real confirmation button commits shelter");
        for(int i=0;i<4;i++)Check(Mathf.Abs(VariableSingleton.GetFloatVariable(IonianDiplomacy.SupportKeys[i])-(beforeConfirm[i]-.03f))<.0001f,"Confirmation charges once "+i);
        Check(ActionResults.Instance.Message.Contains("Refugee shelter"),"Confirmation routes consolidated action report");
        Restore();
    }
}
#endif

