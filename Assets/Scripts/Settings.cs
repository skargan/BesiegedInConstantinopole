using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public AudioSource audioSource;
    [Range(0, 1)] public float MasterVolume = 0.7f;
    [Range(0, 1)] public float MusicVolume = 0.7f;
    [Range(0, 1)] public float SfxVolume = 0.7f;
    private static Settings instance;
    public static Settings Instance { get { if(instance==null) new GameObject("Audio settings").AddComponent<Settings>(); return instance; } }
    public bool SfxEnabled { get; private set; } = true;
    public void SetSfxEnabled(bool enabled) { SfxEnabled=enabled;PlayerPrefs.SetInt("Besieged.SfxEnabled",enabled?1:0);PlayerPrefs.Save(); }
    public bool MusicEnabled { get; private set; } = true;
    public void SetMusicEnabled(bool enabled) {MusicEnabled=enabled;PlayerPrefs.SetInt("Besieged.MusicEnabled",enabled?1:0);PlayerPrefs.Save();ApplyVolume();}
    public void SetMusicVolume(float volume) {MusicVolume=Mathf.Clamp01(volume);PlayerPrefs.SetFloat("Besieged.MusicVolume",MusicVolume);PlayerPrefs.Save();ApplyVolume();}

    public void SetFullscreen(bool enabled)
    {
        PlayerPrefs.SetInt("Besieged.Fullscreen",enabled?1:0);PlayerPrefs.Save();
        Screen.fullScreenMode=enabled?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed;
    }

    private void OnValidate() { ApplyVolume(); }
    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        SfxEnabled=PlayerPrefs.GetInt("Besieged.SfxEnabled",1)!=0;
        MusicEnabled=PlayerPrefs.GetInt("Besieged.MusicEnabled",1)!=0;
        MusicVolume=PlayerPrefs.GetFloat("Besieged.MusicVolume",MusicVolume);
        // Explicit capture/window arguments take precedence over saved display preferences.
        var args=System.Environment.GetCommandLineArgs();
        if(!args.Contains("-besieged-capture")&&!args.Contains("-screen-fullscreen"))
            Screen.fullScreenMode=PlayerPrefs.GetInt("Besieged.Fullscreen",1)==1?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += ConnectSettings;
    }
    private void OnDestroy() { SceneManager.sceneLoaded -= ConnectSettings; }
    private void ConnectSettings(Scene scene, LoadSceneMode mode)
    {
        audioSource = FindObjectsOfType<AudioSource>().FirstOrDefault(a=>a.GetComponent<ButtonAudio>()==null);
        ApplyVolume();
    }
    private void ApplyVolume()
    {
        if(audioSource==null) audioSource=FindObjectsOfType<AudioSource>().FirstOrDefault(a=>a.GetComponent<AudioSingleton>()!=null);
        if (audioSource != null) {audioSource.volume = MasterVolume * MusicVolume;audioSource.mute=!MusicEnabled;}
    }
}
