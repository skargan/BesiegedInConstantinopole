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

// Opt-in visual/interaction verification. No diplomacy order is committed.
public sealed class DiplomaticArtworkCapture : MonoBehaviour
{
    string output;CouncilUI council;readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-diplomatic-art-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/DiplomaticArt"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Diplomatic art verification");DontDestroyOnLoad(go);go.AddComponent<DiplomaticArtworkCapture>().output=folder;}
    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    string State(){var state=VariableSingleton.Instance.CaptureState();state.randomState=default(UnityEngine.Random.State);return JsonUtility.ToJson(state);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    GameObject ScreenRoot()=>GameObject.Find("Council screen");
    Rect Bounds(RectTransform rect){var c=new Vector3[4];rect.GetWorldCorners(c);var p=c.Select(v=>RectTransformUtility.WorldToScreenPoint(null,v)).ToArray();return Rect.MinMaxRect(p.Min(v=>v.x),p.Min(v=>v.y),p.Max(v=>v.x),p.Max(v=>v.y));}
    bool Inside(Rect a,Rect b)=>b.xMin>=a.xMin-1&&b.xMax<=a.xMax+1&&b.yMin>=a.yMin-1&&b.yMax<=a.yMax+1;
    bool Overlap(Rect a,Rect b)=>Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin)>1&&Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin)>1;
    List<string> ArtAndText(Transform panel,string realm,string attitude)
    {
        var errors=new List<string>();var views=panel.GetComponentsInChildren<DiplomaticArtworkView>();
        string[] expected={"Flags/"+DiplomaticArtwork.CourtId(realm),"Attitudes/"+attitude.ToLowerInvariant()};
        foreach(string key in expected){var view=views.FirstOrDefault(v=>v.Key==key);var image=view==null?null:view.GetComponent<Image>();
            if(image==null||image.sprite==null||image.sprite.name!="DiplomaticArtwork/"+key||image.raycastTarget||!image.preserveAspect)errors.Add("invalid art "+key);
            else if(!Inside(Bounds((RectTransform)panel),Bounds(image.rectTransform)))errors.Add("art outside panel "+key);
        }
        if(views.Length!=2)errors.Add("expected exactly flag and attitude emblem");
        var texts=panel.GetComponentsInChildren<TMP_Text>();foreach(var text in texts){text.ForceMeshUpdate();if(text.isTextOverflowing)errors.Add("text overflow "+text.name+": "+text.text);
            foreach(var view in views)if(Overlap(Bounds(text.rectTransform),Bounds((RectTransform)view.transform)))errors.Add("art overlaps "+text.name);
        }
        for(int i=0;i<texts.Length;i++)for(int j=i+1;j<texts.Length;j++)if(Overlap(Bounds(texts[i].rectTransform),Bounds(texts[j].rectTransform)))errors.Add("text regions overlap: "+texts[i].name+" / "+texts[j].name);
        if(!texts.Any(t=>t.text.Contains(attitude)&&t.text.Contains("% acceptance")))errors.Add("readable attitude/acceptance missing");
        return errors;
    }
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        string report="DIPLOMATIC_ART_CHECKS "+results.Count+" passed; all courts and five attitudes at two resolutions. No diplomatic actions committed.";File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;Check(SaveGames.Automated,"isolated archives");SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+20;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;Check(VariableSingleton.IsInitialized,"campaign initialized");yield return new WaitForSecondsRealtime(.5f);
        CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());CountryDiplomacy.Ensure();
        int count=CountryDiplomacy.Definitions.Length;Check(count>=31,"at least31 requested courts present");Check(DiplomaticArtwork.CourtId("ionian_lordships")=="ionian_tocco","Ionian alias resolves to stable art ID");
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);string context=size.x+"x"+size.y;Check(Screen.width==size.x&&Screen.height==size.y,context+" display applied");
            var seen=new HashSet<string>();int pages=(count+7)/8;
            for(int page=0;page<pages;page++){
                typeof(CouncilUI).GetField("diplomacyPage",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(council,page);
                string before=State(),rng=Rng();council.DiplomaticRelations();yield return new WaitForSecondsRealtime(.25f);Canvas.ForceUpdateCanvases();
                var cards=ScreenRoot().GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Diplomatic court ")).ToArray();var issues=new List<string>();
                if(cards.Length!=Math.Min(8,count-page*8))issues.Add("card count changed");
                for(int n=0;n<cards.Length;n++){int index=page*8+n;var d=CountryDiplomacy.Definitions[index];seen.Add(d.Id);issues.AddRange(ArtAndText(cards[n].transform,d.Id,CountryDiplomacy.Attitude(index).ToString()));
                    if(!cards[n].GetComponentsInChildren<TMP_Text>().Any(t=>t.text==d.Name))issues.Add("court name missing "+d.Id);
                }
                if(ScreenRoot().GetComponentsInChildren<DiplomaticMaskBadge>().Length>0)issues.Add("legacy mask still drawn");
                Check(issues.Count==0,context+" list page"+(page+1)+": "+string.Join(" | ",issues));Check(before==State()&&rng==Rng(),context+" list inspection preserves state/RNG");
                Check(DiplomaticArtwork.CachedCount<=16,context+" cache bounded after page transition");
                if(page==0||page==pages-1)yield return Capture("courts-"+(page+1)+"-"+context);
            }
            Check(seen.Count==count,context+" every court has a flag on its list card");
            for(int index=0;index<count;index++){
                string before=State(),rng=Rng();council.DiplomaticPower(index);yield return null;Canvas.ForceUpdateCanvases();var d=CountryDiplomacy.Definitions[index];var header=GameObject.Find("Relationship");
                var issues=ArtAndText(header.transform,d.Id,CountryDiplomacy.Attitude(index).ToString());
                foreach(var view in header.GetComponentsInChildren<DiplomaticArtworkView>())foreach(var button in ScreenRoot().GetComponentsInChildren<Button>())if(Overlap(Bounds((RectTransform)view.transform),Bounds((RectTransform)button.transform)))issues.Add("art overlaps action "+button.name);
                Check(issues.Count==0,context+" detail "+d.Id+": "+string.Join(" | ",issues));if(before!=State()||rng!=Rng()){File.WriteAllText(Path.Combine(output,"before.json"),before);File.WriteAllText(Path.Combine(output,"after.json"),State());File.WriteAllText(Path.Combine(output,"rng-diff.txt"),rng+"\n"+Rng());}Check(before==State()&&rng==Rng(),context+" detail inspection preserves state/RNG "+d.Id);
            }
            for(int level=0;level<5;level++){
                CountryDiplomacy.SetAttitude(0,(CountryDiplomacy.AttitudeLevel)level);string before=State(),rng=Rng();council.DiplomaticPower(0);yield return new WaitForSecondsRealtime(.25f);
                var issues=ArtAndText(GameObject.Find("Relationship").transform,CountryDiplomacy.Definitions[0].Id,((CountryDiplomacy.AttitudeLevel)level).ToString());Check(issues.Count==0,context+" attitude "+level+": "+string.Join(" | ",issues));Check(before==State()&&rng==Rng(),context+" attitude artwork has no effects");
                if(level==0||level==4)yield return Capture("attitude-"+level+"-"+context);
            }
        }
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
