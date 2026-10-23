using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AssaultDuelUI:MonoBehaviour {
 public static AssaultDuelUI Current;public SwordDuel Duel=new SwordDuel();GameObject panel;RectTransform field,enemy,blade,weapon;TMP_Text status,feedback;Action<bool,int> done;bool started,settled;float recoil,fall;Vector2 previous;bool havePrevious;
 public static void Open(Transform parent,Action<bool,int> callback){var go=new GameObject("Sword duel");var ui=go.AddComponent<AssaultDuelUI>();Current=ui;ui.done=callback;ui.Build(parent);}
 void Build(Transform parent){
  panel=CampaignHUD.Panel(parent,"Hold the passage",new Vector2(.015f,.015f),new Vector2(.985f,.865f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
  CampaignHUD.Label(panel.transform,"Hold the passage · mouse-sword prototype",new Vector2(.035f,.9f),new Vector2(.96f,.98f),30);
  var arena=CampaignHUD.Panel(panel.transform,"Duel field",new Vector2(.035f,.2f),new Vector2(.64f,.88f));SceneBackdrop.Apply(arena,"assault-battlefield");field=arena.GetComponent<RectTransform>();arena.AddComponent<RectMask2D>();
  var e=new GameObject("Approaching opponent",typeof(RectTransform),typeof(RawImage));e.transform.SetParent(field,false);enemy=e.GetComponent<RectTransform>();enemy.anchorMin=new Vector2(.27f,.04f);enemy.anchorMax=new Vector2(.73f,.95f);enemy.offsetMin=enemy.offsetMax=Vector2.zero;var img=e.GetComponent<RawImage>();img.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/soldier-brown");img.uvRect=new Rect(152f/1024,(1536-1399f)/1536,725f/1024,1282f/1536);img.raycastTarget=false;
  blade=CampaignHUD.Panel(field,"Your blade",Vector2.zero,Vector2.zero).GetComponent<RectTransform>();blade.sizeDelta=new Vector2(100,5);blade.GetComponent<Image>().color=new Color(1,.9f,.6f);blade.GetComponent<Image>().raycastTarget=false;
  weapon=CampaignHUD.Panel(field,"Enemy weapon cue",Vector2.zero,Vector2.zero).GetComponent<RectTransform>();weapon.sizeDelta=new Vector2(22,22);weapon.GetComponent<Image>().color=new Color(.2f,.85f,.9f);weapon.GetComponent<Image>().raycastTarget=false;
  status=CampaignHUD.Explanation(panel.transform,"Three opponents. Hold the left mouse button and move to cut. Steel and shields deflect your blade. Intercept the cyan weapon marker during its swing to parry; cut the exposed face or legs. You may leave at any time. Maximum 20 health lost; the guard prevents death.",new Vector2(.67f,.4f),new Vector2(.96f,.87f),24);status.enableAutoSizing=true;status.fontSizeMin=18;
  feedback=CampaignHUD.Label(panel.transform,"Read the controls, then begin.",new Vector2(.67f,.23f),new Vector2(.96f,.4f),24);feedback.enableAutoSizing=true;feedback.fontSizeMin=18;
  CampaignHUD.MakeButton(panel.transform,"Start duel",new Vector2(.04f,.04f),new Vector2(.47f,.16f),()=>{started=true;});
  CampaignHUD.MakeButton(panel.transform,"Let the guard intervene",new Vector2(.52f,.04f),new Vector2(.96f,.16f),()=>{Duel.Exit();Finish();});
 }
 public string Stroke(Vector2 a,Vector2 b){if(fall>0)return "";string result=Duel.Stroke(a,b);if(result!=""){feedback.text=result;if(result.Contains("DEFLECTED"))recoil=.16f;if(result.Contains("DOWN"))fall=.4f;enemy.localRotation=Quaternion.Euler(0,0,result.Contains("DOWN")?12:result.Contains("CUT")?-4:0);}return result;}
 void Update(){
  if(!started||settled||!PhasedAssaultUI.CanAnimate||(CampaignHUD.Instance!=null&&CampaignHUD.Instance.HasModal))return;
  Duel.Tick(Time.unscaledDeltaTime);recoil=Mathf.Max(0,recoil-Time.unscaledDeltaTime);if(fall>0){fall=Mathf.Max(0,fall-Time.unscaledDeltaTime);enemy.localRotation=Quaternion.Euler(0,0,(.4f-fall)*170);if(fall==0){enemy.localRotation=Quaternion.identity;var image=enemy.GetComponent<RawImage>();string[] names={"soldier-brown","soldier-red","soldier-janissary"};image.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/"+names[Mathf.Clamp(3-Duel.Opponents,0,2)]);image.uvRect=new Rect(0,0,1,1);}}Vector2 local;
  if(RectTransformUtility.ScreenPointToLocalPointInRectangle(enemy,Input.mousePosition,null,out local)){
   Vector2 p=new Vector2((local.x-enemy.rect.xMin)/enemy.rect.width,(local.y-enemy.rect.yMin)/enemy.rect.height);
   if(Input.GetMouseButton(0)&&havePrevious)Stroke(previous,p);previous=p;havePrevious=true;
  }
  if(RectTransformUtility.ScreenPointToLocalPointInRectangle(field,Input.mousePosition,null,out local)){blade.anchorMin=blade.anchorMax=new Vector2(.5f,.5f);blade.anchoredPosition=local+new Vector2(-recoil*110,recoil*45);blade.localRotation=Quaternion.Euler(0,0,Input.GetMouseButton(0)?-35:35);}
  Vector2 wp=Duel.Weapon;Vector3 world=enemy.TransformPoint(new Vector2(enemy.rect.xMin+wp.x*enemy.rect.width,enemy.rect.yMin+wp.y*enemy.rect.height));weapon.position=world;
  enemy.localScale=Vector3.one*(.88f+Mathf.Sin(Duel.Cycle/2.3f*Mathf.PI)*.12f);status.text="OPPONENTS LEFT: "+Duel.Opponents+"\nWounds: "+Duel.Wounds+" / 20\nParries: "+Duel.Parries+"\n\n"+Duel.Cue+"\n\nHold LMB and move to cut. Cyan marks the weapon to intercept. Face and legs are exposed; shield and armour deflect.\n"+Mathf.Max(0,20-Duel.Clock).ToString("0.0")+"s remaining";
  if(Duel.Finished)Finish();
 }
 void Finish(){if(settled)return;settled=true;var callback=done;Destroy(panel);Current=null;Destroy(gameObject);callback?.Invoke(Duel.Won,Duel.Wounds);}
 void OnDestroy(){if(panel!=null)Destroy(panel);if(Current==this)Current=null;}
}
