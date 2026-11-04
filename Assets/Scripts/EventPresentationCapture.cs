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

public sealed class EventPresentationCapture : MonoBehaviour
{

    string output;readonly List<string> checks=new List<string>();
    VariableSingleton.CampaignSave baseline;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-event-presentation-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/EventPresentation"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");
        var go=new GameObject("Event presentation verification");DontDestroyOnLoad(go);go.AddComponent<EventPresentationCapture>().output=path;
    }
    void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add("PASS "+name);}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    IEnumerator Start(){var stack=new Stack<IEnumerator>();stack.Push(Run());Exception failure=null;int width=Screen.width,height=Screen.height;var mode=Screen.fullScreenMode;
        while(stack.Count>0){object next=null;bool moved=false;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null)break;if(!moved){stack.Pop();continue;}if(next is IEnumerator child)stack.Push(child);else yield return next;}
        Screen.SetResolution(width,height,mode);yield return new WaitForSecondsRealtime(.25f);
        File.WriteAllLines(Path.Combine(output,"details.txt"),checks);string result=failure==null?"EVENT_PRESENTATION_CHECKS "+checks.Count+" passed":"FAILED "+failure;
        File.WriteAllText(Path.Combine(output,"checks.txt"),result);Debug.Log(result);Application.Quit(failure==null?0:1);
    }
    void Fixture(bool war){var data=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));data.turn=war?VariableSingleton.EffectiveSiegeTurn+1:0;VariableSingleton.Instance.RestoreValidated(data);
        VariableSingleton.SetInt("iMoney",999999);VariableSingleton.SetInt("iPopulation",999999);VariableSingleton.SetInt("iMaterials",999999);VariableSingleton.SetInt("iProvisions",999999);}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(.8f);
        Check(VariableSingleton.IsInitialized,"campaign initialised");Check(SaveGames.Automated,"isolated saves enabled");CampaignHUD.Instance.DismissReport();baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.35f);
            if(FindObjectOfType<ManagerUI>()==null){UnityEngine.SceneManagement.SceneManager.LoadScene("Continue Game");yield return new WaitForSecondsRealtime(.6f);CampaignHUD.Instance.DismissReport();}
            foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
            var council=CouncilUI.For(FindObjectOfType<ManagerUI>(true));
            foreach(bool war in new[]{false,true}){
                Fixture(war);VariableSingleton.ChangeBool("bGoldenHornChain",true);council.Fleet();yield return new WaitForSecondsRealtime(.3f);
                var chain=FindObjectOfType<FleetChainBackdrop>();Check(chain!=null,"chain behaviour exists");var spot=chain.GetComponent<LocationHotspot>();var art=chain.transform.parent.GetComponent<RawImage>();
                string phase=war?"war":"peace";Check(art.texture.name=="fleet-"+phase,"correct chained phase artwork");Check(spot.raycastTarget,"present chain can be hovered");
                string before=State();chain.OnPointerEnter(null);yield return null;Check(GameObject.Find("Great Chain description")!=null,"chain tooltip shown");
                Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text.Contains("barring the Ottoman fleet")),"chain tooltip prose visible");
                Capture("fleet-"+phase+"-chain-hover-"+size.x+".png");chain.OnPointerExit(null);Check(GameObject.Find("Great Chain description")==null,"hover tooltip closes");Check(before==State(),"hover preserves gameplay state and randomness");
                VariableSingleton.ChangeBool("bGoldenHornChain",false);yield return null;yield return null;
                Check(art.texture.name=="fleet-"+phase+"-no-chain","lost chain changes artwork without reopening");Check(!spot.raycastTarget,"absent chain not hoverable");chain.OnPointerEnter(null);Check(GameObject.Find("Great Chain description")==null,"absent chain has no ghost tooltip");Capture("fleet-"+phase+"-no-chain-"+size.x+".png");
                VariableSingleton.ChangeBool("bGoldenHornChain",true);yield return null;yield return null;Check(art.texture.name=="fleet-"+phase,"restored chain updates live");
            }
            Canvas.ForceUpdateCanvases();var hud=FindObjectOfType<CampaignStatusStrip>();Check(hud!=null,"HUD exists");
            foreach(var label in hud.GetComponentsInChildren<TMP_Text>()){label.ForceMeshUpdate();Check(!label.isTextOverflowing,"HUD text fits: "+label.text);}
            Check(hud.transform.Find("IMPERIAL RESOURCES")!=null&&hud.transform.Find("MILITARY")!=null,"group names updated");
            Check(!hud.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("CONSTANTINOPLE")||t.text.Contains("Preparations")),"old status banner removed");
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Campaign menu").onClick.Invoke();yield return null;
            Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text==CouncilUI.BuildLabel),"version accessible inside campaign menu");Capture("campaign-menu-"+size.x+".png");
            FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Resume").onClick.Invoke();yield return null;council.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Continue Game");yield return new WaitForSecondsRealtime(.6f);Fixture(false);CampaignHUD.Instance.DismissReport();
            FindObjectOfType<ManagerUI>(true).gameObject.SetActive(false);var eventUI=FindObjectOfType<EventUI>(true);eventUI.SetStartNode("story0");eventUI.gameObject.SetActive(true);
            for(int n=0;n<40&&!FindObjectsOfType<Yarn.Unity.OptionButton>().Any(o=>o.IsInteractable());n++)yield return new WaitForSecondsRealtime(.15f);
            yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();
            var options=FindObjectsOfType<Yarn.Unity.OptionButton>();Check(options.Length>=5,"actual preparation focus choices rendered");
            var scrolls=FindObjectsOfType<ScrollRect>();var narrative=scrolls.First(s=>s.content!=null&&s.content.GetComponent<TMP_Text>()!=null&&s.name.Contains("narrative"));
            Check(narrative.viewport.GetComponent<RectMask2D>()!=null,"narrative has clipping viewport");Check(narrative.content.GetComponent<TMP_Text>().maskable,"narrative text obeys mask");
            Check(narrative.content.rect.height>narrative.viewport.rect.height,"long narrative scrolls");
            var picture=eventUI.GetComponentsInChildren<RawImage>().First(i=>i.name=="Illustration");Check(picture.texture!=null,"event illustration is loaded");
            var corners=new Vector3[4];picture.rectTransform.GetWorldCorners(corners);Check(corners[2].y-corners[0].y>Screen.height*.22f,"event illustration is substantial");
            foreach(var option in options){var label=option.GetComponentInChildren<TMP_Text>();label.ForceMeshUpdate();Check(!label.isTextOverflowing,"choice text fits");}
            Capture("focus-top-"+size.x+".png");narrative.verticalNormalizedPosition=0;foreach(var s in scrolls.Where(s=>s.name=="Scrollable choices"))s.verticalNormalizedPosition=0;
            yield return null;Capture("focus-bottom-"+size.x+".png");
            // Presentation scrolling must leave the pending choice untouched.
            Check(options.All(o=>o.gameObject.activeInHierarchy),"scrolling keeps all choices available");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Continue Game");yield return new WaitForSecondsRealtime(.6f);CampaignHUD.Instance.DismissReport();
        }
    }
    void Capture(string filename)
    {
        Canvas.ForceUpdateCanvases();var go=new GameObject("Maritime capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,filename),shot.EncodeToPNG());}
        finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);}
    }
}
#endif
