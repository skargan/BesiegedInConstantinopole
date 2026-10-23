using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CostMultiplyingDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text multipliedCost;
    public int TroopCost { private get; set; }

    public void UpdateMultipliedCost(int numberOfUnits) 
    {
        multipliedCost.text = TroopCost * numberOfUnits + "g";
    }
}
