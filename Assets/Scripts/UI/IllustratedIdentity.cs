using UnityEngine;
using UnityEngine.UI;
public static class IllustratedIdentity {
    public static void Faith(Transform parent,string religion,bool orthodox,Vector2 min,Vector2 max) {
        int cell=religion=="Muslim"?2:orthodox?1:0;
        Atlas(parent,"UI/Identity/faith-medallions",new Rect(cell/3f,0,1f/3f,1),min,max,.85f);
    }
    public static void Portrait(Transform parent,int index) {
        CharacterPortraits.Draw(parent,index,new Vector2(.015f,.06f),new Vector2(.18f,.94f));
    }
    static void Atlas(Transform parent,string resource,Rect uv,Vector2 min,Vector2 max,float ratio) {
        var holder=CampaignHUD.Panel(parent,"Illustrated identity",min,max);holder.GetComponent<Image>().color=Color.clear;holder.GetComponent<Image>().raycastTarget=false;
        var obj=new GameObject("Artwork",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));obj.transform.SetParent(holder.transform,false);
        var fit=obj.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=ratio;
        var image=obj.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>(resource);image.uvRect=uv;image.raycastTarget=false;if(image.texture!=null)fit.aspectRatio=(image.texture.width*uv.width)/(image.texture.height*uv.height);
    }
}
