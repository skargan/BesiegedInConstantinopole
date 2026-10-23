using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad] public static class CharacterDialogueSmokeTest {
 static CharacterDialogueSmokeTest(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("DialogueSmoke",false))new GameObject("Dialogue checks").AddComponent<CharacterDialogueSmokeProbe>();};}
 public static void Run(){AssetDatabase.Refresh();SessionState.SetBool("DialogueSmoke",true);EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");EditorApplication.EnterPlaymode();}
}
public class CharacterDialogueSmokeProbe:MonoBehaviour {
 int checks;List<string> failures=new List<string>();
 void Check(bool ok,string text){checks++;if(!ok)failures.Add(text);}
 void Set(string k,int v)=>VariableSingleton.SetInt(k,v);
 IEnumerator Start(){DontDestroyOnLoad(gameObject);VariableSingleton.TestSavePath=System.IO.Path.GetFullPath("Logs/dialogue-test-save.json");yield return null;FindObjectOfType<MainMenuUI>().NewGame();yield return null;
 try {
 foreach(int id in CharacterQuestions.People){
 var e=CharacterQuestions.For(id);Check(e!=null&&e.Replies.Length==3&&e.Responses.Length==3,"exchange "+id);
 Check(e.Question.Length>100&&e.Replies.All(x=>x.Length>10)&&e.Responses.All(x=>x.Length>30),"authoredlines "+id);
 Check(GardenConversations.Personal(id).Length>100&&GardenConversations.Advice(id).Length>60&&GardenConversations.Siege(id).Length>60,"topics "+id);
 Check(!GardenConversations.Name(id).Contains("Unknown")&&GardenConversations.Role(id).Length>4,"identity "+id);
 }
 for(int n=0;n<6;n++){
 int id=100+n;Set("iAdventurerChoice",n+1);Set("iAdventurerArrived",0);Check(!GardenConversations.Available(id),"notyetarrived "+id);
 Set("iAdventurerArrived",1);Check(GardenConversations.Available(id),"arrived "+id);
 Check(!GardenConversations.Available(100+(n+1)%6),"otherinviteabsent "+id);
 Set("iHardOfficerReturn"+id,TrainingPool.Day(VariableSingleton.GetDate())+1);Check(!GardenConversations.Available(id),"absent "+id);Set("iHardOfficerReturn"+id,0);
 bool seen=false;for(int k=0;k<40;k++){Set("iLifeLocationTurn",0);if(PalaceLocations.Visitor(0)==id||PalaceLocations.Visitor(1)==id)seen=true;}Check(seen,"locationpool "+id);
 Set("iLifeQuestionReply"+id,2);
 }
 VariableSingleton.Instance.Save();Set("iLifeQuestionReply100",0);VariableSingleton.Instance.Load();Check(DailyLifeEvents.I("iLifeQuestionReply100")==2,"rememberreplysave");
 Check(CharacterQuestions.For(999)==null&&!GardenConversations.Available(999),"invalididentity");
 }catch(Exception e){failures.Add(e.ToString());}
 Debug.Log("DIALOGUE_SMOKE checks="+checks+" failures="+failures.Count+"\n"+string.Join("\n",failures));SessionState.SetBool("DialogueSmoke",false);EditorApplication.Exit(failures.Count==0?0:1);
 }
}
