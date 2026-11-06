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

public sealed class VicinityMapCapture:MonoBehaviour
{
    string folder;int checks;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Boot(){if(!Environment.GetCommandLineArgs().Contains("-vicinity-map-capture"))return;Application.runInBackground=true;var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/VicinityMap"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var go=new GameObject("Vicinity map checks");DontDestroyOnLoad(go);go.AddComponent<VicinityMapCapture>().folder=folder;}
    void Check(bool value,string message){if(!value)throw new Exception(message);checks++;}
    GameObject Map=>GameObject.Find("War Beyond the Walls map");GameObject Local=>GameObject.Find("Vicinity of Constantinople");
    string State()=>JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());
    Button LocalButton(string caption)=>Local.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text==caption);
    IEnumerator Start(){File.WriteAllText(Path.Combine(folder,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());while(stack.Count>0){object next=null;bool more=false;Exception error=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){error=e;}if(error!=null){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+checks+"\n"+error);Application.Quit(1);yield break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}File.WriteAllText(Path.Combine(folder,"checks.txt"),checks+" vicinity/name checks passed");Application.Quit(0);}
    IEnumerator Shot(string name){ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.18f);}
    IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(1);CampaignHUD.Instance.DismissReport();
        var fontFixture=new GameObject("Native name fixture",typeof(RectTransform));checks+=NativeNameChecks.Run(fontFixture.transform);Destroy(fontFixture);yield return null;
        Check(Vector4.Distance(WarTheatre.Tint(WarTheatre.Find("serbia")),WarTheatre.Tint(WarTheatre.Find("bosnia")))>.3f,"distinct Serbia/Bosnia palette");
        var possession=WarTheatre.Data.regions.First(r=>r.group=="genoa"&&r.kind=="island");
        var prior=DiplomaticNames.Mode;
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);DiplomaticNames.Mode=DiplomaticNames.Style.Short;WarTheatreUI.Show();yield return null;
            Check(!Map.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Symbols"),"retired Symbols control");
            var hover=Map.transform.Find("Map hover plaque").GetComponentInChildren<TMP_Text>();var selector=Map.GetComponentsInChildren<Button>().First(b=>b.name=="Realm naming selector");
            Check(hover.rectTransform.rect.width>((RectTransform)selector.transform).rect.width*2,"name plaque has more room than selector");
            var territory=Map.GetComponentsInChildren<TerritoryGraphic>().First(t=>t.name=="wallachia");territory.OnPointerEnter(new PointerEventData(EventSystem.current));
            foreach(var mode in new[]{DiplomaticNames.Style.Short,DiplomaticNames.Style.Full,DiplomaticNames.Style.Original}){
                DiplomaticNames.Mode=mode;yield return null;yield return null;Check(selector.GetComponentInChildren<TMP_Text>().text=="Names: "+mode,"selector caption follows shared mode");Check(hover.text==DiplomaticNames.Name("wallachia"),"fixed name plaque follows mode "+mode);Canvas.ForceUpdateCanvases();hover.ForceMeshUpdate();Check(!hover.isTextOverflowing,"hover name fits "+mode);if(mode==DiplomaticNames.Style.Original)Check(hover.font==DiplomaticNames.OriginalTypeface,"native plaque font family");
            }
            territory.OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});yield return null;yield return Shot("native-name-"+size.x);
            DiplomaticNames.Mode=DiplomaticNames.Style.Short;var island=Map.GetComponentsInChildren<TerritoryGraphic>().First(t=>t.name==possession.id);island.OnPointerEnter(new PointerEventData(EventSystem.current));Check(hover.text.Contains("Genoa")&&hover.text.Contains(possession.name),"possession names show parent and island");
            Map.GetComponentsInChildren<TerritoryGraphic>().First(t=>t.name=="serbia").OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});yield return null;Check(Map.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Ottoman vassal")),"Serbian tributary label");yield return Shot("serbia-"+size.x);
            VariableSingleton.SetInt(MapLandmarks.OutcomeKey,1);string before=State();Map.transform.Find("Theatre viewport/Geographic base/Landmark · Constantinople");
            Map.GetComponentsInChildren<Button>().First(b=>b.name=="Landmark · Constantinople").onClick.Invoke();yield return null;Check(Local!=null,"capital medallion opens close-up");
            Check(Local.GetComponent<Image>()!=null&&Local.GetComponent<Image>().color.a==1,"opaque local map backdrop");Check(!Map.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("\u00c2")||t.text.Contains("\u00e2\u20ac")),"map has no encoding artefacts");Check(Local.GetComponentInChildren<RawImage>().texture.name=="ConstantinopleVicinity","local geographic artwork loaded");
            var capital=Local.transform.Find("Local atlas viewport/Bosphorus geography/Local site city");Check(capital!=null&&capital.GetComponentInChildren<CapitalSeal>()!=null,"unique capital seal");
            ImperialTheme.Button(capital.GetComponent<Button>());Check(capital.GetComponent<Graphic>().color.a==0,"late theme pass preserves circular capital seal");Check(!capital.GetComponentsInChildren<ImperialFrame>().Any(),"capital has no active square border");Check(((RectTransform)capital).rect.width>Local.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Local site ")&&b.name!="Local site city").Max(b=>((RectTransform)b.transform).rect.width),"capital is largest site icon");
            foreach(var site in ConstantinopleVicinityUI.Sites){var button=Local.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="Local site "+site.Id);Check(button!=null,"site represented "+site.Id);button.onClick.Invoke();yield return null;foreach(var text in Local.transform.Find("Local site information").GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"site text fits "+site.Id);}}
            Local.GetComponentsInChildren<Button>().First(b=>b.name=="Local site city").onClick.Invoke();yield return null;yield return Shot("vicinity-"+size.x);
            Check(State()==before,"atlas and marker inspections preserve campaign/RNG");ControlSingleton.Instance.GoBack();yield return null;Check(Local==null&&Map!=null,"Back returns to regional map");
            ConstantinopleVicinityUI.Show();yield return null;Local.GetComponentsInChildren<Button>().First(b=>b.name=="Local site rumeli").onClick.Invoke();LocalButton("Inspect fortress").onClick.Invoke();yield return null;Check(MapLandmarks.ViewOpen&&Local!=null,"existing fortress view opens above local map");ControlSingleton.Instance.GoBack();yield return null;Check(!MapLandmarks.ViewOpen&&Local!=null,"fortress Back preserves close-up");LocalButton("Regional map").onClick.Invoke();yield return null;Check(Local==null&&Map!=null,"regional map button preserves overview");WarTheatreUI.Close();
        }
        ConstantinopleVicinityUI.Show();yield return null;LocalButton("The Palace").onClick.Invoke();yield return null;Check(Local==null&&Map==null&&GameObject.Find("Location painting")!=null,"capital destination opens existing Palace");DiplomaticNames.Mode=prior;
    }
}
#endif
