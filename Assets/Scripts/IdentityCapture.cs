#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
public sealed class IdentityCapture : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if (!Environment.GetCommandLineArgs().Contains("-identity-capture")) return;
        Application.runInBackground = true;
        var go = new GameObject("Identity verification"); DontDestroyOnLoad(go); go.AddComponent<IdentityCapture>();
    }
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        Canvas.ForceUpdateCanvases();
        var texts = FindObjectsOfType<TMP_Text>();
        var heading = texts.SingleOrDefault(t => t.text == GameIdentity.MenuTitle);
        if (Application.productName != GameIdentity.Title || heading == null || heading.isTextOverflowing ||
            texts.Any(t => t.text.ToUpperInvariant().Contains("BESIEGED IN CONSTANTINOPLE")))
        { Debug.LogError("IDENTITY_CHECK_FAILED: product/title/overflow"); Application.Quit(1); yield break; }
        string folder = Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures")); Directory.CreateDirectory(folder);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"identity-"+Screen.width+"x"+Screen.height+".png"));
        Debug.Log("IDENTITY_CHECK_PASSED: product name, visible title, no overflow, no old title");
        yield return new WaitForSeconds(2); Application.Quit(0);
    }
}
#endif
