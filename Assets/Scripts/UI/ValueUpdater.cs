using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ValueUpdater : MonoBehaviour
{
    [SerializeField] TMP_Text targetText;
    [Space(5)]
    [SerializeField] bool floatAsPercentage;
    [Space(5)]
    [SerializeField] string variableName;
    

    private void OnEnable()
    {
        if(variableName=="fInfantryQuality" || variableName=="fArchersQuality" || variableName=="fCavalryQuality")
        {
            string unit=variableName.Substring(1).Replace("Quality","");
            targetText.text="Lethality\n×"+TroopTraining.Lethality(System.Array.IndexOf(TroopTraining.Units,unit)).ToString("0.##");
            targetText.fontSize=20;
            var tooltip=targetText.GetComponent<LethalityTooltip>()??targetText.gameObject.AddComponent<LethalityTooltip>();tooltip.Unit=System.Array.IndexOf(TroopTraining.Units,unit);targetText.raycastTarget=true;
            return;
        }
        switch(variableName[0])
        {
            case 'f':
                targetText.text = floatAsPercentage ?
                                  VariableSingleton.GetPercentFloat(variableName) :
                                  VariableSingleton.GetFloatVariable(variableName).ToString();
                break;

            case 'i':
                targetText.text = VariableSingleton.GetIntVariable(variableName).ToString(); 
                break;

        }
    }
}
