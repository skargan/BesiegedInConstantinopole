using UnityEngine;

public partial class CouncilUI
{
    public void FleetOrders()
    {
        Open("Orders for the whole fleet","Changing mission does not renew preparation bonuses.");
        ControlSingleton.Instance.ReturnAction=Fleet;BodyHeight(6*155);
        var battle=Row(5,155);
        CampaignHUD.Label(battle.transform,"Seek battle · "+NavalState.VictoryChance+"% victory chance",new Vector2(.03f,.25f),new Vector2(.65f,.9f),25);
        var attack=CampaignHUD.MakeButton(battle.transform,"Review battle",new Vector2(.7f,.17f),new Vector2(.97f,.83f),ReviewNavalBattle);attack.interactable=CouncilActions.Int("iShips")>0&&NavalState.EnemyShips>0;
        string[] titles={"Raid Ottoman supplies","Raid coastal shipping","Bring grain from Crimea","Defend the Golden Horn","Bring volunteers"};
        string[] effects={"−3 days of Ottoman food stores","+750 gold","+150 provisions","Inflict "+RecurringEvents.FleetDefensiveDamage+" enemy casualties","+50 infantry and +25 archers"};
        for(int i=0;i<5;i++){
            int mission=i;var row=Row(i,155);
            CampaignHUD.Label(row.transform,titles[i]+"\nSuccessful report: "+effects[i],new Vector2(.03f,.12f),new Vector2(.65f,.90f),24);
            bool current=CouncilActions.Int("iFleet")==i;
            var button=CampaignHUD.MakeButton(row.transform,current?"Current orders":"Review orders",new Vector2(.7f,.17f),new Vector2(.97f,.83f),()=>ReviewFleetMission(mission));
            button.interactable=!current&&CouncilActions.Int("iShips")>0&&(i!=2||VariableSingleton.GetBoolVariable("bStraitsOpen"));
        }
    }
    public void ReviewFleetMission(int mission)
    {
        if(mission<0||mission>4)return;
        string[] names={"Raid Ottoman supplies","Raid coastal shipping","Bring grain from Crimea","Defend the Golden Horn","Bring volunteers","Awaiting orders"};
        string[] effects={"−3 days of Ottoman food stores","+750 gold","+150 provisions",RecurringEvents.FleetDefensiveDamage+" enemy casualties","+50 infantry and +25 archers"};
        Open("Review fleet orders",names[mission]);ControlSingleton.Instance.ReturnAction=FleetOrders;BodyHeight(500);
        var current=SpyPanel("Current fleet orders",0,260);var cr=current.GetComponent<RectTransform>();cr.anchorMax=new Vector2(.49f,1);
        FleetText(current.transform,"CURRENT MISSION",.05f,.72f,.95f,.95f,24);
        FleetText(current.transform,names[Mathf.Clamp(CouncilActions.Int("iFleet"),0,5)],.05f,.36f,.95f,.70f,28);
        FleetText(current.transform,CouncilActions.Int("iShips")+" ships in service",.05f,.08f,.95f,.30f,23);
        var proposed=SpyPanel("Proposed fleet orders",0,260);var pr=proposed.GetComponent<RectTransform>();pr.anchorMin=new Vector2(.51f,1);
        FleetText(proposed.transform,"PROPOSED MISSION",.05f,.72f,.95f,.95f,24);
        FleetText(proposed.transform,names[mission],.05f,.36f,.95f,.70f,28);
        FleetText(proposed.transform,"Successful report: "+effects[mission],.05f,.07f,.95f,.32f,23);
        var terms=SpyPanel("Order consequences",275,115);
        FleetText(terms.transform,"The whole fleet changes mission.\n"+(CouncilActions.Int("iFleetPrepared")>0?"Your prepared expedition bonus will be lost.":"No preparation bonus is granted.")+" Rewards arrive through later fleet reports.",.03f,.08f,.97f,.92f,23);
        var controls=SpyPanel("Confirm fleet orders",405,80);bool submitted=false;
        CampaignHUD.MakeButton(controls.transform,"Return",new Vector2(.03f,.12f),new Vector2(.47f,.88f),FleetOrders);
        var confirm=CampaignHUD.MakeTurnButton(controls.transform,"Confirm mission",new Vector2(.53f,.12f),new Vector2(.97f,.88f),()=>{if(submitted)return;bool ok=PreparationCampaign.ChangeFleet(mission,out var result);submitted=ok;Finish(ok,result);});
        confirm.interactable=CouncilActions.Int("iShips")>0&&CouncilActions.Int("iFleet")!=mission&&(mission!=2||VariableSingleton.GetBoolVariable("bStraitsOpen"));
    }

    public void ReviewNavalBattle()
    {
        Open("Commit the fleet","Victory and defeat both cost ships.");ControlSingleton.Instance.ReturnAction=FleetOrders;BodyHeight(440);
        var terms=Row(0,290);CampaignHUD.Explanation(terms.transform,"Our ships: "+CouncilActions.Int("iShips")+" · Ottoman vessels: "+NavalState.EnemyShips+"\nVictory chance: "+NavalState.VictoryChance+"%\nVictory: lose 5–15% of our ships; enemy loses 40–65%.\nDefeat: lose 25–50% of our ships; enemy loses 10–25%.\nThe Ottomans receive one replacement ship each siege day.",new Vector2(.04f,.08f),new Vector2(.96f,.94f),25);
        bool submitted=false;var controls=Row(2,145);CampaignHUD.MakeButton(controls.transform,"Return",new Vector2(.04f,.1f),new Vector2(.45f,.9f),FleetOrders);CampaignHUD.MakeTurnButton(controls.transform,"Confirm battle",new Vector2(.52f,.1f),new Vector2(.96f,.9f),()=>{if(submitted||CouncilActions.Int("iShips")<=0||NavalState.EnemyShips<=0)return;submitted=true;bool won=Random.Range(0,100)<NavalState.VictoryChance;NavalState.Battle(won);Finish(true,(won?"The enemy yields the sea.\n":"Our captains withdraw to the harbour.\n")+NavalState.LastReport);});
    }
}
