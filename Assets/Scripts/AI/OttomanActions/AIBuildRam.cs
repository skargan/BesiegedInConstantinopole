using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBuildRam : OttomanAction
{
    public override bool IsRestricted()
    {
        return VariableSingleton.GetBoolVariable("bBatteringRam");
    }

    public override void Execute()
    {
        AI.Instance.AIBuildRam();
    }
}
