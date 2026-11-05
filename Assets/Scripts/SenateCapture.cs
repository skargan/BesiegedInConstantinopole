#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class SenateCapture:MonoBehaviour
{
    string folder;int passed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-senate-capture"))return;var path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Senate"));Directory.CreateDirectory(path);SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");var go=new GameObject("Senate verification");DontDestroyOnLoad(go);go.AddComponent<SenateCapture>().folder=path;}
    void Check(bool ok,string note){if(!ok)throw new Exception(note);passed++;}
    IEnumerator Start(){var run=Run();while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception ex){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+passed+"\n"+ex);Application.Quit(1);yield break;}if(!more)break;yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),"SENATE_CHECKS "+passed+" passed");Application.Quit(0);}
    IEnumerator Run(){Application.runInBackground=true;yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();var council=FindObjectOfType<CouncilUI>(true);Check(council!=null,"council exists");int gold=CouncilActions.Int("iMoney");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);council.CivicRelations();yield return null;
            for(int i=0;i<5;i++){
                GameObject.Find("Senate delegation "+i).GetComponent<Button>().onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();
                Check(GameObject.Find("Senate painting").GetComponent<Image>().sprite!=null,"art loaded");
                Check(GameObject.Find("Senate painting").GetComponentsInChildren<LocationHotspot>().Length==5,"five art targets");
                var detail=GameObject.Find("Senate selected delegation");foreach(var label in detail.GetComponentsInChildren<TMP_Text>())Check(!label.isTextOverflowing,"detail fits "+label.text);
                Check(CouncilActions.Int("iMoney")==gold,"selection does not spend");
                if(i==0||i==4){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"senate-"+i+"-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);}
                GameObject.Find("Senate review action").GetComponent<Button>().onClick.Invoke();yield return null;Check(GameObject.Find("Confirm society action")!=null,"existing confirmation reached");Check(CouncilActions.Int("iMoney")==gold,"review does not spend");council.CivicRelations();yield return null;
            }
            ControlSingleton.Instance.GoBack();yield return null;Check(GameObject.Find("Senate scene")==null,"return leaves senate");
        }
    }
}
#endif
