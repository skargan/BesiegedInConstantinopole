using UnityEngine;

public partial class CouncilUI
{
    public void ReviewAssassination()
    {
        Open("Assassinate Mehmed","Ten agents in the enemy camp can attempt to end the siege at its source.");
        ControlSingleton.Instance.ReturnAction=Spies;BodyHeight(520);
        var terms=SpyPanel("Assassination terms",0,390);
        CampaignHUD.Label(terms.transform,"20% SUCCESS",new Vector2(.04f,.79f),new Vector2(.96f,.97f),30);
        CampaignHUD.Label(terms.transform,"Success: Mehmed dies. Constantinople wins the campaign.\n\nFailure: Sultan’s fury +50 points, up to 100%. Enemy camp alert rises to 100% and can never fall below 50% for the rest of this campaign.\n\nAnother attempt may be made after 30 days.",new Vector2(.04f,.15f),new Vector2(.96f,.77f),26);
        CampaignHUD.Label(terms.transform,MehmedAssassination.Unavailable??"Ten agents required · No gold cost",new Vector2(.04f,.02f),new Vector2(.96f,.15f),22);
        var buttons=SpyPanel("Assassination decision",404,100);bool submitted=false;
        CampaignHUD.MakeButton(buttons.transform,"Cancel",new Vector2(.03f,.13f),new Vector2(.46f,.87f),Spies);
        var confirm=CampaignHUD.MakeTurnButton(buttons.transform,"Confirm attempt",new Vector2(.52f,.13f),new Vector2(.97f,.87f),()=>{
            if(submitted)return;bool accepted=MehmedAssassination.Attempt(out var report);if(accepted)submitted=true;
            Finish(accepted,report);if(accepted&&MehmedAssassination.Victory)CampaignHUD.Instance.CheckEnding();
        });confirm.interactable=MehmedAssassination.Available;
    }
}
