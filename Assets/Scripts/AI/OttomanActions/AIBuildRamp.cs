using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBuildRamp : OttomanAction
{
    public override bool IsRestricted()
    {
        return VariableSingleton.GetFloatVariable("fSiegeRamp") >= 1;
    }

    public override void Execute()
    {
        AI.Instance.AIBuildRamp();
    }
}
