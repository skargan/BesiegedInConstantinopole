using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// Calendar and one-time event state use the existing campaign dictionaries/save service.
public static class PreparationCampaign
{
    public const int SiegeTurn=16;
    public static readonly string[] Nodes={"story2","story8","story10","story11","story6","story15","story19","story18","story14","prepIntelligence","prepNaval","prepGrain","story16","story17","prepScorched","story13","story12","story9","prepAdventurers","story7","story20"};
    public static readonly string[] Titles={"Walls","Call on a Major Christian Power","Recruitment","Morean Orders","Provisions Planning","Galata","The Great Chain of the Golden Horn","Orders for the Fleet","Commander","Intelligence","Naval Warfare","Overseas Grain Missions","Assault on the Straitcutter Fortress","Greek Fire","Scorched Earth","Our Turks","Should We Trust the Venetians?","Mind over Matter","Call on Adventurers","Lord of the Imperial Wardrobe","Fate of Stadia and Therapia"};
    public static readonly string[] Summaries={"Strengthen the City's walls.","Ask foreign powers for aid.","Raise troops for the defence.","Decide what the Morea should contribute.","Build a reserve before prices rise.","Support, neutrality, gifts or control of Galata.","Keep, sell or repurpose the harbour chain.","Prepare the fleet's first mission; routine orders remain in the Fleet menu.","Appoint an available commander and gain 10 army morale points.","Plant agents in the approaching Ottoman army.","Invest in ships, recovery, stores, a hidden anchorage or pilots.","Dispatch merchant agents; grain arrives later.","Act against the fortress on the Bosphorus.","Seek the lost incendiary weapon.","Deny the advancing army food, routes and materiel.","Decide the place of Turkish households and soldiers in our service.","Hear Captain Diedo and weigh Venetian service against old wounds.","Receive Johannes Grant and his proposal for an engineering workshop.","Invite one traveller, inventor or dispossessed prince to your court.","Hear Sphrantzes and decide the fate of the imperial treasures.","Withdraw, sacrifice or reinforce the outpost garrisons."};
    // Provisional unlock counts use the three implemented career achievements, not event flags.
    public static readonly int[] UnlockCounts={0,0,0,0,0,0,0,0,0,0,0,1,2,3,2,0,0,0,0,0,0};
    static int I(string key)=>VariableSingleton.GetIntVariable(key);
    static void Add(string key,int value)=>VariableSingleton.ChangeInt(key,value);
    static bool B(string key)=>VariableSingleton.GetBoolVariable(key);
    public static bool Active=>I("iPreparationVersion")==2;
    public static int CareerCount=>Enumerable.Range(0,Achievements.Names.Length).Count(i=>AchievementPlatform.Earned(i));
    public static bool Flexible(int turn)=>new[]{1,2,4,6,7,8,9,10,13,14}.Contains(turn);
    public static DateTime Date(int turn)=>turn<12?new DateTime(1452,turn+1,turn==11?12:1):turn==12?new DateTime(1453,1,1):turn==13?new DateTime(1453,2,1):turn==14?new DateTime(1453,3,1):turn==15?new DateTime(1453,4,1):new DateTime(1453,4,6).AddDays(turn-SiegeTurn);
    public static string FixedNode(int turn)
    {
        switch(turn){case 0:return "story0";case 3:return "prepFortress";case 5:return "story3";case 11:return "story1";case 12:return "story5";case 15:return "story22";case 16:return "story23";default:return null;}
    }
    public static string Unavailable(int index,int achievements=-1)
    {
        if(index<0||index>=Nodes.Length)return "Unknown preparation";
        if(!Active||VariableSingleton.HasSiegeStarted()||!Flexible(VariableSingleton.Turn))return "Not a selectable preparation month";
        if(I("iPrepDone"+index)>0)return "Already completed";
        int count=achievements<0?CareerCount:achievements;
        if(count<UnlockCounts[index])return "Requires "+UnlockCounts[index]+" career achievements (earned: "+count+")";
        if(Nodes[index]=="story15"&&GalataState!=0)return "Galata is no longer neutral";
        if(Nodes[index]=="story16"&&VariableSingleton.GetDate()<new DateTime(1452,9,1))return "Available from September 1452";
        if(Nodes[index]=="story18"&&I("iShips")==0)return "Requires a fleet";
        return null;
    }
    public static string Followup(string node)=>Active&&node=="story5"?"story4":null;
    public static void Complete(string node)
    {
        int index=Array.IndexOf(Nodes,node);
        if(index>=0&&I("iPrepDone"+index)==0){VariableSingleton.SetInt("iPrepDone"+index,1);if(node=="story14" && I("iCommander")>=0)VariableSingleton.ChangePercentFloat("fTroopRep",.10f);}
        if(node.StartsWith("galata")&&node.Length==8&&int.TryParse(node.Substring(6),out int id)&&id>=26&&id<=32)VariableSingleton.SetInt("iGalataEvent"+id,1);
    }
    public static void InitializeDefaults(bool legacy=false)
    {
        VariableSingleton.SetInt("iPreparationVersion",legacy?0:2);
        CharacterPreparations.Initialize();
        NavalState.Initialise();
        foreach(string key in new[]{"iGalataSupportRequested","iGalataNeutralPledge","iRumeliRecon","iGalataDebt","iGalataGovernor","iDeposed","iGalataState","iGalataPromise","iGalataGovernance","iGalataClaims","iArrivalAlert","iFleetPrepared","iNavalRepair","iNavalStores","iNavalBase","iNavalPilots","iDamagedShips","iGrainDue","iGrainCargo","iGiovanniWarning","iGiovanniCrisis","iOttomanResolvedTurn"})VariableSingleton.SetInt(key,0);
        VariableSingleton.SetInt("iOttomanResolvedTurn",-1);
        VariableSingleton.SetInt("iGalataDefences",100);VariableSingleton.SetInt("iArmySupport",50);
        for(int i=0;i<Nodes.Length;i++)VariableSingleton.SetInt("iPrepDone"+i,0);
        for(int i=0;i<CourtRoster.Keys.Length;i++)VariableSingleton.SetInt("iLoyalty"+i,50);
        for(int i=26;i<=32;i++)VariableSingleton.SetInt("iGalataEvent"+i,0);
    }
    public static string Advance()
    {
        string characterArrival=CharacterPreparations.Advance();
        if(VariableSingleton.HasSiegeStarted()&&I("iArrivalAlert")>0){VariableSingleton.SetInt("iCampAlert",Mathf.Min(100,EspionageSystem.Alert+I("iArrivalAlert")));VariableSingleton.SetInt("iArrivalAlert",0);}
        if(I("iGrainDue")>0&&TrainingPool.Day(VariableSingleton.GetDate())>=I("iGrainDue")){int cargo=I("iGrainCargo");Add("iProvisions",cargo);VariableSingleton.SetInt("iGrainDue",0);VariableSingleton.SetInt("iGrainCargo",0);return characterArrival+"Overseas grain mission arrives: +"+cargo+" provisions.\n\n";}
        return characterArrival;
    }
    [YarnCommand("PrepIntelligence")] public static void Intelligence(int option)
    {
        int[] costs={0,2000,5000,3000};if(option<0||option>3||I("iMoney")<costs[option]||(option==3&&I("iFocus")!=4))throw new InvalidOperationException("Intelligence requirements not met");
        Add("iMoney",-costs[option]);Add("iSpies",option+1);
        if(option==0){Add("iEnemyDelay",2);Add("iArrivalAlert",25);}
        if(option==1)Add("iSecrets",1);
    }
    [YarnCommand("PrepScorched")] public static void Scorched(int option)
    {
        int[] costs={1000,3000,6000,4000,3000};if(option<0||option>4||I("iMoney")<costs[option]||(option==3&&I("iFocus")!=0)||(option==4&&(I("iFocus")!=1||I("iCavalry")<50||I("iBalkans")<500)))throw new InvalidOperationException("Scorched-earth requirements not met");
        Add("iMoney",-costs[option]);
        if(option<3){int[] delays={2,4,6},food={3,7,12},losses={50,150,300};Add("iEnemyDelay",delays[option]);Add("iFood",-food[option]);Add("iBazouks",-losses[option]);VariableSingleton.ChangeFloat("fPeopleRep",-(option+1)*.03f);}
        if(option==3){Add("iEnemyDelay",4);Add("iFood",-5);Add("iTurkCannons",-Mathf.Min(2,I("iTurkCannons")));}
        if(option==4){Add("iBalkans",-500);Add("iCavalry",50);Add("iArchers",150);Add("iInfantry",300);}
    }
    public static int GalataState=>I("iGalataState");
    public static readonly string[] GalataStates={"Neutral Galata","Direct imperial control","Giovanni's autonomous rule","Turkish control"};
    [YarnCommand("GalataState")] public static void SetGalataState(int state){VariableSingleton.SetInt("iGalataState",Mathf.Clamp(state,0,3));VariableSingleton.ChangeBool("bGalataIndependence",state==0);}
    [YarnCommand("CharacterLoyalty")] public static void Loyalty(int character,int delta)=>VariableSingleton.SetInt("iLoyalty"+character,Mathf.Clamp(I("iLoyalty"+character)+delta,0,100));
    [YarnCommand("CaptureGalata")] public static void Capture(string route)
    {
        if(GalataState!=0)throw new InvalidOperationException("Galata is not neutral");
        if(route!="giovanni"&&route!="varangians")throw new InvalidOperationException("Unknown capture route");
        if(route=="giovanni"){if(!B("bCharWarrior0")||I("iFocus")!=4)throw new InvalidOperationException("Giovanni and Diplomacy focus required");Loyalty(0,-10);}
        SetGalataState(1);VariableSingleton.SetInt("iGalataGovernance",1);
    }
    [YarnCommand("GrantGalata")] public static void Grant(string choice)
    {
        if(I("iGalataGovernance")!=1||GalataState!=1)throw new InvalidOperationException("Confirmed capture with governance pending required");
        if((choice=="now"||choice=="later"||choice=="governor")&&!B("bCharWarrior0"))throw new InvalidOperationException("Giovanni is not available");
        if(!new[]{"now","later","governor","imperial","neutral"}.Contains(choice))throw new InvalidOperationException("Unknown grant");
        if(choice=="now"){SetGalataState(2);Loyalty(0,30);Add("iGalataDefences",25);}
        if(choice=="later"){VariableSingleton.SetInt("iGalataPromise",1);Loyalty(0,15);}
        if(choice=="governor")VariableSingleton.SetInt("iGalataGovernor",1);
        if(choice=="neutral")SetGalataState(0);
        VariableSingleton.SetInt("iGalataGovernance",0);
    }
    public static void HonourVictory(){if(I("iGalataPromise")==1&&GalataState==1&&B("bCharWarrior0")){SetGalataState(2);VariableSingleton.SetInt("iGalataPromise",2);}}
    public static bool CanAttackGalata=>VariableSingleton.HasSiegeStarted()&&(GalataState==1||GalataState==2)&&I("iEnemyDelay")==0;
    public static string AttackGalata()
    {
        if(!CanAttackGalata)return null;
        int damage=UnityEngine.Random.Range(10,21);Add("iGalataDefences",-damage);
        if(I("iGalataDefences")==0){SetGalataState(3);return "The Ottomans breach Galata's defences and take control of the district.";}
        return "The Ottomans attack Galata. Defences −"+damage+"; "+I("iGalataDefences")+" remain.";
    }
    public static IEnumerable<string> OptionalNodes()
    {
        if(!VariableSingleton.HasSiegeStarted()||GalataState==3)yield break;
        int goodwill=Mathf.RoundToInt(VariableSingleton.GetFloatVariable("fGalataRep")*100);
        int[] thresholds={100,70,60,50,40,30,0};
        for(int i=0;i<7;i++){int id=26+i;if(I("iGalataEvent"+id)>0)continue;if(id==26&&GalataState!=0)continue;if(i<4?goodwill>=thresholds[i]:goodwill<=thresholds[i])yield return "galata"+id;}
        if(B("bCharWarrior0")&&GalataState==2&&I("iLoyalty0")<=30&&I("iGiovanniWarning")==0)yield return "giovanniWarning";
        if(B("bCharWarrior0")&&GalataState==2&&I("iLoyalty0")<=20&&I("iGiovanniWarning")>0&&I("iGiovanniCrisis")==0)yield return "giovanniThrone";
    }
    public static bool ChangeFleet(int mission,out string report)
    {
        report="A fleet and an available route are required.";if(mission<0||mission>4||I("iShips")<=0||(mission==2&&!B("bStraitsOpen")))return false;
        if(I("iFleet")==mission){report="The fleet already has those orders.";return false;}
        VariableSingleton.SetInt("iFleet",mission);FleetOperations.OrdersChanged(mission);VariableSingleton.SetInt("iFleetPrepared",0);report="The whole fleet receives its new mission. This uses the major action; no preparation bonus is renewed.";return true;
    }
    [YarnCommand("PrepFleet")] public static void PrepareFleet(int mission){if(mission<0||mission>4||I("iShips")<=0||(mission==2&&!B("bStraitsOpen")))throw new InvalidOperationException("Fleet mission unavailable");VariableSingleton.SetInt("iFleet",mission);FleetOperations.OrdersChanged(mission);VariableSingleton.SetInt("iFleetPrepared",1);}
    [YarnCommand("FleetRetreat")] public static void Retreat(){int sailed=I("iShips"),survived=(sailed+1)/2;VariableSingleton.SetInt("iShips",survived);Add("iDamagedShips",(sailed-survived)/2);VariableSingleton.SetInt("iFleet",3);VariableSingleton.SetInt("iFleetPrepared",0);}
    [YarnCommand("PrepGrain")] public static void Grain(int route)
    {
        int[] costs={4000,6000,3000},cargo={3000,5000,2500},days={30,45,21};if(route<0||route>2||I("iMoney")<costs[route]||(route==1&&!B("bStraitsOpen"))||(route==2&&GalataState==3))throw new InvalidOperationException("Grain mission unavailable");
        Add("iMoney",-costs[route]);VariableSingleton.SetInt("iGrainCargo",cargo[route]);VariableSingleton.SetInt("iGrainDue",TrainingPool.Day(VariableSingleton.GetDate())+days[route]);
    }
    [YarnCommand("PrepNaval")] public static void Naval(int option)
    {
        int[] costs={6000,4000,3000,4500,2500};if(option<0||option>4||I("iMoney")<costs[option]||(option==1&&I("iMaterials")<30)||(option==2&&I("iProvisions")<500)||(option==3&&I("iSpies")<1))throw new InvalidOperationException("Naval investment requirements not met");
        Add("iMoney",-costs[option]);
        if(option==0)Add("iShips",3);
        if(option==1){Add("iMaterials",-30);VariableSingleton.SetInt("iNavalRepair",2);}
        if(option==2){Add("iProvisions",-500);VariableSingleton.SetInt("iNavalStores",3);}
        if(option==3){Add("iSpies",-1);VariableSingleton.SetInt("iNavalBase",1);}
        if(option==4)VariableSingleton.SetInt("iNavalPilots",2);
    }
    [YarnFunction("RumeliChance")] public static int RumeliChance()=>45+(I("iRumeliRecon")>0?30:0);
    [YarnFunction("RumeliReconLabel")] public static string RumeliReconLabel()=>I("iRumeliRecon")>0?"<color=#65EF83>Intelligence acquired · +30 percentage points (75% success)</color>":"45% success · reconnaissance would add 30 percentage points";
    [YarnCommand("RumeliRecon")] public static void Recon(string route,bool success)
    {
        string resource=route=="varangians"?"iVarangians":"iSpies";int required=route=="varangians"?5:1;
        if((route!="varangians"&&route!="spy")||I(resource)<required)throw new InvalidOperationException("Reconnaissance party unavailable");
        if(success){Add("iSecrets",1);VariableSingleton.SetInt("iRumeliRecon",1);}else Add(resource,-required);
    }
    [YarnCommand("GalataLeak")] public static void Leak(){VariableSingleton.ChangeFloat("fOuterWall",-.05f);VariableSingleton.SetInt("iCampAlert",Mathf.Min(100,EspionageSystem.Alert+10));}
    public static float FleetMultiplier(int mission,bool consume)
    {
        float bonus=I("iFleetPrepared")>0?.5f:0;
        if(I("iNavalStores")>0)bonus+=.25f;
        bool pilots=(mission==2||mission==4)&&I("iNavalPilots")>0;
        bool raid=(mission==0||mission==1)&&I("iNavalBase")>0;
        if(pilots)bonus+=.25f;if(raid)bonus+=.5f;
        if(consume){VariableSingleton.SetInt("iFleetPrepared",0);if(I("iNavalStores")>0)Add("iNavalStores",-1);if(pilots)Add("iNavalPilots",-1);if(raid&&UnityEngine.Random.Range(0,100)<20)VariableSingleton.SetInt("iNavalBase",0);
            if(I("iNavalRepair")>0&&I("iDamagedShips")>0){int ships=Mathf.Min(2,I("iDamagedShips"));Add("iShips",ships);Add("iDamagedShips",-ships);Add("iNavalRepair",-1);}}
        return 1+bonus;
    }
    public static string GalataActionUnavailable(int action) {
        if(GalataState==3)return "Under Ottoman control";
        if((action==0||action==8||action==9)&&GalataState!=0)return "Requires neutral Galata";
        if(action==8&&DailyLifeEvents.I("iGalataSupportRequested")>0)return "Support already received";
        if(action==9&&DailyLifeEvents.I("iGalataNeutralPledge")>0)return "Neutrality already pledged";
        if(action>=2&&action<=5&&I("iGalataClaims")<1)return "Requires a diplomatic claim";
        if(action==7&&GalataState!=2)return "Requires Giovanni's autonomy";
        if(action==6&&!B("bCharWarrior0"))return "Giovanni unavailable";
        int cost=action==0?7500:action==1?2000:action==6?3000:0;
        return I("iMoney")<cost?"Requires "+cost.ToString("N0")+" gold":null;
    }
    public static bool DiplomaticAction(int action,out string report)
    {
        report=GalataActionUnavailable(action);if(report!=null)return false;report="";
        if(action==0){if(I("iMoney")<7500||GalataState!=0)return false;Add("iMoney",-7500);VariableSingleton.ChangeFloat("fGalataRep",.3f);report="The podesta receives 7,500 gold. Galata goodwill +30; control is unchanged.";}
        else if(action==1){if(I("iMoney")<2000)return false;Add("iMoney",-2000);int goodwill=I("iFocus")==3?30:10;VariableSingleton.ChangeFloat("fGalataRep",goodwill/100f);report="Gifts and negotiations: −2,000 gold; +"+goodwill+" goodwill.";}
        else if(action>=2&&action<=5){if(I("iGalataClaims")<1)return false;Add("iGalataClaims",-1);if(action==2){Add("iProvisions",1000);report="One diplomatic claim redeemed for 1,000 provisions.";}else if(action==3){Add("iMoney",5000);Add("iGalataDebt",5000);report="One claim secures a 5,000-gold, ten-year loan. No repayment falls due during this siege.";}else if(action==4){Add("iInfantry",150);Add("iArchers",100);report="One claim brings 150 infantry and 100 archers.";}else{Add("iGalataDefences",10);report="One claim strengthens the harbour anchorage: Galata defences +10.";}}
        else if(action==6){if(!B("bCharWarrior0")||I("iMoney")<3000)return false;Add("iMoney",-3000);Loyalty(0,10);report="Pay Giovanni's men: −3,000 gold; personal loyalty +10.";}
        else if(action==7){if(GalataState!=2)return false;Add("iMoney",3000);Loyalty(0,-20);report="Requisition autonomous Galata's revenues: +3,000 gold; Giovanni loyalty −20. Repeated demands may provoke a political crisis.";}
        else if(action==8){VariableSingleton.SetInt("iGalataSupportRequested",1);VariableSingleton.ChangeFloat("fGalataRep",-.2f);Add("iInfantry",300);Add("iArchers",300);Add("iMoney",4500);Add("iProvisions",500);report="The podestà sends immediate support under protest: 300 infantry, 300 archers, 4,500 gold and 500 provisions. Goodwill −20. Galata remains neutral.";}
        else if(action==9){VariableSingleton.SetInt("iGalataNeutralPledge",1);VariableSingleton.ChangeFloat("fGalataRep",.1f);report="You publicly recognise Galata's friendly neutrality. Goodwill +10. Its gates remain its own.";}
        else return false;return true;
    }
}
