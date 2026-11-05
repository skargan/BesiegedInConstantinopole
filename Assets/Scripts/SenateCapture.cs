#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public sealed class SenateCapture:MonoBehaviour
{
    string folder;int passed;readonly List<string> errors=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-senate-capture"))return;var path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Senate"));Directory.CreateDirectory(path);SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");var go=new GameObject("Senate verification");DontDestroyOnLoad(go);go.AddComponent<SenateCapture>().folder=path;}
    void Check(bool ok,string note){if(!ok)throw new Exception(note);passed++;}
    void Log(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error)errors.Add(message+"\n"+stack);}
    IEnumerator Start(){Application.logMessageReceived+=Log;var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+passed+"\n"+ex+"\n"+string.Join("\n",errors));Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),"SENATE_CHECKS "+passed+" passed");Application.Quit(0);}
    RectTransform Painting()=>GameObject.Find("Senate painting").GetComponent<RectTransform>();
    Vector2 Point(float x,float y){var r=Painting();return RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(r.rect.xMin+r.rect.width*x/1672f,r.rect.yMax-r.rect.height*y/941f)));}
    GameObject Hit(Vector2 point){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);return hits.Count==0?null:hits[0].gameObject;}
    LocationHotspot Target(string name,float x,float y){var point=Point(x,y);var hit=Hit(point);Check(hit!=null&&hit.name=="Hotspot · "+name,"raycast reaches "+name+"; actual "+(hit==null?"none":hit.name));return hit.GetComponent<LocationHotspot>();}
    void Click(LocationHotspot target,float x,float y)=>target.OnPointerClick(new PointerEventData(EventSystem.current){position=Point(x,y),button=PointerEventData.InputButton.Left});
    IEnumerator Run(){Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();var council=FindObjectOfType<CouncilUI>(true);Check(council!=null,"council exists");int gold=CouncilActions.Int("iMoney");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1440,1080)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);council.CivicRelations();yield return null;Canvas.ForceUpdateCanvases();
            Check(GameObject.Find("Senate title").GetComponent<TMP_Text>().alignment==TextAlignmentOptions.Center,"centred title");
            Check(Painting().GetComponentsInChildren<LocationHotspot>().Length==10,"five groups, connected Galata flag and four distinct shortcuts");
            string[] names={"Officers","Townspeople","Noble households","Clergy","Galata delegation"};float[] px={210,491,960,1257,1510};float[] py={541,552,555,563,569};
            for(int i=0;i<5;i++){
                var target=Target(names[i],px[i],py[i]);Check(target.Shape.Length>25,"traced silhouette "+names[i]);Check(!target.Contains(new Vector2(.4f,.85f)),"sky outside delegation");
                target.OnPointerEnter(new PointerEventData(EventSystem.current){position=Point(px[i],py[i])});Canvas.ForceUpdateCanvases();if(i==0||i==4){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"outline-"+i+"-"+size.x+".png"));yield return new WaitForSecondsRealtime(.15f);}target.SetHighlighted(false);
                Click(target,px[i],py[i]);yield return null;Canvas.ForceUpdateCanvases();
                var detail=GameObject.Find("Senate selected delegation");Check(detail.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==names[i]),"correct delegation selected");foreach(var label in detail.GetComponentsInChildren<TMP_Text>())Check(!label.isTextOverflowing,"detail fits "+label.text);
                Check(CouncilActions.Int("iMoney")==gold,"selection does not spend");
                GameObject.Find("Senate review action").GetComponent<Button>().onClick.Invoke();yield return null;Check(GameObject.Find("Confirm society action")!=null,"existing confirmation reached");Check(CouncilActions.Int("iMoney")==gold,"review does not spend");council.CivicRelations();yield return null;
            }
            var flag=Target("Galata delegation",1560,365);Check(flag.OutlinePeers!=null&&flag.OutlinePeers.Length==2,"flag and delegation share outline highlight");
            string[] portals={"Research tower","Shipyard","Hagia Sophia","Bribe the podestà"};float[] x={465,620,1250,1355},y={300,398,303,523};
            for(int i=0;i<4;i++){
                var target=Target(portals[i],x[i],y[i]);target.OnPointerEnter(new PointerEventData(EventSystem.current){position=Point(x[i],y[i])});if(size.x==1600){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"landmark-"+i+".png"));yield return new WaitForSecondsRealtime(.15f);}target.SetHighlighted(false);Click(target,x[i],y[i]);yield return null;
                Check(i==0?GameObject.Find("Technology desk")!=null:i==1?GameObject.Find("Shipyard screen")!=null:i==2?GameObject.Find("Location painting")!=null:GameObject.Find("Galata confirm 0")!=null,"shortcut destination "+portals[i]);
                Check(CouncilActions.Int("iMoney")==gold,"shortcut opens without debit");
                if(i==3){GameObject.Find("Galata return").GetComponent<Button>().onClick.Invoke();}else ControlSingleton.Instance.GoBack();yield return null;Check(GameObject.Find("Senate scene")!=null,"shortcut returns to Senate");
            }
            ControlSingleton.Instance.GoBack();yield return null;Check(GameObject.Find("Senate scene")==null,"return leaves Senate");
        }
        council.PrivateChamber();yield return null;Check(GameObject.Find("Private chamber painting").GetComponent<Image>().sprite!=null,"private chamber painting loads without former null reference");ControlSingleton.Instance.GoBack();yield return null;
        Check(errors.Count==0,"no runtime exceptions: "+string.Join("; ",errors));
    }
}
#endif
