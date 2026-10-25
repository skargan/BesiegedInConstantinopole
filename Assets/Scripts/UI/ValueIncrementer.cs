using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ValueIncrementer : MonoBehaviour
{
    [SerializeField] TMP_InputField numberDisplayed;
    [SerializeField] CostMultiplyingDisplay totalCostDisplay;

    int maxAffordable;
    public void SetMaxAffordable(int newMax) => maxAffordable = newMax;

    int _numberHeld = 0;
    int numberHeld
    {
        get => _numberHeld;
        set
        {
            if (_numberHeld != value)
            { 
                if (value <= 0) _numberHeld = 0;
                else if(value >= maxAffordable) _numberHeld = maxAffordable;
                else _numberHeld = value;

                numberDisplayed.text = _numberHeld.ToString();
                totalCostDisplay.UpdateMultipliedCost(numberHeld);
            }
        }
    }

    public int CommitNumber() 
    {
        int reading = numberHeld;
        numberHeld = 0;
        return reading;
    }

    public void IncreaseNumber(int delta) 
    {
        numberHeld += delta;
    }

    public void OverrideNumber(string newNumberText) 
    {
        if (!string.IsNullOrEmpty(newNumberText)) 
        {
            numberHeld = int.TryParse(newNumberText, out int parsed) ? parsed : 0;
        }
        else numberHeld = 0;
    }

    private void OnEnable()
    {
        numberHeld = 0;
    }
}
