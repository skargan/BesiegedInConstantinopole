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
    
    public string LastReport { get; private set; } = "";
    public int GuardsRequired => assaultTest;
    public bool CanDeployGuards => assaultTest > 0 && VariableSingleton.GetIntVariable("iVarangians") >= assaultTest;
    public bool WasAssault { get; private set; }
    public bool AwaitingBreachDecision { get; private set; }

    public void TakeAIAction()
    {
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

    void RecordEnemyLosses(int enemyLosses) 
    {
        switch (assaultType)
        {
            case AssaultType.Balkan:
                VariableSingleton.ChangeInt("iBalkans", -enemyLosses); break;

            case AssaultType.Bazouk:
                VariableSingleton.ChangeInt("iBazouks", -enemyLosses); break;

            case AssaultType.Janissary:
                VariableSingleton.ChangeInt("iJanissaries", -enemyLosses); break;
        }

        enemyLossDisplay.SetLossesInfo((int)assaultType, enemyLosses);
    }

    public void AIBalkanSubjectAssault()
    {
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
