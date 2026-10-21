using UnityEngine;
public partial class CouncilUI
{
    public void ReviewPoliticalFavour(int bloc)
    {
        Open(PoliticalSystem.Names[bloc],"A favour from the faction strengthens its hand at court.");
        ControlSingleton.Instance.ReturnAction=Politics;BodyHeight(500);
        CampaignHUD.Explanation(body,PoliticalSystem.Effects[bloc]+"\n\nUses your major action. Their influence rises by up to 5 points. You will owe one favour, which they may call in from "+VariableSingleton.GetDate().AddDays(30).ToString("d MMM yyyy")+".\n\nYou may request another favour from this faction after seven days. Passive benefit: "+PoliticalSystem.Passives[bloc]+".",new Vector2(.05f,.30f),new Vector2(.95f,.95f),25);
        bool submitted=false;
        var accept=CampaignHUD.MakeButton(body,"Accept favour and obligation",new Vector2(.52f,.06f),new Vector2(.95f,.23f),()=>{if(submitted)return;bool ok=PoliticalSystem.Aid(bloc,out var result);submitted=ok;Finish(ok,result);});
        accept.interactable=PoliticalSystem.CanAid(bloc);
        CampaignHUD.MakeButton(body,"Return",new Vector2(.05f,.06f),new Vector2(.46f,.23f),Politics);
    }
}
