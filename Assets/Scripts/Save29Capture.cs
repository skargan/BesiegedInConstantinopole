#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Save29Capture:MonoBehaviour {
    string root;int checks;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-save29-capture")||Environment.GetCommandLineArgs().Contains("-ai32-capture")||Environment.GetCommandLineArgs().Contains("-theatre33-capture")){var go=new GameObject("Save29 verification");DontDestroyOnLoad(go);go.AddComponent<Save29Capture>();}}
    void Check(bool v,string label){if(!v)throw new Exception("SAVE29 PLAYER: "+label);checks++;}
    private IEnumerator Shot(string name)
    {
        yield return new WaitForSeconds(1);
        yield return null;
        File.WriteAllLines(Path.Combine(root,name+"-text.txt"),FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.name+" | "+t.rectTransform.rect+" | "+t.maxVisibleCharacters+" | "+string.Join(",",t.GetComponentsInParent<CanvasGroup>().Select(g=>g.name+":"+g.alpha))+" | "+t.text));
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
        File.WriteAllBytes(Path.Combine(root,name+".png"),shot.EncodeToPNG());
        RenderTexture.active=previous;
        for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
        camera.targetTexture=null;target.Release();Destroy(target);Destroy(shot);Destroy(cameraObject);
        yield return new WaitForSeconds(.3f);
    }
    IEnumerator Loaded(){yield return null;yield return new WaitForSecondsRealtime(.35f);Check(VariableSingleton.Instance!=null,"campaign scene loaded");var profile=typeof(VariableSingleton).Assembly.GetType("EmperorProfile");if(profile!=null&&(bool)profile.GetField("NeedsOpening").GetValue(null)){profile.GetMethod("Allocate").Invoke(null,new object[]{new int[]{6,5,5}});var opening=GameObject.Find("What kind of Emperor am I?");if(opening!=null)Destroy(opening);VariableSingleton.Instance.Save();}}
    void Click(string text){var b=FindObjectsOfType<Button>().FirstOrDefault(x=>x.GetComponentInChildren<TMPro.TMP_Text>()?.text.StartsWith(text)==true);if(b==null||!b.interactable)throw new Exception("Missing button "+text);b.onClick.Invoke();}
    void Reload(string path){SaveGames.RequestedFile=path;VariableSingleton.ContinueRequested=true;VariableSingleton.QuickStartRequested=false;SceneManager.LoadScene("Continue Game");}
    IEnumerator Start(){
        Application.runInBackground=true;root=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Save29"));Directory.CreateDirectory(root);SaveGames.TestRoot=Path.Combine(root,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        if(Environment.GetCommandLineArgs().Contains("-theatre33-capture")){yield return Theatre33Run();yield break;}
        if(Environment.GetCommandLineArgs().Contains("-ai32-capture")){yield return AI32Run();yield break;}
        SaveMenu.NewCampaign(FindObjectOfType<MainMenuUI>().transform.root);yield return Shot("01-mode-selection");SaveMenu.Close();yield return Shot("00-main-menu");
        bool hadDisplay=PlayerPrefs.HasKey("Besieged.Fullscreen");int previousDisplay=PlayerPrefs.GetInt("Besieged.Fullscreen",1);AudioSettingsUI.Show(FindObjectOfType<MainMenuUI>().transform.root);yield return Shot("09-settings-display");Click("Windowed");yield return new WaitForSecondsRealtime(1);Check(!Screen.fullScreen,"windowed display applied");Click("Fullscreen");yield return new WaitForSecondsRealtime(1);Check(Screen.fullScreen,"fullscreen display applied");Click("Windowed");yield return new WaitForSecondsRealtime(.5f);AudioSettingsUI.Close();if(hadDisplay)PlayerPrefs.SetInt("Besieged.Fullscreen",previousDisplay);else PlayerPrefs.DeleteKey("Besieged.Fullscreen");PlayerPrefs.Save();
        SaveGames.PrepareNew(false);VariableSingleton.ContinueRequested=false;SceneManager.LoadScene("Continue Game");yield return Loaded();
        Check(!SaveGames.Ironman&&SaveGames.CanManualSave,"Normal allows council saves");Check(SaveGames.Named("Test council",out var message),"named save written");
        var file=SaveGames.Entries().First(e=>e.archive?.name=="Test council").path;int money=CouncilActions.Int("iMoney");VariableSingleton.ChangeInt("iMoney",-123);Reload(file);yield return Loaded();Check(CouncilActions.Int("iMoney")==money&&!SaveGames.Ironman,"Normal named save reloads without mode promotion");
        SaveMenu.Show(FindObjectOfType<ManagerUI>().transform.parent);yield return Shot("02-archive-browser");SaveMenu.Close();
        var manager=FindObjectOfType<ManagerUI>();CouncilUI.For(manager).Technologies();yield return Shot("03-technology-art");var thumbnail=FindObjectsOfType<Button>().First(b=>b.name.StartsWith("Technology artwork "));thumbnail.onClick.Invoke();yield return Shot("04-technology-enlarged");Check(TechnologyArtwork.ClosePreview()&&!TechnologyArtwork.ClosePreview(),"image preview closes once");ControlSingleton.Instance.GoBack();yield return null;
        if(Environment.GetCommandLineArgs().Contains("-polish31-capture")){Debug.Log("POLISH31_PLAYER_CHECKS display switching, archives, technology thumbnails and enlargement passed");Application.Quit();yield break;}
        WorldMapUI.Show();yield return Shot("07-world-map");Check(GameObject.Find("World map viewer")!=null,"world map opens");Click("+");yield return Shot("08-world-map-zoom");Click("Fit");WorldMapUI.Close();
        var legacy=VariableSingleton.Instance.CaptureState();legacy.version=1;legacy.ints.First(v=>v.key=="iPopulation").value=100;legacy.ints.Add(new VariableSingleton.IntValue{key="iPopulationRemainder",value=499});VariableSingleton.Instance.RestoreValidated(legacy);Check(PopulationAccounting.People==100499,"legacy save migrates in live state");var modern=VariableSingleton.Instance.CaptureState();VariableSingleton.Instance.RestoreValidated(modern);Check(PopulationAccounting.People==100499&&modern.version==2,"new save reload does not multiply population");
        VariableSingleton.SetInt("iPopulation",100499);Check(PopulationAccounting.Food(false)==100,"100499 needs 100 provisions");PopulationAccounting.Add(1);Check(PopulationAccounting.Food(false)==101,"100500 needs 101 provisions");
        var saved=VariableSingleton.Instance.CaptureState();int storyCount=saved.story.Count;if(storyCount>0)VariableSingleton.Instance.RemovePossibleEvent("story",saved.story[0]);Check(saved.story.Count==storyCount,"snapshot owns story list");
        var invalid=SaveGames.Read(file);invalid.campaign.ints.First(v=>v.key=="iMoney").value=999999;string bad=Path.Combine(SaveGames.Root,"damaged.besieged.json");File.WriteAllText(bad,JsonUtility.ToJson(invalid));Check(!SaveGames.Load(bad,out var error)&&CouncilActions.Int("iMoney")==money,"invalid save rejected before live mutation");
        SaveGames.PrepareNew(true);VariableSingleton.ContinueRequested=false;SceneManager.LoadScene("Continue Game");yield return Loaded();Check(SaveGames.Ironman&&!SaveGames.CanManualSave&&!SaveGames.Named("blocked",out message),"Ironman forbids manual slots");
        Check(!SaveGames.AchievementEligible,"automated gameplay never awards real achievements");
        // A paid completed event must resume its report without rerunning the choice.
        VariableSingleton.ChangeInt("iMoney",-5000);int after=CouncilActions.Int("iMoney"),spies=CouncilActions.Int("iSpies");VariableSingleton.ChangeInt("iSpies",2);SaveGames.Checkpoint("event","story23","Recorded decision","A test decision has completed. Gold and agents must not change on reload.");
        string rolling=VariableSingleton.TestSavePath;Reload(rolling);yield return Loaded();Check(SaveGames.Ironman&&CampaignHUD.Instance.HasModal&&CouncilActions.Int("iMoney")==after&&CouncilActions.Int("iSpies")==spies+2,"Ironman resumes paid event outcome without duplicate reward");yield return Shot("05-ironman-resume");
        // A committed assault resumes its result; Continue advances exactly one turn.
        SaveGames.Checkpoint("advance",null,"Recorded assault","The assault has already resolved.");int turn=VariableSingleton.Turn;Reload(rolling);yield return Loaded();Check(VariableSingleton.Turn==turn&&CampaignHUD.Instance.HasModal,"completed battle resumes before calendar advance");Click("Continue");yield return null;Check(VariableSingleton.Turn==turn+1,"battle continuation advances once");
        Check(SaveGames.Checkpoint(),"rolling checkpoint writes");var a=SaveGames.Read(rolling);Check(a.ironman&&a.saveFormat==SaveGames.Format&&a.build==CouncilUI.BuildLabel,"mode and versions persist");Check(File.Exists(rolling+".bak"),"automatic recovery backup exists");
        SaveMenu.Show(FindObjectOfType<ManagerUI>().transform.parent);yield return Shot("06-ironman-archives");SaveMenu.Close();
        Debug.Log("SAVE29_PLAYER_CHECKS "+checks+" passed: portable slots, mode gate, corruption rejection, event resume, assault resume, technology art");Application.Quit();
    }
    IEnumerator Theatre33Run(){
        root=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Theatre33"));Directory.CreateDirectory(root);SaveGames.TestRoot=Path.Combine(root,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        SaveGames.PrepareNew(false);VariableSingleton.ContinueRequested=false;VariableSingleton.QuickStartRequested=true;SceneManager.LoadScene("Continue Game");yield return Loaded();yield return new WaitForSecondsRealtime(1);
        WarTheatre.Validate(WarTheatre.Data);Check(WarTheatre.Data.regions.Count(r=>r.kind=="province")==11,"eleven provinces");Check(Enum.GetValues(typeof(OttomanDirector.Temperament)).Length==3,"three personalities");
        OttomanDirector.Ensure();VariableSingleton.SetInt("iOttPersonality",0);OttomanDirector.Ensure();Check(OttomanDirector.Personality==OttomanDirector.Temperament.Aggressive&&DailyLifeEvents.I("iOttPersonality")==1,"legacy Rash becomes Aggressive");
        foreach(int old in new[]{1,2,3}){VariableSingleton.SetInt("iOttPersonality",old);OttomanDirector.Ensure();Check((int)OttomanDirector.Personality==old,"existing personality stable "+old);}
        var original=VariableSingleton.Instance.CaptureState();
        VariableSingleton.SetInt("iSpies",5);VariableSingleton.SetInt("iMoney",10000);VariableSingleton.SetInt("iWbwState",0);VariableSingleton.SetInt("iWbwDue",0);VariableSingleton.SetInt("iWbwProvince",0);
        Check(ProvincialIntelligence.ProvinceId=="macedonia","legacy cell retains Macedonia");
        VariableSingleton.SetInt("iWbwProvince",6);VariableSingleton.SetInt("iWbwLedger_saruhan",1);
        Check(ProvincialIntelligence.ProvinceId=="aydin"&&ProvincialIntelligence.Ledger==1,"Lydia assignment and ledger migrate to Thrakesion");
        VariableSingleton.SetInt("iWbwProvince",10);Check(ProvincialIntelligence.ProvinceId=="anatolia","old Anatolia slot stays stable");
        VariableSingleton.SetInt("iWbwProvince",11);Check(ProvincialIntelligence.ProvinceId=="armeniakon","new Armeniakon uses appended save slot");
        VariableSingleton.SetInt("iWbwProvince",12);Check(ProvincialIntelligence.ProvinceId=="candar","Paphlagonia appended save slot");
        VariableSingleton.SetInt("iWbwProvince",0);
        Check(WarTheatre.Find("karasi").points.All(v=>(1-v.y)*683>=353),"Opsikion stays on Asian shore");
        Check(WarTheatre.Find("candar").kind=="province"&&WarTheatre.Control(WarTheatre.Find("candar"))==100,"Paphlagonia starts under full Ottoman authority");

        Check(!ProvincialIntelligence.Dispatch("nonexistent",out var bad)&&CouncilActions.Int("iMoney")==10000,"invalid province no debit");
        // The quick-start scene may still be in preparation. Set its siege date for the operation probe.
        typeof(VariableSingleton).GetField("turn",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,18);
        Check(ProvincialIntelligence.Dispatch("bithynia",out var dispatch),"Bithynia accepts dispatch");Check(ProvincialIntelligence.ProvinceId=="bithynia"&&CouncilActions.Int("iMoney")==9500&&CouncilActions.Int("iSpies")==4,"province and costs recorded once");
        Check(!ProvincialIntelligence.Dispatch("thrace",out var blocked)&&CouncilActions.Int("iMoney")==9500,"second assignment cannot duplicate");
        var saved=VariableSingleton.Instance.CaptureState();VariableSingleton.Instance.RestoreValidated(saved);Check(ProvincialIntelligence.ProvinceId=="bithynia","destination persists on restore");
        ProvincialIntelligence.Advance(VariableSingleton.GetDate().AddDays(7));Check(ProvincialIntelligence.State==ProvincialIntelligence.CellState.Resident,"agent arrives in selected province");
        Check(ProvincialIntelligence.Begin(0,out var op),"shared ledger operation available");
        VariableSingleton.SetInt("iWbwLedger",1);Check(ProvincialIntelligence.Ledger==0,"old Thessaloniki ledger does not leak into Bithynia");VariableSingleton.SetInt("iWbwLedger_bithynia",1);Check(ProvincialIntelligence.Ledger==1,"province ledger read correctly");
        VariableSingleton.Instance.RestoreValidated(original);
        WarTheatreUI.Show();yield return Shot("01-theatre");
        var state=UnityEngine.Random.state;WarTheatreUI.Close();yield return null;WarTheatreUI.Show();Check(JsonUtility.ToJson(state)==JsonUtility.ToJson(UnityEngine.Random.state),"view does not consume campaign randomness");yield return null;Canvas.ForceUpdateCanvases();
        PointerClick("Symbols");yield return Shot("02-symbols");PointerClick("Historical atlas");yield return Shot("03-historical-reference");Check(WorldMapUI.Close(),"historical reference closes independently");yield return null;
        PointerClick("+");yield return Shot("04-map-zoom");PointerClick("Fit");
        var shape=FindObjectsOfType<TerritoryGraphic>().First(x=>x.name=="bithynia");var map=shape.rectTransform;var point=WarTheatre.Find("bithynia").label;Vector2 screen=RectTransformUtility.WorldToScreenPoint(null,map.TransformPoint(new Vector3(map.rect.xMin+point.x*map.rect.width,map.rect.yMin+point.y*map.rect.height)));
        Check(shape.Raycast(screen,null),"polygon raycast selects interior");
        shape.OnPointerEnter(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));shape.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));yield return Shot("05-province");
        PointerClick("Provincial operations");yield return Shot("06-operations");Check(FindObjectsOfType<TMPro.TMP_Text>().Any(x=>x.text.Contains("Bithynia")),"selected province panel");
        WarTheatreUI.Show();yield return null;var capital=FindObjectsOfType<TerritoryGraphic>().First(x=>x.name=="Constantinople");capital.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));yield return Shot("07-palace");Check(GameObject.Find("War Beyond the Walls map")==null,"Constantinople returns to Palace");
        Debug.Log("THEATRE33_CHECKS "+checks+" passed");Application.Quit();
    }
    void PointerClick(string prefix){
        var b=FindObjectsOfType<Button>().First(x=>x.interactable&&x.GetComponentInChildren<TMPro.TMP_Text>()?.text.StartsWith(prefix)==true);
        var r=b.GetComponent<RectTransform>();Vector2 point=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
        var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
        var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(e,hits);Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b,"unobstructed pointer target "+prefix);
        UnityEngine.EventSystems.ExecuteEvents.Execute(b.gameObject,e,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    }
    IEnumerator AI32Run(){
        root=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/AI32"));Directory.CreateDirectory(root);SaveGames.TestRoot=Path.Combine(root,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"check.besieged.json");
        SaveGames.PrepareNew(false);VariableSingleton.ContinueRequested=false;SceneManager.LoadScene("Continue Game");yield return Loaded();
        int consolidation=ConsolidationChecks.Run();SceneManager.LoadScene("Continue Game");yield return Loaded();
        int provincial=ProvincialChecks.Run();int core=AI32Checks.Run();int exercises=AI32Exercise.Run();Debug.Log("AI32_CHECKS core="+core+" consolidation="+consolidation+" provincial="+provincial);
        SaveGames.PrepareNew(false);SceneManager.LoadScene("Continue Game");yield return Loaded();
        var setup=VariableSingleton.Instance.CaptureState();setup.turn=16;VariableSingleton.Instance.RestoreValidated(setup);OttomanDirector.PrepareTurn();VariableSingleton.SetInt("iSpies",10);
        var manager=FindObjectOfType<ManagerUI>();CouncilUI.For(manager).Spies();yield return Shot("01-intelligence");
        // Inspect the intelligence report grid without the operation list hiding its later rows.
        var viewport=GameObject.Find("Intelligence reports");if(viewport!=null)Debug.Log("AI32_INTEL_READY "+OttomanDirector.PlanReport);
        var intelScroll=GameObject.Find("Council scroll").GetComponent<ScrollRect>();intelScroll.content.anchoredPosition=new Vector2(0,740);yield return Shot("01b-intelligence-plans");ControlSingleton.Instance.GoBack();yield return null;
        bool hadDisplay=PlayerPrefs.HasKey("Besieged.Fullscreen");int display=PlayerPrefs.GetInt("Besieged.Fullscreen",1);
        CampaignHUD.Instance.TogglePause();AudioSettingsUI.Show(manager.transform.root);yield return Shot("02-settings");
        for(int i=0;i<3;i++){PointerClick("Windowed");yield return new WaitForSecondsRealtime(.5f);Check(!Screen.fullScreen,"windowed switch "+i);PointerClick("Fullscreen");yield return new WaitForSecondsRealtime(.5f);Check(Screen.fullScreen,"fullscreen return "+i);}
        Settings.Instance.SetFullscreen(false);yield return new WaitForSecondsRealtime(.5f);AudioSettingsUI.Close();CampaignHUD.Instance.TogglePause();
        if(hadDisplay)PlayerPrefs.SetInt("Besieged.Fullscreen",display);else PlayerPrefs.DeleteKey("Besieged.Fullscreen");PlayerPrefs.Save();
        manager.BringUpSiegeUI();FindObjectOfType<DefenseActions>(true).GetSelection("fOuterWall");yield return Shot("03-repair-materials");ControlSingleton.Instance.GoBack();yield return null;
        CouncilUI.For(manager).ProvincialOperations();yield return Shot("04-provincial");ControlSingleton.Instance.GoBack();yield return null;
        PhasedAssaultUI.TestFocus=true;bool oldActive=PhasedAssaultUI.ActiveMode;PhasedAssaultUI.ActiveMode=true;AssaultPractice.Show(manager);AssaultPractice.Launch(3);yield return null;var ui=PhasedAssaultUI.Current;ui.Battle.ArrowScreens=true;ui.Volley();yield return Shot("05-cannon-priority");Click("Shatter the arrow");Check(!ui.Battle.ArrowScreens&&ui.Battle.GunResolved,"screen destruction via actual choice");yield return null;
        PhasedAssaultUI.ActiveMode=oldActive;Debug.Log("AI32_PLAYER_CHECKS "+checks+" passed; pointer display cycling, intelligence, repairs, provincial navigation, cannon priorities");Application.Quit();
    }

}
#endif
