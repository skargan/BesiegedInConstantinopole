#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Opt-in exported-player verification; normal launches never run this fixture.
public class SultanAudienceCapture : MonoBehaviour
{
    string output;
    int passed;
    CouncilUI council;
    VariableSingleton.CampaignSave baseline;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if (!Environment.GetCommandLineArgs().Contains("-sultan-audience-capture")) return;
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/SultanAudience"));
        Directory.CreateDirectory(root);
        SaveGames.TestRoot = Path.Combine(root, "Saves");
        Directory.CreateDirectory(SaveGames.TestRoot);
        VariableSingleton.TestSavePath = Path.Combine(SaveGames.TestRoot, "checkpoint.besieged.json");
        var go = new GameObject("Sultan audience verification");
        DontDestroyOnLoad(go);
        go.AddComponent<SultanAudienceCapture>().output = root;
    }

    void Check(bool ok, string label)
    {
        if (!ok) throw new InvalidOperationException("SULTAN AUDIENCE: " + label);
        passed++;
    }

    IEnumerator Start()
    {
        File.WriteAllText(Path.Combine(output, "checks.txt"), "RUNNING: Sultan audience verification");
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
        File.WriteAllText(Path.Combine(output, "checks.txt"), passed + " exported-player Sultan audience checks passed.");
        Debug.Log("SULTAN_AUDIENCE_CHECKS " + passed + " passed");
        Application.Quit(0);
    }

    GameObject Audience()
    {
        var region = GameObject.Find("Sultan audience");
        Check(region != null, "Audience region is absent");
        return region;
    }

    GameObject ScreenPanel()
    {
        var panel = GameObject.Find("Council screen");
        Check(panel != null, "Council screen is absent");
        return panel;
    }

    Button Choice(int index)
    {
        var result = Audience().GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "Sultan choice " + index);
        Check(result != null, "Choice " + index + " is absent");
        return result;
    }

    string Caption(Button button) => string.Join(" ", button.GetComponentsInChildren<TMP_Text>().Select(t => t.text));

    Button LabelButton(string label)
    {
        var button = ScreenPanel().GetComponentsInChildren<Button>().FirstOrDefault(b => Caption(b) == label);
        Check(button != null, "Missing button: " + label);
        return button;
    }

    void Click(Button button)
    {
        Check(button != null && button.IsActive() && button.interactable, "Attempt to click unavailable button");
        button.onClick.Invoke();
    }

    void Fixture(bool siege)
    {
        CampaignHUD.Instance.DismissReport();
        var state = JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline));
        if (siege) state.turn = VariableSingleton.EffectiveSiegeTurn;
        VariableSingleton.Instance.RestoreValidated(state);
        foreach (int i in new[] { 0, 1, 3, 4 }) VariableSingleton.SetInt("iSultanVisit" + i, 0);
        for (int i = 0; i < 3; i++) {
            VariableSingleton.SetInt("iSultanSeen" + i, 0);
            VariableSingleton.SetInt("iSultanTalk" + i, 0);
        }
        VariableSingleton.SetInt("iSultanConversationDay", 0);
        VariableSingleton.SetInt("iTermsDay", 0);
        VariableSingleton.SetInt("iSurrenderEnding", 0);
        VariableSingleton.SetInt("iMoney", 20000);
        VariableSingleton.SetInt("iProvisions", 1000);
        VariableSingleton.SetInt("iMaterials", 1000);
        VariableSingleton.SetInt("iSecrets", 10);
        VariableSingleton.ChangeFloat("fSultanRage", .2f - VariableSingleton.GetFloatVariable("fSultanRage"));
        VariableSingleton.ChangeFloat("fTroopRep", .5f - VariableSingleton.GetFloatVariable("fTroopRep"));
        VariableSingleton.ChangeFloat("fPeopleRep", .5f - VariableSingleton.GetFloatVariable("fPeopleRep"));
        council.Sultan();
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

    bool Overlaps(Rect a, Rect b) => Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin) > 2 &&
        Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin) > 2;

    void CheckHit(Button button)
    {
        var pointer = new PointerEventData(EventSystem.current) { position = Bounds(button.GetComponent<RectTransform>()).center };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button,
            button.name + " is obscured at its centre");
    }

    void CheckLayout()
    {
        Canvas.ForceUpdateCanvases();
        var region = Audience();
        var regionBounds = Bounds(region.GetComponent<RectTransform>());
        var screenBounds = new Rect(0, 0, Screen.width, Screen.height);
        Check(Inside(screenBounds, regionBounds), "Audience region extends beyond the screen");
        Check(!region.GetComponentsInChildren<ScrollRect>().Any(s => s.isActiveAndEnabled), "Audience requires scrolling");
        var panel = ScreenPanel();
        var panelBounds = Bounds(panel.GetComponent<RectTransform>());
        Check(!panel.GetComponentsInChildren<ScrollRect>().Any(s => s.isActiveAndEnabled), "Original council scroll is still active");
        Check(!panel.GetComponentsInChildren<Scrollbar>().Any(s => s.isActiveAndEnabled), "Original council scrollbar is still active");
        var cards = region.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Sultan choice ")).ToArray();
        Check(cards.Length == 7 && cards.Select(b => b.name).Distinct().Count() == 7, "Seven distinct choices must be visible");
        var bounds = new List<Rect>();
        for (int i = 0; i < 7; i++) {
            var button = Choice(i);
            Rect rect = Bounds(button.GetComponent<RectTransform>());
            Check(Inside(regionBounds, rect) && Inside(screenBounds, rect), "Choice " + i + " is clipped");
            Check(button.interactable == SultanNegotiations.Available(i), "Choice " + i + " has stale availability");
            CheckHit(button);
            bounds.Add(rect);
        }
        for (int i = 0; i < bounds.Count; i++)
            for (int j = i + 1; j < bounds.Count; j++) Check(!Overlaps(bounds[i], bounds[j]), "Choices overlap: " + i + "/" + j);
        var portrait = region.GetComponentsInChildren<RectTransform>().FirstOrDefault(t => t.name == "Portrait 202");
        Check(portrait != null, "Mehmed portrait is absent");
        var frame = portrait.Find("Portrait frame") as RectTransform;
        Check(frame != null, "Mehmed portrait frame is absent");
        Rect faceBounds = Bounds(frame);
        Check(Mathf.Abs(faceBounds.center.x - regionBounds.center.x) <= Mathf.Max(3, regionBounds.width * .015f), "Mehmed portrait is not horizontally centred");
        Check(Inside(regionBounds, faceBounds), "Portrait extends outside the audience");
        Check(faceBounds.height >= regionBounds.height * .40f, "Mehmed portrait remains a small thumbnail");
        Check(portrait.GetComponentInChildren<RawImage>()?.texture == CharacterPortraits.Texture(202), "Wrong or missing Mehmed portrait texture");
        foreach (Rect rect in bounds) Check(!Overlaps(faceBounds, rect), "Portrait overlaps an option");
        var back = LabelButton("Return");
        Rect backBounds = Bounds(back.GetComponent<RectTransform>());
        var offer = panel.GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "Sultan offer review");
        Check((offer != null) == SurrenderTerms.HasOffer, "Existing-offer control visibility is incorrect");
        if (offer != null) {
            Rect offerBounds = Bounds(offer.GetComponent<RectTransform>());
            Check(Inside(panelBounds, offerBounds) && Inside(screenBounds, offerBounds), "Offer review is clipped");
            Check(offer.interactable, "Existing/expired offer cannot be read");
            CheckHit(offer);
            foreach (Rect rect in bounds) Check(!Overlaps(offerBounds, rect), "Offer review overlaps a choice");
            Check(!Overlaps(offerBounds, faceBounds), "Offer review overlaps Mehmed");
            Check(!Overlaps(offerBounds, backBounds), "Offer review overlaps Return");
        }
        // Include the page introduction, identity/name and footer, not only the cards.
        foreach (var text in panel.GetComponentsInChildren<TMP_Text>()) {
            text.ForceMeshUpdate();
            Check(!text.isTextOverflowing, "Text overflows: " + text.name + " | " + text.text);
        }
        Check(back.interactable && Inside(panelBounds, backBounds) && Inside(screenBounds, backBounds), "Return is unavailable or clipped");
        CheckHit(back);
    }

    IEnumerator Resize(int width, int height)
    {
        Screen.SetResolution(width, height, false);
        yield return new WaitForSecondsRealtime(.6f);
        Canvas.ForceUpdateCanvases();
        Check(Screen.width == width && Screen.height == height, "Requested display size was not applied");
    }

    int[] ResourcesNow() => new[] { CouncilActions.Int("iMoney"), CouncilActions.Int("iProvisions"), CouncilActions.Int("iMaterials"), CouncilActions.Int("iSecrets") };

    IEnumerator Run()
    {
        AudioListener.volume = 0;
        Application.runInBackground = true;
        yield return null;
        SaveMenu.StartCampaign(false);
        float deadline = Time.realtimeSinceStartup + 15;
        while ((!VariableSingleton.IsInitialized || FindObjectOfType<ManagerUI>() == null) && Time.realtimeSinceStartup < deadline) yield return null;
        Check(VariableSingleton.IsInitialized && FindObjectOfType<ManagerUI>() != null, "Campaign did not initialise");
        yield return new WaitForSecondsRealtime(.5f);
        council = CouncilUI.For(FindObjectOfType<ManagerUI>());
        baseline = VariableSingleton.Instance.CaptureState();
        yield return Resize(1600, 900);

        Fixture(false);
        yield return null;
        Check(!VariableSingleton.HasSiegeStarted(), "Preparation fixture is already besieged");
        foreach (int i in new[] { 1, 3, 4 }) Check(!Choice(i).interactable, "Literary topic is available during preparation");
        foreach (int i in new[] { 0, 2, 5, 6 }) Check(Choice(i).interactable, "An existing preparation option was removed");
        CheckLayout();
        yield return Capture("sultan-preparation");

        Fixture(true);
        yield return null;
        Check(VariableSingleton.HasSiegeStarted(), "Siege fixture is still preparing");
        for (int i = 0; i < 7; i++) Check(Choice(i).interactable, "Fresh siege choice is unavailable: " + i);
        CheckLayout();
        yield return Capture("sultan-fresh-siege");

        // Browsing and cancelling gifts must not spend money, resources, fury or time.
        VariableSingleton.SetInt("iMoney", 0);
        council.Sultan();
        yield return null;
        int[] stock = ResourcesNow();
        float fury = VariableSingleton.GetFloatVariable("fSultanRage");
        int turn = VariableSingleton.Turn;
        Click(Choice(2));
        yield return null;
        var reviews = ScreenPanel().GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Gift card ")).ToArray();
        Check(reviews.Length == SultanGifts.Gifts.Length && reviews.Length == 8, "Gift catalogue lost an option");
        Click(reviews[0]);
        yield return null;
        var confirmGift = ScreenPanel().GetComponentsInChildren<Button>().FirstOrDefault(b => Caption(b) != "Return" && Caption(b) != "Choose another gift");
        Check(confirmGift != null && !confirmGift.interactable, "Unaffordable gift confirmation is not disabled");
        Click(LabelButton("Choose another gift"));
        yield return null;
        ControlSingleton.Instance.GoBack();
        yield return null;
        Check(ResourcesNow().SequenceEqual(stock) && Mathf.Approximately(VariableSingleton.GetFloatVariable("fSultanRage"), fury), "Gift browsing/cancellation spent resources or changed fury");
        Check(VariableSingleton.Turn == turn && FindObjectOfType<RestUI>() == null, "Gift browsing consumed the major action");
        Check(Choice(2).interactable, "Gift catalogue is inaccessible at zero gold");

        // Terms and the shared literary allowance are independent.
        Fixture(true);
        yield return null;
        turn = VariableSingleton.Turn;
        stock = ResourcesNow();
        Click(Choice(0));
        yield return null;
        Check(SurrenderTerms.HasOffer && SurrenderTerms.Valid, "Request did not generate today's offer");
        Check(CouncilActions.Int("iSultanVisit0") == TrainingPool.Day(VariableSingleton.GetDate()), "Terms request did not consume its daily allowance");
        Check(CouncilActions.Int("iSurrenderEnding") == 0, "Reviewing terms accepted surrender");
        Check(ScreenPanel().GetComponentsInChildren<TMP_Text>().Any(t => t.text == "The Sultan's terms"), "Request skipped the terms review");
        ControlSingleton.Instance.GoBack();
        yield return null;
        Check(!Choice(0).interactable && Choice(1).interactable, "Terms and literary conversation share the wrong lock");
        Click(Choice(1));
        yield return null;
        Check(CampaignHUD.Instance.HasModal, "Conversation report did not open");
        var modal = GameObject.Find("Event outcome");
        Check(modal != null, "Conversation outcome is missing");
        Click(modal.GetComponentsInChildren<Button>().First(b => Caption(b) == "Continue"));
        yield return null;
        foreach (int i in new[] { 1, 3, 4 }) Check(!Choice(i).interactable, "Shared daily conversation allowance not enforced");
        Check(VariableSingleton.Turn == turn && ResourcesNow().SequenceEqual(stock) && FindObjectOfType<RestUI>() == null, "Free exchanges consumed time, resources or the major action");
        CheckLayout();
        yield return Capture("sultan-used-and-terms");
        var offer = ScreenPanel().GetComponentsInChildren<Button>().First(b => b.name == "Sultan offer review");
        Click(offer);
        yield return null;
        Check(SurrenderTerms.Valid && CouncilActions.Int("iSurrenderEnding") == 0, "Reviewing an existing offer changed its outcome");
        ControlSingleton.Instance.GoBack();
        yield return null;

        yield return Resize(1100, 760);
        CheckLayout();
        yield return Capture("sultan-1100x760");
        yield return Resize(1280, 720);
        CheckLayout();
        yield return Capture("sultan-1280x720");

        Fixture(true);
        VariableSingleton.SetInt("iSultanSeen0", 7);
        council.Sultan();
        yield return null;
        Check(!Choice(1).interactable && SultanNegotiations.Unavailable(1) == "All exchanges read", "Completed history topic reopened");
        Check(Choice(3).interactable && Choice(4).interactable, "Completed history closed other topics");
        CheckLayout();
        yield return Capture("sultan-exhausted-topic");

        Fixture(true);
        SurrenderTerms.Request();
        VariableSingleton.SetInt("iTermsDay", TrainingPool.Day(VariableSingleton.GetDate()) - 1);
        council.Sultan();
        yield return null;
        Check(SurrenderTerms.HasOffer && !SurrenderTerms.Valid, "Expired offer fixture is invalid");
        CheckLayout();
        yield return Capture("sultan-expired-offer");
        Click(ScreenPanel().GetComponentsInChildren<Button>().First(b => b.name == "Sultan offer review"));
        yield return null;
        var accept = ScreenPanel().GetComponentsInChildren<Button>().FirstOrDefault(b => Caption(b).StartsWith("Accept terms"));
        Check(accept != null && !accept.interactable, "Expired offer can be accepted");
        ControlSingleton.Instance.GoBack();
        yield return null;
        Click(LabelButton("Return"));
        yield return null;
        Check(GameObject.Find("Sultan audience") == null && ScreenPanel().GetComponentsInChildren<TMP_Text>().Any(t => t.text == "Diplomacy and court"), "Return did not reach diplomacy");

        // Last fixture spends a major action. Deliberate duplicate callback tests its guard.
        yield return Resize(1600, 900);
        Fixture(true);
        yield return null;
        turn = VariableSingleton.Turn;
        var demand = Choice(5);
        Click(demand);
        demand.onClick.Invoke();
        yield return null;
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fTroopRep") - .53f) < .0001f, "Major action applied morale more than once");
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fSultanRage") - .25f) < .0001f, "Major action applied fury more than once");
        Check(FindObjectOfType<RestUI>() != null, "Major action did not enter the order-result phase");
        Check(GameObject.Find("Sultan audience") == null && !FindObjectsOfType<Button>().Any(b => b.name.StartsWith("Sultan choice ")), "Major action leaves Sultan choices available");
        Check(!SaveGames.CanCheckpointCouncil, "Major action still reports council phase as available");
        Check(VariableSingleton.Turn == turn, "Order result advanced the day prematurely");
        ControlSingleton.Instance.GoBack();
        yield return null;
        Check(FindObjectOfType<RestUI>() != null && GameObject.Find("Sultan audience") == null, "Back navigation undid the major action");
    }

    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.35f);
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
