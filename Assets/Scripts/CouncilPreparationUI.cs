using UnityEngine;

public partial class CouncilUI
{
    public void FleetOrders()
    {
        Open("Orders for the whole fleet","Changing mission uses the major action. It does not renew preparation bonuses.");
        ControlSingleton.Instance.ReturnAction=Fleet;BodyHeight(6*155);
        var battle=Row(5,155);
        CampaignHUD.Label(battle.transform,"Seek battle · "+NavalState.VictoryChance+"% victory chance",new Vector2(.03f,.25f),new Vector2(.65f,.9f),25);
        var attack=CampaignHUD.MakeButton(battle.transform,"Review battle",new Vector2(.7f,.17f),new Vector2(.97f,.83f),ReviewNavalBattle);attack.interactable=CouncilActions.Int("iShips")>0&&NavalState.EnemyShips>0;
        string[] titles={"Raid Ottoman supplies","Raid coastal shipping","Bring grain from Crimea","Defend the Golden Horn","Bring volunteers"};
        for(int i=0;i<5;i++){
            int mission=i;var row=Row(i,155);
            CampaignHUD.Label(row.transform,titles[i],new Vector2(.03f,.2f),new Vector2(.65f,.85f),26);
            bool current=CouncilActions.Int("iFleet")==i;
            var button=CampaignHUD.MakeButton(row.transform,current?"Current orders":"Issue orders",new Vector2(.7f,.17f),new Vector2(.97f,.83f),()=>{bool ok=PreparationCampaign.ChangeFleet(mission,out var result);Finish(ok,result);});
            button.interactable=!current&&CouncilActions.Int("iShips")>0&&(i!=2||VariableSingleton.GetBoolVariable("bStraitsOpen"));
        }
    }
    public void ReviewNavalBattle()
    {
        Open("Commit the fleet","Victory and defeat both cost ships. This uses your major action.");ControlSingleton.Instance.ReturnAction=FleetOrders;BodyHeight(440);
        var terms=Row(0,290);CampaignHUD.Explanation(terms.transform,"Our ships: "+CouncilActions.Int("iShips")+" · Ottoman vessels: "+NavalState.EnemyShips+"\nVictory chance: "+NavalState.VictoryChance+"%\nVictory: lose 5–15% of our ships; enemy loses 40–65%.\nDefeat: lose 25–50% of our ships; enemy loses 10–25%.\nThe Ottomans receive one replacement ship each siege day.",new Vector2(.04f,.08f),new Vector2(.96f,.94f),25);
        bool submitted=false;var controls=Row(2,145);CampaignHUD.MakeButton(controls.transform,"Return",new Vector2(.04f,.1f),new Vector2(.45f,.9f),FleetOrders);CampaignHUD.MakeButton(controls.transform,"Confirm battle",new Vector2(.52f,.1f),new Vector2(.96f,.9f),()=>{if(submitted||CouncilActions.Int("iShips")<=0||NavalState.EnemyShips<=0)return;submitted=true;bool won=Random.Range(0,100)<NavalState.VictoryChance;NavalState.Battle(won);Finish(true,(won?"The enemy yields the sea.\n":"Our captains withdraw to the harbour.\n")+NavalState.LastReport);});
    }
    public void Galata()
    {
        int state=PreparationCampaign.GalataState;
        Open("Galatan diplomacy",PreparationCampaign.GalataStates[state]+" · Goodwill "+VariableSingleton.GetPercentFloat("fGalataRep")+" · Defences "+CouncilActions.Int("iGalataDefences")+" · Giovanni loyalty "+CouncilActions.Int("iLoyalty0"));
        ControlSingleton.Instance.ReturnAction=Diplomacy;BodyHeight(11*145);
        var status=Row(0,145);
        CampaignHUD.Explanation(status.transform,"Diplomatic claims: "+CouncilActions.Int("iGalataClaims")+" · Debt: "+CouncilActions.Int("iGalataDebt")+" gold.\n"+(state==0?"Respect their neutrality, buy goodwill or press for immediate support. Armed seizure remains a preparation decision.":state==3?"Ottoman control prevents interaction.":"Galata is exposed to Ottoman attacks. Giovanni's personal loyalty is separate from Galatan goodwill."),new Vector2(.03f,.08f),new Vector2(.97f,.94f),23);
        int[] order={8,9,0,1,2,3,4,5,6,7};
        string[] labels={"Bribe the podestà · 7,500 gold · +30 goodwill","Gifts and negotiation · 2,000 gold · "+(CouncilActions.Int("iFocus")==3?"+30 goodwill · Faith":"+10 goodwill"),"Redeem claim · 1,000 provisions","Redeem claim · 5,000-gold loan","Redeem claim · 150 infantry and 100 archers","Redeem claim · +10 Galata defences","Pay Giovanni's men · 3,000 gold · +10 loyalty","Requisition autonomous revenues · +3,000 gold · −20 loyalty","Demand immediate support · 300 infantry, 300 archers, 4,500 gold, 500 provisions · −20 goodwill","Respect friendly neutrality · +10 goodwill · once per campaign"};
        for(int pos=0;pos<order.Length;pos++) {
            int action=order[pos];var row=Row(pos+1,145);
            CampaignHUD.Label(row.transform,labels[action],new Vector2(.03f,.10f),new Vector2(.69f,.92f),23);
            string reason=PreparationCampaign.GalataActionUnavailable(action);
            var button=CampaignHUD.MakeButton(row.transform,reason??"Order · major action",new Vector2(.72f,.18f),new Vector2(.97f,.82f),()=>{bool ok=PreparationCampaign.DiplomaticAction(action,out var report);Finish(ok,report);});button.interactable=reason==null;
        }
    }
}
