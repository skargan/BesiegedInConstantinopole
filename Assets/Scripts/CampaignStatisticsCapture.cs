#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Explicit opt-in exported-player checks. All campaign archives stay in Captures/Statistics.
public sealed class CampaignStatisticsCapture : MonoBehaviour
{
    string output;
    int passed;
    CouncilUI council;
    VariableSingleton.CampaignSave baseline;
    readonly List<string> results = new List<string>();
    static readonly string[] Keys = {"fHealth","iMoney","iPopulation","iMaterials","iProvisions","iSecrets","iArchers","iInfantry","iCavalry","iVarangians","fTroopRep","fPeopleRep","fPriestRep","fNobleRep"};
    static readonly string[] Ledgers = {"CITY & STORES","GARRISON & EQUIPMENT","MORALE & SUPPORT","SPECIALISTS & COURT","RECRUITMENT & LOGISTICS"};

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if (!Environment.GetCommandLineArgs().Contains("-statistics-capture")) return;
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Statistics"));
        Directory.CreateDirectory(root);
        SaveGames.TestRoot=Path.Combine(root,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Campaign statistics verification");DontDestroyOnLoad(go);
        go.AddComponent<CampaignStatisticsCapture>().output=root;
    }
    void Check(bool ok,string reason)
    {
        if(!ok)throw new InvalidOperationException("CAMPAIGN STATISTICS: "+reason);
        passed++;results.Add("PASS "+reason);
    }
    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING: campaign statistics verification");
        var work=new Stack<IEnumerator>();work.Push(Run());
        while(work.Count>0) {
            object next=null;bool moved=false;Exception failure=null;
            try {moved=work.Peek().MoveNext();if(moved)next=work.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+passed+" checks\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){work.Pop();continue;}if(next is IEnumerator nested)work.Push(nested);else yield return next;
        }
        string summary="STATISTICS_CHECKS "+passed+" passed across three display sizes; no gameplay actions executed.";
        File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),summary);
        Debug.Log(summary);Application.Quit(0);
    }
    string State(){var state=VariableSingleton.Instance.CaptureState();state.randomState=default(UnityEngine.Random.State);return JsonUtility.ToJson(state);}
    string RandomState()=>JsonUtility.ToJson(UnityEngine.Random.state);
    GameObject Overview()=>GameObject.Find("Council screen");
    string Text(Transform parent)=>string.Join("\n",parent.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
    Rect Bounds(RectTransform rect)
    {
        var corners=new Vector3[4];rect.GetWorldCorners(corners);
        var p=corners.Select(c=>RectTransformUtility.WorldToScreenPoint(null,c)).ToArray();
        return Rect.MinMaxRect(p.Min(v=>v.x),p.Min(v=>v.y),p.Max(v=>v.x),p.Max(v=>v.y));
    }
    bool Inside(Rect a,Rect b)=>b.xMin>=a.xMin-2&&b.xMax<=a.xMax+2&&b.yMin>=a.yMin-2&&b.yMax<=a.yMax+2;
    bool Overlap(Rect a,Rect b)=>Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin)>2&&Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin)>2;
    void TextFits(GameObject root,string context)
    {
        Canvas.ForceUpdateCanvases();var failures=new List<string>();
        foreach(var t in root.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();if(t.isTextOverflowing)failures.Add(t.name+" ["+t.rectTransform.rect.size+"] "+t.text);}
        Check(failures.Count==0,context+" text fits: "+string.Join(" | ",failures));
    }
    void Fixture(int mode)
    {
        CampaignHUD.Instance.DismissReport();council.Hide();
        var state=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));
        VariableSingleton.Instance.RestoreValidated(state);
        if(mode>0)for(int i=0;i<Keys.Length;i++){
            if(Keys[i].StartsWith("f"))VariableSingleton.ChangeFloat(Keys[i],(mode==1?0f:.73f)-VariableSingleton.GetFloatVariable(Keys[i]));
            else VariableSingleton.SetInt(Keys[i],mode==1?0:1234567);
        }
        VariableSingleton.SetInt("iSpies",0);
        VariableSingleton.SetInt("iArmySupport",9); // Obsolete alias must not replace fTroopRep.
        if(mode==0){VariableSingleton.SetInt("iSecrets",17);VariableSingleton.ChangeFloat("fTroopRep",.67f-VariableSingleton.GetFloatVariable("fTroopRep"));}
    }
    void Header(string context)
    {
        var root=GameObject.Find("Campaign Status");Check(root!=null,context+" header exists");
        var strip=root.GetComponent<CampaignStatusStrip>();Check(strip!=null,context+" grouped strip exists");
        var cells=strip.GetComponentsInChildren<CampaignStatTooltip>();
        Check(cells.Length==14&&cells.Select(c=>c.Index).Distinct().Count()==14,context+" fourteen distinct statistics");
        var failures=new List<string>();var rects=new List<Rect>();var outer=Bounds(root.GetComponent<RectTransform>());
        for(int i=0;i<Keys.Length;i++){
            var cell=cells.First(c=>c.Index==i);var value=cell.GetComponentsInChildren<TMP_Text>().FirstOrDefault(t=>t.name=="Stat value "+i);
            string expected=Keys[i].StartsWith("f")?Mathf.RoundToInt(VariableSingleton.GetFloatVariable(Keys[i])*100)+"%":VariableSingleton.GetIntVariable(Keys[i]).ToString("N0",CultureInfo.InvariantCulture);
            if(value==null||value.text!=expected)failures.Add(i+" expected "+expected+" got "+(value==null?"missing":value.text));
            var r=Bounds(cell.GetComponent<RectTransform>());if(!Inside(outer,r))failures.Add("outside header "+i);rects.Add(r);
        }
        for(int i=0;i<rects.Count;i++)for(int j=i+1;j<rects.Count;j++)if(Overlap(rects[i],rects[j]))failures.Add("overlap "+i+"/"+j);
        Check(failures.Count==0,context+" values exact and cells contained without overlap: "+string.Join("; ",failures));
        Check(new[]{"CITY & STORES","GARRISON","SUPPORT"}.All(n=>strip.transform.Find(n)!=null),context+" three named groups");
        Check(Inside(new Rect(0,0,Screen.width,Screen.height),outer),context+" header stays on screen");
        TextFits(root,context+" header");
    }
    IEnumerator HoverChecks(string suffix)
    {
        foreach(var t in FindObjectsOfType<CampaignStatTooltip>())t.OnPointerExit(null);
        yield return null;
        foreach(int index in new[]{0,7,13}){
            var cell=FindObjectsOfType<CampaignStatTooltip>().First(t=>t.Index==index);
            string state=State(),rng=RandomState();
            cell.OnPointerEnter(new PointerEventData(EventSystem.current){position=Bounds(cell.GetComponent<RectTransform>()).center});
            yield return null;
            var detail=GameObject.Find("Statistic detail");
            Check(detail!=null&&Text(detail.transform).Contains(CampaignStatistics.Header[index].Name)&&Text(detail.transform).Contains(CampaignStatistics.Header[index].Value),suffix+" hover "+index+" explains exact statistic");
            Check(Inside(new Rect(0,0,Screen.width,Screen.height),Bounds(detail.GetComponent<RectTransform>())),suffix+" hover "+index+" remains on screen");
            TextFits(detail,suffix+" hover "+index);
            if(index==13)yield return Capture("tooltip-"+suffix);
            cell.OnPointerExit(null);yield return null;
            Check(GameObject.Find("Statistic detail")==null,suffix+" hover "+index+" closes on exit");
            Check(state==State()&&rng==RandomState(),suffix+" hover "+index+" preserves campaign and RNG");
        }
    }
    void CheckOverview(string suffix)
    {
        var root=Overview();Check(root!=null,suffix+" overview opens");
        var transforms=root.GetComponentsInChildren<Transform>();
        Check(Ledgers.All(n=>transforms.Count(t=>t.name==n)==1)&&!transforms.Any(t=>t.name=="FACTION INFLUENCE"),suffix+" five retained ledgers present; retired faction ledger absent");
        string text=Text(root.transform);
        Check(CampaignStatistics.Header.All(s=>text.Contains(s.Name)&&text.Contains(s.Value)),suffix+" all permanent statistics also present in overview");
        Check(new[]{"Engineers available","Monks","Agents in camp","Agents abroad","Characters in service","Available manpower","Ammunition","Wounded:","Food remaining","Galata relations"}.All(text.Contains),suffix+" specialists, logistics, medical and secondary statistics retained");
        Check(transforms.Any(t=>t.name=="Defences")&&transforms.Any(t=>t.name=="Enemy camp"),suffix+" existing defence and enemy sections retained");
        bool gated=true;
        for(int i=0;i<10;i++){
            var row=transforms.FirstOrDefault(t=>t.name=="Enemy report "+i);
            if(row==null||!Text(row).Contains("Requires "+(i+1)+" agents"))gated=false;
        }
        Check(gated,suffix+" zero-agent fixture keeps all ten enemy intelligence values locked");
        TextFits(root,suffix+" overview");
    }
    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;
        Check(SaveGames.Automated,"Saves isolated before campaign start");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+20;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized&&FindObjectOfType<ManagerUI>()!=null,"Campaign initialised");
        yield return new WaitForSecondsRealtime(.5f);
        CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720),new Vector2Int(1100,760)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);
            string suffix=size.x+"x"+size.y;Check(Screen.width==size.x&&Screen.height==size.y,suffix+" resolution applied");
            for(int mode=0;mode<3;mode++){
                Fixture(mode);yield return new WaitForSecondsRealtime(.2f);Header(suffix+" fixture "+mode);
                if(mode==2)yield return Capture("header-large-"+suffix);
            }
            Fixture(0);yield return new WaitForSecondsRealtime(.2f);yield return Capture("council-"+suffix);
            yield return HoverChecks(suffix);
            string state=State(),rng=RandomState();council.Overview();yield return null;
            yield return Capture("overview-"+suffix);
            File.WriteAllText(Path.Combine(output,"overview-before-"+suffix+".json"),state);
            File.WriteAllText(Path.Combine(output,"overview-after-"+suffix+".json"),State());
            File.WriteAllText(Path.Combine(output,"overview-rng-"+suffix+".txt"),"BEFORE "+rng+"\nAFTER "+RandomState());
            CheckOverview(suffix);
            Check(rng==RandomState(),suffix+" opening overview preserves RNG");
            Check(state==State(),suffix+" opening overview preserves campaign state");
            var scroll=Overview().GetComponentsInChildren<ScrollRect>().FirstOrDefault(s=>s.isActiveAndEnabled);
            Check(scroll!=null,suffix+" longer statistics remain scrollable");
            float range=Mathf.Max(1,scroll.content.rect.height-scroll.viewport.rect.height);
            scroll.verticalNormalizedPosition=Mathf.Clamp01(1-370f/range);Canvas.ForceUpdateCanvases();
            yield return Capture("overview-lower-stats-"+suffix);
            Check(state==State()&&rng==RandomState(),suffix+" scrolling preserves campaign and RNG");
            council.Hide();
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

