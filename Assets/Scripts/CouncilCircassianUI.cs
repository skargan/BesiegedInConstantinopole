using UnityEngine;
public partial class CouncilUI
{
 public void CircassianPort()
 {
 var pane=MilitaryCanvas("Copa · The Circassian coast",CircassianOutpost.Established?"copa-compound":"copa-port","An anchorage between the river, the steppe and the sea.");ControlSingleton.Instance.ReturnAction=()=>{Hide();WarTheatreUI.Show();};
 var desk=(RectTransform)pane;desk.anchorMin=new Vector2(.025f,.025f);desk.anchorMax=new Vector2(.975f,.34f);desk.offsetMin=desk.offsetMax=Vector2.zero;
 screen.transform.Find("Scene heading").gameObject.SetActive(false);
 CompactText(pane,"THE KUBAN ANCHORAGE",.02f,.79f,.48f,.97f,27);
 CompactText(pane,CircassianOutpost.Summary,.02f,.26f,.34f,.76f,20);
 if(!CircassianOutpost.Established){var b=CampaignHUD.MakeButton(pane,"Establish compound · 5,000 gold",new Vector2(.02f,.05f),new Vector2(.34f,.23f),()=>ReviewCopa(0));b.interactable=CircassianOutpost.EstablishReason==null;}
 else {CampaignHUD.MakeButton(pane,"Ship gold · 350 / week",new Vector2(.02f,.05f),new Vector2(.175f,.23f),()=>{CircassianOutpost.SetCargo(false);VariableSingleton.Instance.Save();CircassianPort();});CampaignHUD.MakeButton(pane,"Ship food · "+CircassianOutpost.FoodQuantity+" / week",new Vector2(.185f,.05f),new Vector2(.34f,.23f),()=>{CircassianOutpost.SetCargo(true);VariableSingleton.Instance.Save();CircassianPort();});}
 var c=MercenaryRoster.All[CircassianOutpost.Company];CompactText(pane,"The Kuban Household · "+c.Package+"\nLocal agreement: "+(CircassianOutpost.Established?95:80)+"% · Half of "+c.Gold.ToString("N0")+" gold paid in advance. A failed negotiation loses the messenger and advance.",.37f,.27f,.72f,.76f,20);
 var recruit=CampaignHUD.MakeButton(pane,"Negotiate passage",new Vector2(.37f,.05f),new Vector2(.72f,.23f),()=>ReviewCopa(1));recruit.interactable=CircassianOutpost.RecruitReason==null;
 CompactText(pane,"An anchorage between the river, the steppe and the sea.",.76f,.30f,.98f,.76f,20);
 CampaignHUD.MakeButton(pane,"Return to map",new Vector2(.76f,.05f),new Vector2(.98f,.23f),()=>ControlSingleton.Instance.GoBack());
 }
 void ReviewCopa(int action)
 {
 Open("Copa · Review agreement","");ControlSingleton.Instance.ReturnAction=CircassianPort;BodyHeight(400);
 CampaignHUD.Explanation(body,action==0?"Pay 5,000 gold to establish a trading compound. Choose gold or food for future weekly convoys; deliveries take fourteen days and face sea hazards. Closed Bosphorus holds departures. Food comes from a finite export reserve.":"Pay half the mercenary contract now. The messenger has a "+(CircassianOutpost.Established?95:80)+"% chance of securing local agreement. Failure kills the messenger and loses the advance. On success, the company follows the ordinary saved-voyage rules; the remaining half is due on arrival.",new Vector2(.04f,.35f),new Vector2(.96f,.95f),25);
 CampaignHUD.MakeButton(body,"Confirm",new Vector2(.04f,.12f),new Vector2(.60f,.29f),()=>{string report=action==0?CircassianOutpost.Establish():CircassianOutpost.Recruit();SaveGames.Checkpoint();CircassianPort();CampaignHUD.Instance.ShowReport("Copa · Imperial dispatch",report,()=>CampaignHUD.Instance.DismissReport());});CampaignHUD.MakeButton(body,"Cancel",new Vector2(.65f,.12f),new Vector2(.96f,.29f),CircassianPort);
 }
}
