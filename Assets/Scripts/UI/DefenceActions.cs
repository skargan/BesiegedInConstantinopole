using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using TMPro;

using UnityEngine.UI;



public class DefenseActions : MonoBehaviour

{

    string wallSelected;

    bool abandonable;

    int repairCost;



    [SerializeField] float repairMin;

    [SerializeField] float repairMax;



    [SerializeField] GameObject Options;

    [SerializeField] TMP_Text selectionName;

    [SerializeField] TMP_Text repairCostDisplay;

    [SerializeField] Button abandonButton;

    [SerializeField] Button repairButton;



    public void GetSelection(string variableName) 

    { 

        Options.SetActive(false);



        wallSelected = variableName;

        abandonable = variableName == "f" + VariableSingleton.Instance.EnduringWall.GetCodeName() ? true : false;

        

        switch (variableName)

        {

            case "fOuterWall":

                selectionName.text = "Theodosian";

                break;



            case "fMiddleWall":

                selectionName.text = "Constantinian";

                break;



            case "fInnerWall":

                selectionName.text = "Severan";

                break;

        }



        selectionName.text += " Wall Actions";



        repairCost = Mathf.RoundToInt((1 - VariableSingleton.GetFloatVariable(variableName)) * 1000 * (1-CourtRoster.Bonus(2,.05f,.15f)));

        float health = VariableSingleton.GetFloatVariable(variableName);

        repairCostDisplay.text = $"{repairCost:N0} gold | +{Mathf.Min(repairMin, 1-health)*100:0.#}-{Mathf.Min(repairMax, 1-health)*100:0.#} percentage points\nIntegrity after repair: {Mathf.Min(1,health+repairMin)*100:0.#}-{Mathf.Min(1,health+repairMax)*100:0.#}% | Uses major action";

        repairCostDisplay.fontStyle |= FontStyles.Italic;

        repairButton.interactable = repairCost <= VariableSingleton.GetIntVariable("iMoney") && 1 > VariableSingleton.GetFloatVariable(variableName);



        abandonButton.interactable = abandonable;



        Options.SetActive(true);

    }





    /// <summary>

    /// Repairs the selected wall using its configured repair range, capped at full integrity.

    /// </summary>

    public void RepairSelected()

    {

        if (string.IsNullOrEmpty(wallSelected) || VariableSingleton.GetIntVariable("iMoney") < repairCost) return;

        if (VariableSingleton.GetFloatVariable(wallSelected) >= 1) return;

        VariableSingleton.ChangeInt("iMoney", -repairCost);

        VariableSingleton.ChangePercentFloat(wallSelected, Random.value * (repairMax - repairMin) + repairMin);

        ActionResults.Instance.Message = "Repairs completed for " + repairCost + " gold. Wall integrity: " + VariableSingleton.GetPercentFloat(wallSelected) + ".";

        FindObjectOfType<ManagerUI>(true).CompleteAction();

    }



    public void AbandonSelected() 

    {

        VariableSingleton.Instance.EnduringWall.Abandon();

        ActionResults.Instance.Message = "You withdraw to the next defensive line.";

        FindObjectOfType<ManagerUI>(true).CompleteAction();



    }

}

