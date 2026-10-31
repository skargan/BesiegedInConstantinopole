#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Opt-in UI verification. Fixtures never execute a spy mission or alter real saves.
public sealed class IntelligenceMenuCapture : MonoBehaviour
{
    string output;
    int checks;
    CouncilUI council;
    VariableSingleton.CampaignSave baseline;
    static readonly string[] Tracked = { "iMoney", "iSecrets", "iSpies", "iCampAlert", "iEnemyDelay",
        "iTurkSpies", "iTurkCannons", "iFood", "iProvisions", "iMaterials",
        "iMehmedAssassinationFailed", "iMehmedAssassinationNextDay", "iMehmedAssassinationVictory" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if (!Environment.GetCommandLineArgs().Contains("-intelligence-capture")) return;
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/Intelligence"));
        Directory.CreateDirectory(root);
        SaveGames.TestRoot = Path.Combine(root, "Saves");
        Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath = Path.Combine(SaveGames.TestRoot, "checkpoint.besieged.json");
        var go = new GameObject("Intelligence menu verification");
        DontDestroyOnLoad(go);
        go.AddComponent<IntelligenceMenuCapture>().output = root;
    }

    void Check(bool ok, string label)
    {
        if (!ok) throw new InvalidOperationException("INTELLIGENCE UI: " + label);
        checks++;
    }

    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output, "checks.txt"), "RUNNING: intelligence menu verification");
        var work = new Stack<IEnumerator>();
        work.Push(Run());
        while (work.Count > 0) {
            object next = null;
            bool moved = false;
            Exception failure = null;
            try { moved = work.Peek().MoveNext(); if (moved) next = work.Peek().Current; }
            catch (Exception error) { failure = error; }
            if (failure != null) {
                File.WriteAllText(Path.Combine(output, "checks.txt"), "FAILED after " + checks + " checks.\n" + failure);
                Debug.LogException(failure);
                Application.Quit(1);
                yield break;
            }
            if (!moved) { work.Pop(); continue; }
            if (next is IEnumerator nested) work.Push(nested);
            else yield return next;
        }
        string result = "INTELLIGENCE_CHECKS " + checks + " passed";
        File.WriteAllText(Path.Combine(output, "checks.txt"), result);
        Debug.Log(result);
        Application.Quit(0);
    }

    GameObject Panel() => GameObject.Find("Council screen");
    GameObject Desk()
    {
        var desk = GameObject.Find("Intelligence desk");
        Check(desk != null, "Intelligence desk is missing");
        return desk;
    }
    string Text(Transform parent) => string.Join(" ", parent.GetComponentsInChildren<TMP_Text>().Select(t => t.text));
    string Normal(string value) => Regex.Replace(Regex.Replace(value ?? "", "<[^>]+>", ""), "\\s+", " ").Trim();
    Button Named(string name)
    {
        var button = Panel().GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == name);
        Check(button != null, "Missing button: " + name);
        return button;
    }
    Button Caption(string caption)
    {
        var button = Panel().GetComponentsInChildren<Button>().FirstOrDefault(b => Normal(Text(b.transform)) == caption);
        Check(button != null, "Missing button caption: " + caption);
        return button;
    }
    void Click(Button button)
    {
        Check(button.IsActive() && button.interactable, "Unavailable button clicked: " + button.name);
        button.onClick.Invoke();
    }
    void SelectPayment(bool secrets) => Click(Named(secrets ? "Spy payment secrets" : "Spy payment gold"));

    string Signature() => VariableSingleton.Turn + "|" + string.Join("|", Tracked.Select(k => k + "=" + CouncilActions.Int(k)))
        + "|fury=" + VariableSingleton.GetFloatVariable("fSultanRage")
        + "|morale=" + VariableSingleton.GetFloatVariable("fTroopRep");
    void Unchanged(string before, string reason)
    {
        Check(Signature() == before, reason + " changed resources, agents, alert, cooldown or calendar");
        Check(FindObjectOfType<RestUI>() == null, reason + " consumed the major action");
    }

    void Fixture(int spies, bool siege = true)
    {
        CampaignHUD.Instance.DismissReport();
        var state = JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));
        if (siege) state.turn = VariableSingleton.EffectiveSiegeTurn;
        VariableSingleton.Instance.RestoreValidated(state);
        MehmedAssassination.ResetForRestore();
        VariableSingleton.SetInt("iSpies", spies);
        VariableSingleton.SetInt("iMoney", 20000);
        VariableSingleton.SetInt("iSecrets", 10);
        VariableSingleton.SetInt("iTurkCannons", 16);
        VariableSingleton.SetInt("iTurkSpies", 4);
        VariableSingleton.SetInt("iFood", 90);
        VariableSingleton.SetInt("iCampAlert", 28);
        VariableSingleton.SetInt("iEnemyDelay", 3);
        council.Spies();
    }

    Rect Bounds(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        var points = corners.Select(p => RectTransformUtility.WorldToScreenPoint(null, p)).ToArray();
        return Rect.MinMaxRect(points.Min(p => p.x), points.Min(p => p.y), points.Max(p => p.x), points.Max(p => p.y));
    }
    bool Inside(Rect outer, Rect inner, float tolerance = 2) => inner.xMin >= outer.xMin - tolerance &&
        inner.yMin >= outer.yMin - tolerance && inner.xMax <= outer.xMax + tolerance && inner.yMax <= outer.yMax + tolerance;
    bool Overlap(Rect a, Rect b) => Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin) > 2 &&
        Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin) > 2;
    void CheckHit(Button button)
    {
        var pointer = new PointerEventData(EventSystem.current) { position = Bounds(button.GetComponent<RectTransform>()).center };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button, button.name + " is obscured at its centre");
    }
    void CheckText()
    {
        Canvas.ForceUpdateCanvases();
        foreach (var text in Panel().GetComponentsInChildren<TMP_Text>()) {
            text.ForceMeshUpdate();
            Check(!text.isTextOverflowing, "Text overflows: " + text.name + " | rect=" + text.rectTransform.rect.size
                + " | fontSize=" + text.fontSize + " | " + text.text);
        }
    }
    void CheckDesk()
    {
        Canvas.ForceUpdateCanvases();
        Rect bounds = Bounds(Desk().GetComponent<RectTransform>());
        Check(Inside(new Rect(0, 0, Screen.width, Screen.height), bounds), "Desk extends outside screen");
        Check(!Panel().GetComponentsInChildren<ScrollRect>().Any(s => s.isActiveAndEnabled), "Compact desk still requires scrolling");
        Check(!Panel().GetComponentsInChildren<Scrollbar>().Any(s => s.isActiveAndEnabled), "Old scrollbar remains active");
        foreach (var button in Desk().GetComponentsInChildren<Button>()) {
            Check(Inside(bounds, Bounds(button.GetComponent<RectTransform>())), "Control extends outside desk: " + button.name);
            if (button.interactable) CheckHit(button);
        }
        CheckText();
    }
    void CheckReports(int spies)
    {
        CheckDesk();
        var desk = Desk();
        var ledger = desk.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Intelligence ledger");
        Check(ledger != null, "Missing intelligence ledger");
        var bounds = Bounds(ledger.GetComponent<RectTransform>());
        var rows = ledger.GetComponentsInChildren<RectTransform>().Where(t => t.name.StartsWith("Intelligence row ")).ToArray();
        Check(rows.Length == 10, "Ledger does not show all ten intelligence levels");
        var rowBounds = new List<Rect>();
        for (int i = 0; i < 10; i++) {
            var row = rows.FirstOrDefault(t => t.name == "Intelligence row " + i);
            Check(row != null, "Missing intelligence level " + (i + 1));
            Rect rect = Bounds(row);
            Check(Inside(bounds, rect), "Intelligence level " + (i + 1) + " is clipped");
            string text = Normal(Text(row));
            string expected = spies > i ? EspionageSystem.IntelligenceValue(i) : "Requires " + (i + 1) + " agents";
            Check(text.Contains(Normal(expected)), "Level " + (i + 1) + " missing full current value: " + expected);
            // The troop catalogue may be browsed at zero agents; it hides actual numbers itself.
            if (spies <= i && i != 0) Check(!row.GetComponentsInChildren<Button>().Any(b => b.interactable), "Locked level has an active action: " + (i + 1));
            rowBounds.Add(rect);
        }
        for (int i = 0; i < rowBounds.Count; i++)
            for (int j = i + 1; j < rowBounds.Count; j++) Check(!Overlap(rowBounds[i], rowBounds[j]), "Intelligence rows overlap");
        Check(Named("Intelligence assassination").interactable == (spies >= 10), "Assassination review unlock is stale");
    }
    bool ExpectedMission(int mission, bool secrets) => (mission == 0 || CouncilActions.Int("iSpies") > 0)
        && CouncilActions.Int(secrets ? "iSecrets" : "iMoney") >= (secrets ? 1 : 1000)
        && (mission != 1 || CouncilActions.Int("iTurkCannons") > 0)
        && (mission != 2 || CouncilActions.Int("iFood") > 0)
        && (mission != 3 || CouncilActions.Int("iTurkSpies") > 0);
    void CheckOperations(bool secrets)
    {
        CheckDesk();
        var desk = Desk();
        var cards = desk.GetComponentsInChildren<RectTransform>().Where(t => Regex.IsMatch(t.name, "^Spy mission [0-5]$")).ToArray();
        Check(cards.Length == 6, "Operations do not show all six missions");
        var bounds = new List<Rect>();
        for (int i = 0; i < 6; i++) {
            var card = cards.FirstOrDefault(t => t.name == "Spy mission " + i);
            Check(card != null, "Missing mission " + i);
            Rect cardBounds = Bounds(card);
            Check(Inside(Bounds(desk.GetComponent<RectTransform>()), cardBounds), "Mission card is clipped: " + i);
            var button = Named("Review spy mission " + i);
            Check(Inside(cardBounds, Bounds(button.GetComponent<RectTransform>())), "Mission control escapes card: " + i);
            Check(button.interactable == ExpectedMission(i, secrets), "Wrong mission availability: " + i);
            string text = Normal(Text(card));
            Check(text.Contains(EspionageSystem.LossChance(i).ToString("0.#") + "%"), "Current agent-loss risk missing: " + i);
            string cost = secrets ? "1 secret" : "1,000 gold";
            Check(Normal(Text(desk.transform)).IndexOf(cost, StringComparison.OrdinalIgnoreCase) >= 0, "Selected cost missing from payment controls");
            if (button.interactable) Check(text.IndexOf(cost, StringComparison.OrdinalIgnoreCase) >= 0, "Exact selected cost missing: " + i);
            bounds.Add(cardBounds);
        }
        for (int i = 0; i < bounds.Count; i++)
            for (int j = i + 1; j < bounds.Count; j++) Check(!Overlap(bounds[i], bounds[j]), "Mission cards overlap");
    }

    IEnumerator Resize(int width, int height)
    {
        Screen.SetResolution(width, height, false);
        yield return new WaitForSecondsRealtime(.6f);
        Check(Screen.width == width && Screen.height == height, "Requested display size not applied");
    }
    IEnumerator Operations(bool secrets)
    {
        if (GameObject.Find("Intelligence ledger") != null) {
            Click(Named("Intelligence operations tab"));
            yield return null;
        }
        SelectPayment(secrets);
        yield return null;
    }

    IEnumerator Run()
    {
        AudioListener.volume = 0;
        Application.runInBackground = true;
        yield return null;
        Check(SaveGames.Automated, "Test saves were not isolated before loading");
        SaveMenu.StartCampaign(false);
        float deadline = Time.realtimeSinceStartup + 15;
        while ((!VariableSingleton.IsInitialized || FindObjectOfType<ManagerUI>() == null) && Time.realtimeSinceStartup < deadline) yield return null;
        Check(VariableSingleton.IsInitialized && FindObjectOfType<ManagerUI>() != null, "Campaign did not initialise");
        yield return new WaitForSecondsRealtime(.5f);
        council = CouncilUI.For(FindObjectOfType<ManagerUI>());
        baseline = VariableSingleton.Instance.CaptureState();

        foreach (var size in new[] { new Vector2Int(1600, 900), new Vector2Int(1280, 720) }) {
            yield return Resize(size.x, size.y);
            string suffix = size.x + "x" + size.y;
            Fixture(1);
            yield return null;
            yield return Capture("reports-one-agent-" + suffix);
            CheckReports(1);
            Fixture(10);
            yield return null;
            yield return Capture("reports-full-network-" + suffix);
            CheckReports(10);
            string before = Signature();
            yield return Operations(false);
            yield return Capture("operations-gold-" + suffix);
            CheckOperations(false);
            yield return Operations(true);
            yield return Capture("operations-secrets-" + suffix);
            CheckOperations(true);
            Click(Named("Intelligence reports tab"));
            yield return null;
            CheckReports(10);
            Unchanged(before, "Tab and payment switching");
            VariableSingleton.SetInt("iSpies", 1);
            council.Spies();
            yield return null;
            CheckReports(1);
        }

        // All ordinary reviews and the separate disruption review cancel without paying.
        Fixture(10);
        yield return null;
        for (int i = 0; i < 6; i++) {
            yield return Operations(i % 2 == 1);
            string before = Signature();
            Click(Named("Review spy mission " + i));
            yield return null;
            var confirm = Named(i == 5 ? "Confirm spy disruption" : "Confirm spy mission " + i);
            Check(confirm.interactable, "Affordable mission cannot be confirmed: " + i);
            CheckText();
            Unchanged(before, "Opening mission " + i + " review");
            Click(i == 5 ? Caption("Return") : Named("Cancel spy mission"));
            yield return null;
            Unchanged(before, "Cancelling mission " + i);
        }

        Fixture(0);
        yield return null;
        CheckReports(0);
        yield return Operations(false);
        yield return Capture("operations-no-agents");
        CheckOperations(false);
        Fixture(10);
        VariableSingleton.SetInt("iMoney", 0);
        council.Spies();
        yield return null;
        yield return Operations(false);
        CheckOperations(false);
        yield return Operations(true);
        CheckOperations(true);
        VariableSingleton.SetInt("iSecrets", 0);
        yield return Operations(true);
        yield return Capture("operations-no-funds");
        CheckOperations(true);
        VariableSingleton.SetInt("iMoney", 20000);
        VariableSingleton.SetInt("iTurkCannons", 0);
        VariableSingleton.SetInt("iFood", 0);
        VariableSingleton.SetInt("iTurkSpies", 0);
        yield return Operations(false);
        yield return Capture("operations-no-targets");
        CheckOperations(false);

        Fixture(10);
        VariableSingleton.SetInt("iMehmedAssassinationFailed", 1);
        VariableSingleton.SetInt("iCampAlert", 0);
        council.Spies();
        yield return null;
        Check(EspionageSystem.Alert == 50 && EspionageSystem.AlertFloor == 50, "Permanent alert floor was lost");
        string summary = Text(Desk().transform);
        Check(summary.Contains("50%") && (summary.IndexOf("minimum", StringComparison.OrdinalIgnoreCase) >= 0 || summary.IndexOf("floor", StringComparison.OrdinalIgnoreCase) >= 0), "Permanent alert minimum is not visible");
        yield return Capture("reports-permanent-alert-floor");
        CheckReports(10);
        yield return Operations(false);
        CheckOperations(false);

        Fixture(10);
        yield return null;
        string original = Signature();
        Click(Named("Intelligence assassination"));
        yield return null;
        Check(Caption("Confirm attempt").interactable, "Siege assassination unavailable with ten agents");
        Click(Caption("Cancel"));
        yield return null;
        Unchanged(original, "Assassination review and cancellation");
        VariableSingleton.SetInt("iMehmedAssassinationNextDay", TrainingPool.Day(VariableSingleton.GetDate()) + 30);
        council.Spies();
        yield return null;
        Click(Named("Intelligence assassination"));
        yield return null;
        Check(!Caption("Confirm attempt").interactable, "Assassination cooldown ignored");
        Click(Caption("Cancel"));
        yield return null;
        Fixture(10, false);
        yield return null;
        Check(!VariableSingleton.HasSiegeStarted(), "Preparation fixture unexpectedly under siege");
        Click(Named("Intelligence assassination"));
        yield return null;
        Check(!Caption("Confirm attempt").interactable, "Assassination enabled before siege");
        Click(Caption("Cancel"));
        yield return null;

        // Preserve the troop breakdown route without executing any mission.
        Fixture(1);
        yield return null;
        var rowZero = Desk().GetComponentsInChildren<Transform>().First(t => t.name == "Intelligence row 0");
        var troopLink = rowZero.GetComponentsInChildren<Button>().FirstOrDefault(b => b.interactable);
        Check(troopLink != null, "Troop breakdown route missing");
        original = Signature();
        Click(troopLink);
        yield return null;
        Check(Text(Panel().transform).Contains("The Ottoman field army"), "Troop link did not open breakdown");
        ControlSingleton.Instance.GoBack();
        yield return null;
        Unchanged(original, "Troop breakdown browsing");
        Check(Panel().GetComponentsInChildren<Button>().Any(b => Text(b.transform).Contains("War Beyond the Walls")), "Provincial map route missing");
        Check(Panel().GetComponentsInChildren<Button>().Any(b => Text(b.transform).Contains("Specialists")), "Specialist route missing");
    }

    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.2f);
        Canvas.ForceUpdateCanvases();
        File.WriteAllLines(Path.Combine(output, name + "-text.txt"), FindObjectsOfType<TMP_Text>().Select(t => t.name + " | " + t.rectTransform.rect + " | " + t.text));
        var cameraObject = new GameObject("Intelligence capture camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true; camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = ImperialTheme.Navy; camera.nearClipPlane = .1f; camera.farClipPlane = 100;
        var canvases = FindObjectsOfType<Canvas>().Where(c => c.isRootCanvas).ToArray();
        var modes = canvases.Select(c => c.renderMode).ToArray();
        var cameras = canvases.Select(c => c.worldCamera).ToArray();
        var distances = canvases.Select(c => c.planeDistance).ToArray();
        var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        var previous = RenderTexture.active;
        Texture2D shot = null;
        try {
            camera.targetTexture = target;
            foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10; }
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
            shot = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); shot.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), shot.EncodeToPNG());
        } finally {
            RenderTexture.active = previous;
            for (int i = 0; i < canvases.Length; i++) { canvases[i].renderMode = modes[i]; canvases[i].worldCamera = cameras[i]; canvases[i].planeDistance = distances[i]; }
            camera.targetTexture = null; RenderTexture.ReleaseTemporary(target);
            if (shot != null) Destroy(shot); Destroy(cameraObject); Canvas.ForceUpdateCanvases();
        }
        yield return null;
    }
}
#endif
