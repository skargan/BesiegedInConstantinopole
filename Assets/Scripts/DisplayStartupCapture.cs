#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

// Opt-in: exercises real startup without screen arguments, then both settings modes.
public class DisplayStartupCapture : MonoBehaviour
{
    bool hadPreference; int preference;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-display-startup-check"))return;
        var go=new GameObject("Display startup check");DontDestroyOnLoad(go);
        var test=go.AddComponent<DisplayStartupCapture>();
        test.hadPreference=PlayerPrefs.HasKey("Besieged.Fullscreen");test.preference=PlayerPrefs.GetInt("Besieged.Fullscreen",1);
        PlayerPrefs.SetInt("Besieged.Fullscreen",1);Application.runInBackground=true;
    }
    IEnumerator Start()
    {
        var settings=Settings.Instance;
        yield return new WaitForSecondsRealtime(2);
        int width=Display.main.systemWidth,height=Display.main.systemHeight;
        bool initial=Screen.fullScreenMode==FullScreenMode.FullScreenWindow&&Screen.width==width&&Screen.height==height;
        string report="Startup: "+Screen.width+"x"+Screen.height+" / desktop "+width+"x"+height+"; native fullscreen="+initial;
        settings.SetFullscreen(false);yield return new WaitForSecondsRealtime(1);
        bool window=!Screen.fullScreen&&Screen.width==Mathf.Min(1600,Mathf.RoundToInt(width*.85f))&&Screen.height==Mathf.Min(900,Mathf.RoundToInt(height*.85f));
        report+="\nWindowed dimensions correct="+window;
        settings.SetFullscreen(true);yield return new WaitForSecondsRealtime(1);
        bool back=Screen.fullScreenMode==FullScreenMode.FullScreenWindow&&Screen.width==width&&Screen.height==height;
        report+="\nFullscreen restoration correct="+back;
        if(hadPreference)PlayerPrefs.SetInt("Besieged.Fullscreen",preference);else PlayerPrefs.DeleteKey("Besieged.Fullscreen");PlayerPrefs.Save();
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/DisplayStartup"));Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder,"checks.txt"),(initial&&window&&back?"PASS\n":"FAILED\n")+report);
        Application.Quit(initial&&window&&back?0:1);
    }
}
#endif
