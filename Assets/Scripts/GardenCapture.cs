#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using UnityEngine;
public class GardenCapture:MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-garden-capture")<0)return;var go=new GameObject("Garden verification");DontDestroyOnLoad(go);go.AddComponent<GardenCapture>();}
    string output;
    IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return new WaitForSeconds(.7f);}
    IEnumerator Start(){
        AudioListener.volume=0;Application.runInBackground=true;
        output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures"));Directory.CreateDirectory(output);VariableSingleton.TestSavePath=Path.Combine(output,"test-save.json");
        yield return null;FindObjectOfType<MainMenuUI>().NewGame();yield return new WaitForSeconds(.5f);
        yield return Shot("council");var manager=FindObjectOfType<ManagerUI>();var c=CouncilUI.For(manager);
        VariableSingleton.ChangeBool("bCharWarrior0",true);c.Garden();yield return Shot("garden");
        typeof(CouncilUI).GetMethod("GardenTalk",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(c,new object[]{200,0});yield return Shot("kjold");
        c.ReviewFocus();yield return Shot("focus");
        c.IntelligenceBriefing();yield return Shot("intelligence");
        int passed=0;string report;
        VariableSingleton.ChangeBool("bCharDiplomat4",false);VariableSingleton.SetInt("iMoney",5000);VariableSingleton.SetInt("iFocusChanges",0);int next=(CouncilActions.Int("iFocus")+1)%5;
        if(!FocusReorganisation.Change(next,out report)||CouncilActions.Int("iMoney")!=0)throw new Exception("Focus transaction failed");passed++;
        VariableSingleton.SetInt("iMoney",10000);if(FocusReorganisation.Change((next+1)%5,out report))throw new Exception("Focus limit failed");passed++;
        if(!GardenConversations.Available(200))throw new Exception("Kjold absent at start");passed++;
        VariableSingleton.SetInt("iKjoldState",3);if(GardenConversations.Available(200))throw new Exception("Dead Kjold available");passed++;
        VariableSingleton.SetInt("iHardOfficerReturn0",TrainingPool.Day(VariableSingleton.GetDate())+2);if(GardenConversations.Available(0))throw new Exception("Wounded officer in garden");passed++;
        File.WriteAllText(Path.Combine(output,"checks.txt"),passed+" exported-player checks passed.");Application.Quit();
    }
}
#endif
