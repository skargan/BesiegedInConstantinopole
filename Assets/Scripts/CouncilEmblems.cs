using UnityEngine;
using UnityEngine.UI;

// Council subjects are independent of the five strategic focus symbols.
public static class CouncilEmblems
{
    public static readonly string[] Names={"overview","military","fleet","spies","palace","rest","walls","society","diplomacy","technology","treasury"};
    private static readonly Sprite[] Cache=new Sprite[11];
    public static void Add(Transform parent,int index)
    {
        if(index<0||index>=Names.Length)return;
        if(Cache[index]==null){var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Council/"+Names[index]);if(texture==null)return;Cache[index]=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));}
        var icon=new GameObject("Council emblem",typeof(RectTransform),typeof(Image));icon.transform.SetParent(parent,false);
        var r=icon.GetComponent<RectTransform>();r.anchorMin=new Vector2(.025f,.10f);r.anchorMax=new Vector2(.22f,.90f);r.offsetMin=r.offsetMax=Vector2.zero;
        var image=icon.GetComponent<Image>();image.sprite=Cache[index];image.preserveAspect=true;image.raycastTarget=false;
    }
}
