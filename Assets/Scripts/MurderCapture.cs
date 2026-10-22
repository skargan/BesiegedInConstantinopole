#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
public class MurderCapture:MonoBehaviour
{
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Begin(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-murder-capture")<0)return;var go=new GameObject("Murder verification");DontDestroyOnLoad(go);go.AddComponent<MurderCapture>();}
 string output;int passed;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;}
 void Set(string key,int n)=>VariableSingleton.SetInt("iLifeMurder"+key,n);
 int Read(string key)=>DailyLifeEvents.I("iLifeMurder"+key);
 void Reset(int culprit){foreach(string key in new[]{"Stage","Evidence","Outcome","Exposed","Accused","Due"})Set(key,0);foreach(int id in new[]{2,3,4})VariableSingleton.ChangeBool(CourtRoster.Keys[id],true);MurderMystery.Start();Set("Culprit",culprit+1);}
 IEnumerator Start(){
  AudioListener.volume=0;Application.runInBackground=true;
  output=Path.GetFullPath(Path.Combine(Application.dataPath,"../MurderCaptures"));Directory.CreateDirectory(output);VariableSingleton.TestSavePath=Path.Combine(output,"test-save.json");
  yield return null;FindObjectOfType<MainMenuUI>().NewGame();yield return new WaitForSeconds(.5f);
  for(int culprit=0;culprit<4;culprit++){
   Reset(culprit);MurderMystery.Start();Check(Read("Culprit")==culprit+1,"Case rerolled");
   for(int stage=1;stage<=6;stage++){
    Check(MurderMystery.Text(stage).Contains("<b>"),"Missing stage prose");MurderMystery.Choose(stage,0);Check(Read("Stage")==stage+1,"Wrong next stage");
   }
   Check(MurderMystery.EvidenceCount==4,"Evidence incomplete");
   int day=Read("Due");Check(MurderMystery.DueNode==null,"Followup due too soon");Set("Due",TrainingPool.Day(VariableSingleton.GetDate()));Check(MurderMystery.DueNode=="murder7","Due followup missing");Set("Due",day);
   VariableSingleton.Instance.Save();Set("Culprit",99);Set("Evidence",0);VariableSingleton.Instance.Load();Check(Read("Culprit")==culprit+1&&MurderMystery.EvidenceCount==4,"Save lost mystery state");
   MurderMystery.Choose(7,culprit);Check(Read("Outcome")==1,"Supported verdict failed");Check(MurderMystery.DueNode==null&&!MurderMystery.OptionalNodes().Any(),"Closed case repeated");
   int outcome=Read("Outcome");MurderMystery.Choose(7,5);Check(Read("Outcome")==outcome,"Duplicate choice changed outcome");
  }
  Reset(0);Set("Stage",7);Set("Evidence",3);MurderMystery.Choose(7,1);Check(Read("Outcome")==3&&!CourtRoster.InService(2),"False accusation missing consequences");
  Reset(1);Set("Stage",7);Set("Evidence",3);MurderMystery.Choose(7,4);Check(Read("Outcome")==2&&CourtRoster.InService(2),"Sealed case removed adviser");
  Reset(1);Set("Stage",7);MurderMystery.Choose(7,4);Check(Read("Stage")==7,"Unproven seal allowed");MurderMystery.Choose(7,5);Check(Read("Outcome")==4,"Open verdict failed");
  Reset(0);MurderMystery.Choose(1,2);Check(Read("Outcome")==5,"Suppression failed");
  Reset(0);Set("Stage",5);MurderMystery.Choose(5,2);Check(Read("Outcome")==3&&Read("Accused")==5,"Porter conviction failed");
  Reset(0);Set("Stage",3);MurderMystery.Choose(3,2);Set("Stage",6);MurderMystery.Choose(6,0);Check((Read("Evidence")&8)==0,"Lost interception restored");
  Set("Stage",0);foreach(int id in new[]{2,3,4})VariableSingleton.ChangeBool(CourtRoster.Keys[id],false);MurderMystery.Start();Check(Read("Culprit")==1,"Absent official chosen");
  Reset(1);Set("Stage",7);Set("Evidence",15);VariableSingleton.ChangeBool(CourtRoster.Keys[2],false);VariableSingleton.SetInt("iCourtState2",3);MurderMystery.Choose(7,1);Check(DailyLifeEvents.I("iCourtState2")==3,"Dead official resurrected");
  var manager=FindObjectOfType<ManagerUI>();manager.gameObject.SetActive(false);var ev=FindObjectOfType<EventUI>(true);
  string[] labels={"I will examine","Have the surgeon","Protect the widow","Authenticate the household","Reconstruct the porter","Guard the widow","Accuse Markos"};
  for(int stage=1;stage<=7;stage++){
   ev.gameObject.SetActive(false);var runner=ev.GetComponentInChildren<DialogueRunner>(true);if(runner.IsDialogueRunning)runner.Stop();CampaignHUD.Instance.DismissReport();
   Reset(0);Set("Stage",stage);if(stage>1)Set("Evidence",15);
   ev.SetStartNode("murder"+stage);ev.gameObject.SetActive(true);
   for(int n=0;n<40&&!FindObjectsOfType<OptionButton>().Any(o=>o.IsInteractable());n++)yield return new WaitForSeconds(.15f);
   var options=FindObjectsOfType<OptionButton>().Where(o=>o.IsInteractable()).ToArray();
   Check(options.Length>=3,"Yarn choices missing at stage "+stage);
   if(stage==1||stage==4||stage==7)yield return Capture("murder-stage-"+stage);
   var selected=options.FirstOrDefault(o=>o.GetComponentsInChildren<TMPro.TMP_Text>(true).Any(t=>t.text.Contains(labels[stage-1])));
   Check(selected!=null,"Expected choice label missing at stage "+stage);selected.InvokeOptionSelected();
   for(int n=0;n<40&&runner.IsDialogueRunning;n++)yield return new WaitForSeconds(.15f);
   Check(Read("Stage")==(stage==7?8:stage+1),"Yarn choice dispatch failed at stage "+stage);
   string report=string.Join("\n",FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.text));
   Check(!report.Contains("Life Murder")&&!report.Contains("Murder Culprit"),"Hidden mystery state leaked");
   if(stage==7)yield return Capture("murder-verdict");
  }
  CampaignHUD.Instance.DismissReport();VariableSingleton.ChangePercentFloat("fHealth",-1f);CampaignHUD.Instance.CheckEnding();
  yield return Capture("murder-epilogue");
  string ending=string.Join("\n",FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.text));
  Check(ending.Contains("The names beneath the wax")&&ending.Contains("Petros"),"Campaign ending forgot the case");
  Check(ending.Contains("Markos"),"Campaign ending forgot supported culprit");
  File.WriteAllText(Path.Combine(output,"checks.txt"),passed+" exported-player checks passed.");Application.Quit();
 }
    private IEnumerator Capture(string name)
    {
        yield return new WaitForSeconds(1);
        yield return null;
        File.WriteAllLines(Path.Combine(output,name+"-text.txt"),FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.name+" | "+t.rectTransform.rect+" | "+t.maxVisibleCharacters+" | "+string.Join(",",t.GetComponentsInParent<CanvasGroup>().Select(g=>g.name+":"+g.alpha))+" | "+t.text));
        // Render explicitly: a hidden Windows player can otherwise return a black back buffer.
        var cameraObject = new GameObject("Capture camera",typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic=true; camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.06f,.07f,.08f); camera.nearClipPlane=.1f; camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();
        var modes=canvases.Select(c=>c.renderMode).ToArray();
        var cameras=canvases.Select(c=>c.worldCamera).ToArray();
        var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=new RenderTexture(1600,900,24);
        camera.targetTexture=target;
        foreach(var canvas in canvases) {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
        Canvas.ForceUpdateCanvases();
        camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=target;
        var shot=new Texture2D(1600,900,TextureFormat.RGB24,false);
        shot.ReadPixels(new Rect(0,0,1600,900),0,0);shot.Apply();
        File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        RenderTexture.active=previous;
        for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
        camera.targetTexture=null;target.Release();Destroy(target);Destroy(shot);Destroy(cameraObject);
        yield return new WaitForSeconds(.3f);
    }
}
#endif

