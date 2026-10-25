using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEditor.Events;

using UnityEngine;

using UnityEngine.UI;

using UnityEngine.SceneManagement;

using TMPro;

using Yarn.Unity;

using System.Linq;

using System.IO;



public static class ConfigurePlaytest

{

    private static T Find<T>() where T : Object { return Object.FindObjectOfType<T>(true); }

    private static GameObject Named(string name)

    {

        return Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)

            .First(t => t.gameObject.scene.IsValid() && t.name == name).gameObject;

    }

    private static void Bind(Object target, string property, Object value)

    {

        var serialized = new SerializedObject(target);

        serialized.FindProperty(property).objectReferenceValue = value;

        serialized.ApplyModifiedPropertiesWithoutUndo();

    }

    private static void Rewire(Button button, UnityEngine.Events.UnityAction action)

    {

        while (button.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(button.onClick, 0);

        UnityEventTools.AddPersistentListener(button.onClick, action);

    }

    private static void Caption(GameObject obj, string value)

    {

        var label = obj.GetComponentInChildren<TMP_Text>(true);

        if (label != null) { label.text = value; label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = 30; }

    }

    private static void Rect(GameObject obj, Vector2 min, Vector2 max)

    {

        var rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = min; rect.anchorMax = max;

        rect.offsetMin = rect.offsetMax = Vector2.zero;

        rect.localScale = Vector3.one;

    }

    public static void Run()

    {

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");

        var menu = Find<MainMenuUI>();

        var panel = menu.gameObject;

        Rect(panel, new Vector2(.56f,.15f), new Vector2(.93f,.78f));

        string[] names = { "ButtonNew", "ButtonContinue", "ButtonOptions", "ButtonQuit" };

        string[] labels = { "New campaign", "Continue checkpoint", "How to play", "Quit" };

        for (int i = 0; i < names.Length; i++)

        {

            var obj = Named(names[i]);

            Caption(obj, labels[i]);

            Rect(obj, new Vector2(.04f,.73f-i*.22f), new Vector2(.96f,.91f-i*.22f));

        }

        var canvas = Find<Canvas>();

        CampaignHUD.Label(canvas.transform, "BESIEGED IN CONSTANTINOPLE",

            new Vector2(.07f,.82f), new Vector2(.93f,.94f), 48);

        var help = CampaignHUD.Panel(canvas.transform, "How To Play", new Vector2(.15f,.14f), new Vector2(.85f,.82f));

        CampaignHUD.Label(help.transform,

            "Defend the city against the odds.\n\nEach turn: choose your management action, hear an event, then face the enemy. Repairs and rest use your major action.\n\nPrepare your walls and army before the siege. During assaults, spend your irreplaceable Varangians to hold a breach, or retreat to the next wall. Losing the final wall ends the campaign.\n\nSpace / Enter: advance narrative text. Escape: back or campaign menu. Checkpoints resume at the start of management.",

            new Vector2(.06f,.2f), new Vector2(.94f,.94f), 27);

        var close = CampaignHUD.MakeButton(help.transform, "Back", new Vector2(.35f,.04f), new Vector2(.65f,.15f), () => {});

        UnityEventTools.AddBoolPersistentListener(close.onClick, help.SetActive, false);

        help.SetActive(false);

        Bind(menu, "optionsDialogue", help);

        EditorSceneManager.SaveScene(scene);



        scene = EditorSceneManager.OpenScene("Assets/Scenes/Continue Game.unity");

        var manager = Find<ManagerUI>();

        var variables = Find<VariableSingleton>();

        variables.gameObject.AddComponent<CampaignHUD>();

        var runner = Find<DialogueRunner>();

        runner.startAutomatically = false;

        var variableSettings = new SerializedObject(variables);

        // Use all authored numbered story events; branching nodes remain linked from their parent.

        var story = variableSettings.FindProperty("possibleStory");

        story.arraySize = 25;

        for (int i = 0; i < 25; i++) story.GetArrayElementAtIndex(i).intValue = i;

        variableSettings.ApplyModifiedPropertiesWithoutUndo();



        // Keep incomplete subsystems visible, with honest labels instead of dead navigation.

        foreach (var entry in new[] { ("ButtonToDiplomacy","Diplomacy — coming later"), ("ButtonToSpy","Spies — coming later"), ("ButtonToPeople","Factions — coming later") })

        {

            var obj = Named(entry.Item1);

            Caption(obj, entry.Item2);

            obj.GetComponent<Button>().interactable = false;

        }

        Caption(Named("ButtonToTroops"), "Recruit troops");

        Caption(Named("ButtonToSiege"), "Inspect / repair walls");

        Caption(manager.GetComponentsInChildren<Button>(true).First(b=>b.name=="ButtonToContinue").gameObject, "Rest — end turn");

        var defense = Find<DefenseActions>();

        Rewire(Named("Repair").GetComponent<Button>(), defense.RepairSelected);

        Rewire(Named("Abandon").GetComponent<Button>(), defense.AbandonSelected);

        Caption(Named("Repair"), "Repair — end turn");

        Caption(Named("Abandon"), "Abandon wall — end turn");

        var recruit = Find<RecruitationSelection>();

        Rewire(Named("Buy Button").GetComponent<Button>(), recruit.CommitRecruitmentTransaction);

        var assault = Find<AssaultUI>();

        Rewire(Named("ButtonToResult").GetComponent<Button>(), assault.ShowResult);

        Named("TOBEIMPLEMENTED half").SetActive(false);

        assault.enemyActionText = CampaignHUD.Label(assault.transform, "",

            new Vector2(.1f,.72f), new Vector2(.9f,.87f), 28);



        // Remove old duplicate action forms whose approval buttons never changed game state.

        foreach (var name in new[] { "ButtonTrain", "ButtonRepair" }) Named(name).SetActive(false);

        foreach (var toggle in Object.FindObjectsByType<UITogglableButtons>(FindObjectsInactive.Include, FindObjectsSortMode.None))

            toggle.gameObject.SetActive(false);



        // The existing phase layout is authored for 1920x1080.

        var scale = Find<CanvasScaler>();

        scale.referenceResolution = new Vector2(1920,1080);

        scale.matchWidthOrHeight = .5f;

        var top = Named("Panel");

        Rect(top, new Vector2(0,0), new Vector2(1,.88f));

        foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))

            text.raycastTarget = false;

        EditorSceneManager.SaveScene(scene);

        EditorBuildSettings.scenes = new[] {

            new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),

            new EditorBuildSettingsScene("Assets/Scenes/Continue Game.unity", true)

        };

        PlayerSettings.defaultScreenWidth = 1600;

        PlayerSettings.defaultScreenHeight = 900;

        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;

        AssetDatabase.SaveAssets();

        Debug.Log("PLAYTEST_CONFIGURATION_COMPLETE");

    }



    public static void Build()

    {

        ValidateStory.Run();

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {

            scenes = new[] { "Assets/Scenes/Menu.unity", "Assets/Scenes/Continue Game.unity" },

            locationPathName = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-disruption-build") >= 0 ? "Builds/Version-33/Besieged.exe" : System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-current-build") >= 0 ? "Builds/Windows-MidnightMosaic/Besieged.exe" : System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-military-build") >= 0 ? "Builds/Windows-MilitaryClarity/Besieged.exe" : System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-review-build") >= 0 ? "Builds/Windows-ActionReview/Besieged.exe" : System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-spy-build") >= 0 ? "Builds/Windows-Espionage/Besieged.exe" : System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-typography-build") >= 0
                ? "Builds/Windows-Typography/Besieged.exe" : "Builds/Windows-MidnightMosaic/Besieged.exe",

            target = BuildTarget.StandaloneWindows64,

            options = BuildOptions.Development

        });

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)

            throw new System.Exception("Build failed: " + report.summary.result);

        Debug.Log("PLAYTEST_BUILD_COMPLETE");

    }

}

