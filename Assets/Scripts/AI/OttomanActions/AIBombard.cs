using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBombard : OttomanAction
{
    public override bool IsRestricted()
    {
        return VariableSingleton.GetIntVariable("iTurkCannons") == 0;
    }

    public override void Execute()
    {
        AI.Instance.AIBombard();
    }
}
