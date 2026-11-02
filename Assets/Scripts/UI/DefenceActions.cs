using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefenseActions : MonoBehaviour
{
    string wallSelected;
    bool abandonable;
    int repairCost;
    int committedTurn = int.MinValue;
    Slider engineerSlider;
    TMP_Text engineerLabel;

    // Keep the scene's serialized references and old preview fields intact.
    [SerializeField] float repairMin;
    [SerializeField] float repairMax;
    [SerializeField] GameObject Options;
    [SerializeField] TMP_Text selectionName;
    [SerializeField] TMP_Text repairCostDisplay;
    [SerializeField] Button abandonButton;
    [SerializeField] Button repairButton;

    bool Committed => committedTurn == VariableSingleton.Turn;

    public void GetSelection(string variableName)
    {
        bool sameWall = wallSelected == variableName;
        int previous = sameWall && engineerSlider != null ? Mathf.RoundToInt(engineerSlider.value) : WallRepairRules.DefaultAssignment;
        if (previous <= 0) previous = WallRepairRules.DefaultAssignment;
        Options.SetActive(false);
        wallSelected = variableName;
        abandonable = variableName == "f" + VariableSingleton.Instance.EnduringWall.GetCodeName();
        selectionName.text = variableName == "fOuterWall" ? "Theodosian Wall" : variableName == "fMiddleWall" ? "Constantinian Wall" : "Severan Wall";
        ImperialTheme.Panel(Options, false);
        WallDefenceLayout.StyleOptions(Options, selectionName, repairCostDisplay, repairButton, abandonButton);
        if (engineerSlider == null)
            engineerSlider = WallDefenceLayout.EngineerSlider(Options.transform, out engineerLabel, value => RefreshPreview());
        WallDefenceLayout.StyleEngineerSlider(engineerSlider);
        var plan = WallRepairRules.Preview(wallSelected, previous);
        engineerSlider.minValue = plan.MaximumEngineers > 0 ? 1 : 0;
        engineerSlider.maxValue = Mathf.Max(1, plan.MaximumEngineers);
        engineerSlider.SetValueWithoutNotify(plan.MaximumEngineers > 0 ? Mathf.Clamp(previous, 1, plan.MaximumEngineers) : 0);
        Options.SetActive(true);
        RefreshPreview();
    }

    void RefreshPreview()
    {
        if (engineerSlider == null) return;
        var plan = WallRepairRules.Preview(wallSelected, Mathf.RoundToInt(engineerSlider.value));
        engineerSlider.interactable = !Committed && plan.MaximumEngineers > 1;
        engineerLabel.text = plan.MaximumEngineers > 0
            ? "Engineers: " + plan.Engineers + "  /  " + plan.MaximumEngineers + " assignable  ·  " + plan.AvailableEngineers + " available"
            : "No repair detail available  ·  " + plan.AvailableEngineers + " engineers";
        repairCost = plan.Gold;
        repairMin = plan.MinimumRestored / 100f;
        repairMax = plan.MaximumRestored / 100f;
        if (plan.CanRepair)
        {
            string bonus = plan.BonusCapped ? " · capped at full integrity" : "";
            string discount = WallRepairRules.LabourDiscount > 0 ? " (Notaras −" + Mathf.RoundToInt(WallRepairRules.LabourDiscount * 100) + "%)" : "";
            repairCostDisplay.text = plan.Gold.ToString("N0") + " gold" + discount + " · " + plan.Materials + " materials\n"
                + "Restore " + plan.MinimumRestored.ToString("0.#") + "–" + plan.MaximumRestored.ToString("0.#") + " points" + bonus + "\n"
                + "Integrity: " + plan.Before.ToString("0.#") + "% → " + (plan.Before + plan.MinimumRestored).ToString("0.#") + "–" + (plan.Before + plan.MaximumRestored).ToString("0.#") + "%\n"
                + "Engineers return " + WallRepairRules.ReturnDate.ToString("d MMM") + "; wounded require treatment.";
        }
        else repairCostDisplay.text = plan.Unavailable ?? "Choose the engineers to assign.";
        repairCostDisplay.fontStyle = FontStyles.Normal;
        repairButton.interactable = !Committed && plan.CanRepair;
        abandonButton.interactable = !Committed && abandonable;
        TurnActionButton.Mark(repairButton, "Repairs take effect before the next attack.\nEngineers return after two calendar days.\nOne task roll: 95% safe · 2% one killed · 3% one wounded.");
    }

    public void RepairSelected()
    {
        if (Committed || engineerSlider == null) return;
        if (!WallRepairRules.TryRepair(wallSelected, Mathf.RoundToInt(engineerSlider.value), out var report)) {
            // A failed preflight spends neither resources nor the daytime action.
            RefreshPreview();
            repairCostDisplay.text = report;
            return;
        }
        committedTurn = VariableSingleton.Turn;
        repairButton.interactable = abandonButton.interactable = engineerSlider.interactable = false;
        ActionResults.Instance.Message = report;
        FindObjectOfType<ManagerUI>(true).CompleteAction();
    }

    public void AbandonSelected()
    {
        if (Committed || !abandonable) return;
        committedTurn = VariableSingleton.Turn;
        repairButton.interactable = abandonButton.interactable = false;
        if (engineerSlider != null) engineerSlider.interactable = false;
        VariableSingleton.Instance.EnduringWall.Abandon();
        ActionResults.Instance.Message = "You withdraw to the next defensive line.";
        FindObjectOfType<ManagerUI>(true).CompleteAction();
    }
}
