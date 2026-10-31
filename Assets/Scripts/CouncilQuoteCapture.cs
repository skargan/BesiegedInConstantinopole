#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

// Opt-in player verification; all archives and preparation fixtures remain in the capture folder.
public class CouncilQuoteCapture : MonoBehaviour
{
    string output;
    int passed, captionsChecked;
    readonly List<string> layoutRows = new List<string> { "width,height,id,characters,lines,font_size,overflow" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if (!Environment.GetCommandLineArgs().Contains("-council-quote-capture")) return;
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/CouncilQuotes"));
        Directory.CreateDirectory(root);
        SaveGames.TestRoot = Path.Combine(root, "Saves");
        Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath = Path.Combine(SaveGames.TestRoot, "checkpoint.besieged.json");
        var go = new GameObject("Council quotation verification");
        DontDestroyOnLoad(go);
        go.AddComponent<CouncilQuoteCapture>().output = root;
    }

    void Check(bool ok, string label)
    {
        if (!ok) throw new InvalidOperationException("COUNCIL QUOTES: " + label);
        passed++;
    }

    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output, "checks.txt"), "RUNNING: Council quotation verification");
        var work = new Stack<IEnumerator>();
        work.Push(Run());
        while (work.Count > 0) {
            object next = null;
            bool moved = false;
            Exception failure = null;
            try { moved = work.Peek().MoveNext(); if (moved) next = work.Peek().Current; }
            catch (Exception ex) { failure = ex; }
            if (failure != null) {
                File.WriteAllLines(Path.Combine(output, "caption-layout.csv"), layoutRows);
                File.WriteAllText(Path.Combine(output, "checks.txt"), "FAILED after " + passed + " checks; " + captionsChecked + " caption layouts examined.\n" + failure);
                Debug.LogError(failure);
                Application.Quit(1);
                yield break;
            }
            if (!moved) { work.Pop(); continue; }
            if (next is IEnumerator nested) work.Push(nested);
            else yield return next;
        }
        File.WriteAllLines(Path.Combine(output, "caption-layout.csv"), layoutRows);
        string summary = passed + " exported-player checks passed; " + captionsChecked + " quotation layouts examined (392 quotations at three resolutions).";
        File.WriteAllText(Path.Combine(output, "checks.txt"), summary);
        Debug.Log("COUNCIL_QUOTE_CHECKS " + summary);
        Application.Quit(0);
    }

    string State() => JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    string RandomState() => JsonUtility.ToJson(UnityEngine.Random.state);

    CouncilQuoteView View()
    {
        var view = FindObjectOfType<CouncilQuoteView>();
        Check(view != null && view.gameObject.name == "Council quotation", "Visible council quotation is absent");
        return view;
    }

    void CheckActual(CouncilQuoteView view)
    {
        view.Refresh();
        var expected = CouncilQuotations.ForTurn(SaveGames.CampaignId, VariableSingleton.Turn);
        Check(view.QuoteId == expected.id && view.GetComponent<TMP_Text>().text == expected.DisplayText,
            "Council caption does not match this campaign and turn");
    }

    IEnumerator WaitForCampaign(int previousInstance)
    {
        float deadline = Time.realtimeSinceStartup + 20;
        while (Time.realtimeSinceStartup < deadline) {
            if (VariableSingleton.IsInitialized && VariableSingleton.Instance != null &&
                VariableSingleton.Instance.GetInstanceID() != previousInstance && FindObjectOfType<ManagerUI>() != null &&
                FindObjectOfType<CouncilQuoteView>() != null) break;
            yield return null;
        }
        Check(VariableSingleton.IsInitialized && VariableSingleton.Instance != null &&
            VariableSingleton.Instance.GetInstanceID() != previousInstance && FindObjectOfType<ManagerUI>() != null,
            "Campaign did not initialise");
        yield return new WaitForSecondsRealtime(.3f);
        CampaignHUD.Instance.DismissReport();
        CheckActual(View());
    }

    void SelectorChecks()
    {
        string state = State(), rng = RandomState();
        DateTime date = VariableSingleton.GetDate();
        var entries = CouncilQuotations.Entries;
        int count = entries.Count;
        Check(count == 392, "Imported catalogue count differs from the 392 selected quotations");
        var known = new HashSet<string>(entries.Select(q => q.id));
        Check(known.Count == count, "Catalogue IDs are not unique");
        Check(entries.All(q => (q.labels ?? new string[0]).All(label => !q.displayAttribution.Contains(label))),
            "Editorial labels leaked into the simplified council attribution");
        string identity = "quote-fixture-a";
        var expected = new string[count * 3];
        for (int cycle = 0; cycle < 3; cycle++) {
            var seen = new HashSet<string>();
            for (int offset = 0; offset < count; offset++) {
                int turn = cycle * count + offset;
                var quote = CouncilQuotations.ForTurn(identity, turn);
                expected[turn] = quote.id;
                seen.Add(quote.id);
            }
            Check(seen.SetEquals(known), "Cycle " + cycle + " does not visit the complete catalogue exactly once");
        }
        Check(expected[count - 1] != expected[count] && expected[2 * count - 1] != expected[2 * count],
            "A quotation repeats immediately across a cycle boundary");
        foreach(int start in new[]{VariableSingleton.EffectiveSiegeTurn,count-1})
            Check(new HashSet<string>(Enumerable.Range(start,count).Select(t=>CouncilQuotations.ForTurn(identity,t).id)).Count==count,
                "Starting at a non-zero turn repeats a quotation before the whole deck has been used");
        Check(Enumerable.Range(0, count).All(i => CouncilQuotations.ForTurn(identity, i).id == expected[i]),
            "Repeated requests changed the sequence");
        Check(Enumerable.Range(0, 32).Any(i => CouncilQuotations.ForTurn("quote-fixture-b", i).id != expected[i]),
            "Different identities produced the same sampled sequence");
        int[] jumps = { count + 12, 0, 2 * count, count - 1, 8, 2 * count - 1, count, 1 };
        bool independent = true;
        foreach (int turn in jumps) {
            CouncilQuotations.ForTurn("another-campaign", turn);
            independent &= CouncilQuotations.ForTurn(identity, turn).id == expected[turn];
        }
        Check(independent, "Out-of-order requests or identity switches changed a known result");
        Check(CouncilQuotations.ForTurn(identity, -1).id == expected[0], "Negative preview turn did not clamp to the first turn");
        Check(RandomState() == rng, "Deck reads changed Unity gameplay randomness");
        Check(State() == state && VariableSingleton.GetDate() == date, "Deck reads changed campaign resources, time or state");
    }

    IEnumerator Run()
    {
        AudioListener.volume = 0;
        Application.runInBackground = true;
        yield return null;
        SaveMenu.StartCampaign(false);
        yield return WaitForCampaign(0);
        SelectorChecks();

        string identity = SaveGames.CampaignId;
        var baseline = VariableSingleton.Instance.CaptureState();
        string quoteId = View().QuoteId;
        var archive = SaveGames.Snapshot("Council quotation fixture");
        string archivePath = Path.Combine(SaveGames.TestRoot, "quote-campaign.besieged.json");
        SaveGames.AtomicWrite(archivePath, JsonUtility.ToJson(archive, true));
        var read = SaveGames.Read(archivePath);
        Check(SaveGames.Format == 2 && read.saveFormat == 2 && read.campaign.version == baseline.version && read.campaignId == identity,
            "Quotation integration changed the archive format or identity");

        int oldInstance = VariableSingleton.Instance.GetInstanceID();
        SaveMenu.StartCampaign(false);
        yield return WaitForCampaign(oldInstance);
        string secondIdentity = SaveGames.CampaignId;
        Check(secondIdentity != identity, "A new campaign reused the previous identity");
        Check(Enumerable.Range(0, 32).Any(i => CouncilQuotations.ForTurn(identity, i).id != CouncilQuotations.ForTurn(secondIdentity, i).id),
            "Actual new campaigns did not produce different sampled sequences");

        Check(SaveGames.Load(archivePath, out var loadError), "Archive load failed: " + loadError);
        yield return null;
        Check(SaveGames.CampaignId == identity && View().QuoteId == quoteId, "Loading the archive did not restore the original quotation");
        CheckActual(View());
        Check(VariableSingleton.Turn == baseline.turn && RandomState() == JsonUtility.ToJson(baseline.randomState),
            "Archive load did not restore the saved turn and gameplay random state");
        Check(baseline.ints.All(v => VariableSingleton.GetIntVariable(v.key) == v.value), "Archive load changed a saved resource or integer state");

        // A preparation preset is reused state, but Start at Siege is a distinct campaign.
        var preset = JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));
        preset.turn = VariableSingleton.EffectiveSiegeTurn;
        SaveGames.AtomicWrite(VariableSingleton.PreparationPresetPath, JsonUtility.ToJson(preset));
        oldInstance = VariableSingleton.Instance.GetInstanceID();
        SaveMenu.StartSiegeCampaign(false);
        yield return WaitForCampaign(oldInstance);
        string siegeIdentity = SaveGames.CampaignId;
        Check(siegeIdentity != identity && siegeIdentity != secondIdentity && VariableSingleton.Turn == preset.turn,
            "Starting from a preparation preset did not create a fresh campaign identity at the siege");
        CheckActual(View());
        Check(SaveGames.Load(archivePath, out loadError), "Could not restore original campaign after preset check: " + loadError);
        yield return null;
        var view = View();
        CheckActual(view);

        // Reopening the menu must retain the quotation and leave gameplay state alone.
        var manager = FindObjectOfType<ManagerUI>();
        string before = State();
        DateTime date = VariableSingleton.GetDate();
        manager.gameObject.SetActive(false);
        yield return null;
        manager.gameObject.SetActive(true);
        yield return null;
        CheckActual(view);
        Check(view.QuoteId == quoteId && State() == before && VariableSingleton.GetDate() == date,
            "Reopening the council changed its quotation or campaign state");
        before = State();
        for (int i = 0; i < 20; i++) view.Refresh();
        Check(State() == before, "Refreshing the quotation changed campaign state");

        // Move the fixture directly: ordinary turn processing legitimately changes resources.
        // This isolates the view's reaction to a changed turn from those gameplay systems.
        var next = JsonUtility.FromJson<VariableSingleton.CampaignSave>(State());
        next.turn++;
        VariableSingleton.Instance.RestoreValidated(next);
        before = State();
        date = VariableSingleton.GetDate();
        yield return null;
        CheckActual(view);
        Check(view.QuoteId != quoteId && State() == before && VariableSingleton.GetDate() == date,
            "New-turn quotation did not change cleanly");
        Check(SaveGames.Load(archivePath, out loadError), "Original campaign restore failed: " + loadError);
        yield return null;
        CheckActual(view);

        foreach (var size in new[] { new Vector2Int(1600, 900), new Vector2Int(1100, 760), new Vector2Int(1280, 720) }) {
            yield return Resize(size.x, size.y);
            yield return SweepCaptions(view, size.x, size.y);
        }
        yield return Resize(1600, 900);
        CheckActual(view);
        yield return Capture("council-actual-1600x900");
    }

    IEnumerator Resize(int width, int height)
    {
        Screen.SetResolution(width, height, false);
        yield return new WaitForSecondsRealtime(.6f);
        Canvas.ForceUpdateCanvases();
        Check(Screen.width == width && Screen.height == height, "Requested display dimensions were not applied");
    }

    IEnumerator SweepCaptions(CouncilQuoteView view, int width, int height)
    {
        var text = view.GetComponent<TMP_Text>();
        var entries = CouncilQuotations.Entries;
        string before = State(), originalText = text.text, originalId = view.QuoteId;
        bool enabled = view.enabled;
        view.enabled = false;
        var overflowing = new List<string>();
        CouncilQuotations.Quotation longest = entries.OrderByDescending(q => q.DisplayText.Length).First();
        CouncilQuotations.Quotation tallest = longest;
        int mostLines = -1;
        try {
            for (int i = 0; i < entries.Count; i++) {
                var quote = entries[i];
                text.text = quote.DisplayText;
                text.ForceMeshUpdate();
                captionsChecked++;
                if (text.isTextOverflowing) overflowing.Add(quote.id);
                if (text.textInfo.lineCount > mostLines) { mostLines = text.textInfo.lineCount; tallest = quote; }
                layoutRows.Add(width + "," + height + "," + quote.id + "," + quote.DisplayText.Length + "," + text.textInfo.lineCount + "," +
                    text.fontSize.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "," + text.isTextOverflowing);
                if (i % 32 == 31) yield return null;
            }
            text.text = longest.DisplayText;
            text.ForceMeshUpdate();
            yield return Capture("council-longest-" + width + "x" + height + "-" + longest.id);
            if (tallest.id != longest.id) {
                text.text = tallest.DisplayText;
                text.ForceMeshUpdate();
                yield return Capture("council-most-lines-" + width + "x" + height + "-" + tallest.id);
            }
            Check(overflowing.Count == 0, "Quotation overflow at " + width + "x" + height + ": " + string.Join(", ", overflowing));
        }
        finally {
            text.text = originalText;
            view.enabled = enabled;
            view.Refresh();
            text.ForceMeshUpdate();
        }
        Check(view.QuoteId == originalId && text.text == originalText && State() == before,
            "Caption layout sweep did not restore the live quotation or changed campaign state");
    }

    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.15f);
        Canvas.ForceUpdateCanvases();
        File.WriteAllLines(Path.Combine(output, name + "-text.txt"), FindObjectsOfType<TMP_Text>()
            .Select(t => t.name + " | " + t.rectTransform.rect + " | " + t.text));
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
