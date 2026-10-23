using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class AssaultSystem : MonoBehaviour

{

    [Space(3)]

    [SerializeField] int rampAV;

    [SerializeField] int siegeTowerAV;

    [SerializeField] int batteringRamAV;

    [Space(3)]

    [SerializeField] int outerWallBonus;

    [SerializeField] int middleWallBonus;

    [SerializeField] int innerWallBonus;

    [Space(3)]

    [SerializeField] float cannonLethalityFactor;

    [SerializeField] float varangianLethalityFactor;

    [SerializeField] float jannisaryLethalityFacotr;

    [SerializeField] float commonEnemyLethalityFactor;

    [SerializeField] int archerLossWeight;

    [SerializeField] int infantryLossWeight;





    [SerializeField] PlayerLossDisplay playerLossDisplay;



    public float EnemyLethality(bool janissary) => janissary?jannisaryLethalityFacotr:commonEnemyLethalityFactor;
    public float CannonLethality => cannonLethalityFactor * (1+CourtRoster.Bonus(5,.05f,.20f)) * (TechnologySystem.Has("Grapeshot")?3.5f:1);
    public float GuardLethality => varangianLethalityFactor * (TechnologySystem.Has("VarangianDrills")?1.2f:1f);
    public void SetPhasedReport(string stages,string losses){StageReport=stages;LossReport=losses;}
    public string StageReport { get; private set; }="";
    public string LossReport { get; private set; }="";
    public void ClearLossDisplay() {StageReport=LossReport="";playerLossDisplay.SetLossesInfo(0,0,0,0,true);}
    public int AssaultAftermath(bool isJanissaryAssault,int attackerNumbers,bool isWallHeld=true,int VarangiansDeployed=0)
    {
        float moat=VariableSingleton.GetFloatVariable("fMoat")>0?attackerNumbers*.1f:0;
        float ranged=(TechnologySystem.Quality("Archers")+(TechnologySystem.Has("Handcannons")?.5f:0))*(1+(VariableSingleton.GetIntVariable("iFleet")==3&&VariableSingleton.GetIntVariable("iShips")>0?CourtRoster.Bonus(1,.05f,.15f):0));
        float arrows=ranged*VariableSingleton.GetIntVariable("iArchers");
        float fire=TechnologySystem.Has("GreekFire")?attackerNumbers*.05f:0;
        float guns=CannonLethality*VariableSingleton.GetIntVariable("iCannons");
        int reached=Mathf.Clamp(Mathf.RoundToInt(attackerNumbers-moat-arrows-fire-guns),0,attackerNumbers);
        VariableSingleton.ChangeFloat("fMoat",-.1f);
        float exposure=isWallHeld?1-VariableSingleton.Instance.EnduringWall.GetHealth():1;
        float enemyLethality=isJanissaryAssault?jannisaryLethalityFacotr:commonEnemyLethalityFactor;
        float enemyRoll=Random.value*100-50;
        int losses=Mathf.Max(0,Mathf.RoundToInt((reached*enemyLethality+enemyRoll)*exposure));
        int archerLoss=losses*archerLossWeight/(archerLossWeight+infantryLossWeight);
        int infantryLoss=losses*infantryLossWeight/(archerLossWeight+infantryLossWeight);
        float foot=(TechnologySystem.Quality("Infantry")+(TechnologySystem.Has("Grenades")?.25f:0))*(1+CourtRoster.Bonus(0,.05f,.15f));
        float infantryPower=foot*VariableSingleton.GetIntVariable("iInfantry");
        float cavalryPower=TroopTraining.WallCavalry*VariableSingleton.GetIntVariable("iCavalry");
        float guards=GuardLethality*VarangiansDeployed,ourRoll=Random.value*100-50;
        float command=1+CourtRoster.Bonus(3,.05f,.15f);
        int enemyLoss=Mathf.Clamp(Mathf.RoundToInt((attackerNumbers-reached+Mathf.RoundToInt(infantryPower+cavalryPower+guards+ourRoll))*command),0,attackerNumbers);
        int personal=Mathf.Min(EmperorCombat.WallKills,attackerNumbers-enemyLoss);enemyLoss+=personal;
        EmperorCombat.ConsumeWallPresence();
        int infantry=VariableSingleton.GetIntVariable("iInfantry"),archers=VariableSingleton.GetIntVariable("iArchers"),cavalry=VariableSingleton.GetIntVariable("iCavalry");
        archerLoss+=Mathf.Max(0,infantryLoss-infantry);infantryLoss=Mathf.Min(infantryLoss,infantry);
        int cavalryLoss=Mathf.Min(cavalry,Mathf.Max(0,archerLoss-archers));archerLoss=Mathf.Min(archerLoss,archers);
        VariableSingleton.ChangeInt("iInfantry",-infantryLoss);VariableSingleton.ChangeInt("iArchers",-archerLoss);VariableSingleton.ChangeInt("iCavalry",-cavalryLoss);VariableSingleton.ChangeInt("iVarangians",-VarangiansDeployed);
        if(!isWallHeld)VariableSingleton.ChangeFloat("fSiegeRamp",-VariableSingleton.GetFloatVariable("fSiegeRamp"));
        else{VariableSingleton.ChangeBool("bSiegeTower",false);VariableSingleton.ChangeBool("bBatteringRam",false);}
        playerLossDisplay.SetLossesInfo(infantryLoss,archerLoss,cavalryLoss,VarangiansDeployed,isWallHeld);
        StageReport="<b>1 · THE APPROACH</b>  "+attackerNumbers.ToString("N0")+" attackers\n"+
            "Moat: "+moat.ToString("N0")+" · Archers: "+arrows.ToString("N0")+" ("+ranged.ToString("0.##")+" each)\n"+
            "Cannons: "+guns.ToString("N0")+" ("+CannonLethality.ToString("0.##")+" each) · Greek fire: "+fire.ToString("N0")+"\n"+
            "Reached the wall: "+reached.ToString("N0")+"\n\n<b>2 · AT THE WALL</b>\n"+
            "Infantry: "+infantryPower.ToString("N0")+" ("+foot.ToString("0.##")+" each) · Cavalry: "+cavalryPower.ToString("N0")+"\n"+
            "Guard: "+guards.ToString("N0")+" · Fighting variation: "+ourRoll.ToString("+0;-0;0")+" · Command ×"+command.ToString("0.##")+"\n"+
            "Your personal kills: "+personal+"\n\n<b>3 · THE PRICE</b>\n"+
            "Enemy lethality: "+enemyLethality.ToString("0.##")+" × "+reached+" reaching the wall\n"+
            "Variation: "+enemyRoll.ToString("+0;-0;0")+" · Defender exposure: "+(exposure*100).ToString("0")+"%\n"+
            "<i>Contributions are calculated before rounding and caps. Enemy losses cannot exceed the attacking wave; exposure falls as wall integrity rises.</i>";
        LossReport="<b>OUR LOSSES</b>\nInfantry  "+infantryLoss+"\nArchers  "+archerLoss+"\nCavalry  "+cavalryLoss+"\nVarangian Guard  "+VarangiansDeployed+"\n\n<b>ENEMY LOSSES</b>\n"+enemyLoss.ToString("N0")+" of "+attackerNumbers.ToString("N0")+" attackers\n\n"+(isWallHeld?"The defenders held the wall.":"The defenders withdrew.");
        return enemyLoss;
    }

    public int TestAssault(int assaultTypeAddend)

    {

        return assaultTypeAddend + VariableSingleton.GetIntVariable("iSultan") + SiegeEngeneeringAddend()

                            - Mathf.RoundToInt(100 * VariableSingleton.Instance.EnduringWall.GetHealth())

                            - Mathf.RoundToInt(100 * VariableSingleton.GetFloatVariable("fTroopRep")) - WallBonus() - Mathf.RoundToInt(CourtRoster.Bonus(6,2,6)+CourtRoster.Bonus(7,3,10)) - (TechnologySystem.Has("RomanArchitecture")?5:0);                            //May need to add optional value from fleet

    }



    int SiegeEngeneeringAddend() 

    {

        return (VariableSingleton.GetFloatVariable("fSiegeRamp") >= 1 ? rampAV : 0)

                  + (VariableSingleton.GetBoolVariable("bSiegeTower") ? siegeTowerAV : 0 )

                  + (VariableSingleton.GetBoolVariable("bBatteringRam") ? batteringRamAV : 0);

    }  



    int WallBonus() 

    { 

        switch (VariableSingleton.Instance.EnduringWall.GetCodeName()) 

        {

            case "OuterWall":

                return outerWallBonus;



            case "MiddleWall":

                return middleWallBonus;



            case "InnerWall":

                return innerWallBonus;

        }

        Debug.Log("Exception invalid enduringWall = " + VariableSingleton.Instance.EnduringWall.GetCodeName());

        return 0;

    }

}

