// Editor-only launcher. It neither changes Yarn nor adds code to player builds.
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BesiegedEventMap
{
    [MenuItem("Tools/Besieged/Event Dependency Map")]
    public static void Open()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string page = Path.Combine(projectRoot, "Tools", "BesiegedEventMap", "Besieged Event Map.html");
        if (!File.Exists(page))
        {
            EditorUtility.DisplayDialog("Besieged Event Map", "Map not found at:\n" + page + "\n\nCopy the supplied Tools/BesiegedEventMap folder into this Unity project.", "OK");
            return;
        }
        Application.OpenURL(new Uri(page).AbsoluteUri);
    }

    [MenuItem("Tools/Besieged/Event Map Instructions")]
    public static void Instructions()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string readme = Path.Combine(projectRoot, "Tools", "BesiegedEventMap", "README.md");
        if (File.Exists(readme)) EditorUtility.RevealInFinder(readme);
    }
}
#endif
