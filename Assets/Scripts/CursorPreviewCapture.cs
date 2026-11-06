#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Opt-in exported-player proof; stays on the main menu and never opens a campaign.
public sealed class CursorPreviewCapture : MonoBehaviour
{
    const string Flag="-cursor-preview";
    static string output;
    int checks;
    static bool Requested=>Environment.GetCommandLineArgs().Contains(Flag);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void IsolateArchives()
    {
        if(!Requested)return;
        output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Captures","CursorPreview"));
        Directory.CreateDirectory(output);
        SaveGames.TestRoot=Path.Combine(output,"IsolatedSaves");
        VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"unused-preview-checkpoint.json");
        // Automated=true makes MainMenuUI skip importing the user's legacy saves.
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin()
    {
        if(!Requested)return;
        var runner=new GameObject("Cursor preview verification");
        DontDestroyOnLoad(runner);
        runner.AddComponent<CursorPreviewCapture>();
    }

    void Check(bool condition,string message)
    {
        if(!condition)throw new InvalidOperationException("CURSOR_CHECK_FAILED: "+message);
        checks++;
    }

    IEnumerator Start()
    {
        Application.runInBackground=true;
        yield return null;
        yield return new WaitForSecondsRealtime(.35f);
        try {
            VerifyAndCapture();
            string result="CURSOR_CHECKS "+checks+" passed";
            File.AppendAllText(Path.Combine(output,"cursor-checks.txt"),result+Environment.NewLine);
            Debug.Log(result);Application.Quit(0);
        }catch(Exception error){
            Debug.LogException(error);
            File.WriteAllText(Path.Combine(output,"cursor-failure.txt"),error.ToString());
            Application.Quit(1);
        }
    }

    void VerifyAndCapture()
    {
        Check(SaveGames.Automated&&SaveGames.Root==Path.GetFullPath(SaveGames.TestRoot),"isolated archives before menu");
        Check(FindObjectOfType<VariableSingleton>()==null,"no campaign loaded");
        var menu=FindObjectOfType<MainMenuUI>();Check(menu!=null,"main menu loaded");
        var cursors=FindObjectsOfType<ByzantineCursor>();Check(cursors.Length==1,"one cursor controller");
        var cursor=cursors[0];
        var hand=cursor.GetComponentInChildren<RawImage>(true);
        var expected=Resources.Load<Texture2D>(ByzantineCursor.ResourcePath);
        Check(expected!=null&&hand!=null&&hand.texture==expected,"displayed image uses requested resource");
        Check(expected.width>0&&expected.height>0&&expected.width==expected.height*2,"two square cursor cells prevent stretched proportions");
        Canvas.ForceUpdateCanvases();
        Check(Mathf.Approximately(hand.rectTransform.rect.width,ByzantineCursor.PixelSize)&&Mathf.Approximately(hand.rectTransform.rect.height,ByzantineCursor.PixelSize),"actual cursor dimensions match PixelSize");
        Check(Mathf.Approximately(ByzantineCursor.PixelSize,56f),"preview uses actual 56 pixel cursor");
        Check(!hand.raycastTarget,"cursor does not intercept input");
        Check(Vector2.Distance(hand.rectTransform.pivot,ByzantineCursor.Hotspot)<.00001f,"displayed hotspot matches controller constant");
        var button=menu.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.interactable&&b.GetComponentInChildren<TMP_Text>()?.text.IndexOf("New campaign",StringComparison.OrdinalIgnoreCase)>=0)
            ??menu.GetComponentsInChildren<Button>().First(b=>b.interactable);
        var bounds=button.GetComponent<RectTransform>();
        Vector3 point=bounds.TransformPoint(new Vector3(bounds.rect.xMin+bounds.rect.width*.18f,bounds.rect.center.y));
        var position=RectTransformUtility.WorldToScreenPoint(null,point);
        cursor.enabled=false;Cursor.visible=false;
        var layer=cursor.GetComponent<Canvas>();layer.enabled=true;
        hand.rectTransform.position=position;
        Check(layer.enabled&&hand.gameObject.activeInHierarchy,"cursor forced visible while player is unfocused");
        File.WriteAllText(Path.Combine(output,"cursor-checks.txt"),
            "Resource: "+ByzantineCursor.ResourcePath+"\nActual texture: "+hand.texture.name+
            "\nImported dimensions: "+expected.width+" x "+expected.height+
            "\nImported format: "+expected.format+"\nScreen: "+Screen.width+" x "+Screen.height+
            "\nActual cursor rectangle: "+hand.rectTransform.rect+"\nHotspot: "+hand.rectTransform.pivot+
            "\nPosition: "+position+"\nUnderlying button: "+button.name+"\n");
        Capture("01-cursor-actual-size.png");
        AddEnlargedPanel(expected);
        Capture("02-cursor-four-times.png");
        hand.uvRect=new Rect(.5f,0,.5f,1);hand.rectTransform.pivot=new Vector2(.5f,.65f);
        var enlarged=GameObject.Find("Enlarged cursor").GetComponent<RawImage>();enlarged.uvRect=hand.uvRect;
        Capture("03-grip-four-times.png");
        Check(FindObjectOfType<VariableSingleton>()==null,"preview remains outside campaign");
    }

    void AddEnlargedPanel(Texture texture)
    {
        var root=new GameObject("Cursor enlargement",typeof(RectTransform),typeof(Canvas));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32759;
        var panel=CampaignHUD.Panel(root.transform,"Four times cursor",Vector2.zero,Vector2.zero);
        var rect=panel.GetComponent<RectTransform>();rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(360,330);rect.anchoredPosition=new Vector2(Screen.width*.27f,Screen.height*.5f);
        ImperialTheme.Panel(panel,false);panel.GetComponent<Image>().raycastTarget=false;
        var label=CampaignHUD.Label(panel.transform,"Cursor · 4× enlargement",new Vector2(.05f,.85f),new Vector2(.95f,.98f),23);label.alignment=TextAlignmentOptions.Center;
        var obj=new GameObject("Enlarged cursor",typeof(RectTransform),typeof(RawImage));obj.transform.SetParent(panel.transform,false);
        var image=obj.GetComponent<RawImage>();image.texture=texture;image.uvRect=new Rect(0,0,.5f,1);image.raycastTarget=false;
        var area=obj.GetComponent<RectTransform>();area.anchorMin=area.anchorMax=new Vector2(.5f,.45f);area.pivot=new Vector2(.5f,.5f);area.sizeDelta=Vector2.one*ByzantineCursor.PixelSize*4;
    }

    void Capture(string name)
    {
        // Explicit rendering includes disabled-window canvases without relying on a live back buffer.
        var cameraObject=new GameObject("Cursor capture camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();
        var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try {
            camera.targetTexture=target;
            foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
            shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();
            File.WriteAllBytes(Path.Combine(output,name),shot.EncodeToPNG());
        }finally{
            RenderTexture.active=previous;
            for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
            camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(cameraObject);
        }
    }
}
#endif
