using UnityEngine;
using UnityEngine.UI;

// Software cursor keeps the generated alpha intact and does not require readable texture imports.
public class ByzantineCursor:MonoBehaviour
{
    public const string ResourcePath = "UI/MidnightMosaic/cursor-black-gold-pair";
    public const float PixelSize = 56f;
    public static readonly Vector2 Hotspot = new Vector2(208f/887f, 1f-168f/887f);
    private RectTransform hand;private Canvas layer;private RawImage image;
    static Object gripOwner;
    public static bool IsGripping=>gripOwner!=null;
    public static void SetGripping(Object owner,bool active){if(active)gripOwner=owner;else if(gripOwner==owner)gripOwner=null;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartCursor(){if(FindObjectOfType<ByzantineCursor>()!=null)return;var go=new GameObject("Byzantine hand cursor");DontDestroyOnLoad(go);go.AddComponent<ByzantineCursor>();}
    private void Awake(){
        var texture=Resources.Load<Texture2D>(ResourcePath);if(texture==null){enabled=false;return;}
        layer=gameObject.AddComponent<Canvas>();layer.renderMode=RenderMode.ScreenSpaceOverlay;layer.sortingOrder=32760;
        var imageObject=new GameObject("Hand",typeof(RectTransform),typeof(RawImage));imageObject.transform.SetParent(transform,false);hand=imageObject.GetComponent<RectTransform>();hand.sizeDelta=Vector2.one * PixelSize;hand.pivot=Hotspot; // Opaque upper-left fingertip is the click point.
        image=imageObject.GetComponent<RawImage>();image.texture=texture;image.uvRect=new Rect(0,0,.5f,1);image.raycastTarget=false;
    }
    private void LateUpdate(){if(layer==null)return;Vector3 p=Input.mousePosition;bool inside=p.x>=0&&p.y>=0&&p.x<Screen.width&&p.y<Screen.height&&Application.isFocused;layer.enabled=inside;Cursor.visible=!inside;hand.position=p;bool grip=gripOwner!=null;image.uvRect=new Rect(grip?.5f:0,0,.5f,1);hand.pivot=grip?new Vector2(.5f,.65f):Hotspot;if(!Input.GetMouseButton(0))gripOwner=null;}
    private void OnApplicationFocus(bool focus){if(!focus){gripOwner=null;Cursor.visible=true;if(layer!=null)layer.enabled=false;}}
    private void OnDisable(){Cursor.visible=true;if(layer!=null)layer.enabled=false;}
    private void OnApplicationQuit(){Cursor.visible=true;}
}

