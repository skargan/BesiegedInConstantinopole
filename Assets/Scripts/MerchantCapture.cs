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
public sealed class MerchantCapture : MonoBehaviour
{
    string output; CouncilUI council; readonly List<string> results=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-merchant-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Merchant"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Merchant verification");DontDestroyOnLoad(go);go.AddComponent<MerchantCapture>().output=folder;}
    void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);results.Add("PASS "+text);}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    GameObject Root()=>GameObject.Find("Council screen");
    Button Button(string name)=>Root().GetComponentsInChildren<Button>().Single(b=>b.name==name);
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        var report="MERCHANT_CHECKS "+results.Count+" passed; both market paintings, 30 remarks, three resolutions, raycasts and campaign state isolation.";File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+20;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(.5f);CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());
        Check(MarketMerchant.RemarkCount==30,"thirty authored remarks loaded");var baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720),new Vector2Int(1280,960)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);
            foreach(bool war in new[]{false,true}){
                var fixture=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));fixture.turn=war?VariableSingleton.EffectiveSiegeTurn:baseline.turn;VariableSingleton.Instance.RestoreValidated(fixture);
                council.Market();yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();
                var merchant=Root().GetComponentInChildren<MarketMerchant>();Check(merchant!=null,"merchant hover attached");
                var spot=merchant.GetComponent<LocationHotspot>();var image=Root().GetComponentInChildren<RawImage>();
                Check(image.texture.name==(war?"market-war":"market-peace"),"phase-specific edited art");
                // Resolve a point on the face through the artwork's actual UV crop.
                var uv=image.uvRect;var rect=image.rectTransform.rect;var normalized=new Vector2((.26f-uv.x)/uv.width,(.50f-uv.y)/uv.height);
                var world=image.rectTransform.TransformPoint(new Vector3(rect.xMin+rect.width*normalized.x,rect.yMin+rect.height*normalized.y,0));
                var screenPoint=RectTransformUtility.WorldToScreenPoint(null,world);Check(spot.IsRaycastLocationValid(screenPoint,null),"hotspot follows the displayed face at "+size);
                var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=screenPoint};
                var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,hits);
                Check(hits.Count>0&&hits[0].gameObject==spot.gameObject,"merchant receives pointer above the painting");
                string before=State(),rng=JsonUtility.ToJson(UnityEngine.Random.state);var lines=new HashSet<string>();
                for(int i=0;i<30;i++){
                    UnityEngine.EventSystems.ExecuteEvents.Execute(merchant.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerEnterHandler);
                    Check(merchant.Speaking&&lines.Add(merchant.CurrentRemark),"each hover yields a distinct remark within the shuffled deck");
                    Canvas.ForceUpdateCanvases();foreach(var label in GameObject.Find("Merchant speech").GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();Check(!label.isTextOverflowing,"remark fits: "+label.text);}
                    if(i==0)yield return Capture("merchant-"+(war?"war":"peace")+"-"+size.x+"x"+size.y);
                    foreach(var graphic in GameObject.Find("Merchant speech").GetComponentsInChildren<UnityEngine.UI.Graphic>())Check(!graphic.raycastTarget,"speech cannot steal mouse input");
                    UnityEngine.EventSystems.ExecuteEvents.Execute(merchant.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.pointerExitHandler);Check(!merchant.Speaking,"exit hides quote and highlight");
                }
                Check(State()==before&&JsonUtility.ToJson(UnityEngine.Random.state)==rng,"merchant chatter never changes campaign or random stream");
                var buy=Button("Market buy iMaterials");var corners=new Vector3[4];((RectTransform)buy.transform).GetWorldCorners(corners);
                pointer.position=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f);hits.Clear();UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer,hits);
                Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>()==buy,"purchasing remains unobstructed");
            }
        }
        CampaignHUD.Instance.ShowReport("Lethality and unit roles",ManagerUI.MilitaryExplanation,()=>CampaignHUD.Instance.DismissReport());yield return null;yield return Capture("unit-guide-top");
        var scroll=GameObject.Find("Outcome scroll").GetComponent<UnityEngine.UI.ScrollRect>();scroll.verticalNormalizedPosition=0;yield return null;yield return Capture("unit-guide-bottom");
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Diplomacy capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
