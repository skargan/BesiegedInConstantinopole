#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public sealed class MapDragCapture:MonoBehaviour {
    int checks;string output;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot(){if(!Environment.GetCommandLineArgs().Contains("-map-drag-check"))return;
        var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/MapDrag"));Directory.CreateDirectory(folder);
        SaveGames.TestRoot=Path.Combine(folder,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Map drag checks");DontDestroyOnLoad(go);go.AddComponent<MapDragCapture>().output=folder;}
    void Check(bool result,string label){if(!result)throw new Exception(label);checks++;}
    IEnumerator Start(){var run=Run();while(true){bool more;object next=null;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception e){File.WriteAllText(Path.Combine(output,"checks.txt"),e.ToString());Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(output,"checks.txt"),"MAP_DRAG_CHECKS "+checks+" passed");Application.Quit();}
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(1);
        foreach(var module in FindObjectsOfType<BaseInputModule>())module.enabled=false;
        Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.5f);
        Action[] open={WarTheatreUI.Show,ConstantinopleVicinityUI.Show,WorldMapUI.Show};
        string[] roots={"War Beyond the Walls map","Vicinity of Constantinople","World map viewer"};
        for(int i=0;i<open.Length;i++){
            open[i]();yield return null;Canvas.ForceUpdateCanvases();
            var scroll=GameObject.Find(roots[i]).GetComponentInChildren<MapDragScrollRect>();Check(scroll!=null,roots[i]+" shares grab behaviour");
            scroll.content.sizeDelta*=2;Canvas.ForceUpdateCanvases();
            var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=new Vector2(700,400)};
            var before=scroll.content.anchoredPosition;scroll.OnBeginDrag(pointer);Check(ByzantineCursor.IsGripping,"fist on begin");
            pointer.position+=new Vector2(80,25);scroll.OnDrag(pointer);Check(scroll.content.anchoredPosition!=before,"map moves");
            var cursor=FindObjectOfType<ByzantineCursor>();typeof(ByzantineCursor).GetMethod("LateUpdate",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(cursor,null);
            Check(cursor.GetComponentInChildren<RawImage>().uvRect.x==.5f,"fist sprite selected");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"map-"+i+".png"));
            scroll.OnEndDrag(pointer);Check(!ByzantineCursor.IsGripping,"release restores pointer");
            pointer.button=PointerEventData.InputButton.Right;scroll.OnBeginDrag(pointer);Check(!ByzantineCursor.IsGripping,"right click stays pointer");
            pointer.button=PointerEventData.InputButton.Left;scroll.OnBeginDrag(pointer);scroll.SendMessage("OnApplicationFocus",false);Check(!ByzantineCursor.IsGripping,"focus loss restores pointer");
            scroll.OnBeginDrag(pointer);GameObject.Find(roots[i]).SetActive(false);Check(!ByzantineCursor.IsGripping,"closing restores pointer");
        }
    }
}
#endif
