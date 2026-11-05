using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Local background music only; never consumes campaign RNG.
public sealed class SoundtrackLibrary:MonoBehaviour
{
    static SoundtrackLibrary instance;
    string[] files=new string[0];int index=-1;bool loading;float retryAt;AudioClip custom,original;AudioSource player;bool originalLoop;
    public string Status {get;private set;}="";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static string TestFolder;
#endif
    public static string Folder {
        get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(!string.IsNullOrEmpty(TestFolder))return TestFolder;
#endif
            return Path.Combine(Application.persistentDataPath,"Soundtrack");
        }
    }
    public static bool Enabled=>PlayerPrefs.GetInt("Besieged.CustomSoundtrack",0)==1;
    public static SoundtrackLibrary Instance {get{if(instance==null){var go=new GameObject("Custom soundtrack");DontDestroyOnLoad(go);instance=go.AddComponent<SoundtrackLibrary>();instance.Refresh();}return instance;}}
    public int Count=>files.Length;
    public static bool Supports(string path)=>new[]{".mp3",".wav",".ogg"}.Contains(Path.GetExtension(path).ToLowerInvariant());
    public void OpenFolder(){try{Directory.CreateDirectory(Folder);Application.OpenURL(new Uri(Folder+Path.DirectorySeparatorChar).AbsoluteUri);}catch(Exception e){Status="Cannot open soundtrack folder: "+e.Message;}}
    public void Refresh(){try{Directory.CreateDirectory(Folder);files=Directory.GetFiles(Folder).Where(Supports).OrderBy(p=>p,StringComparer.OrdinalIgnoreCase).ToArray();index=-1;Status=files.Length+" custom tracks found.";}catch(Exception e){files=new string[0];Status="Cannot read soundtrack folder: "+e.Message;}}
    public void SetEnabled(bool value){PlayerPrefs.SetInt("Besieged.CustomSoundtrack",value?1:0);PlayerPrefs.Save();if(!value)Restore();else{Refresh();Next();}}
    public void Next(){if(!Enabled||loading||files.Length==0)return;index=(index+1)%files.Length;StartCoroutine(Load(files[index]));}
    IEnumerator Load(string path){loading=true;string ext=Path.GetExtension(path).ToLowerInvariant();AudioType type=ext==".mp3"?AudioType.MPEG:ext==".ogg"?AudioType.OGGVORBIS:AudioType.WAV;
        using(var request=UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri,type)){
            yield return request.SendWebRequest();
            if(request.result!=UnityWebRequest.Result.Success){Status="Could not play "+Path.GetFileName(path)+". Use a valid MP3, WAV or OGG file.";retryAt=Time.unscaledTime+10;loading=false;yield break;}
            var clip=DownloadHandlerAudioClip.GetContent(request);if(!Enabled){Destroy(clip);loading=false;yield break;}
            var source=Settings.Instance.audioSource;
            if(source==null){Destroy(clip);Status="Music player not ready yet.";loading=false;yield break;}
            if(player!=source){player=source;original=source.clip;originalLoop=source.loop;}
            source.Stop();var previous=custom;custom=clip;source.clip=custom;source.loop=false;source.Play();if(previous!=null)Destroy(previous);Status=Path.GetFileNameWithoutExtension(path);
        }loading=false;
    }
    void Restore(){StopAllCoroutines();loading=false;if(player!=null&&player.clip==custom){player.Stop();player.clip=original;player.loop=originalLoop;if(original!=null)player.Play();}if(custom!=null)Destroy(custom);custom=null;player=null;}
    void Update(){if(!Enabled||loading||files.Length==0||Time.unscaledTime<retryAt)return;var source=Settings.Instance.audioSource;if(source==null)return;if(player!=source||custom==null||(source.clip==custom&&!source.isPlaying&&Settings.Instance.MusicEnabled))Next();}
    void OnDestroy(){if(instance==this)instance=null;if(custom!=null)Destroy(custom);}
}
