using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AlbaniaWarTrialUI : MonoBehaviour {
 static GameObject selection;
 public static void TestMenu(Transform parent){
  if(selection!=null)Destroy(selection);selection=CampaignHUD.Panel(parent,"Assault test selection",new Vector2(.12f,.18f),new Vector2(.88f,.82f));ImperialTheme.Panel(selection);
  CampaignHUD.Label(selection.transform,"TEST ASSAULTS & REGIONAL WARS",new Vector2(.05f,.77f),new Vector2(.95f,.95f),34);
  CampaignHUD.Explanation(selection.transform,"Albania: a bounded 30-day war, with four scenarios and explicit revolt interactions. No campaign is loaded or modified.\n\nThe existing siege assault minigames remain accessible through Test assaults in the Imperial Council.",new Vector2(.06f,.36f),new Vector2(.94f,.74f),25);
  CampaignHUD.MakeButton(selection.transform,"Albania · regional war trial",new Vector2(.06f,.20f),new Vector2(.94f,.32f),()=>{Destroy(selection);selection=null;Show();});
  CampaignHUD.MakeButton(selection.transform,"Return",new Vector2(.62f,.04f),new Vector2(.94f,.15f),()=>{Destroy(selection);selection=null;});
 }
 static GameObject root;Transform body;AlbaniaWarTrial trial;bool assumptions;int scenario;
 public static bool Close(){if(root==null)return false;root.SetActive(false);Destroy(root);root=null;return true;}
 public static void Show(){Close();root=new GameObject("Regional war test",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Image),typeof(AlbaniaWarTrialUI));root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=260;var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);root.GetComponent<Image>().color=new Color(.018f,.035f,.049f);var ui=root.GetComponent<AlbaniaWarTrialUI>();ui.trial=new AlbaniaWarTrial(1453,1);ui.scenario=1;ui.Draw();}
 void Draw(){if(body!=null){body.gameObject.SetActive(false);Destroy(body.gameObject);}body=CampaignHUD.Panel(transform,"Albania trial",Vector2.zero,Vector2.one).transform;
  CampaignHUD.Label(body,"ALBANIA · WAR BEYOND THE WALLS",new Vector2(.035f,.925f),new Vector2(.75f,.985f),32);
  CampaignHUD.MakeButton(body,"Return",new Vector2(.83f,.93f),new Vector2(.97f,.985f),()=>Close());
  CampaignHUD.MakeButton(body,assumptions?"Battlefield":"Rules & assumptions",new Vector2(.035f,.85f),new Vector2(.30f,.91f),()=>{assumptions=!assumptions;Draw();});
  CampaignHUD.MakeButton(body,"Restart · seed 1453",new Vector2(.32f,.85f),new Vector2(.61f,.91f),()=>{trial=new AlbaniaWarTrial(1453,scenario);Draw();});
  CampaignHUD.Label(body,"Isolated trial · no campaign changes",new Vector2(.64f,.85f),new Vector2(.97f,.91f),22);
  if(assumptions){CampaignHUD.Explanation(body,AlbaniaWarTrial.Assumptions,new Vector2(.045f,.055f),new Vector2(.955f,.825f),23);return;}
  string[] names={"No revolt","Strong revolt","Fragile revolt","Supply crisis"};for(int i=0;i<4;i++){int choice=i;CampaignHUD.MakeButton(body,(scenario==i?"Selected · ":"")+names[i],new Vector2(.035f+i*.238f,.765f),new Vector2(.26f+i*.238f,.825f),()=>{scenario=choice;trial=new AlbaniaWarTrial(1453,scenario);Draw();});}
  CampaignHUD.Label(body,"Day "+trial.Day+" / 30 · "+trial.Outcome,new Vector2(.04f,.69f),new Vector2(.96f,.75f),29);
  var track=CampaignHUD.Panel(body,"Frontline control",new Vector2(.045f,.60f),new Vector2(.955f,.67f));track.GetComponent<Image>().color=new Color(.37f,.13f,.12f);var fill=CampaignHUD.Panel(track.transform,"Albanian control",Vector2.zero,new Vector2(trial.Front/100f,1));fill.GetComponent<Image>().color=new Color(.35f,.23f,.57f);
  var caption=CampaignHUD.Label(track.transform,"Albanian pressure "+trial.Front+" / 100 · Ottoman pressure "+(100-trial.Front)+" / 100",Vector2.zero,Vector2.one,25);caption.alignment=TextAlignmentOptions.Center;
  CampaignHUD.Explanation(body,"ALBANIA\n"+trial.Albanians.ToString("N0")+" troops · "+trial.AlbanianFood+" food-days\nAid packages: "+trial.Support,new Vector2(.045f,.43f),new Vector2(.33f,.58f),24);
  CampaignHUD.Explanation(body,"OTTOMANS\n"+trial.Ottomans.ToString("N0")+" troops · "+trial.OttomanFood+" food-days\nIntent: "+trial.EnemyIntent,new Vector2(.35f,.43f),new Vector2(.67f,.58f),24);
  CampaignHUD.Explanation(body,"PROVINCIAL REVOLT\nAuthority "+trial.Authority+"% · Resistance "+trial.Resistance+"%\n"+(trial.Liberated?"Liberated":trial.Suppressed?"Suppressed":trial.Rebellion?"Fighting · local exports stopped":"Not yet in open revolt"),new Vector2(.69f,.43f),new Vector2(.96f,.58f),24);
  CampaignHUD.Explanation(body,trial.Last,new Vector2(.045f,.205f),new Vector2(.955f,.415f),25);
  string[] orders={"Hold\nLimit losses","Advance\nPressure & rebel support","Raid supplies\nDestroy 1–3 food-days","Aid rebels\nSpend one aid package"};for(int i=0;i<4;i++){var order=(AlbaniaWarTrial.Order)i;var button=CampaignHUD.MakeButton(body,orders[i],new Vector2(.035f+i*.238f,.075f),new Vector2(.26f+i*.238f,.18f),()=>{trial.Step(order);Draw();});button.interactable=trial.Can(order);}
  CampaignHUD.Label(body,"One click = one day. No countdown. Scenario buttons restart the trial; Rules & assumptions lists all prototype modifiers.",new Vector2(.04f,.015f),new Vector2(.96f,.06f),18);
 }
}
