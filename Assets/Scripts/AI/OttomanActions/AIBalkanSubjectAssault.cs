using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBalkanSubjectAssault : OttomanAction
{
    public override bool IsRestricted()
    {
        return VariableSingleton.GetIntVariable("iBalkans") == 0;
    }

    public override void Execute()
    {
        AI.Instance.AIBalkanSubjectAssault();
    }
}
