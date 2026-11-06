using UnityEngine;
public partial class CouncilUI
{
 public void CircassianPort()
 {
 var pane=MilitaryCanvas("Copa · The Circassian coast",CircassianOutpost.Established?"copa-compound":"copa-port","An anchorage between the river, the steppe and the sea.");ControlSingleton.Instance.ReturnAction=()=>{Hide();WarTheatreUI.Show();};
 CompactText(pane,"THE KUBAN ANCHORAGE",.04f,.91f,.96f,.98f,28);
 CompactText(pane,CircassianOutpost.Established?"Genoese factors bargain with Circassian lords along the quays. Timber, grain and men change hands here; an imperial seal may open a warehouse door, but it will not settle every quarrel beyond the harbour.":"From a grassy rise, your emissary surveys the anchorage with the local Circassian lords. Below them lie an open meadow and a sheltered landing. There is room for warehouses and a small harbour, if an agreement can be reached.",.04f,.70f,.96f,.90f,24);
 CompactText(pane,CircassianOutpost.Summary,.04f,.49f,.96f,.70f,22);
 if(!CircassianOutpost.Established){var b=CampaignHUD.MakeButton(pane,"Establish compound · 5,000 gold",new Vector2(.04f,.39f),new Vector2(.96f,.48f),()=>ReviewCopa(0));b.interactable=CircassianOutpost.EstablishReason==null;}
 else {CampaignHUD.MakeButton(pane,"Ship gold · 350 / week",new Vector2(.04f,.39f),new Vector2(.48f,.48f),()=>{CircassianOutpost.SetCargo(false);VariableSingleton.Instance.Save();CircassianPort();});CampaignHUD.MakeButton(pane,"Ship food · "+CircassianOutpost.FoodQuantity+" / week",new Vector2(.52f,.39f),new Vector2(.96f,.48f),()=>{CircassianOutpost.SetCargo(true);VariableSingleton.Instance.Save();CircassianPort();});}
 var c=MercenaryRoster.All[CircassianOutpost.Company];CompactText(pane,"The Kuban Household · "+c.Package+"\nLocal agreement: "+(CircassianOutpost.Established?95:80)+"% · Half of "+c.Gold.ToString("N0")+" gold paid in advance. A failed negotiation loses the messenger and advance.",.04f,.21f,.96f,.37f,21);
 var recruit=CampaignHUD.MakeButton(pane,"Negotiate the company's passage",new Vector2(.04f,.115f),new Vector2(.96f,.20f),()=>ReviewCopa(1));recruit.interactable=CircassianOutpost.RecruitReason==null;
 CampaignHUD.MakeButton(pane,"Return to map",new Vector2(.23f,.025f),new Vector2(.77f,.095f),()=>ControlSingleton.Instance.GoBack());
 }
 void ReviewCopa(int action)
 {
 Open("Copa · Review agreement","");ControlSingleton.Instance.ReturnAction=CircassianPort;BodyHeight(400);
 CampaignHUD.Explanation(body,action==0?"Pay 5,000 gold to establish a trading compound. Choose gold or food for future weekly convoys; deliveries take fourteen days and face sea hazards. Closed Bosphorus holds departures. Food comes from a finite export reserve.":"Pay half the mercenary contract now. The messenger has a "+(CircassianOutpost.Established?95:80)+"% chance of securing local agreement. Failure kills the messenger and loses the advance. On success, the company follows the ordinary saved-voyage rules; the remaining half is due on arrival.",new Vector2(.04f,.35f),new Vector2(.96f,.95f),25);
 CampaignHUD.MakeButton(body,"Confirm",new Vector2(.04f,.12f),new Vector2(.60f,.29f),()=>{string report=action==0?CircassianOutpost.Establish():CircassianOutpost.Recruit();SaveGames.Checkpoint();CircassianPort();CampaignHUD.Instance.ShowReport("Copa · Imperial dispatch",report,()=>CampaignHUD.Instance.DismissReport());});CampaignHUD.MakeButton(body,"Cancel",new Vector2(.65f,.12f),new Vector2(.96f,.29f),CircassianPort);
 }
}
