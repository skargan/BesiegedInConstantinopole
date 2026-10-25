using UnityEngine;
using UnityEngine.UI;
// Stable character IDs; portraits never affect recruitment, loyalty or availability.
public static class CharacterPortraits {
 public static readonly int[] People={0,1,2,3,4,5,6,7,8,100,101,102,103,104,105,200,201,202};
 public static string Resource(int id)=>id>=0&&id<=8?"UI/Identity/court-portraits":System.Array.IndexOf(People,id)>=0?"UI/Identity/Characters/"+id:null;
 public static Texture2D Texture(int id){var path=Resource(id);return path==null?null:Resources.Load<Texture2D>(path);}
 public static Rect UV(int id){
  if(id<0||id>8)return new Rect(0,0,1,1);
  // Sheet boundaries measured on the supplied 1223 x 1286 artwork, not assumed equal cells.
  float[] x={0,416f/1223,809f/1223,1},y={0,416f/1286,822f/1286,1};int c=id%3,r=id/3;
  return new Rect(x[c],1-y[r+1],x[c+1]-x[c],y[r+1]-y[r]);
 }
 public static GameObject Draw(Transform parent,int id,Vector2 min,Vector2 max){
  var texture=Texture(id);if(texture==null)return null;
  var holder=CampaignHUD.Panel(parent,"Portrait "+id,min,max);holder.GetComponent<Image>().color=Color.clear;holder.GetComponent<Image>().raycastTarget=false;
  var frame=new GameObject("Portrait frame",typeof(RectTransform),typeof(Image),typeof(AspectRatioFitter));frame.transform.SetParent(holder.transform,false);
  var uv=UV(id);var fit=frame.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture.width*uv.width/(texture.height*uv.height);
  var edge=frame.GetComponent<Image>();edge.color=new Color(.65f,.48f,.22f);edge.raycastTarget=false;
  var face=new GameObject("Face",typeof(RectTransform),typeof(RawImage));face.transform.SetParent(frame.transform,false);
  var rt=face.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(2,2);rt.offsetMax=new Vector2(-2,-2);
  var image=face.GetComponent<RawImage>();image.texture=texture;image.uvRect=uv;image.raycastTarget=false;return holder;
 }
}
