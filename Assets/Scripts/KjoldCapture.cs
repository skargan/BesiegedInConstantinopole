#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class KjoldCapture:MonoBehaviour
{
    string output;CouncilUI council;VariableSingleton.CampaignSave baseline;
    readonly List<string> checks=new List<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-kjold-capture"))return;
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Kjold"));Directory.CreateDirectory(path);
        SaveGames.TestRoot=Path.Combine(path,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
        var go=new GameObject("Kjold verification");DontDestroyOnLoad(go);go.AddComponent<KjoldCapture>().output=path;
    }
    void Check(bool value,string name){if(!value)throw new Exception(name);checks.Add("PASS "+name);}
    string Rng()=>JsonUtility.ToJson(UnityEngine.Random.state);
    VariableSingleton.CampaignSave Copy()=>JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
    GameObject Root=>GameObject.Find("Council screen");
    void Restore(){council.Hide();CampaignHUD.Instance.DismissReport();VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(baseline)));}
    void Talk(int topic){typeof(CouncilUI).GetMethod("GardenTalk",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(council,new object[]{GardenConversations.Kjold,topic});}
    Button FindButton(string text)=>Root.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==text));
    bool HasButton(string text)=>Root.GetComponentsInChildren<Button>().Any(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==text));
    void PrepareCore(int id){VariableSingleton.SetInt("iCourtSentence"+id,0);VariableSingleton.ChangeBool(CourtRoster.Keys[id],true);VariableSingleton.SetInt("iLoyalty"+id,50);}
    IEnumerator Start(){File.WriteAllText(Path.Combine(output,"checks.txt"),"RUNNING");var stack=new Stack<IEnumerator>();stack.Push(Run());while(stack.Count>0){bool more=false;object next=null;Exception error=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){error=e;}if(error!=null){File.WriteAllLines(Path.Combine(output,"details.txt"),checks);File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED after "+checks.Count+"\n"+error);Application.Quit(1);yield break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}File.WriteAllLines(Path.Combine(output,"details.txt"),checks);File.WriteAllText(Path.Combine(output,"checks.txt"),"KJOLD_CHECKS "+checks.Count+" passed");Application.Quit();}
    IEnumerator Run(){
        Application.runInBackground=true;AudioListener.volume=0;yield return null;SaveMenu.StartCampaign(false);
        float deadline=Time.realtimeSinceStartup+25;while((!VariableSingleton.IsInitialized||FindObjectOfType<ManagerUI>()==null)&&Time.realtimeSinceStartup<deadline)yield return null;
        Check(VariableSingleton.IsInitialized,"Campaign initialized");yield return new WaitForSecondsRealtime(.3f);CampaignHUD.Instance.DismissReport();council=CouncilUI.For(FindObjectOfType<ManagerUI>());baseline=Copy();
        Check(KjoldConversations.Text.biography.Contains("Cretan")&&KjoldConversations.Text.biography.Contains("Constantinople"),"Approved Constantinopolitan biography and Cretan mother");
        Check(KjoldConversations.Text.guardHistory.Split(new[]{"\n\n"},StringSplitOptions.None).Length==4,"Four Guard history paragraphs");
        Check(KjoldConversations.Text.guardHistory.Contains("two Tatars")&&KjoldConversations.Text.guardHistory.Contains("Finn")&&KjoldConversations.Text.guardHistory.Contains("thirty"),"Approved fictional mixed Guard retained");
        foreach(int agents in new[]{0,7,8,10}){
            VariableSingleton.SetInt("iSpies",agents);VariableSingleton.SetInt("iTurkSpies",13);string rng=Rng();string text=KjoldConversations.SecurityAssessment();
            Check(text.Contains("not currently aware of any assassination plots"),"Fixed assassination reassurance at "+agents+" agents");
            Check(agents>=8?text.Contains("13 enemy spies"):text.Contains("unknown number")&&!text.Contains("13 enemy spies"),"Existing eight-agent information gate at "+agents);
            Check(Rng()==rng,"Security inspection does not consume RNG");
        }
        Restore();PrepareCore(0);string ignored;Check(!KjoldCourtOrders.CarryOut(0,KjoldCourtOrders.Sentence.Execute,out ignored),"Orders locked before hearing assessment");
        Check(!KjoldCourtOrders.Targets().Contains(200),"Kjold is not ordered to sentence himself");
        foreach(var sentence in new[]{KjoldCourtOrders.Sentence.Exile,KjoldCourtOrders.Sentence.Imprison,KjoldCourtOrders.Sentence.Execute})for(int id=0;id<8;id++){
            Restore();PrepareCore(id);KjoldConversations.HearSecurity();VariableSingleton.SetInt("iCommander",id);
            int gold=CouncilActions.Int("iMoney"),turn=VariableSingleton.Turn;float support=VariableSingleton.GetFloatVariable("fPeopleRep");string rng=Rng();
            Check(KjoldCourtOrders.CarryOut(id,sentence,out var report),"Core order accepted "+id+" "+sentence);
            Check(!CourtRoster.InService(id)&&!GardenConversations.Available(id),"Core service and conversation stop "+id+" "+sentence);
            Check(CourtRoster.Bonus(id,5,15)==0&&!CourtRoster.CanCommand(id)&&DailyLifeEvents.I("iCommander")==-1,"Core commander and specialist benefit removed "+id+" "+sentence);
            Check(DailyLifeEvents.I("iCourtState"+id)==(sentence==KjoldCourtOrders.Sentence.Exile?6:sentence==KjoldCourtOrders.Sentence.Imprison?4:3),"Existing fate state reused "+id+" "+sentence);
            Check(!KjoldCourtOrders.Targets().Contains(id)&&!KjoldCourtOrders.CarryOut(id,sentence,out ignored),"Repeat sentence rejected "+id+" "+sentence);
            Check(CouncilActions.Int("iMoney")==gold&&VariableSingleton.Turn==turn&&VariableSingleton.GetFloatVariable("fPeopleRep")==support&&Rng()==rng,"No invented costs, turn, support or RNG "+id+" "+sentence);
        }
        foreach(var sentence in new[]{KjoldCourtOrders.Sentence.Exile,KjoldCourtOrders.Sentence.Imprison,KjoldCourtOrders.Sentence.Execute})for(int candidate=0;candidate<6;candidate++){
            Restore();VariableSingleton.SetInt("iAdventurerChoice",candidate+1);VariableSingleton.SetInt("iAdventurerArrived",1);int person=100+candidate;VariableSingleton.SetInt("iCommander",person);KjoldConversations.HearSecurity();
            Check(KjoldCourtOrders.Targets().Contains(person)&&KjoldCourtOrders.CarryOut(person,sentence,out ignored),"Visitor sentence "+candidate+" "+sentence);
            Check(!CharacterPreparations.InService&&!CharacterPreparations.IsCommander&&!GardenConversations.Available(person)&&DailyLifeEvents.I("iCommander")==-1,"Visitor leaves service, conversations and command "+candidate+" "+sentence);
            Check(CharacterPreparations.Advance()==""&&!CharacterPreparations.InService,"Visitor cannot re-arrive after sentence "+candidate+" "+sentence);
        }
        foreach(var sentence in new[]{KjoldCourtOrders.Sentence.Exile,KjoldCourtOrders.Sentence.Imprison,KjoldCourtOrders.Sentence.Execute}){
            Restore();VariableSingleton.SetInt("iShahStage",4);VariableSingleton.SetInt("iShahStatus",7);KjoldConversations.HearSecurity();
            Check(KjoldCourtOrders.Targets().Contains(201)&&KjoldCourtOrders.CarryOut(201,sentence,out ignored),"Storyteller sentence "+sentence);
            Check(!KjoldCourtOrders.IsServing(201)&&!KjoldCourtOrders.Targets().Contains(201),"Storyteller stops serving "+sentence);
        }
        Restore();PrepareCore(2);KjoldConversations.HearSecurity();Check(KjoldCourtOrders.CarryOut(2,KjoldCourtOrders.Sentence.Imprison,out ignored),"Prepare save roundtrip");
        Check(SaveGames.Named("Kjold custody",out var message),"Named save writes court order");var saved=SaveGames.Entries().First(e=>e.Compatible);var archive=SaveGames.Read(saved.path);Restore();VariableSingleton.Instance.RestoreValidated(archive.campaign);
        Check(KjoldConversations.SecurityHeard&&KjoldCourtOrders.For(2)==KjoldCourtOrders.Sentence.Imprison&&!CourtRoster.InService(2),"Real save/read restores unlock and sentence");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Restore();Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.35f);PrepareCore(0);
            Talk(0);yield return null;Check(!HasButton("I have orders"),"Orders button initially absent "+size);Layout("Biography");yield return Capture("kjold-biography-"+size.x);
            FindButton("How do you assess my security?").onClick.Invoke();yield return null;Check(HasButton("I have orders")&&KjoldConversations.SecurityHeard,"Assessment unlocks actual orders button "+size);Layout("Security");
            var scroll=GameObject.Find("Kjold reading viewport").GetComponent<ScrollRect>();Check(scroll.content.rect.height>scroll.viewport.rect.height&&scroll.GetComponent<RectMask2D>()!=null,"Long security passage scrolls inside mask "+size);scroll.verticalNormalizedPosition=0;yield return null;yield return Capture("kjold-security-end-"+size.x);
            FindButton("Tell me about the Varangian Guard").onClick.Invoke();yield return null;Layout("Guard history");yield return Capture("kjold-history-"+size.x);
            FindButton("I have orders").onClick.Invoke();yield return null;Check(HasButton("Exile a courtier")&&HasButton("Imprison a courtier")&&HasButton("Execute a courtier"),"Three order actions "+size);
            FindButton("Imprison a courtier").onClick.Invoke();yield return null;var first=Root.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Review sentence");first.onClick.Invoke();yield return null;Layout("Sentence review");yield return Capture("kjold-review-"+size.x);
            int[] beforeTargets=KjoldCourtOrders.Targets().ToArray();FindButton("Cancel").onClick.Invoke();yield return null;Check(beforeTargets.SequenceEqual(KjoldCourtOrders.Targets()),"Cancel leaves court unchanged "+size);
            Root.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Review sentence").onClick.Invoke();yield return null;FindButton("Confirm sentence").onClick.Invoke();yield return null;
            Check(KjoldCourtOrders.For(beforeTargets[0])==KjoldCourtOrders.Sentence.Imprison,"Actual confirm applies imprisonment "+size);
            CampaignHUD.Instance.DismissReport();council.Court();yield return null;
            Check(Root.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="IMPRISONED"),"Sentenced character retained with stamp "+size);
            Check(!Root.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("Expected arrival:")&&t.text.Contains(KjoldCourtOrders.Name(beforeTargets[0]))),"No false incoming notice for sentenced character "+size);yield return Capture("kjold-court-stamp-"+size.x);
        }
    }
    void Layout(string page){Canvas.ForceUpdateCanvases();foreach(var button in Root.GetComponentsInChildren<Button>()){
        foreach(var text in button.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,page+" button text fits "+text.text);}
        var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);Check(corners.All(c=>c.x>=-1&&c.y>=-1&&c.x<=Screen.width+1&&c.y<=Screen.height+1),page+" button inside viewport");
    }}
    IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.12f);Canvas.ForceUpdateCanvases();var go=new GameObject("Capture camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.1f;camera.farClipPlane=100;var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cams=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var prior=RenderTexture.active;Texture2D shot=null;try{camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());}finally{RenderTexture.active=prior;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cams[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(go);}}
}
#endif
