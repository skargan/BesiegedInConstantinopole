using UnityEngine;
using UnityEngine.UI;

// Image's default alpha mapping assumes a centred pivot when preserving aspect.
// Court figures are bottom-pivoted, trimmed sprites: map the displayed rect explicitly.
public sealed class CourtSilhouetteImage : Image
{
    public override bool IsRaycastLocationValid(Vector2 point, Camera camera)
    {
        if(sprite==null || !sprite.texture.isReadable)return base.IsRaycastLocationValid(point,camera);
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,point,camera,out var local))return false;
        Rect area=GetPixelAdjustedRect();
        if(preserveAspect){float ratio=sprite.rect.width/sprite.rect.height;
            if(area.width/area.height>ratio){float w=area.height*ratio;area.x+=(area.width-w)*rectTransform.pivot.x;area.width=w;}
            else {float h=area.width/ratio;area.y+=(area.height-h)*rectTransform.pivot.y;area.height=h;}}
        if(!area.Contains(local))return false;
        Rect uv=sprite.rect;var tex=sprite.texture;
        float x=(uv.x+(local.x-area.x)/area.width*uv.width)/tex.width;
        float y=(uv.y+(local.y-area.y)/area.height*uv.height)/tex.height;
        return tex.GetPixelBilinear(x,y).a>=.08f;
    }
}
