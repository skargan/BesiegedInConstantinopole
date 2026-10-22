using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LadderDefenceUI:MonoBehaviour {
 public static LadderDefenceUI Current;public LadderDefence Game=new LadderDefence();GameObject panel;RectTransform field,bowString;RectTransform[] ladders=new RectTransform[3];RectTransform[,] crews=new RectTransform[3,4];TMP_Text status;Action<int,int> result;bool started,settled,pulling;int pushed=-1;Vector2 previous;
 public static void Open(Transform parent,Action<int,int> done){var go=new GameObject("Ladder defence");var ui=go.AddComponent<LadderDefenceUI>();Current=ui;ui.result=done;ui.Build(parent);}
 void Build(Transform parent){
  panel=CampaignHUD.Panel(parent,"Drive off the ladders",new Vector2(.015f,.015f),new Vector2(.985f,.865f));ImperialTheme.Panel(panel);panel.transform.SetAsLastSibling();
  CampaignHUD.Label(panel.transform,"Drive off the ladders",new Vector2(.035f,.91f),new Vector2(.96f,.98f),31);
  var art=CampaignHUD.Panel(panel.transform,"Exterior wall view",new Vector2(.035f,.19f),new Vector2(.75f,.88f));field=art.GetComponent<RectTransform>();art.AddComponent<RectMask2D>();SceneBackdrop.Apply(art,Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/ladder-wall")!=null?"ladder-wall":"walls-war");
  for(int i=0;i<3;i++){
   var ladder=new GameObject("Ladder "+i,typeof(RectTransform));ladder.transform.SetParent(field,false);var r=ladder.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.25f+i*.25f,.43f);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(22,170);ladders[i]=r;
   var a=CampaignHUD.Panel(r,"Left rail",new Vector2(.1f,0),new Vector2(.25f,1));a.GetComponent<Image>().color=new Color(.42f,.28f,.13f);var b=CampaignHUD.Panel(r,"Right rail",new Vector2(.75f,0),new Vector2(.9f,1));b.GetComponent<Image>().color=new Color(.42f,.28f,.13f);
   for(int n=0;n<10;n++){var rung=CampaignHUD.Panel(r,"Rung",new Vector2(.2f,.05f+n*.09f),new Vector2(.8f,.075f+n*.09f));rung.GetComponent<Image>().color=new Color(.55f,.38f,.19f);rung.GetComponent<Image>().raycastTarget=false;}
   for(int n=0;n<4;n++){var go=new GameObject("Ladder carrier",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(field,false);var cr=go.GetComponent<RectTransform>();cr.anchorMin=cr.anchorMax=new Vector2(.25f+i*.25f,.1f);cr.pivot=new Vector2(.5f,0);var image=go.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/ladder-carrier");if(image.texture==null)image.texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/soldier-brown");image.color=new Color(.8f,.75f,.66f);image.raycastTarget=false;crews[i,n]=cr;}
  }
  status=CampaignHUD.Explanation(panel.transform,"Three crews approach. Pull the crossbow string down until it locks, or hold W. Click a crew or ladder to shoot. Once upright, hold the ladder and drag down repeatedly to push it away. Your pressure decays when released. Read first; the challenge lasts 12 seconds.",new Vector2(.78f,.43f),new Vector2(.96f,.87f),23);status.enableAutoSizing=true;status.fontSizeMin=17;
  var bow=CampaignHUD.Panel(panel.transform,"Crossbow",new Vector2(.79f,.23f),new Vector2(.95f,.40f));bow.GetComponent<Image>().color=ImperialTheme.Navy;var stock=CampaignHUD.Panel(bow.transform,"Stock",new Vector2(.47f,.1f),new Vector2(.53f,.9f));stock.GetComponent<Image>().color=new Color(.5f,.32f,.15f);var limbs=CampaignHUD.Panel(bow.transform,"Bow limbs",new Vector2(.10f,.66f),new Vector2(.9f,.72f));limbs.GetComponent<Image>().color=new Color(.7f,.56f,.32f);bowString=CampaignHUD.Panel(bow.transform,"Pull string",new Vector2(.12f,.61f),new Vector2(.88f,.64f)).GetComponent<RectTransform>();bowString.GetComponent<Image>().color=new Color(.94f,.86f,.64f);
  CampaignHUD.MakeButton(panel.transform,"Start ladder defence · 12 seconds",new Vector2(.04f,.04f),new Vector2(.60f,.14f),()=>started=true);CampaignHUD.MakeButton(panel.transform,"Let the wall crews take over",new Vector2(.63f,.04f),new Vector2(.96f,.14f),Finish);
 }
 void Update(){
  if(!started||settled||!PhasedAssaultUI.CanAnimate)return;Game.Tick(Time.unscaledDeltaTime);if(Input.GetKey(KeyCode.W))Game.DrawString(Time.unscaledDeltaTime*1.2f);
  Vector2 point;RectTransformUtility.ScreenPointToLocalPointInRectangle(field,Input.mousePosition,null,out point);Vector2 norm=new Vector2((point.x-field.rect.xMin)/field.rect.width,(point.y-field.rect.yMin)/field.rect.height);
  if(Input.GetMouseButtonDown(0)){Vector2 bowPoint;var bow=bowString.parent as RectTransform;RectTransformUtility.ScreenPointToLocalPointInRectangle(bow,Input.mousePosition,null,out bowPoint);pulling=bow.rect.Contains(bowPoint);pushed=-1;if(!pulling){int lane=Mathf.Clamp(Mathf.RoundToInt((norm.x-.25f)/.25f),0,2);if(norm.x>=.10f&&norm.x<=.90f){if(Game.Loaded)Game.Shoot(lane);else if(Game.Ladders[lane].State==LadderDefence.Phase.Upright)pushed=lane;}}}
  if(Input.GetMouseButton(0)){float down=Mathf.Max(0,previous.y-norm.y);if(pulling)Game.DrawString(down*4);else if(pushed>=0)Game.Push(pushed,down*4);}
  if(Input.GetMouseButtonUp(0)){pulling=false;pushed=-1;}previous=norm;
  bowString.anchoredPosition=new Vector2(0,-Game.StringDraw*35);
  for(int i=0;i<3;i++){var l=Game.Ladders[i];var r=ladders[i];r.gameObject.SetActive(l.State!=LadderDefence.Phase.Waiting&&l.State!=LadderDefence.Phase.Down&&l.State!=LadderDefence.Phase.Breached);float angle=l.State==LadderDefence.Phase.Approach?-80:l.State==LadderDefence.Phase.Raise?-80*(1-l.Age/1.2f):l.State==LadderDefence.Phase.Falling?l.Age/.7f*100:0;r.localRotation=Quaternion.Euler(0,0,angle);
   float y=l.State==LadderDefence.Phase.Approach?Mathf.Lerp(.08f,.43f,l.Age/2):.43f;r.anchorMin=r.anchorMax=new Vector2(.25f+i*.25f,y);
   for(int n=0;n<4;n++){var c=crews[i,n];bool visible=l.State!=LadderDefence.Phase.Waiting&&l.State!=LadderDefence.Phase.Down&&l.State!=LadderDefence.Phase.Breached;c.gameObject.SetActive(visible);float size=l.State==LadderDefence.Phase.Approach?Mathf.Lerp(55,28,l.Age/2):28;c.sizeDelta=new Vector2(size*.65f,size);c.anchorMin=c.anchorMax=new Vector2(.25f+i*.25f+(n-1.5f)*.025f,y-.045f);c.anchoredPosition=new Vector2(0,Mathf.Sin(Game.Clock*8+n)*1.5f);c.localRotation=Quaternion.Euler(0,0,l.State==LadderDefence.Phase.Falling?l.Age*220:0);}
  }
  status.text="Ladders stopped: "+Game.Felled+" / 3\nClimbed: "+Game.Breaches+"\n\n"+(Game.Loaded?"CROSSBOW LOCKED · click a target":"Pull the string down, or hold W")+"\n\nUpright ladder: hold and drag down to push.\n"+Mathf.Max(0,12-Game.Clock).ToString("0.0")+"s";
  if(Game.Finished)Finish();
 }
 void Finish(){if(settled)return;settled=true;Destroy(panel);Current=null;var callback=result;Destroy(gameObject);callback?.Invoke(Game.Felled,Game.Breaches);}
 void OnDestroy(){if(panel!=null)Destroy(panel);if(Current==this)Current=null;}
}
