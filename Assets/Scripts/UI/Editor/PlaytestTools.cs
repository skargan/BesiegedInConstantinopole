using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
public static class PlaytestTools
{
    public static void Audit()
    {
        var text = new StringBuilder();
        foreach (var path in new[] { "Assets/Scenes/Menu.unity", "Assets/Scenes/Continue Game.unity" })
        {
            var scene = EditorSceneManager.OpenScene(path);
            text.AppendLine("SCENE " + path);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var components = t.GetComponents<MonoBehaviour>();
                text.AppendLine("OBJECT " + t.name + " active=" + t.gameObject.activeSelf);
                foreach (var component in components)
                {
                    if (component == null) { text.AppendLine(" MISSING SCRIPT"); continue; }
                    if (component.GetType().Namespace == null)
                        text.AppendLine("  " + component.GetType().Name + " " + EditorJsonUtility.ToJson(component));
                }
                var button = t.GetComponent<Button>();
                if (button != null)
                {
                    for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                        text.AppendLine(" BUTTON " + t.name + " => " + button.onClick.GetPersistentTarget(i) + "." + button.onClick.GetPersistentMethodName(i));
                }
            }
        }
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/scene-audit.txt", text.ToString());
    }
}
