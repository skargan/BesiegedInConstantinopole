using System;
using UnityEngine;
using UnityEngine.UI;

// Presentation only. Call from the current HUD's existing stat construction code.
public static class CampaignStatArt
{
    public enum Stat
    {
        Health, Gold, People, Materials, Food, Secrets,
        Archers, Infantry, Cavalry, Guard,
        ArmySupport, PublicSupport, ClergySupport, NoblesSupport
    }

    private const string ResourceRoot = "UI/MidnightMosaic/Stats/";
    private const float SourceSize = 1254f;
    private const float SourcePadding = 12f;

    private sealed class Entry
    {
        public readonly string ResourcePath;
        public readonly RectInt Bounds;
        public bool Attempted;
        public Sprite Sprite;

        public Entry(string filename, int left, int top, int right, int bottom)
        {
            ResourcePath = ResourceRoot + filename;
            Bounds = new RectInt(left, top, right - left, bottom - top);
        }
    }

    // Original 1254 x 1254 PNG bounds at alpha >= 8, in top-left coordinates.
    // Precomputed bounds allow the imported textures to remain non-readable.
    // Entries are ordered to match Stat; update these bounds if the art changes.
    private static readonly Entry[] Entries =
    {
        new Entry("health",          140, 201, 1114, 1078),
        new Entry("gold",            133, 165, 1106, 1091),
        new Entry("people",           62, 273, 1191, 1042),
        new Entry("materials",        85, 262, 1171, 1023),
        new Entry("food",            348,  87,  906, 1171),
        new Entry("secrets",         128, 152, 1121, 1144),
        new Entry("archers",         106, 115, 1172, 1140),
        new Entry("infantry",        101, 111, 1153, 1149),
        new Entry("cavalry",         132,  87, 1147, 1179),
        new Entry("guard",           181, 157, 1173, 1109),
        new Entry("army-support",    249,  98, 1059, 1132),
        new Entry("public-support",  214, 115, 1099, 1147),
        new Entry("clergy-support",  179, 196, 1074, 1056),
        new Entry("nobles-support",   81, 277, 1173,  977)
    };

    /// <summary>
    /// Loads and crops a stat sprite once. Missing resources return null and
    /// produce one warning per stat for the lifetime of this static cache.
    /// </summary>
    public static Sprite Get(Stat stat)
    {
        int index = (int)stat;
        if (index < 0 || index >= Entries.Length)
            throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown campaign stat.");

        Entry entry = Entries[index];
        if (entry.Attempted)
            return entry.Sprite;

        entry.Attempted = true;
        Texture2D texture = Resources.Load<Texture2D>(entry.ResourcePath);
        if (texture == null)
        {
            Debug.LogWarning("CampaignStatArt: could not load Texture2D at Resources/"
                + entry.ResourcePath + ".png.");
            return null;
        }

        entry.Sprite = Sprite.Create(texture, ImportedRect(texture, entry.Bounds),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        entry.Sprite.name = "CampaignStatArt/" + stat;
        return entry.Sprite;
    }

    /// <summary>
    /// Applies artwork without changing layout or stat bindings. A null target
    /// or missing resource returns false and leaves the target unchanged.
    /// </summary>
    public static bool Apply(Image target, Stat stat)
    {
        if (target == null)
            return false;

        Sprite sprite = Get(stat);
        if (sprite == null)
            return false;

        target.overrideSprite = null;
        target.sprite = sprite;
        target.color = Color.white;
        target.type = Image.Type.Simple;
        target.preserveAspect = true;
        target.raycastTarget = false;
        return true;
    }

    private static Rect ImportedRect(Texture2D texture, RectInt bounds)
    {
        float scaleX = texture.width / SourceSize;
        float scaleY = texture.height / SourceSize;
        float left = Mathf.Max(0f, bounds.xMin - SourcePadding);
        float right = Mathf.Min(SourceSize, bounds.xMax + SourcePadding);
        float top = Mathf.Max(0f, bounds.yMin - SourcePadding);
        float bottom = Mathf.Min(SourceSize, bounds.yMax + SourcePadding);

        // Sprite rectangles use bottom-left coordinates. Round outwards after
        // scaling so importer downsampling cannot trim the visible icon edge.
        return Rect.MinMaxRect(
            Mathf.Floor(left * scaleX),
            Mathf.Floor((SourceSize - bottom) * scaleY),
            Mathf.Ceil(right * scaleX),
            Mathf.Ceil((SourceSize - top) * scaleY));
    }
}
