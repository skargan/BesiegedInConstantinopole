#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;

// Explicit exported-player art inspection. Saves are isolated before scene load.
// Uses actual EventCardArt.Refresh, without running narrative commands or choices.
public sealed class ArtIntegrationCapture : MonoBehaviour
{
    const string ManifestResource = "Content/EventArtwork";
    [Serializable] sealed class ArtEntry { public string node,path,source,fit; public float focusY; }
    [Serializable] sealed class ArtManifest { public ArtEntry[] entries; }
    string output;
    int passed;
    readonly List<string> checks=new List<string>();
    readonly List<string> textures=new List<string>();
    ArtEntry[] entries;
    GameObject fixture;
    Canvas canvas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-art-integration-capture")&&!Environment.GetCommandLineArgs().Contains("-event-frame-capture"))return;
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,Environment.GetCommandLineArgs().Contains("-event-frame-capture")?"../Captures/EventFrames":"../Captures/ArtIntegration"));
        Directory.CreateDirectory(root);
        SaveGames.TestRoot=Path.Combine(root,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Art integration verification");DontDestroyOnLoad(go);
        go.AddComponent<ArtIntegrationCapture>().output=root;
    }
    void Check(bool ok,string label)
    {
        if(!ok)throw new InvalidOperationException("ART INTEGRATION: "+label);
        passed++;checks.Add("PASS "+label);
    }
    string State(){var save=VariableSingleton.Instance.CaptureState();save.randomState=default(UnityEngine.Random.State);return JsonUtility.ToJson(save);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    string Text(GameObject root)=>string.Join("\n",root.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
    Rect Bounds(RectTransform rect)
    {
        var corners=new Vector3[4];rect.GetWorldCorners(corners);
        var p=corners.Select(c=>RectTransformUtility.WorldToScreenPoint(null,c)).ToArray();
        return Rect.MinMaxRect(p.Min(v=>v.x),p.Min(v=>v.y),p.Max(v=>v.x),p.Max(v=>v.y));
    }
    bool Inside(Rect outer,Rect inner)=>inner.xMin>=outer.xMin-2&&inner.xMax<=outer.xMax+2&&inner.yMin>=outer.yMin-2&&inner.yMax<=outer.yMax+2;
    bool Overlap(Rect a,Rect b)=>Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin)>2&&Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin)>2;
    void TextFits(GameObject root,string label)
    {
        foreach(var text in root.GetComponentsInChildren<TMP_Text>()){
            text.ForceMeshUpdate();Check(!text.isTextOverflowing,label+" text fits: "+text.name);
        }
    }
    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");
        var work=new Stack<IEnumerator>();work.Push(Environment.GetCommandLineArgs().Contains("-event-frame-capture")?FrameRun():Run());
        while(work.Count>0){
            object next=null;bool moved=false;Exception failure=null;
            try{moved=work.Peek().MoveNext();if(moved)next=work.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){
                File.WriteAllLines(Path.Combine(output,"details.txt"),checks);
                File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+passed+" checks\n"+failure);
                Debug.LogException(failure);Application.Quit(1);yield break;
            }
            if(!moved){work.Pop();continue;}
            if(next is IEnumerator nested)work.Push(nested);else yield return next;
        }
        string result=Environment.GetCommandLineArgs().Contains("-event-frame-capture")?"EVENT_FRAME_CHECKS "+passed+" passed across six PC resolutions; no event decisions executed.":"ART_INTEGRATION_CHECKS "+passed+" passed; 14 header sprites, all "+entries.Length+" manifest entries, and aspect-preserving rendering checked at 1600x900 / 1280x720. Ten representative event-art fixtures captured at each resolution. No event choices executed.";
        File.WriteAllLines(Path.Combine(output,"details.txt"),checks);
        File.WriteAllLines(Path.Combine(output,"textures.txt"),textures);
        File.WriteAllText(Path.Combine(output,"checks.txt"),result);Debug.Log(result);Application.Quit(0);
    }
    void Header(string label)
    {
        var root=GameObject.Find("Campaign Status");Check(root!=null,label+" header exists");
        var cells=root.GetComponentsInChildren<CampaignStatTooltip>();
        Check(cells.Length==14&&cells.Select(t=>t.Index).Distinct().Count()==14,label+" fourteen distinct statistics");
        var bounds=new List<Rect>();var outer=Bounds(root.GetComponent<RectTransform>());
        foreach(var cell in cells){
            var index=cell.Index;
            var value=cell.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Stat value "+index);
            Check(value.text==CampaignStatistics.Header[index].Value,label+" exact statistic value "+index);
            // The new integration uses a real Image/Sprite; dividers/backgrounds do not.
            var icons=cell.GetComponentsInChildren<Image>().Where(i=>i.sprite!=null&&i.gameObject.name=="Stat symbol "+index).ToArray();
            Check(icons.Length==1,label+" one raster stat icon "+index);
            var icon=icons[0];var sprite=CampaignStatArt.Get((CampaignStatArt.Stat)index);
            Check(sprite!=null&&icon.sprite==sprite&&icon.color.a>0,label+" mapped sprite rendered "+index);
            Check(!icon.raycastTarget&&icon.preserveAspect,label+" icon preserves aspect and cannot block tooltip "+index);
            var cellBounds=Bounds((RectTransform)cell.transform);bounds.Add(cellBounds);
            Check(Inside(outer,cellBounds)&&Inside(cellBounds,Bounds(icon.rectTransform)),label+" stat/icon contained "+index);
            Check(!cell.GetComponentsInChildren<CampaignStatGlyph>().Any(),label+" no vector fallback masking missing art "+index);
        }
        for(int i=0;i<bounds.Count;i++)for(int j=i+1;j<bounds.Count;j++)Check(!Overlap(bounds[i],bounds[j]),label+" cells separated "+i+"/"+j);
        Check(Inside(new Rect(0,0,Screen.width,Screen.height),outer),label+" header stays on screen");
        TextFits(root,label+" header");
    }
    IEnumerator HeaderTooltips(string suffix)
    {
        foreach(var cell in FindObjectsOfType<CampaignStatTooltip>().OrderBy(c=>c.Index)){
            string state=State(),rng=Rng();var stat=CampaignStatistics.Header[cell.Index];
            cell.OnPointerEnter(new PointerEventData(EventSystem.current){position=Bounds((RectTransform)cell.transform).center});yield return null;
            var tip=GameObject.Find("Statistic detail");
            Check(tip!=null&&Text(tip).Contains(stat.Name)&&Text(tip).Contains(stat.Value),suffix+" exact tooltip "+cell.Index);
            Check(Inside(new Rect(0,0,Screen.width,Screen.height),Bounds(tip.GetComponent<RectTransform>())),suffix+" tooltip contained "+cell.Index);
            TextFits(tip,suffix+" tooltip "+cell.Index);
            if(cell.Index==13)yield return Capture("header-tooltip-"+suffix);
            cell.OnPointerExit(null);yield return null;
            Check(GameObject.Find("Statistic detail")==null,suffix+" tooltip closes "+cell.Index);
            Check(State()==state&&Rng()==rng,suffix+" tooltip leaves campaign and random stream unchanged "+cell.Index);
        }
    }
    void CreateFixture()
    {
        fixture=CampaignHUD.Panel(canvas.transform,"Event art inspection fixture",new Vector2(.025f,.025f),new Vector2(.975f,.835f));
        ImperialTheme.Panel(fixture);
        var narrative=CampaignHUD.Panel(fixture.transform,"Event narrative",new Vector2(.025f,.035f),new Vector2(.43f,.675f));
        narrative.GetComponent<Image>().color=ImperialTheme.Navy;ImperialTheme.Frame(narrative);
        CampaignHUD.Label(narrative.transform,"Event artwork inspection\n\nThis isolated preview uses the production artwork renderer. No event choice or campaign consequence is executed.",new Vector2(.04f,.08f),new Vector2(.96f,.94f),24);
        var choices=CampaignHUD.Panel(fixture.transform,"Choice area",new Vector2(.46f,.035f),new Vector2(.975f,.965f));
        choices.GetComponent<Image>().color=ImperialTheme.Navy;
        CampaignHUD.Label(choices.transform,"ARTWORK REVIEW",new Vector2(.04f,.85f),new Vector2(.96f,.97f),29);
        CampaignHUD.Label(choices.transform,"Event identity",new Vector2(.04f,.62f),new Vector2(.96f,.83f),26).name="Inspected event node";
        CampaignHUD.MakeButton(choices.transform,"Inspection only",new Vector2(.04f,.38f),new Vector2(.96f,.52f),()=>{}).interactable=false;
    }
    void ValidatePicture(ArtEntry entry,string context)
    {
        var frame=fixture.transform.Find("Event artwork");Check(frame!=null&&frame.gameObject.activeInHierarchy,context+" artwork frame visible");
        var raw=frame.GetComponentInChildren<RawImage>();var expected=Resources.Load<Texture2D>(entry.path);
        Check(raw!=null&&expected!=null&&raw.texture==expected,context+" correct manifest texture displayed");
        Check(!raw.raycastTarget,context+" artwork does not intercept choices");
        var viewport=frame.Find("Artwork viewport") as RectTransform;
        Check(viewport!=null&&viewport.GetComponent<RectMask2D>()!=null&&raw.maskable,context+" inset image clipping active");
        Check(viewport.offsetMin==Vector2.one*EventArtworkView.ArtInset&&viewport.offsetMax==-Vector2.one*EventArtworkView.ArtInset,context+" image sits inside gold border");
        var rect=Bounds(raw.rectTransform);var uv=raw.uvRect;
        Check(Inside(Bounds(viewport),rect),context+" image stays inside inset viewport");
        Check(rect.width>1&&rect.height>1&&Inside(Bounds((RectTransform)frame),rect),context+" picture contained in artwork frame");
        Check(uv.width>0&&uv.height>0&&uv.xMin>=-.001f&&uv.yMin>=-.001f&&uv.xMax<=1.001f&&uv.yMax<=1.001f,context+" valid image crop");
        float imageAspect=(expected.width*uv.width)/(expected.height*uv.height);
        float displayedAspect=rect.width/rect.height;
        Check(Mathf.Abs(displayedAspect/imageAspect-1)<.02f,context+" no horizontal or vertical distortion");
        if(string.IsNullOrEmpty(entry.fit)||entry.fit=="contain")Check(Mathf.Abs(uv.x)<.001f&&Mathf.Abs(uv.y)<.001f&&Mathf.Abs(uv.width-1)<.001f&&Mathf.Abs(uv.height-1)<.001f,context+" entire contained illustration retained");
    }
    ArtEntry[] Representatives()
    {
        var result=new List<ArtEntry>();
        string[] priorities={"story1","story3","story8","story11","story19","story22","random4","random13","lifeSurprise","combatAssassin","foodShortage"};
        foreach(string node in priorities){var e=entries.FirstOrDefault(v=>v.node==node);if(e!=null&&!result.Any(v=>v.path==e.path))result.Add(e);if(result.Count==10)return result.ToArray();}
        foreach(var e in entries)if(!result.Any(v=>v.path==e.path)){result.Add(e);if(result.Count==10)break;}
        Check(result.Count==10,"ten distinct representative illustrations available");return result.ToArray();
    }
    IEnumerator PreviewChecks(ArtEntry entry,string suffix)
    {
        EventCardArt.Refresh(fixture.transform,entry.node,"Preview interaction verification.");yield return null;
        var frame=fixture.transform.Find("Event artwork");var button=frame.GetComponent<Button>();
        Check(button!=null&&button.interactable,suffix+" artwork frame opens enlarged illustration");
        string state=State(),rng=Rng();button.onClick.Invoke();yield return null;
        var overlay=GameObject.Find("Event artwork enlarged");
        Check(overlay!=null,suffix+" enlarged illustration opens");
        Check(overlay.transform.parent==canvas.rootCanvas.transform,suffix+" preview belongs to original root canvas");
        Check(overlay.GetComponent<Image>().raycastTarget&&overlay.GetComponent<GraphicRaycaster>()!=null,suffix+" preview blocks underlying pointer input");
        var raw=overlay.GetComponentInChildren<RawImage>();var texture=Resources.Load<Texture2D>(entry.path);
        Check(raw!=null&&raw.texture==texture&&raw.uvRect==new Rect(0,0,1,1),suffix+" enlarged view preserves whole image");
        var rect=Bounds(raw.rectTransform);
        Check(Inside(new Rect(0,0,Screen.width,Screen.height),rect),suffix+" enlarged picture remains on screen");
        Check(Mathf.Abs((rect.width/rect.height)/((float)texture.width/texture.height)-1)<.02f,suffix+" enlarged picture has no distortion");
        yield return Capture("event-enlarged-"+suffix);
        overlay.GetComponentsInChildren<Button>().Single(b=>b.name=="Close").onClick.Invoke();yield return null;
        Check(GameObject.Find("Event artwork enlarged")==null,suffix+" Close dismisses enlarged art");
        button.onClick.Invoke();yield return null;
        var control=ControlSingleton.Instance;var oldAction=control.ReturnAction;bool navigated=false;
        control.ReturnAction=()=>{navigated=true;};control.GoBack();yield return null;
        bool callbackIntact=control.ReturnAction!=null;control.ReturnAction=oldAction;
        Check(!navigated&&callbackIntact&&GameObject.Find("Event artwork enlarged")==null,suffix+" shared Escape/Back dispatcher closes art without navigating");
        button.onClick.Invoke();yield return null;
        EventCardArt.Refresh(fixture.transform,entry.node,"Refresh closes the previous enlargement.");yield return null;
        Check(GameObject.Find("Event artwork enlarged")==null,suffix+" refreshing artwork closes old preview");
        button.onClick.Invoke();yield return null;fixture.SetActive(false);yield return null;
        Check(GameObject.Find("Event artwork enlarged")==null,suffix+" disabling event closes preview");
        fixture.SetActive(true);yield return null;ValidatePicture(entry,suffix+" re-enabled event");
        Check(State()==state&&Rng()==rng,suffix+" preview interactions preserve campaign and random stream");
    }
    IEnumerator PinnedViewCheck(ArtEntry entry,ArtEntry[] samples,string suffix)
    {
        var pinned=CampaignHUD.Panel(canvas.transform,"Pinned art ownership check",new Vector2(.02f,.02f),new Vector2(.3f,.3f));
        EventCardArt.Refresh(pinned.transform,entry.node,"Pinned view.");yield return null;
        var raw=pinned.GetComponentInChildren<RawImage>();var texture=raw.texture;
        foreach(var e in samples){EventCardArt.Refresh(fixture.transform,e.node,"Cache turnover.");yield return null;}
        Check(texture!=null&&raw.texture==texture,suffix+" cycling ten other images never unloads a second live event view");
        pinned.GetComponentInChildren<Button>().onClick.Invoke();yield return null;Destroy(pinned);yield return null;
        Check(GameObject.Find("Event artwork enlarged")==null,suffix+" destroying owner closes its enlarged art");
    }
    IEnumerator Run()
    {
        Application.runInBackground=true;AudioListener.volume=0;yield return null;
        Check(SaveGames.Automated,"ordinary campaign archives isolated before startup");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+25;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized&&FindObjectOfType<ManagerUI>()!=null,"campaign initialized");yield return new WaitForSecondsRealtime(.5f);
        CampaignHUD.Instance.DismissReport();CouncilUI.For(FindObjectOfType<ManagerUI>()).Hide();
        canvas=GameObject.Find("Campaign Status").GetComponentInParent<Canvas>();
        var manifest=Resources.Load<TextAsset>(ManifestResource);Check(manifest!=null,"event art manifest included in exported player");
        entries=JsonUtility.FromJson<ArtManifest>(manifest.text).entries;
        Check(entries!=null&&entries.Length>0,"event art entries available");
        Check(entries.Select(e=>e.node).Distinct(StringComparer.Ordinal).Count()==entries.Length,"manifest nodes unique");
        foreach(var e in entries){
            var texture=Resources.Load<Texture2D>(e.path);Check(texture!=null,"manifest texture loads: "+e.node+" -> "+e.path);
            Check(!string.IsNullOrWhiteSpace(e.source),"source trace retained: "+e.node);
            textures.Add(e.node+" | "+e.path+" | "+texture.width+"x"+texture.height+" | "+e.fit+" | "+e.source);
        }
        var samples=Representatives();string original=State(),random=Rng();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);
            string suffix=size.x+"x"+size.y;Check(Screen.width==size.x&&Screen.height==size.y,suffix+" display size applied");
            Canvas.ForceUpdateCanvases();Header(suffix);yield return Capture("header-"+suffix);yield return HeaderTooltips(suffix);
            CreateFixture();yield return null;
            foreach(var e in entries){
                EventCardArt.Refresh(fixture.transform,e.node,"Artwork manifest verification.");Canvas.ForceUpdateCanvases();yield return null;
                ValidatePicture(e,suffix+" "+e.node);
            }
            for(int i=0;i<samples.Length;i++){
                var e=samples[i];fixture.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Inspected event node").text=e.node;
                EventCardArt.Refresh(fixture.transform,e.node,"Representative artwork inspection.");yield return null;
                ValidatePicture(e,suffix+" sample "+i);yield return Capture("event-"+(i+1).ToString("00")+"-"+e.node+"-"+suffix);
            }
            yield return PreviewChecks(samples[0],suffix);
            yield return PinnedViewCheck(samples[0],samples,suffix);
            Destroy(fixture);fixture=null;yield return null;
            Check(State()==original&&Rng()==random,suffix+" all art refreshes preserve campaign and random stream");
        }
    }
    IEnumerator FrameRun()
    {
        int width=Screen.width,height=Screen.height;var mode=Screen.fullScreenMode;
        Application.runInBackground=true;AudioListener.volume=0;yield return null;
        Check(SaveGames.Automated,"isolated test saves");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+25;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"campaign ready");yield return new WaitForSecondsRealtime(.5f);
        CampaignHUD.Instance.DismissReport();CouncilUI.For(FindObjectOfType<ManagerUI>()).Hide();
        foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
        canvas=GameObject.Find("Campaign Status").GetComponentInParent<Canvas>();
        entries=JsonUtility.FromJson<ArtManifest>(Resources.Load<TextAsset>(ManifestResource).text).entries;
        string original=State(),random=Rng();
        var sample=entries.First(e=>e.node=="story7");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080),new Vector2Int(1920,1200),new Vector2Int(1280,960),new Vector2Int(2560,1080)})
        {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);
            string suffix=Screen.width+"x"+Screen.height;
            Check(Screen.width==size.x&&Screen.height==size.y,"Resolution applied "+suffix);
            CreateFixture();yield return null;
            foreach(var entry in entries){EventCardArt.Refresh(fixture.transform,entry.node,"Frame regression");yield return null;Canvas.ForceUpdateCanvases();ValidatePicture(entry,suffix+" "+entry.node);}
            // Exercise both fit policies and the generic fallback without a manifest change.
            var frame=fixture.transform.Find("Event artwork");var view=frame.GetComponent<EventArtworkView>();
            foreach(bool contain in new[]{true,false})
            {
                view.Configure(sample.path,sample.path,contain,.75f,.5f);yield return null;Canvas.ForceUpdateCanvases();
                ValidatePicture(new ArtEntry{path=sample.path,fit=contain?"contain":"cover"},suffix+" explicit fit");
            }
            EventCardArt.Refresh(fixture.transform,"unmapped-frame-fixture","soldiers at the wall");yield return null;
            ValidatePicture(new ArtEntry{path="EventArt/military-r11",fit="cover"},suffix+" generic fallback");
            EventCardArt.Refresh(fixture.transform,sample.node,"Normal card");yield return null;
            yield return Capture("event-frame-"+suffix);
            yield return PreviewChecks(sample,suffix);
            // Match the wide shallow presentation in the reported screenshot.
            frame=fixture.transform.Find("Event artwork");var rt=(RectTransform)frame;
            rt.anchorMin=new Vector2(.025f,.35f);rt.anchorMax=new Vector2(.975f,.965f);
            Canvas.ForceUpdateCanvases();yield return null;ValidatePicture(sample,suffix+" resized wide frame");
            yield return Capture("event-wide-frame-"+suffix);
            Destroy(fixture);fixture=null;yield return null;
            Check(State()==original&&Rng()==random,"No state or random changes "+suffix);
        }
        Screen.SetResolution(width,height,mode);yield return new WaitForSecondsRealtime(.3f);
    }
    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.12f);Canvas.ForceUpdateCanvases();
        var obj=new GameObject("Art capture camera",typeof(Camera));var camera=obj.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{
            camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}
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
