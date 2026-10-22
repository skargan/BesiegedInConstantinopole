using UnityEngine;
using UnityEngine.UI;
// Decorative artwork follows the existing campaign phase, never its own calendar.
public class SceneBackdrop:MonoBehaviour {
 public static string Phase(string area)=>area+(VariableSingleton.HasSiegeStarted()?"-war":"-peace");
 public static string ForCouncil(string title){switch(title){case "Diplomacy and court":return "diplomacy";case "Treasury and market":return Phase("market");case "Spies and intelligence":return "intelligence";case "The Imperial Fleet":return Phase("fleet");default:return null;}}
 public static void Apply(GameObject root,string key){
  var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+key);if(texture==null)return;
  var found=root.transform.Find("Scene artwork");GameObject obj;
  if(found==null){obj=new GameObject("Scene artwork",typeof(RectTransform),typeof(RawImage),typeof(SceneBackdrop));obj.transform.SetParent(root.transform,false);}else obj=found.gameObject;
  obj.transform.SetAsFirstSibling();var rect=obj.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
  var image=obj.GetComponent<RawImage>();image.texture=texture;image.color=Color.white;image.raycastTarget=false;obj.GetComponent<SceneBackdrop>().Fit();SceneParticles.Attach(obj,key);
 }
 void OnRectTransformDimensionsChange(){Fit();}
 void Fit(){var image=GetComponent<RawImage>();if(image==null||image.texture==null)return;var r=((RectTransform)transform).rect;if(r.width<=0||r.height<=0)return;float target=r.width/r.height,source=(float)image.texture.width/image.texture.height;float w=1,h=1;if(source>target)w=target/source;else h=source/target;image.uvRect=new Rect((1-w)/2,(1-h)/2,w,h);}
}
