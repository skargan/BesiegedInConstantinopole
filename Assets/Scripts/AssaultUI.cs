using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AssaultUI : UIWindowFam
{
    [SerializeField] GameObject managerUI;
    [SerializeField] GameObject wallsHeldMessage;
    [SerializeField] GameObject wallsOverwhelmedChoice;
    [SerializeField] GameObject assaultAftermathMessage;
    [SerializeField] Button procedeToAssaultResult;
    public TMP_Text enemyActionText;
    public bool DeployVarangiansDecision { private get; set; }
    private TMP_Text stages,losses;
    private bool wallHeld;
    private bool finished;
    private System.Collections.Generic.List<TMP_Text> reportFields=new System.Collections.Generic.List<TMP_Text>();
    private void Awake()
    {
        foreach(var button in GetComponentsInChildren<Button>(true))
            if(button==procedeToAssaultResult || button.name=="ButtonToContinue")
            {
                var rect=button.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.30f,.025f);rect.anchorMax=new Vector2(.70f,.13f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            }
        foreach(var text in GetComponentsInChildren<TMP_Text>(true))
        {
            if(text.text.Trim()=="Title") text.text="Ottoman siege activity";
            if(text.text.StartsWith("Description")) reportFields.Add(text);
            if(text.text.Contains("sacrifice")) text.text="Continue to the next day";
        }
    }
    private void BuildBattleReport()
    {
        if(stages!=null)return;
        var panel=CampaignHUD.Panel(assaultAftermathMessage.transform,"Battle stages",new Vector2(.015f,.17f),new Vector2(.985f,.96f));ImperialTheme.Panel(panel,false);
        stages=CampaignHUD.Label(panel.transform,"",new Vector2(.025f,.035f),new Vector2(.70f,.97f),22);stages.enableAutoSizing=true;stages.fontSizeMin=16;stages.fontSizeMax=22;
        losses=CampaignHUD.Label(panel.transform,"",new Vector2(.73f,.06f),new Vector2(.98f,.94f),25);losses.enableAutoSizing=true;losses.fontSizeMin=18;losses.fontSizeMax=25;
    }
    private void RefreshReport()
    {
        foreach(var text in reportFields) text.text="Ottoman action — siege day "+VariableSingleton.SiegeDay+"\n\n"+AI.Instance.LastReport;
        if(enemyActionText!=null) enemyActionText.text=AI.Instance.LastReport;
    }

    public void ContinueToEmperorAction()
    {
        if (finished || AI.Instance.AwaitingBreachDecision) return;
        finished = true;
        VariableSingleton.IncrementTurn();
        wallsHeldMessage.SetActive(false);
        wallsOverwhelmedChoice.SetActive(false);
        assaultAftermathMessage.SetActive(false);
        SwitchPhase(managerUI);
    }

    public void SetAssaultResultButton(bool wallsHeld) { wallHeld = wallsHeld; }
    public void ShowResult()
    {
        procedeToAssaultResult.gameObject.SetActive(false);
        if (!AI.Instance.WasAssault) { ContinueToEmperorAction(); return; }
        if (wallHeld) AssaultAftermathUI();
        else WallsOverwhelmed();
    }
    public void WallsHeld() { AssaultAftermathUI(); }
    public void WallsOverwhelmed()
    {
        if (!AI.Instance.WasAssault) { ContinueToEmperorAction(); return; }
        wallsOverwhelmedChoice.SetActive(true);
        foreach (var button in wallsOverwhelmedChoice.GetComponentsInChildren<Button>(true))
            if (button.name == "ButtonToVarangians") button.interactable = AI.Instance.CanDeployGuards;
        if (enemyActionText != null) enemyActionText.text = AI.Instance.LastReport +
            "\nHold the breach with " + AI.Instance.GuardsRequired + " Varangians, or abandon this wall.";
    }
    public void AssaultAftermathUI()
    {
        if (!AI.Instance.WasAssault) { ContinueToEmperorAction(); return; }
        wallsHeldMessage.SetActive(false);
        wallsOverwhelmedChoice.SetActive(false);
        assaultAftermathMessage.SetActive(true);
        BuildBattleReport();
        var battle=FindObjectOfType<AssaultSystem>();stages.text=battle.StageReport;losses.text=battle.LossReport;
        RefreshReport();
    }
    public void DecideVarangianDeployment() { AI.Instance.BreachingAssaultAftermath(DeployVarangiansDecision); }
    private void OnEnable()
    {
        ImperialTheme.LegacySurface(gameObject);
        foreach(var image in GetComponentsInChildren<Image>(true))if(image.GetComponent<Button>()==null && (image.sprite==null || (image.color.r>image.color.b*1.2f && image.color.g>.15f && image.color.a>.3f)))ImperialTheme.Panel(image.gameObject);
        finished = false;
        wallsHeldMessage.SetActive(false);
        wallsOverwhelmedChoice.SetActive(false);
        assaultAftermathMessage.SetActive(false);
        procedeToAssaultResult.gameObject.SetActive(true);
        AI.Instance.TakeAIAction();
        RefreshReport();
    }
}
