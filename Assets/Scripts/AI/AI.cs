using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AI: MonoBehaviour
{
    private static AI _instance;
    public static AI Instance => _instance;

    [SerializeField] AssaultUI assaultUI;

    OttomanDecision ottomanTactic;

    [SerializeField] GameObject TacticPrepare;
    [SerializeField] GameObject TacticBombardWall;
    [SerializeField] GameObject TacticTestWall;
    [SerializeField] GameObject TacticAttackWall;
    [SerializeField] GameObject TacticSendAssault;
    [SerializeField] GameObject TacticAdvance;

    [SerializeField] AssaultSystem assaultSystem;

    [SerializeField] int WallOverrunThreshold;

    [SerializeField] int christianBalkanSubjectAV;
    [SerializeField] int bashiBazoukAV;
    [SerializeField] int janissaryAV;

    [SerializeField] int averageBalkanSubjectForce;
    [SerializeField] int averageBashiBazoukForce;
    [SerializeField] int averageJanissaryForce;
    //fields for assault numbers

    [SerializeField] EnemyLossDisplay enemyLossDisplay;
    
    public int TroopAssaultRating(int type) => type==0?christianBalkanSubjectAV:type==1?bashiBazoukAV:janissaryAV;
    public int TroopWaveSize(int type) => type==0?averageBalkanSubjectForce:type==1?averageBashiBazoukForce:averageJanissaryForce;
    public float TroopLethality(int type) => assaultSystem.EnemyLethality(type==2);
    public string LastReport { get; private set; } = "";
    public int GuardsRequired => assaultTest;
    public bool CanDeployGuards => assaultTest > 0 && VariableSingleton.GetIntVariable("iVarangians") >= assaultTest;
    public bool WasAssault { get; private set; }
    public bool AwaitingBreachDecision { get; private set; }

    public PhasedAssault PendingPhased { get; private set; }
    public bool HasPendingPhased=>PendingPhased!=null&&!PendingPhased.Committed;
    void BeginPhased(int type) {
        WasAssault=true;AwaitingBreachDecision=false;assaultType=(AssaultType)type;
        string key=type==0?"iBalkans":type==1?"iBazouks":"iJanissaries";
        var b=new PhasedAssault(Random.Range(1,int.MaxValue));
        b.Type=type;b.Attackers=b.EnemyLeft=Mathf.Min(TroopWaveSize(type),VariableSingleton.GetIntVariable(key));
        if(b.Attackers==0){WasAssault=false;PendingPhased=null;LastReport="No troops remain to mount this assault.";return;}
        b.Frontline=DailyLifeEvents.I("iEmperorAtWall")==1;b.PersonalBudget=1+EmperorCombat.Skill/10;
        b.InitialHealth=b.Health=Mathf.RoundToInt(VariableSingleton.GetFloatVariable("fHealth")*100);
        PopulateDisposition(b);
        b.Tower=VariableSingleton.GetBoolVariable("bSiegeTower");b.Ram=VariableSingleton.GetBoolVariable("bBatteringRam");
        PendingPhased=b;LastReport=(type==2?"Janissaries":type==1?"Bashi-bazouks":"Ottoman Balkan troops")+" assemble for an assault: "+b.Attackers.ToString("N0")+" committed attackers. Prepare to command the defence.";
        assaultUI.SetAssaultResultButton(true);
    }
    // Recalculate allocation without creating another encounter or rerolling its random stream.
    void PopulateDisposition(PhasedAssault b) {
        b.InitialInfantry=b.Infantry=DefencePosture.LandInfantry;b.InitialArchers=b.Archers=DefencePosture.LandArchers;
        b.InitialCavalry=b.Cavalry=DefencePosture.Dismounted?CouncilActions.Int("iCavalry"):0;b.InitialGuards=b.Guards=CouncilActions.Int("iVarangians");

        b.Wall=VariableSingleton.Instance.EnduringWall.GetHealth();b.Breach=assaultSystem.TestAssault(TroopAssaultRating(b.Type))-WallOverrunThreshold;
        b.ArcherBudget=b.Archers*(TechnologySystem.Quality("Archers")+(TechnologySystem.Has("Handcannons")?.5f:0))*(1+(CouncilActions.Int("iFleet")==3&&CouncilActions.Int("iShips")>0?CourtRoster.Bonus(1,.05f,.15f):0));
        b.Cannons=CouncilActions.Int("iCannons");b.Ammo=Mathf.Min(CouncilActions.Int("iAmmunition"),b.Cannons>0?3:0);
        b.GunBudget=b.Cannons*assaultSystem.CannonLethality*(b.Ammo/3f);
        b.FootBudget=.45f*b.Infantry*(TechnologySystem.Quality("Infantry")+(TechnologySystem.Has("Grenades")?.25f:0))*(1+CourtRoster.Bonus(0,.05f,.15f))*(1+CourtRoster.Bonus(3,.05f,.15f));
        b.CavalryBudget=b.Cavalry*TroopTraining.WallCavalry;b.GuardPower=assaultSystem.GuardLethality;b.EnemyLethality=TroopLethality(b.Type);
        if(b.Infantry+b.Archers+b.Cavalry==0)b.Breach=Mathf.Max(b.Breach,b.Attackers/20+1);
    }
    public void RefreshDisposition(PhasedAssault b){
        if(b==null||b.Practice||b.Stage!=PhasedAssault.Phase.Approach||b.Shots>0||b.GunResolved||b.CoverResolved)return;
        PopulateDisposition(b);
    }
    public void CommitPhased(PhasedAssault b) {
        if(b==null||(!b.Practice&&b!=PendingPhased)||!b.TryCommit())return;if(b.Practice)return;var r=b.Outcome;
        VariableSingleton.ChangeInt("iInfantry",r.InfantryDelta);VariableSingleton.ChangeInt("iArchers",r.ArcherDelta);
        VariableSingleton.ChangeInt("iCavalry",r.CavalryDelta);VariableSingleton.ChangeInt("iVarangians",r.GuardDelta);
        VariableSingleton.ChangeInt("iAmmunition",-r.AmmoSpent);VariableSingleton.ChangeInt("iMaterials",-r.MaterialsSpent);VariableSingleton.ChangePercentFloat("fHealth",(r.HealthDelta)/100f);
        VariableSingleton.SetInt("iEmperorAtWall",0);
        RecordEnemyLosses(r.EnemyDead,b.Type);VariableSingleton.ChangeFloat("fMoat",-.1f);
        VariableSingleton.ChangeBool("bSiegeTower",r.Tower);VariableSingleton.ChangeBool("bBatteringRam",r.Ram);
        if(!r.Held&&r.Health>0){VariableSingleton.Instance.EnduringWall.Abandon();VariableSingleton.ChangeFloat("fSiegeRamp",-VariableSingleton.GetFloatVariable("fSiegeRamp"));}
        LastReport=r.Summary;AwaitingBreachDecision=false;assaultUI.SetAssaultResultButton(r.Held);
        assaultSystem.SetPhasedReport(r.Ledger,"Enemy dead: "+r.EnemyDead+"\nEnemy survivors: "+r.EnemySurvivors+"\nInfantry lost: "+(-r.InfantryDelta)+"\nArchers lost: "+(-r.ArcherDelta)+"\nCavalry lost: "+(-r.CavalryDelta)+"\nGuards lost: "+(-r.GuardDelta)+"\nAmmunition spent: "+r.AmmoSpent+"\n\n"+r.Summary);
    }

    public void TakeAIAction()
    {
        PendingPhased=null;
        WasAssault = false;
        AwaitingBreachDecision = false;
        LastReport = "No major Ottoman operation was completed today.";
        assaultUI.SetAssaultResultButton(true);
        enemyLossDisplay.SetLossesInfo(0, 0);
        assaultSystem.ClearLossDisplay();
        if (!VariableSingleton.HasSiegeStarted()) { LastReport="The siege has not begun. No Ottoman attack takes place."; return; }
        string supplyReport=CouncilActions.ResolveEnemySupplies();
        if (VariableSingleton.GetIntVariable("iEnemyDelay")>0)
        {
            VariableSingleton.ChangeInt("iEnemyDelay",-1);
            LastReport="Ottoman siege operations are delayed this turn. "+VariableSingleton.GetIntVariable("iEnemyDelay")+" more days of delay remain. Food consumption and naval pressure continue."+(string.IsNullOrEmpty(supplyReport)?"":"\n"+supplyReport);
            return;
        }
        if (!string.IsNullOrEmpty(supplyReport)) { LastReport=supplyReport; return; }
        if(DailyLifeEvents.I("iLifeSurprise")==1)
        {
            VariableSingleton.SetInt("iLifeSurprise",0);
            if(VariableSingleton.GetIntVariable("iBazouks")>0){AIBashiBazoukAssault();return;}
        }
        if(PreparationCampaign.CanAttackGalata&&UnityEngine.Random.Range(0,100)<15){LastReport=PreparationCampaign.AttackGalata();return;}
        float moatRemaining = VariableSingleton.GetFloatVariable("fMoat");
        float wallHealth = VariableSingleton.Instance.EnduringWall.GetHealth();     //needs function identifying current wall
        float sultanRage = VariableSingleton.GetFloatVariable("fSultanRage");    //needs update
        if (moatRemaining > 0) 
        {
            Debug.Log("Enemy chose preparatory tactic");
            ottomanTactic = Instantiate(TacticPrepare, transform).GetComponent<OttomanDecision>();
        }
        else switch (wallHealth) 
        {
            case > 0.75f:
                Debug.Log("Enemy chose bombardment tactic");
                ottomanTactic = Instantiate(TacticBombardWall, transform).GetComponent<OttomanDecision>();
                break;

            case > 0.5f:
                Debug.Log("Enemy chose test wall tactic");
                ottomanTactic = Instantiate(TacticTestWall, transform).GetComponent<OttomanDecision>();
                break;

            case > 0.25f:
                Debug.Log("Enemy chose attack wall tactic");
                ottomanTactic = Instantiate(TacticAttackWall, transform).GetComponent<OttomanDecision>();
                break;

            case > 0:
                Debug.Log("Enemy chose send assault tactic");
                ottomanTactic = Instantiate(TacticSendAssault, transform).GetComponent<OttomanDecision>();
                break;

            default:
                Debug.Log("Enemy chose advance tactic");
                ottomanTactic = Instantiate(TacticAdvance, transform).GetComponent<OttomanDecision>();
                break;

        }

        ottomanTactic.EliminateRedundant();
        ottomanTactic.ExecuteRandomAction();

        Destroy(ottomanTactic.gameObject);
    }

    /****************
     * SIEGE OPTIONS*/

    public void AIFillMoat()
    {
        Debug.Log("*Enemy is filling the moat!");
        VariableSingleton.ChangeFloat("fMoat", -0.2f, -0.09f);
        LastReport = "Ottoman labourers fill the moat, reducing its protection.";
    }

    public void AIBuildTower()
    {
        Debug.Log("*Enemy is building a tower!");
        VariableSingleton.ChangeBool("bSiegeTower", true);
        LastReport = "The Ottomans have built a siege tower.";
    }

    public void AIBuildRam()
    {
        Debug.Log("*Enemy is building a ram!");
        VariableSingleton.ChangeBool("bBatteringRam", true);
        LastReport = "The Ottomans have built a battering ram.";
    }

    public void AIBuildRamp()
    {
        Debug.Log("*Enemy is building a ramp!");
        VariableSingleton.ChangePercentFloat("fSiegeRamp", Random.Range(0.1f, 0.2f));
        LastReport = "The Ottoman siege ramp grows closer to the wall.";
    }

    public void AIMoarCannons()
    {
        Debug.Log("*Enemy is bringing more cannons!");
        VariableSingleton.ChangeInt("iTurkCannons", 1);
        LastReport = "Another Ottoman cannon has reached the siege lines.";
    }


    /******************
     * ASSAULT OPTIONS*/

    public void AIBombard()
    {
        // Mechanics document: each Ottoman cannon removes one percentage point.
        int cannons = VariableSingleton.GetIntVariable("iTurkCannons");
        VariableSingleton.ChangeFloat("f" + VariableSingleton.Instance.EnduringWall.GetCodeName(), -cannons / 100f);
        LastReport = "Ottoman bombardment damages the current wall by " + cannons + "%.";
    }

    enum AssaultType 
    { 
        Balkan,
        Bazouk,
        Janissary
    }
    //there could be a custom object to contain these for turns where it's necessary
    int assaultTest;            //equal to required varangian n
    AssaultType assaultType;
    int attackerNumbers;

    public void BreachingAssaultAftermath(bool deployVarangians) 
    {
        if (!AwaitingBreachDecision) return;
        if (deployVarangians && !CanDeployGuards) return;
        AwaitingBreachDecision = false;
        bool held = deployVarangians;
        int guards = held ? assaultTest : 0;
        int enemyLosses = assaultSystem.AssaultAftermath(assaultType == AssaultType.Janissary, attackerNumbers, held, guards);
        RecordEnemyLosses(enemyLosses);
        if (!held) VariableSingleton.Instance.EnduringWall.Abandon();
        LastReport = held ? "The Varangian Guard holds the breach at a terrible cost." : "Your defenders fall back from the breached wall.";
        assaultUI.AssaultAftermathUI();
    }

    void RecordEnemyLosses(int enemyLosses,int type=-1)
    {
        var resolvedType=type>=0?(AssaultType)type:assaultType;
        switch (resolvedType)
        {
            case AssaultType.Balkan:
                VariableSingleton.ChangeInt("iBalkans", -enemyLosses); break;

            case AssaultType.Bazouk:
                VariableSingleton.ChangeInt("iBazouks", -enemyLosses); break;

            case AssaultType.Janissary:
                VariableSingleton.ChangeInt("iJanissaries", -enemyLosses); break;
        }

        enemyLossDisplay.SetLossesInfo((int)resolvedType, enemyLosses);
    }

    public void AIBalkanSubjectAssault()
    {
        if(PhasedAssaultUI.Enabled){BeginPhased(0);return;}
        WasAssault = true;
        assaultType = AssaultType.Balkan;
        LastReport = "Ottoman Balkan troops launch an assault.";
        Debug.Log("*Enemy launched Balkan Subject Assault");
        int balkanSubjectNumbers = VariableSingleton.GetIntVariable("iBalkans");
        assaultTest = assaultSystem.TestAssault(christianBalkanSubjectAV) - WallOverrunThreshold;
        if (assaultTest > 0) 
        {
            AwaitingBreachDecision = true;
            WasAssault = true;
        assaultType = AssaultType.Balkan;
            attackerNumbers = averageBalkanSubjectForce < balkanSubjectNumbers ? averageBalkanSubjectForce : balkanSubjectNumbers;
            assaultUI.SetAssaultResultButton(false);
        }
        else
        {
            int enemyLosses = assaultSystem.AssaultAftermath(false, averageBalkanSubjectForce < balkanSubjectNumbers ? averageBalkanSubjectForce : balkanSubjectNumbers);
            RecordEnemyLosses(enemyLosses);
            assaultUI.SetAssaultResultButton(true);
        }
    }

    public void AIBashiBazoukAssault()
    {
        if(PhasedAssaultUI.Enabled){BeginPhased(1);return;}
        WasAssault = true;
        assaultType = AssaultType.Bazouk;
        LastReport = "Ottoman Bazouk troops launch an assault.";
        Debug.Log("*Enemy launched Bashi Bazouk Assault");
        int bashiBazoukNumbers = VariableSingleton.GetIntVariable("iBazouks");
        assaultTest = assaultSystem.TestAssault(bashiBazoukAV) - WallOverrunThreshold;
        if (assaultTest > 0) 
        {
            AwaitingBreachDecision = true;
            WasAssault = true;
        assaultType = AssaultType.Bazouk;
            attackerNumbers = averageBashiBazoukForce < bashiBazoukNumbers ? averageBashiBazoukForce : bashiBazoukNumbers;
            assaultUI.SetAssaultResultButton(false);
        }
        else
        {
            int enemyLosses = assaultSystem.AssaultAftermath(false, averageBashiBazoukForce < bashiBazoukNumbers ? averageBashiBazoukForce : bashiBazoukNumbers);
            RecordEnemyLosses(enemyLosses);
            assaultUI.SetAssaultResultButton(true);
        }
    }

    public void AIJanissaryAssault()
    {
        if(PhasedAssaultUI.Enabled){BeginPhased(2);return;}
        WasAssault = true;
        assaultType = AssaultType.Janissary;
        LastReport = "Ottoman Janissary troops launch an assault.";
        Debug.Log("*Enemy launched Janissary Assault");
        int janissaryNumbers = VariableSingleton.GetIntVariable("iJanissaries");
        assaultTest = assaultSystem.TestAssault(janissaryAV) - WallOverrunThreshold;
        if (assaultTest > 0)
        {
            AwaitingBreachDecision = true;
            WasAssault = true;
        assaultType = AssaultType.Janissary;
            attackerNumbers = averageJanissaryForce < janissaryNumbers ? averageJanissaryForce : janissaryNumbers;
            assaultUI.SetAssaultResultButton(false);
        }
        else
        {
            int enemyLosses = assaultSystem.AssaultAftermath(true, averageJanissaryForce < janissaryNumbers ? averageJanissaryForce : janissaryNumbers);
            RecordEnemyLosses(enemyLosses);
            assaultUI.SetAssaultResultButton(true);
        }
    }

    /*********
     * UNITY */

    private void Awake()
    {
        _instance = this;
    }
}
