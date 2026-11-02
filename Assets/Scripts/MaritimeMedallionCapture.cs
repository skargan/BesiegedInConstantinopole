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

public sealed class MaritimeMedallionCapture : MonoBehaviour
{
    string output;int passed;int oldWidth,oldHeight;FullScreenMode oldMode;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-maritime-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Maritime"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");
        Directory.CreateDirectory(SaveGames.TestRoot);
        var go=new GameObject("Maritime verification");DontDestroyOnLoad(go);go.AddComponent<MaritimeMedallionCapture>().output=path;
    }
    void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add("PASS "+name);passed++;}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    void Capture(string filename)
    {
        Canvas.ForceUpdateCanvases();var go=new GameObject("Maritime capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,filename),shot.EncodeToPNG());}
        finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);}
    }
    MaritimeMapMedallions Badge(bool passage)=>FindObjectsOfType<MaritimeMapMedallions>().Single(x=>x.Passage==passage);
    IEnumerator Start()
    {
        oldWidth=Screen.width;oldHeight=Screen.height;oldMode=Screen.fullScreenMode;
        var stack=new Stack<IEnumerator>();stack.Push(Run());Exception failure=null;
        while(stack.Count>0)
        {
            object next=null;bool moved=false;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null)break;
            if(!moved){stack.Pop();continue;}if(next is IEnumerator child)stack.Push(child);else yield return next;
        }
        // Do not leave a QA-only resolution behind for the next normal launch.
        Screen.SetResolution(oldWidth,oldHeight,oldMode);yield return new WaitForSecondsRealtime(.4f);
        File.WriteAllLines(Path.Combine(output,"details.txt"),checks);
        File.WriteAllText(Path.Combine(output,"checks.txt"),failure==null?"MARITIME_CHECKS "+passed+" passed":"FAILED "+failure);
        if(failure!=null)Debug.LogException(failure);Application.Quit(failure==null?0:1);
    }
    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;
        SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+25;
        while(!VariableSingleton.IsInitialized&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"Campaign initialised with isolated saves");yield return new WaitForSecondsRealtime(.6f);CampaignHUD.Instance.DismissReport();
        foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
        // Populate existing lazy read defaults before checking navigation for mutations.
        var council=FindObjectOfType<CouncilUI>(true);council.Fleet();yield return null;council.Hide();
        foreach(string id in new[]{"sea-imperial","sea-contested","sea-ottoman","strait-open","strait-interdicted"})
            Check(Resources.Load<Texture2D>("UI/Maps/Maritime/"+id)!=null,"Imported "+id);
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);
            foreach(var numbers in new[]{new Vector2Int(60,48),new Vector2Int(48,48),new Vector2Int(26,48),Vector2Int.zero})
            {
                VariableSingleton.SetInt("iShips",numbers.x);VariableSingleton.SetInt("iTurkShips",numbers.y);
                foreach(bool open in new[]{false,true})
                {
                    VariableSingleton.ChangeBool("bStraitsOpen",open);WarTheatreUI.Show();yield return null;Canvas.ForceUpdateCanvases();
                    var sea=Badge(false);var strait=Badge(true);
                    Check(sea.GetComponent<RawImage>().texture.name==MaritimeMapMedallions.SeaAsset(numbers.x,numbers.y),"Correct naval image "+numbers);
                    Check(strait.GetComponent<RawImage>().texture.name==(open?"strait-open":"strait-interdicted"),"Independent passage image "+open);
                    string state=State();string rng=JsonUtility.ToJson(UnityEngine.Random.state);
                    sea.OnPointerEnter(new PointerEventData(EventSystem.current));Check(sea.GetComponent<Outline>().enabled,"Sea gold hover");sea.OnPointerExit(new PointerEventData(EventSystem.current));Check(!sea.GetComponent<Outline>().enabled,"Hover clears");
                    var rt=(RectTransform)sea.transform;
                    Check(sea.IsRaycastLocationValid(RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center)),null),"Circular centre clickable");
                    Check(!sea.IsRaycastLocationValid(RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.max)),null),"Transparent corner passes through");
                    strait.Activate();Canvas.ForceUpdateCanvases();
                    Check(GameObject.Find("Province information").GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains(open?"Passage open":"Passage interdicted")),"Passage details");
                    Check(state==State()&&rng==JsonUtility.ToJson(UnityEngine.Random.state),"Inspection changes no campaign state or RNG");
                    if(numbers.x==26&&!open){sea.OnPointerEnter(new PointerEventData(EventSystem.current));Capture("map-"+size.x+".png");yield return null;}
                    sea.Activate();yield return null;
                    Check(GameObject.Find("War Beyond the Walls map")==null,"Map closes for Fleet management");
                    Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text=="The Imperial Fleet"),"Fleet management opened");
                    Check(state==State()&&rng==JsonUtility.ToJson(UnityEngine.Random.state),"Fleet navigation changes no campaign state or RNG");
                    ControlSingleton.Instance.ReturnAction();yield return null;
                    Check(FindObjectsOfType<MaritimeMapMedallions>().Length==2,"Return opens map with two medallions");
                }
            }
            // Open map updates immediately after the authoritative passage flag changes.
            VariableSingleton.ChangeBool("bStraitsOpen",false);yield return null;
            VariableSingleton.ChangeBool("bStraitsOpen",true);yield return null;
            Check(Badge(true).GetComponent<RawImage>().texture.name=="strait-open","Live passage refresh");
            var saved=VariableSingleton.Instance.CaptureState();VariableSingleton.ChangeBool("bStraitsOpen",false);VariableSingleton.Instance.RestoreValidated(saved);yield return null;
            Check(Badge(true).GetComponent<RawImage>().texture.name=="strait-open","Save restoration refresh");
            WarTheatreUI.Close();
        }
    }
}
#endif
