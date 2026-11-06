using System.Linq;
using UnityEngine;
using TMPro;
public partial class CouncilUI
{
    public void CommanderSelection()
    {
        Open("Siege commander","Choose a serving adviser to review their appointment.");
        ControlSingleton.Instance.ReturnAction=Overview;
        var candidates=Enumerable.Range(0,8).Where(CourtRoster.InService).ToList();
        foreach(int special in new[]{200,203})if(CourtRoster.CanCommand(special))candidates.Add(special);
        if(CharacterPreparations.InService)candidates.Add(CharacterPreparations.CommanderId);
        BodyHeight(Mathf.Max(140,candidates.Count*140));
        for(int i=0;i<candidates.Count;i++){
            int id=candidates[i];var row=Row(i,140);bool current=CouncilActions.Int("iCommander")==id;
            var title=CampaignHUD.Label(row.transform,CourtRoster.CommanderName(id)+(current?" · Current commander":""),new Vector2(.025f,.55f),new Vector2(.73f,.96f),26);
            var bonus=CampaignHUD.Label(row.transform,CourtRoster.CommanderBenefit(id),new Vector2(.025f,.06f),new Vector2(.73f,.54f),22);
            foreach(var text in new[]{title,bonus}){text.enableAutoSizing=true;text.fontSizeMin=17;text.fontSizeMax=text.fontSize;}
            var review=CampaignHUD.MakeButton(row.transform,current?"Appointed":"Review appointment",new Vector2(.76f,.18f),new Vector2(.98f,.82f),()=>ReviewCommander(id));review.interactable=!current;
        }
        if(candidates.Count==0)CampaignHUD.Label(body,"No serving adviser is available for appointment.",new Vector2(.03f,.2f),new Vector2(.97f,.9f),26);
    }
    public void ReviewCommander(int id)
    {
        if(!CourtRoster.CanCommand(id)){CommanderSelection();return;}
        Open("Appoint siege commander",CourtRoster.CommanderName(id));
        ControlSingleton.Instance.ReturnAction=CommanderSelection;BodyHeight(360);
        CampaignHUD.Label(body,CourtRoster.CommanderBenefit(id)+"\n\nReplaces "+CourtRoster.CommanderName(CouncilActions.Int("iCommander"))+" as siege commander. The outgoing adviser's ordinary specialist benefit remains while they are in service.",new Vector2(.05f,.37f),new Vector2(.95f,.94f),27);
        CampaignHUD.MakeButton(body,"Return",new Vector2(.05f,.08f),new Vector2(.46f,.26f),CommanderSelection);
        bool submitted=false;
        var appoint=CampaignHUD.MakeTurnButton(body,"Confirm appointment",new Vector2(.52f,.08f),new Vector2(.95f,.26f),()=>{if(submitted)return;bool ok=CourtRoster.AppointCommander(id,out var report);submitted=ok;Finish(ok,report);});
        appoint.interactable=CouncilActions.Int("iCommander")!=id&&CourtRoster.CanCommand(id);
    }
}
