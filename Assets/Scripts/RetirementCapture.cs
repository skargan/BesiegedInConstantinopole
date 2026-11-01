#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Explicit opt-in player verification. This runner never writes ordinary campaign archives.
public sealed class RetirementCapture : MonoBehaviour
{
    string output; int passed;
    ManagerUI manager; CouncilUI council; GameObject walls;
    VariableSingleton.CampaignSave baseline;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-retirement-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Retirement"));
        Directory.CreateDirectory(path);SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Retirement verification");DontDestroyOnLoad(go);go.AddComponent<RetirementCapture>().output=path;
    }
    void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);passed++;checks.Add("PASS "+label);}
    string State(){var state=VariableSingleton.Instance.CaptureState();state.randomState=default(UnityEngine.Random.State);return JsonUtility.ToJson(state);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    string Text(GameObject root)=>string.Join("\n",root.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
    GameObject ScreenRoot()=>GameObject.Find("Council screen");
    T Field<T>(object owner,string name)=>(T)owner.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(owner);
    Button FindButton(GameObject root,string text)=>root.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==text));
    Image Icon(Button button)=>button.transform.Find("Turn hourglass")?.GetComponent<Image>();
    Rect Bounds(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);var p=corners.Select(c=>RectTransformUtility.WorldToScreenPoint(null,c)).ToArray();return Rect.MinMaxRect(p.Min(v=>v.x),p.Min(v=>v.y),p.Max(v=>v.x),p.Max(v=>v.y));}
    bool Inside(Rect outer,Rect inner)=>inner.xMin>=outer.xMin-2&&inner.xMax<=outer.xMax+2&&inner.yMin>=outer.yMin-2&&inner.yMax<=outer.yMax+2;
    void Marked(Button button,bool expected,string label)
    {
        var icon=Icon(button);Check((button.GetComponent<TurnActionButton>()!=null)==expected,label+" commitment marker");
        if(expected){
            Check(icon!=null&&icon.sprite!=null&&icon.sprite==TurnActionButton.Emblem&&!icon.raycastTarget&&Inside(Bounds((RectTransform)button.transform),Bounds(icon.rectTransform)),label+" visible contained nonblocking emblem");
            var box=(RectTransform)button.transform;var local=icon.rectTransform;
            float inset=-local.anchoredPosition.x-local.rect.width*.5f;
            Check(inset>=19.99f&&inset<=28.01f,label+" hourglass has a generous frame inset");
            foreach(var caption in button.GetComponentsInChildren<TMP_Text>().Where(t=>t.gameObject.activeSelf&&t.enabled&&t.rectTransform.anchorMax.x>.8f)){
                caption.ForceMeshUpdate();
                var corners=new Vector3[4];caption.rectTransform.GetWorldCorners(corners);
                float captionRight=corners.Max(c=>box.InverseTransformPoint(c).x);
                float iconLeft=box.rect.xMax+local.anchoredPosition.x-local.rect.width*.5f;
                Check(captionRight<=iconLeft-9.9f,label+" caption stays clear of hourglass");
                Check(!caption.isTextOverflowing,label+" caption fits reserved text area: "+caption.text);
            }
        }
        else Check(icon==null,label+" no misleading emblem");
    }
    void NoActionProse(GameObject root,string label)
    {
        string text=Text(root).ToLowerInvariant();
        Check(!new[]{"major action","minor action","end turn","end-turn"}.Any(text.Contains),label+" action prose replaced while costs remain");
    }
    void ScrollBottom(GameObject root)
    {
        Canvas.ForceUpdateCanvases();
        var scroll=root.GetComponentsInChildren<ScrollRect>().FirstOrDefault(s=>s.isActiveAndEnabled);
        if(scroll!=null){scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();}
    }
    void Restore(bool preparation=false)
    {
        CampaignHUD.Instance.DismissReport();if(council!=null)council.Hide();
        foreach(string field in new[]{"siegeUI","troopsUI","restPhaseUI","diplomacyUI","spiesUI","peopleUI"})Field<GameObject>(manager,field).SetActive(false);
        var state=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));state.turn=preparation?1:PreparationCampaign.SiegeTurn;
        VariableSingleton.Instance.RestoreValidated(state);
        VariableSingleton.SetInt("iMoney",100000);VariableSingleton.SetInt("iMaterials",1000);VariableSingleton.SetInt("iShips",30);VariableSingleton.SetInt("iFleet",0);
        if(!manager.gameObject.activeSelf)manager.gameObject.SetActive(true);
        CampaignHUD.Instance.DismissReport();ControlSingleton.Instance.ClearHistory();
    }
    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){
            object next=null;bool moved=false;Exception failure=null;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),checks);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+passed+" checks\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;
        }
        string result="RETIREMENT_CHECKS "+passed+" passed at 1600x900 and 1280x720; retired systems, saved-state behaviour and active navigation verified.";
        File.WriteAllLines(Path.Combine(output,"details.txt"),checks);File.WriteAllText(Path.Combine(output,"checks.txt"),result);Debug.Log(result);Application.Quit(0);
    }
    IEnumerator Hover(Button button,string suffix)
    {
        string before=State(),rng=Rng();var marker=button.GetComponent<TurnActionButton>();
        marker.OnPointerEnter(new PointerEventData(EventSystem.current){position=Bounds((RectTransform)button.transform).center});yield return null;
        var tip=GameObject.Find("Turn meaning");
        Check(tip!=null&&Text(tip).Contains(marker.Hint),suffix+" tooltip explains time");
        Check(Inside(new Rect(0,0,Screen.width,Screen.height),Bounds(tip.GetComponent<RectTransform>())),suffix+" tooltip remains on screen");
        yield return Capture("tooltip-"+suffix);
        marker.OnPointerExit(null);yield return null;Check(GameObject.Find("Turn meaning")==null,suffix+" tooltip closes on exit");
        bool enabled=button.interactable;button.interactable=false;yield return null;yield return null;
        Check(Icon(button).color.a<1&&Icon(button).color.r<.8f,suffix+" unavailable action emblem is dimmed");
        marker.OnPointerEnter(null);Check(GameObject.Find("Turn meaning")==null,suffix+" unavailable action does not show an active hint");
        button.interactable=enabled;yield return null;
        Check(before==State()&&rng==Rng(),suffix+" hover and disabled presentation preserve campaign and RNG");
    }
    void SeedRetired()
    {
        for(int i=0;i<4;i++){
            VariableSingleton.SetInt("iBloc"+i,80);VariableSingleton.SetInt("iBlocBan"+i,1);
            VariableSingleton.SetInt("iPolOwed"+i,3);VariableSingleton.SetInt("iPolIssued"+i,3);
            VariableSingleton.SetInt("iPolBlocked"+i,PoliticalSystem.Day+100);
        }
        VariableSingleton.SetInt("iPolDemand",2);VariableSingleton.SetInt("iAllegiance0",2);
        VariableSingleton.SetInt("iBanishmentUsed",1);VariableSingleton.SetInt("iMerchantFavor",77);
        VariableSingleton.SetInt("iFencing",100);VariableSingleton.SetInt("iEmperorAtWall",1);
    }
    void NoRetired(GameObject root,string label)
    {
        var text=Text(root).ToLowerInvariant();
        foreach(var term in new[]{"merchant favour","fencing","mentoring","faction influence","political dealings","weekly favour","imperial affairs","lead next wall assault"})Check(!text.Contains(term),label+" omits "+term);
    }
    IEnumerator Run()
    {
        Application.runInBackground=true;AudioListener.volume=0;yield return null;
        Check(SaveGames.Automated,"Archives isolated before start");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+25;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized&&FindObjectOfType<ManagerUI>()!=null,"Campaign initialized");
        yield return new WaitForSecondsRealtime(.5f);manager=FindObjectOfType<ManagerUI>();council=CouncilUI.For(manager);baseline=VariableSingleton.Instance.CaptureState();
        Restore();SeedRetired();var old=VariableSingleton.Instance.CaptureState();
        var supports=CouncilActions.FactionKeys.Select(VariableSingleton.GetFloatVariable).ToArray();
        VariableSingleton.Instance.RestoreValidated(old);
        Check(CouncilActions.FactionKeys.Select(VariableSingleton.GetFloatVariable).SequenceEqual(supports),"Old save preserves all five civic measures");
        for(int i=0;i<4;i++)Check(!PoliticalSystem.Passive(i)&&!PoliticalSystem.Banned(i)&&!PoliticalSystem.CanAid(i)&&PoliticalSystem.Owed(i)==0,"Old faction "+i+" inactive");
        Check(PoliticalDemands.PendingNode==null,"Saved faction demand cannot be scheduled");
        Check(EmperorCombat.WallKills==0,"Saved assault leadership has no passive kills");
        SeedRetired();string state=State(),rng=Rng();
        Check(PoliticalSystem.AdvancePolitics()=="","No daily faction drift report");
        PoliticalDemands.Schedule();PoliticalSystem.Gain(0,10);PoliticalSystem.SetAllegiance(0,1);
        Check(!PoliticalSystem.Aid(3,out _)&&!PoliticalSystem.Banish(1,out _),"Legacy faction actions cannot spend or reward");
        Check(state==State()&&rng==Rng(),"Retired faction APIs leave campaign and RNG unchanged");
        Check(SupplyEconomy.FoodAfterPolitics(101)==101,"No retired food discount");
        Check(SupplyEconomy.Cost("iMaterials",100)==1000,"No retired market discount");
        for(int i=0;i<TrainingPool.Units.Length;i++)Check(TrainingPool.Duration(TrainingPool.Units[i])==TrainingPool.Days[i],"Baseline recruitment time "+TrainingPool.Units[i]);
        state=State();DailyLifeEvents.Merchant(-15);Check(state==State(),"Legacy merchant command has no effect");
        Check(!EmperorCombat.Train(out _)&&EmperorCombat.WallKills==0,"Fencing training and preset wall leadership retired");
        Restore();VariableSingleton.ChangeBool("bCharAdministrator2",true);VariableSingleton.SetInt("iCommander",2);
        Check(CourtRoster.CommanderBenefit(2).Contains("−15%")&&WallRepairRules.Labour==425,"Notaras summary matches actual repair labour discount");
        Check(!CourtRoster.AppointCommander(2,out _)&&!CourtRoster.AppointCommander(99,out _),"Duplicate and invalid appointments rejected");
        VariableSingleton.ChangeBool("bCharWarrior0",true);Check(CourtRoster.AppointCommander(0,out _)&&CouncilActions.Int("iCommander")==0,"Serving adviser can become commander");
        Check(WallRepairRules.Labour==475,"Outgoing commander retains specialist repair benefit");
        for(int i=0;i<5;i++){
            Restore();VariableSingleton.ChangeFloat(CouncilActions.FactionKeys[i],.4f-VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[i]));
            int gold=CouncilActions.Int("iMoney");
            Check(CouncilActions.SupportFaction(i,out _),"Civic audience "+i+" available");
            Check(CouncilActions.Int("iMoney")==gold-300&&Mathf.Abs(VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[i])-.43f)<.0001f,"Civic audience "+i+" retains cost and benefit");
        }
        Restore();VariableSingleton.SetInt("iEngineers",2);VariableSingleton.SetInt("iInfantry",3000);DailyLifeEvents.Mentor();
        Check(CouncilActions.Int("iEngineers")==1&&CouncilActions.Int("iInfantry")==2999,"Apprentice event still reserves instructor and pupil");
        Check(DailyLifeEvents.I("iLifeMentorDue")==TrainingPool.Day(VariableSingleton.GetDate())+5,"Apprentice event retains five-day follow-up");
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);string suffix=size.x+"x"+size.y;
            Restore();council.Factions();yield return null;
            var society=ScreenRoot();string text=Text(society);
            Check(text.Contains("Society and politics")&&CouncilActions.FactionNames.All(text.Contains),suffix+" society includes five civic measures");NoRetired(society,suffix+" society");
            Check(society.GetComponentsInChildren<Button>().Count(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("300 gold")))==4,suffix+" four civic audiences plus Galata");
            yield return Capture("society-"+suffix);ScrollBottom(society);yield return Capture("society-galata-"+suffix);
            council.Politics();yield return null;NoRetired(ScreenRoot(),suffix+" legacy politics route");
            council.ReviewPoliticalFavour(2);yield return null;NoRetired(ScreenRoot(),suffix+" legacy favour route");
            Restore();council.Overview();yield return null;NoRetired(ScreenRoot(),suffix+" overview");
            Check(Text(ScreenRoot()).Contains("Daily merchant levy"),suffix+" independent merchant income retained");yield return Capture("overview-"+suffix);
            var commandPanel=ScreenRoot().GetComponentsInChildren<RectTransform>().First(t=>t.name=="Siege commander");
            Check(Text(commandPanel.gameObject).Contains(CourtRoster.CommanderBenefit(CouncilActions.Int("iCommander"))),suffix+" overview commander bonus visible");
            var commandScroll=ScreenRoot().GetComponentsInChildren<ScrollRect>().First(t=>t.isActiveAndEnabled);
            commandScroll.verticalNormalizedPosition=.47f;Canvas.ForceUpdateCanvases();yield return Capture("commander-overview-"+suffix);
            string commandState=State();FindButton(commandPanel.gameObject,"Change commander").onClick.Invoke();yield return null;
            Check(Text(ScreenRoot()).Contains("Choose a serving adviser"),suffix+" commander button opens selection");
            Check(State()==commandState,suffix+" commander selection does not spend the turn");yield return Capture("commander-selection-"+suffix);
            VariableSingleton.ChangeBool("bCharWarrior0",true);VariableSingleton.SetInt("iCommander",2);council.ReviewCommander(0);yield return null;
            Marked(FindButton(ScreenRoot(),"Confirm appointment"),true,suffix+" commander final confirmation");yield return Capture("commander-confirmation-"+suffix);
            council.Overview();yield return null;
            var scroll=ScreenRoot().GetComponentsInChildren<ScrollRect>().First(s=>s.isActiveAndEnabled);scroll.verticalNormalizedPosition=.88f;Canvas.ForceUpdateCanvases();yield return Capture("overview-specialists-"+suffix);
            council.Specialists();yield return null;NoRetired(ScreenRoot(),suffix+" specialists");yield return Capture("specialists-"+suffix);
            Restore();council.Palace();yield return null;
            var hotspots=ScreenRoot().GetComponentsInChildren<LocationHotspot>();
            Check(hotspots.Any(h=>h.name.Contains("To the garden")),suffix+" garden remains accessible");
            var map=hotspots.Single(h=>h.name.Contains("Defence of the walls"));
            yield return Capture("palace-"+suffix);
            var rect=map.rectTransform;var local=new Vector3(rect.rect.xMin+rect.rect.width*.55f,rect.rect.yMin+rect.rect.height*.32f,0);
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(local));
            Check(map.IsRaycastLocationValid(point,null),suffix+" palace map click falls within hotspot");
            map.OnPointerClick(new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left});yield return null;
            walls=Field<GameObject>(manager,"siegeUI");Check(walls.activeInHierarchy,suffix+" real palace map click opens wall defence");
            NoRetired(walls,suffix+" wall defence");yield return Capture("palace-to-walls-"+suffix);
        }
        Restore();council.CivicRelations();yield return null;
        FindButton(ScreenRoot(),"Return").onClick.Invoke();yield return null;
        Check(ScreenRoot()==null||!ScreenRoot().activeInHierarchy,"Society Return exits to council without loop");
        Restore();VariableSingleton.ChangeBool("bCharAdministrator2",true);VariableSingleton.ChangeBool("bCharWarrior0",true);VariableSingleton.SetInt("iCommander",2);
        council.ReviewCommander(0);yield return null;
        var rest=Field<GameObject>(manager,"restPhaseUI");var probe=rest.AddComponent<TurnActionRestProbe>();int before=probe.Enables;
        var confirm=FindButton(ScreenRoot(),"Confirm appointment");confirm.onClick.Invoke();confirm.onClick.Invoke();
        Check(CouncilActions.Int("iCommander")==0,"Actual confirmation appoints selected commander");
        Check(probe.Enables==before+1,"Double activation commits only one council action");
        Check(WallRepairRules.Labour==475,"Actual appointment updates outgoing specialist benefit");

    }
    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.2f);Canvas.ForceUpdateCanvases();
        File.WriteAllLines(Path.Combine(output,name+"-text.txt"),FindObjectsOfType<TMP_Text>().Select(t=>t.name+" | "+t.rectTransform.rect+" | "+t.text));
        var obj=new GameObject("Turn action capture camera",typeof(Camera));var camera=obj.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{
            camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        }finally{
            RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
            camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(obj);Canvas.ForceUpdateCanvases();
        }
        yield return null;
    }
}
#endif
