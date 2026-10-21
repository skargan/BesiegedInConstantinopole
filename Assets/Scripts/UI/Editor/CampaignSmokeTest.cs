using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;

[InitializeOnLoad]
public static class CampaignSmokeTest
{
    static CampaignSmokeTest()
    {
        EditorApplication.playModeStateChanged += state => {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("BesiegedSmoke", false))
            {
                new GameObject("Campaign Smoke Test").AddComponent<CampaignSmokeProbe>();
            }
        };
    }
    public static void Run()
    {
        ValidateStory.Run();
        SessionState.SetBool("BesiegedSmoke", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
        EditorApplication.EnterPlaymode();
    }
}

public class CampaignSmokeProbe : MonoBehaviour
{
    private readonly List<string> failures = new List<string>();
    private readonly List<string> checks = new List<string>();
    private float deadline;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Application.logMessageReceived += Log;
        deadline = Time.realtimeSinceStartup + 160;
        VariableSingleton.TestSavePath = Path.GetFullPath("Logs/smoke-save.json");
        StartCoroutine(Test());
    }
    private void Log(string message, string stack, LogType type)
    {
        if (message.Contains("Can't start dialogue") || type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            failures.Add(message + "\n" + stack);
    }
    private void Update()
    {
        if (Time.realtimeSinceStartup > deadline) Finish("Timed out.");
    }
    private void Check(bool success, string name)
    {
        (success ? checks : failures).Add(name);
    }
    private static T Find<T>() where T : UnityEngine.Object { return FindObjectOfType<T>(true); }
    private IEnumerator Frames(int count = 5)
    {
        for (int i = 0; i < count; i++) yield return null;
    }
    private IEnumerator Test()
    {
        yield return Frames();
        Check(Find<MainMenuUI>() != null, "Main menu exists");
        Find<MainMenuUI>().NewGame();
        yield return Frames(10);
        var manager = Find<ManagerUI>();
        Check(manager.gameObject.activeInHierarchy, "New game enters management");
        Check(VariableSingleton.Turn == 0, "New game resets turn");
        Check(Find<DialogueRunner>().yarnProject.NodeNames.Contains("story0"), "Story project contains opening event");
        Check(VariableSingleton.GetIntVariable("iMoney") == 100000, "New game loads resources");
        Check(VariableSingleton.GetIntVariable("iPopulation") == 100, "Population represents 100,000 residents");
        Check(VariableSingleton.HasSave(), "Management checkpoint written");
        VariableSingleton.ChangeInt("iMoney", -123);
        VariableSingleton.Instance.Load();
        Check(VariableSingleton.GetIntVariable("iMoney") == 100000, "Load restores money");

        var reviewCouncil=CouncilUI.For(manager);
        string beforeReview=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
        for(int audience=0;audience<5;audience++) {
            reviewCouncil.ReviewFaction(audience);
            Check(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState())==beforeReview,"Audience "+audience+" preview leaves campaign and RNG unchanged");
            ControlSingleton.Instance.GoBack();
            Check(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState())==beforeReview,"Audience "+audience+" return leaves campaign unchanged");
        }
        reviewCouncil.ReviewFaction(3);
        var confirmAction=GameObject.Find("Confirm society action").GetComponent<Button>();
        int goldBefore=CouncilActions.Int("iMoney");float clergyBefore=VariableSingleton.GetFloatVariable("fPriestRep");
        confirmAction.onClick.Invoke();confirmAction.onClick.Invoke();
        Check(CouncilActions.Int("iMoney")==goldBefore-300,"Confirm charges once despite double invocation");
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fPriestRep")-clergyBefore-.03f)<.0001f,"Confirm applies clergy support once");
        Check(Find<RestUI>().gameObject.activeInHierarchy,"Confirmed action opens completed-order screen");
        Check(!Find<RestUI>().GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="100%"),"Completed order hides legacy percentage placeholder");
        Find<RestUI>().gameObject.SetActive(false);VariableSingleton.Instance.Load();manager.gameObject.SetActive(true);
        VariableSingleton.SetInt("iMoney",299);reviewCouncil.ReviewFaction(3);
        Check(!GameObject.Find("Confirm society action").GetComponent<Button>().interactable,"Unaffordable confirmation disabled");
        VariableSingleton.Instance.Load();VariableSingleton.ChangePercentFloat("fPriestRep",.44f);reviewCouncil.ReviewFaction(3);
        Check(GameObject.Find("Review BENEFIT").GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="+1 point"),"Review previews capped support gain");
        reviewCouncil.Hide();ControlSingleton.Instance.ClearHistory();VariableSingleton.Instance.Load();manager.gameObject.SetActive(true);
        Check(EspionageSystem.Alert==0,"New campaign camp alert starts at zero");
        Check(EspionageSystem.IntelligenceValue(7)=="Unknown","Locked intelligence conceals its value");
        for(int mission=0;mission<CouncilActions.SpyOrders.Length;mission++) {
            VariableSingleton.Instance.Load();
            VariableSingleton.SetInt("iSpies",8);VariableSingleton.SetInt("iCampAlert",40);
            VariableSingleton.SetInt("iTurkSpies",3);VariableSingleton.SetInt("iFood",20);VariableSingleton.SetInt("iTurkCannons",8);
            Check(EspionageSystem.LossChance(mission)==(mission==3?5:25),"Mission "+mission+" risk uses starting alert");
            int money=CouncilActions.Int("iMoney");
            Check(CouncilActions.SpyMission(mission,false,out var result),"Mission "+mission+" executes");
            Check(EspionageSystem.Alert==40+EspionageSystem.AlertGain[mission],"Mission "+mission+" exact alert increase");
            Check(CouncilActions.Int("iMoney")==money-1000,"Mission "+mission+" charged once");
            int expected=mission==0?9:8;
            Check(CouncilActions.Int("iSpies")==expected || CouncilActions.Int("iSpies")==expected-1,"Mission "+mission+" loses at most one agent");
        }
        VariableSingleton.Instance.Load();
        VariableSingleton.SetInt("iEnemyDelay",6);VariableSingleton.SetInt("iSecrets",2);VariableSingleton.SetInt("iSpies",3);
        int disruptionGold=CouncilActions.Int("iMoney");
        Check(CouncilActions.SpyMission(5,true,out var disruptionReport),"Disruption accepts secret payment");
        Check(CouncilActions.Int("iEnemyDelay")==7 && CouncilActions.Int("iSecrets")==1 && CouncilActions.Int("iMoney")==disruptionGold,"Disruption stacks with castle delay and charges only selected currency");
        VariableSingleton.SetInt("iSpies",0);
        Check(!CouncilActions.SpyMission(5,true,out disruptionReport) && CouncilActions.Int("iEnemyDelay")==7 && CouncilActions.Int("iSecrets")==1,"Disruption without agents spends nothing");
        VariableSingleton.SetInt("iSpies",3);VariableSingleton.SetInt("iSecrets",0);
        Check(!CouncilActions.SpyMission(5,true,out disruptionReport) && CouncilActions.Int("iEnemyDelay")==7,"Unaffordable disruption leaves delay unchanged");
        var disruptionSave=VariableSingleton.Instance.CaptureState();
        VariableSingleton.SetInt("iEnemyDelay",0);
        typeof(VariableSingleton).GetMethod("RestoreState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(VariableSingleton.Instance,new object[]{disruptionSave});
        Check(CouncilActions.Int("iEnemyDelay")==7,"Disruption delay survives save restoration");
        VariableSingleton.SetInt("iCampAlert",99);
        Check(EspionageSystem.FinishOperation(2,100),"Certain loss removes an agent");
        Check(EspionageSystem.Alert==100,"Alert caps at 100");
        Check(EspionageSystem.LossChance(1)==55,"Maximum camp risk is 55 percent");
        int startingAgents=CouncilActions.Int("iSpies");
        Check(!EspionageSystem.FinishOperation(3,0) && CouncilActions.Int("iSpies")==startingAgents,"Zero risk leaves agents intact");
        VariableSingleton.SetInt("iCampAlert",50);int previousTurn=VariableSingleton.Turn;
        VariableSingleton.IncrementTurn();
        Check(EspionageSystem.Alert>=40 && EspionageSystem.Alert<=49 && VariableSingleton.Turn==previousTurn+1,"Turn reduces alert by one D10");
        VariableSingleton.SetInt("iCampAlert",1);EspionageSystem.AdvanceTurn();Check(EspionageSystem.Alert==0,"Decay floors at zero");
        VariableSingleton.SetInt("iCampAlert",73);
        var saved=VariableSingleton.Instance.CaptureState();
        var restore=typeof(VariableSingleton).GetMethod("RestoreState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
        VariableSingleton.SetInt("iCampAlert",0);restore.Invoke(VariableSingleton.Instance,new object[]{saved});Check(EspionageSystem.Alert==73,"Alert restores from saved state");
        saved.ints.RemoveAll(v=>v.key=="iCampAlert");restore.Invoke(VariableSingleton.Instance,new object[]{saved});Check(EspionageSystem.Alert==0,"Old save defaults alert to zero");
        VariableSingleton.Instance.Load();
        VariableSingleton.SetInt("iMoney",0);int unchanged=EspionageSystem.Alert;
        Check(!CouncilActions.SpyMission(0,false,out var denied) && EspionageSystem.Alert==unchanged,"Unaffordable mission has no alert effect");
        VariableSingleton.Instance.Load();
        string councilReport;
        Check(CouncilActions.SupportFaction(0,out councilReport), "Faction audience succeeds");
        Check(VariableSingleton.GetIntVariable("iMoney")==99700, "Faction audience costs 300 gold");
        Check(councilReport.Contains("now"), "Faction outcome gives new totals");
        VariableSingleton.Instance.Load();
        int agents=CouncilActions.Int("iSpies");
        Check(CouncilActions.SpyMission(0,false,out councilReport), "Recruit agent succeeds");
        Check((CouncilActions.Int("iSpies")==agents+1 || CouncilActions.Int("iSpies")==agents) && CouncilActions.Int("iMoney")==99000, "Agent recruitment exact cost and gain");
        VariableSingleton.Instance.Load();
        VariableSingleton.SetInt("iMoney",0);
        Check(!CouncilActions.SpyMission(0,false,out councilReport), "Spy operation rejects unaffordable payment");
        VariableSingleton.Instance.Load();
        int foodBefore=CouncilActions.Int("iFood");
        Check(CouncilActions.SpyMission(2,false,out councilReport),"Food sabotage is playable");
        Check(foodBefore-CouncilActions.Int("iFood")>=1 && foodBefore-CouncilActions.Int("iFood")<=10,"Food sabotage burns 1 to 10 stores");
        VariableSingleton.Instance.Load();
        VariableSingleton.SetInt("iFood",1);
        int irregulars=CouncilActions.Int("iBazouks");
        Check(CouncilActions.ResolveEnemySupplies().Contains("instead of attacking"),"Shortage replaces attack with resupply");
        Check(CouncilActions.Int("iFood")==15 && CouncilActions.Int("iBazouks")==irregulars-Mathf.CeilToInt(irregulars*.01f),"Shortage causes exact desertion and replenishment");
        Check(CouncilActions.ResolveEnemySupplies()=="" && CouncilActions.Int("iFood")==14,"Supplied day consumes one store without another desertion");
        VariableSingleton.Instance.Load();
        UnityEngine.Random.InitState(11);
        Check(CouncilActions.RequestAid(0,out councilReport), "Diplomatic appeal resolves");
        Check(CouncilActions.Int("iMoney")==99000, "Appeal exact cost");
        Check(!CouncilActions.RequestAid(0,out councilReport), "Duplicate appeal rejected");
        VariableSingleton.SetInt("iAidState0",1);VariableSingleton.SetInt("iAidDays0",2);
        int infantryBefore=CouncilActions.Int("iInfantry");
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        Check(CouncilActions.Int("iAidDays0")==2, "Aid countdown survives checkpoint");
        Check(CouncilActions.AdvanceAid(1)=="", "Aid does not arrive early");
        Check(CouncilActions.AdvanceAid(1).Contains("Hungary") && CouncilActions.Int("iInfantry")==infantryBefore+5000, "Aid delivers troops on due day");
        CouncilActions.AdvanceAid(10);
        Check(CouncilActions.Int("iInfantry")==infantryBefore+5000, "Aid cannot deliver twice");
        foreach(int screen in new[]{0,1,2})
        {
            if(screen==0) manager.BringUpDiplomacy(); else if(screen==1) manager.BringUpSpiesUI(); else manager.BringUpPeopleUI();
            Check(!manager.gameObject.activeSelf && ControlSingleton.Instance.CanGoBack, "Council navigation opens screen "+screen);
            ControlSingleton.Instance.GoBack();
            Check(manager.gameObject.activeSelf, "Council Return restores management "+screen);
        }
        SceneManager.LoadScene("Continue Game");yield return Frames(10);manager=Find<ManagerUI>();

        manager.BringUpTroopsUI();
        yield return Frames();
        Check(Find<PercentConscriptedUpdater>().GetComponent<TMP_Text>().text.Contains("5.2%"), "Conscription displays 5.2 percent");
        Check(Find<RecruitationSelection>() != null, "Recruitment panel exists");
        var recruitment = Find<RecruitationSelection>();
        recruitment.GetSelectionIndex(0);
        recruitment.GetComponentInChildren<ValueIncrementer>(true).OverrideNumber("2");
        recruitment.CommitRecruitmentTransaction();
        Check(VariableSingleton.GetIntVariable("iInfantry") == 3000 && TrainingPool.Orders.Count==1 && TrainingPool.Orders[0].quantity==2, "Recruitment queues two soldiers without immediate strength");
        CampaignHUD.Instance.DismissReport();
        Check(VariableSingleton.GetIntVariable("iMoney") == 99980, "Recruitment deducts exact price");
        Check(VariableSingleton.Turn == 0, "Recruitment does not spend a turn");
        Check(!recruitment.gameObject.activeInHierarchy, "Recruitment closes overlay after purchase");
        ControlSingleton.Instance.GoBack();
        Check(manager.gameObject.activeInHierarchy, "Return reaches management after recruitment");
        var orderedDate=VariableSingleton.GetDate();
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        Check(TrainingPool.Orders.Count==1 && TrainingPool.Remaining(TrainingPool.Orders[0])==5,"Training pool survives save and load");
        Check(TrainingPool.CompleteDueOrders(orderedDate.AddDays(4))=="" && CouncilActions.Int("iInfantry")==3000,"Infantry cannot complete early");
        Check(TrainingPool.CompleteDueOrders(orderedDate.AddDays(5)).Contains("Infantry") && CouncilActions.Int("iInfantry")==3002,"Infantry completes on day five");
        TrainingPool.CompleteDueOrders(orderedDate.AddDays(5));
        Check(CouncilActions.Int("iInfantry")==3002 && TrainingPool.Orders.Count==0,"Training delivers exactly once");
        foreach(var unit in TrainingPool.Units)
        {
            int beforeUnit=CouncilActions.Int(unit);
            Check(TrainingPool.Place(unit,3,1),"Training order accepted: "+unit);
            Check(TrainingPool.CompleteDueOrders(orderedDate.AddDays(TrainingPool.Duration(unit)-1))=="","No early completion: "+unit);
            TrainingPool.CompleteDueOrders(orderedDate.AddDays(TrainingPool.Duration(unit)));
            Check(CouncilActions.Int(unit)==beforeUnit+3,"Correct completion day: "+unit);
        }
        int goldBeforeQueue=CouncilActions.Int("iMoney");
        Check(!TrainingPool.Place("iInfantry",int.MaxValue,int.MaxValue) && CouncilActions.Int("iMoney")==goldBeforeQueue,"Oversized order cannot overflow price or spend gold");
        TrainingPool.Place("iInfantry",2,1);TrainingPool.Place("iArchers",4,1);
        VariableSingleton.IncrementTurn();
        Check(TrainingPool.Orders.Count==0,"Monthly preparation completes both short training orders");
        Check(VariableSingleton.ArrivalNotice.Contains("Infantry"),"Turn completion reports newly ready units");
        var savedQueue=VariableSingleton.Instance.CaptureState();
        TrainingPool.Place("iShips",1,1);
        Check(savedQueue.training.Count==0,"Save snapshot is independent of later orders");
        SceneManager.LoadScene("Continue Game");
        yield return Frames(10);
        manager = Find<ManagerUI>();
        manager.BringUpSiegeUI();
        yield return Frames();
        var defense = Find<DefenseActions>();
        defense.GetSelection("fOuterWall");
        defense.RepairSelected();
        Check(VariableSingleton.GetIntVariable("iMoney") < 100000, "Repair spends gold");
        Check(VariableSingleton.GetFloatVariable("fOuterWall") > .8f, "Repair restores wall");
        Check(Find<RestUI>().gameObject.activeInHierarchy, "Repair ends management");
        SceneManager.LoadScene("Continue Game");
        yield return Frames(10);

        // Keep the phase-progression fixture alive; loss is tested separately below.
        UnityEngine.Random.InitState(1453);
        // Follow the actual phase components and authored narrative, choosing available options.
        VariableSingleton.SetInt("iPreparationVersion",0); // Exercise compatibility with existing weekly saves. Monthly progression has its own suite.
        for (int turn = 0; turn < 27; turn++)
        {
            foreach(var reputation in new[]{"fHealth","fPeopleRep","fTroopRep","fNobleRep","fPriestRep"})
                VariableSingleton.ChangeFloat(reputation,1f);
            manager = Find<ManagerUI>();
            if (!manager.gameObject.activeInHierarchy) { failures.Add("Management inaccessible at turn " + turn); break; }
            manager.GetComponentsInChildren<Button>().First(b => b.name == "ButtonToContinue").onClick.Invoke();
            yield return Frames();
            Find<RestUI>().GetComponentsInChildren<Button>().First(b => b.name == "ButtonToContinue").onClick.Invoke();
            yield return Frames(6);
            var runner = Find<DialogueRunner>();
            int attempts = 0;
            int choicesSeen = 0;
            while (runner.IsDialogueRunning && attempts++ < 1000)
            {
                var option = FindObjectsByType<OptionButton>(FindObjectsSortMode.None).FirstOrDefault(o => o.IsInteractable());
                if (option != null)
                {
                    choicesSeen++;
                    var safeArrival=FindObjectsByType<OptionButton>(FindObjectsSortMode.None).FirstOrDefault(o=>o.IsInteractable() && o.Option.Line.Text.Text.Contains("Greet our guests"));
                    if(safeArrival!=null) option=safeArrival;
                    if(option.Option.Line.Text.Text.Contains("dungeon"))
                        option=FindObjectsByType<OptionButton>(FindObjectsSortMode.None).First(o=>o.IsInteractable() && o.Option.Line.Text.Text.Contains("Hire him"));
                    option.InvokeOptionSelected();
                }
                else foreach (var line in FindObjectsByType<LineView>(FindObjectsSortMode.None)) line.OnContinueClicked();
                yield return new WaitForSecondsRealtime(0.03f);
            }
            if(turn == 0) Check(choicesSeen > 0, "Opening event offers a real focus choice");
            Check(attempts < 1000, "Dialogue completed at turn " + turn);
            Find<EventUI>().ContinueToAssault();
            yield return Frames();
            if (VariableSingleton.HasSiegeStarted() && Find<AssaultUI>().gameObject.activeInHierarchy)
            {
                var assault = Find<AssaultUI>();
                assault.ShowResult();
                if (AI.Instance.AwaitingBreachDecision)
                {
                    assault.DeployVarangiansDecision = AI.Instance.CanDeployGuards;
                    assault.DecideVarangianDeployment();
                }
                assault.ContinueToEmperorAction();
            }
            yield return Frames();
            Check(VariableSingleton.Turn == turn + 1, "Exactly one turn advanced: " + turn);
            if (failures.Count > 0) break;
        }

        Check(VariableSingleton.HasPreparationPreset, "Reaching siege creates quick-start preset");
        VariableSingleton.QuickStartRequested=true;
        SceneManager.LoadScene("Continue Game");
        yield return Frames(10);
        Check(VariableSingleton.Turn==23 && VariableSingleton.SiegeDay==1, "Quick start restores siege day one");
        Check(VariableSingleton.GetDate().ToString("yyyy-MM-dd")=="1453-04-06", "Siege begins on 6 April 1453");
        yield return EventChoice("story1","will not enforce");
        Check(VariableSingleton.GetBoolVariable("bCharCardinal7") && VariableSingleton.GetIntVariable("iReligion")==2,"Partial Union retains Isidore");
        yield return EventChoice("story1","defend the City together");
        Check(VariableSingleton.GetBoolVariable("bCharCardinal7") && VariableSingleton.GetIntVariable("iReligion")==0,"Wartime compact retains Orthodoxy and Isidore");
        yield return EventChoice("story1","send Cardinal");
        Check(!VariableSingleton.GetBoolVariable("bCharCardinal7"),"Explicit exile removes Isidore");
        yield return EventChoice("story5","every blade");
        Check(VariableSingleton.GetIntVariable("iInfantry")==3500 && VariableSingleton.GetIntVariable("iArchers")==2200,"Genoese acceptance adds 500 infantry and 200 archers");
        var beforePopulation=VariableSingleton.Instance.CaptureState();
        VariableSingleton.ChangeInt("iPopulation",9);
        var populationReport=EventOutcome.Describe(beforePopulation,VariableSingleton.Instance.CaptureState());
        Check(populationReport.Contains("9,000") && populationReport.Contains("109,000"),"Population report shows delta and total in people");

        yield return EventChoice("story20","Leave the garrisons");
        Check(CouncilActions.Int("iEnemyDelay")>=5 && CouncilActions.Int("iEnemyDelay")<=9,"Castle garrison buys five to nine operations of delay");
        yield return EventChoice("story20","Reinforce the castles");
        Check(CouncilActions.Int("iEnemyDelay")==9 && CouncilActions.Int("iInfantry")==2800,"Castle reinforcement commits 200 infantry and buys nine days");
        yield return EventChoice("story20","Grant the castles");
        Check(CouncilActions.Int("iArchers")==2100,"Genoese castle transfer returns 100 archers");
        yield return EventChoice("story21","Refuse publicly");
        Check(VariableSingleton.GetFloatVariable("fSultanRage")>.39f,"Envoy refusal raises fury");
        yield return EventChoice("story21","Receive him courteously");
        Check(CampaignHUD.Instance.HasModal,"Envoy intelligence choice produces an outcome");
        yield return EventChoice("story24","Open the gates");
        Check(CouncilActions.Int("iPopulation")>=101 && CouncilActions.Int("iPopulation")<=103,"Opening gates saves 1000 to 3000 people");
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fMoat")-.7f)<.001f,"Opening gates costs 30 moat points");

        VariableSingleton.ContinueRequested=false;SceneManager.LoadScene("Continue Game");yield return Frames(10);
        string technologyReport;
        Check(TechnologySystem.Unlock(3,out technologyReport),"Roman Training unlocks with three secrets");
        Check(VariableSingleton.GetIntVariable("iSecrets")==0 && Mathf.Abs(TechnologySystem.Quality("Infantry")-.625f)<.001f,"Roman Tactics adds 25 percent to trained lethality");
        Check(!TechnologySystem.Unlock(3,out technologyReport),"Technology cannot be purchased twice");
        Check(!TechnologySystem.Unlock(5,out technologyReport),"Unaffordable technology spends nothing");
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        Check(TechnologySystem.Has("RomanTraining") && Mathf.Abs(TechnologySystem.Quality("Infantry")-.625f)<.001f,"Technology survives save load without stacked bonuses");
        VariableSingleton.ChangeBool("bCharArtillerist5",true);
        Check(CourtRoster.Status(5)=="In service","Court reports actual availability");
        VariableSingleton.ChangeBool("bCharArtillerist5",false);VariableSingleton.SetInt("iCourtState5",4);
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        Check(CourtRoster.Status(5)=="Under guard / imprisoned","Character fate survives save load");
        Check(CourtRoster.Status(2).Contains("commander"),"Court identifies appointed commander");
        var council=CouncilUI.For(Find<ManagerUI>());council.Court();yield return Frames();
        Check(GameObject.Find("Council screen")!=null,"Court screen opens");ControlSingleton.Instance.GoBack();
        council.Technologies();yield return Frames();
        Check(GameObject.Find("Council screen")!=null,"Technology screen opens");ControlSingleton.Instance.GoBack();
        council.Diplomacy();council.Court();ControlSingleton.Instance.GoBack();yield return Frames();
        Check(FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text=="Diplomacy and court"),"Court returns to Diplomacy and court");
        ControlSingleton.Instance.GoBack();
        int reportContinues=0;
        CampaignHUD.Instance.ShowReport("Preserve this outcome","A decision has consequences.",()=>{reportContinues++;CampaignHUD.Instance.DismissReport();});
        var originalReport=GameObject.Find("Event outcome");
        CampaignHUD.Instance.TogglePause();yield return Frames();
        Check(originalReport!=null && !originalReport.activeSelf,"Pause preserves pending outcome");
        CampaignHUD.Instance.TogglePause();yield return Frames();
        Check(originalReport.activeSelf && GameObject.Find("Event outcome")==originalReport,"Resume restores same outcome");
        originalReport.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Continue").onClick.Invoke();
        Check(reportContinues==1 && !CampaignHUD.Instance.HasModal,"Restored outcome continues exactly once");
        council.Spies();yield return Frames();
        Check(FindObjectsByType<Scrollbar>(FindObjectsSortMode.None).Length>0,"Long council screen has visible scroll control");
        ControlSingleton.Instance.GoBack();
        Check(!TechnologySystem.CounterBattery(out technologyReport),"Counterbattery rejects unavailable operation");
        VariableSingleton.Instance.Save();
        int basicAssault=Find<AssaultSystem>().TestAssault(100);
        VariableSingleton.ChangeBool("bTechRomanArchitecture",true);
        Check(Find<AssaultSystem>().TestAssault(100)==basicAssault-5,"Architecture reduces assault value by five");
        VariableSingleton.Instance.Load();UnityEngine.Random.InitState(22);
        int baseLosses=Find<AssaultSystem>().AssaultAftermath(false,10000,true,0);
        foreach(string upgrade in new[]{"GreekFire","Grenades","Handcannons","Grapeshot","UpgradedCrossbows","PlateArmour"})
        {
            VariableSingleton.Instance.Load();VariableSingleton.ChangeBool("bTech"+upgrade,true);UnityEngine.Random.InitState(22);
            Check(Find<AssaultSystem>().AssaultAftermath(false,10000,true,0)>baseLosses,"Combat consumes technology effect: "+upgrade);
        }
        VariableSingleton.Instance.Load();VariableSingleton.SetInt("iSecrets",20);
        Check(TechnologySystem.Unlock(1,out technologyReport),"Counterbattery unlocks");
        VariableSingleton.Instance.overrideTurn=23;VariableSingleton.IncrementTurn();
        int oldAmmo=CouncilActions.Int("iAmmunition");int oldCannons=CouncilActions.Int("iTurkCannons");
        Check(TechnologySystem.CounterBattery(out technologyReport) && CouncilActions.Int("iAmmunition")==oldAmmo-3 && CouncilActions.Int("iTurkCannons")<oldCannons,"Counterbattery spends ammunition and destroys actual cannons");
        VariableSingleton.ChangeBool("bTechRomanTaxation",true);int treasuryBefore=CouncilActions.Int("iMoney");
        VariableSingleton.IncrementTurn();
        Check(CouncilActions.Int("iMoney")==treasuryBefore+CouncilActions.Int("iPopulation")*3,"Taxation adds one daily siege income");
        yield return EventChoice("story7","Sell the collection");
        Check(VariableSingleton.GetBoolVariable("bCharDiplomat4"),"Wardrobe audience introduces Sphrantzes to court");

        yield return EventChoice("story5","Turn them away");
        Check(CourtRoster.Status(0).Contains("Departed"),"Declining Giustiniani records his departure");
        for(int focus=0;focus<5;focus++) {
            int selectedFocus=focus;
            yield return EventChoice("story10","Postpone the levy",()=>{VariableSingleton.SetInt("iMoney",0);VariableSingleton.SetInt("iFocus",selectedFocus);});
            Check(CouncilActions.Int("iMoney")==0,"Poor recruitment fallback spends no gold for focus "+focus);
        }
        yield return EventChoice("prepScorched","Leave the approaches",()=>VariableSingleton.SetInt("iMoney",0));
        yield return EventChoice("prepNaval","Make do with the fleet",()=>VariableSingleton.SetInt("iMoney",0));
        yield return EventChoice("story14","personal direction",()=>{foreach(var key in CourtRoster.Keys)VariableSingleton.ChangeBool(key,false);});
        Check(CouncilActions.Int("iCommander")==-1,"No advisers still permits commander event resolution");
        yield return EventChoice("random21","Burn the spoiled stores",()=>VariableSingleton.SetInt("iProvisions",20));
        Check(CouncilActions.Int("iProvisions")==0,"Spoilage fallback bears loss without negative stock");
        yield return EventChoice("story13","Keep them under close supervision");
        Check(CourtRoster.Status(3).Contains("Under guard"),"Orhan hostage status follows the event");

        VariableSingleton.ContinueRequested=false;SceneManager.LoadScene("Continue Game");yield return Frames(10);
        Check(Enumerable.Range(0,4).Sum(PoliticalSystem.Share)==100 && PoliticalSystem.Share(0)==40,"Political blocs begin with a complete influence distribution");
        Check(PoliticalSystem.Allegiance(2)=="Imperial Loyalists" && PoliticalSystem.Allegiance(0)=="Unaffiliated","Notaras alone begins politically aligned");
        string politicalReport;
        int startingGold=CouncilActions.Int("iMoney");
        Check(PoliticalSystem.Aid(3,out politicalReport) && CouncilActions.Int("iMoney")==startingGold+3000,"Pragmatist favour pays its promised gold");
        Check(PoliticalSystem.Share(3)==25 && Enumerable.Range(0,4).Sum(PoliticalSystem.Share)==100,"Calling in a favour redistributes influence");
        VariableSingleton.SetInt("iBloc1",19);VariableSingleton.SetInt("iBloc0",36);
        Check(!PoliticalSystem.Aid(1,out politicalReport),"Below twenty percent blocks special action");
        VariableSingleton.SetInt("iBloc1",20);VariableSingleton.SetInt("iBloc0",35);
        VariableSingleton.SetInt("iAidState1",1);VariableSingleton.SetInt("iAidDays1",2);
        Check(PoliticalSystem.Aid(2,out politicalReport) && CouncilActions.Int("iAidDays1")==1,"Western favour expedites committed aid with one-day floor");
        VariableSingleton.ChangeBool("bCharWarrior0",true);PoliticalSystem.SetAllegiance(0,2);VariableSingleton.SetInt("iCommander",0);
        Check(Mathf.Abs(CourtRoster.Bonus(0,.05f,.15f)-.15f)<.001f,"Appointed available commander provides stronger bonus");
        VariableSingleton.SetInt("iCommander",2);
        Check(Mathf.Abs(CourtRoster.Bonus(0,.05f,.15f)-.05f)<.001f,"Unappointed officer retains specialist bonus");
        VariableSingleton.SetInt("iCommander",0);
        Check(PoliticalSystem.BanishPreview(1).Contains("Giovanni"),"Banishment preview names affected characters");
        float formerSupport=VariableSingleton.GetFloatVariable("fPeopleRep");
        Check(PoliticalSystem.Banish(1,out politicalReport),"Faction banishment succeeds");
        Check(PoliticalSystem.Share(1)==0 && Enumerable.Range(0,4).Sum(PoliticalSystem.Share)==100,"Banishment preserves total influence");
        Check(!VariableSingleton.GetBoolVariable("bCharWarrior0") && CouncilActions.Int("iCommander")==-1 && CourtRoster.Bonus(0,.05f,.15f)==0,"Banished commander loses office and bonuses");
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fPeopleRep")-(formerSupport-.15f))<.001f,"Banishment applies civic support penalty");
        Check(!PoliticalSystem.Banish(2,out politicalReport),"Banishment cannot be repeated");
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        Check(PoliticalSystem.Banned(1) && CourtRoster.Status(0)=="Banished","Political exile survives checkpoint restoration");
        VariableSingleton.ChangeBool("bCharWarrior0",true);
        Check(!VariableSingleton.GetBoolVariable("bCharWarrior0"),"Later introductions cannot bypass political exile");
        VariableSingleton.SetInt("iMoney",2000);VariableSingleton.SetInt("iTurkSpies",2);
        Check(CouncilActions.SpyMission(3,false,out politicalReport) && CouncilActions.Int("iMoney")==1000 && CouncilActions.Int("iTurkSpies")<=2,"Counterespionage consumes payment and resolves a valid mission");
        council=CouncilUI.For(Find<ManagerUI>());council.Politics();yield return Frames();
        Check(FindObjectsByType<Image>(FindObjectsSortMode.None).Count(im=>im.name=="Influence slice")==4,"Internal politics renders four influence slices");
        ControlSingleton.Instance.GoBack();ControlSingleton.Instance.GoBack();

        // Exercise irreversible outcomes and fresh restart independently of random play.
        VariableSingleton.Instance.EnduringWall.Restore("InnerWall", false);
        VariableSingleton.Instance.EnduringWall.Abandon();
        CampaignHUD.Instance.CheckEnding();
        Check(GameObject.Find("Campaign Modal") != null, "Loss opens ending screen");
        VariableSingleton.ContinueRequested = false;
        SceneManager.LoadScene("Continue Game");
        yield return Frames(10);
        Check(!VariableSingleton.Instance.EnduringWall.CityLost && VariableSingleton.Turn == 0, "Restart resets campaign");
        Check(CouncilActions.Allies.Length==17,"All seventeen diplomatic powers present");
        Check(!SultanNegotiations.Available(1),"Literary conversations locked before siege");
        VariableSingleton.Instance.overrideTurn=VariableSingleton.EffectiveSiegeTurn;VariableSingleton.IncrementTurn();
        string talk;int dayTurn=VariableSingleton.Turn;int gold=CouncilActions.Int("iMoney");
        Check(SultanNegotiations.Act(0,out talk),"Terms remain separate from conversations");
        Check(SultanNegotiations.Act(1,out talk) && talk.Length>100,"History conversation available");
        Check(!SultanNegotiations.Available(1) && !SultanNegotiations.Available(3) && !SultanNegotiations.Available(4),"All three topics share one daily allowance");
        Check(VariableSingleton.Turn==dayTurn && CouncilActions.Int("iMoney")==gold,"Conversation does not use major action or gold");
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        Check(!SultanNegotiations.Available(3),"Shared conversation limit saved");
        VariableSingleton.IncrementTurn();Check(SultanNegotiations.Available(1),"Conversation renews next date");
        var historyLines=new System.Collections.Generic.HashSet<string>();
        for(int attempt=0;attempt<30 && SultanNegotiations.Available(1);attempt++) {
            Check(SultanNegotiations.Act(1,out talk),"Unread history exchange delivered or messenger lost");
            if(!talk.Contains("messenger never reaches"))Check(historyLines.Add(talk),"History exchange does not repeat");
            VariableSingleton.IncrementTurn();
        }
        Check(!SultanNegotiations.Available(1),"Exhausted topic stays closed");
        VariableSingleton.SetInt("iMoney",999);Check(!SultanNegotiations.Act(2,out talk),"Gift affordability enforced");
        VariableSingleton.SetInt("iMoney",2000);Check(SultanNegotiations.Act(2,out talk) && (CouncilActions.Int("iMoney")==1000 || CouncilActions.Int("iMoney")==1300),"Gift charged with possible silver return gift");
        RecurringEvents.PlantGardens();Check(CouncilActions.Int("iHarvestDay")==TrainingPool.Day(VariableSingleton.GetDate())+25 && !RecurringEvents.HarvestDue,"Harvest scheduled after 25 calendar days");
        VariableSingleton.SetInt("iHarvestDay",TrainingPool.Day(VariableSingleton.GetDate()));Check(RecurringEvents.HarvestDue,"Harvest due on calendar date");
        RecurringEvents.Conscript(750);Check(TrainingPool.Orders.Last().quantity==750 && TrainingPool.Remaining(TrainingPool.Orders.Last())==5,"Conscript event uses training pool");
        VariableSingleton.SetInt("iAidState15",1);VariableSingleton.SetInt("iAidDays15",2);
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        int archers=CouncilActions.Int("iArchers");CouncilActions.AdvanceAid(1);
        Check(CouncilActions.Int("iAidDays15")==1 && CouncilActions.Int("iArchers")==archers,"New ally countdown survives save and waits");
        CouncilActions.AdvanceAid(1);int delivered=CouncilActions.Int("iArchers");CouncilActions.AdvanceAid(5);
        Check(delivered==archers+CouncilActions.Allies[15].Archers && delivered==CouncilActions.Int("iArchers"),"New ally delivers exactly once");
        VariableSingleton.SetInt("iAidState8",1);VariableSingleton.SetInt("iAidDays8",10);
        Check(PoliticalSystem.Aid(2,out talk) && CouncilActions.Int("iAidDays8")==7,"Unionist aid includes new Western powers");
        RecurringEvents.RndFraction("fPeopleRep",5,5);Check(VariableSingleton.GetFloatVariable("fPeopleRep")==1,"Recurring morale stays within 100 percent");
        for(int id=0;id<23;id++) yield return EventChoice("random"+id, "");
        yield return EventChoice("random8a", "");
        yield return EventChoice("random6","grab more recruits");
        Check(TrainingPool.Orders.Any(o=>o.quantity==750 && TrainingPool.Remaining(o)==5),"Yarn conscription command creates delayed order");
        yield return EventChoice("random8","wait for the harvest");
        Check(CouncilActions.Int("iHarvestDay")==TrainingPool.Day(VariableSingleton.GetDate())+25,"Yarn garden command schedules calendar harvest");
        yield return EventChoice("random18","Hear the report");
        yield return EventChoice("random8a","carefully ration");
        Check(CouncilActions.Int("iHarvestDay")==0,"Harvest clears its pending date");
        yield return EventChoice("shah1","Give her freedom",()=>SultanGifts.ReturnGift(0));
        Check(CouncilActions.Int("iShahMercy")==1 && SultanGifts.PendingNode==null,"Shaherezade welcomed and followup delayed");
        int identity=CouncilActions.Int("iShahIdentity");VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();
        Check(CouncilActions.Int("iShahIdentity")==identity && CouncilActions.Int("iShahStage")==2,"Identity and chain survive save");
        SultanGifts.ReturnGift(0);Check(CouncilActions.Int("iShahStage")==2 && CouncilActions.Int("iShahIdentity")==identity,"Return gifts cannot duplicate or reroll Shaherezade");
        yield return EventChoice("shah2","trace the letter",()=>{SultanGifts.ReturnGift(0);VariableSingleton.SetInt("iShahStage",2);});
        Check(CouncilActions.Int("iShahEvidence")==1 && CouncilActions.Int("iShahStage")==3,"Investigation advances chain");
        yield return EventChoice("shah3","court storyteller",()=>{VariableSingleton.SetInt("iShahStage",3);VariableSingleton.SetInt("iShahIdentity",0);});
        Check(SultanGifts.Status=="Court storyteller" && SultanGifts.PendingNode==null,"Innocent can join court and chain resolves");
        yield return EventChoice("shah3","Offer protection",()=>{VariableSingleton.SetInt("iShahStage",3);VariableSingleton.SetInt("iShahIdentity",1);VariableSingleton.SetInt("iShahEvidence",1);});
        Check(SultanGifts.Status=="Working with your spies" && VariableSingleton.GetFloatVariable("fHealth")==1,"Investigated assassin can be turned without injury");
        yield return EventChoice("shah3","Keep her alive",()=>{VariableSingleton.SetInt("iShahStage",3);VariableSingleton.SetInt("iShahIdentity",1);VariableSingleton.SetInt("iShahAccess",1);});
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.65f)<.001f,"Unsupervised assassin inflicts 35 health points");
        SultanGifts.Attempt();Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.65f)<.001f,"Attack cannot repeat");
        yield return EventChoice("shah1","Send her back",()=>SultanGifts.ReturnGift(0));
        Check(SultanGifts.Status=="Departed" && SultanGifts.PendingNode==null,"Returning gift closes chain");
        var giftBefore=VariableSingleton.Instance.CaptureState();int food=CouncilActions.Int("iProvisions");SultanGifts.ReturnGift(10);
        Check(CouncilActions.Int("iProvisions")==food+40,"Ordinary return gift awarded");
        VariableSingleton.SetInt("iShahIdentity",1);Check(!EventOutcome.Describe(giftBefore,VariableSingleton.Instance.CaptureState()).Contains("Identity"),"Outcome never exposes hidden identity");
        yield return EventChoice("random0","",()=>SultanGifts.ReturnGift(0));
        int sameTurn=VariableSingleton.Turn;Find<EventUI>().ContinueToAssault();yield return new WaitForSecondsRealtime(.3f);
        Check(VariableSingleton.Turn==sameTurn && FindObjectsByType<OptionButton>(FindObjectsSortMode.None).Any(o=>o.IsInteractable() && o.Option.Line.Text.Text.Contains("Give her freedom")),"Shaherezade follows ordinary event without skipping a turn");
        VariableSingleton.ContinueRequested=false;SceneManager.LoadScene("Continue Game");yield return Frames(10);
        int pantry=CouncilActions.Int("iProvisions");VariableSingleton.IncrementTurn();Check(CouncilActions.Int("iProvisions")==pantry,"Preparations consume no siege rations");
        VariableSingleton.Instance.overrideTurn=23;VariableSingleton.IncrementTurn();pantry=CouncilActions.Int("iProvisions");
        Check(SupplyEconomy.Price("iProvisions")==10,"Wartime provision price is ten gold");
        VariableSingleton.IncrementTurn();Check(CouncilActions.Int("iProvisions")==pantry-SupplyEconomy.DailyNeed,"One completed siege day consumes population-based rations");
        VariableSingleton.SetInt("iProvisions",0);VariableSingleton.IncrementTurn();Check(SupplyEconomy.PendingNode=="foodShortage" && !CampaignHUD.Instance.HasEnded,"Empty stores queue recoverable shortage");
        VariableSingleton.Instance.Save();VariableSingleton.Instance.Load();Check(SupplyEconomy.PendingNode=="foodShortage","Pending shortage survives save");
        int treasury=CouncilActions.Int("iMoney");Check(SupplyEconomy.Buy("iProvisions",100,out var supplyReport) && CouncilActions.Int("iMoney")==treasury-1000,"Market pays exact wartime cost");
        Check(SupplyEconomy.PendingNode==null,"Restocking averts shortage encounter");
        VariableSingleton.SetInt("iMoney",0);Check(!SupplyEconomy.Buy("iProvisions",100,out supplyReport) && CouncilActions.Int("iProvisions")==100,"Unaffordable market purchase changes nothing");
        VariableSingleton.ChangeFloat("fGalataRep",10);Check(VariableSingleton.GetFloatVariable("fGalataRep")==1,"Raw float command cannot exceed 100 percent support");
        VariableSingleton.ChangeFloat("fArchersQuality",.2f);Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fArchersQuality")-1.2f)<.001f,"Equipment quality retains its multiplier scale");
        yield return EventChoice("foodShortage","Requisition",()=>VariableSingleton.SetInt("iFoodShortfall",100));
        Check(CouncilActions.Int("iProvisions")==3200 && SupplyEconomy.PendingNode==null,"Shortage requisition supplies two days and resolves encounter");
        yield return EventChoice("story3","Hire him, whatever");
        Check(CouncilActions.Int("iMoney")==80000,"Orban hiring deducts its advertised twenty thousand gold");
        yield return EventChoice("story3","Negotiate and offer",()=>VariableSingleton.SetInt("iFocus",0));
        Check(CouncilActions.Int("iMoney")==90000,"Focus Orban hiring deducts its advertised ten thousand gold");
        yield return EventChoice("story16","Bribe someone",()=>VariableSingleton.SetInt("iFocus",4));
        Check(CouncilActions.Int("iMoney")==97000,"Fortress sabotage pays three thousand on either result");
        yield return EventChoice("story19","greatest cannon",()=>VariableSingleton.ChangeBool("bCharArtillerist5",true));
        Check(CouncilActions.Int("iMoney")==95000,"Grand Cannon deducts its advertised five thousand gold");
        yield return EventChoice("story17","steel and cannon",()=>VariableSingleton.SetInt("iFocus",0));
        Check(CouncilActions.Int("iMoney")==97000,"Counterbattery workshop pays the cost stated in its outcome");
        // New orders, independent bonuses and sealed offers use isolated test saves.
        VariableSingleton.SetInt("iMoney",20000);
        VariableSingleton.ChangeFloat("fInfantryQuality",.5f-VariableSingleton.GetFloatVariable("fInfantryQuality"));
        VariableSingleton.ChangeBool("bTechRomanTraining",false);
        VariableSingleton.SetInt("iDrillDue0",0);
        var drillDate=VariableSingleton.GetDate();int troopsBefore=CouncilActions.Int("iInfantry");
        Check(TroopTraining.Order(0,out var drillReport) && CouncilActions.Int("iMoney")==15000,"Training charges 5000 at order");
        Check(CouncilActions.Int("iInfantry")==troopsBefore && Mathf.Abs(TroopTraining.Base(0)-.5f)<.001f,"Drilling troops remain available without an early bonus");
        Check(!TroopTraining.Order(0,out drillReport) && CouncilActions.Int("iMoney")==15000,"Duplicate drills rejected without spending");
        VariableSingleton.Instance.Save();VariableSingleton.SetInt("iDrillDue0",0);VariableSingleton.Instance.Load();
        Check(TroopTraining.Due(0)==TrainingPool.Day(drillDate)+5,"Drill completion date survives reload");
        Check(TroopTraining.Complete(drillDate.AddDays(4))=="","Drills do not finish before day five");
        Check(TroopTraining.Complete(drillDate.AddDays(5)).Contains("Infantry") && Mathf.Abs(TroopTraining.Base(0)-.75f)<.001f,"Five days improve training one tier");
        Check(TroopTraining.Complete(drillDate.AddDays(6))=="","Drills complete once");
        VariableSingleton.ChangeFloat("fInfantryQuality",1-TroopTraining.Base(0));VariableSingleton.ChangeBool("bTechRomanTraining",true);
        Check(Mathf.Abs(TechnologySystem.Quality("Infantry")-1.25f)<.001f,"Technology remains useful at Elite");
        Check(!TroopTraining.Order(0,out drillReport),"Elite troops cannot buy another training tier");
        VariableSingleton.ChangeBool("bTechDismountedChampions",false);float halfCavalry=TroopTraining.WallCavalry;
        Check(Mathf.Abs(halfCavalry-TroopTraining.Lethality(2)*.5f)<.001f,"Cavalry fights at half lethality on walls");
        VariableSingleton.ChangeBool("bTechDismountedChampions",true);
        Check(Mathf.Abs(TroopTraining.WallCavalry-halfCavalry*2)<.001f,"Dismounted Champions restores full wall lethality");
        VariableSingleton.SetInt("iFleet",3);VariableSingleton.SetInt("iShips",10);VariableSingleton.SetInt("iBazouks",1000);VariableSingleton.ChangeBool("bTechArtilleryShips",true);RecurringEvents.FleetReport();
        Check(CouncilActions.Int("iBazouks")==920,"Artillery Ships doubles ship-scaled defensive fleet report damage");
        VariableSingleton.SetInt("iGrandCannon",1);int cannonAmmo=CouncilActions.Int("iAmmunition");
        Check(!TechnologySystem.FireGrandCannon(out var grandReport) && CouncilActions.Int("iAmmunition")==cannonAmmo,"Enemy-owned Grand Cannon cannot be fired");
        VariableSingleton.Instance.overrideTurn=24;VariableSingleton.IncrementTurn();
        VariableSingleton.SetInt("iGrandCannon",2);VariableSingleton.SetInt("iAmmunition",10);VariableSingleton.SetInt("iTurkCannons",10);
        Check(TechnologySystem.FireGrandCannon(out grandReport) && CouncilActions.Int("iAmmunition")==7 && CouncilActions.Int("iTurkCannons")>=5 && CouncilActions.Int("iTurkCannons")<=9,"City Grand Cannon spends three ammunition and destroys one to five guns");
        VariableSingleton.ChangePercentFloat("fSultanRage",-1);VariableSingleton.ChangePercentFloat("fPeopleRep",.5f-VariableSingleton.GetFloatVariable("fPeopleRep"));
        SultanNegotiations.Act(6,out var tauntReport);
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fSultanRage")-.05f)<.001f && Mathf.Abs(VariableSingleton.GetFloatVariable("fPeopleRep")-.53f)<.001f,"Taunt adds five fury and three public morale");
        VariableSingleton.ChangePercentFloat("fTroopRep",.5f-VariableSingleton.GetFloatVariable("fTroopRep"));SultanNegotiations.Act(5,out tauntReport);
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fSultanRage")-.10f)<.001f && Mathf.Abs(VariableSingleton.GetFloatVariable("fTroopRep")-.53f)<.001f,"Demand adds five fury and three army morale");
        VariableSingleton.SetInt("iTermsDay",0);VariableSingleton.SetInt("iCampAlert",0);VariableSingleton.ChangePercentFloat("fSultanRage",-1);VariableSingleton.ChangeFloat("fOuterWall",1);VariableSingleton.Instance.EnduringWall.Restore("OuterWall",false);SurrenderTerms.Request();
        Check(SurrenderTerms.Fate(2)==4 && SurrenderTerms.AssassinationRisk==0,"Strong wall and calm camp yield vassalage offer");
        VariableSingleton.Instance.Save();int termsDay=CouncilActions.Int("iTermsDay");VariableSingleton.SetInt("iTermsDay",0);VariableSingleton.Instance.Load();
        Check(SurrenderTerms.Valid && CouncilActions.Int("iTermsDay")==termsDay,"Sealed offer survives reload");
        VariableSingleton.ChangePercentFloat("fSultanRage",1);SurrenderTerms.Request();
        Check(SurrenderTerms.Fate(2)==4,"Same-day offer cannot be rerolled by changed conditions");
        VariableSingleton.SetInt("iTermsDay",0);VariableSingleton.SetInt("iCampAlert",40);VariableSingleton.ChangeFloat("fOuterWall",.4f-VariableSingleton.GetFloatVariable("fOuterWall"));SurrenderTerms.Request();
        Check(SurrenderTerms.Fate(2)==3 && SurrenderTerms.Fate(0)==0 && SurrenderTerms.AssassinationRisk==20,"Offer can sacrifice Emperor and household while sparing inhabitants");
        VariableSingleton.SetInt("iTermsDay",0);VariableSingleton.SetInt("iCampAlert",100);VariableSingleton.ChangeFloat("fOuterWall",-1);SurrenderTerms.Request();
        Check(SurrenderTerms.AssassinationRisk==80 && SurrenderTerms.Fate(0)==3,"Harshest offer accumulates 80 percent assassination risk");
        VariableSingleton.SetInt("iTermsDay",termsDay-1);
        Check(!SurrenderTerms.Accept(out var surrenderReport) && CouncilActions.Int("iSurrenderEnding")==0,"Expired offers cannot end the campaign");
        VariableSingleton.SetInt("iTermsDay",termsDay);
        int seed=0;while(true){UnityEngine.Random.InitState(seed);if(UnityEngine.Random.Range(0,100)<80)break;seed++;}UnityEngine.Random.InitState(seed);
        Check(SurrenderTerms.Accept(out surrenderReport) && CouncilActions.Int("iSurrenderEnding")==2 && surrenderReport.Contains("siege continues without you"),"Assassination rejects surrender and ends the Emperor's reign");
        Check(!SurrenderTerms.Accept(out surrenderReport),"Acceptance cannot roll twice");VariableSingleton.Instance.Load();
        Check(CouncilActions.Int("iSurrenderEnding")==2,"Surrender ending persists through reload");
        VariableSingleton.SetInt("iSurrenderEnding",0);VariableSingleton.SetInt("iTermsDay",0);VariableSingleton.SetInt("iCampAlert",0);VariableSingleton.ChangePercentFloat("fSultanRage",-1);VariableSingleton.ChangeFloat("fOuterWall",1);SurrenderTerms.Request();
        Check(SurrenderTerms.Accept(out surrenderReport) && CouncilActions.Int("iSurrenderEnding")==1,"Accepted safe terms produce negotiated campaign ending");
        Check(DiplomaticChronicles.All.Length==CouncilActions.Allies.Length && DiplomaticChronicles.All.All(p=>p.Paragraphs.Length==3),"Every diplomatic power has three chronicle paragraphs");
        Check(Achievements.Names.Length==18 && Achievements.Descriptions.Length==18,"Eighteen achievements listed");
        Finish(null);
    }
    private IEnumerator EventChoice(string node, string choice, System.Action setup=null)
    {
        VariableSingleton.ContinueRequested=false;
        SceneManager.LoadScene("Continue Game");
        yield return Frames(10);
        VariableSingleton.SetInt("iFocus",3);
        setup?.Invoke();
        var eventUI=Find<EventUI>();
        Find<ManagerUI>().gameObject.SetActive(false);
        eventUI.SetStartNode(node);
        eventUI.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(.3f);
        Check(FindObjectsByType<OptionButton>(FindObjectsSortMode.None).Any(o=>o.IsInteractable()),"Choices appear without advancing text: "+node);
        Check(!eventUI.GetComponentsInChildren<Button>().Any(b=>b.name=="ButtonToContinue"),"No duplicate event Continue: "+node);
        var runner=Find<DialogueRunner>();
        bool selected=false;
        for(int i=0;runner.IsDialogueRunning && i<600;i++)
        {
            var options=FindObjectsByType<OptionButton>(FindObjectsSortMode.None).Where(o=>o.IsInteractable()).ToArray();
            if(options.Length>0)
            {
                var option=options.FirstOrDefault(o=>o.Option.Line.Text.Text.Contains(choice));
                Check(option!=null,"Expected choice present: "+choice);
                if(option==null) break;
                option.InvokeOptionSelected();selected=true;
            }
            // Event text advances into choices or the outcome without input.
            yield return new WaitForSecondsRealtime(.03f);
        }
        Check(selected && !runner.IsDialogueRunning,"Choice completes: "+choice);
        yield return Frames(3);
        Check(CampaignHUD.Instance.HasModal,"Event outcome opens automatically: "+node);
    }
    private void Finish(string extra)
    {
        if (extra != null) failures.Add(extra);
        Application.logMessageReceived -= Log;
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/smoke-results.txt", "PASSED CHECKS\n" + string.Join("\n", checks) +
            "\nFAILURES\n" + string.Join("\n", failures));
        SessionState.SetBool("BesiegedSmoke", false);
        Debug.Log("SMOKE_COMPLETED checks=" + checks.Count + " failures=" + failures.Count);
        EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
    }
}
