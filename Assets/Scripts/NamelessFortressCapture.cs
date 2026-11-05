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
 string folder;int checks;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-nameless-fortress-capture"))return;var go=new GameObject("Nameless fortress check");DontDestroyOnLoad(go);go.AddComponent<NamelessFortressCapture>();}
 void Check(bool yes,string message){if(!yes)throw new Exception(message);checks++;}
 float[] Weights(VultureFlight.Pose p){var a=new float[4];a[p.first]=1-p.blend;a[p.second]+=p.blend;return a;}
 IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/NamelessFortress"));Directory.CreateDirectory(folder);var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+checks+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),checks+" vulture motion and presentation checks passed");Application.Quit(0);}
 IEnumerator Run(){yield return null;
 var prev=VultureFlight.Sample(0);float dt=1f/60;
 for(int i=1;i<=1920;i++){var p=VultureFlight.Sample(i*dt);Check(Vector2.Distance(p.anchor,prev.anchor)<.0004f,"continuous path "+i);Check(Mathf.Abs(p.bank-prev.bank)<.03f,"continuous bank "+i);Check(p.depth>=.779f&&p.depth<=1.001f,"positive perspective scale "+i);Check(p.anchor.x>=.169f&&p.anchor.x<=.351f&&p.anchor.y>=.914f&&p.anchor.y<=.966f,"orbit above tower "+i);var a=Weights(prev);var b=Weights(p);Check(Enumerable.Range(0,4).Max(k=>Mathf.Abs(a[k]-b[k]))<.01f,"continuous pose transition "+i);prev=p;}
 var start=VultureFlight.Sample(0);Check(Vector2.Distance(start.anchor,prev.anchor)<.00001f&&Mathf.Abs(start.bank-prev.bank)<.0001f,"seamless circuit closure");
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);NamelessFortressUI.Show();yield return null;Canvas.ForceUpdateCanvases();var root=GameObject.Find("Nameless Fortress visit");Check(root.GetComponentsInChildren<RawImage>().All(i=>i.texture!=null),"textures loaded");foreach(var text in root.GetComponentsInChildren<TMP_Text>())Check(!text.isTextOverflowing,"text fits");var flight=root.GetComponentInChildren<VultureFlight>();var bird=flight.GetComponent<RectTransform>();var before=bird.anchorMin;yield return new WaitForSecondsRealtime(.6f);Check(before!=bird.anchorMin,"live motion");flight.enabled=false;
 foreach(float seconds in new[]{0f,6.4f,8f,14.4f,16f,22.4f,24f,30.4f}){flight.Present(seconds);yield return null;Check(bird.localScale==Vector3.one,"no mirrored transform");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"flight-"+size.x+"-"+seconds.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+".png"));yield return new WaitForEndOfFrame();}
 root.GetComponentsInChildren<Button>().Single().onClick.Invoke();yield return null;Check(GameObject.Find("Nameless Fortress visit")==null,"return closes view");}
 // Review-only GIF source frames. The game retains full alpha PNG sprites, not GIF decoding.
 Screen.SetResolution(640,360,false);yield return new WaitForSecondsRealtime(.3f);NamelessFortressUI.Show();yield return null;Canvas.ForceUpdateCanvases();var clip=GameObject.Find("Nameless Fortress visit").GetComponentInChildren<VultureFlight>();clip.enabled=false;Directory.CreateDirectory(Path.Combine(folder,"Loop"));
 for(int i=0;i<128;i++){clip.Present(i*.25f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Loop",i.ToString("D3")+".png"));yield return null;}
 NamelessFortressUI.Close();yield return null;
 }
}
#endif
