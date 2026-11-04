#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public sealed class Content391Capture:MonoBehaviour {
 string output;int checks;
 static int I(string key)=>DailyLifeEvents.I(key);
 static void Set(string key,int n)=>VariableSingleton.SetInt(key,n);
 void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Begin(){
  if(!Environment.GetCommandLineArgs().Contains("-content391-capture"))return;
  Application.runInBackground=true;string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/Content391"));Directory.CreateDirectory(path);
  SaveGames.TestRoot=Path.Combine(path,"Saves");VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.besieged.json");
  var obj=new GameObject("Content391 checks");DontDestroyOnLoad(obj);obj.AddComponent<Content391Capture>().output=path;
 }
 IEnumerator Start(){
  yield return new WaitForSecondsRealtime(2);
  if(Environment.GetCommandLineArgs().Contains("-warm-lantern-preview")){
   var buttons=FindObjectOfType<MainMenuUI>().GetComponentsInChildren<Button>().Where(b=>b.IsInteractable()).ToArray();
   for(int i=0;i<buttons.Length;i++){
    var light=buttons[i].GetComponent<ImperialButtonLightDriver>();if(light!=null)light.Light.SetSelected(i%2==0);
   }
   yield return new WaitForSecondsRealtime(.3f);yield return Capture("warm-lanterns");Application.Quit();yield break;
  }
  yield return Capture("main-menu");SaveMenu.StartCampaign(false);
  yield return new WaitForSecondsRealtime(3);
  try { Run(); }catch(Exception e){File.WriteAllText(Path.Combine(output,"checks.txt"),"FAILED "+e);Debug.LogException(e);Application.Quit(1);yield break;}
  var baseline=VariableSingleton.Instance.CaptureState();
  foreach(string kind in new[]{"army","people","nobles","clergy","assassination","conquest"}){
   VariableSingleton.Instance.RestoreValidated(baseline);
   if(kind=="conquest")VariableSingleton.Instance.EnduringWall.Restore(baseline.wall,true);
   else if(kind=="assassination"){VariableSingleton.ChangeFloat("fHealth",-1);Set("iLifePoisonEnding",1);}
   else VariableSingleton.ChangeFloat(kind=="army"?"fTroopRep":kind=="people"?"fPeopleRep":kind=="nobles"?"fNobleRep":"fPriestRep",-1);
   var hud=FindObjectOfType<CampaignHUD>();
   typeof(CampaignHUD).GetField("ended",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(hud,false);
   hud.CheckEnding();yield return Capture("ending-"+kind);
  }
  File.WriteAllText(Path.Combine(output,"checks.txt"),"PASS "+checks+" focused assertions; six ending screens captured.");Application.Quit(0);
 }
 void Run(){
  Check(VariableSingleton.IsInitialized,"campaign initialised");
  var original=VariableSingleton.Instance.CaptureState();
  for(int policy=1;policy<=5;policy++){
   VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(original)));CharacterPreparations.Initialize();Set("iFocus",policy==4?1:policy==5?4:0);Set("iTurkSpies",7);
   foreach(string k in new[]{"fTroopRep","fPeopleRep","fNobleRep","fPriestRep"})VariableSingleton.ChangeFloat(k,.5f-VariableSingleton.GetFloatVariable(k));
   int inf=I("iInfantry"),arch=I("iArchers"),cav=I("iCavalry"),money=I("iMoney");var before=VariableSingleton.Instance.CaptureState();
   CharacterPreparations.TurkishHouseholds(policy);
   Check(I("iInfantry")==inf+(policy==1?0:policy==2?150:policy==4?600:300),"Infantry total "+policy);
   Check(I("iArchers")==arch+(policy==4?200:0)&&I("iCavalry")==cav+(policy==4?50:0),"Other troop totals "+policy);
   Check(I("iTurkSpies")==7+(policy==3?1:policy==4?2:0),"Spy consequence "+policy);
   Check(VariableSingleton.GetBoolVariable("bCharNomad3")== (policy!=1),"Orhan availability "+policy);
   Check(I("iLoyalty3")== (policy==1?0:policy==2?50:90),"Orhan loyalty "+policy);
   if(policy==1){Check(I("iMoney")-money>=500&&I("iMoney")-money<=3000,"Gold range");VariableSingleton.ChangeBool("bCharNomad3",true);Check(!VariableSingleton.GetBoolVariable("bCharNomad3"),"Permanent loss");}
   else Check(I("iMoney")==money,"No extra fee");
   foreach(string k in new[]{"fTroopRep","fPeopleRep","fNobleRep","fPriestRep"}){
    float expected=.5f+(policy==1&&k=="fPeopleRep"?.05f:policy==4||policy==3&&k=="fNobleRep"?-.05f:0);
    Check(Mathf.Abs(VariableSingleton.GetFloatVariable(k)-expected)<.001f,"Support "+policy+" "+k);
   }
   string visible=EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState());
   Check(!visible.Contains("Ottoman spies")&&!CharacterPreparations.TurksOutcome().Contains("enemy spy"),"Hidden infiltration "+policy);
   Check(I("iTurkishBattalionCreated")== (policy==4?1:0),"Battalion flag "+policy);
   Check(I("iTurkScreeningIntercepted")== (policy==5?1:0),"Screening record "+policy);
   Check(DecisionJournal.Entries.Count(e=>e.id=="turkish_battalion_created")== (policy==4?1:0),"Journal "+policy);
   var saved=VariableSingleton.Instance.CaptureState();string encoded=JsonUtility.ToJson(saved);CharacterPreparations.TurkishHouseholds(policy);Check(encoded==JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()),"No reroll/duplicate "+policy);
   string path=Path.Combine(output,"branch-"+policy+".json");File.WriteAllText(path,encoded);VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(File.ReadAllText(path)));
   Check(I("iTurkPolicy")==policy&&I("iLoyalty3")== (policy==1?0:policy==2?50:90),"Save roundtrip "+policy);
   Check(I("iTurkSpies")==7+(policy==3?1:policy==4?2:0)&&I("iTurkishBattalionCreated")== (policy==4?1:0),"Saved spy/flag "+policy);
   Check(DecisionJournal.Entries.Count(e=>e.id=="our_turks_ruling")==1,"Saved journal "+policy);
  }
  VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(original)));CharacterPreparations.Initialize();Set("iFocus",0);CharacterPreparations.TurkishHouseholds(4);CharacterPreparations.TurkishHouseholds(5);Check(I("iTurkPolicy")==0,"Policy gates enforced in command");
  Check(I("iTurkishBattalionCreated")==0&&I("iTurkScreeningIntercepted")==0,"No stale flags on restore");
  int day=TrainingPool.Day(VariableSingleton.GetDate()),foot=I("iInfantry"),bows=I("iArchers");
  DailyLifeEvents.TavernPledge();Check(I("iLifeTavernDue")==day+1,"Tavern next day");DailyLifeEvents.CompleteTavernPledge();Check(I("iInfantry")==foot,"Not early");
  Set("iLifeTavernDue",day);var tavern=VariableSingleton.Instance.CaptureState();Set("iLifeTavernDue",0);VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(tavern)));DailyLifeEvents.CompleteTavernPledge();DailyLifeEvents.CompleteTavernPledge();Check(I("iInfantry")==foot+5&&I("iArchers")==bows+5,"Tavern once only after save");
  Set("iMoney",4);DailyLifeEvents.TavernPay(25);Check(I("iMoney")==0,"Tavern loss capped");
  foreach(string k in new[]{"army","people","nobles","clergy","assassination","conquest"})Check(Resources.Load<Texture2D>("UI/Endings/"+k)!=null,"Ending art "+k);
  Check(Resources.Load<Texture2D>("UI/Identity/Characters/0")!=null&&Resources.Load<Texture2D>("UI/Identity/CourtFigures/0")!=null,"Giustiniani resources");
  var yarn=Resources.FindObjectsOfTypeAll<Yarn.Unity.YarnProject>().First(p=>p.NodeNames.Contains("story13"));Check(yarn.NodeNames.Contains("lifeTavern")&&yarn.NodeNames.Contains("lifeTavernPledge"),"Compiled Tavern and Turks nodes");
  VariableSingleton.Instance.RestoreValidated(JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(original)));
 }
    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(.12f);Canvas.ForceUpdateCanvases();
        var obj=new GameObject("Art capture camera",typeof(Camera));var camera=obj.GetComponent<Camera>();
        camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;Texture2D shot=null;
        try{
            camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;shot=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            shot.ReadPixels(new Rect(0,0,target.width,target.height),0,0);shot.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        }finally{
            RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
            camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);if(shot!=null)Destroy(shot);Destroy(obj);Canvas.ForceUpdateCanvases();
        }
        yield return null;
    }
}
#endif
