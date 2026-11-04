#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MaritimeMedallionCapture : MonoBehaviour
{
    string output;int passed;int oldWidth,oldHeight;FullScreenMode oldMode;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin()
    {
        if(!Environment.GetCommandLineArgs().Contains("-maritime-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Maritime"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");
        Directory.CreateDirectory(SaveGames.TestRoot);
        var go=new GameObject("Maritime verification");DontDestroyOnLoad(go);go.AddComponent<MaritimeMedallionCapture>().output=path;
    }
    void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add("PASS "+name);passed++;}
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    void Capture(string filename)
    {
        Canvas.ForceUpdateCanvases();var go=new GameObject("Maritime capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,filename),shot.EncodeToPNG());}
        finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);}
    }
    MaritimeMapMedallions Badge(bool passage)=>FindObjectsOfType<MaritimeMapMedallions>().Single(x=>x.Passage==passage);
    IEnumerator Start()
    {
        oldWidth=Screen.width;oldHeight=Screen.height;oldMode=Screen.fullScreenMode;
        var stack=new Stack<IEnumerator>();stack.Push(Run());Exception failure=null;
        while(stack.Count>0)
        {
            object next=null;bool moved=false;
            try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null)break;
            if(!moved){stack.Pop();continue;}if(next is IEnumerator child)stack.Push(child);else yield return next;
        }
        // Do not leave a QA-only resolution behind for the next normal launch.
        Screen.SetResolution(oldWidth,oldHeight,oldMode);yield return new WaitForSecondsRealtime(.4f);
        File.WriteAllLines(Path.Combine(output,"details.txt"),checks);
        File.WriteAllText(Path.Combine(output,"checks.txt"),failure==null?"MARITIME_CHECKS "+passed+" passed":"FAILED "+failure);
        if(failure!=null)Debug.LogException(failure);Application.Quit(failure==null?0:1);
    }
    IEnumerator Run()
    {
        AudioListener.volume=0;Application.runInBackground=true;yield return null;
        SaveMenu.StartCampaign(false);float deadline=Time.realtimeSinceStartup+25;
        while(!VariableSingleton.IsInitialized&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"Campaign initialised with isolated saves");yield return new WaitForSecondsRealtime(.6f);CampaignHUD.Instance.DismissReport();
        foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
        // Populate existing lazy read defaults before checking navigation for mutations.
        var council=FindObjectOfType<CouncilUI>(true);council.Fleet();yield return null;council.Hide();
        foreach(string id in new[]{"sea-imperial","sea-contested","sea-ottoman","strait-open","strait-interdicted"})
            Check(Resources.Load<Texture2D>("UI/Maps/Maritime/"+id)!=null,"Imported "+id);
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);
            foreach(var numbers in new[]{new Vector2Int(60,48),new Vector2Int(48,48),new Vector2Int(26,48),Vector2Int.zero})
            {
                VariableSingleton.SetInt("iShips",numbers.x);VariableSingleton.SetInt("iTurkShips",numbers.y);
                foreach(bool open in new[]{false,true})
                {
                    VariableSingleton.ChangeBool("bStraitsOpen",open);WarTheatreUI.Show();yield return null;Canvas.ForceUpdateCanvases();
                    var sea=Badge(false);var strait=Badge(true);
                    Check(sea.GetComponentInChildren<TMP_Text>().text=="Sea control\n"+NavalState.SeaControl,"Sea caption matches fleet rules");
                    Check(MaritimeMapMedallions.SeaTitle(numbers.x,numbers.y)==NavalState.SeaControl,"SeaTitle agrees with authoritative rules");
                    Check(sea.GetComponent<RawImage>().texture.name==MaritimeMapMedallions.SeaAsset(numbers.x,numbers.y),"Correct naval image "+numbers);
                    Check(strait.GetComponent<RawImage>().texture.name==(open?"strait-open":"strait-interdicted"),"Independent passage image "+open);
                    string state=State();string rng=JsonUtility.ToJson(UnityEngine.Random.state);
                    sea.OnPointerEnter(new PointerEventData(EventSystem.current));Check(sea.GetComponent<Outline>().enabled,"Sea gold hover");sea.OnPointerExit(new PointerEventData(EventSystem.current));Check(!sea.GetComponent<Outline>().enabled,"Hover clears");
                    var rt=(RectTransform)sea.transform;
                    Check(sea.IsRaycastLocationValid(RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center)),null),"Circular centre clickable");
                    Check(!sea.IsRaycastLocationValid(RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.max)),null),"Transparent corner passes through");
                    strait.Activate();Canvas.ForceUpdateCanvases();
                    Check(GameObject.Find("Province information").GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains(open?"Passage open":"Passage interdicted")),"Passage details");
                    Check(state==State()&&rng==JsonUtility.ToJson(UnityEngine.Random.state),"Inspection changes no campaign state or RNG");
                    if(numbers.x==26&&!open){sea.OnPointerEnter(new PointerEventData(EventSystem.current));Capture("map-"+size.x+".png");yield return null;}
                    sea.Activate();yield return null;
                    Check(GameObject.Find("War Beyond the Walls map")==null,"Map closes for Fleet management");
                    Check(FindObjectsOfType<TMP_Text>().Any(t=>t.text=="The Imperial Fleet"),"Fleet management opened");
                    Check(state==State()&&rng==JsonUtility.ToJson(UnityEngine.Random.state),"Fleet navigation changes no campaign state or RNG");
                    ControlSingleton.Instance.ReturnAction();yield return null;
                    Check(FindObjectsOfType<MaritimeMapMedallions>().Length==2,"Return opens map with two medallions");
                }
            }
            // Live balance changes refresh both picture and caption without reopening.
            VariableSingleton.SetInt("iShips",80);yield return null;
            Check(Badge(false).GetComponentInChildren<TMP_Text>().text=="Sea control\n"+NavalState.SeaControl,"Live sea caption refresh");
            Check(Badge(false).GetComponent<RawImage>().texture.name=="sea-imperial","Live sea artwork refresh");
            // Open map updates immediately after the authoritative passage flag changes.
            VariableSingleton.ChangeBool("bStraitsOpen",false);yield return null;
            VariableSingleton.ChangeBool("bStraitsOpen",true);yield return null;
            Check(Badge(true).GetComponent<RawImage>().texture.name=="strait-open","Live passage refresh");
            var saved=VariableSingleton.Instance.CaptureState();VariableSingleton.ChangeBool("bStraitsOpen",false);VariableSingleton.Instance.RestoreValidated(saved);yield return null;
            Check(Badge(true).GetComponent<RawImage>().texture.name=="strait-open","Save restoration refresh");
            WarTheatreUI.Close();
        }
        yield return PresentationCorrections();
    }
    IEnumerator PresentationCorrections()
    {
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1280,720)})
        {
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Continue Game");yield return new WaitForSecondsRealtime(.7f);
            CampaignHUD.Instance.DismissReport();
            foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
            var council=CouncilUI.For(FindObjectOfType<ManagerUI>(true));
            council.Technologies();yield return null; // Allow existing lazy defaults to initialise.
            foreach(int amount in new[]{0,3,12345})
            {
                VariableSingleton.SetInt("iSecrets",amount);yield return new WaitForSecondsRealtime(.35f); // Let the throttled global HUD settle too.
                string before=State(),rng=JsonUtility.ToJson(UnityEngine.Random.state);
                yield return null;Canvas.ForceUpdateCanvases();
                var counter=GameObject.Find("Available research secrets");
                Check(counter!=null&&counter.GetComponent<ImperialFrame>()==null&&counter.GetComponentInChildren<ImperialFrame>()!=null,"Secrets counter has gold frame");
                Check(counter.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==amount.ToString("N0")),"Live secrets balance "+amount);
                foreach(var text in counter.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Secrets text fits "+text.text);}
                var title=FindObjectsOfType<TMP_Text>().Single(t=>t.text=="Technologies and workshops");title.ForceMeshUpdate();
                Check(!title.isTextOverflowing,"Technology title fits");
                Check(title.rectTransform.anchorMax.x<counter.GetComponent<RectTransform>().anchorMin.x,"Title and counter separated");
                Check(before==State()&&rng==JsonUtility.ToJson(UnityEngine.Random.state),"Secrets display is read-only");
                if(amount==3)Capture("technology-secrets-"+size.x+".png");
            }
            CountryDiplomacy.Ensure();
            int pages=(CountryDiplomacy.Definitions.Length+7)/8;
            for(int page=0;page<pages;page++)
            {
                typeof(CouncilUI).GetField("diplomacyPage",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(council,page);
                council.DiplomaticRelations();yield return null;Canvas.ForceUpdateCanvases();
                var cards=FindObjectsOfType<Button>().Where(b=>b.name.StartsWith("Diplomatic court ")).ToArray();
                Check(cards.Length==Math.Min(8,CountryDiplomacy.Definitions.Length-page*8),"Court page complete");
                foreach(var card in cards)
                {
                    var badge=card.transform.Find("Court attitude badge");
                    Check(badge!=null&&badge.GetComponentInChildren<ImperialFrame>()!=null,"Attitude has gold frame "+card.name);
                    Check(badge.GetComponentsInChildren<Graphic>().All(g=>!g.raycastTarget),"Badge does not block court click");
                    foreach(var text in card.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Court text fits "+text.text);}
                }
                Capture("diplomacy-badges-"+page+"-"+size.x+".png");
                cards[0].onClick.Invoke();yield return null;
                Check(GameObject.Find("Relationship")!=null,"Court tile opens diplomatic details");
            }
            council.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Continue Game");yield return new WaitForSecondsRealtime(.6f);CampaignHUD.Instance.DismissReport();
            foreach(var input in FindObjectsOfType<BaseInputModule>())input.enabled=false;
            FindObjectOfType<ManagerUI>(true).gameObject.SetActive(false);
            var eventUI=FindObjectOfType<EventUI>(true);eventUI.SetStartNode("story19");eventUI.gameObject.SetActive(true);
            for(int n=0;n<40&&!FindObjectsOfType<Yarn.Unity.OptionButton>().Any(o=>o.IsInteractable());n++)yield return new WaitForSecondsRealtime(.15f);
            yield return new WaitForSecondsRealtime(.3f);
            var view=eventUI.GetComponentInChildren<EventArtworkView>();
            Check(view!=null&&view.Picture.texture==Resources.Load<Texture2D>("EventArt/Covers/story19"),"Golden Horn event resolves replacement art");
            Check(view.Picture.texture.width==1672&&view.Picture.texture.height==941,"Golden Horn replacement resolution preserved");
            Check(FindObjectsOfType<Yarn.Unity.OptionButton>().Any(o=>o.IsInteractable()),"Golden Horn choices available");
            string eventState=State();Capture("golden-horn-card-"+size.x+".png");
            EventCardArt.OpenPreview(view);yield return null;
            Check(GameObject.Find("Event artwork enlarged")!=null,"Event artwork opens enlarged");Capture("golden-horn-enlarged-"+size.x+".png");
            GameObject.Find("Event artwork enlarged").GetComponentInChildren<Button>().onClick.Invoke();yield return null;
            Check(GameObject.Find("Event artwork enlarged")==null&&view.gameObject.activeInHierarchy,"Close returns to event");
            Check(eventState==State(),"Artwork inspection leaves pending decision untouched");
        }
    }

}
#endif
