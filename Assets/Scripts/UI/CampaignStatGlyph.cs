using UnityEngine;
using UnityEngine.UI;

// Small, resolution-independent symbols for the campaign's shared statistics.
// Coordinates use a 24-unit square; a single ink colour keeps them legible at HUD size.
public sealed class CampaignStatGlyph : MaskableGraphic
{
    public int Kind;
    Rect drawingRect;
    float scale;

    public static CampaignStatGlyph Add(Transform parent, int kind, Vector2 min, Vector2 max)
    {
        var go = new GameObject("Stat symbol " + kind, typeof(RectTransform), typeof(CanvasRenderer), typeof(CampaignStatGlyph));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var glyph = go.GetComponent<CampaignStatGlyph>();
        glyph.Kind = kind;
        glyph.color = new Color(.86f, .71f, .42f, 1);
        glyph.raycastTarget = false;
        return glyph;
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        drawingRect = rectTransform.rect;
        scale = Mathf.Min(drawingRect.width, drawingRect.height) / 24f;
        if (scale <= 0) return;
        switch (Kind) {
            case 0: // Heart: health.
                Disc(mesh, 7.5f, 15, 4.5f); Disc(mesh, 16.5f, 15, 4.5f);
                Triangle(mesh, V(3, 14), V(21, 14), V(12, 3));
                break;
            case 1: // Stacked coins.
                Ellipse(mesh, 10, 7, 7, 3); Line(mesh, 3, 7, 3, 11); Line(mesh, 17, 7, 17, 11);
                Ellipse(mesh, 10, 11, 7, 3); Ellipse(mesh, 14, 16, 7, 3);
                Line(mesh, 7, 16, 7, 19); Line(mesh, 21, 16, 21, 19); Ellipse(mesh, 14, 19, 7, 3);
                break;
            case 2: // Citizens.
                Person(mesh, 6, 11, 2.5f); Person(mesh, 18, 11, 2.5f); Person(mesh, 12, 15, 3);
                break;
            case 3: // Masonry / materials.
                Box(mesh, 3, 4, 18, 15); Line(mesh, 3, 9, 21, 9); Line(mesh, 3, 14, 21, 14);
                Line(mesh, 9, 4, 9, 9); Line(mesh, 15, 4, 15, 9);
                Line(mesh, 6, 9, 6, 14); Line(mesh, 13, 9, 13, 14); Line(mesh, 18, 14, 18, 19); Line(mesh, 10, 14, 10, 19);
                break;
            case 4: // Ear of wheat.
                Line(mesh, 12, 2, 12, 21);
                for (int i = 0; i < 3; i++) {
                    float y = 7 + i * 4;
                    Diamond(mesh, 8.5f, y, 3, 2); Diamond(mesh, 15.5f, y + 2, 3, 2);
                }
                Diamond(mesh, 12, 20, 2, 3);
                break;
            case 5: // Key: secrets and privileged knowledge.
                Ellipse(mesh, 7, 16, 4.5f, 4.5f); Line(mesh, 10, 13, 20, 3, 2);
                Line(mesh, 15, 8, 18, 11, 2); Line(mesh, 18, 5, 21, 8, 2);
                break;
            case 6: // Bow and arrow.
                Path(mesh, V(7, 3), V(12, 6), V(14, 12), V(12, 18), V(7, 21));
                Line(mesh, 7, 3, 7, 21, 1); Line(mesh, 3, 12, 22, 12);
                Path(mesh, V(18, 16), V(22, 12), V(18, 8));
                break;
            case 7: // Upright sword.
                Path(mesh, V(10, 9), V(10, 18), V(12, 22), V(14, 18), V(14, 9));
                Line(mesh, 6, 8, 18, 8, 2); Line(mesh, 12, 8, 12, 3, 2); Disc(mesh, 12, 2.5f, 1.5f);
                break;
            case 8: // Horseshoe.
                Path(mesh, V(6, 20), V(3, 14), V(3, 8), V(6, 4), V(12, 2), V(18, 4), V(21, 8), V(21, 14), V(18, 20));
                Path(mesh, V(15, 20), V(17, 13), V(17, 9), V(15, 7), V(9, 7), V(7, 9), V(7, 13), V(9, 20));
                Line(mesh, 6, 20, 9, 20); Line(mesh, 15, 20, 18, 20);
                break;
            case 9: // Varangian double axe.
                Line(mesh, 12, 2, 12, 22, 2.2f);
                Triangle(mesh, V(12, 15), V(4, 21), V(3, 11));
                Triangle(mesh, V(12, 15), V(20, 21), V(21, 11));
                Line(mesh, 3, 11, 4, 21, 2); Line(mesh, 21, 11, 20, 21, 2);
                break;
            case 10: // Army standard.
                Line(mesh, 6, 2, 6, 22, 2); Disc(mesh, 6, 22, 1);
                Path(mesh, V(7, 20), V(21, 20), V(17, 15), V(21, 10), V(7, 10));
                Line(mesh, 13, 13, 13, 18); Line(mesh, 10.5f, 15.5f, 15.5f, 15.5f);
                break;
            case 11: // Civic hope: a radiant sun.
                Ellipse(mesh, 12, 12, 4, 4);
                for (int i = 0; i < 8; i++) {
                    float a = i * Mathf.PI / 4;
                    Vector2 dir = V(Mathf.Cos(a), Mathf.Sin(a));
                    Segment(mesh, V(12, 12) + dir * 7, V(12, 12) + dir * 10, 1.8f);
                }
                break;
            case 12: // Clergy: Byzantine cross.
                Line(mesh, 12, 2, 12, 22, 2); Line(mesh, 8, 19, 16, 19, 2);
                Line(mesh, 4, 14, 20, 14, 2); Line(mesh, 8, 8, 16, 5, 2);
                break;
            case 13: // Noble coronet.
                Path(mesh, V(4, 7), V(2, 18), V(8, 13), V(12, 21), V(16, 13), V(22, 18), V(20, 7), V(4, 7));
                Line(mesh, 4, 4, 20, 4, 2); Disc(mesh, 2, 19, 1); Disc(mesh, 12, 22, 1); Disc(mesh, 22, 19, 1);
                break;
        }
    }

    static Vector2 V(float x, float y) => new Vector2(x, y);
    Vector2 Point(Vector2 p) => drawingRect.center + (p - V(12, 12)) * scale;
    void Triangle(VertexHelper m, Vector2 a, Vector2 b, Vector2 c)
    {
        int i = m.currentVertCount;
        m.AddVert(Point(a), color, Vector2.zero); m.AddVert(Point(b), color, Vector2.zero); m.AddVert(Point(c), color, Vector2.zero);
        m.AddTriangle(i, i + 1, i + 2);
    }
    void Segment(VertexHelper m, Vector2 a, Vector2 b, float width)
    {
        Vector2 d = b - a, n = V(-d.y, d.x).normalized * (width / 2);
        Triangle(m, a - n, a + n, b + n); Triangle(m, a - n, b + n, b - n);
    }
    void Line(VertexHelper m, float ax, float ay, float bx, float by, float width = 1.6f) => Segment(m, V(ax, ay), V(bx, by), width);
    void Path(VertexHelper m, params Vector2[] points) { for (int i = 1; i < points.Length; i++) Segment(m, points[i - 1], points[i], 1.6f); }
    void Box(VertexHelper m, float x, float y, float w, float h) => Path(m, V(x, y), V(x + w, y), V(x + w, y + h), V(x, y + h), V(x, y));
    void Diamond(VertexHelper m, float x, float y, float w, float h) { Triangle(m, V(x - w, y), V(x, y + h), V(x + w, y)); Triangle(m, V(x - w, y), V(x + w, y), V(x, y - h)); }
    void Disc(VertexHelper m, float x, float y, float radius)
    {
        for (int i = 0; i < 16; i++) {
            float a = i * Mathf.PI / 8, b = (i + 1) * Mathf.PI / 8;
            Triangle(m, V(x, y), V(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius), V(x + Mathf.Cos(b) * radius, y + Mathf.Sin(b) * radius));
        }
    }
    void Ellipse(VertexHelper m, float x, float y, float rx, float ry)
    {
        for (int i = 0; i < 20; i++) {
            float a = i * Mathf.PI / 10, b = (i + 1) * Mathf.PI / 10;
            Segment(m, V(x + Mathf.Cos(a) * rx, y + Mathf.Sin(a) * ry), V(x + Mathf.Cos(b) * rx, y + Mathf.Sin(b) * ry), 1.5f);
        }
    }
    void Person(VertexHelper m, float x, float y, float radius)
    {
        Disc(m, x, y + 4, radius);
        Triangle(m, V(x - radius - 1, y), V(x + radius + 1, y), V(x + radius + 1, y - 5));
        Triangle(m, V(x - radius - 1, y), V(x + radius + 1, y - 5), V(x - radius - 1, y - 5));
    }
}
