using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Art is resolved by stable technology key, never the visible row order.
public static class TechnologyArtwork
{
    const string Root = "UI/MidnightMosaic/Technologies/";
    static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
    static GameObject enlarged;

    // Close the artwork before the council's existing Back/Escape navigation runs.
    public static bool ClosePreview()
    {
        if (enlarged == null) return false;
        Object.Destroy(enlarged);
        enlarged = null;
        return true;
    }

    public static Texture2D Load(string key)
    {
        if (key.StartsWith("bTech")) key = key.Substring(5);
        if (!Cache.TryGetValue(key, out var texture))
        {
            texture = Resources.Load<Texture2D>(Root + key);
            Cache[key] = texture;
            if (texture == null) Debug.LogWarning("Missing technology artwork: " + key);
        }
        return texture;
    }

    public static void Attach(Transform row, string key)
    {
        var texture = Load(key);
        if (texture == null) return;
        var frame = new GameObject("Technology artwork " + key, typeof(RectTransform), typeof(Image), typeof(Button));
        frame.transform.SetParent(row, false);
        var rect = frame.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(.012f, .06f);
        rect.anchorMax = new Vector2(.235f, .94f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        frame.GetComponent<Image>().color = new Color(.015f, .025f, .05f, 1f);
        Fit(frame.transform, texture);
        frame.GetComponent<Button>().onClick.AddListener(() => Enlarge(row, texture));
    }

    static void Fit(Transform parent, Texture2D texture)
    {
        var art = new GameObject("Uncropped mosaic", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
        art.transform.SetParent(parent, false);
        var rect = art.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = art.GetComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        var fit = art.GetComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fit.aspectRatio = (float)texture.width / texture.height;
    }

    static void Enlarge(Transform row, Texture2D texture)
    {
        ClosePreview();
        var canvas = row.GetComponentInParent<Canvas>().rootCanvas;
        enlarged = new GameObject("Technology artwork enlarged", typeof(RectTransform), typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image), typeof(Button));
        var overlay=enlarged.GetComponent<Canvas>();overlay.renderMode=RenderMode.ScreenSpaceOverlay;overlay.sortingOrder=220;var scaler=enlarged.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var rect = enlarged.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        enlarged.GetComponent<Image>().color = new Color(.008f, .015f, .03f, .98f);
        var holder = new GameObject("Artwork", typeof(RectTransform));
        holder.transform.SetParent(enlarged.transform, false);
        var area = holder.GetComponent<RectTransform>();
        area.anchorMin = new Vector2(.04f, .09f);
        area.anchorMax = new Vector2(.96f, .96f);
        area.offsetMin = area.offsetMax = Vector2.zero;
        Fit(holder.transform, texture);
        var hint = CampaignHUD.Label(enlarged.transform, "Click anywhere or press Esc to close", new Vector2(.1f, .015f), new Vector2(.9f, .075f), 22);
        hint.alignment = TMPro.TextAlignmentOptions.Center;
        foreach (var graphic in enlarged.GetComponentsInChildren<Graphic>())
            if (graphic.gameObject != enlarged) graphic.raycastTarget = false;
        enlarged.GetComponent<Button>().onClick.AddListener(() => { ClosePreview(); });
    }
}
