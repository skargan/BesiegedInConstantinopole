#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class PalaceFigureCapture : MonoBehaviour
{
    string output;CouncilUI council;VariableSingleton.CampaignSave baseline;readonly List<string> results=new List<string>();
    static readonly int[] People={0,1,2,3,4,5,6,7,100,101,102,103,104,105,200};
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-palace-figure-capture")&&!Environment.GetCommandLineArgs().Contains("-compactdesk-capture"))return;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/PalaceFigures"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Palace figure verification");DontDestroyOnLoad(go);go.AddComponent<PalaceFigureCapture>().output=folder;}
    void Check(bool ok,string label){if(!ok)throw new InvalidOperationException(label);results.Add("PASS "+label);}
    string State(){var s=VariableSingleton.Instance.CaptureState();s.randomState=default(UnityEngine.Random.State);return JsonUtility.ToJson(s);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    RectTransform Painting()=>GameObject.Find("Location painting").GetComponent<RectTransform>();
    Vector2 Point(RectTransform r,float x,float y)=>RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(r.rect.xMin+r.rect.width*x,r.rect.yMin+r.rect.height*y)));
    GameObject Hit(Vector2 point){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);return hits.Count==0?null:hits[0].gameObject;}
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED\n"+failure);Debug.LogException(failure);Application.Quit(1);yield break;}
            if(!moved){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        string report="PALACE_FIGURE_CHECKS "+results.Count+" passed; all 15 IDs in all three locations at 1600x900 and 1280x720; palace, Garden Kjold and Hagia Isidore screenshots.";File.WriteAllLines(Path.Combine(output,"details.txt"),results);File.WriteAllText(Path.Combine(output,"checks.txt"),report);Debug.Log(report);Application.Quit(0);}
    void Fixture(int person,int place=0){CampaignHUD.Instance.DismissReport();council.Hide();VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline)));
        if(person<8)VariableSingleton.ChangeBool(CourtRoster.Keys[person],true);
        if(person>=100&&person<106){VariableSingleton.SetInt("iAdventurerChoice",person-99);VariableSingleton.SetInt("iAdventurerArrived",1);}
        if(person==200)VariableSingleton.SetInt("iKjoldState",1);
        VariableSingleton.SetInt("iHardOfficerReturn"+person,0);
        VariableSingleton.SetInt("iLifeLocationTurn",VariableSingleton.Turn+1);for(int p=0;p<3;p++)VariableSingleton.SetInt("iLifeLocationVisitor"+p,p==place?person:-1);
        // Sanctuary recovery is an existing first-visit action; isolate presentation checks from it.
        VariableSingleton.SetInt("iLifeSanctuaryVisit",VariableSingleton.Turn+1);}
    void OpenLocation(int place){if(place==0)council.Palace();else if(place==1)council.Garden();else council.HagiaSophia();}
    sealed class PortalSample{public string name;public float x,y;public int destination;public PortalSample(string n,float px,float py,int d){name=n;x=px;y=py;destination=d;}}
    static PortalSample[] Portals(int p)=>p==0?new[]{new PortalSample("To the garden",.897f,.60f,1),new PortalSample("Hagia Sophia",.410f,.705f,2),new PortalSample("Defence of the walls",.60f,.32f,-1)}:p==1?new[]{new PortalSample("Return to the Palace",.045f,.60f,0),new PortalSample("Hagia Sophia",.695f,.69f,2)}:new[]{new PortalSample("Return to the Palace",.145f,.40f,0),new PortalSample("Walk in the garden",.81f,.35f,1)};
    IEnumerator Run(){AudioListener.volume=0;Application.runInBackground=true;yield return null;Check(SaveGames.Automated,"isolated archives");SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+20;
        while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;Check(VariableSingleton.IsInitialized,"campaign initialized");yield return new WaitForSecondsRealtime(.5f);
        council=CouncilUI.For(FindObjectOfType<ManagerUI>());baseline=VariableSingleton.Instance.CaptureState();
        if(Environment.GetCommandLineArgs().Contains("-compactdesk-capture")){yield return CheckCompactDesks();yield return CheckPreferences();yield break;}

        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);Check(Screen.width==size.x&&Screen.height==size.y,"resolution "+size);
            for(int place=0;place<3;place++)foreach(int person in People){Fixture(person,place);string before=State(),rng=Rng();OpenLocation(place);yield return null;Canvas.ForceUpdateCanvases();string context=size.x+"x"+size.y+" location"+place+" ID"+person;
                var go=GameObject.Find("Palace court figure "+person);Check(go!=null,context+" actual full-body asset exists");var view=go.GetComponent<PalaceCourtFigure>();var image=go.GetComponent<Image>();var rect=go.GetComponent<RectTransform>();
                Check(image.sprite!=null&&image.sprite.texture.isReadable&&image.alphaHitTestMinimumThreshold>0,context+" alpha-backed silhouette hit-testing active");
                var placement=PalaceCourtFigure.Placement(place);Check(view.Location==place&&Mathf.Abs(rect.anchorMin.x-placement.x)<.0001f&&Mathf.Abs(rect.anchorMin.y-placement.y)<.0001f&&rect.pivot==new Vector2(.5f,0)&&rect.rect.height<=Painting().rect.height*placement.z+.5f,context+" bottom-centred feet and location-specific bounded height");
                Check(!Painting().GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("Portrait ")),context+" legacy portrait card removed from location");
                Check(before==State()&&rng==Rng(),context+" opening preserves visitor state and RNG");
                Check(PalaceFigureArt.CachedCount<=4,context+" lazy cache remains bounded after previous view releases");
                var plate=GameObject.Find("Palace figure name "+person).GetComponent<Button>();var text=plate.GetComponentInChildren<TMP_Text>();text.ForceMeshUpdate();Check(!text.isTextOverflowing,context+" nameplate fits");
                var portals=Painting().GetComponentsInChildren<LocationHotspot>();
                foreach(var target in Portals(place)){var portal=portals.First(p=>p.name=="Hotspot · "+target.name);Check(Hit(Point(Painting(),target.x,target.y))==portal.gameObject,context+" "+target.name+" remains clickable");}
                bool tested=false;var crop=image.sprite.rect;
                for(int yi=1;yi<10&&!tested;yi++)for(int xi=1;xi<10&&!tested;xi++){float x=xi/10f,y=yi/10f;var colour=image.sprite.texture.GetPixelBilinear((crop.x+crop.width*x)/image.sprite.texture.width,(crop.y+crop.height*y)/image.sprite.texture.height);
                    if(colour.a>.01f)continue;var point=Point(rect,x,y);Check(!image.IsRaycastLocationValid(point,null)&&Hit(point)!=go,context+" transparent margin falls through to underlying scene");tested=true;}
                Check(tested,context+" transparent sample found");
                foreach(float band in new[]{.25f,.55f,.85f,.94f}){bool opaque=false;for(int x=1;x<20;x++){float u=x/20f;var sr=image.sprite.rect;var tc=image.sprite.texture.GetPixelBilinear((sr.x+u*sr.width)/image.sprite.texture.width,(sr.y+band*sr.height)/image.sprite.texture.height);if(tc.a<.7f)continue;opaque=true;Check(image.IsRaycastLocationValid(Point(rect,u,band),null)&&Hit(Point(rect,u,band))==go,context+" silhouette clickable at height "+band);break;}Check(opaque,context+" body sample found at "+band);}
                view.OnPointerEnter(null);yield return null;Check(before==State()&&rng==Rng(),context+" hover has no gameplay effect");
                if((place==0&&person==0)||(place==1&&person==200)||(place==2&&person==7))yield return Capture(new[]{"palace","garden-kjold","hagia-isidore"}[place]+"-"+size.x+"x"+size.y);view.OnPointerExit(null);
                if(person==0||person==100||(place==1&&person==200)||(place==2&&person==7)){go.GetComponent<Button>().onClick.Invoke();yield return null;Check(GameObject.Find("Council screen").GetComponentsInChildren<TMP_Text>().Any(t=>t.text==GardenConversations.Name(person)),context+" figure opens existing conversation");ControlSingleton.Instance.GoBack();yield return null;Check(GameObject.Find("Palace court figure "+person)!=null,context+" conversation returns to same visitor");
                    GameObject.Find("Palace figure name "+person).GetComponent<Button>().onClick.Invoke();yield return null;ControlSingleton.Instance.GoBack();yield return null;Check(before==State()&&rng==Rng(),context+" nameplate and return retain state and RNG");}
                if((place==0&&person==0)||(place==1&&person==200)||(place==2&&person==7)){foreach(var target in Portals(place).Where(t=>t.destination>=0)){var point=Point(Painting(),target.x,target.y);var portal=Hit(point).GetComponent<LocationHotspot>();portal.OnPointerClick(new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left});yield return null;Check(GameObject.Find("Council screen").GetComponentsInChildren<TMP_Text>().Any(t=>t.text==new[]{"The Palace","The Palace Garden","Hagia Sophia"}[target.destination]),context+" doorway opens "+target.name);OpenLocation(place);yield return null;Check(GameObject.Find("Palace court figure "+person)!=null,context+" return retains assigned visitor");}Check(before==State()&&rng==Rng(),context+" doorway travel preserves primed state and RNG");}
            }
        }
    }
    IEnumerator CheckCompactDesks(){
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);
            council.Hide();var manager=FindObjectOfType<ManagerUI>(true);manager.gameObject.SetActive(true);manager.BringUpTroopsUI();yield return null;
            Check(GameObject.Find("Military selection desk")!=null,"military right desk "+size);
            for(int i=0;i<6;i++){var select=GameObject.Find("Select military unit "+i).GetComponent<Button>();select.onClick.Invoke();yield return null;
                Check(GameObject.Find("Military selection desk").GetComponentsInChildren<Button>().Count()==12,"six selectors and six shared actions "+i+" "+size);}
            yield return Capture("military-desk-"+size.x);
            manager.BringUpTroopsUI();yield return null;Check(GameObject.Find("Select military unit 0")!=null,"military reopen "+size);
            council.Spies();yield return null;
            for(int i=0;i<10;i++){GameObject.Find("Select intelligence report "+i).GetComponent<Button>().onClick.Invoke();yield return null;
                var detail=GameObject.Find("Selected intelligence report");Check(detail.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==EspionageSystem.IntelligenceLabels[i]),"report selection "+i+" "+size);}
            yield return Capture("intelligence-desk-"+size.x);
            GameObject.Find("Intelligence operations tab").GetComponent<Button>().onClick.Invoke();yield return null;
            for(int i=0;i<6;i++)Check(GameObject.Find("Review spy mission "+i)!=null,"operation retained "+i+" "+size);
            yield return Capture("intelligence-operations-"+size.x);
        }
    }
    IEnumerator CheckPreferences(){
        council.Hide();var manager=FindObjectOfType<ManagerUI>(true);manager.gameObject.SetActive(true);council.Palace();yield return null;
        SaveMenu.Show(manager.transform.root);yield return null;
        var name=GameObject.Find("Save name").GetComponent<TMP_InputField>();Check(name.interactable,"named save field editable");name.text="The Emperor's Test Council";
        Check(SaveGames.Named(name.text,out var result),"named save succeeds: "+result);Check(SaveGames.Entries().Any(e=>e.archive!=null&&e.archive.name==name.text),"custom name read from archive");SaveMenu.Close();
        bool enabled=SoundtrackLibrary.Enabled;var settings=Settings.Instance;bool music=settings.MusicEnabled,sfx=settings.SfxEnabled;
        SoundtrackLibrary.Instance.SetEnabled(false);SoundtrackLibrary.TestFolder=Path.Combine(output,"Soundtrack");Directory.CreateDirectory(SoundtrackLibrary.Folder);
        using(var writer=new BinaryWriter(File.Create(Path.Combine(SoundtrackLibrary.Folder,"Test track.wav")))){int n=44100;writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+n*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(44100);writer.Write(88200);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(n*2);for(int i=0;i<n;i++)writer.Write((short)0);}
        var library=SoundtrackLibrary.Instance;library.Refresh();Check(library.Count==1,"WAV soundtrack discovered");Check(SoundtrackLibrary.Supports("song.MP3")&&!SoundtrackLibrary.Supports("notes.txt"),"music extension filter");
        library.SetEnabled(true);float until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until&&library.Status!="Test track")yield return null;
        Check(library.Status=="Test track","custom WAV decoded and played");Check(settings.SfxEnabled==sfx,"custom music preserves sound effects setting");
        settings.SetMusicEnabled(false);Check(settings.audioSource.mute,"custom music respects mute");settings.SetMusicEnabled(music);
        AudioSettingsUI.ShowSoundtrack(manager.transform.root);yield return null;yield return Capture("soundtrack-settings");AudioSettingsUI.Close();
        library.SetEnabled(false);SoundtrackLibrary.TestFolder=null;library.Refresh();library.SetEnabled(enabled);
        var trakaiState=VariableSingleton.Instance.CaptureState();int lithuania=CountryDiplomacy.Index("lithuania");
        VariableSingleton.SetInt(TrakaiIconGift.Key,0);VariableSingleton.SetInt("iDiplomacyEnvoy_"+lithuania,2);VariableSingleton.SetInt("iMoney",1749);
        Check(!TrakaiIconGift.Send(lithuania,out _)&&CouncilActions.Int("iMoney")==1749,"Trakai insufficient gold has no debit");
        VariableSingleton.SetInt("iMoney",5000);float goodwill=CountryDiplomacy.Goodwill(lithuania);
        Check(TrakaiIconGift.Send(lithuania,out _)&&CouncilActions.Int("iMoney")==3250,"Trakai charges exactly1750");
        Check(Mathf.Abs(CountryDiplomacy.Goodwill(lithuania)-Mathf.Min(95,goodwill+15))<.01f,"Trakai grants15goodwill withexistingcap");
        Check(!TrakaiIconGift.Send(lithuania,out _)&&CouncilActions.Int("iMoney")==3250,"Trakai cannot be purchased twice");
        VariableSingleton.Instance.RestoreValidated(trakaiState);VariableSingleton.SetInt("iDiplomacyEnvoy_"+lithuania,2);
        typeof(CouncilUI).GetMethod("ReviewTrakaiGift",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(council,new object[]{lithuania});yield return null;yield return Capture("trakai-icon-review");
        VariableSingleton.Instance.RestoreValidated(trakaiState);
        RomeVisitUI.Show();yield return null;
        Check(GameObject.Find("Rome basilica visit")!=null,"Rome view opens");
        Check(GameObject.Find("Old Saint Peter's painting").GetComponent<RawImage>().texture!=null,"Rome painting loaded");
        yield return Capture("rome-visit");
        var papacy=GameObject.Find("Rome basilica visit").GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text=="Diplomacy with the Papacy");papacy.onClick.Invoke();yield return null;
        Check(GameObject.Find("Rome basilica visit")==null,"Rome closes on diplomacy");
    }
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.1f);Canvas.ForceUpdateCanvases();var go=new GameObject("Palace capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}
        finally{RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);Canvas.ForceUpdateCanvases();}}
}
#endif
