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

// Explicit opt-in development checks. All test saves are isolated from player archives.
public class OptimiseMechanicsCapture:MonoBehaviour
{
    static bool launched;
    string output;readonly List<string> results=new List<string>();bool failed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Launch() {
        if(launched||!Environment.GetCommandLineArgs().Contains("-optimise-mechanics-capture"))return;launched=true;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Optimise"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");
        var go=new GameObject("Optimise mechanics verification");DontDestroyOnLoad(go);go.AddComponent<OptimiseMechanicsCapture>().output=path;
    }
    void Check(bool ok,string text){results.Add((ok?"PASS ":"FAIL ")+text);if(!ok)failed=true;}
    void Suite(string name,Func<object> run) {try{results.Add("PASS "+name+": "+run());}catch(Exception ex){failed=true;results.Add("FAIL "+name+": "+ex);}}
    IEnumerator Start() {
        AudioListener.volume=0;Application.runInBackground=true;File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");
        var work=new Stack<IEnumerator>();work.Push(Run());
        while(work.Count>0) {
            bool moved=false;object next=null;Exception failure=null;
            try{moved=work.Peek().MoveNext();if(moved)next=work.Peek().Current;}catch(Exception ex){failure=ex;}
            if(failure!=null){failed=true;results.Add("FAIL runner: "+failure);break;}
            if(!moved){work.Pop();continue;}if(next is IEnumerator inner)work.Push(inner);else yield return next;
        }
        File.WriteAllText(Path.Combine(output,"checks.txt"),(failed?"FAILED":"PASSED")+"\n"+string.Join("\n",results));Application.Quit(failed?1:0);
    }
    IEnumerator Run() {
        yield return null;SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+30;
        while(!VariableSingleton.IsInitialized&&Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.5f);Check(VariableSingleton.IsInitialized,"Campaign starts");
        CampaignHUD.Instance.DismissReport();
        Suite("Diplomacy",()=>DiplomacyMechanicsChecks.Run());
        Suite("Wall repairs",()=>WallRepairChecks.Run());
        Suite("Finite interventions",()=>WarInterventionChecks.Run());
        Suite("Naval personnel and battle diplomacy",()=>NavalPersonnelChecks.Run());
        var research=typeof(VariableSingleton).Assembly.GetType("MonasticResearchChecks");
        if(research!=null)Suite("Monastic research",()=>research.GetMethod("Run").Invoke(null,null));else Check(false,"Monastic checks present");
        MonthlyAndArchiveChecks();
        var baseline=VariableSingleton.Instance.CaptureState();
        var council=CouncilUI.For(FindObjectOfType<ManagerUI>());
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}) {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);
            VariableSingleton.Instance.RestoreValidated(baseline);VariableSingleton.SetInt("iMoney",100000);
            VariableSingleton.SetInt("iHealers",20);VariableSingleton.SetInt("iSecrets",10);
            MedicalRecovery.ApplyCombatLosses(true,20,10,0,0);MonasticResearch.ApplyAllocation(6,out _);
            council.Infirmary();yield return Capture("infirmary-"+size.x);
            var scroll=GameObject.Find("Council scroll").GetComponent<ScrollRect>();scroll.verticalNormalizedPosition=0;
            yield return Capture("research-"+size.x);
            var slider=GameObject.Find("Research monk allocation").GetComponent<Slider>();slider.value=10;
            var apply=GameObject.Find("Apply monk allocation").GetComponent<Button>();Check(apply.interactable,"Allocation preview enables apply "+size.x);int turn=VariableSingleton.Turn;int money=DailyLifeEvents.I("iMoney");apply.onClick.Invoke();yield return null;
            Check(MonasticResearch.HasPending&&MonasticResearch.PendingResearchMonks==10&&MonasticResearch.ResearchMonks==0,"Allocation applies tomorrow only "+size.x);
            Check(turn==VariableSingleton.Turn&&money==DailyLifeEvents.I("iMoney"),"Allocation is free "+size.x);
            yield return Capture("research-pending-"+size.x);
            council.Technologies();yield return Capture("technologies-"+size.x);
            council.Specialists();yield return Capture("specialists-"+size.x);
            council.DiplomaticRelations();yield return Capture("diplomacy-"+size.x);
            council.DiplomaticPower(3);yield return Capture("papacy-"+size.x);
            typeof(CouncilUI).GetMethod("AdvancedDiplomacy",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(council,new object[]{3});yield return Capture("papacy-orders-"+size.x);
            council.DiplomaticPower(23);yield return Capture("mamluks-"+size.x);
            council.Training();yield return Capture("training-"+size.x);
            council.Fleet();yield return Capture("fleet-"+size.x);
        }
    }
    void MonthlyAndArchiveChecks() {
        var baseline=VariableSingleton.Instance.CaptureState();
        try{
            VariableSingleton.SetInt("iMoney",100000);VariableSingleton.SetInt("iHealers",20);VariableSingleton.SetInt("iEngineers",30);VariableSingleton.SetInt("iMaterials",1000);VariableSingleton.SetInt("iProvisions",10000);
            MonasticResearch.ApplyAllocation(6,out _);VariableSingleton.SetInt("iMonasticProgress",98);int secrets=DailyLifeEvents.I("iSecrets");
            MedicalRecovery.ApplyCombatLosses(true,20,10,0,0);
            Check(TrainingPool.Place("iHealers",5,200)&&TrainingPool.Place("iEngineers",1,250)&&TrainingPool.Place("iEnvoys",1,750)&&TrainingPool.Place("iCannons",1,1000),"Mixed paid personnel/construction orders accepted");
            Check(CountryDiplomacy.Dispatch(0,out _),"Envoy departs before calendar advance");float goodwill=CountryDiplomacy.Goodwill(0);
            var archive=SaveGames.Snapshot("Integrated personnel fixture");string path=Path.Combine(SaveGames.TestRoot,"integrated-personnel.besieged.json");SaveGames.AtomicWrite(path,JsonUtility.ToJson(archive,true));
            var read=SaveGames.Read(path);Check(read.campaign.training.Count==4,"Archive validates new order fields");
            VariableSingleton.Instance.RestoreValidated(baseline);Check(SaveGames.Load(path,out var error),"Actual archive load: "+error);
            Check(TrainingPool.Orders.Count==4&&MonasticResearch.HasPending&&MonasticResearch.Progress==98&&CountryDiplomacy.EnvoyState(0)==CountryDiplomacy.EnvoyAssignment.Outbound,"Archive restores reservations, pending research and envoy travel");
            var previous=VariableSingleton.GetDate();VariableSingleton.IncrementTurn();int days=(VariableSingleton.GetDate()-previous).Days;
            Check(days==31,"Real preparation turn advances31calendar days");
            Check(MonasticResearch.Progress==(98+6*days)%100&&DailyLifeEvents.I("iSecrets")==secrets+(98+6*days)/100,"Real turn awards dated research once");
            Check(MedicalRecovery.Monks==25&&MonasticResearch.InfirmaryMonks==19&&TrainingPool.Orders.Count==0,"Real turn settles recruits on their dates");
            Check(Math.Abs(CountryDiplomacy.Goodwill(0)-goodwill-3.25f)<.001f,"Real turn counts13resident days after18travel days");
            Check(MedicalRecovery.Total==0&&VariableSingleton.ArrivalNotice.Contains("15 recovered patients"),"Month-end report retains intermediate recovered patients");
            string state=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());CampaignPersonnelCalendar.Advance(previous,VariableSingleton.GetDate());
            Check(state==JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()),"Repeating elapsed interval cannot duplicate work");
        }finally{VariableSingleton.Instance.RestoreValidated(baseline);}
    }
    IEnumerator Capture(string name) {
        yield return new WaitForSecondsRealtime(.15f);Canvas.ForceUpdateCanvases();
        var texts=FindObjectsOfType<TMP_Text>();foreach(var text in texts)text.ForceMeshUpdate();
        File.WriteAllLines(Path.Combine(output,name+".txt"),texts.Select(t=>"overflow="+t.isTextOverflowing+" | "+t.text));
        var page=GameObject.Find("Council screen");var overflow=page.GetComponentsInChildren<TMP_Text>().Where(t=>t.isTextOverflowing).Select(t=>t.text).ToArray();
        Check(overflow.Length==0,"Text fit "+name+" "+string.Join(" | ",overflow));
        var go=new GameObject("Capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.07f,.08f);camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=new RenderTexture(Screen.width,Screen.height,24);var shot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;target.Release();Destroy(target);Destroy(shot);Destroy(go);}
    }
}

#endif
