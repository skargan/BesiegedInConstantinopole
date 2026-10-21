#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class HotspotCapture:MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-hotspot-capture")<0)return;var go=new GameObject("Hotspot verification");DontDestroyOnLoad(go);go.AddComponent<HotspotCapture>();}
    string output;int passed;
    void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;}
    LocationHotspot Find(string label)=>FindObjectsOfType<LocationHotspot>().First(x=>x.name=="Hotspot · "+label);
    PointerEventData Pointer(LocationHotspot h,Vector2 p){var r=h.rectTransform.rect;var world=h.rectTransform.TransformPoint(new Vector3(r.xMin+p.x*r.width,r.yMin+p.y*r.height));return new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,world),button=PointerEventData.InputButton.Left};}
    IEnumerator Start(){
        AudioListener.volume=0;Application.runInBackground=true;
        output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures"));Directory.CreateDirectory(output);VariableSingleton.TestSavePath=Path.Combine(output,"test-save.json");
        yield return null;FindObjectOfType<MainMenuUI>().NewGame();yield return new WaitForSeconds(.5f);
        var c=CouncilUI.For(FindObjectOfType<ManagerUI>());c.Palace();yield return Capture("palace-idle");
        var door=Find("To the garden");Check(door.Contains(new Vector2(.895f,.60f)),"Door centre missed");Check(!door.Contains(new Vector2(.845f,.845f)),"Door bounding corner incorrectly clickable");
        var mouse=Pointer(door,new Vector2(.895f,.60f));var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(mouse,hits);Check(hits.Any(x=>x.gameObject==door.gameObject),"Invisible hotspot not raycastable");
        door.OnPointerEnter(mouse);yield return Capture("palace-door-hover");
        var cursor=FindObjectOfType<ByzantineCursor>();cursor.enabled=false;cursor.GetComponent<Canvas>().enabled=true;cursor.transform.GetChild(0).position=mouse.position;
        yield return Capture("palace-hand-hover");cursor.GetComponent<Canvas>().enabled=false;cursor.enabled=true;door.OnPointerExit(mouse);
        var cathedral=Find("Hagia Sophia");Check(cathedral.Contains(new Vector2(.410f,.705f)),"Cathedral missed");Check(!cathedral.Contains(new Vector2(.367f,.746f)),"Cathedral bounding corner clickable");
        cathedral.OnPointerEnter(Pointer(cathedral,new Vector2(.410f,.705f)));yield return Capture("palace-cathedral-hover");cathedral.OnPointerExit(mouse);
        door.OnPointerClick(mouse);yield return null;Check(FindObjectsOfType<LocationHotspot>().Any(x=>x.name=="Hotspot · Return to the Palace"),"Door navigation failed");
        var back=Find("Return to the Palace");back.OnPointerEnter(Pointer(back,new Vector2(.045f,.60f)));yield return Capture("garden-door-hover");
        back.OnPointerClick(Pointer(back,new Vector2(.045f,.60f)));yield return null;Check(FindObjectsOfType<LocationHotspot>().Any(x=>x.name=="Hotspot · To the garden"),"Return failed");
        var before=VariableSingleton.Instance.CaptureState();cathedral=Find("Hagia Sophia");cathedral.OnPointerClick(Pointer(cathedral,new Vector2(.410f,.705f)));yield return null;Check(PalaceLocations.VisitedSanctuary,"Cathedral click failed");
        c.Palace();yield return null;Screen.SetResolution(1100,760,false);yield return new WaitForSeconds(.5f);Canvas.ForceUpdateCanvases();
        door=Find("To the garden");mouse=Pointer(door,new Vector2(.895f,.60f));Check(door.IsRaycastLocationValid(mouse.position,null),"Resized hit region failed");
        Check(Resources.Load<Texture2D>("UI/MidnightMosaic/byzantine-hand")!=null,"Cursor asset missing");
        Check(FindObjectOfType<ByzantineCursor>()!=null,"Cursor controller missing");
        File.WriteAllText(Path.Combine(output,"checks.txt"),passed+" exported-player checks passed.");Application.Quit();
    }
    private IEnumerator Capture(string name)
    {
        yield return new WaitForSeconds(1);
        yield return null;
        File.WriteAllLines(Path.Combine(output,name+"-text.txt"),FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.name+" | "+t.rectTransform.rect+" | "+t.maxVisibleCharacters+" | "+string.Join(",",t.GetComponentsInParent<CanvasGroup>().Select(g=>g.name+":"+g.alpha))+" | "+t.text));
        // Render explicitly: a hidden Windows player can otherwise return a black back buffer.
        var cameraObject = new GameObject("Capture camera",typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic=true; camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.06f,.07f,.08f); camera.nearClipPlane=.1f; camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();
        var modes=canvases.Select(c=>c.renderMode).ToArray();
        var cameras=canvases.Select(c=>c.worldCamera).ToArray();
        var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=new RenderTexture(1600,900,24);
        camera.targetTexture=target;
        foreach(var canvas in canvases) {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
        Canvas.ForceUpdateCanvases();
        camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=target;
        var shot=new Texture2D(1600,900,TextureFormat.RGB24,false);
        shot.ReadPixels(new Rect(0,0,1600,900),0,0);shot.Apply();
        File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        RenderTexture.active=previous;
        for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
        camera.targetTexture=null;target.Release();Destroy(target);Destroy(shot);Destroy(cameraObject);
        yield return new WaitForSeconds(.3f);
    }
}
#endif
