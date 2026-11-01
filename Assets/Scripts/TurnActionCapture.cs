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
public sealed class TurnActionCapture : MonoBehaviour
{
    string output; int passed;
    ManagerUI manager; CouncilUI council; GameObject walls;
    VariableSingleton.CampaignSave baseline;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-turn-action-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/TurnActions"));
        Directory.CreateDirectory(path);SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Turn action verification");DontDestroyOnLoad(go);go.AddComponent<TurnActionCapture>().output=path;
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
        string result="TURN_ACTION_CHECKS "+passed+" passed at 1600x900 and 1280x720; one isolated repair committed.";
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
    IEnumerator Run()
    {
        Application.runInBackground=true;AudioListener.volume=0;yield return null;
        Check(SaveGames.Automated,"Archives isolated before start");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+25;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized&&FindObjectOfType<ManagerUI>()!=null,"Campaign initialized");
        yield return new WaitForSecondsRealtime(.5f);manager=FindObjectOfType<ManagerUI>();council=CouncilUI.For(manager);baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);string suffix=size.x+"x"+size.y;
            Check(Screen.width==size.x&&Screen.height==size.y,suffix+" display size applied");
            Restore();var restButton=manager.GetComponentsInChildren<Button>().First(b=>b.name=="ButtonToContinue");
            Canvas.ForceUpdateCanvases();Marked(restButton,true,suffix+" council rest");
            yield return Capture("council-rest-"+suffix);yield return Hover(restButton,"rest-"+suffix);
            Restore();manager.BringUpSiegeUI();yield return null;
            walls=Field<GameObject>(manager,"siegeUI");var actions=walls.GetComponentInChildren<DefenseActions>(true);
            actions.GetSelection("fOuterWall");Canvas.ForceUpdateCanvases();
            var repair=Field<Button>(actions,"repairButton");var abandon=Field<Button>(actions,"abandonButton");
            Marked(repair,true,suffix+" repair");Marked(abandon,true,suffix+" abandon");
            var labels=repair.GetComponentsInChildren<TMP_Text>();var offsets=labels.Select(t=>t.rectTransform.offsetMax).ToArray();Sprite emblem=Icon(repair).sprite;
            for(int i=0;i<5;i++){actions.GetSelection(i%2==0?"fMiddleWall":"fOuterWall");Canvas.ForceUpdateCanvases();}
            actions.GetSelection("fOuterWall");Canvas.ForceUpdateCanvases();
            Check(Icon(repair).sprite==emblem&&repair.GetComponentsInChildren<TurnActionButton>(true).Length==1&&repair.GetComponentsInChildren<Image>().Count(i=>i.name=="Turn hourglass")==1,suffix+" repeated wall selection preserves one emblem");
            Check(labels.Select((t,i)=>(t.rectTransform.offsetMax-offsets[i]).sqrMagnitude<.01f).All(v=>v),suffix+" repeated wall selection preserves caption padding");
            string cost=Field<TMP_Text>(actions,"repairCostDisplay").text;
            Check(cost.Contains(WallRepairRules.Labour.ToString("N0")+" gold")&&cost.Contains(WallRepairRules.Materials("fOuterWall")+" materials"),suffix+" repair labour and material costs visible");
            NoActionProse(walls,suffix+" wall actions");yield return Capture("walls-"+suffix);yield return Hover(repair,"walls-"+suffix);

            Restore();council.Market();yield return null;
            var market=ScreenRoot();var buys=market.GetComponentsInChildren<Button>().Where(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Buy\n"))).ToArray();
            Check(buys.Length==3,suffix+" three resource purchases");foreach(var b in buys)Marked(b,false,suffix+" "+b.GetComponentInChildren<TMP_Text>().text.Replace("\n"," "));
            Marked(FindButton(market,"Collect emergency tax"),true,suffix+" emergency tax");NoActionProse(market,suffix+" market");
            yield return Capture("market-"+suffix);
            ScrollBottom(market);yield return Capture("market-tax-"+suffix);

            Restore(true);PreparationMenu.Show(manager.transform,_=>throw new InvalidOperationException("Presentation check must not commit preparation"));yield return null;
            var prep=GameObject.Find("Monthly preparation");var choose=prep.GetComponentsInChildren<Button>().Where(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Choose")).ToArray();
            Check(choose.Length>=7,suffix+" preparation choices available");foreach(var b in choose)Marked(b,true,suffix+" preparation choice");
            Marked(FindButton(prep,"Pass this month"),true,suffix+" pass month");yield return Capture("preparation-"+suffix);prep.SetActive(false);Destroy(prep);yield return null;

            Restore();council.DiplomaticPower(0);yield return null;
            var aid=ScreenRoot().GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Request aid")));
            Marked(aid,true,suffix+" request aid");yield return Capture("aid-"+suffix);

            council.FleetOrders();yield return null;
            var reviews=ScreenRoot().GetComponentsInChildren<Button>().Where(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Review"))).ToArray();
            Check(reviews.Length>0,suffix+" fleet review choices present");foreach(var b in reviews)Marked(b,false,suffix+" fleet review");
            council.ReviewFleetMission(1);yield return null;var confirm=FindButton(ScreenRoot(),"Confirm mission");
            ScrollBottom(ScreenRoot());
            Marked(confirm,true,suffix+" confirm fleet mission");Marked(FindButton(ScreenRoot(),"Return"),false,suffix+" cancel fleet mission");
            NoActionProse(ScreenRoot(),suffix+" fleet confirmation");yield return Capture("fleet-confirmation-"+suffix);yield return Hover(confirm,"fleet-"+suffix);

            council.Sultan();yield return null;
            for(int choice=0;choice<7;choice++){var b=ScreenRoot().GetComponentsInChildren<Button>().First(v=>v.name=="Sultan choice "+choice);Marked(b,choice==5||choice==6,suffix+" Sultan choice "+choice);}
            council.Infirmary();yield return null;Marked(ScreenRoot().GetComponentsInChildren<Button>().First(b=>b.name=="Recruit five monks"),false,suffix+" monk recruitment");
            typeof(CouncilUI).GetMethod("ReviewTraining",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).Invoke(council,new object[]{0});yield return null;
            Marked(FindButton(ScreenRoot(),"Confirm training"),true,suffix+" quality training confirmation");
        }
        // Invoke the same handler used by the serialized repair button: affordability is a rule guard.
        Restore();manager.BringUpSiegeUI();yield return null;walls=Field<GameObject>(manager,"siegeUI");
        var defence=walls.GetComponentInChildren<DefenseActions>(true);defence.GetSelection("fOuterWall");
        VariableSingleton.SetInt("iMoney",0);defence.GetSelection("fOuterWall");
        string stateBefore=State(),rngBefore=Rng();var rest=Field<GameObject>(manager,"restPhaseUI");var probe=rest.AddComponent<TurnActionRestProbe>();
        Check(!Field<Button>(defence,"repairButton").interactable,"Unaffordable repair is disabled");
        defence.RepairSelected();yield return null;
        Check(!rest.activeSelf&&probe.Enables==0&&stateBefore==State()&&rngBefore==Rng(),"Unaffordable repair cannot spend resources, RNG or advance phase");
        VariableSingleton.SetInt("iMoney",100000);defence.GetSelection("fOuterWall");
        int gold=CouncilActions.Int("iMoney"),materials=CouncilActions.Int("iMaterials"),labour=WallRepairRules.Labour,costMaterials=WallRepairRules.Materials("fOuterWall"),turn=VariableSingleton.Turn;
        float integrity=VariableSingleton.GetFloatVariable("fOuterWall");
        Field<Button>(defence,"repairButton").onClick.Invoke();yield return null;yield return null;
        Check(CouncilActions.Int("iMoney")==gold-labour&&CouncilActions.Int("iMaterials")==materials-costMaterials,"Confirmed repair deducts exact displayed gold and materials once");
        float change=VariableSingleton.GetFloatVariable("fOuterWall")-integrity;
        Check(change>=.0799f&&change<=.1101f,"Confirmed repair restores configured integrity range");
        Check(rest.activeSelf&&!walls.activeSelf&&!manager.gameObject.activeSelf&&probe.Enables==1&&VariableSingleton.Turn==turn,"Confirmed repair reaches RestUI once without prematurely advancing calendar");
        Marked(FindButton(rest,"Hear the next petition"),false,"Report acknowledgement");
        yield return Capture("completed-repair");
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
public sealed class TurnActionRestProbe : MonoBehaviour
{
    public int Enables {get;private set;}
    void OnEnable(){Enables++;}
}
#endif

