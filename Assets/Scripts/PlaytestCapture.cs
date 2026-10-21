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
