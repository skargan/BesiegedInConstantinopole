#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Opt-in visual QA for development builds only; ordinary launches never run this.
public class PlaytestCapture : MonoBehaviour
{
    private string output;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        if (!Environment.GetCommandLineArgs().Contains("-besieged-capture")) return;
        var obj = new GameObject("Playtest Capture");
        DontDestroyOnLoad(obj);
        obj.AddComponent<PlaytestCapture>();
    }
    private IEnumerator Frames(int count){for(int i=0;i<count;i++)yield return null;}
    private void ChooseAssaultIncident(){var ui=PhasedAssaultUI.Current;if(ui.CurrentIncident<0)return;var e=AssaultIncidents.All[ui.CurrentIncident];foreach(var c in e.Choices)if(AssaultIncidents.Unavailable(c,ui.Battle)==null){AssaultClick(c.Name);return;}throw new Exception("No incident choice");}
    private void AssaultClick(string prefix) {
        var button=FindObjectsOfType<Button>().FirstOrDefault(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text.StartsWith(prefix)==true);
        if(button==null)throw new Exception("Missing assault button: "+prefix);
        if(!button.interactable)throw new Exception("Disabled assault button: "+prefix);
        button.onClick.Invoke();
    }
    private IEnumerator Start()
    {
        AudioListener.volume = 0;
        Application.runInBackground = true;
        output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures"));
        Directory.CreateDirectory(output);
        VariableSingleton.TestSavePath = Path.Combine(output, "capture-save.json");
        yield return Capture("01-menu");
        if(Environment.GetCommandLineArgs().Contains("-current-capture")) {
            Achievements.Show(FindObjectOfType<MainMenuUI>().transform.root);yield return Capture("current-achievements");
        }
        FindObjectOfType<MainMenuUI>().NewGame();
        yield return Capture("02-management");
        var manager = FindObjectOfType<ManagerUI>();
        if(Environment.GetCommandLineArgs().Contains("-infiltration-capture")) {
            int initialGold=CouncilActions.Int("iMoney"),initialSpies=CouncilActions.Int("iSpies");
            manager.gameObject.SetActive(false);var eventView=FindObjectOfType<EventUI>(true);eventView.SetStartNode("story23");eventView.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(1);yield return Capture("28-infiltration-choice");var infiltration=FindObjectsOfType<Yarn.Unity.OptionButton>().FirstOrDefault(o=>o.Option.Line.Text.Text.StartsWith("Infiltrate the Ottoman camp"));if(infiltration==null||!infiltration.IsInteractable())throw new Exception("Missing available infiltration choice");infiltration.InvokeOptionSelected();
            yield return new WaitForSecondsRealtime(1);yield return Capture("28-infiltration-outcome");
            int recruits=CouncilActions.Int("iSpies")-initialSpies;
            if(CouncilActions.Int("iMoney")!=initialGold-5000||!(recruits==1||recruits==2||recruits==3||recruits==5)||!CampaignHUD.Instance.HasModal)throw new Exception("Infiltration branch did not complete with recruitment, cost and visible outcome");
            var report=string.Join(" ",FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.text));if(!report.Contains("new agents inside the Ottoman camp"))throw new Exception("Missing infiltration outcome narrative");
            Debug.Log("INFILTRATION_PLAYER_CHECKS completed story23 choice, recruitment, exact cost and visible outcome passed");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-assault27-capture")) {
            PhasedAssaultUI.TestFocus=true;bool previous=PhasedAssaultUI.ActiveMode;
            AssaultClick("Test assault");yield return Capture("27-practice-menu");
            int beforeTurn=VariableSingleton.Turn,beforeFoot=CouncilActions.Int("iInfantry"),beforeAmmo=CouncilActions.Int("iAmmunition"),beforeGold=CouncilActions.Int("iMoney");
            PhasedAssaultUI.ActiveMode=false;AssaultPractice.ForcedIncident=0;AssaultPractice.Launch(0);yield return null;
            var ui=PhasedAssaultUI.Current;var b=ui.Battle;b.Breach=50;AssaultClick("Command from shelter");yield return Capture("27-incident");AssaultClick("Keep the captains");yield return null;
            if(b.Cover!=1||b.TimedMoments!=0)throw new Exception("OFF cover neutrality");AssaultClick("Automatic volleys");yield return null;
            if(b.VolleyMultiplier!=1)throw new Exception("OFF volley neutrality");
            ChooseAssaultIncident();yield return Capture("27-crowd-contact");AssaultClick("Resolve remaining");yield return new WaitForSecondsRealtime(1.3f);
            AssaultClick("Divide evenly");yield return null;AssaultClick("Commit ");yield return Capture("27-aftermath");
            if(!b.Committed||VariableSingleton.Turn!=beforeTurn||CouncilActions.Int("iInfantry")!=beforeFoot||CouncilActions.Int("iAmmunition")!=beforeAmmo||CouncilActions.Int("iMoney")!=beforeGold)throw new Exception("Practice changed campaign");
            AssaultClick("Return to assault practice");yield return null;
            PhasedAssaultUI.ActiveMode=true;AssaultPractice.Launch(0);yield return null;ui=PhasedAssaultUI.Current;AssaultClick("Command from shelter");yield return null;AssaultClick("Keep the captains");yield return null;
            if(FindObjectsOfType<Button>().Any(x=>x.GetComponentInChildren<TMPro.TMP_Text>()?.text.StartsWith("Start cover")==true)){AssaultClick("Start cover");yield return null;var ef=typeof(PhasedAssaultUI).GetField("elapsed",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);ef.SetValue(ui,1.7f);AssaultClick("Take cover");yield return null;}
            AssaultClick("Start volleys");yield return Capture("27-active-controls");
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;var elapsed=typeof(PhasedAssaultUI).GetField("elapsed",flags);elapsed.SetValue(ui,.4f);CampaignHUD.Instance.TogglePause();yield return Frames(8);if((float)elapsed.GetValue(ui)!=.4f)throw new Exception("Pause clock");CampaignHUD.Instance.TogglePause();yield return null;
            for(int i=0;i<3;i++){elapsed.SetValue(ui,i*PhasedAssaultUI.VolleyDuration/3+PhasedAssaultUI.VolleyDuration/6);ui.SendMessage("LoadBow");ui.SendMessage("DrawBow");typeof(PhasedAssaultUI).GetField("drawAge",flags).SetValue(ui,.3f);typeof(PhasedAssaultUI).GetField("aimPoint",flags).SetValue(ui,new Vector2(.515f,.4f));ui.SendMessage("DrawBow");if((float)typeof(PhasedAssaultUI).GetField("drawAge",flags).GetValue(ui)<.3f)throw new Exception("Repeated draw reset ready bow");ui.SendMessage("Fire");int shots=ui.Battle.Shots;ui.SendMessage("Fire");if(ui.Battle.Shots!=shots)throw new Exception("Double volley");}
            if(ui.Battle.VolleyMultiplier!=1.25f)throw new Exception("Perfect volley bound");Destroy(ui.gameObject);yield return null;
            AssaultPractice.Launch(3);yield return null;ui=PhasedAssaultUI.Current;ui.Volley();yield return null;AssaultClick("Take the gun");yield return Capture("27-cannon-aim");
            typeof(PhasedAssaultUI).GetField("aimPoint",flags).SetValue(ui,new Vector2(.515f,.4f));AssaultClick("Fire the cannon");yield return Capture("27-cannon-blast");if(!ui.Battle.GunResolved||ui.Battle.Dead<=0)throw new Exception("Gun minigame");
            yield return new WaitForSecondsRealtime(.8f);Destroy(ui.gameObject);yield return null;
            AssaultPractice.Duel();yield return Capture("27-duel");var d=AssaultDuelUI.Current;AssaultClick("Start duel");yield return new WaitForSecondsRealtime(.25f);
            d.Stroke(new Vector2(.4f,.5f),new Vector2(.6f,.5f));yield return Capture("27-armour-deflection");AssaultClick("Let the guard intervene");yield return null;
            var frameSamples=new System.Collections.Generic.List<float>();AssaultPractice.Launch(0);yield return null;
            ui=PhasedAssaultUI.Current;ui.Volley();yield return null;AssaultClick("Start volleys");
            for(int i=0;i<180;i++){frameSamples.Add(Time.unscaledDeltaTime*1000);yield return null;}frameSamples.Sort();
            Debug.Log("ASSAULT_ACTIVE_FRAME_MS median="+frameSamples[90].ToString("0.00")+" p95="+frameSamples[171].ToString("0.00"));
            PhasedAssaultUI.ActiveMode=previous;Debug.Log("ASSAULT27_PLAYER_CHECKS neutral OFF, practice isolation, controls, pause, bounded volleys, cannon and duel passed");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-assault-capture")) {
            PhasedAssaultUI.TestFocus=true;bool previous=PhasedAssaultUI.ActiveMode;PhasedAssaultUI.ActiveMode=false;
            VariableSingleton.SetInt("iVarangians",500);VariableSingleton.SetInt("iTurkCannons",5);VariableSingleton.ChangeBool("bSiegeTower",true);
            AI.Instance.AIJanissaryAssault();var b=AI.Instance.PendingPhased;b.ArcherBudget*=.1f;b.FootBudget*=.1f;b.Breach=30;
            int beforeTurn=VariableSingleton.Turn,enemyBefore=CouncilActions.Int("iJanissaries");
            FindObjectOfType<AssaultUI>(true).ShowResult();yield return Capture("26-assault-command");
            AssaultClick("Command from shelter");yield return Capture("26-assault-cover");
            AssaultClick("Let captains");yield return Capture("26-assault-volley");
            AssaultClick("Let the captains judge");yield return Capture("26-assault-sortie");
            AssaultClick("Send up to 100");yield return Capture("26-assault-sortie-recall");AssaultClick("Quick strike");
            yield return Capture("26-assault-contact");
            for(int i=0;i<3;i++){AssaultClick("Resolve this wave");yield return null;}
            yield return Capture("26-assault-reserve");AssaultClick("Divide evenly");yield return null;
            AssaultClick("Commit ");yield return Capture("26-assault-aftermath");
            if(!b.Committed||VariableSingleton.Turn!=beforeTurn||enemyBefore-CouncilActions.Int("iJanissaries")!=b.Dead)throw new Exception("Phased settlement mismatch");
            if(DailyLifeEvents.I("iEmperorAtWall")!=0)throw new Exception("Queued wall order not consumed");
            int after=CouncilActions.Int("iJanissaries");AI.Instance.CommitPhased(b);if(CouncilActions.Int("iJanissaries")!=after)throw new Exception("Double settlement");
            AssaultClick("Continue to siege report");yield return Capture("26-assault-report");
            // Active timing: three deliberate centre releases produce only +25%, not compounded output.
            AI.Instance.AIBashiBazoukAssault();var activeBattle=AI.Instance.PendingPhased;PhasedAssaultUI.ActiveMode=true;
            PhasedAssaultUI.Open(activeBattle,manager.transform.parent);yield return null;PhasedAssaultUI.Current.Volley();yield return Capture("26-active-volley");
            var field=typeof(PhasedAssaultUI).GetField("elapsed",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            for(int i=0;i<3;i++){field.SetValue(PhasedAssaultUI.Current,i*10f+5);AssaultClick("Loose arrows");}
            if(Mathf.Abs(activeBattle.VolleyMultiplier-1.25f)>.001f)throw new Exception("Volley multiplier");
            Destroy(PhasedAssaultUI.Current.gameObject);yield return null;
            AI.Instance.AIBashiBazoukAssault();var timed=AI.Instance.PendingPhased;timed.ArcherBudget=timed.FootBudget=timed.CavalryBudget=0;timed.Breach=2000;
            PhasedAssaultUI.Open(timed,manager.transform.parent);yield return null;AssaultClick("Join the parapet");yield return null;
            var ui=PhasedAssaultUI.Current;field.SetValue(ui,4f);CampaignHUD.Instance.TogglePause();yield return Frames(8);
            if(Mathf.Abs((float)field.GetValue(ui)-4)> .001f)throw new Exception("Timer advanced while paused");
            CampaignHUD.Instance.TogglePause();yield return null;field.SetValue(ui,30f);yield return Frames(2);
            if(timed.Cover!=.9f)throw new Exception("Unsafe cover timeout");
            field.SetValue(ui,30f);yield return Frames(2);
            if(timed.Shots!=3 || timed.VolleyMultiplier!=.75f || timed.TimedMoments>2)throw new Exception("Volley timeout or timed moment cap");
            AssaultClick("Keep the gates shut");yield return null;
            for(int i=0;i<3;i++){AssaultClick("Resolve this wave");yield return null;}
            AssaultClick("Divide evenly");yield return Capture("26-emperor-emergency");
            AssaultClick("Accept the escort");yield return null;timed.Guards=0;
            // Insufficient guards must still leave retreat available.
            AssaultClick("Abandon this line");yield return Capture("26-assault-fallback");
            if(!timed.Committed||timed.Held||timed.Health<=0)throw new Exception("Fallback or safe escort");
            PhasedAssaultUI.ActiveMode=previous;Debug.Log("PHASED_ASSAULT_PLAYER_CHECKS assisted path, settlement, active volleys, pause, timeouts, personal emergency and fallback passed");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-gift-capture")) {
            int checks=0;
            for(int i=0;i<SultanGifts.Gifts.Length;i++) {
                var g=SultanGifts.Gifts[i];
                VariableSingleton.SetInt("iMoney",0);string report;
                if(SultanGifts.Send(i,out report))throw new Exception("Unaffordable gift sent");checks++;
                VariableSingleton.SetInt("iMoney",g.Gold);if(g.Quantity>0)VariableSingleton.SetInt(g.Resource,g.Quantity-1);
                if(g.Quantity>0 && SultanGifts.Send(i,out report))throw new Exception("Resource gate bypassed");
                if(g.Quantity>0)VariableSingleton.SetInt(g.Resource,g.Quantity);
                if(SultanGifts.GiftUnavailable(i)!=null)throw new Exception("Affordable gift blocked");checks++;
                VariableSingleton.ChangeFloat("fSultanRage",1);
                if(!SultanGifts.Send(i,out report) || Mathf.Abs(VariableSingleton.GetFloatVariable("fSultanRage")-(1-g.Fury/100f))>.001f)throw new Exception("Gift effect");checks++;
            }
            var c=CouncilUI.For(manager);VariableSingleton.SetInt("iMoney",10000);VariableSingleton.SetInt("iSecrets",10);VariableSingleton.SetInt("iProvisions",1000);VariableSingleton.SetInt("iMaterials",1000);c.SultanGiftMenu();yield return Capture("25-gifts");
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Review gift").onClick.Invoke();yield return Capture("25-gift-review"); if(!FindObjectsOfType<Button>().Any(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Confirm — send gift"))throw new Exception("Gift review missing");checks++;
            Debug.Log("GIFT_PLAYER_CHECKS "+checks+" passed");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-achievement-capture")) {
            int checks=0;
            if(Achievements.Names.Length!=18 || Achievements.Descriptions.Length!=18)throw new Exception("Achievement inventory");checks++;
            VariableSingleton.SetInt("iAchievementOttomanDead",0);VariableSingleton.SetInt("iBazouks",50);
            VariableSingleton.ChangeInt("iBazouks",-100);
            if(CouncilActions.Int("iAchievementOttomanDead")!=50)throw new Exception("Loss cap");checks++;
            VariableSingleton.ChangeInt("iBazouks",100);
            if(CouncilActions.Int("iAchievementOttomanDead")!=50)throw new Exception("Reinforcements counted");checks++;
            VariableSingleton.SetInt("iAchievementWallRepaired",0);VariableSingleton.ChangeFloat("fOuterWall",-.1f);
            if(CouncilActions.Int("iAchievementWallRepaired")!=0)throw new Exception("Damage counts as repair");checks++;
            VariableSingleton.ChangeFloat("fOuterWall",.1f);
            if(CouncilActions.Int("iAchievementWallRepaired")!=1)throw new Exception("Repair untracked");checks++;
            VariableSingleton.Instance.Save();VariableSingleton.SetInt("iAchievementOttomanDead",0);VariableSingleton.Instance.Load();
            if(CouncilActions.Int("iAchievementOttomanDead")!=50 || CouncilActions.Int("iAchievementWallRepaired")!=1)throw new Exception("Achievement save persistence");checks++;
            Achievements.Show(manager.transform.root);yield return Capture("24-achievements-top");
            var achievementPanel=GameObject.Find("Achievements");achievementPanel.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition=0;
            yield return Capture("24-achievements-bottom");
            Debug.Log("ACHIEVEMENT_PLAYER_CHECKS "+checks+" passed");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-background-capture")) {
            var c=CouncilUI.For(manager);int checks=0;
            foreach(string name in new[]{"walls-peace","walls-war","diplomacy","market-peace","market-war","intelligence","army","fleet-peace","fleet-war"}){if(Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+name)==null)throw new Exception("Missing background "+name);checks++;}
            c.Diplomacy();yield return Capture("23-diplomacy-art");c.Spies();yield return Capture("23-intelligence-art");
            c.Market();yield return Capture("23-market-peace");c.Fleet();yield return Capture("23-fleet-peace");
            c.Hide();manager.gameObject.SetActive(true);manager.BringUpSiegeUI();yield return Capture("23-walls-peace");ControlSingleton.Instance.GoBack();manager.BringUpTroopsUI();yield return Capture("23-army-art");ControlSingleton.Instance.GoBack();
            VariableSingleton.Instance.overrideTurn=VariableSingleton.EffectiveSiegeTurn;VariableSingleton.IncrementTurn();VariableSingleton.ArrivalNotice="";
            if(SceneBackdrop.Phase("walls")!="walls-war")throw new Exception("Siege artwork did not switch");
            c.Market();yield return Capture("23-market-war");c.Fleet();yield return Capture("23-fleet-war");c.Hide();manager.gameObject.SetActive(true);manager.BringUpSiegeUI();yield return Capture("23-walls-war");
            Debug.Log("BACKGROUND_PLAYER_CHECKS assets="+checks+" phase=passed");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-portrait-capture")) {
            var c=CouncilUI.For(manager);int checkedPortraits=0;
            foreach(int id in CharacterPortraits.People){if(CharacterPortraits.Texture(id)==null)throw new Exception("Missing portrait "+id);var uv=CharacterPortraits.UV(id);if(uv.xMin<0||uv.yMin<0||uv.xMax>1.001f||uv.yMax>1.001f)throw new Exception("Bad portrait crop "+id);checkedPortraits++;}
            for(int i=0;i<8;i++)VariableSingleton.ChangeBool(CourtRoster.Keys[i],true);
            VariableSingleton.SetInt("iAdventurerChoice",4);VariableSingleton.SetInt("iAdventurerArrived",1);
            c.Court();yield return Capture("23-court");
            FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("23-court-visitor");
            c.Garden();yield return Capture("23-garden");
            var talk=typeof(CouncilUI).GetMethod("GardenTalk",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var question=typeof(CouncilUI).GetMethod("PersonalQuestion",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            foreach(int person in new[]{0,103,200}){talk.Invoke(c,new object[]{person,0});yield return Capture("23-conversation-"+person);}
            question.Invoke(c,new object[]{103});yield return Capture("23-question");
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text==CharacterQuestions.For(103).Replies[0]).onClick.Invoke();yield return Capture("23-reply");
            var back=FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Return to our conversation");
            var scroll=FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy);if(scroll.content.rect.height>scroll.viewport.rect.height+1)throw new Exception("Conversation overflows viewport");back.onClick.Invoke();
            c.Sultan();yield return Capture("23-sultan");
            Debug.Log("PORTRAIT_PLAYER_CHECKS portraits="+checkedPortraits+" return=passed");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-dialogue-review-capture")) {
            var c=CouncilUI.For(manager);int old=CouncilActions.Int("iFleet");VariableSingleton.SetInt("iFleetPrepared",1);
            c.ReviewFleetMission(0);yield return Capture("22-fleet-review");
            if(CouncilActions.Int("iFleet")!=old||CouncilActions.Int("iFleetPrepared")!=1)throw new Exception("Fleet changed during preview");
            ControlSingleton.Instance.GoBack();if(CouncilActions.Int("iFleet")!=old||CouncilActions.Int("iFleetPrepared")!=1)throw new Exception("Return changed orders");
            c.ReviewFleetMission(0);FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Confirm mission").onClick.Invoke();
            yield return Capture("22-fleet-confirmed");if(CouncilActions.Int("iFleet")!=0||CouncilActions.Int("iFleetPrepared")!=0)throw new Exception("Mission was not confirmed");
            FindObjectOfType<RestUI>(true).gameObject.SetActive(false);manager.gameObject.SetActive(true);
            VariableSingleton.SetInt("iAdventurerChoice",4);VariableSingleton.SetInt("iAdventurerArrived",1);
            typeof(CouncilUI).GetMethod("GardenTalk",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(c,new object[]{103,0});yield return Capture("22-fontana");
            typeof(CouncilUI).GetMethod("PersonalQuestion",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(c,new object[]{103});yield return Capture("22-question");
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text==CharacterQuestions.For(103).Replies[0]).onClick.Invoke();yield return Capture("22-reply");
            if(DailyLifeEvents.I("iLifeQuestionReply103")!=1)throw new Exception("Reply not remembered");
            Debug.Log("DIALOGUE_REVIEW_PLAYER_CHECKS passed=4");Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-political-system-capture")) {
            var c=CouncilUI.For(manager);c.Factions();yield return Capture("21-factions");
            c.ReviewPoliticalFavour(0);yield return Capture("21-favour");
            yield return CaptureEvent("politicalDemand","21-demand",true,()=>{VariableSingleton.SetInt("iPolDemand",4);VariableSingleton.SetInt("iPolIssued3",1);VariableSingleton.SetInt("iPolOwed3",1);VariableSingleton.SetInt("iPolDebtDay3_1",PoliticalSystem.Day-30);});
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-naval-capture")) {
            var c=CouncilUI.For(manager);
            c.Fleet();yield return Capture("20-fleet");
            FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("20-fleet-bottom");
            c.FleetOrders();yield return Capture("20-orders");
            AudioSettingsUI.Show(manager.transform.root);yield return Capture("20-settings");AudioSettingsUI.Close();
            c.Garden();yield return Capture("20-garden");c.ReviewFocus();yield return Capture("20-focus");c.IntelligenceBriefing();yield return Capture("20-intelligence");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-politics-capture")) {
            var c=CouncilUI.For(manager);
            c.Factions();yield return Capture("political-factions");
            c.CivicRelations();yield return Capture("civic-relations");
            ControlSingleton.Instance.GoBack();yield return Capture("society-return");
            c.Politics();yield return Capture("political-dealings");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-operations-capture")) {
            var c=CouncilUI.For(manager);
            AudioSettingsUI.Show(manager.transform.root);yield return Capture("18-settings");AudioSettingsUI.Close();
            c.Overview();yield return Capture("18-overview");
            c.Galata();yield return Capture("18-galata");
            c.Sultan();yield return Capture("18-sultan");
            c.ReviewDisruption();yield return Capture("19-disruption");
            c.Fleet();yield return Capture("19-fleet");c.FleetOrders();yield return Capture("19-fleet-orders");c.ReviewNavalBattle();yield return Capture("19-naval-review");
            c.Court();yield return Capture("19-court");c.Chronicle(5);yield return Capture("19-chronicle");c.DiplomaticRelations();yield return Capture("19-realms");
            VariableSingleton.ChangeBool("bTechArtilleryShips",true);c.Technologies();yield return Capture("19-technologies");c.Market();yield return Capture("19-market");
            c.Hide();manager.gameObject.SetActive(true);VariableSingleton.Instance.overrideTurn=VariableSingleton.EffectiveSiegeTurn;VariableSingleton.IncrementTurn();
            var assault=FindObjectOfType<AssaultUI>(true);manager.gameObject.SetActive(false);assault.gameObject.SetActive(true);AI.Instance.AIBalkanSubjectAssault();assault.AssaultAftermathUI();yield return Capture("19-battle-stages");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-theme-capture")) {
            CouncilUI.For(manager).Spies();yield return Capture("theme-spies");
            yield return CaptureEvent("story0","theme-focus",false);
            yield return CaptureEvent("story8","theme-event",false);
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-current-capture")) {
            var council=CouncilUI.For(manager);
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Fleet").onClick.Invoke();yield return Capture("current-fleet");ControlSingleton.Instance.GoBack();
            manager.BringUpTroopsUI();yield return Capture("current-military");
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Upgrades").onClick.Invoke();yield return Capture("current-upgrades");
            council.ReviewTraining(0);yield return Capture("current-training-review");
            TroopTraining.Order(0,out var drillReport);council.Training();yield return Capture("current-ongoing-training");
            council.Diplomacy();yield return Capture("current-diplomacy");
            council.DiplomaticRelations();yield return Capture("current-relations");
            council.Chronicle(13);yield return Capture("current-chronicle");
            council.Hide();manager.BringUpSiegeUI();yield return Capture("current-walls");
            GameObject.Find("Special actions").GetComponent<Button>().onClick.Invoke();yield return Capture("current-artillery");
            VariableSingleton.SetInt("iTermsDay",0);VariableSingleton.SetInt("iCampAlert",40);VariableSingleton.ChangePercentFloat("fSultanRage",1);VariableSingleton.ChangeFloat("fOuterWall",.4f-VariableSingleton.GetFloatVariable("fOuterWall"));SurrenderTerms.Request();
            council.ReviewTerms();yield return Capture("current-terms");
            FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("current-terms-bottom");
            council.ConfirmTerms();yield return Capture("current-surrender-review");
            SurrenderTerms.Accept(out var result);council.Hide();CampaignHUD.Instance.CheckEnding();yield return Capture("current-ending");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-military-capture")) {
            manager.BringUpTroopsUI();yield return Capture("military-clarity");
            GameObject.Find("Quality and roles").GetComponent<Button>().onClick.Invoke();yield return Capture("military-explanation");CampaignHUD.Instance.DismissReport();
            CouncilUI.For(manager).Training();yield return Capture("ongoing-training");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-review-capture")) {
            CouncilUI.For(manager).ReviewFaction(3);yield return Capture("review-clergy");
            ControlSingleton.Instance.GoBack();yield return Capture("review-return");
            CouncilUI.For(manager).ReviewFaction(3);
            GameObject.Find("Confirm society action").GetComponent<Button>().onClick.Invoke();yield return Capture("review-completed");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-spy-capture")) {
            CouncilUI.For(manager).Spies();yield return Capture("spy-network-start");
            FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=.56f;yield return Capture("spy-actions");
            FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("spy-actions-bottom");
            VariableSingleton.SetInt("iSpies",8);VariableSingleton.SetInt("iCampAlert",75);CouncilUI.For(manager).Spies();yield return Capture("spy-network-full");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-typography-capture")) {
            CouncilUI.For(manager).Factions();yield return Capture("type-factions");
            CouncilUI.For(manager).Court();yield return Capture("type-court");
            CouncilUI.For(manager).Technologies();yield return Capture("type-technologies");
            CouncilUI.For(manager).Spies();yield return Capture("type-spies");
            CouncilUI.For(manager).DiplomaticRelations();yield return Capture("type-diplomacy");
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-r11-capture")) {
            CouncilUI.For(manager).Market();yield return Capture("r11-market");
            yield return CaptureEvent("story2","r11-military",false);
            yield return CaptureEvent("random22","r11-soldiers",false);
            yield return CaptureEvent("story15","r11-long-story",false);
            yield return CaptureEvent("shah1","r11-intrigue",true,()=>SultanGifts.ReturnGift(0));
            yield return CaptureEvent("random2","r11-civic",false);
            yield return CaptureEvent("foodShortage","r11-hunger",true,()=>{VariableSingleton.Instance.overrideTurn=24;VariableSingleton.IncrementTurn();VariableSingleton.SetInt("iProvisions",0);VariableSingleton.SetInt("iFoodShortfall",100);});
            Application.Quit();yield break;
        }
        if(Environment.GetCommandLineArgs().Contains("-shah-capture")) {
            SultanGifts.ReturnGift(0);CouncilUI.For(manager).Court();
            FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("shah-court");
            yield return CaptureEvent("shah1","shah-arrival",true,()=>SultanGifts.ReturnGift(0));
            yield return CaptureEvent("shah2","shah-letter",false,()=>VariableSingleton.SetInt("iShahStage",2));
            yield return CaptureEvent("shah3","shah-assassin",true,()=>{VariableSingleton.SetInt("iShahStage",3);VariableSingleton.SetInt("iShahIdentity",1);VariableSingleton.SetInt("iShahEvidence",1);});
            yield return CaptureEvent("shah3","shah-innocent",true,()=>VariableSingleton.SetInt("iShahStage",3));
            Application.Quit();yield break;
        }

        manager.BringUpPeopleUI();yield return Capture("02b-factions");ControlSingleton.Instance.GoBack();
        CouncilUI.For(manager).Politics();yield return Capture("02b-politics");FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("02b-politics-bottom");ControlSingleton.Instance.GoBack();ControlSingleton.Instance.GoBack();
        manager.BringUpSpiesUI();yield return Capture("02c-spies");FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>()?.text=="Secrets").onClick.Invoke();yield return Capture("02c-secrets");ControlSingleton.Instance.GoBack();
        manager.BringUpDiplomacy();yield return Capture("02d-diplomacy");
        CouncilUI.For(manager).DiplomaticRelations();yield return Capture("02d-relations");
        FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=.4f;yield return Capture("02d-relations-east");
        FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("02d-relations-bottom");
        CouncilUI.For(manager).Sultan();yield return Capture("02d-sultan");
        FindObjectsOfType<ScrollRect>().First(v=>v.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("02d-sultan-bottom");
        SultanNegotiations.Act(3,out var verse);CampaignHUD.Instance.ShowReport("An exchange of poetry",verse,()=>CampaignHUD.Instance.DismissReport());yield return Capture("02d-sultan-poetry");CampaignHUD.Instance.DismissReport();
        ControlSingleton.Instance.GoBack();ControlSingleton.Instance.GoBack();
        CouncilUI.For(manager).Court();yield return Capture("02e-court");
        FindObjectsOfType<ScrollRect>().First(s=>s.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("02e-court-bottom");ControlSingleton.Instance.GoBack();
        CouncilUI.For(manager).Technologies();yield return Capture("02f-technologies");
        FindObjectsOfType<ScrollRect>().First(s=>s.gameObject.activeInHierarchy).verticalNormalizedPosition=0;yield return Capture("02f-technologies-bottom");ControlSingleton.Instance.GoBack();
        CouncilUI.For(manager).Palace();yield return Capture("02g-palace");TechnologySystem.Unlock(0,out var adoption);CampaignHUD.Instance.ShowReport("Greek Fire adopted",adoption,()=>CampaignHUD.Instance.DismissReport());yield return Capture("02h-adoption");CampaignHUD.Instance.DismissReport();ControlSingleton.Instance.GoBack();
        manager.BringUpSiegeUI();
        FindObjectOfType<DefenseActions>().GetSelection("fOuterWall");
        yield return Capture("03-walls");
        ControlSingleton.Instance.GoBack();
        manager.BringUpTroopsUI();
        yield return Capture("04-recruitment");
        FindObjectOfType<RecruitationSelection>(true).GetSelectionIndex(0);
        yield return Capture("04b-recruitment-order");
        FindObjectOfType<RecruitationSelection>(true).GetComponentInChildren<ValueIncrementer>(true).OverrideNumber("120");
        FindObjectOfType<RecruitationSelection>(true).CommitRecruitmentTransaction();
        yield return Capture("04c-training-order");CampaignHUD.Instance.DismissReport();
        TrainingPool.Place("iShips",2,1000);TrainingPool.Place("iArchers",50,20);
        FindObjectsOfType<Button>().First(b=>b.name=="Ongoing training").onClick.Invoke();
        yield return Capture("04d-training-pool");
        ControlSingleton.Instance.GoBack();
        ControlSingleton.Instance.GoBack();
        manager.ContinueToRest();
        FindObjectOfType<RestUI>().ContinueToEvent();
        yield return new WaitForSeconds(1);
        for(int i=0;i<10 && !FindObjectsOfType<Yarn.Unity.OptionButton>().Any(o=>o.IsInteractable());i++)
        {
            // Wait only: choices must appear without advancing.
            yield return new WaitForSeconds(.3f);
        }
        yield return Capture("05-event");
        yield return CaptureEvent("story8","06-major-powers",false);
        yield return CaptureEvent("story5","07-genoese",true);
        yield return CaptureEvent("story1","08-schism",false);
        yield return CaptureEvent("story20","09-castles",true);
        yield return CaptureEvent("story21","10-envoy",true);
        yield return CaptureEvent("random13","11-sortie",true);
        yield return CaptureEvent("random8","12-gardens",true);
        Application.Quit();
    }
    private IEnumerator CaptureEvent(string node,string name,bool finish,System.Action setup=null)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Continue Game");
        yield return new WaitForSeconds(.5f);
        VariableSingleton.SetInt("iFocus",3);
        setup?.Invoke();
        FindObjectOfType<ManagerUI>().gameObject.SetActive(false);
        var eventUI=FindObjectOfType<EventUI>(true);
        eventUI.SetStartNode(node);eventUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        for(int i=0;i<20 && !FindObjectsOfType<Yarn.Unity.OptionButton>().Any(o=>o.IsInteractable());i++)
        {
            // Wait only: choices and outcomes advance automatically.
            yield return new WaitForSeconds(.15f);
        }
        yield return Capture(name);
        var scroll=FindObjectsOfType<ScrollRect>().FirstOrDefault(v=>v.name=="Scrollable choices");
        if(scroll!=null){scroll.verticalNormalizedPosition=0;yield return Capture(name+"-bottom");}
        if(!finish) yield break;
        FindObjectsOfType<Yarn.Unity.OptionButton>().First(o=>o.IsInteractable()).InvokeOptionSelected();
        var runner=FindObjectOfType<Yarn.Unity.DialogueRunner>();
        for(int i=0;i<80 && runner.IsDialogueRunning;i++)
        {
            // Wait only: choices and outcomes advance automatically.
            yield return new WaitForSeconds(.15f);
        }
        yield return Capture(name+"-outcome");
    }
    private IEnumerator Capture(string name)
    {
        yield return new WaitForSeconds(1);
        yield return null;
        File.WriteAllLines(Path.Combine(output,name+"-text.txt"),FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.name+" | "+t.rectTransform.rect+" | "+t.maxVisibleCharacters+" | "+string.Join(",",t.GetComponentsInParent<CanvasGroup>().Select(g=>g.name+":"+g.alpha))+" | "+t.text));
        // Render explicitly: a hidden Windows player can otherwise return a black back buffer.
        var cameraObject = new GameObject("Capture camera",typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic=true; camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.06f,.07f,.08f); camera.nearClipPlane=.1f; camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();
        var modes=canvases.Select(c=>c.renderMode).ToArray();
        var cameras=canvases.Select(c=>c.worldCamera).ToArray();
        var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=new RenderTexture(1600,900,24);
        camera.targetTexture=target;
        foreach(var canvas in canvases) {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
        Canvas.ForceUpdateCanvases();
        camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=target;
        var shot=new Texture2D(1600,900,TextureFormat.RGB24,false);
        shot.ReadPixels(new Rect(0,0,1600,900),0,0);shot.Apply();
        File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        RenderTexture.active=previous;
        for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
        camera.targetTexture=null;target.Release();Destroy(target);Destroy(shot);Destroy(cameraObject);
        yield return new WaitForSeconds(.3f);
    }
}
#endif



