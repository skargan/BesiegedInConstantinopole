using System;
using UnityEngine;
// Core mechanics1.3:500gold labour;30/20/10 materials;7+1d4 integrity points.
public static class WallRepairRules
{
    public static int BaseMaterials(string wall)=>wall=="fOuterWall"?30:wall=="fMiddleWall"?20:wall=="fInnerWall"?10:0;
    public static int Materials(string wall)=>Mathf.CeilToInt(BaseMaterials(wall)*TechnologySystem.RepairMultiplier);
    public static int Labour=>Mathf.RoundToInt(500*(1-CourtRoster.Bonus(2,.05f,.15f)));
    public static bool TryRepair(string wall,out string report){report="This wall cannot be repaired.";int materials=Materials(wall);if(BaseMaterials(wall)==0||VariableSingleton.GetFloatVariable(wall)>=1)return false;
        report="Requires "+Labour+" gold for labour and "+materials+" materials.";if(CouncilActions.Int("iMoney")<Labour||CouncilActions.Int("iMaterials")<materials)return false;
        VariableSingleton.ChangeInt("iMoney",-Labour);VariableSingleton.ChangeInt("iMaterials",-materials);int points=UnityEngine.Random.Range(8,12);float before=VariableSingleton.GetFloatVariable(wall);VariableSingleton.ChangePercentFloat(wall,points/100f);report="Repairs completed: "+Labour+" gold for labour; "+materials+" materials consumed. Integrity +"+Mathf.RoundToInt((VariableSingleton.GetFloatVariable(wall)-before)*100)+" points → "+VariableSingleton.GetPercentFloat(wall)+".";return true;
    }
}
