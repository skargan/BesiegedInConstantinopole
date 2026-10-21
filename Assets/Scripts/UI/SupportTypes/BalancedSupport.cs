using System;
using UnityEngine;

[Serializable]
public class BalancedSupport : SupportType
{
    [SerializeField] int infantryMin;
    [SerializeField] int infantryMax;
    [SerializeField] int archersMin;
    [SerializeField] int archersMax;
    [SerializeField] int cavalryMin;
    [SerializeField] int cavalryMax;
    [Space(10)]
    [SerializeField] int MoneyMin;
    [SerializeField] int MoneyMax;

    public override void LendHelp()
    {
        VariableSingleton.ChangeInt("iInfantry", infantryMin, infantryMax);
        VariableSingleton.ChangeInt("iArchers", archersMin, archersMax);
        VariableSingleton.ChangeInt("iCavalry", cavalryMin, cavalryMax);

        VariableSingleton.ChangeInt("iMoney", MoneyMin, MoneyMax);
    }
}
