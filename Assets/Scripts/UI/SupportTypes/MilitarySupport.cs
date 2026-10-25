using System;
using UnityEngine;

[Serializable]
public class MilitarySupport : SupportType
{
    [SerializeField] int infantryMin;
    [SerializeField] int infantryMax;
    [SerializeField] int archersMin;
    [SerializeField] int archersMax;
    [SerializeField] int cavalryMin;
    [SerializeField] int cavalryMax;

    public override void LendHelp()
    {
        VariableSingleton.ChangeInt("iInfantry", infantryMin, infantryMax);
        VariableSingleton.ChangeInt("iArchers", archersMin, archersMax);
        VariableSingleton.ChangeInt("iCavalry", cavalryMin, cavalryMax);
    }
}
