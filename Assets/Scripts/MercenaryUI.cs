using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class MercenaryUI:MonoBehaviour
{
 static GameObject root;static int selected;static bool ledger;static string notice="";
 public static void Show(int id=-1){if(id>=0){selected=id;ledger=MercenaryContracts.State(id)!=MercenaryContracts.Status.Unhired;}Draw();}
 public static bool Close(){if(root==null)return false;root.SetActive(false);Destroy(root);root=null;return true;}
 static TMP_Text Text(Transform p,string s,float x,float y,float w,float h,int size=22){var t=CampaignHUD.Label(p,s,new Vector2(x,y),new Vector2(x+w,y+h),size);t.enableAutoSizing=true;t.fontSizeMax=size;t.fontSizeMin=16;return t;}
 static void Button(Transform p,string s,float x,float y,float w,float h,Action action,bool enabled=true){var b=CampaignHUD.MakeButton(p,s,new Vector2(x,y),new Vector2(x+w,y+h),()=>action());b.interactable=enabled;}
 static void Act(Func<string> action){notice=action();SaveGames.Checkpoint(SaveGames.Phase,null,"Mercenary contracts",notice);Draw();}
 static void Draw()
 {
 Close();root=new GameObject("Mercenary brokers",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(MercenaryUI));root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=265;var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
 SceneBackdrop.Apply(root,"mercenary-brokers");
 var top=CampaignHUD.Panel(root.transform,"Broker heading",new Vector2(.015f,.885f),new Vector2(.985f,.985f));ImperialTheme.Panel(top,false);Text(top.transform,"Malta · The Mercenary Brokers",.025f,.46f,.72f,.50f,30);Text(top.transform,"Gold: "+DailyLifeEvents.I("iMoney").ToString("N0")+" · "+VariableSingleton.GetDate().ToString("MMMM yyyy"),.025f,.04f,.72f,.38f,20);Button(top.transform,"Return",.83f,.17f,.15f,.65f,()=>Close());
 var intro=CampaignHUD.Panel(root.transform,"Broker introduction",new Vector2(.025f,.09f),new Vector2(.32f,.33f));ImperialTheme.Panel(intro,false);Text(intro.transform,"Mercenaries serve throughout these seas. Malta is as good a place as any to meet their brokers; each company sails from its own muster port.\n\nA captain’s name is worth weighing beside his price.",.05f,.07f,.90f,.87f,22);
 var pane=CampaignHUD.Panel(root.transform,"Contract desk",new Vector2(.345f,.025f),new Vector2(.985f,.875f));ImperialTheme.Panel(pane,false);
 Button(pane.transform,"Offers this month",.025f,.925f,.46f,.06f,()=>{ledger=false;notice="";Draw();});Button(pane.transform,"Contracts · "+MercenaryContracts.Pending+" awaiting reply",.515f,.925f,.46f,.06f,()=>{ledger=true;notice="";Draw();});
 int[] list=ledger?Enumerable.Range(0,MercenaryRoster.All.Length).Where(i=>MercenaryContracts.State(i)!=MercenaryContracts.Status.Unhired).ToArray():MercenaryContracts.Offers;
 if(list.Length==0){Text(pane.transform,"No contracts have been signed.",.04f,.70f,.92f,.13f,25);return;}
 if(!list.Contains(selected))selected=list[0];
 // Compact scrolling index: offers are five; the ledger may contain every company.
 var index=CampaignHUD.Panel(pane.transform,"Company index",new Vector2(.025f,.15f),new Vector2(.40f,.91f));index.AddComponent<RectMask2D>();
 var content=new GameObject("Company list",typeof(RectTransform));content.transform.SetParent(index.transform,false);var cr=(RectTransform)content.transform;cr.anchorMin=new Vector2(0,1);cr.anchorMax=Vector2.one;cr.pivot=new Vector2(.5f,1);cr.sizeDelta=new Vector2(0,Mathf.Max(560,list.Length*105));
 for(int n=0;n<list.Length;n++){int id=list[n];var c=MercenaryRoster.All[id];var card=CampaignHUD.Panel(content.transform,"Company offer "+id,new Vector2(.01f,1),new Vector2(.99f,1));var rr=(RectTransform)card.transform;rr.pivot=new Vector2(.5f,1);rr.anchoredPosition=new Vector2(0,-n*105);rr.sizeDelta=new Vector2(0,97);ImperialTheme.Panel(card,false);Portrait(card.transform,id,new Vector2(.01f,.05f),new Vector2(.26f,.95f));Text(card.transform,c.Name+"\n"+(ledger?MercenaryContracts.StatusText(id):c.Gold.ToString("N0")+" gold · "+c.Days+" days"),.28f,.06f,.69f,.88f,21);var b=card.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=card.GetComponent<UnityEngine.UI.Image>();b.onClick.AddListener(()=>{selected=id;notice="";Draw();});}
 var scroll=index.AddComponent<ScrollRect>();scroll.content=cr;scroll.viewport=(RectTransform)index.transform;scroll.horizontal=false;scroll.scrollSensitivity=40;scroll.movementType=ScrollRect.MovementType.Clamped;CampaignHUD.AddScrollbar(scroll,pane.transform,new Vector2(.401f,.15f),new Vector2(.415f,.91f));
 var company=MercenaryRoster.All[selected];var state=MercenaryContracts.State(selected);Portrait(pane.transform,selected,new Vector2(.44f,.63f),new Vector2(.64f,.90f));Text(pane.transform,company.Name+"\n"+company.Captain+"\n"+company.Tradition,.66f,.63f,.31f,.27f,25);
 string detail=company.Package+"\n\n"+company.Port+" · "+company.Days+" days · "+(company.Flotilla?"Company flotilla":"Chartered passage");
 if(state==MercenaryContracts.Status.Unhired)detail+="\nArrival without incident: "+MercenaryContracts.SafeChance(selected)+"%\n"+company.Gold.ToString("N0")+" gold · Half now, half on arrival"+(company.Sanctuary?"\n150 dependants · Noble support −5 points":"");
 else detail+="\n"+MercenaryContracts.StatusText(selected)+"\nPaid: "+MercenaryContracts.Get(selected,"Paid").ToString("N0")+" · Due on arrival: "+(MercenaryContracts.Get(selected,"Price")/2).ToString("N0");
 if(state==MercenaryContracts.Status.Decision||(state==MercenaryContracts.Status.Lost&&MercenaryContracts.Get(selected,"Handled")>0&&MercenaryContracts.Get(selected,"Incident")>0))detail=MercenaryContracts.IncidentTitle((MercenaryContracts.Incident)MercenaryContracts.Get(selected,"Incident"))+"\n\n"+MercenaryContracts.IncidentText(selected);
 if(state==MercenaryContracts.Status.Lost&&(MercenaryContracts.Incident)MercenaryContracts.Get(selected,"Incident")==MercenaryContracts.Incident.Straits){var art=new GameObject("Ambush in the straits artwork",typeof(RectTransform),typeof(RawImage));art.transform.SetParent(root.transform,false);var r=(RectTransform)art.transform;r.anchorMin=new Vector2(.025f,.37f);r.anchorMax=new Vector2(.32f,.85f);r.offsetMin=r.offsetMax=Vector2.zero;art.GetComponent<RawImage>().texture=Resources.Load<Texture2D>("UI/Maps/Landmarks/rumeli-intact");art.GetComponent<RawImage>().raycastTarget=false;}
 Text(pane.transform,detail,.44f,.29f,.53f,.32f,23);
 if(state==MercenaryContracts.Status.Unhired){string reason=MercenaryContracts.HireReason(selected);Button(pane.transform,"Sign · "+(company.Gold/2).ToString("N0")+" gold",.44f,.20f,.53f,.075f,()=>Act(()=>{MercenaryContracts.Sign(selected,out var r);return r;}),reason==null);if(reason!=null)Text(pane.transform,reason,.44f,.145f,.53f,.05f,18);}
 else if(state==MercenaryContracts.Status.Payment){Button(pane.transform,"Pay balance · "+(MercenaryContracts.Get(selected,"Price")/2).ToString("N0")+" gold",.44f,.20f,.53f,.075f,()=>Act(()=>MercenaryContracts.Resolve(selected,0)),DailyLifeEvents.I("iMoney")>=MercenaryContracts.Get(selected,"Price")/2);Button(pane.transform,"Turn them away · They join Mehmed",.44f,.11f,.53f,.075f,()=>Act(()=>MercenaryContracts.Resolve(selected,2)));}
 else if(state==MercenaryContracts.Status.Decision){bool disputed=(MercenaryContracts.Incident)MercenaryContracts.Get(selected,"Incident")==MercenaryContracts.Incident.Disputed;int extra=MercenaryContracts.Get(selected,"Price")/(disputed?2:5);Button(pane.transform,"Pay surcharge · "+extra.ToString("N0")+" gold",.44f,.205f,.53f,.065f,()=>Act(()=>MercenaryContracts.Resolve(selected,0)),DailyLifeEvents.I("iMoney")>=extra);if(disputed)Button(pane.transform,"Spend one camp agent permanently",.44f,.13f,.53f,.065f,()=>Act(()=>MercenaryContracts.Resolve(selected,1)),DailyLifeEvents.I("iSpies")>0);Button(pane.transform,"Abandon contract",.44f,.055f,.53f,.065f,()=>Act(()=>MercenaryContracts.Resolve(selected,2)));}
 Text(pane.transform,notice,.025f,.005f,.39f,.13f,18);
 }
 static void Portrait(Transform p,int id,Vector2 min,Vector2 max){var go=new GameObject("Captain "+id,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(p,false);var rect=(RectTransform)go.transform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var image=go.GetComponent<RawImage>();image.texture=Resources.Load<Texture2D>("UI/Mercenaries/captains");int artId=id<20?id:0;image.uvRect=new Rect(artId%5*.2f,(3-artId/5)*.25f,.2f,.25f);image.raycastTarget=false;}
 void Update(){if(Input.GetKeyDown(KeyCode.Escape))Close();}
}
