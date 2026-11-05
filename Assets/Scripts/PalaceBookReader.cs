using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Text stays independent of generated art: no rasterised prose or clipped paragraphs.
public sealed class PalaceBookReader : MonoBehaviour
{
    [Serializable] public sealed class Book {public string title,subtitle,text;}
    [Serializable] sealed class Library {public Book[] books;}
    static Library library;static PalaceBookReader active;
    static readonly string[] Images={"empire","city","sunset","emperor","fall"};
    TMP_Text copy,counter;Button previous,next;int page;
    public static Book[] Books {get{if(library==null){var data=Resources.Load<TextAsset>("Content/PalaceBooks");library=JsonUtility.FromJson<Library>(data.text);}return library.books;}}
    public static void Open(int index)
    {
        if(active!=null)Destroy(active.gameObject);
        var root=new GameObject("Palace library reader",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(PalaceBookReader));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=280;
        var scale=root.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.matchWidthOrHeight=.5f;
        active=root.GetComponent<PalaceBookReader>();active.Draw(index);PalaceBookSound.Play();
    }
    void Draw(int index)
    {
        var shade=CampaignHUD.Panel(transform,"Reading shade",Vector2.zero,Vector2.one);shade.GetComponent<Image>().color=new Color(0,0,0,.86f);
        var leaf=CampaignHUD.Panel(shade.transform,"Illustrated leaf",new Vector2(.035f,.04f),new Vector2(.965f,.96f));
        var image=leaf.AddComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/PalaceLibrary/"+Images[index]);leaf.GetComponent<Image>().color=Color.clear;
        var ink=new Color(.17f,.105f,.055f);var book=Books[index];
        var title=CampaignHUD.Label(leaf.transform,book.title,new Vector2(.40f,.86f),new Vector2(.95f,.95f),30);title.color=ink;title.alignment=TextAlignmentOptions.Center;title.enableAutoSizing=true;title.fontSizeMin=22;title.fontSizeMax=30;
        var sub=CampaignHUD.Label(leaf.transform,book.subtitle,new Vector2(.40f,.80f),new Vector2(.95f,.86f),20);sub.color=ink;sub.alignment=TextAlignmentOptions.Center;
        copy=CampaignHUD.Label(leaf.transform,book.text,new Vector2(.42f,.16f),new Vector2(.94f,.78f),23);copy.color=ink;copy.alignment=TextAlignmentOptions.TopLeft;copy.enableWordWrapping=true;copy.overflowMode=TextOverflowModes.Page;copy.richText=false;copy.lineSpacing=3;copy.paragraphSpacing=8;
        previous=CampaignHUD.MakeButton(leaf.transform,"Previous",new Vector2(.42f,.055f),new Vector2(.55f,.12f),()=>Turn(-1));
        next=CampaignHUD.MakeButton(leaf.transform,"Next",new Vector2(.70f,.055f),new Vector2(.83f,.12f),()=>Turn(1));
        counter=CampaignHUD.Label(leaf.transform,"",new Vector2(.56f,.055f),new Vector2(.69f,.12f),21);counter.color=ink;counter.alignment=TextAlignmentOptions.Center;
        CampaignHUD.MakeButton(leaf.transform,"Close",new Vector2(.845f,.055f),new Vector2(.965f,.12f),()=>Destroy(gameObject));
        Canvas.ForceUpdateCanvases();copy.ForceMeshUpdate();page=1;Refresh();
    }
    void Turn(int change){page=Mathf.Clamp(page+change,1,Mathf.Max(1,copy.textInfo.pageCount));Refresh();PalaceBookSound.Play();}
    void Refresh(){copy.pageToDisplay=page;int pages=Mathf.Max(1,copy.textInfo.pageCount);counter.text=page+" / "+pages;previous.interactable=page>1;next.interactable=page<pages;}
    void Update(){if(Input.GetKeyDown(KeyCode.Escape))Destroy(gameObject);else if(Input.GetKeyDown(KeyCode.RightArrow))Turn(1);else if(Input.GetKeyDown(KeyCode.LeftArrow))Turn(-1);}
}

public sealed class PalaceBookSound:MonoBehaviour
{
    static PalaceBookSound instance;AudioSource source;AudioClip paper;
    public static void Play(){var settings=Settings.Instance;if(settings!=null&&!settings.SfxEnabled)return;
        if(instance==null){var go=new GameObject("Library paper audio");DontDestroyOnLoad(go);instance=go.AddComponent<PalaceBookSound>();instance.source=go.AddComponent<AudioSource>();instance.source.playOnAwake=false;instance.source.spatialBlend=0;instance.paper=MakePaper();}
        instance.source.PlayOneShot(instance.paper,settings==null?.35f:settings.MasterVolume*settings.SfxVolume*.65f);}
    static AudioClip MakePaper(){const int rate=22050;var samples=new float[rate/2];var random=new System.Random(902);float previous=0;
        for(int i=0;i<samples.Length;i++){float t=(float)i/rate,noise=(float)random.NextDouble()*2-1;
            float envelope=Mathf.Sin(Mathf.PI*t/.5f)*Mathf.Exp(-t*2);float pulse=.55f+.45f*Mathf.Sin(t*61);
            samples[i]=(noise-previous*.8f)*envelope*pulse*.22f+Mathf.Sin(t*550)*Mathf.Exp(-t*55)*.12f;previous=noise;}
        var clip=AudioClip.Create("Opening parchment leaves",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;}
}
