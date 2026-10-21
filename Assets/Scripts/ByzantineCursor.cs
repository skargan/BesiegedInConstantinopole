using UnityEngine;
using UnityEngine.UI;

// Software cursor keeps the generated alpha intact and does not require readable texture imports.
public class ByzantineCursor:MonoBehaviour
{
    private RectTransform hand;private Canvas layer;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartCursor(){if(FindObjectOfType<ByzantineCursor>()!=null)return;var go=new GameObject("Byzantine hand cursor");DontDestroyOnLoad(go);go.AddComponent<ByzantineCursor>();}
    private void Awake(){
        var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/byzantine-hand");if(texture==null){enabled=false;return;}
        layer=gameObject.AddComponent<Canvas>();layer.renderMode=RenderMode.ScreenSpaceOverlay;layer.sortingOrder=32760;
        var imageObject=new GameObject("Hand",typeof(RectTransform),typeof(RawImage));imageObject.transform.SetParent(transform,false);hand=imageObject.GetComponent<RectTransform>();hand.sizeDelta=new Vector2(64,64);hand.pivot=new Vector2(.432f,.942f);
        var image=imageObject.GetComponent<RawImage>();image.texture=texture;image.raycastTarget=false;
    }
    private void LateUpdate(){if(layer==null)return;Vector3 p=Input.mousePosition;bool inside=p.x>=0&&p.y>=0&&p.x<Screen.width&&p.y<Screen.height&&Application.isFocused;layer.enabled=inside;Cursor.visible=!inside;hand.position=p;}
    private void OnApplicationFocus(bool focus){if(!focus){Cursor.visible=true;if(layer!=null)layer.enabled=false;}}
    private void OnDisable(){Cursor.visible=true;if(layer!=null)layer.enabled=false;}
    private void OnApplicationQuit(){Cursor.visible=true;}
}
