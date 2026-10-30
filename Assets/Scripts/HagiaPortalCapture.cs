#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// Development-player verification only; ordinary launches do not create this component.
public class HagiaPortalCapture : MonoBehaviour
{
    const string PalaceLabel = "Return to the Palace";
    const string GardenLabel = "Walk in the garden";
    static readonly Vector2 PalaceCentre = new Vector2(.145f, .40f);
    static readonly Vector2 GardenCentre = new Vector2(.81f, .35f);
    static readonly Vector2 PalaceCorner = new Vector2(.109f, .62f);
    static readonly Vector2 GardenCorner = new Vector2(.783f, .54f);
    string output;
    int passed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if (!Environment.GetCommandLineArgs().Contains("-hagia-portal-capture")) return;
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/HagiaPortals"));
        Directory.CreateDirectory(root);
        // Set these before the menu can import a legacy campaign or write a checkpoint.
        SaveGames.TestRoot = Path.Combine(root, "Saves");
        VariableSingleton.TestSavePath = Path.Combine(SaveGames.TestRoot, "checkpoint.besieged.json");
        var go = new GameObject("Hagia portal verification");
        DontDestroyOnLoad(go);
        go.AddComponent<HagiaPortalCapture>().output = root;
    }

    void Check(bool ok, string label)
    {
        if (!ok) throw new InvalidOperationException("HAGIA PORTALS: " + label);
        passed++;
    }

    LocationHotspot Find(string label)
    {
        var found = FindObjectsOfType<LocationHotspot>().FirstOrDefault(x => x.name == "Hotspot · " + label);
        Check(found != null, "Missing portal: " + label);
        return found;
    }

    PointerEventData Pointer(LocationHotspot portal, Vector2 point,
        PointerEventData.InputButton button = PointerEventData.InputButton.Left)
    {
        Rect r = portal.rectTransform.rect;
        Vector3 world = portal.rectTransform.TransformPoint(new Vector3(r.xMin + point.x * r.width, r.yMin + point.y * r.height));
        return new PointerEventData(EventSystem.current) {
            position = RectTransformUtility.WorldToScreenPoint(null, world), button = button
        };
    }

    void CheckRaycast(LocationHotspot portal, Vector2 centre, Vector2 outside)
    {
        Check(portal.Contains(centre), portal.name + " excludes its passage centre");
        Check(!portal.Contains(outside), portal.name + " includes the exterior arch corner");
        var point = Pointer(portal, centre);
        Check(portal.IsRaycastLocationValid(point.position, null), portal.name + " rejects its screen-space centre");
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(point, hits);
        Check(hits.Count > 0 && hits[0].gameObject == portal.gameObject, portal.name + " is blocked by another UI element");
        point = Pointer(portal, outside);
        Check(!portal.IsRaycastLocationValid(point.position, null), portal.name + " accepts an exterior screen-space point");
        hits.Clear();
        EventSystem.current.RaycastAll(point, hits);
        Check(!hits.Any(x => x.gameObject == portal.gameObject), portal.name + " receives exterior raycasts");
    }

    // Catch failures in nested capture coroutines too, so automation exits instead of hanging.
    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output, "checks.txt"), "RUNNING: Hagia portal verification");
        var work = new Stack<IEnumerator>();
        work.Push(Run());
        while (work.Count > 0) {
            object next = null;
            bool moved = false;
            Exception failure = null;
            try { moved = work.Peek().MoveNext(); if (moved) next = work.Peek().Current; }
            catch (Exception ex) { failure = ex; }
            if (failure != null) {
                File.WriteAllText(Path.Combine(output, "checks.txt"), "FAILED after " + passed + " checks.\n" + failure);
                Debug.LogError(failure);
                Application.Quit(1);
                yield break;
            }
            if (!moved) { work.Pop(); continue; }
            if (next is IEnumerator nested) work.Push(nested);
            else yield return next;
        }
        File.WriteAllText(Path.Combine(output, "checks.txt"), passed + " exported-player Hagia portal checks passed.");
        Debug.Log("HAGIA_PORTAL_CHECKS " + passed + " passed");
        Application.Quit(0);
    }

    IEnumerator Run()
    {
        AudioListener.volume = 0;
        Application.runInBackground = true;
        Screen.SetResolution(1600, 900, false);
        yield return null;
        // Bypass the interactive Normal/Ironman selector using the existing campaign API.
        SaveMenu.StartCampaign(false);
        float deadline = Time.realtimeSinceStartup + 15;
        while ((!VariableSingleton.IsInitialized || FindObjectOfType<ManagerUI>() == null) && Time.realtimeSinceStartup < deadline)
            yield return null;
        Check(VariableSingleton.IsInitialized && FindObjectOfType<ManagerUI>() != null, "Campaign did not initialise");
        yield return new WaitForSecondsRealtime(.5f);
        var council = CouncilUI.For(FindObjectOfType<ManagerUI>());
        VariableSingleton.ChangeFloat("fHealth", .5f - VariableSingleton.GetFloatVariable("fHealth"));
        int turn = VariableSingleton.Turn;
        DateTime date = VariableSingleton.GetDate();
        int gold = CouncilActions.Int("iMoney");
        int provisions = CouncilActions.Int("iProvisions");
        council.HagiaSophia();
        yield return null;
        Canvas.ForceUpdateCanvases();
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth") - .51f) < .0001f, "First Hagia visit must restore exactly one health point");
        Check(PalaceLocations.VisitedSanctuary, "Sanctuary visit was not recorded");
        yield return Capture("hagia-idle");

        var palace = Find(PalaceLabel);
        var garden = Find(GardenLabel);
        CheckRaycast(palace, PalaceCentre, PalaceCorner);
        CheckRaycast(garden, GardenCentre, GardenCorner);
        palace.OnPointerEnter(Pointer(palace, PalaceCentre));
        yield return Capture("hagia-left-hover");
        palace.OnPointerExit(Pointer(palace, PalaceCentre));
        garden.OnPointerEnter(Pointer(garden, GardenCentre));
        yield return Capture("hagia-right-hover");
        garden.OnPointerExit(Pointer(garden, GardenCentre));

        palace.OnPointerClick(Pointer(palace, PalaceCentre, PointerEventData.InputButton.Right));
        garden.OnPointerClick(Pointer(garden, GardenCentre, PointerEventData.InputButton.Right));
        yield return null;
        Check(Find(PalaceLabel) == palace && Find(GardenLabel) == garden, "Right-click navigated away from Hagia");
        palace.OnPointerClick(Pointer(palace, PalaceCorner));
        garden.OnPointerClick(Pointer(garden, GardenCorner));
        yield return null;
        Check(Find(PalaceLabel) == palace && Find(GardenLabel) == garden, "Click outside a passage navigated away from Hagia");

        palace.OnPointerClick(Pointer(palace, PalaceCentre));
        yield return null;
        Check(FindObjectsOfType<LocationHotspot>().Any(x => x.name == "Hotspot · Imperial affairs"), "Palace portal did not open the Palace");
        council.HagiaSophia();
        yield return null;
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth") - .51f) < .0001f, "Returning from the Palace healed a second time");
        garden = Find(GardenLabel);
        garden.OnPointerClick(Pointer(garden, GardenCentre));
        yield return null;
        Check(FindObjectsOfType<LocationHotspot>().Any(x => x.name == "Hotspot · Hagia Sophia") &&
            !FindObjectsOfType<LocationHotspot>().Any(x => x.name == "Hotspot · Imperial affairs"), "Garden portal did not open the garden");
        council.HagiaSophia();
        yield return null;
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth") - .51f) < .0001f, "Returning from the garden healed a second time");
        Check(VariableSingleton.Turn == turn && VariableSingleton.GetDate() == date, "Visiting consumed campaign time");
        Check(CouncilActions.Int("iMoney") == gold && CouncilActions.Int("iProvisions") == provisions, "Visiting spent resources");

        Screen.SetResolution(1100, 760, false);
        yield return new WaitForSecondsRealtime(.6f);
        Canvas.ForceUpdateCanvases();
        Check(Screen.width == 1100 && Screen.height == 760, "Player did not resize to 1100 by 760");
        palace = Find(PalaceLabel);
        garden = Find(GardenLabel);
        CheckRaycast(palace, PalaceCentre, PalaceCorner);
        CheckRaycast(garden, GardenCentre, GardenCorner);
        garden.OnPointerEnter(Pointer(garden, GardenCentre));
        yield return Capture("hagia-resized");
        garden.OnPointerExit(Pointer(garden, GardenCentre));
    }

    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.35f);
        Canvas.ForceUpdateCanvases();
        File.WriteAllLines(Path.Combine(output, name + "-text.txt"), FindObjectsOfType<TMPro.TMP_Text>()
            .Select(t => t.name + " | " + t.rectTransform.rect + " | " + t.text));
        // Explicitly render because a hidden Windows player can return a black back buffer.
        var cameraObject = new GameObject("Capture camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.06f, .07f, .08f);
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 100;
        var canvases = FindObjectsOfType<Canvas>().Where(c => c.isRootCanvas).ToArray();
        var modes = canvases.Select(c => c.renderMode).ToArray();
        var cameras = canvases.Select(c => c.worldCamera).ToArray();
        var distances = canvases.Select(c => c.planeDistance).ToArray();
        // Use the live dimensions so the resized screenshot tests the actual aspect ratio.
        int width = Screen.width, height = Screen.height;
        var target = new RenderTexture(width, height, 24);
        var shot = new Texture2D(width, height, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try {
            camera.targetTexture = target;
            foreach (var canvas in canvases) {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 10;
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            shot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            shot.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), shot.EncodeToPNG());
        }
        finally {
            RenderTexture.active = previous;
            for (int i = 0; i < canvases.Length; i++) {
                canvases[i].renderMode = modes[i];
                canvases[i].worldCamera = cameras[i];
                canvases[i].planeDistance = distances[i];
            }
            camera.targetTexture = null;
            target.Release();
            Destroy(target);
            Destroy(shot);
            Destroy(cameraObject);
            Canvas.ForceUpdateCanvases();
        }
        yield return null;
    }
}
#endif
