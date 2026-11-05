#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class CouncilChamberCapture : MonoBehaviour
{
    string output; CouncilUI council; readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-chamber-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Chamber"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Merchant verification");DontDestroyOnLoad(go);go.AddComponent<CouncilChamberCapture>().output=folder;}
    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    GameObject Root()=>GameObject.Find("Council screen");
    Button Button(string name)=>Root().GetComponentsInChildren<Button>().Single(b=>b.name==name);
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        var report="CHAMBER_CHECKS "+results.Count+" passed; chamber scaling, lifecycle, still mode, fallback, quotation, campaign isolation, journal persistence and menu navigation.";File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    IEnumerator Run(){
        AudioListener.volume=0;Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(1);
        CampaignHUD.Instance.DismissReport();var manager=FindObjectOfType<ManagerUI>(true);manager.gameObject.SetActive(true);council=CouncilUI.For(manager);
        var chamber=manager.GetComponentInChildren<CouncilChamber>();Check(chamber!=null,"chamber replaces portrait");Check(chamber.EffectsAvailable,"dedicated painting loaded");
        string before=State(),rng=JsonUtility.ToJson(UnityEngine.Random.state);var quote=manager.GetComponentInChildren<CouncilQuoteView>();string quoteId=quote.QuoteId;
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720),new Vector2Int(1280,960)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();
            var painting=(RectTransform)chamber.transform;Check(Mathf.Abs(painting.rect.width-painting.rect.height)<1,"painting keeps square proportions");
            foreach(var g in chamber.transform.parent.GetComponentsInChildren<Graphic>())Check(!g.raycastTarget,"decoration cannot intercept clicks");
            for(int i=0;i<2;i++){
                var flame=(RectTransform)chamber.transform.Find("Candle effects/Anchored flame "+i);var point=i==0?chamber.settings.left:chamber.settings.right;
                Check(Vector2.Distance(flame.anchorMin,point)<.0001f&&flame.anchoredPosition==Vector2.zero&&flame.pivot==new Vector2(.5f,0),"flame anchored to reference wick");
            }
            var menuButton=manager.GetComponentsInChildren<Button>().First(b=>b.GetComponent<ImperialButtonLightDriver>()!=null&&b.interactable);
            var edge=menuButton.GetComponentInChildren<ImperialButtonLight>();Check(edge!=null&&!edge.raycastTarget,"shared edge highlight cannot intercept clicks");
            menuButton.GetComponent<ImperialButtonLightDriver>().OnPointerEnter(null);yield return new WaitForSecondsRealtime(.2f);
            yield return Capture("chamber-"+size.x+"x"+size.y);
            menuButton.GetComponent<ImperialButtonLightDriver>().OnPointerExit(null);
            var haze=(RectTransform)chamber.transform.Find("Candle effects/Window haze 0");var hazeBefore=haze.anchorMin;
            yield return new WaitForSecondsRealtime(.5f);Check(chamber.IsStill||haze.anchorMin!=hazeBefore,"window haze moves when animation enabled");
            yield return Capture("chamber-motion-"+size.x+"x"+size.y);
            float elapsed=chamber.Elapsed;manager.gameObject.SetActive(false);yield return new WaitForSecondsRealtime(.25f);Check(chamber.Elapsed==elapsed,"hidden chamber stops animation");
            manager.gameObject.SetActive(true);yield return null;Check(chamber.Elapsed-elapsed<=.051f,"reopen has no time jump");Check(manager.GetComponentsInChildren<CouncilChamber>(true).Length==1,"no duplicate animation component");
            chamber.ForceStill=true;yield return null;elapsed=chamber.Elapsed;yield return new WaitForSecondsRealtime(.15f);Check(chamber.Elapsed==elapsed,"reduced-motion frame stays still");yield return Capture("chamber-still-"+size.x);chamber.ForceStill=false;
        }
        var ship=chamber.GetComponentsInChildren<RawImage>(true).Single(i=>i.name=="Passing merchantman");Check(ship.texture!=null,"passing ship artwork loaded");
        var elapsedField=typeof(CouncilChamber).GetField("<Elapsed>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic);
        elapsedField.SetValue(chamber,18f);chamber.SendMessage("Render");yield return Capture("chamber-gust-and-ship");
        var shipPosition=ship.rectTransform.anchorMin;yield return new WaitForSecondsRealtime(.5f);Check(chamber.IsStill||ship.rectTransform.anchorMin!=shipPosition,"ship advances across water");
        Check(quote.QuoteId==quoteId,"quotation preserved through resize and reopen");Check(State()==before&&JsonUtility.ToJson(UnityEngine.Random.state)==rng,"decoration preserves campaign state and RNG");
        chamber.EffectsAvailable=false;chamber.SendMessage("Render");yield return Capture("chamber-static-fallback");Check(chamber.GetComponentsInChildren<RawImage>().Where(i=>i.name.StartsWith("Anchored flame")).All(i=>!i.enabled),"effect failure leaves static painting");chamber.EffectsAvailable=true;
        council.CivicRelations();yield return null;ControlSingleton.Instance.GoBack();yield return null;Check(manager.gameObject.activeInHierarchy&&manager.GetComponentsInChildren<CouncilChamber>().Length==1,"menu Return restores same chamber");
        yield return JournalChecks(manager);
        float total=0,max=0;int frames=240;for(int i=0;i<frames;i++){yield return null;total+=Time.unscaledDeltaTime;max=Mathf.Max(max,Time.unscaledDeltaTime);}results.Add("OBSERVED frame milliseconds mean="+(total/frames*1000).ToString("F2")+" max="+(max*1000).ToString("F2")+" in exported capture process; not a controlled benchmark.");
    }
    GameObject JournalRoot()=>FindObjectsOfType<Canvas>().FirstOrDefault(c=>c.name=="Decision Journal")?.gameObject;
    IEnumerator JournalChecks(ManagerUI manager){
        var original=VariableSingleton.Instance.CaptureState();string notice=VariableSingleton.ArrivalNotice;
        DecisionJournal.Entries.Clear();VariableSingleton.ArrivalNotice="A convoy reaches the harbour with provisions.";
        manager.gameObject.SetActive(false);manager.gameObject.SetActive(true);yield return null;
        var checkpoint=SaveGames.Read(VariableSingleton.SavePath);
        Check(checkpoint.campaign.journal.Count==1&&checkpoint.campaign.journal[0].text=="A convoy reaches the harbour with provisions.","daily report enters its first council checkpoint");
        Check(VariableSingleton.ArrivalNotice=="","displayed council notice is cleared");CampaignHUD.Instance.DismissReport();
        DecisionJournal.Record("release-decision","Harbour watch","The watch has been doubled.",true);
        DecisionJournal.Record("release-decision","Duplicate","This must not replace the first entry.",true);
        Check(DecisionJournal.Entries.Count==2&&DecisionJournal.Entries.Last().text=="The watch has been doubled.","journal IDs prevent duplicate or overwritten consequences");
        Check(SaveGames.Checkpoint(),"journal archive written through normal save system");
        DecisionJournal.Entries.Clear();string error;
        Check(SaveGames.Load(VariableSingleton.SavePath,out error),"journal archive loads: "+error);
        Check(DecisionJournal.Entries.Count==2&&DecisionJournal.Entries.Last().decision,"daily and decision entries persist across save/load");
        Check(manager.GetComponentsInChildren<Button>().Any(b=>b.name=="Decision Journal"),"journal occupies council menu");
        Check(!manager.GetComponentsInChildren<Button>().Any(b=>b.name=="Development Options"),"development options no longer occupy council");
        manager.GetComponentsInChildren<Button>().Single(b=>b.name=="Decision Journal").onClick.Invoke();yield return null;
        var journal=JournalRoot();
        Check(journal!=null&&journal.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("convoy reaches")),"daily notes tab displays public history");
        yield return Capture("journal-daily-notes");
        journal.GetComponentsInChildren<Button>().Single(b=>b.name=="Decisions and affairs").onClick.Invoke();yield return null;
        journal=JournalRoot();
        Check(journal.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("THE GOLDEN HORN")&&t.text.Contains("Harbour watch")),"decisions tab shows chain status and recorded decisions");
        yield return Capture("journal-decisions");
        journal.GetComponentsInChildren<Button>().Single(b=>b.name=="Return").onClick.Invoke();yield return null;
        Check(JournalRoot()==null&&manager.gameObject.activeInHierarchy,"journal Return preserves council");
        DecisionJournal.Show();yield return null;ControlSingleton.Instance.GoBack();yield return null;
        Check(JournalRoot()==null&&manager.gameObject.activeInHierarchy,"journal back navigation closes only journal");
        CampaignHUD.Instance.TogglePause();yield return null;
        var modal=GameObject.Find("Campaign Modal");
        Check(modal!=null&&modal.GetComponentsInChildren<Button>().Any(b=>b.name=="Development Options"),"campaign menu contains development options");
        modal.GetComponentsInChildren<Button>().Single(b=>b.name=="Development Options").onClick.Invoke();yield return null;
        Check(AssaultPractice.Active&&GameObject.Find("Campaign Modal")==null,"campaign development button opens practice menu");
        Check(!FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("Albanian war test")),"standalone Albanian test absent from practice menu");
        ControlSingleton.Instance.GoBack();yield return null;
        Check(!AssaultPractice.Active&&manager.gameObject.activeInHierarchy,"development Return restores campaign council");
        // Build an actual pre-journal archive, with the checksum calculated over its original schema.
        DecisionJournal.Entries.Clear();var old=SaveGames.Snapshot("Pre-journal compatibility fixture");old.payloadHash=null;
        string legacy=JsonUtility.ToJson(old).Replace(",\"journal\":[]","").Replace(",\"journal\":null","");
        Check(!legacy.Contains("\"journal\":"),"legacy fixture genuinely omits journal property");
        string legacyHash;using(var sha=System.Security.Cryptography.SHA256.Create())legacyHash=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(legacy))).Replace("-","");
        legacy=legacy.Replace("\"payloadHash\":\"\"","\"payloadHash\":\""+legacyHash+"\"").Replace("\"payloadHash\":null","\"payloadHash\":\""+legacyHash+"\"");
        string legacyPath=Path.Combine(SaveGames.TestRoot,"pre-journal.besieged.json");File.WriteAllText(legacyPath,legacy);
        DecisionJournal.Record("temporary","Must disappear","Previous in-memory history.");
        Check(SaveGames.Load(legacyPath,out error),"old signed archive without journal loads: "+error);
        Check(DecisionJournal.Entries.Count==0,"old archive starts with empty journal rather than stale history");
        string tamperedPath=Path.Combine(SaveGames.TestRoot,"tampered-pre-journal.besieged.json");File.WriteAllText(tamperedPath,legacy.Replace("Pre-journal compatibility fixture","Tampered fixture"));
        bool rejected=false;try{SaveGames.Read(tamperedPath);}catch(InvalidDataException){rejected=true;}
        Check(rejected,"pre-journal compatibility still rejects changed archive contents");
        VariableSingleton.Instance.RestoreValidated(original);VariableSingleton.ArrivalNotice=notice;Check(SaveGames.Checkpoint(),"test restores campaign checkpoint after journal checks");
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
