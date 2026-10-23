#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Yarn.Unity;

// Opt-in QA only. Real campaign/event code, automated choices; no normal launch path.
public class Campaign200Probe:MonoBehaviour
{
    static readonly string[] Groups={"mixed","scarcity","training","naval","espionage","politics","diplomacy","defence","poor-timing","archives"};
    bool diagnostic;string root,issuePath,tracePath,runPath;int run=-1,group,seed,lastTurn=-1,still,turnActions,assaults,choices,events,saveChecks,ourLoss,enemyLoss;string lastState="",fatal="",node="";System.Random policy;
    readonly HashSet<string> seenNodes=new HashSet<string>();readonly HashSet<string> runIssues=new HashSet<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(!Environment.GetCommandLineArgs().Contains("-campaign200"))return;var go=new GameObject("Campaign200 QA");DontDestroyOnLoad(go);go.AddComponent<Campaign200Probe>();}
    static string Q(object x)=>"\""+(x??"").ToString().Replace("\"","\"\"").Replace("\r"," ").Replace("\n"," ")+"\"";
    void Write(string path,params object[] values)=>File.AppendAllText(path,string.Join(",",values.Select(Q))+"\n");
    void Issue(string id,string detail){if(run<0)return;if(runIssues.Add(id))Write(issuePath,run+1,Groups[group],seed,VariableSingleton.IsInitialized?VariableSingleton.Turn:-1,node,id,detail);}
    void Log(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert){Issue("runtime-error",message+" | "+stack);fatal=message;}}
    IEnumerator Start(){
        Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;AudioListener.volume=0;
        diagnostic=Environment.GetCommandLineArgs().Contains("-campaign200-diagnostic");root=Path.GetFullPath(Path.Combine(Application.dataPath,diagnostic?"../Campaign200Diagnostic":"../Campaign200"));Directory.CreateDirectory(root);issuePath=Path.Combine(root,"issues.csv");tracePath=Path.Combine(root,"trace.csv");runPath=Path.Combine(root,"runs.csv");
        File.WriteAllText(issuePath,"run,group,seed,turn,node,issue,detail\n");File.WriteAllText(tracePath,"run,group,turn,node,kind,detail\n");File.WriteAllText(runPath,"run,group,seed,start,turn,siege_days,end,gold,people,food,infantry,archers,our_dead,enemy_dead,assaults,choices,nodes,savechecks,issues\n");
        File.WriteAllText(Path.Combine(root,"method.txt"),"Version31 source. 200 seeded automated campaigns,10x20. Groups scarcity/naval/espionage/politics/defence/poor-timing start at siege turn16 from default state, without preparation rewards; other groups start at turn0. Real Yarn options and campaign transitions; no frame-by-frame human input. Assault rounds resolved via authoritative PhasedAssault model. Poor-timing=-25% and late cover; defence=+25%; others neutral. Safety cap270 turns; stalls recorded rather than silently bypassed. Test saves isolated; no real achievements. Seeds310000..310199. No balance fixes during this sweep.\n");
        SaveGames.TestRoot=Path.Combine(root,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"current.besieged.json");Application.logMessageReceived+=Log;
        bool oldActive=PhasedAssaultUI.ActiveMode;PhasedAssaultUI.ActiveMode=false;
        for(run=diagnostic?180:0;run<(diagnostic?181:200);run++){
            group=run/20;seed=310000+run;policy=new System.Random(seed);fatal="";node="";seenNodes.Clear();runIssues.Clear();lastTurn=-1;still=turnActions=assaults=choices=events=saveChecks=0;lastState="";ourLoss=enemyLoss=0;
            SaveGames.PrepareNew(false);VariableSingleton.ContinueRequested=false;VariableSingleton.QuickStartRequested=false;SceneManager.LoadScene("Continue Game");yield return null;yield return null;yield return null;
            foreach(var view in FindObjectsOfType<OptionListButtons>(true))typeof(OptionListButtons).GetField("fadeTime",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(view,0f);
            UnityEngine.Random.InitState(seed);bool siegeStart=new[]{1,3,4,5,7,8}.Contains(group);if(siegeStart){var start=VariableSingleton.Instance.CaptureState();start.turn=16;VariableSingleton.Instance.RestoreValidated(start);}
            if(group==1){VariableSingleton.SetInt("iMoney",500);VariableSingleton.SetInt("iProvisions",200);}
            int initialFoot=I("iInfantry")+I("iArchers")+I("iCavalry")+I("iVarangians"),enemyStart=I("iBalkans")+I("iBazouks")+I("iJanissaries");string outcome="";
            for(int step=0;step<120000;step++){
                try{outcome=Step();}catch(Exception e){Issue("harness-or-runtime-exception",e.ToString());outcome="exception";}
                if(outcome!="")break;yield return null;
            }
            if(outcome==""){Issue("step-cap","120000 frame steps reached");outcome="step cap";}
            string ending=string.Join(" | ",FindObjectsOfType<TMPro.TMP_Text>().Where(t=>t.text.Contains("Siege days survived")).Select(t=>t.text));
            Write(runPath,run+1,Groups[group],seed,siegeStart?"siege scenario":"new campaign",VariableSingleton.Turn,VariableSingleton.CompletedSiegeDays,outcome+" "+ending,I("iMoney"),PopulationAccounting.People,I("iProvisions"),I("iInfantry"),I("iArchers"),ourLoss,enemyLoss,assaults,choices,seenNodes.Count,saveChecks,string.Join(";",runIssues));
            File.WriteAllText(Path.Combine(root,"run-"+(run+1).ToString("D3")+"-final.json"),JsonUtility.ToJson(VariableSingleton.Instance.CaptureState(),true));
            if((run+1)%10==0)Debug.Log("CAMPAIGN200_PROGRESS "+(run+1)+"/200 "+Groups[group]+" "+outcome);
        }
        PhasedAssaultUI.ActiveMode=oldActive;Application.logMessageReceived-=Log;File.WriteAllText(Path.Combine(root,"complete.txt"),diagnostic?"Single replay diagnostic completed.":"200 campaigns completed; inspect runs.csv, issues.csv, trace.csv and method.txt.\n");Debug.Log(diagnostic?"CAMPAIGN200_DIAGNOSTIC_COMPLETE":"CAMPAIGN200_COMPLETE 200");Application.Quit();
    }
    int I(string key)=>DailyLifeEvents.I(key);
    string Step(){
        if(diagnostic&&saveChecks>0)return "diagnostic complete";if(fatal!="")return "runtime error";if(SaveGames.Terminal)return "campaign ended";if(VariableSingleton.Turn>=270){Issue("turn-cap","Campaign reached270 turns without ending");return "turn cap";}
        if(lastTurn!=VariableSingleton.Turn){lastTurn=VariableSingleton.Turn;still=0;turnActions=0;CheckState();Write(tracePath,run+1,Groups[group],lastTurn,node,"turn","gold="+I("iMoney")+";food="+I("iProvisions")+";population="+PopulationAccounting.People+";ships="+I("iShips")+";enemyShips="+NavalState.EnemyShips+";morale="+VariableSingleton.GetFloatVariable("fTroopRep")+";wall="+VariableSingleton.Instance.EnduringWall.GetCodeName()+";integrity="+VariableSingleton.Instance.EnduringWall.GetHealth());}
        string state=VariableSingleton.Turn+":"+(FindObjectOfType<EventUI>()?.Node??"")+":"+FindObjectsOfType<OptionButton>().Count(o=>o.IsInteractable())+":"+(FindObjectOfType<RestUI>()!=null)+":"+(FindObjectOfType<AssaultUI>()!=null)+":"+(CampaignHUD.Instance.HasModal);
        if(state==lastState)still++;else{lastState=state;still=0;}if(still>1500){Issue("stalled-ui",state+" | "+string.Join(" | ",FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.text).Take(12)));return "stalled";}
        if(CampaignHUD.Instance.HasModal){var next=FindObjectsOfType<Button>().FirstOrDefault(b=>b.interactable&&b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Continue");if(next!=null)next.onClick.Invoke();return "";}
        var options=FindObjectsOfType<OptionButton>().Where(o=>o.IsInteractable()&&o.Option!=null).ToArray();
        if(options.Length>0){var ev=FindObjectOfType<EventUI>();node=ev?.Node??"";if(seenNodes.Add(node))events++;var option=options[policy.Next(options.Length)];Write(tracePath,run+1,Groups[group],lastTurn,node,"choice",option.Option.Line.Text.Text);choices++;option.InvokeOptionSelected();return "";}
        var prep=GameObject.Find("Preparation choices");if(prep!=null){var available=prep.GetComponentsInChildren<Button>().Where(b=>b.interactable).ToArray();if(available.Length==0){Issue("no-preparation","No unlocked preparation");return "blocked preparation";}var chosen=available[policy.Next(available.Length)];Write(tracePath,run+1,Groups[group],lastTurn,"","preparation",chosen.transform.parent.name);chosen.onClick.Invoke();return "";}
        var evui=FindObjectOfType<EventUI>();if(evui!=null){node=evui.Node;seenNodes.Add(node);return "";}
        var assault=FindObjectOfType<AssaultUI>();if(assault!=null){
            var b=AI.Instance.PendingPhased;if(b!=null&&!b.Committed){assaults++;float score=group==8?-.25f:group==7?.25f:0;b.Shelter(group==8?1:.65f);while(b.Shots<3)b.Volley(score);b.Approach(VariableSingleton.GetFloatVariable("fMoat"),TechnologySystem.Has("GreekFire")?b.Attackers*.05f:0);b.Sortie(0,0);b.AssignReserve(50);for(int i=0;i<3;i++)b.Clash();if(b.Frontline)b.Emergency(false);b.Resolve(b.Guards>=b.GuardsNeeded);AI.Instance.CommitPhased(b);ourLoss+=b.OurDead;enemyLoss+=b.Dead;Write(tracePath,run+1,Groups[group],lastTurn,"","assault",b.Outcome.Ledger);if(b.Dead>1000&&b.OurDead>0&&b.Dead/(float)b.OurDead>100)Issue("extreme-casualty-ratio","Enemy "+b.Dead+" / defenders "+b.OurDead);}
            if(AI.Instance.AwaitingBreachDecision)AI.Instance.BreachingAssaultAftermath(AI.Instance.CanDeployGuards);assault.ContinueToEmperorAction();return "";
        }
        var rest=FindObjectOfType<RestUI>();if(rest!=null){rest.ContinueToEvent();return "";}
        var manager=FindObjectOfType<ManagerUI>();if(manager!=null){if(turnActions++>0)return "";Manage(manager);return "";}
        return "";
    }
    void Manage(ManagerUI manager){string report="";bool acted=false;
        if(group==1&&SupplyEconomy.DailyNeed>0)SupplyEconomy.Buy("iProvisions",SupplyEconomy.DailyNeed,out report);
        if(group==2)acted=TrainingPool.Place(VariableSingleton.Turn%2==0?"iInfantry":"iArchers",100,10);
        if(group==3){if(I("iShips")<NavalState.EnemyShips)acted=TrainingPool.Place("iShips",1,1000);else if(I("iShips")>0&&VariableSingleton.Turn%7==0){NavalState.Battle(policy.Next(100)<NavalState.VictoryChance);acted=true;}else VariableSingleton.SetInt("iFleet",policy.Next(6));acted=true;}
        if(group==4)acted=CouncilActions.SpyMission(policy.Next(CouncilActions.SpyOrders.Length),false,out report);
        if(group==5)acted=PoliticalSystem.Aid(policy.Next(4),out report);
        if(group==6)acted=CouncilActions.RequestAid(policy.Next(CouncilActions.Allies.Length),out report);
        if(group==7){if(SupplyEconomy.DaysLeft<5)SupplyEconomy.Buy("iProvisions",SupplyEconomy.DailyNeed*10,out report);var wall=VariableSingleton.Instance.EnduringWall;if(wall.GetHealth()<.8f&&I("iMoney")>=1000){var repair=FindObjectOfType<DefenseActions>(true);repair.GetSelection("f"+wall.GetCodeName());repair.RepairSelected();return;}}
        if(group==9&&VariableSingleton.Turn%3==0){var before=VariableSingleton.Instance.CaptureState();SaveGames.Checkpoint();VariableSingleton.ChangeInt("iMoney",-1);if(!SaveGames.Load(VariableSingleton.TestSavePath,out report))Issue("save-load-failed",report);else if(JsonUtility.ToJson(before)!=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState())){Issue("save-roundtrip-mismatch","State differs after immediate reload");if(diagnostic){File.WriteAllText(Path.Combine(root,"before.json"),JsonUtility.ToJson(before,true));File.WriteAllText(Path.Combine(root,"after.json"),JsonUtility.ToJson(VariableSingleton.Instance.CaptureState(),true));}}saveChecks++;}
        if(acted){Write(tracePath,run+1,Groups[group],lastTurn,node,"action",report);ActionResults.Instance.Message=report;manager.CompleteAction();}else manager.ContinueToRest();
    }
    void CheckState(){var s=VariableSingleton.Instance.CaptureState();if(s.ints.Any(v=>v.value<0&&!new[]{"iCommander","iOttomanResolvedTurn"}.Contains(v.key)))Issue("negative-integer",string.Join(";",s.ints.Where(v=>v.value<0).Select(v=>v.key+"="+v.value)));if(s.floats.Any(v=>float.IsNaN(v.value)||float.IsInfinity(v.value)))Issue("nonfinite-state","Float corruption");if(Enumerable.Range(0,4).Sum(PoliticalSystem.Share)!=100)Issue("political-shares",string.Join(";",Enumerable.Range(0,4).Select(PoliticalSystem.Share)));}
}
#endif
