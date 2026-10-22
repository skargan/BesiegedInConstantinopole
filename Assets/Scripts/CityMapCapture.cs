#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class CityMapCapture:MonoBehaviour
{
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Begin(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-city-map-capture")<0)return;var go=new GameObject("City map verification");DontDestroyOnLoad(go);go.AddComponent<CityMapCapture>();}
 string output;int passed;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;}
 CityMapView Map()=>FindObjectOfType<CityMapView>();
 string Visible()=>string.Join("\n",FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.text));
 LocationHotspot Wall(int index)=>Map().GetComponentsInChildren<CityMapWall>().First(w=>w.Index==index).GetComponent<LocationHotspot>();
 PointerEventData Pointer(LocationHotspot h){Vector2 p=(h.Shape[2]+h.Shape[h.Shape.Length-3])*.5f;var r=h.rectTransform.rect;var world=h.rectTransform.TransformPoint(new Vector3(r.xMin+p.x*r.width,r.yMin+p.y*r.height));return new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,world),button=PointerEventData.InputButton.Left};}
 IEnumerator Start(){
  AudioListener.volume=0;Application.runInBackground=true;
  output=Path.GetFullPath(Path.Combine(Application.dataPath,"../CityMapCaptures"));Directory.CreateDirectory(output);VariableSingleton.TestSavePath=Path.Combine(output,"test-save.json");
  yield return new WaitForSeconds(.5f);
  var menu=FindObjectOfType<MainMenuUI>();Check(menu!=null,"Title menu missing");
  var titleLink=GameObject.Find("City map navigation").GetComponent<Button>();titleLink.onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();
  Check(Map()!=null&&!Map().InCampaign,"Title menu did not open browsing map");Check(!VariableSingleton.IsInitialized,"Browsing started a campaign");
  Check(Map().GetComponentsInChildren<CityMapWall>().Length==4,"Wall targets missing");
  var art=Resources.Load<Texture2D>("UI/MidnightMosaic/Locations/city-map");Check(art!=null&&art.width>1000,"City artwork missing");
  var wall=Wall(0);var pointer=Pointer(wall);Check(wall.IsRaycastLocationValid(pointer.position,null),"Wall centre not clickable");
  var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Check(hits.Any(h=>h.gameObject==wall.gameObject),"Invisible wall target not raycastable");
  wall.OnPointerEnter(pointer);yield return Capture("title-map-hover");wall.OnPointerExit(pointer);
  for(int index=1;index<3;index++){var other=Wall(index);var hover=Pointer(other);other.OnPointerEnter(hover);yield return Capture("title-map-wall-"+index);other.OnPointerExit(hover);}
  wall.OnPointerClick(pointer);yield return null;
  Check(GameObject.Find("Wall information")!=null&&Visible().Contains("Theodosian Wall"),"Title wall description missing");Check(!VariableSingleton.IsInitialized,"Title inspection mutated campaign");
  Map().Close();yield return null;Check(Map()==null&&menu.gameObject.activeInHierarchy,"Return to title failed");
  yield return Capture("title-menu-map-entry");
  menu.NewGame();yield return new WaitForSeconds(.5f);
  var manager=FindObjectOfType<ManagerUI>();Check(manager!=null,"Council missing");
  yield return Capture("council-map-entry");
  var link=GameObject.Find("City map navigation").GetComponent<Button>();link.onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();
  Check(Map()!=null&&Map().InCampaign,"Council did not open campaign map");yield return Capture("campaign-map");
  int gold=CouncilActions.Int("iMoney"),turn=VariableSingleton.Turn;
  for(int i=0;i<3;i++){
   wall=Wall(i);pointer=Pointer(wall);wall.OnPointerClick(pointer);yield return null;
   Check(Map()==null,"Map did not yield to wall actions");Check(Visible().Contains(new[]{"Theodosian Wall Actions","Constantinian Wall Actions","Severan Wall Actions"}[i]),"Wrong wall selected");
   ControlSingleton.Instance.GoBack();yield return null;Check(Map()!=null,"Return from wall inspector failed");
  }
  Check(CouncilActions.Int("iMoney")==gold&&VariableSingleton.Turn==turn,"Inspection spent resources or time");
  Screen.SetResolution(1100,760,false);yield return new WaitForSeconds(.4f);Canvas.ForceUpdateCanvases();wall=Wall(1);pointer=Pointer(wall);Check(wall.IsRaycastLocationValid(pointer.position,null),"Resized wall target misaligned");
  Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.4f);Canvas.ForceUpdateCanvases();
  VariableSingleton.ChangeFloat("fOuterWall",1-VariableSingleton.GetFloatVariable("fOuterWall"));Wall(0).OnPointerClick(Pointer(Wall(0)));yield return null;
  var defense=FindObjectOfType<DefenseActions>();defense.RepairSelected();Check(CouncilActions.Int("iMoney")==gold,"Full wall charged for repair");ControlSingleton.Instance.GoBack();yield return null;
  VariableSingleton.ChangeFloat("fOuterWall",.60f-VariableSingleton.GetFloatVariable("fOuterWall"));VariableSingleton.SetInt("iMoney",0);Wall(0).OnPointerClick(Pointer(Wall(0)));yield return null;
  defense=FindObjectOfType<DefenseActions>();defense.RepairSelected();Check(VariableSingleton.GetFloatVariable("fOuterWall")==.60f&&CouncilActions.Int("iMoney")==0,"Unaffordable repair mutated state");ControlSingleton.Instance.GoBack();yield return null;
  VariableSingleton.SetInt("iMoney",10000);Wall(0).OnPointerClick(Pointer(Wall(0)));yield return Capture("wall-repair-inspector");
  defense=FindObjectOfType<DefenseActions>();defense.RepairSelected();yield return null;
  Check(VariableSingleton.GetFloatVariable("fOuterWall")>.60f&&CouncilActions.Int("iMoney")<10000,"Existing repair action failed");Check(!manager.gameObject.activeInHierarchy&&FindObjectOfType<RestUI>()!=null,"Repair did not use major action flow");
  Check(Map()==null,"Map remained over completed action");
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
