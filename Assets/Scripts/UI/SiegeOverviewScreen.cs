using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class SiegeOverviewScreen : MonoBehaviour
{
    [SerializeField] BarInput spyNumberBar;
    [SerializeField] string BarVariableName;

    [SerializeField] TMP_Text[] intelTypes;
    [SerializeField] TMP_Text[] intelValues;

    public void UpdateIntelTypes() 
    { 
        for(int i = intelTypes.Length-1; i >= 0; i--)
        {
            if (i >= spyNumberBar.Value) intelTypes[i].alpha = 0.5f;
            else intelTypes[i].alpha = 1;
        }
    }

    public void UpdateIntelValues() 
    {
        for (int i = intelValues.Length-1; i >= 0; i--)
        {
            if (i >= spyNumberBar.Value) intelValues[i].alpha = 0;
            else intelValues[i].alpha = 1;
        }
    }

    private void Start()
    {
        UpdateIntelTypes();
        UpdateIntelValues();
    }

    private void OnEnable()
    {
        spyNumberBar.Value = VariableSingleton.GetIntVariable(BarVariableName);
        UpdateIntelTypes() ;
        UpdateIntelValues() ;
    }
}
