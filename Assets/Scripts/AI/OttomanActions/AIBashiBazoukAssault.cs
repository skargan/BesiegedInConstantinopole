using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBashiBazoukAssault : OttomanAction
{
    public override bool IsRestricted()
    {
        return VariableSingleton.GetIntVariable("iBazouks") == 0;
    }

    public override void Execute()
    {
        AI.Instance.AIBashiBazoukAssault();
    }
}