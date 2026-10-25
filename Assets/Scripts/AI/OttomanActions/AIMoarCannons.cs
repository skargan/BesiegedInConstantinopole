using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMoarCannons : OttomanAction
{
    public override bool IsRestricted()
    {
        return false;
    }

    public override void Execute()
    {
        AI.Instance.AIMoarCannons();
    }
}
