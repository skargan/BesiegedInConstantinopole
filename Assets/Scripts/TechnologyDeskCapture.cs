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
public sealed class TechnologyDeskCapture : MonoBehaviour
{
    string output;CouncilUI council;readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-technology-desk-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/TechnologyDesk"));Directory.CreateDirectory(folder);
        SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Technology verification");DontDestroyOnLoad(go);go.AddComponent<TechnologyDeskCapture>().output=folder;}
    readonly List<string> layoutErrors=new List<string>();
    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    GameObject Root()=>GameObject.Find("Council screen");
    Button Button(string name)=>Root().GetComponentsInChildren<Button>().Single(b=>b.name==name);
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    void Layout(string context){Canvas.ForceUpdateCanvases();
        Check(Root().GetComponentsInChildren<ScrollRect>().Length==0,context+" entire catalogue visible without scrolling");
        Check(Root().GetComponentsInChildren<RawImage>().Count(t=>t.texture!=null&&t.name=="Uncropped mosaic")==1,context+" only one technology illustration");
        Check(Root().GetComponentsInChildren<RawImage>().Any(t=>t.texture!=null&&t.texture.name=="technologies"),context+" generated background loaded");
        foreach(var text in Root().GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();if(text.isTextOverflowing)layoutErrors.Add(context+" | "+text.text+" | rect="+text.rectTransform.rect+" font="+text.fontSize+" preferred="+text.preferredHeight);else Check(true,context+" text fits");}
        var buttons=Root().GetComponentsInChildren<Button>();
        foreach(var button in buttons){var r=(RectTransform)button.transform;Check(r.rect.width>60&&r.rect.height>25,context+" usable target "+button.name);}
        for(int i=0;i<buttons.Length;i++)for(int j=i+1;j<buttons.Length;j++){
            var a=new Vector3[4];var b=new Vector3[4];((RectTransform)buttons[i].transform).GetWorldCorners(a);((RectTransform)buttons[j].transform).GetWorldCorners(b);
            Check(!new Rect(a[0],a[2]-a[0]).Overlaps(new Rect(b[0],b[2]-b[0])),context+" controls do not overlap");}
    }
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+results.Count+" checks\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        File.WriteAllLines(Path.Combine(output,"layout-errors.txt"),layoutErrors);
        string report=(layoutErrors.Count>0?"LAYOUT NEEDS REVIEW "+layoutErrors.Count+" errors. ":"")+"TECHNOLOGY_DESK_CHECKS "+results.Count+" passed; catalogue selection, affordability, artwork, navigation, new-campaign split and saved allocation.";
        File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;Check(SaveGames.Automated,"isolated archives");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+20;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());
        Check(MonasticResearch.TotalMonks==100&&MonasticResearch.ResearchMonks==50&&MonasticResearch.InfirmaryMonks==50,"actual new campaign starts fifty-fifty");
        Check(!MonasticResearch.HasPending&&MonasticResearch.Progress==0,"initial allocation immediate without free progress");
        Check(MonasticResearchChecks.Run()>0,"existing allocation/calendar/legacy migration checks pass");
        var baseline=State();
        VariableSingleton.SetInt("iHealers",101);MonasticResearch.BeginCampaign();Check(MonasticResearch.ResearchMonks==50&&MonasticResearch.InfirmaryMonks==51,"odd monk remains in care");
        VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(baseline));
        MonasticResearch.ApplyAllocation(70,out _);MonasticResearch.AllocationsForDay(VariableSingleton.GetDate().AddDays(1));
        string save=Path.Combine(output,"Saves","allocation.json");File.WriteAllText(save,State());
        VariableSingleton.SetInt("iMonasticActive",1);VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(File.ReadAllText(save)));
        Check(MonasticResearch.ResearchMonks==70&&MonasticResearch.InfirmaryMonks==30,"save/load keeps player's later allocation");
        VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(baseline));
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720),new Vector2Int(1920,1200),new Vector2Int(2560,1080)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);
            Check(Screen.width==size.x&&Screen.height==size.y,"requested resolution applied");
            council.Technologies();yield return null;
            for(int i=0;i<TechnologySystem.All.Length;i++){
                string before=State();Button("Select technology "+i).onClick.Invoke();yield return null;
                Check(State()==before,"selecting technology does not spend resources or advance time");Layout(size+" technology "+i);
                var tech=TechnologySystem.All[i];Check(Root().GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains(tech.Effect)),"selected effects match authority");
                Check(Root().GetComponentsInChildren<RawImage>().Any(t=>t.texture==TechnologyArtwork.Load(tech.Key)),"selected art maps to stable key");
            }
            Button("Select technology 0").onClick.Invoke();yield return null;yield return Capture("technology-desk-"+size.x+"x"+size.y);
        }
        Button("Select technology 0").onClick.Invoke();yield return null;
        Root().GetComponentsInChildren<Button>().Single(b=>b.name.StartsWith("Technology artwork ")).onClick.Invoke();yield return null;
        Check(GameObject.Find("Technology artwork enlarged")!=null,"art enlargement opens");Check(TechnologyArtwork.ClosePreview(),"enlargement closes");yield return null;
        VariableSingleton.SetInt("iSecrets",0);council.Technologies();yield return null;Check(!Button("Research selected technology").interactable,"unaffordable research disabled");
        VariableSingleton.SetInt("iSecrets",3);council.Technologies();yield return null;Check(Button("Research selected technology").interactable,"affordable research enabled");
        Button("Technology monasteries").onClick.Invoke();yield return null;Check(GameObject.Find("Infirmary desk")!=null,"monastery link opens infirmary");
        Button("Medical return").onClick.Invoke();yield return null;Check(GameObject.Find("Technology desk")!=null,"monastery return restores technology desk");
        Button("Research selected technology").onClick.Invoke();yield return null;
        Check(TechnologySystem.Has("GreekFire")&&CouncilActions.Int("iSecrets")==0,"actual research button unlocks exactly once at the displayed price");
        Check(!TechnologySystem.Unlock(0,out _)&&CouncilActions.Int("iSecrets")==0,"repeat unlock spends nothing");
        council.Technologies();yield return null;Check(!Button("Research selected technology").interactable,"active technology cannot be purchased twice");
        Button("Technology return").onClick.Invoke();yield return null;Check(GameObject.Find("Technology desk")==null,"Return leaves technology desk");
        File.WriteAllLines(Path.Combine(output,"layout-errors.txt"),layoutErrors);
        Check(layoutErrors.Count==0,"all catalogue and description text fits at every checked resolution");
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
