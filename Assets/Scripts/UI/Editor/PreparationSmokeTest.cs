using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Yarn.Unity;

[InitializeOnLoad]
public static class PreparationSmokeTest
{
    static PreparationSmokeTest(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("PreparationSmoke",false))new GameObject("Preparation smoke").AddComponent<PreparationSmokeProbe>();};}
    public static void Run(){SessionState.SetBool("PreparationSmoke",true);EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");EditorApplication.EnterPlaymode();}
}
public class PreparationSmokeProbe:MonoBehaviour
{
    readonly List<string> failures=new List<string>();int checks;float deadline;
    void Awake(){DontDestroyOnLoad(gameObject);VariableSingleton.TestSavePath=Path.GetFullPath("Logs/preparation-test-save.json");deadline=Time.realtimeSinceStartup+100;Application.logMessageReceived+=Log;StartCoroutine(Run());}
    void Log(string msg,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)failures.Add(msg);}
    void Update(){if(Time.realtimeSinceStartup>deadline){failures.Add("Test timeout");Finish();}}
    void Check(bool result,string name){checks++;if(!result)failures.Add(name);}
    int I(string key)=>VariableSingleton.GetIntVariable(key);
    void Set(string key,int value)=>VariableSingleton.SetInt(key,value);
    IEnumerator Run(){yield return null;FindObjectOfType<MainMenuUI>().NewGame();for(int i=0;i<10;i++)yield return null;
        try{Tests();}catch(Exception e){failures.Add(e.ToString());}Finish();
    }
    void Tests()
    {
        Check(PreparationCampaign.Active&&VariableSingleton.GetDate()==new DateTime(1452,1,1),"New campaign starts January 1452");
        var runner=FindObjectOfType<DialogueRunner>(true);
        Check(PreparationCampaign.Nodes.Length==PreparationCampaign.UnlockCounts.Length && PreparationCampaign.Nodes.Length==PreparationCampaign.Summaries.Length,"Preparation arrays aligned");
        var original=VariableSingleton.Instance.CaptureState();
        int enemies=I("iTurkSpies");CharacterPreparations.TurkishHouseholds(3);
        Check(I("iTurkSpies")==enemies+2,"Liberal policy adds two hidden infiltrators");
        Check(!EventOutcome.Describe(original,VariableSingleton.Instance.CaptureState()).Contains("Ottoman spies"),"Policy outcome hides infiltrator effect");
        CharacterPreparations.TurkishHouseholds(3);Check(I("iTurkSpies")==enemies+2,"Policy reward cannot repeat");
        typeof(VariableSingleton).GetMethod("RestoreState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(VariableSingleton.Instance,new object[]{original});
        CharacterPreparations.Invite(2);CharacterPreparations.Invite(3);Check(I("iAdventurerChoice")==3,"Only one invitation allowed");
        Check(CharacterPreparations.Advance()=="","Guest does not arrive early");
        Set("iAdventurerDue",TrainingPool.Day(VariableSingleton.GetDate()));Check(CharacterPreparations.Advance().Contains("Bertrandon"),"Guest arrival names selected candidate");
        Check(CharacterPreparations.Advance()=="","Guest arrival occurs once");
        typeof(VariableSingleton).GetMethod("RestoreState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(VariableSingleton.Instance,new object[]{original});
        foreach(string node in PreparationCampaign.Nodes.Concat(new[]{"prepFortress","prepGalataGovernor","galata26","galata32","giovanniThrone"}))Check(runner.yarnProject.NodeNames.Contains(node),"Compiled Yarn node "+node);
        Check(PreparationCampaign.FixedNode(3)=="prepFortress"&&PreparationCampaign.FixedNode(11)=="story1","April warning and December union fixed");
        VariableSingleton.IncrementTurn();Check(PreparationCampaign.Flexible(VariableSingleton.Turn),"February selectable");
        Check(PreparationCampaign.Unavailable(1,0)==null,"Christian power is base selectable");
        Check(PreparationCampaign.Unavailable(11,0)!=null&&PreparationCampaign.Unavailable(11,1)==null,"Career count unlocks grain missions");
        int army=I("iArmySupport");PreparationCampaign.Complete("story14");PreparationCampaign.Complete("story14");Check(I("iArmySupport")==army+10&&PreparationCampaign.Unavailable(8,3)!=null,"Commander support awarded once");
        Set("iVarangians",50);Set("iSecrets",0);PreparationCampaign.Recon("varangians",false);Check(I("iVarangians")==45&&I("iRumeliRecon")==0,"Failed guard recon loses exactly five");
        Set("iSpies",3);PreparationCampaign.Recon("spy",false);Check(I("iSpies")==2,"Failed spy recon loses one");
        PreparationCampaign.Recon("spy",true);Check(I("iSpies")==2&&I("iSecrets")==1&&PreparationCampaign.RumeliChance()==75,"Successful recon returns agent and adds secret and 30 percentage points");
        Set("iMoney",100000);Set("iEnemyDelay",0);Set("iSpies",0);Set("iSecrets",0);
        PreparationCampaign.Intelligence(0);Check(I("iSpies")==1&&I("iEnemyDelay")==2&&I("iArrivalAlert")==25,"Beggar mission effects");
        PreparationCampaign.Intelligence(1);Check(I("iMoney")==98000&&I("iSpies")==3&&I("iSecrets")==1,"Merchant and companion effects");
        Set("iFocus",4);PreparationCampaign.Intelligence(3);Check(I("iMoney")==95000&&I("iSpies")==7,"Diplomatic four-agent mission");
        Set("iFocus",1);Set("iCavalry",50);Set("iBalkans",1000);int infantry=I("iInfantry"),archers=I("iArchers");PreparationCampaign.Scorched(4);
        Check(I("iCavalry")==100&&I("iInfantry")==infantry+300&&I("iArchers")==archers+150&&I("iBalkans")==500,"Serbian defection conserves troop counts");
        VariableSingleton.ChangeBool("bCharWarrior0",true);Set("iFocus",4);PreparationCampaign.Capture("giovanni");Check(PreparationCampaign.GalataState==1&&I("iLoyalty0")==40,"Giovanni capture costs ten loyalty");
        PreparationCampaign.Grant("now");Check(PreparationCampaign.GalataState==2&&I("iLoyalty0")==70&&I("iGalataDefences")==125,"Immediate grant creates autonomy and adds thirty loyalty");
        bool rejected=false;try{PreparationCampaign.Grant("now");}catch(InvalidOperationException){rejected=true;}Check(rejected,"Governance cannot repeat without capture");
        Set("iShips",10);PreparationCampaign.Retreat();Check(I("iShips")==5&&I("iDamagedShips")==2,"Fleet defeat leaves operational and repairable survivors");
        PreparationCampaign.PrepareFleet(0);Check(PreparationCampaign.FleetMultiplier(0,false)==1.5f,"Preparation improves first report");
        PreparationCampaign.ChangeFleet(1,out var report);Check(I("iFleet")==1&&I("iFleetPrepared")==0,"Core orders clear bonus");
        PreparationCampaign.Grain(0);Check(I("iGrainCargo")==3000,"Overseas grain records cargo");
        VariableSingleton.Instance.Save();Set("iRumeliRecon",0);Set("iLoyalty0",0);VariableSingleton.Instance.Load();Check(I("iRumeliRecon")==1&&I("iLoyalty0")==70&&PreparationCampaign.Active,"Save restores new campaign state");
        while(VariableSingleton.Turn<16)VariableSingleton.IncrementTurn();
        Check(VariableSingleton.GetDate()==new DateTime(1453,4,6)&&VariableSingleton.HasSiegeStarted(),"Siege starts April 6 after fixed and flexible calendar");
        Check(I("iArrivalAlert")==0&&I("iCampAlert")>=25,"Arrival alert applied once");
        Check(I("iGrainDue")==0&&I("iGrainCargo")==0,"Delayed grain delivered during calendar advance");
        Set("iEnemyDelay",1);Check(!PreparationCampaign.CanAttackGalata,"Land delay protects Galata");Set("iEnemyDelay",0);Check(PreparationCampaign.CanAttackGalata,"Controlled Galata becomes attackable");
        Set("iGalataDefences",1);PreparationCampaign.AttackGalata();Check(PreparationCampaign.GalataState==3,"Galata falls when defences exhausted");
        PreparationCampaign.SetGalataState(0);Check(!PreparationCampaign.CanAttackGalata,"Neutral Galata not attacked");
        VariableSingleton.ChangeFloat("fGalataRep",-1);Check(PreparationCampaign.OptionalNodes().Contains("galata32"),"Lowest goodwill permits betrayal");PreparationCampaign.Complete("galata32");Check(!PreparationCampaign.OptionalNodes().Contains("galata32"),"Optional event cannot repeat");
        PreparationCampaign.SetGalataState(2);Set("iLoyalty0",20);Check(PreparationCampaign.OptionalNodes().Contains("giovanniWarning")&&!PreparationCampaign.OptionalNodes().Contains("giovanniThrone"),"Warning precedes throne crisis");Set("iGiovanniWarning",1);Check(PreparationCampaign.OptionalNodes().Contains("giovanniThrone"),"Unresolved warning enables throne challenge");
    }
    void Finish(){SessionState.SetBool("PreparationSmoke",false);Application.logMessageReceived-=Log;Debug.Log("PREPARATION_SMOKE_COMPLETED checks="+checks+" failures="+failures.Count+"\n"+string.Join("\n",failures));EditorApplication.Exit(failures.Count==0?0:1);}
}
