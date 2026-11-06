#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Explicit opt-in exported-player test; never reads or writes player archives.
public sealed class DiscoveryCapture : MonoBehaviour
{
    string output;CouncilUI council;readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-discovery-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Discoveries"));Directory.CreateDirectory(folder);
        SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        Application.runInBackground=true;
        var go=new GameObject("Technology verification");DontDestroyOnLoad(go);go.AddComponent<DiscoveryCapture>().output=folder;}

    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    GameObject Root()=>GameObject.Find("Council screen");
    TechnologyDiscoveryRoom Room()=>Root().GetComponent<TechnologyDiscoveryRoom>();
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+results.Count+" checks\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"DISCOVERY_CHECKS "+results.Count+" passed");Application.Quit(0);}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;Check(SaveGames.Automated,"isolated player archives");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+20;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());
        Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.5f);council.Technologies();yield return null;
        Check(Room()!=null,"room installed");Check(!SaveGames.DiscoveryAchievementEligible,"test runs cannot unlock career achievements");
        Check(Root().GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="RESEARCH AND REDISCOVERY"),"research title renamed");
        var cornerRing=Root().GetComponentInChildren<TechnologyPullRing>();
        Check(cornerRing!=null&&cornerRing.mainTexture==Resources.Load<Texture2D>("UI/MidnightMosaic/technology-pull-ring"),"generated bronze handle installed");
        Check(cornerRing.rectTransform.anchorMin==Vector2.one&&cornerRing.rectTransform.anchorMax==Vector2.one,"handle anchored to desk corner");
        foreach(var name in new[]{"grind","pour","fire","bell","boil","choir","growl","coins"}){var clip=Resources.Load<AudioClip>("Audio/Discoveries/"+name);Check(clip!=null&&clip.length>.4f&&clip.length<=3,"audio imported "+name);}
        // Read eligibility only; never award against real user preferences.
        var testPath=VariableSingleton.TestSavePath;VariableSingleton.TestSavePath=null;
        Check(SaveGames.DiscoveryAchievementEligible&&!SaveGames.AchievementEligible,"Normal allows discovery only");
        VariableSingleton.TestSavePath=testPath;SaveGames.PrepareNew(true);SaveGames.BeginNew();VariableSingleton.TestSavePath=null;
        Check(SaveGames.DiscoveryAchievementEligible&&SaveGames.AchievementEligible,"Ironman allows both categories");
        VariableSingleton.TestSavePath=testPath;SaveGames.PrepareNew(false);SaveGames.BeginNew();
        VariableSingleton.SetInt("iSecrets",2);yield return null;Check(!Root().GetComponentsInChildren<Button>().Single(b=>b.name=="Research selected technology").interactable,"research initially unaffordable");
        int gold=DailyLifeEvents.I("iMoney"),secrets=DailyLifeEvents.I("iSecrets");
        Check(!Room().Activate(1),"cannot skip alchemy step");Check(Room().Activate(6),"letter independent of alchemy");Check(DailyLifeEvents.I("iSecrets")==secrets+1,"letter gives one secret immediately");
        yield return new WaitForSecondsRealtime(2.7f);Check(Root().GetComponentsInChildren<Button>().Single(b=>b.name=="Research selected technology").interactable,"discovery refreshes research affordability without reopening");Check(Room().Activate(6),"letter revisitable");yield return new WaitForSecondsRealtime(2.7f);Check(DailyLifeEvents.I("iSecrets")==secrets+1,"letter cannot duplicate reward");
        Check(Room().Activate(0),"first alchemy step");yield return new WaitForSecondsRealtime(2.7f);Check(Room().Experiment.Step==1,"step advances after interaction");
        Root().GetComponentsInChildren<Button>().Single(b=>b.name=="Technology return").onClick.Invoke();yield return null;council.Technologies();yield return null;
        Check(Room().Experiment.Step==0,"leaving room resets incomplete experiment");Check(TechnologyDiscoveries.Found(TechnologyDiscoveries.Kind.Letter),"leaving preserves independent discovery");
        for(int n=0;n<6;n++){Check(Room().Activate(n),"alchemy sequence "+n);Check(!Room().Activate(n),"busy guard "+n);yield return new WaitForSecondsRealtime(2.7f);}
        Check(DailyLifeEvents.I("iSecrets")==secrets+2,"alchemy reward exactly one secret");Check(!Room().Activate(0),"completed alchemy cannot farm rewards");
        var rail=Root().GetComponentInChildren<TechnologyPanelRail>();var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=new Vector2(1200,800)};
        rail.OnPointerDown(pointer);Check(ByzantineCursor.IsGripping,"grab cursor selected");rail.OnPointerUp(pointer);Check(!ByzantineCursor.IsGripping,"cursor released");yield return null;
        Check(GameObject.Find("Technology desk").GetComponent<RectTransform>().anchorMin.x==.015f,"ring click moves panel left");
        rail.OnPointerDown(pointer);pointer.position+=new Vector2(160,0);rail.OnDrag(pointer);rail.OnPointerUp(pointer);
        var desk=GameObject.Find("Technology desk").GetComponent<RectTransform>();Check(desk.anchorMin.x>.015f&&desk.anchorMin.x<.505f,"drag stays at intermediate rail position");
        rail.OnPointerDown(pointer);pointer.position+=new Vector2(-10000,0);rail.OnDrag(pointer);rail.OnPointerUp(pointer);Check(desk.anchorMin.x==.015f,"rail clamps left edge");
        Room().Hover(7,true);yield return Capture("left-panel-skull");Room().Hover(7,false);Check(Room().Activate(7),"skull independent discovery");yield return new WaitForSecondsRealtime(2.7f);
        Check(DailyLifeEvents.I("iSecrets")==secrets+2&&DailyLifeEvents.I("iMoney")==gold,"skull gives journal entry only");
        Check(Room().Activate(8),"treasure discovery");yield return new WaitForSecondsRealtime(.3f);yield return Capture("treasure-reward");yield return new WaitForSecondsRealtime(2.5f);
        Check(DailyLifeEvents.I("iSecrets")==secrets+3&&DailyLifeEvents.I("iMoney")==gold+2500,"all rewards correct");
        Check(DecisionJournal.Copy().Count(e=>e.id.StartsWith("discovery_"))==4,"four unique journal entries");
        SaveGames.Checkpoint();var archive=SaveGames.Read(VariableSingleton.SavePath);
        foreach(TechnologyDiscoveries.Kind k in Enum.GetValues(typeof(TechnologyDiscoveries.Kind)))VariableSingleton.SetInt(TechnologyDiscoveries.Key(k),0);
        Check(SaveGames.Load(VariableSingleton.SavePath,out var loadError),"archive load succeeds: "+loadError);
        foreach(TechnologyDiscoveries.Kind k in Enum.GetValues(typeof(TechnologyDiscoveries.Kind))){Check(TechnologyDiscoveries.Found(k),"save/load flag "+k);Check(!TechnologyDiscoveries.Claim(k),"save/load no repeat reward "+k);}
        Check(DecisionJournal.Copy().Count(e=>e.id.StartsWith("discovery_"))==4,"journal survives save/load");
        var experiment=new TechnologyDiscoveries.Experiment();experiment.Advance(0,0);experiment.Tick(59);Check(experiment.Step==1,"idle before timeout");experiment.Tick(60);Check(experiment.Step==0,"idle resets at sixty seconds");
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720),new Vector2Int(1920,1200),new Vector2Int(2560,1080)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);council.Technologies();yield return null;Canvas.ForceUpdateCanvases();
            var spots=Root().GetComponentsInChildren<LocationHotspot>();
            var skull=spots.Single(h=>h.name.Contains("curious skull"));var point=TechnologyDiscoveryShapes.Map(TechnologyDiscoveryShapes.Anchors[7],Root().transform.Find("Scene artwork").GetComponent<RawImage>().uvRect);
            var rect=skull.rectTransform.rect;var world=skull.transform.TransformPoint(new Vector3(rect.xMin+point.x*rect.width,rect.yMin+point.y*rect.height));
            var hitData=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,world)};
            var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(hitData,hits);
            Check(hits.Count>0&&hits[0].gameObject!=skull.gameObject,"opaque desk blocks hidden skull "+size);Check(spots.Length==9,"nine hotspots "+size);
            var uv=Root().transform.Find("Scene artwork").GetComponent<RawImage>().uvRect;
            for(int i=0;i<spots.Length;i++){var centre=TechnologyDiscoveryShapes.Map(TechnologyDiscoveryShapes.Anchors[i],uv);Check(spots[i].Contains(centre),"traced interior "+spots[i].name+" "+size);}
            VariableSingleton.SetInt(TechnologyDiscoveries.Key(TechnologyDiscoveries.Kind.Alchemy),0);yield return null;
            for(int i=0;i<6;i++){
                var spot=spots.Single(h=>h.name.Contains(TechnologyDiscoveryRoom.Labels[i]));
                var anchor=TechnologyDiscoveryShapes.Map(TechnologyDiscoveryShapes.Anchors[i],uv);
                var sr=spot.rectTransform.rect;var wp=spot.transform.TransformPoint(new Vector3(sr.xMin+anchor.x*sr.width,sr.yMin+anchor.y*sr.height));
                var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,wp)};
                var actual=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,actual);
                Check(actual.Count>0&&actual[0].gameObject==spot.gameObject,"alchemy accessible beside default desk "+i+" "+size);
                spot.SetHighlighted(true);if(size.x==1600)yield return Capture("alchemy-outline-"+i);spot.SetHighlighted(false);
                Room().Experiment.Advance(i,Time.unscaledTime);yield return null;
            }
            var letter=spots.Single(h=>h.name.Contains("pigeon's letter"));
            var chest=spots.Single(h=>h.name.Contains("concealed catch"));
            Check(!letter.Contains(TechnologyDiscoveryShapes.Map(new Vector2(600/1672f,1-130/941f),uv)),"pigeon body is not a letter target "+size);
            Check(!chest.Contains(TechnologyDiscoveryShapes.Map(new Vector2(1400/1672f,1-400/941f),uv)),"chest corner is not a catch target "+size);
            Check(!skull.Contains(TechnologyDiscoveryShapes.Map(new Vector2(1386/1672f,1-110/941f),uv)),"empty skull bounding-box corner excluded "+size);
            VariableSingleton.SetInt(TechnologyDiscoveries.Key(TechnologyDiscoveries.Kind.Alchemy),1);
            Room().Hover(6,true);letter.SetHighlighted(true);yield return Capture("right-panel-"+size.x+"x"+size.y);Room().Hover(6,false);letter.SetHighlighted(false);
            rail=Root().GetComponentInChildren<TechnologyPanelRail>();rail.OnPointerDown(pointer);rail.OnPointerUp(pointer);yield return null;hits.Clear();UnityEngine.EventSystems.EventSystem.current.RaycastAll(hitData,hits);Check(hits.Count>0&&hits[0].gameObject==skull.gameObject,"exposed skull receives pointer "+size);
            Room().Hover(7,true);skull.SetHighlighted(true);yield return Capture("left-panel-"+size.x+"x"+size.y);skull.SetHighlighted(false);Room().Hover(7,false);
            if(size.x==1600){Room().Hover(8,true);chest.SetHighlighted(true);yield return Capture("chest-knob-outline");chest.SetHighlighted(false);Room().Hover(8,false);}
            foreach(var text in Root().GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"text fits "+size+" "+text.name);}
        }
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);
            council.MonasticResearchView();yield return null;Canvas.ForceUpdateCanvases();
            Check(Root().transform.Find("Scene artwork").GetComponent<RawImage>().texture==Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/monastic-rediscovery"),"rediscovery backdrop installed "+size);
            Check(!Root().transform.Find("Infirmary desk/Medical tab 1").GetComponent<Button>().interactable,"research allocation tab selected "+size);
            yield return Capture("monastic-rediscovery-"+size.x);
            ControlSingleton.Instance.GoBack();yield return null;
            Check(Room()!=null,"monastic research returns to desk "+size);
        }
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
