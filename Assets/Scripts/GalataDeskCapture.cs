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
public sealed class GalataDeskCapture : MonoBehaviour
{
    string output; CouncilUI council; readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-galata-desk-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/GalataDesk"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Galata verification");DontDestroyOnLoad(go);go.AddComponent<GalataDeskCapture>().output=folder;}
    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    GameObject Root()=>GameObject.Find("Council screen");
    Button Button(string name)=>Root().GetComponentsInChildren<Button>().Single(b=>b.name==name);
    void Layout(string context){Canvas.ForceUpdateCanvases();var panel=GameObject.Find("Galata desk").GetComponent<RectTransform>();Check(panel.anchorMin.x>=.5f,context+" majority of background clear");
        var art=Root().GetComponentInChildren<RawImage>();Check(art!=null&&art.texture!=null&&art.texture.name=="galata",context+" Galata image loaded");
        foreach(var text in panel.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,context+" text fits "+text.name+" "+text.text);}
        foreach(var button in panel.GetComponentsInChildren<Button>()){var r=(RectTransform)button.transform;Check(r.rect.width>60&&r.rect.height>25,context+" usable button "+button.name);}
    }
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        var report="GALATA_DESK_CHECKS "+results.Count+" passed; all ten choices, all four control states, review/cancel and two resolutions.";File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;Check(SaveGames.Automated,"isolated archives");SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+20;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);Check(Screen.width==size.x&&Screen.height==size.y,"resolution applied");
            for(int state=0;state<4;state++){
                PreparationCampaign.SetGalataState(state);VariableSingleton.SetInt("iGalataClaims",state==0?0:2);VariableSingleton.SetInt("iMoney",state==1?0:100000);VariableSingleton.ChangeBool("bCharWarrior0",state==2);
                for(int tab=0;tab<3;tab++){
                    typeof(CouncilUI).GetField("galataTab",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(council,tab);
                    string before=State();council.Galata();yield return null;Layout(size+" state"+state+" tab"+tab);Check(before==State(),"inspection preserves state and RNG");
                    var buttons=Root().GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Galata review ")).Select(b=>int.Parse(b.name.Substring(14))).ToArray();
                    Check(buttons.Length==(tab==2?2:4),"all tab choices visible");bool locked=false;foreach(int action in buttons){bool unavailable=PreparationCampaign.GalataActionUnavailable(action)!=null;Check(!locked||unavailable,"unavailable choices follow available");locked|=unavailable;}
                    if(state==0&&tab==0)yield return Capture("galata-relations-"+size.x+"x"+size.y);
                    foreach(int action in buttons){before=State();Button("Galata review "+action).onClick.Invoke();yield return null;Layout("review"+action);Check(before==State(),"review never commits");Check(Button("Galata confirm "+action).interactable==(PreparationCampaign.GalataActionUnavailable(action)==null),"confirmation respects existing eligibility");
                        if(state==0&&action==8)yield return Capture("galata-review-"+size.x+"x"+size.y);
                        Button("Galata return").onClick.Invoke();yield return null;Check(before==State(),"cancel preserves state and RNG");}
                }
            }
        }
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
