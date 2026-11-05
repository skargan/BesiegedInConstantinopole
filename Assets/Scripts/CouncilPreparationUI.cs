using UnityEngine;

public partial class CouncilUI
{
    public void FleetOrders()
    {
        var pane=MilitaryCanvas("Orders for the Fleet","fleet-orders","");ControlSingleton.Instance.ReturnAction=Fleet;
        CompactText(pane,"ORDERS FOR THE FLEET",.04f,.92f,.96f,.99f,28);
        int[] sequence={3,2,4,1,0};for(int n=0;n<sequence.Length;n++){int mission=sequence[n];float top=.91f-n*.158f;var row=CompactCard(pane,"Fleet mission "+mission,top-.15f,top);
        CompactText(row,FleetMissionRules.Names[mission],.025f,.66f,.975f,.98f,23);
        CompactText(row,FleetMissionRules.Descriptions[mission],.025f,.06f,.73f,.65f,19);
        bool current=CouncilActions.Int("iFleet")==mission;bool closed=mission==2&&!VariableSingleton.GetBoolVariable("bStraitsOpen");var button=CampaignHUD.MakeButton(row,current?"Current orders":closed?"Straits closed":"Review",new Vector2(.76f,.08f),new Vector2(.975f,.61f),()=>ReviewFleetMission(mission));button.interactable=!current&&!closed&&CouncilActions.Int("iShips")>0;}
        CampaignHUD.MakeButton(pane,"Return",new Vector2(.04f,.025f),new Vector2(.48f,.095f),Fleet);
        CampaignHUD.MakeButton(pane,"Naval battle",new Vector2(.52f,.025f),new Vector2(.96f,.095f),ReviewNavalBattle);
    }
    public void ReviewFleetMission(int mission)
    {
        if(mission<0||mission>4)return;var pane=MilitaryCanvas("Review Fleet orders","fleet-orders","");ControlSingleton.Instance.ReturnAction=FleetOrders;
        CompactText(pane,FleetMissionRules.Names[mission],.05f,.83f,.95f,.98f,30);
        CompactText(pane,FleetMissionRules.Descriptions[mission],.05f,.48f,.95f,.82f,25);
        CompactText(pane,mission==3?"The Fleet participates in the defence during assaults; no routine report cycle.":"The whole Fleet takes this mission. Reports arrive after 14–21 active siege days; changing orders restarts that interval.",.05f,.25f,.95f,.48f,22);
        bool submitted=false;var confirm=CampaignHUD.MakeTurnButton(pane,"Confirm mission",new Vector2(.05f,.12f),new Vector2(.95f,.23f),()=>{if(submitted)return;bool ok=PreparationCampaign.ChangeFleet(mission,out var report);submitted=ok;Finish(ok,report);});confirm.interactable=CouncilActions.Int("iShips")>0&&CouncilActions.Int("iFleet")!=mission&&(mission!=2||VariableSingleton.GetBoolVariable("bStraitsOpen"));
        CampaignHUD.MakeButton(pane,"Cancel",new Vector2(.25f,.025f),new Vector2(.75f,.10f),FleetOrders);
    }

    public void ReviewNavalBattle()
    {
        Open("Commit the fleet","Victory and defeat both cost ships.");ControlSingleton.Instance.ReturnAction=FleetOrders;BodyHeight(440);
        var terms=Row(0,290);CampaignHUD.Explanation(terms.transform,"Our ships: "+CouncilActions.Int("iShips")+" · Ottoman vessels: "+NavalState.EnemyShips+"\nVictory chance: "+NavalState.VictoryChance+"%\nVictory: lose 5–15% of our ships; enemy loses 40–65%.\nDefeat: lose 25–50% of our ships; enemy loses 10–25%.\nOttoman provincial shipyards dispatch reinforcements fortnightly; output depends on Wealth and ownership.",new Vector2(.04f,.08f),new Vector2(.96f,.94f),25);
        bool submitted=false;var controls=Row(2,145);CampaignHUD.MakeButton(controls.transform,"Return",new Vector2(.04f,.1f),new Vector2(.45f,.9f),FleetOrders);CampaignHUD.MakeTurnButton(controls.transform,"Confirm battle",new Vector2(.52f,.1f),new Vector2(.96f,.9f),()=>{if(submitted||CouncilActions.Int("iShips")<=0||NavalState.EnemyShips<=0)return;submitted=true;bool won=Random.Range(0,100)<NavalState.VictoryChance;NavalState.Battle(won);Finish(true,(won?"The enemy yields the sea.\n":"Our captains withdraw to the harbour.\n")+NavalState.LastReport);});
    }
}
