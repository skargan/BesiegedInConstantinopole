#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class JournalStatusCapture:MonoBehaviour
{
    string folder;int checks;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Begin(){if(!Environment.GetCommandLineArgs().Contains("-journal-status-capture"))return;Application.runInBackground=true;var go=new GameObject("Journal status checks");DontDestroyOnLoad(go);go.AddComponent<JournalStatusCapture>();}
    void Check(bool value,string message){if(!value)throw new Exception(message);checks++;}
    string Status(string id)=>JournalStrategicStatus.Read().Single(r=>r.Id==id).Status;
    IEnumerator Start(){folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures/JournalStatus"));Directory.CreateDirectory(folder);SaveGames.TestRoot=Path.Combine(folder,"Saves");Directory.CreateDirectory(SaveGames.TestRoot);VariableSingleton.TestSavePath=Path.Combine(SaveGames.TestRoot,"checkpoint.json");var stack=new Stack<IEnumerator>();stack.Push(Run());File.WriteAllText(Path.Combine(folder,"checks.txt"),"RUNNING");
        while(stack.Count>0){object next=null;bool more=false;Exception failure=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){failure=e;}if(failure!=null){File.WriteAllText(Path.Combine(folder,"checks.txt"),"FAILED "+checks+"\n"+failure);Application.Quit(1);yield break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested)stack.Push(nested);else yield return next;}
        File.WriteAllText(Path.Combine(folder,"checks.txt"),checks+" journal/status/map checks passed");Application.Quit(0);
    }
    IEnumerator Run(){yield return null;SaveMenu.StartCampaign(false);yield return new WaitForSecondsRealtime(2);CampaignHUD.Instance.DismissReport();DecisionJournal.Entries.Clear();
        string randomBefore=JsonUtility.ToJson(UnityEngine.Random.state);int goldBefore=DailyLifeEvents.I("iMoney");
        Check(JournalStrategicStatus.Read().Count==5,"five initial indicators");Check(Status("union")=="Undecided","initial Union undecided");Check(JsonUtility.ToJson(UnityEngine.Random.state)==randomBefore&&DailyLifeEvents.I("iMoney")==goldBefore,"status projection has no rewards or randomness");
        foreach(bool enabled in new[]{false,true}){VariableSingleton.ChangeBool("bStraitsOpen",enabled);VariableSingleton.ChangeBool("bGoldenHornChain",enabled);Check(Status("bosphorus")== (enabled?"Open":"Closed"),"actual passage");Check(Status("chain")== (enabled?"Present":"Absent"),"actual chain");}
        for(int i=0;i<4;i++){PreparationCampaign.SetGalataState(i);Check(Status("galata")== (i==3?"Pro-Ottoman":i==0?"Neutral":"Pro-Emperor"),"formal Galata "+i);}
        foreach(var pair in new[]{new[]{0,0,1},new[]{15,10,1},new[]{16,10,0},new[]{10,16,2},new[]{0,1,2},new[]{1,0,0}}){VariableSingleton.SetInt("iShips",pair[0]);VariableSingleton.SetInt("iTurkShips",pair[1]);Check(Status("sea")==new[]{"Imperial Fleet","Contested","Ottoman Fleet"}[pair[2]],"shared naval threshold "+pair[0]+":"+pair[1]);}
        VariableSingleton.SetInt("iReligion",1);Check(Status("union")=="Implemented","full Union");VariableSingleton.SetInt("iReligion",2);Check(Status("union")=="Symbolic","symbolic Union");VariableSingleton.SetInt("iReligion",0);PreparationCampaign.Complete("story1");PreparationCampaign.Complete("story1");Check(Status("union")=="Rejected"&&DecisionJournal.Entries.Count(e=>e.id=="union-of-florence")==1,"rejection recorded once");
        VariableSingleton.SetInt("iTurkishBattalionCreated",1);DecisionJournal.Record("turkish_battalion_created","Turkish Battalion Created","Orhan commands the new battalion.",true);Check(Status("battalion")=="Active","battalion active");VariableSingleton.SetInt("iTurkishBattalionCreated",0);Check(Status("battalion")=="Inactive","revealed battalion remains visible");
        Check(SaveGames.Checkpoint(),"save journal");DecisionJournal.Entries.Clear();Check(SaveGames.Load(VariableSingleton.SavePath,out var error),"load journal "+error);Check(Status("union")=="Rejected"&&Status("battalion")=="Inactive","saved status projection");
        DecisionJournal.Record("test-day","A day at the walls","The guards report a quiet morning.");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);Check(Screen.width==size.x&&Screen.height==size.y,"exact resolution "+size);
            foreach(DecisionJournal.Tab tab in Enum.GetValues(typeof(DecisionJournal.Tab))){DecisionJournal.Show(tab);yield return null;yield return null;var root=FindObjectsOfType<Canvas>().Single(c=>c.name=="Decision Journal").gameObject;Check(root!=null,"journal opens");Check(root.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Journal tab "))==3,"three separate tabs");
                if(tab==DecisionJournal.Tab.StrategicStatus){VariableSingleton.ChangeBool("bStraitsOpen",false);yield return null;yield return null;Check(root.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Strategic status bosphorus").text=="Closed","visible status refresh");VariableSingleton.ChangeBool("bStraitsOpen",true);yield return null;yield return null;Check(root.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Strategic status bosphorus").text=="Open","live reopen");}
                foreach(var text in root.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"journal text fits "+text.text);}
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,tab+"-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);
                if(tab==DecisionJournal.Tab.DailyNotes){root.GetComponentsInChildren<Button>().Single(b=>b.name=="Journal tab Decisions").onClick.Invoke();yield return null;var switched=FindObjectsOfType<Canvas>().Single(c=>c.name=="Decision Journal");Check(switched.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Orhan commands")),"Decisions tab button displays decisions");}
                var active=FindObjectsOfType<Canvas>().Single(c=>c.name=="Decision Journal");active.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<TMP_Text>().text=="Return").onClick.Invoke();yield return null;Check(!FindObjectsOfType<Canvas>().Any(c=>c.name=="Decision Journal"),"Return closes journal");
            }
            WarTheatreUI.Show();yield return null;yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"regional-map-"+size.x+".png"));yield return new WaitForSecondsRealtime(.2f);
            foreach(string id in new[]{"venice","crete","negroponte","corfu","kythera","genoa","corsica","morea","anatolia","armeniakon","candar_realm","aq_qoyunlu"})Check(WarTheatre.Find(id)!=null,"map integration "+id);
            Check(WarTheatre.HighlightKey(WarTheatre.Find("crete"))==WarTheatre.HighlightKey(WarTheatre.Find("venice")),"Venetian collective highlight");Check(WarTheatre.HighlightKey(WarTheatre.Find("corsica"))==WarTheatre.HighlightKey(WarTheatre.Find("genoa")),"Genoese collective highlight");Check(WarTheatre.Data.regions.Count(r=>r.kind=="province")==11,"eleven provinces retained");WarTheatreUI.Close();yield return null;
        }
    }
}
#endif
