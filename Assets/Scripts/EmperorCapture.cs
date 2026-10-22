#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class EmperorCapture:MonoBehaviour
{
    static bool begun;int passed;string output;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Begin(){if(begun||Array.IndexOf(Environment.GetCommandLineArgs(),"-emperor-capture")<0)return;begun=true;var go=new GameObject("Emperor verification");DontDestroyOnLoad(go);go.AddComponent<EmperorCapture>();}
    void Check(bool value,string label){if(!value)throw new Exception(label);passed++;}
    Button Button(string title)=>FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text==title);
    IEnumerator Start()
    {
        AudioListener.volume=0;Application.runInBackground=true;output=Path.GetFullPath(Path.Combine(Application.dataPath,"../EmperorCaptures"));Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"checks.txt"),"Running; not yet passed.");VariableSingleton.TestSavePath=Path.Combine(output,"test-save.json");
        yield return new WaitForSeconds(.4f);FindObjectOfType<MainMenuUI>().NewGame();yield return new WaitForSeconds(.7f);
        Check(EmperorProfile.NeedsOpening,"New game skipped its opening");Check(!Button("Face the day").interactable,"Unallocated Emperor accepted");yield return Capture("01-emperor-opening");
        string[] names={"Charisma","Intellect","Martial skill"};int[] adds={2,2,3};
        for(int i=0;i<3;i++){var card=GameObject.Find(names[i]);var plus=card.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text=="+");for(int j=0;j<adds[i];j++)plus.onClick.Invoke();}
        Check(Button("Face the day").interactable,"Seven-point allocation not accepted");Button("Face the day").onClick.Invoke();yield return null;
        Check(!EmperorProfile.NeedsOpening&&EmperorProfile.Value(0)==5&&EmperorProfile.Value(1)==5&&EmperorProfile.Value(2)==6,"Allocation not applied");Check(VariableSingleton.GetFloatVariable("fHealth")==1,"New Emperor not healthy");Check(!EmperorProfile.Allocate(new[]{10,3,3}),"Opening rerolled later");
        foreach(var name in new[]{"charisma","intellect","martial"})Check(Resources.Load<Texture2D>("UI/Identity/Attributes/"+name)!=null,"Generated attribute art missing: "+name);
        Check(EmperorCombat.Skill==60,"Combat does not use unified Martial skill");int old=EmperorProfile.Value(2);Check(EmperorCombat.Train(out var message)&&EmperorProfile.Value(2)==old&&EmperorProfile.XP(2)==25,"Training awarded an instant level");Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.95f)<.001f,"Training health cost changed");
        for(int i=0;i<9;i++)EmperorCombat.Train(out message);Check(EmperorProfile.Value(2)==7&&EmperorProfile.XP(2)==0,"XP threshold or carry failed");Check(EmperorCombat.Skill==70,"Combat compatibility did not follow Martial level");
        VariableSingleton.SetInt("iLifeEmperorStat0",3);CourtConversationData.Mark(2,"personal");CourtConversationData.Mark(2,"arrival");CourtConversationData.Mark(2,"bonds");Check(!CourtConversationData.NewBiography(2),"Locked descendant falsely marked new");EmperorProfile.GainXP(0,175);Check(EmperorProfile.Value(0)==4&&CourtConversationData.NewBiography(2),"New descendant did not reactivate its parent");CourtConversationData.Mark(2,"city");Check(!CourtConversationData.NewBiography(2),"Read descendant stayed new");
        VariableSingleton.SetInt("iCannons",111);Check(!SiegeCharacterContext.CannonsSatisfied,"Orban target opens too early");VariableSingleton.SetInt("iCannons",112);Check(SiegeCharacterContext.CannonsSatisfied,"Orban target never opens");Check(!SiegeCharacterContext.CannonTheory.Contains("112"),"Hidden target disclosed early");VariableSingleton.SetInt("iCannons",3);
        var contextState=VariableSingleton.Instance.CaptureState();var siegeContext=VariableSingleton.Instance.CaptureState();siegeContext.turn=VariableSingleton.EffectiveSiegeTurn;Restore(siegeContext);VariableSingleton.SetInt("iShips",0);CourtConversationData.Mark(1,"context");Check(CourtConversationData.Read(1,"context"),"Read context not remembered");VariableSingleton.SetInt("iSpies",999);Check(CourtConversationData.Read(1,"context"),"Unrelated change reactivated a read topic");VariableSingleton.SetInt("iShips",2);Check(!CourtConversationData.Read(1,"context"),"Changed naval state did not reactivate relevant topic");Restore(contextState);
        var baseState=VariableSingleton.Instance.CaptureState();Check(CourtConversationData.All.characters.Length==15,"Quest roster incomplete");Check(CourtConversationData.All.characters.Sum(q=>q.stages.Length)==45,"Quest stages incomplete");
        foreach(var q in CourtConversationData.All.characters)
        {
            Restore(baseState);Activate(q.person);VariableSingleton.SetInt("iLifeEmperorStat"+q.stat,7);
            Check(CourtConversationData.Decide(q,0,1),"First choice rejected: "+q.name);int xp=EmperorProfile.XP(q.stat);Check(!CourtConversationData.Decide(q,0,2)&&EmperorProfile.XP(q.stat)==xp,"Decision XP farm: "+q.name);
            Check(!CourtConversationData.CanRead(q,1),"Follow-up ignored calendar: "+q.name);VariableSingleton.SetInt("iLifeQuestDay"+q.person+"_0",CourtConversationData.Day-1);
            VariableSingleton.SetInt("iLifeEmperorStat"+q.stat,3);Check(!CourtConversationData.CanRead(q,1),"Secret ignored stat gate: "+q.name);VariableSingleton.SetInt("iLifeEmperorStat"+q.stat,5);Check(CourtConversationData.Decide(q,1,2),"Disclosure unavailable: "+q.name);
            VariableSingleton.SetInt("iLifeQuestDay"+q.person+"_1",CourtConversationData.Day-2);Check(!CourtConversationData.CanRead(q,2),"Final requirement ignored: "+q.name);
            VariableSingleton.SetInt("iLifeQuestChoice"+q.link+"_1",1);Check(CourtConversationData.CanRead(q,2),"Shared information route missing: "+q.name);Check(CourtConversationData.Decide(q,2,1),"Settlement rejected: "+q.name);Check(EmperorProfile.Epilogue().Contains(q.endingA),"Epilogue forgot settlement: "+q.name);
        }
        Restore(baseState);Activate(3);
        Check(GardenConversations.ReportAssessment(3,"The assault is repulsed. The siege continues.").Contains("driven back"),"Orhan not reacting to assault");Check(!GardenConversations.ReportAssessment(3,"The assault is repulsed. The siege continues.").Contains("cannot eat"),"Old unrelated supplies advice returned");
        foreach(string report in new[]{"The assault is repulsed. The siege continues.","This defensive line is abandoned.","Ottoman bombardment damages the current wall by 4%.","Ottoman labourers fill the moat, reducing its protection.","Ottoman siege operations are delayed this turn.","The Ottomans have built a battering ram.",""})foreach(var q in CourtConversationData.All.characters)Check(!string.IsNullOrWhiteSpace(GardenConversations.ReportAssessment(q.person,report)),"Missing contextual assessment");
        int[][] limits={new[]{1,10},new[]{20,40},new[]{10,30},new[]{5,75},new[]{5,20}};
        for(int k=0;k<5;k++){bool valid=true;for(int i=0;i<1000;i++){int roll=EmperorProfile.HealthRoll(k);valid&=roll>=limits[k][0]&&roll<=limits[k][1];}Check(valid,"Health range invalid: "+k);}
        VariableSingleton.ChangePercentFloat("fHealth",1);EmperorProfile.HealthEvent(4);Check(VariableSingleton.GetFloatVariable("fHealth")==1,"Recovery exceeded full health");VariableSingleton.ChangePercentFloat("fHealth",-1);VariableSingleton.ChangePercentFloat("fHealth",.1f);EmperorProfile.HealthEvent(1);Check(VariableSingleton.GetFloatVariable("fHealth")==0,"Health damage not potentially fatal");
        Restore(baseState);VariableSingleton.SetInt("iLifeEmperorXP1",99);string json=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());VariableSingleton.SetInt("iLifeEmperorXP1",0);Restore(JsonUtility.FromJson<VariableSingleton.CampaignSave>(json));Check(EmperorProfile.XP(1)==99,"XP not save-persistent");
        var council=CouncilUI.For(FindObjectOfType<ManagerUI>());Activate(0);VariableSingleton.SetInt("iLifeLocationTurn",VariableSingleton.Turn+1);VariableSingleton.SetInt("iLifeLocationVisitor0",0);council.Palace();yield return Capture("02-palace-mirror-sprite");
        Check(FindObjectsOfType<LocationHotspot>().Any(h=>h.name=="Hotspot · The Emperor’s mirror"),"Mirror hotspot missing");Check(GameObject.Find("Standing figure")!=null,"Standing sprite missing");
        council.EmperorSheet();yield return Capture("03-emperor-sheet");Button("Return to the Palace").onClick.Invoke();yield return null;
        Activate(3);CourtConversationUI.Show(3,()=>council.Palace(),council.EmperorLesson);yield return Capture("04-orhan-conversation");
        var view=FindObjectOfType<CourtConversationUI>();typeof(CourtConversationUI).GetField("passage",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(view,GardenConversations.ReportAssessment(3,"The assault is repulsed. The siege continues."));typeof(CourtConversationUI).GetMethod("Render",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(view,null);yield return Capture("05-orhan-assault-response");
        Button("Leave the conversation").onClick.Invoke();yield return null;Activate(2);CourtConversationUI.Show(2,()=>council.Palace(),council.EmperorLesson);yield return null;var questButton=FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text.Contains("The House with Two Doors")==true);questButton.onClick.Invoke();yield return Capture("06-quest-gates");
        Button("The House with Two Doors").onClick.Invoke();yield return Capture("06b-quest-choices");Button(CourtConversationData.For(2).stages[0].choiceA).onClick.Invoke();Check(CourtConversationData.Choice(2,0)==1,"Visible quest choice did not record decision");
        Button("Leave the conversation").onClick.Invoke();yield return null;Screen.SetResolution(1100,760,false);yield return new WaitForSeconds(.5f);council.EmperorAffairs();yield return Capture("07-smaller-window");
        Restore(baseState);Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.3f);Activate(5);CourtConversationUI.Show(5,()=>council.Palace(),council.EmperorLesson);yield return null;
        Button("How many cannons does a city really need?").onClick.Invoke();var enough=FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text.StartsWith("Finally, do you think we have enough cannons now?")==true);Check(!enough.interactable&&!enough.GetComponentInChildren<TMP_Text>().text.Contains("112"),"Hidden cannon requirement leaked or ignored");yield return Capture("08-orban-locked");Button("Leave the conversation").onClick.Invoke();yield return null;
        VariableSingleton.SetInt("iCannons",112);CourtConversationUI.Show(5,()=>council.Palace(),council.EmperorLesson);yield return null;enough=FindObjectsOfType<Button>().First(b=>b.GetComponentInChildren<TMP_Text>()?.text.StartsWith("Finally, do you think we have enough cannons now?")==true);Check(enough.interactable,"Orban follow-up did not unlock");enough.onClick.Invoke();Check(CourtConversationData.Read(5,"cannonanswer"),"Orban answer was not recorded");yield return Capture("09-orban-answer");Button("Leave the conversation").onClick.Invoke();yield return null;
        council.Hide();var manager=FindObjectOfType<ManagerUI>(true);manager.gameObject.SetActive(false);var ev=FindObjectOfType<EventUI>(true);var runner=ev.GetComponentInChildren<Yarn.Unity.DialogueRunner>(true);
        for(int k=0;k<5;k++){
            Restore(baseState);VariableSingleton.ChangePercentFloat("fHealth",1);ev.gameObject.SetActive(false);if(runner.IsDialogueRunning)runner.Stop();CampaignHUD.Instance.DismissReport();ev.SetStartNode("emperorHealth"+k);ev.gameObject.SetActive(true);yield return null;Check(runner.NodeExists("emperorHealth"+k),"Health Yarn node missing: "+k);
            for(int n=0;n<40&&!FindObjectsOfType<Yarn.Unity.OptionButton>().Any(o=>o.IsInteractable());n++)yield return new WaitForSeconds(.15f);
            var options=FindObjectsOfType<Yarn.Unity.OptionButton>().Where(o=>o.IsInteractable()).ToArray();Check(options.Length==1,"Health event choice missing: "+k);if(k==2)yield return Capture("10-health-event");options[0].InvokeOptionSelected();for(int n=0;n<40&&runner.IsDialogueRunning;n++)yield return new WaitForSeconds(.15f);
            Check(DailyLifeEvents.I("iLifeEmperorLastHealth")>=limits[k][0]&&DailyLifeEvents.I("iLifeEmperorLastHealth")<=limits[k][1],"Health Yarn command not dispatched: "+k);
        }
        File.WriteAllText(Path.Combine(output,"checks.txt"),passed+" exported-player checks passed.");Application.Quit();
    }
    void Restore(VariableSingleton.CampaignSave state)=>typeof(VariableSingleton).GetMethod("RestoreState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(VariableSingleton.Instance,new object[]{state});
    void Activate(int person){if(person==200)VariableSingleton.SetInt("iKjoldState",1);else if(person>=100){VariableSingleton.SetInt("iAdventurerChoice",person-99);VariableSingleton.SetInt("iAdventurerArrived",1);}else VariableSingleton.ChangeBool(CourtRoster.Keys[person],true);}
    IEnumerator Capture(string name)
    {
        yield return new WaitForSeconds(.5f);yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();
        // Render UI even when Windows keeps this test player hidden.
        var cameraGo=new GameObject("Capture camera",typeof(Camera));var camera=cameraGo.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;var target=new RenderTexture(Screen.width,Screen.height,24);camera.targetTexture=target;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}
        Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),texture.EncodeToPNG());RenderTexture.active=old;
        for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}Destroy(texture);target.Release();Destroy(target);Destroy(cameraGo);yield return null;
    }
}
#endif
