using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PercentConscriptedUpdater : MonoBehaviour
{
    [SerializeField]TMP_Text targetText;

    private void Update()
    {
        float fractionConscripted = (float)((VariableSingleton.GetIntVariable("iInfantry") + VariableSingleton.GetIntVariable("iArchers")
                                    + VariableSingleton.GetIntVariable("iCavalry"))) / Mathf.Max(1f, (float)PopulationAccounting.People);

        float percentageToThreeDecimals = Mathf.Round(fractionConscripted * 100000) / 1000;

        targetText.text = percentageToThreeDecimals.ToString() + "% Population Conscripted";
    }
}
