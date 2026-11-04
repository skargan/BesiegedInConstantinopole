using UnityEngine;
using UnityEngine.UI;

// Presentation-only randomness: never consume the campaign or Unity random stream.
public sealed class LaunchMenuArtwork : MonoBehaviour
{
    private static readonly string[] Names = {
        "01-city-before-dawn", "02-unfinished-supper", "03-golden-horn",
        "04-watch-on-walls", "05-bread-stall", "06-hagia-sophia-gallery",
        "07-foundry", "08-garden-between-councils", "09-letters-across-water"
    };
    private static int selected = -1;
    private RawImage artwork;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { selected = -1; }

    public static void Apply(Transform parent)
    {
        if (parent.Find("Launch artwork") != null) return;
        if (selected < 0) selected = new System.Random(System.Guid.NewGuid().GetHashCode()).Next(Names.Length);
        var texture = Resources.Load<Texture2D>("UI/MidnightMosaic/Launch/" + Names[selected]);
        if (texture == null) { Debug.LogWarning("Missing launch artwork: " + Names[selected]); return; }
        var obj = new GameObject("Launch artwork", typeof(RectTransform), typeof(RawImage), typeof(LaunchMenuArtwork));
        obj.transform.SetParent(parent, false);
        var rect = (RectTransform)obj.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = obj.GetComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        var presenter = obj.GetComponent<LaunchMenuArtwork>();
        presenter.artwork = image;
        presenter.Fit();
    }

    private void OnRectTransformDimensionsChange() { Fit(); }

    private void Fit()
    {
        if (artwork == null || artwork.texture == null) return;
        var size = artwork.rectTransform.rect;
        if (size.width <= 0 || size.height <= 0) return;
        float source = (float)artwork.texture.width / artwork.texture.height;
        float target = size.width / size.height;
        // Cover the viewport without stretching; crop symmetrically on unusual aspect ratios.
        artwork.uvRect = target > source
            ? new Rect(0, (1 - source / target) * .5f, 1, source / target)
            : new Rect((1 - target / source) * .5f, 0, target / source, 1);
    }
}
