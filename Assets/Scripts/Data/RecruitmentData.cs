using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecruitmentData
{
    [SerializeField] string variableName;
    public string GetVariableName() => variableName;


    [SerializeField] string unitName;
    public string GetName() => unitName;


    [SerializeField] int cost;
    public int GetCost() => cost;
}
