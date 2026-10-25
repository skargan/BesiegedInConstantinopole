using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBuildTower : OttomanAction
{
    public override bool IsRestricted()
    {
        return VariableSingleton.GetBoolVariable("bSiegeTower");
    }

    public override void Execute()
    {
        AI.Instance.AIBuildTower();
    }
}
