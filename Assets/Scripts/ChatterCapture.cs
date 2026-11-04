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

public sealed class ChatterCapture:MonoBehaviour
{
    string output;int passed;CouncilUI council;ManagerUI manager;
    VariableSingleton.CampaignSave baseline;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-chatter-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Chatter"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");Directory.CreateDirectory(SaveGames.TestRoot);
        var go=new GameObject("Ionian checks");DontDestroyOnLoad(go);go.AddComponent<ChatterCapture>().output=path;
    }
    void Check(bool ok,string message){if(!ok)throw new Exception(message);passed++;checks.Add("PASS "+message);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    GameObject Root=>GameObject.Find("Council screen");
    Button Button(string text)=>Root.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==text));
    void Page(string page)=>typeof(CouncilUI).GetMethod("IonianPage",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(council,new object[]{page});
    void Restore(){council.Hide();VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline)));CampaignHUD.Instance.DismissReport();}
    void Resident(){VariableSingleton.SetInt("iDiplomacyEnvoy_"+IonianDiplomacy.Country,2);}
    IEnumerator Start(){var stack=new Stack<IEnumerator>();stack.Push(Run());while(stack.Count>0){object next=null;bool moved=false;Exception error=null;
        try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){error=e;}
        if(error!=null){File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+passed+"\n"+error);File.WriteAllLines(Path.Combine(output,"details.txt"),checks);Application.Quit(1);yield break;}
        if(!moved){stack.Pop();continue;}if(next is IEnumerator routine)stack.Push(routine);else yield return next;
    }File.WriteAllText(Path.Combine(output,"checks.txt"),"CHATTER_CHECKS "+passed+" passed");File.WriteAllLines(Path.Combine(output,"details.txt"),checks);Application.Quit(0);}
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
    void Layout(string page){Canvas.ForceUpdateCanvases();var panel=GameObject.Find("Ionian diplomatic panel").GetComponent<RectTransform>();
        foreach(var text in panel.GetComponentsInChildren<TMP_Text>()){
            if(text.GetComponentInParent<ScrollRect>()!=null)continue;
            text.ForceMeshUpdate();Check(!text.isTextOverflowing,page+" text fits: "+text.text.Replace("\n"," "));
        }
        foreach(var button in panel.GetComponentsInChildren<Button>()){
            var p=button.GetComponent<RectTransform>();var corners=new Vector3[4];p.GetWorldCorners(corners);
            Check(corners.All(v=>v.x>=0&&v.y>=0&&v.x<=Screen.width+1&&v.y<=Screen.height+1),page+" button inside viewport");
        }
    }

    void ResetChatter(){CharacterChatter.ResetForRestore();}
    VariableSingleton.CampaignSave Copy()=>JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
    void DayForward(){var next=Copy();next.turn++;VariableSingleton.Instance.RestoreValidated(next);}
    void WaitExpired(string id){VariableSingleton.SetInt("iChatter_"+id+"_UntilSeconds",0);VariableSingleton.SetInt("iChatter_"+id+"_UntilMillis",0);}
    IEnumerator Run()
    {
        Application.runInBackground=true;AudioListener.volume=0;yield return null;SaveMenu.StartCampaign(false);
        float until=Time.realtimeSinceStartup+30;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<until)yield return null;
        Check(VariableSingleton.IsInitialized,"Campaign initialized");yield return new WaitForSecondsRealtime(.5f);
        manager=FindObjectOfType<ManagerUI>();council=CouncilUI.For(manager);baseline=Copy();
        var lines=IonianText.Lines;string who=CharacterChatter.Raimondina,bye=CharacterChatter.RaimondinaFarewell;
        Check(lines.Length==30&&IonianText.Data.remarks.All(r=>!r.unmarriedOnly&&!r.beforeShelter),"Thirty approved unrestricted lines");
        Check(lines[14].Contains("What I actually did, was I kept"),"User's edited line fifteen preserved verbatim");
        Check(IonianText.Data.realm.Contains("Madalena Tocco")&&!IonianText.Data.realm.Contains("Theodora"),"Family history uses approved wife name");
        string rng=Rng();ResetChatter();
        Check(CharacterChatter.Current(who,lines,bye)==""&&CharacterChatter.Count(who)==0,"No remark consumed by inspecting state");
        string first=CharacterChatter.Request(who,lines,bye,100);
        Check(lines.Contains(first)&&CharacterChatter.Count(who)==1,"First request yields one normal remark");
        Check(CharacterChatter.Spent(who,102.999),"Spent before three seconds");
        Check(!CharacterChatter.Spent(who,103),"Available at three seconds");
        for(int i=0;i<20;i++)Check(CharacterChatter.Request(who,lines,bye,102)==first,"Rapid request retains current text "+i);
        var snapshot=Copy();ResetChatter();VariableSingleton.Instance.RestoreValidated(snapshot);
        Check(CharacterChatter.Count(who)==1&&CharacterChatter.Request(who,lines,bye,102)==first,"Save/load retains count, current quote and remaining cooldown");
        string second=CharacterChatter.Request(who,lines,bye,103),third=CharacterChatter.Request(who,lines,bye,106);
        Check(new[]{first,second,third}.Distinct().Count()==3,"Three distinct normal remarks");
        Check(CharacterChatter.Request(who,lines,bye,108)==third,"Third quote is not immediately overwritten by farewell");
        Check(CharacterChatter.Spent(who,110)&&CharacterChatter.Request(who,lines,bye,110)==bye,"Next fresh request after three gives farewell");
        Check(CharacterChatter.Request(who,lines,bye,1000)==bye&&CharacterChatter.Count(who)==3,"Further requests keep farewell and daily limit");
        snapshot=Copy();VariableSingleton.Instance.RestoreValidated(snapshot);
        Check(CharacterChatter.Request(who,lines,bye,1001)==bye,"Saved farewell persists on reload");
        CharacterChatter.Request(CharacterChatter.GrainMerchant,new[]{"One","Two","Three"},CharacterChatter.MerchantFarewell,100);
        Check(CharacterChatter.Count(CharacterChatter.GrainMerchant)==1&&CharacterChatter.Count(who)==3,"Different characters have separate allowances");
        var siege=Copy();siege.turn=VariableSingleton.EffectiveSiegeTurn;VariableSingleton.Instance.RestoreValidated(siege);
        Check(CharacterChatter.Count(who)==0,"Calendar change resets daily allowance");
        ResetChatter();var seen=new HashSet<string>();string previous="";double clock=2000;
        for(int day=0;day<10;day++){
            for(int i=0;i<3;i++){var text=CharacterChatter.Request(who,lines,bye,clock);clock+=4;Check(seen.Add(text),"Unused remarks precede repeats "+day+"/"+i);previous=text;}
            var saved=Copy();VariableSingleton.Instance.RestoreValidated(saved);Check(CharacterChatter.Count(who)==3,"Daily count survives reload "+day);DayForward();
        }
        Check(seen.Count==30,"All thirty heard before deck repeats");
        Check(CharacterChatter.Request(who,lines,bye,clock)!=previous,"New deck does not immediately repeat last line");
        Check(Rng()==rng,"Chatter and restores preserve campaign RNG");Restore();
        Check(CharacterChatter.Count(who)==0&&CharacterChatter.Current(who,lines,bye)=="","Earlier save with no chatter does not inherit later state");

        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);Restore();Resident();Page("visit");yield return null;
            Check(CharacterChatter.Count(who)==0,"Opening visit spends no remark");
            var spot=GameObject.Find("Hotspot · Raimondina Ventimiglia").GetComponent<LocationHotspot>();
            var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            UnityEngine.EventSystems.ExecuteEvents.Execute(spot.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);yield return null;
            Check(CharacterChatter.Count(who)==1&&spot.Spent,"Real hover speaks once and sets blue outline");
            Check(File.Exists(VariableSingleton.SavePath),"Hover creates current autosave");
            var archive=JsonUtility.FromJson<SaveGames.Archive>(File.ReadAllText(VariableSingleton.SavePath));
            Check(archive.campaign.ints.Any(p=>p.key=="iChatter_raimondina_Count"&&p.value==1),"Autosave includes hover count");
            yield return Capture("raimondina-cooldown-"+size.x);
            yield return new WaitForSecondsRealtime(3.1f);yield return null;
            Check(!spot.Spent&&CharacterChatter.Count(who)==1,"Held hover becomes gold without advancing dialogue");
            Button("Speak with Raimondina").onClick.Invoke();Check(CharacterChatter.Count(who)==2,"Click after cooldown advances once");
            Page("about");yield return null;Check(CharacterChatter.Count(who)==2,"Full conversation remains open without consuming small talk");
            Page("visit");yield return null;Check(CharacterChatter.Count(who)==2,"Page reopen retains allowance");
            WaitExpired(who);Button("Speak with Raimondina").onClick.Invoke();WaitExpired(who);Button("Speak with Raimondina").onClick.Invoke();
            Check(CharacterChatter.Current(who,lines,bye)==bye,"Actual speak button reaches farewell");
            Check(Button("Secure shelter for refugees").interactable&&Button("Tell me about yourself").interactable,"Spent chatter leaves diplomacy and full conversation enabled");
            var speech=GameObject.Find("Raimondina remark").GetComponentsInChildren<TMP_Text>().First(t=>t.text!= "Speak with Raimondina");
            foreach(var text in lines.Concat(new[]{bye})){speech.text="“"+text+"”";Canvas.ForceUpdateCanvases();speech.ForceMeshUpdate();Check(!speech.isTextOverflowing,"Approved remark fits "+size+": "+Array.IndexOf(lines,text));}
            speech.text="“"+lines.OrderByDescending(t=>t.Length).First()+"”";yield return Capture("raimondina-longest-"+size.x);
            foreach(bool war in new[]{false,true}){
                Restore();var state=Copy();state.turn=war?VariableSingleton.EffectiveSiegeTurn:state.turn;VariableSingleton.Instance.RestoreValidated(state);council.Market();yield return null;
                var merchant=Root.GetComponentInChildren<MarketMerchant>();Check(merchant!=null,"Existing grain merchant attached");
                string marketId=CharacterChatter.GrainMerchant;merchant.OnPointerEnter(pointer);string original=merchant.CurrentRemark;
                merchant.OnPointerExit(pointer);merchant.OnPointerEnter(pointer);
                Check(merchant.CurrentRemark==original&&CharacterChatter.Count(marketId)==1,"Merchant rapid rehover cannot cycle");
                yield return new WaitForSecondsRealtime(3.1f);yield return null;
                Check(CharacterChatter.Count(marketId)==1&&!merchant.GetComponent<LocationHotspot>().Spent,"Merchant held hover does not auto-advance");
                merchant.Speak();WaitExpired(marketId);merchant.Speak();WaitExpired(marketId);merchant.Speak();yield return null;
                Check(merchant.CurrentRemark==CharacterChatter.MerchantFarewell&&CharacterChatter.Count(marketId)==3,"Merchant farewell after three quotes");
                Check(merchant.GetComponent<LocationHotspot>().Spent,"Merchant daily exhaustion blue");
                Check(Root.GetComponentsInChildren<Button>().Any(b=>b.name=="Market buy iMaterials"&&b.interactable),"Trading remains available after farewell");
                foreach(var label in GameObject.Find("Merchant speech").GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();Check(!label.isTextOverflowing,"Merchant farewell fits");}
                yield return Capture("merchant-farewell-"+(war?"war":"peace")+"-"+size.x);
                council.Market();yield return null;merchant=Root.GetComponentInChildren<MarketMerchant>();merchant.Speak();Check(merchant.CurrentRemark==CharacterChatter.MerchantFarewell,"Merchant page reopen cannot reset limit");
            }
        }
        Restore();
    }
}
#endif
