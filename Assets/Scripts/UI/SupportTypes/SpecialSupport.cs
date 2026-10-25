using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpecialSupport : SupportType
{
    [SerializeField] String[] reliefVariableName;

    [SerializeField] float[] reliefMin;
    [SerializeField] float[] reliefMax;

    public override void LendHelp()
    {
        for(int i = 0; i<reliefVariableName.Length; i++) 
        {
            VariableSingleton.RndChange(reliefVariableName[i], reliefMin[i], reliefMax[i]);
        }
    }
}
