using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PrototypeLauncher
{
    private const string PrototypeScene = "Assets/Scenes/Continue Game.unity";

    [MenuItem("Besieged/Open Main Menu")]
    public static void OpenMainMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);
    }

    [MenuItem("Besieged/Open Prototype Scene")]
    public static void OpenPrototypeScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PrototypeScene) == null)
        {
            Debug.LogError("Prototype scene not found: " + PrototypeScene);
            return;
        }

        // Preserve any scene edits before switching to the prototype.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.OpenScene(PrototypeScene, OpenSceneMode.Single);
        Debug.Log("Prototype scene opened. Press Play to test; press Play again to stop.");
    }

    [MenuItem("Besieged/Open Prototype Scene", true)]
    private static bool CanOpenPrototypeScene()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
