using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FactionDiplomacy))]
public class FactionDiplomacyEditor : Editor
{

    public override void OnInspectorGUI()
    {
        FactionDiplomacy targetFaction = target as FactionDiplomacy;

        if (targetFaction.CheckoutLastTag() && targetFaction.isActiveAndEnabled) 
        {
            Debug.Log("CustomEditTriggered");
            targetFaction.ClearSupportType();
            DestroyImmediate(targetFaction.GetComponent<SupportType>());

            switch (targetFaction.supportTypeTag)
            {
                case FactionDiplomacy.SupportTypeTag.militarySupport:
                    targetFaction.SetSupportType(targetFaction.gameObject.AddComponent<MilitarySupport>());
                    break;

                case FactionDiplomacy.SupportTypeTag.economicSupport:
                    targetFaction.SetSupportType(targetFaction.gameObject.AddComponent<EconomicSupport>());
                    break;

                case FactionDiplomacy.SupportTypeTag.balancedSupport:
                    targetFaction.SetSupportType(targetFaction.gameObject.AddComponent<BalancedSupport>());
                    break;

                case FactionDiplomacy.SupportTypeTag.specialSupport:
                    targetFaction.SetSupportType(targetFaction.gameObject.AddComponent<SpecialSupport>());
                    break;
            }
        }
        base.OnInspectorGUI();
    }
}
