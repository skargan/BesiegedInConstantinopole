using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Artwork owns no narrative or campaign state. Manifest metadata is loaded once;
// only the images currently requested by views are loaded.
public static class EventCardArt
{
    [Serializable] sealed class Entry { public string node, path, source, fit; public float focusY = .5f; }
    [Serializable] sealed class Manifest { public Entry[] entries; }
    internal sealed class Lease
    {
        internal string path;
        internal Texture2D texture;
        internal int references;
        internal long lastUse;
    }
    const int CacheLimit = 4;
    static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
    static readonly Dictionary<string, Lease> images = new Dictionary<string, Lease>(StringComparer.Ordinal);
    static readonly HashSet<string> missing = new HashSet<string>(StringComparer.Ordinal);
    static bool manifestRead;
    static long clock;
    static GameObject preview, previousSelection;
    static EventArtworkView previewOwner;
    static RawImage previewImage;

    static void ReadManifest()
    {
        if (manifestRead) return;
        manifestRead = true;
        var asset = Resources.Load<TextAsset>("Content/EventArtwork");
        if (asset == null) return; // Older builds retain their three generic illustrations.
        try
        {
            var manifest = JsonUtility.FromJson<Manifest>(asset.text);
            if (manifest == null || manifest.entries == null) return;
            foreach (var entry in manifest.entries)
                if (entry != null && !string.IsNullOrEmpty(entry.node) && !string.IsNullOrEmpty(entry.path))
                {
                    if (entries.ContainsKey(entry.node)) Debug.LogWarning("Duplicate event artwork: " + entry.node);
                    else entries.Add(entry.node, entry);
                }
        }
        catch (Exception e) { Debug.LogWarning("Event artwork manifest could not be read: " + e.Message); }
    }

    public static void Refresh(Transform parent, string node, string narrative)
    {
        if (parent == null) return;
        ReadManifest();
        node = node ?? "";
        string text = (narrative ?? "").ToLowerInvariant();
        string category = "civic";
        if (node != "foodShortage")
        {
            if (node.StartsWith("shah", StringComparison.Ordinal) || text.Contains("traitor") || text.Contains("spy") || text.Contains("spies") || text.Contains("envoy") || text.Contains("sultan") || text.Contains("messenger")) category = "intrigue";
            else if (text.Contains("cannon") || text.Contains("assault") || text.Contains("fortification") || text.Contains("wall") || text.Contains("soldier")) category = "military";
        }
        string fallback = "EventArt/" + category + "-r11";
        float fallbackFocus = category == "intrigue" ? .75f : category == "military" ? .60f : .55f;
        var frame = parent.Find("Event artwork");
        if (frame == null)
        {
            frame = CampaignHUD.Panel(parent, "Event artwork", new Vector2(.018f, .51f), new Vector2(.448f, .975f)).transform;
            var picture = new GameObject("Illustration", typeof(RectTransform), typeof(RawImage));
            picture.transform.SetParent(frame, false);
        }
        var view = frame.GetComponent<EventArtworkView>() ?? frame.gameObject.AddComponent<EventArtworkView>();
        Entry entry;
        bool mapped = entries.TryGetValue(node, out entry);
        view.Configure(mapped ? entry.path : fallback, fallback,
            mapped && !string.Equals(entry.fit, "cover", StringComparison.OrdinalIgnoreCase),
            mapped ? Mathf.Clamp01(entry.focusY) : fallbackFocus, fallbackFocus);
    }

    internal static Lease Acquire(string path)
    {
        Lease lease;
        if (!images.TryGetValue(path, out lease))
        {
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                if (missing.Add(path)) Debug.LogWarning("Missing event artwork: " + path);
                return null;
            }
            lease = new Lease { path = path, texture = texture };
            images.Add(path, lease);
        }
        lease.references++;
        lease.lastUse = ++clock;
        TrimCache();
        return lease;
    }

    internal static void Release(Lease lease)
    {
        if (lease == null) return;
        lease.references = Mathf.Max(0, lease.references - 1);
        lease.lastUse = ++clock;
        TrimCache();
    }

    static void TrimCache()
    {
        // Live views are pinned. More than four simultaneous live illustrations
        // may exceed the limit until a view releases its image; never unload a live one.
        while (images.Count > CacheLimit)
        {
            Lease oldest = null;
            foreach (var lease in images.Values)
                if (lease.references == 0 && (oldest == null || lease.lastUse < oldest.lastUse)) oldest = lease;
            if (oldest == null) return;
            images.Remove(oldest.path);
            if (oldest.texture != null) Resources.UnloadAsset(oldest.texture);
        }
    }

    internal static void OpenPreview(EventArtworkView owner)
    {
        if (owner == null || !owner.isActiveAndEnabled || owner.Picture == null || owner.Picture.texture == null) return;
        var canvas = owner.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        ClosePreview();
        previewOwner = owner;
        previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        var root = canvas.rootCanvas;
        preview = CampaignHUD.Panel(root.transform, "Event artwork enlarged", Vector2.zero, Vector2.one);
        preview.GetComponent<Image>().color = new Color(.006f, .012f, .02f, .98f);
        preview.GetComponent<Image>().raycastTarget = true;
        var layer = preview.AddComponent<Canvas>();
        layer.overrideSorting = true;
        layer.sortingOrder = Mathf.Max(root.sortingOrder + 1, 230);
        preview.AddComponent<GraphicRaycaster>();
        var holder = new GameObject("Full illustration", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
        holder.transform.SetParent(preview.transform, false);
        previewImage = holder.GetComponent<RawImage>();
        previewImage.texture = owner.Picture.texture;
        previewImage.raycastTarget = false;
        previewImage.uvRect = new Rect(0, 0, 1, 1);
        // Fit inside a separate bounded holder: AspectRatioFitter itself is free
        // to resize its child, never the full-screen input-blocking surface.
        var bounds = new GameObject("Illustration bounds", typeof(RectTransform));
        bounds.transform.SetParent(preview.transform, false);
        var rect = (RectTransform)bounds.transform;
        rect.anchorMin = new Vector2(.025f, .105f); rect.anchorMax = new Vector2(.975f, .975f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        holder.transform.SetParent(bounds.transform, false);
        var fitter = holder.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = (float)previewImage.texture.width / previewImage.texture.height;
        var close = CampaignHUD.MakeButton(preview.transform, "Close", new Vector2(.39f, .025f), new Vector2(.61f, .085f), () => ClosePreview());
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(close.gameObject);
        Canvas.ForceUpdateCanvases();
    }

    // ControlSingleton must call this FIRST in both its Escape and GoBack chains.
    // This consumes one navigation request without also pausing/leaving the event.
    public static bool ClosePreview()
    {
        if (preview == null) { previewOwner = null; previewImage = null; previousSelection = null; return false; }
        if (previewImage != null) previewImage.texture = null;
        preview.SetActive(false);
        UnityEngine.Object.Destroy(preview);
        preview = null; previewOwner = null; previewImage = null;
        if (EventSystem.current != null && previousSelection != null && previousSelection.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(previousSelection);
        previousSelection = null;
        return true;
    }

    internal static void CloseFor(EventArtworkView owner)
    {
        if (previewOwner == owner) ClosePreview();
    }
}

// Each frame holds a cache lease while visible, including while its enlarged
// picture is open. Refresh/disable/destruction close the preview before release.
public sealed class EventArtworkView : MonoBehaviour
{
    // ImperialFrame draws its outer line 3–5 UI units inside the panel.
    public const float ArtInset = 8f;
    RectTransform viewport;
    public RawImage Picture { get; private set; }
    EventCardArt.Lease lease;
    string path, fallback;
    bool contain, configured, displayedContain;
    float focus, fallbackFocus, displayedFocus;

    void Awake()
    {
        Picture = transform.Find("Illustration").GetComponent<RawImage>();
        Picture.raycastTarget = false;
        Picture.color = Color.white;
        // Clip only the image, leaving the gold frame outside the mask.
        var content = new GameObject("Artwork viewport", typeof(RectTransform), typeof(RectMask2D));
        content.transform.SetParent(transform, false);
        viewport = content.GetComponent<RectTransform>();
        viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.one * ArtInset; viewport.offsetMax = -Vector2.one * ArtInset;
        Picture.transform.SetParent(viewport, false);
        Picture.maskable = true;
        var image = GetComponent<Image>();
        image.sprite = null; image.color = new Color(.012f, .025f, .04f, 1); image.raycastTarget = true;
        var button = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        button.targetGraphic = image; button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => EventCardArt.OpenPreview(this));
        var border = GetComponent<Outline>(); if (border != null) border.enabled = false;
        ImperialTheme.Frame(gameObject);
    }

    internal void Configure(string desired, string generic, bool fullImage, float centre, float genericCentre)
    {
        Release();
        path = desired; fallback = generic; contain = fullImage; focus = centre; fallbackFocus = genericCentre; configured = true;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (lease == null && isActiveAndEnabled) Load();
        Canvas.ForceUpdateCanvases(); Layout();
    }
    void OnEnable() { if (configured && lease == null) Load(); }
    void OnDisable() { Release(); }
    void OnDestroy() { Release(); }
    void OnRectTransformDimensionsChange() { Layout(); }

    void Load()
    {
        lease = EventCardArt.Acquire(path);
        displayedContain = contain; displayedFocus = focus;
        if (lease == null && path != fallback)
        {
            lease = EventCardArt.Acquire(fallback);
            displayedContain = false; displayedFocus = fallbackFocus;
        }
        Picture.texture = lease != null ? lease.texture : null;
        if (lease == null) { gameObject.SetActive(false); return; }
        Layout();
    }
    void Release()
    {
        EventCardArt.CloseFor(this);
        if (Picture != null) Picture.texture = null;
        EventCardArt.Release(lease); lease = null;
    }
    void Layout()
    {
        if (Picture == null || Picture.texture == null) return;
        var bounds = ((RectTransform)transform).rect;
        if (bounds.width <= 0 || bounds.height <= 0) return;
        // Derive from the outer dimensions so a resize cannot use yesterday's viewport size.
        bounds.width = Mathf.Max(0, bounds.width - ArtInset * 2);
        bounds.height = Mathf.Max(0, bounds.height - ArtInset * 2);
        if (bounds.width <= 0 || bounds.height <= 0) return;
        float imageAspect = (float)Picture.texture.width / Picture.texture.height;
        float frameAspect = bounds.width / bounds.height;
        var rect = Picture.rectTransform;
        if (displayedContain)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = imageAspect > frameAspect ? new Vector2(bounds.width, bounds.width / imageAspect) : new Vector2(bounds.height * imageAspect, bounds.height);
            Picture.uvRect = new Rect(0, 0, 1, 1);
        }
        else
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            float width = Mathf.Min(1, frameAspect / imageAspect), height = Mathf.Min(1, imageAspect / frameAspect);
            Picture.uvRect = new Rect((1 - width) * .5f, Mathf.Clamp(displayedFocus - height * .5f, 0, 1 - height), width, height);
        }
    }
}
