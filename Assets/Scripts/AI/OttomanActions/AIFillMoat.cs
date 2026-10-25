using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFillMoat : OttomanAction
{
    public override bool IsRestricted() 
    {
        return VariableSingleton.GetFloatVariable("fMoat") == 0;
    }

    public override void Execute() 
    {
        AI.Instance.AIFillMoat();
    }
}
