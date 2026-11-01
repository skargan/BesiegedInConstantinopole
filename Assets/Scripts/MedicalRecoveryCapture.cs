#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

// Explicit opt-in exported-player checks. Test archives never touch a player's saves.
public class MedicalRecoveryCapture : MonoBehaviour
{
    string output;
    int passed;
    VariableSingleton.CampaignSave baseline;
    readonly List<string> results=new List<string>();
    static readonly string[] Units={"iInfantry","iArchers","iCavalry","iVarangians"};

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-medical-recovery-capture"))return;
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/MedicalRecovery"));
        Directory.CreateDirectory(root);
        SaveGames.TestRoot=Path.Combine(root,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Medical recovery verification");DontDestroyOnLoad(go);
        go.AddComponent<MedicalRecoveryCapture>().output=root;
    }
    void Check(bool ok,string label)
    {
        if(!ok)throw new InvalidOperationException("MEDICAL RECOVERY: "+label);
        passed++;results.Add("PASS "+label);
    }
    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING: Medical recovery verification");
        var work=new Stack<IEnumerator>();work.Push(Run());
        while(work.Count>0){
            object next=null;bool moved=false;Exception failure=null;
            try{moved=work.Peek().MoveNext();if(moved)next=work.Peek().Current;}catch(Exception ex){failure=ex;}
            if(failure!=null){
                File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+passed+" checks.\n"+failure+"\n\n"+string.Join("\n",results));
                Debug.LogError(failure);Application.Quit(1);yield break;
            }
            if(!moved){work.Pop();continue;}
            if(next is IEnumerator nested)work.Push(nested);else yield return next;
        }
        string summary=passed+" exported-player medical recovery checks passed.";
        File.WriteAllText(Path.Combine(output,"checks.txt"),summary+"\n\n"+string.Join("\n",results));
        Debug.Log("MEDICAL_RECOVERY_CHECKS "+summary);Application.Quit(0);
    }
    int I(string key)=>VariableSingleton.GetIntVariable(key);
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    string RandomState()=>JsonUtility.ToJson(UnityEngine.Random.state);
    string MedicalState()=>string.Join("|",VariableSingleton.Instance.CaptureState().ints.Where(p=>p.key.StartsWith("iMedical")).OrderBy(p=>p.key).Select(p=>p.key+"="+p.value));
    VariableSingleton.CampaignSave Clone(VariableSingleton.CampaignSave state)=>JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(state));
    void Set(VariableSingleton.CampaignSave state,string key,int value)
    {
        var entry=state.ints.FirstOrDefault(p=>p.key==key);
        if(entry==null)state.ints.Add(new VariableSingleton.IntValue{key=key,value=value});else entry.value=value;
    }
    DateTime Fixture(int monks=100,int provisions=10000)
    {
        var state=Clone(baseline);state.turn=VariableSingleton.EffectiveSiegeTurn;state.training.Clear();
        state.ints.RemoveAll(p=>p.key.StartsWith("iMedical"));
        Set(state,"iHealers",monks);Set(state,"iProvisions",provisions);Set(state,"iMoney",5000);
        foreach(string unit in Units)Set(state,unit,1000);
        VariableSingleton.Instance.RestoreValidated(state);
        Check(MedicalRecovery.Total==0,"Fixture/legacy load starts with an empty infirmary");
        return VariableSingleton.GetDate();
    }
    IEnumerator WaitForCampaign(int previousInstance)
    {
        float deadline=Time.realtimeSinceStartup+20;
        while(Time.realtimeSinceStartup<deadline){
            if(VariableSingleton.IsInitialized&&VariableSingleton.Instance!=null&&VariableSingleton.Instance.GetInstanceID()!=previousInstance&&FindObjectOfType<ManagerUI>()!=null)break;
            yield return null;
        }
        Check(VariableSingleton.Instance!=null&&VariableSingleton.Instance.GetInstanceID()!=previousInstance&&FindObjectOfType<ManagerUI>()!=null,"Campaign initialised");
        yield return new WaitForSecondsRealtime(.3f);CampaignHUD.Instance.DismissReport();
    }
    void ModelChecks()
    {
        DateTime day=Fixture();long population=PopulationAccounting.People;int manpower=RecruitmentLogistics.AvailableManpower;
        string rng=RandomState();
        MedicalRecovery.ApplyCombatLosses(true,20,10,6,4);
        Check(I("iInfantry")==980&&I("iArchers")==990&&I("iCavalry")==994&&I("iVarangians")==996,"Combat subtracts each casualty once");
        Check(MedicalRecovery.Total==20&&MedicalRecovery.CountFor(Units[0])==10&&MedicalRecovery.CountFor(Units[1])==5&&MedicalRecovery.CountFor(Units[2])==3&&MedicalRecovery.CountFor(Units[3])==2,"Admissions preserve the casualty mix and half-loss ceiling");
        Check(MedicalRecovery.RegularWounded==18&&MedicalRecovery.FreePlaces==80&&MedicalRecovery.SoonestReturnDays==15,"Bed and treatment counters match admitted patients");
        Check(PopulationAccounting.People==population,"Treatment intake does not invent or remove population");
        Check(RecruitmentLogistics.AvailableManpower==manpower+18,"Wounded regulars remain committed against recruitment manpower");
        Check(MedicalRecovery.DailyProvisionNeed==4,"Treatment provision need is aggregated across troop types");
        string medical=MedicalState();MedicalRecovery.Advance(day);
        Check(MedicalState()==medical&&I("iProvisions")==10000,"Admission date grants no instant treatment");
        for(int d=1;d<=14;d++)MedicalRecovery.Advance(day.AddDays(d));
        Check(MedicalRecovery.Total==20&&MedicalRecovery.SoonestReturnDays==1&&I("iInfantry")==980,"No soldier returns before fifteen treatment days");
        Check(I("iProvisions")==9944,"Four daily provisions paid for fourteen treatment days");
        MedicalRecovery.Advance(day.AddDays(15));
        Check(MedicalRecovery.Total==0&&I("iInfantry")==990&&I("iArchers")==995&&I("iCavalry")==997&&I("iVarangians")==998,"Recovered soldiers return to their original troop types");
        Check(MedicalRecovery.RecoveredFor(Units[0])==10&&MedicalRecovery.RecoveredFor(Units[3])==2,"Recovered totals persist by unit");
        Check(I("iProvisions")==9940&&PopulationAccounting.People==population,"Treatment charges exact cost without changing population");
        medical=MedicalState();int food=I("iProvisions");MedicalRecovery.Advance(day.AddDays(15));
        Check(MedicalState()==medical&&I("iProvisions")==food,"Repeated daily processing cannot duplicate returns or cost");
        Check(RandomState()==rng,"Medical accounting consumes no gameplay randomness");

        day=Fixture(3);MedicalRecovery.ApplyCombatLosses(true,2,2,2,0);
        Check(MedicalRecovery.Total==3&&MedicalRecovery.DailyProvisionNeed==1,"Small mixed patients share a single provision charge");
        MedicalRecovery.Advance(day.AddDays(1));Check(I("iProvisions")==9999,"Small batches aggregate before rounding cost");
        day=Fixture();MedicalRecovery.ApplyCombatLosses(true,1,1,1,0);
        Check(MedicalRecovery.Total==1&&Units.Sum(MedicalRecovery.CountFor)==1&&Units.All(u=>MedicalRecovery.CountFor(u)<=1),"Odd mixed casualties round the combined half-loss pool once without inventing a unit");
        day=Fixture(8);MedicalRecovery.ApplyCombatLosses(true,20,10,6,4);
        Check(MedicalRecovery.Total==8&&MedicalRecovery.FreePlaces==0,"Full infirmary caps rescue at available places");
        MedicalRecovery.ApplyCombatLosses(true,20,0,0,0);
        Check(MedicalRecovery.Total==8&&I("iInfantry")==960,"Second battle cannot reuse occupied medical places");
        day=Fixture(0);MedicalRecovery.ApplyCombatLosses(true,20,0,0,0);
        Check(MedicalRecovery.Total==0&&I("iInfantry")==980,"Without monks combat losses remain permanent");
        day=Fixture();MedicalRecovery.ApplyCombatLosses(false,20,10,0,0);
        Check(MedicalRecovery.Total==0&&I("iInfantry")==980&&I("iArchers")==990,"Retreat does not rescue casualties from an abandoned position");
        day=Fixture();MedicalRecovery.ApplyCombatLosses(true,5000,-2,0,0);
        Check(I("iInfantry")==0&&I("iArchers")==1000&&MedicalRecovery.Total==100,"Admissions clamp actual losses and reject negative casualty inputs");
        day=Fixture();VariableSingleton.ChangeInt("iInfantry",-100);
        Check(MedicalRecovery.Total==0,"Desertion, transfers and other generic troop reductions are not admitted");

        day=Fixture(10,1);MedicalRecovery.ApplyCombatLosses(true,20,0,0,0);MedicalRecovery.Advance(day.AddDays(1));
        Check(MedicalRecovery.Total==10&&I("iProvisions")==0&&MedicalRecovery.SoonestReturnDays==14,"Limited provisions treat only five patients without free care");
        MedicalRecovery.Advance(day.AddDays(2));
        Check(MedicalRecovery.Total==10&&MedicalRecovery.SoonestReturnDays==14,"Empty stores pause treatment without hidden deaths");
        VariableSingleton.SetInt("iProvisions",100);MedicalRecovery.Advance(day.AddDays(15));
        Check(MedicalRecovery.Total==10&&MedicalRecovery.SoonestReturnDays==1,"Paused days do not count towards recovery");
        MedicalRecovery.Advance(day.AddDays(16));Check(MedicalRecovery.Total==5&&I("iInfantry")==985,"Patients closest to recovery return first");
        MedicalRecovery.Advance(day.AddDays(17));Check(MedicalRecovery.Total==0&&I("iInfantry")==990,"Remaining patients finish after their own full treatment");

        day=Fixture(10);MedicalRecovery.ApplyCombatLosses(true,20,0,0,0);VariableSingleton.SetInt("iHealers",0);
        MedicalRecovery.Advance(day.AddDays(7));Check(MedicalRecovery.Total==10&&MedicalRecovery.SoonestReturnDays==15&&I("iProvisions")==10000,"Losing all monks pauses patients without spending provisions");
        VariableSingleton.SetInt("iHealers",5);MedicalRecovery.Advance(day.AddDays(22));
        Check(MedicalRecovery.Total==5&&I("iInfantry")==985,"Reduced workforce treats only its actual concurrent capacity");
        MedicalRecovery.Advance(day.AddDays(37));Check(MedicalRecovery.Total==0&&I("iInfantry")==990,"Waiting patients recover after places become available");
        day=Fixture();MedicalRecovery.ApplyCombatLosses(true,10,0,0,0);MedicalRecovery.Advance(day.AddDays(30));
        Check(MedicalRecovery.Total==0&&I("iInfantry")==995&&I("iProvisions")==9985,"Preparation calendar jumps process elapsed treatment days only until recovery");

        day=Fixture(100);Check(MedicalRecovery.RecruitMonks(out var recruitReport),"Recruiting five monks succeeds with sufficient gold");
        Check(I("iMoney")==4000&&MedicalRecovery.Monks==100&&MedicalRecovery.QueuedMonks==5,"Monks cost gold and wait in the existing recruitment queue");
        TrainingPool.CompleteDueOrders(day.AddDays(4));Check(MedicalRecovery.Monks==100&&MedicalRecovery.QueuedMonks==5,"Monks cannot arrive early");
        TrainingPool.CompleteDueOrders(day.AddDays(5));Check(MedicalRecovery.Monks==105&&MedicalRecovery.QueuedMonks==0,"Five monks arrive after five days");
        VariableSingleton.SetInt("iMoney",999);Check(!MedicalRecovery.RecruitMonks(out recruitReport)&&I("iMoney")==999&&MedicalRecovery.QueuedMonks==0,"Insufficient gold causes no partial recruitment purchase");
    }
    void SaveChecks()
    {
        DateTime day=Fixture();MedicalRecovery.ApplyCombatLosses(true,20,10,6,4);MedicalRecovery.Advance(day.AddDays(5));
        Check(MedicalRecovery.RecruitMonks(out _),"Monk recruitment fixture created for archive roundtrip");
        string expected=MedicalState(),rng=RandomState();int food=I("iProvisions");
        var archive=SaveGames.Snapshot("Medical recovery fixture");string path=Path.Combine(SaveGames.TestRoot,"medical-fixture.besieged.json");
        SaveGames.AtomicWrite(path,JsonUtility.ToJson(archive,true));
        var read=SaveGames.Read(path);Check(read.saveFormat==2&&read.campaign.version==baseline.version,"Medical data retains the existing save format");
        Fixture(0);Check(SaveGames.Load(path,out var error),"Medical archive loads: "+error);
        Check(MedicalState()==expected&&MedicalRecovery.Total==20&&MedicalRecovery.SoonestReturnDays==10,"Archive restores patient identity and progress");
        Check(MedicalRecovery.QueuedMonks==5&&I("iProvisions")==food&&RandomState()==rng,"Archive restores monk recruitment, provisions and random state");
        MedicalRecovery.Advance(day.AddDays(5));Check(MedicalState()==expected&&I("iProvisions")==food,"Reloading cannot repeat an already-paid treatment day");
        MedicalRecovery.Advance(day.AddDays(15));Check(MedicalRecovery.Total==0&&I("iInfantry")==990,"Saved patients finish on their original schedule");
        var legacy=Clone(baseline);legacy.ints.RemoveAll(p=>p.key.StartsWith("iMedical"));
        VariableSingleton.Instance.RestoreValidated(legacy);
        Check(MedicalRecovery.Total==0&&MedicalRecovery.RecoveredFor(Units[0])==0&&MedicalRecovery.SoonestReturnDays==0,"Loading an older save in the same session clears all new medical history");
    }
    void PhasedCommitChecks()
    {
        Fixture();var ai=AI.Instance;Check(ai!=null,"Live assault controller is available");
        ai.AIBashiBazoukAssault();var battle=ai.PendingPhased;Check(battle!=null,"Live phased assault created");
        battle.Infantry=Math.Max(0,battle.InitialInfantry-40);battle.Archers=Math.Max(0,battle.InitialArchers-20);
        battle.EnemyLeft=0;battle.Resolve(false);
        Check(battle.Finished&&battle.Held,"Resolved held-position fixture");
        int foot=I("iInfantry"),bows=I("iArchers");ai.CommitPhased(battle);
        Check(battle.Committed&&I("iInfantry")==foot-40&&I("iArchers")==bows-20&&MedicalRecovery.Total==30,"Actual assault settlement subtracts casualties and admits wounded once");
        string state=State();ai.CommitPhased(battle);Check(State()==state,"Revisiting committed assault cannot subtract or admit casualties again");
    }
    void DailyHookCheck()
    {
        Fixture();MedicalRecovery.ApplyCombatLosses(true,10,0,0,0);
        VariableSingleton.IncrementTurn();
        Check(MedicalRecovery.Total==5&&MedicalRecovery.SoonestReturnDays==14,"Actual end-of-turn flow advances medical treatment");
        Check(VariableSingleton.ArrivalNotice.Contains("Infirmary:"),"Actual turn report includes the infirmary update");
        string state=MedicalState();MedicalRecovery.Advance(VariableSingleton.GetDate());
        Check(MedicalState()==state,"Reopening the infirmary after a turn cannot advance care twice");
    }
    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;
        SaveMenu.StartCampaign(false);yield return WaitForCampaign(0);
        baseline=VariableSingleton.Instance.CaptureState();
        ModelChecks();SaveChecks();PhasedCommitChecks();DailyHookCheck();
        Fixture();MedicalRecovery.ApplyCombatLosses(true,50,20,10,6);
        int previous=VariableSingleton.Instance.GetInstanceID();SaveMenu.StartCampaign(false);yield return WaitForCampaign(previous);
        Check(MedicalRecovery.Total==0&&MedicalRecovery.QueuedMonks==0&&MedicalRecovery.RecoveredFor(Units[0])==0,"Actual new campaign cannot inherit medical patients or history");
        var council=CouncilUI.For(FindObjectOfType<ManagerUI>());
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);
            Check(Screen.width==size.x&&Screen.height==size.y,"Requested capture resolution applied "+size);
            Fixture();council.Infirmary();yield return Capture("infirmary-empty-"+size.x+"x"+size.y);
            var recruitButton=FindObjectsOfType<UnityEngine.UI.Button>().Single(b=>b.name=="Recruit five monks");
            Check(recruitButton.interactable,"Infirmary recruitment control is available with sufficient gold");
            recruitButton.onClick.Invoke();yield return null;
            Check(I("iMoney")==4000&&MedicalRecovery.QueuedMonks==5,"Infirmary recruitment control places the actual paid order");
            DateTime day=Fixture();MedicalRecovery.ApplyCombatLosses(true,120,40,20,20);MedicalRecovery.Advance(day.AddDays(5));
            MedicalRecovery.RecruitMonks(out _);council.Infirmary();yield return Capture("infirmary-active-"+size.x+"x"+size.y);
            VariableSingleton.SetInt("iHealers",30);VariableSingleton.SetInt("iProvisions",1);MedicalRecovery.Advance(day.AddDays(6));
            council.Infirmary();yield return Capture("infirmary-shortage-"+size.x+"x"+size.y);
            council.Training();yield return Capture("recruitment-recovery-"+size.x+"x"+size.y,false);
        }
    }
    IEnumerator Capture(string name,bool completePage=true)
    {
        yield return new WaitForSecondsRealtime(.2f);Canvas.ForceUpdateCanvases();
        var texts=FindObjectsOfType<TMP_Text>();foreach(var t in texts)t.ForceMeshUpdate();
        File.WriteAllLines(Path.Combine(output,name+"-text.txt"),texts.Select(t=>t.name+" | "+t.rectTransform.rect+" | overflow="+t.isTextOverflowing+" | "+t.text));
        var page=GameObject.Find("Council screen");Check(page!=null,"Infirmary page exists for "+name);
        var checkedTexts=page.GetComponentsInChildren<TMP_Text>().Where(t=>completePage||t.text.StartsWith("LOSSES ·")||t.text.StartsWith("Recruitment pool:")||t.text.StartsWith("Infirmary ·")).ToArray();
        Check(completePage||checkedTexts.Length==3,"All three new recruitment/recovery labels found "+name);
        var overflowing=checkedTexts.Where(t=>t.isTextOverflowing).Select(t=>t.text).ToArray();
        Check(overflowing.Length==0,"Infirmary text fits "+name+": "+string.Join(" | ",overflowing));
        var cameraObject=new GameObject("Medical capture camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.07f,.08f);camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=new RenderTexture(Screen.width,Screen.height,24);var shot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try{
            camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        }finally{
            RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
            camera.targetTexture=null;target.Release();Destroy(target);Destroy(shot);Destroy(cameraObject);Canvas.ForceUpdateCanvases();
        }
        yield return null;
    }
}
#endif
