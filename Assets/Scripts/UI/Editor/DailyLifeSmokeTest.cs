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
public static class DailyLifeSmokeTest
{
    static DailyLifeSmokeTest(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("DailyLifeSmoke",false))new GameObject("Daily life smoke").AddComponent<DailyLifeSmokeProbe>();};}
    public static void Run(){AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);AssetDatabase.ImportAsset("Assets/Yarn/Test/NewProject.yarnproject",ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);SessionState.SetBool("DailyLifeSmoke",true);EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");EditorApplication.EnterPlaymode();}
}
public class DailyLifeSmokeProbe:MonoBehaviour
{
    readonly List<string> failures=new List<string>();int checks;float deadline;
    void Awake(){DontDestroyOnLoad(gameObject);VariableSingleton.TestSavePath=Path.GetFullPath("Logs/daily-life-test-save.json");deadline=Time.realtimeSinceStartup+100;Application.logMessageReceived+=Log;StartCoroutine(Run());}
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
        var runner=FindObjectOfType<DialogueRunner>(true);
        foreach(var n in DailyLifeEvents.Roots.Concat(new[]{"lifeFalseHope","lifeMorea","lifeMercenaries","lifeRefugees","lifeBarrelOwner","lifeSurprise","lifeSaintsDebate","lifeSaintsQuestion","combatAssassin","lifeGunnerResult"}))Check(runner.yarnProject.NodeNames.Contains(n),"Compiled "+n);
        var counts=Enumerable.Range(0,100).GroupBy(DailyLifeEvents.SailsOutcome).ToDictionary(g=>g.Key,g=>g.Count());
        Check(counts[0]==55&&counts[1]==15&&counts[2]==15&&counts[3]==15,"Exact 55/15/15/15 distribution");
        Set("iPopulation",100000);Set("iPopulationRemainder",0);Set("iPopulationFoodCarry",0);Set("iPopulationTaxCarry",0);
        var before=VariableSingleton.Instance.CaptureState();PopulationAccounting.Add(200);
        Check(PopulationAccounting.People==100200,"Exactly 200 refugees");
        Check(EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState()).Contains("+200"),"Outcome shows +200 residents");
        Check(Enumerable.Range(0,5).Sum(_=>PopulationAccounting.Food(true))==500,"Daily food rounds to the nearest thousand without accumulating fractions");
        Check(Enumerable.Range(0,5).Sum(_=>PopulationAccounting.Tax())==1503,"Fractional population tax carries correctly");
        PopulationAccounting.Add(800);Check(I("iPopulation")==101000,"Population stores individual people");
        DailyLifeEvents.Merchant(-15);Check(DailyLifeEvents.I("iMerchantFavor")==35,"Merchant favour defaults to50 then deducts15");
        DailyLifeEvents.Sails(1);Check(DailyLifeEvents.DueNode==null,"Sails not resolved same day");
        Set("iLifeSailsDue",TrainingPool.Day(VariableSingleton.GetDate()));Check(DailyLifeEvents.DueNode!=null,"Sails due deterministically");
        VariableSingleton.Instance.Save();Set("iLifeSailsDue",0);VariableSingleton.Instance.Load();Check(DailyLifeEvents.DueNode!=null,"Pending sighting survives save/load");
        Set("iLifeSailsDue",0);DailyLifeEvents.Barrel(3);Set("iLifeBarrelDue",TrainingPool.Day(VariableSingleton.GetDate()));Check(DailyLifeEvents.DueNode=="lifeBarrelOwner"&&I("iLifeBarrelUse")==3,"Intact infirmary barrel followup");
        Set("iEnemyDelay",1);Check(!DailyLifeEvents.CanSurprise(),"Land delay blocks surprise assault");
        Set("iEnemyDelay",0);Set("iBazouks",100);Check(DailyLifeEvents.CanSurprise(),"Surprise eligible with enemy forces");
        VariableSingleton.ChangePercentFloat("fHealth",1f);Set("iFencing",25);Check(EmperorCombat.Train(out var training)&&EmperorCombat.Skill==30,"Fencing training raises skill");
        Set("iEmperorAtWall",1);Check(EmperorCombat.WallKills==4,"Fencing increases personal wall kills");EmperorCombat.ConsumeWallPresence();Check(EmperorCombat.WallKills==0,"Wall participation consumed once");
        VariableSingleton.ChangePercentFloat("fHealth",1f);EmperorCombat.Fight(1);Check(EmperorCombat.Damage()>=10&&EmperorCombat.Damage()<=25,"Guard defence damage range");
        Set("iLifeMentorDue",0);Set("iEngineers",3);Set("iInfantry",100);DailyLifeEvents.Mentor();Check(I("iEngineers")==2&&I("iInfantry")==99,"Mentor and pupil temporarily removed");
        Set("iLifeSailsDue",0);Set("iLifeBarrelDue",0);Set("iLifeMentorDue",TrainingPool.Day(VariableSingleton.GetDate()));Check(DailyLifeEvents.DueNode=="lifeGunnerResult","Mentor examination queued");
        DailyLifeEvents.Seen("lifeSong");Check(I("iLifeCooldownlifeSong")==TrainingPool.Day(VariableSingleton.GetDate())+30,"Thirty day repeat cooldown");
        Check(Enumerable.Range(0,100).Count(r=>SiegeOpeningIntelligence.RecruitsForRoll(r)==5)==3,"Five-agent opening result exactly 3 percent");
        Check(Enumerable.Range(0,100).Count(r=>SiegeOpeningIntelligence.RecruitsForRoll(r)<=2)==80,"One or two agents on 80 percent of rolls");
        Check(DiplomaticChronicles.All.Length==CouncilActions.Allies.Length,"All realm profiles match aid roster");
        Check(Resources.Load<Texture2D>("UI/Identity/faith-medallions")!=null && Resources.Load<Texture2D>("UI/Identity/court-portraits")!=null,"Faith and court artwork loads");
        foreach(var n in SiegeHardships.Roots.Concat(new[]{"hardGrave1","hardGrave2","hardSand1","hardSand2","hardWoman1","hardMissing1","hardMissing2","sermonGreed","sermonLust","sermonEnvy","sermonGluttony","sermonWrath","sermonSloth"}))Check(runner.yarnProject.NodeNames.Contains(n),"Compiled hardship "+n);
        SiegeHardships.Next("Grave",1,3);Check(SiegeHardships.DueNode==null,"Investigation is delayed");Set("iHardGraveDue",TrainingPool.Day(VariableSingleton.GetDate()));Check(SiegeHardships.DueNode=="hardGrave1","Investigation due node");
        VariableSingleton.Instance.Save();SiegeHardships.Next("Grave",99,0);VariableSingleton.Instance.Load();Check(SiegeHardships.DueNode=="hardGrave1","Investigation state survives save/load");SiegeHardships.Next("Grave",99,0);Check(SiegeHardships.DueNode==null,"Closed investigation cannot repeat");
        SiegeHardships.Next("Sermon",2,7);Set("iHardSermonDue",TrainingPool.Day(VariableSingleton.GetDate()));Check(SiegeHardships.DueNode=="sermonGreed","Sermons advance in order");SiegeHardships.Next("Sermon",99,0);Check(SiegeHardships.DueNode==null,"Arrest closes sermons");
        Set("iHardPressure",0);SiegeHardships.Pressure(1000);Check(SiegeHardships.PressureValue()==100,"Sermon pressure bounded");SiegeHardships.Pressure(-1000);Check(SiegeHardships.PressureValue()==0,"Reform cannot create negative unrest");
        Set("iHardWomanSpy",0);Set("iMoney",2000);int agents=I("iSpies");SiegeHardships.Recruit();SiegeHardships.Recruit();Check(I("iMoney")==1000&&I("iSpies")==agents+1,"Theodora can only be hired once");
        NavalState.Initialise();Set("iShips",60);Check(NavalState.EnemyShips==48&&NavalState.Uncontested,"48 starting Ottoman vessels; more friendly ships controls sea");
        Set("iShips",48);Check(!NavalState.Uncontested,"Equal fleets contest sea");
        for(int sample=0;sample<30;sample++){Set("iShips",100);Set("iTurkShips",100);NavalState.Battle(true);Check(I("iShips")>=85&&I("iShips")<=95&&I("iTurkShips")>=35&&I("iTurkShips")<=60,"Victory loss bounds");Set("iShips",100);Set("iTurkShips",100);NavalState.Battle(false);Check(I("iShips")>=50&&I("iShips")<=75&&I("iTurkShips")>=75&&I("iTurkShips")<=90,"Defeat loss bounds");}
        Set("iTurkShips",48);Set("iNavalLastDay",0);NavalState.CompleteDay();NavalState.CompleteDay();Check(I("iTurkShips")==49,"One Ottoman replacement per date");
        VariableSingleton.Instance.Save();Set("iTurkShips",1);VariableSingleton.Instance.Load();Check(I("iTurkShips")==49,"Naval strength persists");
        Set("iShips",10);Check(RecurringEvents.FleetDefensiveDamage>0,"Defensive fleet damage available");int ten=RecurringEvents.FleetDefensiveDamage;Set("iShips",20);Check(RecurringEvents.FleetDefensiveDamage==ten*2,"Defensive fire scales with ship count");
        Check(runner.yarnProject.NodeNames.Contains("random23"),"Artillery convoy compiled");
        foreach(var mission in new[]{0,1,2,4})Check(ReliefStormSystem.Exposed(mission),"Open-water mission exposed to storm");
        Check(!ReliefStormSystem.Exposed(3),"Harbour mission protected from storms");
        for(int a=0;a<CouncilActions.Allies.Length;a++){Set("iAidState"+a,0);Set("iAidDays"+a,0);}
        Set("iAidState1",1);Set("iAidDays1",7);Set("iAidState0",1);Set("iAidDays0",9);Set("iShips",3);Set("iTurkShips",4);Set("iFleet",3);
        Check(ReliefStormSystem.Incoming().SequenceEqual(new[]{1}),"Storm selects maritime relief, excludes land expedition");
        ReliefStormSystem.Apply(1,1,10,5);Check(I("iAidDays1")==17&&I("iAidDays0")==9,"Storm adds exactly ten days to selected relief only");Check(I("iShips")==3&&I("iTurkShips")==0,"Harbour protected and Ottoman storm losses capped");
        Set("iFleet",0);Set("iTurkShips",20);ReliefStormSystem.Apply(2,1,10,5);Check(I("iAidState1")==4&&I("iAidDays1")==0&&I("iShips")==0&&I("iTurkShips")==10,"Catastrophic storm loses relief and bounds our fleet losses");
        int food=I("iProvisions");CouncilActions.AdvanceAid(100);Check(I("iAidState1")==4&&I("iProvisions")==food,"Lost relief never delivers cargo");
        ReliefStormSystem.Apply(1,-1,1,0);Check(ReliefStormSystem.Report().Contains("no relief fleet"),"Storm explains when no aid was at sea");
        VariableSingleton.Instance.Save();Set("iAidState1",0);VariableSingleton.Instance.Load();Check(I("iAidState1")==4,"Lost relief state persists through checkpoint");
        DailyLifeEvents.Seen("hardStorm");Check(!SiegeHardships.Available().Contains("hardStorm"),"Storm respects thirty-day cooldown");
        PreparationCampaign.SetGalataState(0);Set("iGalataSupportRequested",0);int goldBefore=I("iMoney");Check(PreparationCampaign.DiplomaticAction(8,out var galataReport)&&I("iMoney")==goldBefore+4500,"Galata immediate support pays stated gold");Check(!PreparationCampaign.DiplomaticAction(8,out galataReport),"Galata immediate support cannot repeat");
        Set("iGalataNeutralPledge",0);Check(PreparationCampaign.DiplomaticAction(9,out galataReport)&&!PreparationCampaign.DiplomaticAction(9,out galataReport),"Friendly neutrality pledge only once");
        Set("iMoney",0);Check(PreparationCampaign.GalataActionUnavailable(0)!=null,"Galata menu explains missing bribe funds");
        Set("iAdventurerChoice",1);Set("iAdventurerArrived",0);Check(!CharacterPreparations.Appoint(out var visitorReport)&&CharacterPreparations.Bonus(0)==0,"Visitors cannot command or grant bonuses before arrival");
        Set("iAdventurerArrived",1);Set("iCommander",-1);Check(Mathf.Abs(CharacterPreparations.Bonus(0)-.04f)<.0001f,"Stephen grants four percent infantry benefit");
        Check(CharacterPreparations.Appoint(out visitorReport)&&I("iCommander")==100&&Mathf.Abs(CharacterPreparations.Bonus(0)-.12f)<.0001f,"Stephen appoints as commander with twelve percent infantry benefit");
        Check(!CharacterPreparations.Appoint(out visitorReport),"Repeated same commander appointment rejected");
        VariableSingleton.ChangeBool(CourtRoster.Keys[0],false);Check(Mathf.Abs(CourtRoster.Bonus(0,.05f,.15f)-.12f)<.0001f,"Visitor benefit reaches actual combat hook without original specialist");
        VariableSingleton.ChangeBool(CourtRoster.Keys[0],true);Check(Mathf.Abs(CourtRoster.Bonus(0,.05f,.15f)-.17f)<.0001f,"Serving specialist and visiting commander combine without double command bonus");
        VariableSingleton.Instance.Save();Set("iCommander",-1);VariableSingleton.Instance.Load();Check(CharacterPreparations.IsCommander&&CharacterPreparations.Loyalty==50,"Visiting commander and loyalty persist in checkpoint");
        int[] visitorChannels={0,4,6,5,7,3};float[] visitorValues={.12f,12,4,.12f,6,.09f};
        for(int v=0;v<6;v++){Set("iAdventurerChoice",v+1);Set("iAdventurerArrived",1);Set("iCommander",-1);Check(CharacterPreparations.Appoint(out visitorReport)&&Mathf.Abs(CharacterPreparations.Bonus(visitorChannels[v])-visitorValues[v])<.0001f,"Visiting commander functional bonus "+v);Check(CourtRoster.CommanderName(100+v)==CharacterPreparations.Names[v],"Visiting commander correctly named "+v);}
        Check(!CourtRoster.InService(8),"Enemy champion excluded from serving court");
        VariableSingleton.ChangeBool(CourtRoster.Keys[0],false);Check(!CourtRoster.InService(0),"Inactive officer excluded from serving court");
        DailyLifeEvents.Poison();Check(I("iLifePoisonEnding")==1&&VariableSingleton.GetFloatVariable("fHealth")==0,"Poison is lethal and ending-specific");
    }
    void Finish(){SessionState.SetBool("DailyLifeSmoke",false);Application.logMessageReceived-=Log;Debug.Log("DAILY_LIFE_SMOKE_COMPLETED checks="+checks+" failures="+failures.Count+"\n"+string.Join("\n",failures));EditorApplication.Exit(failures.Count==0?0:1);}
}
