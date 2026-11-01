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

// Opt-in integration checks in a real exported player; archives are isolated.
public sealed class WarTheatreCapture : MonoBehaviour
{
    string output; int passed;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){
        if(!Environment.GetCommandLineArgs().Contains("-theatre-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Theatre"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Theatre verification");DontDestroyOnLoad(go);go.AddComponent<WarTheatreCapture>().output=path;
    }
    void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;checks.Add("PASS "+label);}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    string Text(GameObject go)=>string.Join("\n",go.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
    GameObject Map()=>GameObject.Find("War Beyond the Walls map");
    Dictionary<string,List<TerritoryGraphic>> Groups()=>
        (Dictionary<string,List<TerritoryGraphic>>)typeof(WarTheatreUI).GetField("powerShapes",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Map().GetComponent<WarTheatreUI>());
    void Hover(TerritoryGraphic shape,bool on){if(on)shape.OnPointerEnter(new PointerEventData(EventSystem.current));else shape.OnPointerExit(new PointerEventData(EventSystem.current));Canvas.ForceUpdateCanvases();}
    void Click(TerritoryGraphic shape)=>shape.OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
    void Button(string title){var b=Map().GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==title));b.onClick.Invoke();}
    IEnumerator Start(){
        File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),checks);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+passed+"\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;
        }
        string result="THEATRE_CHECKS "+passed+" passed";File.WriteAllLines(Path.Combine(output,"details.txt"),checks);File.WriteAllText(Path.Combine(output,"checks.txt"),result);Debug.Log(result);Application.Quit(0);
    }
    IEnumerator Run(){
        AudioListener.volume=0;Application.runInBackground=true;yield return null;
        Check(SaveGames.Automated,"Isolated archives");SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+25;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"Campaign initialized");yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();
        WarTheatre.Validate(WarTheatre.Data);Check(WarTheatre.Data.regions.Count(r=>r.kind=="province")==11,"Eleven stable provinces");
        Check(WarTheatre.Find("aq_qoyunlu")!=null,"Aq Qoyunlu on map");
        Check(CouncilActions.AllyIndexForRealm("aq_qoyunlu")==17,"Aq appended at stable index17");
        var baseline=VariableSingleton.Instance.CaptureState();
        var legacy=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));
        legacy.ints.RemoveAll(v=>v.key=="iAidState17"||v.key=="iAidDays17"||v.key=="iAidWarships17");
        VariableSingleton.SetInt("iAidState17",1);VariableSingleton.SetInt("iAidDays17",20);VariableSingleton.SetInt("iAidWarships17",7);
        VariableSingleton.Instance.RestoreValidated(legacy);
        Check(CouncilActions.Int("iAidState17")==0&&CouncilActions.Int("iAidDays17")==0&&AidRouting.Warships(17)==0,"Legacy save does not inherit another campaign's Aq appeal");
        VariableSingleton.SetInt("iAidState17",1);VariableSingleton.SetInt("iAidDays17",35);
        var pending=VariableSingleton.Instance.CaptureState();VariableSingleton.SetInt("iAidState17",0);VariableSingleton.Instance.RestoreValidated(pending);
        Check(CouncilActions.Int("iAidState17")==1&&CouncilActions.Int("iAidDays17")==35,"Pending Aq appeal survives save round trip");
        int gold=CouncilActions.Int("iMoney"),food=CouncilActions.Int("iProvisions");
        string arrival=CouncilActions.AdvanceAid(35);
        Check(CouncilActions.Int("iAidState17")==2&&CouncilActions.Int("iMoney")==gold+3000&&CouncilActions.Int("iProvisions")==food+300&&arrival.Contains("Turkmen Confederations"),"Aq promised supplies arrive on time");
        CouncilActions.AdvanceAid(1);
        Check(CouncilActions.Int("iMoney")==gold+3000&&CouncilActions.Int("iProvisions")==food+300,"Aq aid cannot be delivered twice");
        VariableSingleton.Instance.RestoreValidated(baseline);
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);
            string suffix=size.x+"x"+size.y;WarTheatreUI.Show();yield return null;Canvas.ForceUpdateCanvases();
            string state=State();var groups=Groups();Check(groups.Values.SelectMany(v=>v).All(g=>!g.Highlighted),suffix+" starts without sticky Macedonia highlight");
            foreach(string power in new[]{"venice","genoa","naples","aq_qoyunlu"}){
                var members=groups[power];Check(members.Count>=(power=="aq_qoyunlu"?1:2),suffix+" grouped "+power);
                foreach(var shape in members){
                    Hover(shape,true);Check(members.All(s=>s.Highlighted),suffix+" all "+power+" highlighted from "+shape.name);
                    Check(groups.Where(p=>p.Key!=power).SelectMany(p=>p.Value).All(s=>!s.Highlighted),suffix+" unrelated territories remain quiet for "+shape.name);
                    Click(shape);Check(Text(Map()).Contains(WarTheatre.Find(power).name),suffix+" parent diplomatic identity from "+shape.name);
                    Hover(shape,false);Check(groups.Values.SelectMany(v=>v).All(s=>!s.Highlighted),suffix+" clear on exit "+shape.name);
                }
                Hover(members[0],true);yield return Capture(power+"-hover-"+suffix);Hover(members[0],false);
            }
            // Newly covered land must use the same visible polygon and click target.
            string[] enlarged={"moldavia","bosnia","albania","trebizond","aq_qoyunlu"};
            Vector2[] samples={new Vector2(805,60),new Vector2(480,257),new Vector2(493,300),new Vector2(1500,337),new Vector2(1700,420)};
            for(int i=0;i<enlarged.Length;i++){
                var shape=groups[enlarged[i]][0];var p=new Vector2(samples[i].x/2048f,1-samples[i].y/683f);
                Check(TerritoryGraphic.Contains(p,shape.Points),suffix+" expanded land included: "+enlarged[i]);
                var rect=shape.rectTransform.rect;
                var screen=RectTransformUtility.WorldToScreenPoint(null,shape.rectTransform.TransformPoint(new Vector3(rect.xMin+p.x*rect.width,rect.yMin+p.y*rect.height,0)));
                Check(shape.Raycast(screen,null),suffix+" expanded land clickable: "+enlarged[i]);
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=screen},hits);
                Check(hits.Count>0&&hits[0].gameObject==shape.gameObject,suffix+" expanded land not stolen by neighbour: "+enlarged[i]);
                Hover(shape,true);Click(shape);Check(shape.Highlighted&&Text(Map()).Contains(WarTheatre.Find(enlarged[i]).name),suffix+" expanded territory opens correct details: "+enlarged[i]);Hover(shape,false);
            }
            var easternLabel=Map().GetComponentsInChildren<TMP_Text>().First(t=>t.text=="Turkmen\nConfederations");
            Check(!easternLabel.isTextOverflowing,suffix+" eastern map label fits");
            Check(State()==state,suffix+" map hover and territory selection preserve campaign and RNG");
            // Direct cross-power entry must also clear the previous outline.
            Hover(groups["macedonia"][0],true);Hover(groups["venice"][0],true);
            Check(groups["macedonia"].All(s=>!s.Highlighted),suffix+" crossing to Venice clears Macedonia");
            Hover(groups["genoa"][0],true);Hover(groups["genoa"][1],true);Hover(groups["genoa"][0],false);
            Check(groups["genoa"].All(g=>g.Highlighted),suffix+" late exit from previous dependency cannot clear current hover");Hover(groups["genoa"][1],false);
            Button("Foreign powers");yield return null;groups=Groups();Check(!groups.ContainsKey("venice")&&groups.Values.SelectMany(v=>v).All(s=>!s.Highlighted),suffix+" hiding powers clears highlight");
            Button("Foreign powers");yield return null;groups=Groups();Check(groups.ContainsKey("venice")&&groups["venice"].All(s=>!s.Highlighted),suffix+" restored powers start quiet");
            var geo=Map().GetComponentsInChildren<RawImage>().First(r=>r.name=="Geographic base").rectTransform;float before=geo.rect.width;
            Button("+");Check(geo.rect.width>before,suffix+" zoom works");Button("Fit");Check(Mathf.Abs(geo.rect.width-before)<1,suffix+" fit restores width");
            foreach(var g in groups.Values.SelectMany(v=>v)){
                var points=g.Points;Vector2 min=points.Aggregate(Vector2.Min),max=points.Aggregate(Vector2.Max);Vector2 centre=(min+max)*.5f;Vector2? inside=TerritoryGraphic.Contains(centre,points)?centre:(Vector2?)null;
                for(int x=0;x<24&&!inside.HasValue;x++)for(int y=0;y<24&&!inside.HasValue;y++){var p=new Vector2(Mathf.Lerp(min.x,max.x,(x+.5f)/24),Mathf.Lerp(min.y,max.y,(y+.5f)/24));if(TerritoryGraphic.Contains(p,points))inside=p;}
                Check(inside.HasValue,suffix+" selectable polygon interior "+g.name);
                var rect=g.rectTransform.rect;var local=new Vector3(rect.xMin+inside.Value.x*rect.width,rect.yMin+inside.Value.y*rect.height,0);
                var screen=RectTransformUtility.WorldToScreenPoint(null,g.rectTransform.TransformPoint(local));
                Check(g.Raycast(screen,null),suffix+" polygon raycast "+g.name);
            }
            foreach(string name in new[]{"Galata","Constantinople"}){
                var marker=Map().GetComponentsInChildren<TerritoryGraphic>().First(g=>g.name==name);
                var screen=RectTransformUtility.WorldToScreenPoint(null,marker.rectTransform.TransformPoint(marker.rectTransform.rect.center));
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=screen},hits);
                Check(hits.Count>0&&hits[0].gameObject==marker.gameObject,suffix+" "+name+" has independent unobstructed click target");
            }
            yield return Capture("theatre-fit-"+suffix);
            var aq=groups["aq_qoyunlu"][0];Click(aq);Button("Diplomatic relations");yield return null;
            Check(Map()==null,suffix+" map closes into diplomacy");
            var council=GameObject.Find("Council screen");Check(council!=null&&Text(council).Contains("Turkmen Confederations"),suffix+" Aq profile opens directly");
            yield return Capture("aq-diplomacy-"+suffix);
            council.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="The realm and its interests")).onClick.Invoke();yield return null;
            council=GameObject.Find("Council screen");
            Check(Text(council).Contains("Qara Qoyunlu")&&Text(council).Contains("neither speaks for the other"),suffix+" About distinguishes the rival confederations");
            Check(Text(council).Contains("Your present embassy approaches the Aq Qoyunlu"),suffix+" About identifies the current embassy");
            yield return Capture("turkmen-about-"+suffix);
            var articleScroll=council.GetComponentInChildren<ScrollRect>();
            if(articleScroll!=null){articleScroll.verticalNormalizedPosition=0;yield return Capture("turkmen-about-bottom-"+suffix);}
            FindObjectOfType<CouncilUI>(true).Hide();
        }
    }
    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.2f);
        Canvas.ForceUpdateCanvases();
        File.WriteAllLines(Path.Combine(output, name + "-text.txt"), FindObjectsOfType<TMP_Text>().Select(t => t.name + " | " + t.rectTransform.rect + " | " + t.text));
        var cameraObject = new GameObject("Statistics capture camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true; camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = ImperialTheme.Navy; camera.nearClipPlane = .1f; camera.farClipPlane = 100;
        var canvases = FindObjectsOfType<Canvas>().Where(c => c.isRootCanvas).ToArray();
        var modes = canvases.Select(c => c.renderMode).ToArray();
        var cameras = canvases.Select(c => c.worldCamera).ToArray();
        var distances = canvases.Select(c => c.planeDistance).ToArray();
        var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        var previous = RenderTexture.active;
        Texture2D shot = null;
        try {
            camera.targetTexture = target;
            foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10; }
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
            shot = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); shot.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), shot.EncodeToPNG());
        } finally {
            RenderTexture.active = previous;
            for (int i = 0; i < canvases.Length; i++) { canvases[i].renderMode = modes[i]; canvases[i].worldCamera = cameras[i]; canvases[i].planeDistance = distances[i]; }
            camera.targetTexture = null; RenderTexture.ReleaseTemporary(target);
            if (shot != null) Destroy(shot); Destroy(cameraObject); Canvas.ForceUpdateCanvases();
        }
        yield return null;
    }
}
#endif
