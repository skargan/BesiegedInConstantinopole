#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class TenCampaignProbe : MonoBehaviour
{
    [Serializable] public class Run {public int focus,seed,turns,days,gold,provisions,infantry,archers;public string result;public List<string> issues=new List<string>();public List<string> trace=new List<string>();}
    [Serializable] public class Report {public List<Run> runs=new List<Run>();}
    Report report=new Report();Run current;float deadline;bool finished;
    string directory="Logs/ten-campaigns";
    static T Find<T>() where T:UnityEngine.Object=>FindObjectOfType<T>(true);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() {if(Environment.GetCommandLineArgs().Contains("-ten-campaigns")) new GameObject("Ten Campaign Probe").AddComponent<TenCampaignProbe>();}
    void Awake() {DontDestroyOnLoad(gameObject);Directory.CreateDirectory(directory);Application.runInBackground=true;Application.logMessageReceived+=Log;deadline=Time.realtimeSinceStartup+1200;StartCoroutine(Play());}
    void Log(string text,string stack,LogType type) {if(current!=null && (type==LogType.Error || type==LogType.Exception || type==LogType.Assert)) current.issues.Add(text+"\n"+stack);}
    void Update() {if(!finished && Time.realtimeSinceStartup>deadline) {current?.issues.Add("Playtest timed out");Finish();}}
    IEnumerator Frames(int count=3) {for(int i=0;i<count;i++) yield return null;}
    int Get(string k)=>CouncilActions.Int(k);
    OptionButton Select(int focus,int variant) {
        var choices=FindObjectsOfType<OptionButton>().Where(o=>o.IsInteractable()).ToArray();if(choices.Length==0)return null;
        var opening=choices.FirstOrDefault(o=>o.Option.Line.Text.Text.Contains("Our focus will be "+FocusIcon.Names[focus]));if(opening!=null)return opening;
        Func<OptionButton,int> score=o=> {
            string t=o.Option.Line.Text.Text;int value=0;
            if(t.Contains(FocusIcon.Names[focus]))value+=70;
            if(t.Contains("Greet our guests")||t.Contains("Hire him")||t.Contains("defend the City together")||t.Contains("Grant the castles")||t.Contains("every blade"))value+=50;
            if(t.Contains("Outer")||t.Contains("Theodosian"))value+=5;
            if(t.Contains("Receive him alone")||t.Contains("dungeon")||t.Contains("Execute")||t.Contains("surrender to")||t.Contains("Full Union"))value-=50;
            return value;
        };
        int best=choices.Max(score);var top=choices.Where(o=>score(o)==best).ToArray();return top[(variant+Get("iFocus")+VariableSingleton.Turn)%top.Length];
    }
    void Validate() {
        var state=VariableSingleton.Instance.CaptureState();
        foreach(var v in state.ints) if(v.value<0 && v.key!="iCommander")current.issues.Add("Negative integer "+v.key+"="+v.value);
        foreach(var v in state.floats)if(float.IsNaN(v.value)||float.IsInfinity(v.value)||v.value<0||(v.value>1.0001f && !v.key.EndsWith("Quality")))current.issues.Add("Percentage outside bounds "+v.key+"="+v.value);
        if(VariableSingleton.Turn>0 && Get("iFocus")!=current.focus)current.issues.Add("Focus changed unexpectedly");
    }
    void Major(ManagerUI manager,int variant) {
        int turn=VariableSingleton.Turn;string result;
        int target=VariableSingleton.HasSiegeStarted()?(variant==0?20:10):(variant==0?45:30);
        if(SupplyEconomy.DaysLeft<(VariableSingleton.HasSiegeStarted()?5:target)) {
            int quantity=Mathf.Max(0,SupplyEconomy.DailyNeed*target-Get("iProvisions"));
            SupplyEconomy.Buy("iProvisions",quantity,out var purchase);
        }
        if(VariableSingleton.HasSiegeStarted() && SupplyEconomy.DaysLeft<3 && Get("iMoney")<SupplyEconomy.DailyNeed*SupplyEconomy.Price("iProvisions")*3) {
            SupplyEconomy.EmergencyTax(out result);ActionResults.Instance.Message=result;manager.CompleteAction();return;
        }

        if(turn%10==0 && turn>20 && Get("iMoney")>20000) {TrainingPool.Place("iInfantry",100,10);TrainingPool.Place("iArchers",50,20);TrainingPool.Place("iVarangians",5,500);}
        int weakest=Enumerable.Range(0,4).OrderBy(i=>VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[i])).First();
        if(VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[weakest])<.5f && CouncilActions.SupportFaction(weakest,out result)) {ActionResults.Instance.Message=result;manager.CompleteAction();return;}
        if(VariableSingleton.HasSiegeStarted() && VariableSingleton.Instance.EnduringWall.GetHealth()<.65f && Get("iMoney")>1500) {
            manager.BringUpSiegeUI();Find<DefenseActions>().GetSelection("f"+VariableSingleton.Instance.EnduringWall.GetCodeName());Find<DefenseActions>().RepairSelected();return;
        }
        if(turn>20 && turn%9==0 && Get("iShips")>0 && Get("iMoney")>5000) {
            int ally=Enumerable.Range(0,CouncilActions.Allies.Length).FirstOrDefault(i=>Get("iAidState"+i)==0);
            if(CouncilActions.RequestAid(ally,out result)){ActionResults.Instance.Message=result;manager.CompleteAction();return;}
        }
        if(VariableSingleton.HasSiegeStarted() && variant==1 && turn%3==0 && Get("iMoney")>5000 && CouncilActions.SpyMission(2,false,out result)) {ActionResults.Instance.Message=result;manager.CompleteAction();return;}
        manager.ContinueToRest();
    }
    IEnumerator Play() {
        for(int focus=0;focus<5;focus++)for(int variant=0;variant<2;variant++) {
            current=new Run {focus=focus,seed=145300+focus*101+variant*17};report.runs.Add(current);
            VariableSingleton.TestSavePath=Path.GetFullPath(directory+"/save-"+focus+"-"+variant+".json");VariableSingleton.ContinueRequested=false;VariableSingleton.QuickStartRequested=false;
            SceneManager.LoadScene("Continue Game");yield return Frames(8);UnityEngine.Random.InitState(current.seed);
            for(int step=0;step<270 && !CampaignHUD.Instance.HasEnded;step++) {
                int oldTurn=VariableSingleton.Turn;Validate();
                if(CampaignHUD.Instance.HasModal)CampaignHUD.Instance.DismissReport();
                var manager=Find<ManagerUI>();if(!manager.gameObject.activeInHierarchy){current.issues.Add("Management inaccessible at "+oldTurn);break;}
                Major(manager,variant);yield return Frames();
                var rest=Find<RestUI>();if(!rest.gameObject.activeInHierarchy){current.issues.Add("Major action did not reach petitions at "+oldTurn);break;}
                rest.ContinueToEvent();yield return Frames(4);
                for(int phases=0;phases<8 && VariableSingleton.Turn==oldTurn && !CampaignHUD.Instance.HasEnded;phases++) {
                    var runner=Find<DialogueRunner>();int waits=0;string lastSelection=null;
                    while(runner.IsDialogueRunning && waits++<500) {
                        var option=Select(focus,variant);
                        if(option!=null && Find<EventUI>().Node+option.Option.Line.Text.Text!=lastSelection){lastSelection=Find<EventUI>().Node+option.Option.Line.Text.Text;current.trace.Add("turn "+oldTurn+" / "+Find<EventUI>().Node+" / "+option.Option.Line.Text.Text);option.InvokeOptionSelected();}
                        yield return new WaitForSecondsRealtime(.015f);
                    }
                    if(waits>=500){current.issues.Add("Dialogue stalled at "+Find<EventUI>().Node);break;}
                    var eventUI=Find<EventUI>();if(eventUI.gameObject.activeInHierarchy && CampaignHUD.Instance.HasModal){eventUI.ContinueToAssault();yield return Frames(4);}
                    if(CampaignHUD.Instance.HasEnded)break;
                    if(runner.IsDialogueRunning)continue;
                    var assault=Find<AssaultUI>();if(assault.gameObject.activeInHierarchy) {
                        string action=AI.Instance.LastReport;bool was=AI.Instance.WasAssault;assault.ShowResult();
                        if(AI.Instance.AwaitingBreachDecision){assault.DeployVarangiansDecision=AI.Instance.CanDeployGuards;assault.DecideVarangianDeployment();}
                        if(assault.gameObject.activeInHierarchy)assault.ContinueToEmperorAction();
                        current.trace.Add("turn "+oldTurn+" / Ottoman action / "+action+" / assault="+was);yield return Frames();
                    }
                }
                Validate();if(!CampaignHUD.Instance.HasEnded && VariableSingleton.Turn!=oldTurn+1){current.issues.Add("Turn advanced incorrectly from "+oldTurn+" to "+VariableSingleton.Turn);break;}
            }
            current.turns=VariableSingleton.Turn;current.days=VariableSingleton.CompletedSiegeDays;current.gold=Get("iMoney");current.provisions=Get("iProvisions");current.infantry=Get("iInfantry");current.archers=Get("iArchers");
            current.result=CampaignHUD.Instance.HasEnded?string.Join(" | ",GameObject.Find("Campaign Modal").GetComponentsInChildren<TMPro.TMP_Text>().Select(t=>t.text)):"Campaign stopped before an ending";
            File.WriteAllText(directory+"/report.json",JsonUtility.ToJson(report,true));Debug.Log("CAMPAIGN_RUN focus="+focus+" seed="+current.seed+" days="+current.days+" issues="+current.issues.Count);yield return Frames();
        }
        Finish();
    }
    void Finish() {if(finished)return;finished=true;Application.logMessageReceived-=Log;File.WriteAllText(directory+"/report.json",JsonUtility.ToJson(report,true));Debug.Log("TEN_CAMPAIGNS_COMPLETE runs="+report.runs.Count+" issues="+report.runs.Sum(r=>r.issues.Count));
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(report.runs.Count==10 && report.runs.All(r=>r.issues.Count==0)?0:1);
#else
        Application.Quit();
#endif
    }
}
#endif
