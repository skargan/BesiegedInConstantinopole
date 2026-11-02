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

public sealed class PalaceFigureCapture : MonoBehaviour
{
    string output;CouncilUI council;VariableSingleton.CampaignSave baseline;readonly List<string> results=new List<string>();
    static readonly int[] People={0,1,2,3,4,5,6,7,100,101,102,103,104,105,200};
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-palace-figure-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/PalaceFigures"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Palace figure verification");DontDestroyOnLoad(go);go.AddComponent<PalaceFigureCapture>().output=folder;}
    void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);results.Add("PASS "+label);}
    string State(){var s=VariableSingleton.Instance.CaptureState();s.randomState=default(UnityEngine.Random.State);return JsonUtility.ToJson(s);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    RectTransform Painting()=>GameObject.Find("Location painting").GetComponent<RectTransform>();
    Vector2 Point(RectTransform r,float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(r.rect.xMin+r.rect.width*x,r.rect.yMin+r.rect.height*y)));
    GameObject Hit(Vector2 point){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);return hits.Count==0?null:hits[0].gameObject;}
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        string report="PALACE_FIGURE_CHECKS "+results.Count+" passed; all 15 IDs at 1600x900 and 1280x720, all fifteen visitors captured at 1280x720, plus four at 1600x900.";File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    void Fixture(int person){CampaignHUD.Instance.DismissReport();council.Hide();VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline)));
        if(person<8)VariableSingleton.ChangeBool(CourtRoster.Keys[person],true);
        if(person>=100&&person<106){VariableSingleton.SetInt("iAdventurerChoice",person-99);VariableSingleton.SetInt("iAdventurerArrived",1);}
        if(person==200)VariableSingleton.SetInt("iKjoldState",1);
        VariableSingleton.SetInt("iHardOfficerReturn"+person,0);
        VariableSingleton.SetInt("iLifeLocationTurn",VariableSingleton.Turn+1);VariableSingleton.SetInt("iLifeLocationVisitor0",person);VariableSingleton.SetInt("iLifeLocationVisitor1",-1);VariableSingleton.SetInt("iLifeLocationVisitor2",-1);}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;Check(SaveGames.Automated,"isolated archives");SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+20;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;Check(VariableSingleton.IsInitialized,"campaign initialized");yield return new WaitForSecondsRealtime(.5f);
        council=CouncilUI.For(FindObjectOfType<ManagerUI>());baseline=VariableSingleton.Instance.CaptureState();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);Check(Screen.width==size.x&&Screen.height==size.y,"resolution "+size);
            foreach(int person in People){Fixture(person);string before=State(),rng=Rng();council.Palace();yield return null;Canvas.ForceUpdateCanvases();string context=size.x+"x"+size.y+" ID"+person;
                var go=GameObject.Find("Palace court figure "+person);Check(go!=null,context+" actual full-body asset exists");var view=go.GetComponent<PalaceCourtFigure>();var image=go.GetComponent<Image>();var rect=go.GetComponent<RectTransform>();
                Check(image.sprite!=null&&image.sprite.texture.isReadable&&image.alphaHitTestMinimumThreshold>0,context+" alpha-backed silhouette hit-testing active");
                Check(Mathf.Abs(rect.anchorMin.x-PalaceCourtFigure.FootX)<.0001f&&Mathf.Abs(rect.anchorMin.y-PalaceCourtFigure.FootY)<.0001f&&rect.pivot==new Vector2(.5f,0)&&rect.rect.height<=Painting().rect.height*PalaceCourtFigure.Height+.5f,context+" bottom-centred feet and bounded height");
                Check(before==State()&&rng==Rng(),context+" opening preserves visitor state and RNG");
                Check(PalaceFigureArt.CachedCount<=4,context+" lazy cache remains bounded after previous view releases");
                var plate=GameObject.Find("Palace figure name "+person).GetComponent<Button>();var text=plate.GetComponentInChildren<TMP_Text>();text.ForceMeshUpdate();Check(!text.isTextOverflowing,context+" nameplate fits");
                var portals=Painting().GetComponentsInChildren<LocationHotspot>();
                foreach(var target in new[]{new {name="To the garden",x=.897f,y=.60f},new {name="Hagia Sophia",x=.410f,y=.705f},new {name="Defence of the walls",x=.60f,y=.32f}}){var portal=portals.First(p=>p.name=="Hotspot · "+target.name);Check(Hit(Point(Painting(),target.x,target.y))==portal.gameObject,context+" "+target.name+" remains clickable");}
                bool tested=false;var crop=image.sprite.rect;
                for(int yi=1;yi<10&&!tested;yi++)for(int xi=1;xi<10&&!tested;xi++){float x=xi/10f,y=yi/10f;var colour=image.sprite.texture.GetPixelBilinear((crop.x+crop.width*x)/image.sprite.texture.width,(crop.y+crop.height*y)/image.sprite.texture.height);
                    if(colour.a>.01f)continue;var point=Point(rect,x,y);Check(!image.IsRaycastLocationValid(point,null)&&Hit(point)!=go,context+" transparent margin falls through to underlying scene");tested=true;}
                Check(tested,context+" transparent sample found");view.OnPointerEnter(null);yield return null;Check(before==State()&&rng==Rng(),context+" hover has no gameplay effect");
                if(person<4||size.x==1280)yield return Capture("palace-"+context.Replace(' ','-'));view.OnPointerExit(null);
                if(person==0||person==100){go.GetComponent<Button>().onClick.Invoke();yield return null;Check(GameObject.Find("Council screen").GetComponentsInChildren<TMP_Text>().Any(t=>t.text==GardenConversations.Name(person)),context+" figure opens existing conversation");ControlSingleton.Instance.GoBack();yield return null;Check(GameObject.Find("Palace court figure "+person)!=null,context+" conversation returns to same visitor");
                    GameObject.Find("Palace figure name "+person).GetComponent<Button>().onClick.Invoke();yield return null;ControlSingleton.Instance.GoBack();yield return null;Check(before==State()&&rng==Rng(),context+" nameplate and return retain state and RNG");}
            }
        }
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Palace capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
