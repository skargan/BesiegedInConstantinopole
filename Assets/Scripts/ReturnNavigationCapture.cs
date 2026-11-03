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
public sealed class ReturnNavigationCapture : MonoBehaviour
{
    string output; CouncilUI council; readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-return-navigation-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/ReturnNavigation"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Merchant verification");DontDestroyOnLoad(go);go.AddComponent<ReturnNavigationCapture>().output=folder;}
    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    GameObject Root()=>GameObject.Find("Council screen");
    Button Button(string name)=>Root().GetComponentsInChildren<Button>().Single(b=>b.name==name);
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        var report="RETURN_NAVIGATION_CHECKS "+results.Count+" passed; council menu sample, nested Society and provincial operations at two resolutions.";File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    IEnumerator Run(){
        AudioListener.volume=0;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(1);
        CampaignHUD.Instance.DismissReport();var manager=FindObjectOfType<ManagerUI>(true);council=CouncilUI.For(manager);
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
            foreach(System.Action open in new System.Action[]{council.CivicRelations,council.Overview,council.Market,council.Spies,council.Diplomacy,council.Technologies}){
                manager.gameObject.SetActive(true);open();yield return null;
                var root=GameObject.Find("Council screen");var back=root.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Return"));
                back.onClick.Invoke();yield return null;Check(manager.gameObject.activeInHierarchy,"Return restores council");Check(!root.activeSelf,"Return closes child");
            }
            council.CivicRelations();council.ReviewFaction(0);yield return null;ControlSingleton.Instance.GoBack();yield return null;
            Check(GameObject.Find("Council screen").GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Society and politics"),"Audience returns to Society");ControlSingleton.Instance.GoBack();yield return null;Check(manager.gameObject.activeInHierarchy,"Nested Society returns to council");
            council.ProvincialOperationsFor("macedonia");yield return null;var operations=GameObject.Find("Council screen");
            operations.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Return")).onClick.Invoke();yield return null;
            Check(!operations.activeSelf&&ProvincialConflictUI.Open,"Province Return closes operations and opens command");
            Check(ProvincialConflictUI.Back(),"Province command returns to map");yield return null;Check(!ProvincialConflictUI.Open,"Province command closed");WarTheatreUI.Close();
        }
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
