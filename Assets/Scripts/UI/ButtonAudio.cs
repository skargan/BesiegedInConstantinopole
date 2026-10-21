using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// One shared source lets the click finish when its menu closes or changes scene.
public class ButtonAudio : MonoBehaviour
{
    private static ButtonAudio instance;
    private AudioSource source;
    private float nextScan;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        var obj=new GameObject("Interface audio");
        DontDestroyOnLoad(obj);instance=obj.AddComponent<ButtonAudio>();
        instance.source=obj.AddComponent<AudioSource>();
        instance.source.playOnAwake=false;instance.source.spatialBlend=0;
        instance.source.clip=WoodenThud();
        SceneManager.sceneLoaded+=instance.SceneLoaded;
    }
    private void SceneLoaded(Scene scene,LoadSceneMode mode) {
        foreach(var image in FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(image.name=="Event Screen")ImperialTheme.Panel(image.gameObject);
            if(image.name=="Panel" && image.transform.parent!=null && image.transform.parent.name=="Canvas") {image.sprite=null;image.color=ImperialTheme.Navy;}
            if(image.name=="ManagerBackdrop")image.color=new Color(.12f,.16f,.20f,1);
        }
        BindButtons();
    }
    private void OnDestroy() { SceneManager.sceneLoaded-=SceneLoaded; }
    private void Update()
    {
        if(Time.unscaledTime<nextScan) return;
        nextScan=Time.unscaledTime+.2f;BindButtons();
    }
    private static void BindButtons()
    {
        foreach(var button in FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Attach(button);
        foreach(var text in FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(text.font!=ImperialTheme.Font) text.font=ImperialTheme.Font;
    }
    public static void Attach(Button button)
    {
        if(button.GetComponent<ButtonAudioBound>()!=null) return;
        button.gameObject.AddComponent<ButtonAudioBound>();
        button.onClick.AddListener(Play);
        ImperialTheme.Button(button);
        var image=button.GetComponent<Image>();
        
        var label=button.GetComponentInChildren<TMPro.TMP_Text>(true);
        if(label!=null) {label.alignment=TMPro.TextAlignmentOptions.Center;label.margin=new Vector4(12,6,12,6);label.raycastTarget=false;}
        
    }
    public static void Play()
    {
        if(instance==null) return;
        var settings=Settings.Instance;
        if(!settings.SfxEnabled) return;
        float volume=settings!=null ? settings.MasterVolume*settings.SfxVolume : .49f;
        instance.source.PlayOneShot(instance.source.clip,volume);
    }
    public static AudioClip WoodenThud()
    {
        const int rate=44100;var data=new float[(int)(rate*.16f)];
        var noise=new System.Random(1453);float low=0;
        for(int i=0;i<data.Length;i++)
        {
            float t=(float)i/rate;
            low=.8f*low+.2f*((float)noise.NextDouble()*2-1);
            float attack=1-Mathf.Exp(-t*1400);
            data[i]=attack*(.42f*Mathf.Sin(2*Mathf.PI*185*t)*Mathf.Exp(-t*42)
                +.16f*Mathf.Sin(2*Mathf.PI*397*t)*Mathf.Exp(-t*60)
                +.2f*low*Mathf.Exp(-t*100))*Mathf.Clamp01((.16f-t)/.02f);
        }
        var clip=AudioClip.Create("Soft wooden button thud",data.Length,1,rate,false);clip.SetData(data,0);return clip;
    }
}
public class ButtonAudioBound : MonoBehaviour { }
