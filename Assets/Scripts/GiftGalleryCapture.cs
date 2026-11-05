#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class GiftGalleryCapture:MonoBehaviour
{
 string folder;int passed;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-gift-gallery-capture"))return;var go=new GameObject("Gift gallery check");DontDestroyOnLoad(go);go.AddComponent<GiftGalleryCapture>();}
 void Check(bool ok,string reason){if(!ok)throw new Exception(reason);passed++;}
 IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/GiftGallery"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+passed+" "+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),passed+" gift gallery checks passed");Application.Quit(0);}
 IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();var council=FindObjectOfType<CouncilUI>(true);int gold=CouncilActions.Int("iMoney");
 foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);
 for(int i=0;i<8;i++){council.SultanGiftMenu();yield return null;Canvas.ForceUpdateCanvases();var root=GameObject.Find("Council screen");Check(root.GetComponentsInChildren<RawImage>().Count()==8,"eight artworks");foreach(var image in root.GetComponentsInChildren<RawImage>())Check(image.texture!=null,"texture loaded");foreach(var label in root.GetComponentsInChildren<TMP_Text>())Check(!label.isTextOverflowing,"gallery text fits "+label.text);if(i==0){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"gallery-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);}
 root.transform.Find("Gift card "+i).GetComponent<Button>().onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();root=GameObject.Find("Council screen");Check(root.GetComponentsInChildren<RawImage>().Single().texture!=null,"review artwork");foreach(var label in root.GetComponentsInChildren<TMP_Text>())Check(!label.isTextOverflowing,"review text fits "+label.text);Check(CouncilActions.Int("iMoney")==gold,"no spending before confirmation");if(i==4){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"review-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);}root.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text=="Choose another gift").onClick.Invoke();yield return null;Check(GameObject.Find("Gift card 7")!=null,"back to gallery");}
 }
 }
}
#endif
