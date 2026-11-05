#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class NamelessFortressCapture:MonoBehaviour
{
 string folder;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-nameless-fortress-capture"))return;var go=new GameObject("Nameless fortress check");DontDestroyOnLoad(go);go.AddComponent<NamelessFortressCapture>();}
 IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/NamelessFortress"));Directory.CreateDirectory(folder);yield return null;
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);NamelessFortressUI.Show();yield return null;Canvas.ForceUpdateCanvases();var root=GameObject.Find("Nameless Fortress visit");bool ok=root!=null&&root.GetComponentsInChildren<RawImage>().All(i=>i.texture!=null)&&root.GetComponentsInChildren<TMP_Text>().All(t=>!t.isTextOverflowing);var bird=root.transform.Find("Mountain backdrop/Nameless ruins painting/Circling vulture");var before=bird.GetComponent<RectTransform>().anchorMin;yield return new WaitForSecondsRealtime(.6f);ok&=before!=bird.GetComponent<RectTransform>().anchorMin;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"fortress-"+size.x+".png"));yield return new WaitForSecondsRealtime(.3f);root.GetComponentsInChildren<Button>().Single().onClick.Invoke();ok&=GameObject.Find("Nameless Fortress visit")==null;if(!ok){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED at "+size.x);Application.Quit(1);yield break;}}
 File.WriteAllText(Path.Combine(folder,"checks.txt"),"PASS: textures, text fit, vulture motion and Return at 1280x720 / 1920x1080");Application.Quit(0);}
}
#endif
