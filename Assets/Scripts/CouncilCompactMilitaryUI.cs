using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    // These views only reorganise presentation. Existing order and confirmation handlers own costs/time.
    Transform MilitaryCanvas(string title,string art,string subtitle)
    {
        Open(title,"");
        foreach(Transform child in screen.transform)child.gameObject.SetActive(false);
        SceneBackdrop.Apply(screen,art);
        screen.transform.Find("Scene artwork").gameObject.SetActive(true);
        var heading=CampaignHUD.Panel(screen.transform,"Scene heading",new Vector2(.015f,.85f),new Vector2(.425f,.985f));
        heading.GetComponent<Image>().color=new Color(.015f,.035f,.05f,.84f);
        CompactText(heading.transform,title,.05f,.48f,.96f,.95f,32);
        CompactText(heading.transform,subtitle,.05f,.05f,.96f,.48f,20);
        var pane=CampaignHUD.Panel(screen.transform,"Command desk",new Vector2(.445f,.025f),new Vector2(.985f,.985f));
        pane.GetComponent<Image>().color=new Color(.025f,.045f,.065f,.96f);ImperialTheme.Frame(pane);
        notice=CampaignHUD.Label(screen.transform,"",new Vector2(.025f,.02f),new Vector2(.425f,.12f),20);
        return pane.transform;
    }
    TMP_Text CompactText(Transform parent,string value,float x0,float y0,float x1,float y1,int size=22)
    {
        var t=CampaignHUD.Label(parent,value,new Vector2(x0,y0),new Vector2(x1,y1),size);
        t.alignment=TextAlignmentOptions.MidlineLeft;t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=size;
        t.overflowMode=TextOverflowModes.Overflow;return t;
    }
    Transform CompactCard(Transform parent,string name,float low,float high)
    {
        var card=CampaignHUD.Panel(parent,name,new Vector2(.025f,low),new Vector2(.975f,high));
        card.GetComponent<Image>().color=ImperialTheme.Surface;ImperialTheme.Frame(card);return card.transform;
    }
    void CompactTroopUpgrades(int selected)
    {
        var pane=MilitaryCanvas("Training and equipment","troop-upgrades","A steadier hand. A better edge. A stronger line.");
        // Keep the full three-man ensemble beside the desk, rather than cropping the third man under it.
        screen.transform.Find("Scene artwork").GetComponent<RectTransform>().anchorMax=new Vector2(.88f,1);
        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.BringUpTroopsUI();};
        CompactText(pane,"THE GARRISON'S CRAFT",.045f,.93f,.95f,.985f,28);
        CompactText(pane,"Five days of drills; your troops remain ready for battle.",.045f,.885f,.95f,.935f,21);
        string[] names={"Roman Tactics","Upgraded Crossbows","Cataphract Armour"};
        string[] keys={"RomanTraining","UpgradedCrossbows","PlateArmour"};
        for(int i=0;i<3;i++)
        {
            int unit=i,rank=TroopTraining.Rank(i);float top=.88f-i*.22f;
            var row=CompactCard(pane,"Upgrade "+TroopTraining.Units[i],top-.21f,top);
            CompactText(row,TroopTraining.Units[i]+" · "+TroopTraining.Ranks[rank],.025f,.78f,.69f,.99f,26);
            CompactText(row,"×"+TroopTraining.Lethality(i).ToString("0.##")+" lethality",.70f,.78f,.975f,.99f,23).alignment=TextAlignmentOptions.MidlineRight;
            for(int j=0;j<5;j++)
            {
                var pip=CampaignHUD.Panel(row,"Rank "+j,new Vector2(.025f+j*.19f,.63f),new Vector2(.205f+j*.19f,.76f));
                pip.GetComponent<Image>().color=j<=rank?new Color(.57f,.40f,.16f):new Color(.08f,.10f,.13f);
                var t=CompactText(pip.transform,TroopTraining.Ranks[j],0,0,1,1,18);t.alignment=TextAlignmentOptions.Center;
            }
            string tech=names[i]+": "+(TechnologySystem.Has(keys[i])?"active (+25%)":"not researched");
            string extra=i==0?"Grenades":i==1?"Handcannons":"DismountedChampions";
            string extraName=i==2?"Dismounted Champions":extra;
            tech+=" · "+extraName+": "+(TechnologySystem.Has(extra)?"active":"not researched");
            CompactText(row,tech,.025f,.43f,.975f,.63f,19);
            CompactText(row,TroopTraining.Breakdown(i),.025f,.035f,.625f,.43f,20);
            string reason=TroopTraining.Unavailable(i);
            string label=TroopTraining.Due(i)>0?"Training · "+Mathf.Max(0,TroopTraining.Due(i)-TrainingPool.Day(VariableSingleton.GetDate()))+" days left":reason??"Upgrade training\n5,000 gold · 5 days";
            var button=CampaignHUD.MakeButton(row,label,new Vector2(.65f,.025f),new Vector2(.975f,.425f),()=>ReviewTraining(unit));
            button.name="Review training "+unit;button.interactable=reason==null;
        }
        var specialists=CompactCard(pane,"Specialist equipment",.075f,.215f);
        CompactText(specialists,"SPECIALIST EQUIPMENT",.025f,.70f,.97f,.97f,23);
        string special="Varangian Drills: "+(TechnologySystem.Has("VarangianDrills")?"+20% lethality":"not researched")
            +"\nGrapeshot: "+(TechnologySystem.Has("Grapeshot")?"cannon lethality ×3.5":"not researched")
            +"\nArtillery Ships: "+(TechnologySystem.Has("ArtilleryShips")?"8":"4")+" casualties per ship in harbour reports";
        CompactText(specialists,special,.025f,.04f,.65f,.73f,20);
        CampaignHUD.MakeButton(specialists,"Technologies",new Vector2(.67f,.12f),new Vector2(.975f,.62f),Technologies);
        CampaignHUD.MakeButton(pane,"Return",new Vector2(.66f,.015f),new Vector2(.975f,.065f),()=>ControlSingleton.Instance.GoBack());
    }
    void CompactFleet()
    {
        var pane=MilitaryCanvas("The Imperial Fleet",SceneBackdrop.Phase("fleet"),"");
        screen.transform.Find("Scene heading").gameObject.SetActive(false);
        CompactText(pane,"The Imperial Fleet",.04f,.94f,.96f,.995f,28).alignment=TextAlignmentOptions.Center;
        FleetChainBackdrop.Attach(screen);
        int ships=CouncilActions.Int("iShips"),order=Mathf.Clamp(CouncilActions.Int("iFleet"),0,5);
        bool known=CouncilActions.Int("iSpies")>=2;int queued=0;
        var building=new List<string>();
        foreach(var o in TrainingPool.Orders)if(o.unit=="iShips")
        {
            if(o.kind!=TrainingPool.ShipRepair)queued+=o.quantity;
            building.Add(o.quantity+" warships · "+(o.kind==TrainingPool.ShipRepair?"repairing":"building")+" · "+TrainingPool.ReadyDate(o).ToString("d MMM")+" ("+TrainingPool.Remaining(o)+" days)");
        }
        var strength=CompactCard(pane,"Naval balance",.82f,.935f);
        FleetText(strength,"IMPERIAL",.02f,.72f,.31f,.96f,21);
        FleetText(strength,ships.ToString("N0"),.02f,.25f,.31f,.73f,40);
        FleetText(strength,"warships",.02f,.04f,.31f,.27f,19);
        FleetText(strength,"CONTROL OF THE SEA",.32f,.72f,.68f,.96f,21);
        FleetText(strength,NavalState.SeaControl,.32f,.25f,.68f,.73f,27);
        FleetText(strength,"Outnumber their fleet to secure the sea",.32f,.02f,.68f,.29f,18);
        FleetText(strength,"OTTOMAN",.69f,.72f,.98f,.96f,21);
        FleetText(strength,known?NavalState.EnemyShips.ToString("N0"):"Unknown",.69f,.25f,.98f,.73f,40);
        FleetText(strength,known?"Shipyards supply fortnightly":"Requires 2 camp agents",.69f,.02f,.98f,.29f,18);
        string[] orders=FleetMissionRules.Names,effects=FleetMissionRules.Descriptions;
        var mission=CompactCard(pane,"Current mission",.585f,.81f);
        CompactText(mission,orders[order],.025f,.76f,.975f,.97f,27);
        CompactText(mission,ships==0?"No ships available. Construction takes 30 days.":effects[order],.025f,.56f,.975f,.77f,22);
        CompactText(mission,order==3?"Participates in wall defence · No routine reports":order==2&&!VariableSingleton.GetBoolVariable("bStraitsOpen")?"Grain mission suspended: Bosphorus closed.":"Report in "+FleetOperations.DaysRemaining+" active siege days · 14–21-day cycle.",.025f,.22f,.63f,.55f,20);
        CampaignHUD.MakeButton(mission,"Change mission",new Vector2(.65f,.30f),new Vector2(.975f,.55f),FleetOrders);
        var harbour=CompactCard(pane,"Harbour and command",.355f,.575f);
        CompactText(harbour,"HARBOUR & COMMAND",.025f,.78f,.975f,.98f,24);
        CompactText(harbour,"Great Chain: "+(VariableSingleton.GetBoolVariable("bGoldenHornChain")?"intact":"removed")+" · Artillery Ships: "+(TechnologySystem.Has("ArtilleryShips")?"active":"not researched"),.025f,.57f,.975f,.80f,21);
        float bonus=ships>0&&order==3?CourtRoster.Bonus(1,.05f,.15f)*100:0;
        CompactText(harbour,"Alvise Diedo: "+CourtRoster.Status(1),.025f,.36f,.975f,.58f,21);
        CompactText(harbour,"Archer support +"+bonus.ToString("0")+"% while defending the Golden Horn.\nHarbour reports: 4 casualties per ship; Artillery Ships doubles this.",.025f,.035f,.975f,.37f,20);
        var report=CompactCard(pane,"Latest naval battle",.235f,.345f);
        CompactText(report,CouncilActions.Int("iNavalBattles")>0?"LAST BATTLE\nShips lost: ours "+CouncilActions.Int("iNavalOurLoss")+" · Ottoman "+CouncilActions.Int("iNavalEnemyLoss"):"LAST BATTLE\nNo naval battle has been fought.",.025f,.08f,.61f,.93f,22);
        var battle=CampaignHUD.MakeButton(report,"Plan naval battle",new Vector2(.64f,.17f),new Vector2(.975f,.83f),ReviewNavalBattle);battle.interactable=ships>0&&NavalState.EnemyShips>0;
        var yard=CompactCard(pane,"Shipyards",.08f,.225f);
        CompactText(yard,"SHIPYARDS · "+queued+" building · "+SpecialistPersonnel.RepairingShips+" repairing",.025f,.72f,.975f,.98f,23);
        CompactText(yard,building.Count==0?"No orders in progress.":building[0]+(building.Count>1?" · +"+(building.Count-1)+" orders":""),.025f,.44f,.975f,.72f,20);
        CampaignHUD.MakeButton(yard,"All shipyard orders",new Vector2(.025f,.04f),new Vector2(.47f,.42f),FleetShipyardOrders);
        CampaignHUD.MakeButton(yard,"Build warships",new Vector2(.50f,.04f),new Vector2(.975f,.42f),()=>Shipyard());
        CampaignHUD.MakeButton(pane,"Repair ships · "+SpecialistPersonnel.AvailableDamagedShips+" available",new Vector2(.025f,.015f),new Vector2(.63f,.067f),RepairDamagedShips);
        CampaignHUD.MakeButton(pane,"Return",new Vector2(.66f,.015f),new Vector2(.975f,.067f),()=>ControlSingleton.Instance.GoBack());
    }
    void FleetShipyardOrders()=>Shipyard();
}
