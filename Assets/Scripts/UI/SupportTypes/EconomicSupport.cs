using System;
using UnityEngine;

[Serializable]
public class EconomicSupport : SupportType
{
    [SerializeField] int MoneyMin;
    [SerializeField] int MoneyMax;

    public override void LendHelp()
    {
        VariableSingleton.ChangeInt("iMoney", MoneyMin, MoneyMax);
    }
}

